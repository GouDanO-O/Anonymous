using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Event;
using Core.Game.Map.System;
using Core.Game.Pawn.Model;
using Core.Game.Selection.Define;
using Core.Game.Selection.Event;
using Core.Game.Selection.Model;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.View
{
    /// <summary>
    /// 游戏HUD: 楼层指示器 + 选中信息面板
    /// 程序化创建, 无需prefab
    /// </summary>
    public class GameHudView : MonoBehaviour, IController
    {
        private Canvas _canvas;
        private SelectionDataModel _selectionModel;
        private PawnDataModel _pawnModel;
        private ItemDataModel _itemModel;
        private MapSystem _mapSystem;

        // UI元素
        private Text _floorLabel;
        private Text _infoTitle;
        private Text _infoContent;
        private GameObject _infoPanel;

        private int _currentFloor;

        private void Start()
        {
            _selectionModel = this.GetModel<SelectionDataModel>();
            _pawnModel = this.GetModel<PawnDataModel>();
            _itemModel = this.GetModel<ItemDataModel>();
            _mapSystem = this.GetSystem<MapSystem>();

            CreateUI();

            this.RegisterEvent<SFloorChangedEvent>(OnFloorChanged);
            this.RegisterEvent<SSelectionChangedEvent>(OnSelectionChanged);

            UpdateFloorLabel();
        }

        private void Update()
        {
            // 持续刷新Pawn信息 (位置/状态会变)
            if (_selectionModel.CurrentType == ESelectionType.Pawn)
            {
                UpdatePawnInfo(_selectionModel.Current.TargetId);
            }
        }

        #region 事件

        private void OnFloorChanged(SFloorChangedEvent evt)
        {
            _currentFloor = evt.NewFloor;
            UpdateFloorLabel();
        }

        private void OnSelectionChanged(SSelectionChangedEvent evt)
        {
            if (evt.Type == ESelectionType.None)
            {
                _infoPanel.SetActive(false);
                return;
            }

            _infoPanel.SetActive(true);

            switch (evt.Type)
            {
                case ESelectionType.Cell:
                    UpdateCellInfo(evt.CellX, evt.CellY, evt.Floor);
                    break;
                case ESelectionType.Pawn:
                    UpdatePawnInfo(evt.TargetId);
                    break;
                case ESelectionType.Item:
                    UpdateItemInfo(evt.TargetId);
                    break;
            }
        }

        #endregion

        #region 信息更新

        private void UpdateFloorLabel()
        {
            if (_floorLabel != null)
                _floorLabel.text = $"F{_currentFloor}";
        }

        private void UpdateCellInfo(int x, int y, int floor)
        {
            var cell = _mapSystem.GetCell(x, y, floor);
            _infoTitle.text = $"地块 ({x}, {y})";

            if (cell == null)
            {
                _infoContent.text = "无效区域";
                return;
            }

            string terrain = GetTerrainName(cell.TerrainDefId);
            string floorType = cell.FloorDefId > 0
                ? TempConfigProvider.GetFloorDef(cell.FloorDefId).Name
                : "无";
            string walkable = cell.IsWalkable ? "可通行" : "不可通行";
            string room = cell.RoomId > 0 ? $"房间#{cell.RoomId}" : "户外";

            _infoContent.text = $"地形: {terrain}\n地板: {floorType}\n{walkable} | {room}";
        }

        private void UpdatePawnInfo(long pawnId)
        {
            var pawn = _pawnModel.GetPawn(pawnId);
            if (pawn == null)
            {
                _infoPanel.SetActive(false);
                return;
            }

            var def = TempConfigProvider.GetPawnDef(pawn.PawnDefId);
            _infoTitle.text = $"{pawn.Name} ({def.Name})";
            _infoContent.text =
                $"位置: ({pawn.X}, {pawn.Y}) F{pawn.Floor}\n" +
                $"状态: {pawn.State}\n" +
                $"饥饿: {pawn.Needs.Hunger:F0}  疲劳: {pawn.Needs.Fatigue:F0}\n" +
                $"心情: {pawn.Needs.Mood:F0}\n" +
                $"[右键点击地面下达移动指令]";
        }

        private void UpdateItemInfo(long itemId)
        {
            var item = _itemModel.GetItem(itemId);
            if (item == null)
            {
                _infoPanel.SetActive(false);
                return;
            }

            var def = TempConfigProvider.GetItemDef(item.ItemDefId);
            _infoTitle.text = $"{def.Name}";
            string blocking = def.BlocksMovement ? "阻挡通行" : "可穿越";
            _infoContent.text =
                $"位置: ({item.AnchorX}, {item.AnchorY}) F{item.Floor}\n" +
                $"尺寸: {item.Width}x{item.Height}\n" +
                $"耐久: {item.Health}/{def.MaxHealth}\n" +
                $"{blocking}";
        }

        private string GetTerrainName(int defId)
        {
            return defId switch
            {
                1 => "草地",
                2 => "泥土",
                3 => "沙地",
                4 => "石头",
                5 => "水",
                _ => "未知"
            };
        }

        #endregion

        #region UI构建

        private void CreateUI()
        {
            // Canvas
            var canvasGo = new GameObject("GameHudCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGo.AddComponent<GraphicRaycaster>();

            // === 楼层指示器 (左下角) ===
            var floorPanel = CreatePanel(canvasGo.transform, "FloorPanel",
                TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(20, 20), new Vector2(120, 70));
            AddBackground(floorPanel, new Color(0, 0, 0, 0.6f));

            _floorLabel = CreateText(floorPanel, "FloorLabel",
                "F0", 36, TextAnchor.MiddleCenter, Color.white);
            StretchFill(_floorLabel.rectTransform);

            // === 选中信息面板 (右下角) ===
            _infoPanel = CreatePanel(canvasGo.transform, "InfoPanel",
                TextAnchor.LowerRight, new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-260, 20), new Vector2(-20, 180));
            AddBackground(_infoPanel, new Color(0, 0, 0, 0.7f));

            // 标题
            _infoTitle = CreateText(_infoPanel, "InfoTitle",
                "", 20, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.6f));
            var titleRT = _infoTitle.rectTransform;
            titleRT.anchorMin = new Vector2(0, 0.75f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(10, 0);
            titleRT.offsetMax = new Vector2(-10, -5);

            // 内容
            _infoContent = CreateText(_infoPanel, "InfoContent",
                "", 15, TextAnchor.UpperLeft, Color.white);
            var contentRT = _infoContent.rectTransform;
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(1, 0.75f);
            contentRT.offsetMin = new Vector2(10, 5);
            contentRT.offsetMax = new Vector2(-10, -2);

            _infoPanel.SetActive(false);
        }

        private GameObject CreatePanel(Transform parent, string name,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        private void AddBackground(GameObject panel, Color color)
        {
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }

        private Text CreateText(GameObject parent, string name,
            string text, int fontSize, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.raycastTarget = false;
            return txt;
        }

        private void StretchFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        #endregion

        private void OnDestroy()
        {
            if (_canvas != null)
                Destroy(_canvas.gameObject);
        }

        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}
