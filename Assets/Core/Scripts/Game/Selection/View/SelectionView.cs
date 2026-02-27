using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Utility;
using Core.Game.Selection.Define;
using Core.Game.Selection.Event;
using Core.Game.Selection.Model;
using Core.Game.Selection.System;
using GDFramework.Input;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Selection.View
{
    /// <summary>
    /// 选择视图: 鼠标输入 → SelectionSystem → 高亮渲染
    /// </summary>
    public class SelectionView : MonoBehaviour, IController
    {
        private SelectionSystem _selectionSystem;
        private SelectionDataModel _selectionModel;

        private Camera _camera;
        private int _currentFloor;

        // 高亮渲染
        private GameObject _highlightGo;
        private SpriteRenderer _highlightRenderer;

        // 悬停高亮
        private GameObject _hoverGo;
        private SpriteRenderer _hoverRenderer;

        private static readonly Color SelectionColor = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color HoverColor = new Color(1f, 1f, 1f, 0.15f);
        private static readonly Color PawnSelectionColor = new Color(0.2f, 1f, 0.2f, 0.5f);
        private static readonly Color ItemSelectionColor = new Color(0.4f, 0.7f, 1f, 0.5f);
        private static readonly Color CommandColor = new Color(0.2f, 1f, 0.2f, 0.3f);

        private void Start()
        {
            _selectionSystem = this.GetSystem<SelectionSystem>();
            _selectionModel = this.GetModel<SelectionDataModel>();
            _camera = Camera.main;

            CreateHighlightObjects();

            // 注册输入事件
            this.RegisterEvent<SInputEvent_MouseLeftClick>(OnMouseLeftClick);
            this.RegisterEvent<SInputEvent_MouseRightClick>(OnMouseRightClick);

            // 注册选择变更事件
            this.RegisterEvent<SSelectionChangedEvent>(OnSelectionChanged);
            this.RegisterEvent<Map.Event.SFloorChangedEvent>(OnFloorChanged);
        }

        private void Update()
        {
            UpdateHoverHighlight();
        }

        #region 输入处理

        private void OnMouseLeftClick(SInputEvent_MouseLeftClick evt)
        {
            if (_camera == null) return;

            Vector3 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int tilePos = MapCoordUtility.WorldToTile(worldPos);

            _selectionSystem.HandleLeftClick(tilePos.x, tilePos.y, _currentFloor);
        }

        private void OnMouseRightClick(SInputEvent_MouseRightClick evt)
        {
            if (_camera == null) return;

            Vector3 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int tilePos = MapCoordUtility.WorldToTile(worldPos);

            _selectionSystem.HandleRightClick(tilePos.x, tilePos.y, _currentFloor);
        }

        private void OnFloorChanged(Map.Event.SFloorChangedEvent evt)
        {
            _currentFloor = evt.NewFloor;
            _selectionSystem.ClearSelection();
        }

        #endregion

        #region 高亮渲染

        private void OnSelectionChanged(SSelectionChangedEvent evt)
        {
            if (evt.Type == ESelectionType.None)
            {
                _highlightGo.SetActive(false);
                return;
            }

            // 根据选择类型设置颜色和大小
            switch (evt.Type)
            {
                case ESelectionType.Cell:
                    SetHighlight(evt.CellX, evt.CellY, 1, 1, SelectionColor);
                    break;

                case ESelectionType.Pawn:
                    SetHighlight(evt.CellX, evt.CellY, 1, 1, PawnSelectionColor);
                    break;

                case ESelectionType.Item:
                    // Item可能是多格, 从ItemData取尺寸
                    var itemModel = this.GetModel<ItemDataModel>();
                    var item = itemModel.GetItem(evt.TargetId);
                    if (item != null)
                        SetHighlight(item.AnchorX, item.AnchorY, item.Width, item.Height, ItemSelectionColor);
                    else
                        SetHighlight(evt.CellX, evt.CellY, 1, 1, ItemSelectionColor);
                    break;
            }
        }

        private void SetHighlight(int cellX, int cellY, int width, int height, Color color)
        {
            _highlightGo.SetActive(true);
            _highlightRenderer.color = color;
            _highlightRenderer.sortingOrder = 20000; // 最上层

            // 位置: 左下角 + 尺寸/2 偏移
            _highlightGo.transform.position = new Vector3(
                cellX + width * 0.5f,
                cellY + height * 0.5f,
                0f);
            _highlightGo.transform.localScale = new Vector3(width, height, 1f);
        }

        /// <summary>
        /// 鼠标悬停高亮 (每帧更新)
        /// </summary>
        private void UpdateHoverHighlight()
        {
            if (_camera == null || _hoverGo == null) return;

            Vector3 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int tilePos = MapCoordUtility.WorldToTile(worldPos);

            // 检查是否在地图范围内
            if (tilePos.x >= 0 && tilePos.y >= 0)
            {
                _hoverGo.SetActive(true);
                _hoverRenderer.color = HoverColor;
                _hoverGo.transform.position = new Vector3(
                    tilePos.x + 0.5f,
                    tilePos.y + 0.5f,
                    0f);
            }
            else
            {
                _hoverGo.SetActive(false);
            }
        }

        #endregion

        #region 初始化

        private void CreateHighlightObjects()
        {
            var sprite = CreateSquareSprite();

            // 选择高亮
            _highlightGo = new GameObject("SelectionHighlight");
            _highlightGo.transform.SetParent(transform, false);
            _highlightRenderer = _highlightGo.AddComponent<SpriteRenderer>();
            _highlightRenderer.sprite = sprite;
            _highlightRenderer.sortingOrder = 20000;
            _highlightGo.SetActive(false);

            // 悬停高亮
            _hoverGo = new GameObject("HoverHighlight");
            _hoverGo.transform.SetParent(transform, false);
            _hoverRenderer = _hoverGo.AddComponent<SpriteRenderer>();
            _hoverRenderer.sprite = sprite;
            _hoverRenderer.sortingOrder = 19999;
            _hoverGo.SetActive(false);
        }

        /// <summary>
        /// 程序化创建1x1白色方块Sprite
        /// </summary>
        private Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f), 4); // pivot中心, 4ppu → 1世界单位
        }

        #endregion

        private void OnDestroy()
        {
            if (_highlightGo != null) Destroy(_highlightGo);
            if (_hoverGo != null) Destroy(_hoverGo);
        }

        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}
