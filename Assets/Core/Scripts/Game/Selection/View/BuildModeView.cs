using Core.Game.Blueprint.System;
using Core.Game.Map.Data;
using Core.Game.Map.System;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Selection.View
{
    /// <summary>
    /// 建造模式: 底部工具栏 + 鼠标放置蓝图
    /// B键开关建造面板, ESC退出
    /// 格子占据式: 点击格子直接放置结构/家具
    /// </summary>
    public class BuildModeView : MonoBehaviour, IController
    {
        private enum EBuildMode
        {
            None,
            Wall,
            Door,
            Window,
            Furniture,
            Demolish,
            Disassemble,
        }

        private Canvas _canvas;
        private GameObject _toolbar;
        private Text _modeLabel;

        private BlueprintSystem _blueprintSystem;
        private MapSystem _mapSystem;
        private Camera _mainCamera;

        private EBuildMode _currentMode = EBuildMode.None;

        // 结构DefId: 1=WoodWall, 2=StoneWall, 3=WoodDoor, 4=StoneDoor, 5=WoodWindow, 6=StoneWindow
        private int _selectedWallStructureId = 1;
        private int _selectedDoorStructureId = 3;
        private int _selectedWindowStructureId = 5;
        private int _selectedItemDefId = 1; // 默认床
        private bool _panelVisible;

        // 按钮引用
        private Button _wallBtn, _doorBtn, _windowBtn, _furnitureBtn, _demolishBtn, _disassembleBtn, _cancelBtn;

        // 家具选择
        private int[] _availableFurniture = { 1, 2, 3, 4, 5, 6 }; // Bed,Table,Chair,Crate,Rug,Lamp
        private int _furnitureIndex;

        private void Start()
        {
            _blueprintSystem = this.GetSystem<BlueprintSystem>();
            _mapSystem = this.GetSystem<MapSystem>();
            _mainCamera = Camera.main;

            CreateUI();
            _toolbar.SetActive(false);
        }

        private void Update()
        {
            // B键切换建造面板
            if (Input.GetKeyDown(KeyCode.B))
            {
                TogglePanel();
            }

            // ESC退出建造模式
            if (Input.GetKeyDown(KeyCode.Escape) && _panelVisible)
            {
                if (_currentMode != EBuildMode.None)
                {
                    SetMode(EBuildMode.None);
                }
                else
                {
                    TogglePanel();
                }
            }

            // Tab切换家具类型
            if (_currentMode == EBuildMode.Furniture && Input.GetKeyDown(KeyCode.Tab))
            {
                _furnitureIndex = (_furnitureIndex + 1) % _availableFurniture.Length;
                _selectedItemDefId = _availableFurniture[_furnitureIndex];
                UpdateModeLabel();
            }

            // 鼠标左键放置
            if (_currentMode != EBuildMode.None && Input.GetMouseButtonDown(0))
            {
                // 检查是否点击在UI上
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                HandleBuildClick();
            }
        }

        private void HandleBuildClick()
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            int cellX = Mathf.FloorToInt(worldPos.x);
            int cellY = Mathf.FloorToInt(worldPos.y);
            int floor = _mapSystem.CurrentFloor;

            switch (_currentMode)
            {
                case EBuildMode.Wall:
                    _blueprintSystem.PlaceBuildStructureBlueprint(_selectedWallStructureId, cellX, cellY, floor);
                    break;

                case EBuildMode.Door:
                    _blueprintSystem.PlaceBuildStructureBlueprint(_selectedDoorStructureId, cellX, cellY, floor);
                    break;

                case EBuildMode.Window:
                    _blueprintSystem.PlaceBuildStructureBlueprint(_selectedWindowStructureId, cellX, cellY, floor);
                    break;

                case EBuildMode.Furniture:
                    _blueprintSystem.PlaceBuildFurnitureBlueprint(_selectedItemDefId, cellX, cellY, floor);
                    break;

                case EBuildMode.Demolish:
                {
                    var cell = _mapSystem.GetCell(cellX, cellY, floor);
                    if (cell == null) break;

                    // 优先拆结构 (墙/门/窗)
                    if (cell.HasStructure)
                    {
                        _blueprintSystem.PlaceDemolishStructureBlueprint(cellX, cellY, floor);
                        break;
                    }

                    // 再检查家具
                    if (cell.ObjectIds != null)
                    {
                        foreach (long objId in cell.ObjectIds)
                        {
                            if (objId >= 1_000_000 && objId < 2_000_000)
                            {
                                _blueprintSystem.PlaceDemolishItemBlueprint(objId);
                                break;
                            }
                        }
                    }
                    break;
                }

                case EBuildMode.Disassemble:
                {
                    var cell = _mapSystem.GetCell(cellX, cellY, floor);
                    if (cell?.ObjectIds == null) break;

                    foreach (long objId in cell.ObjectIds)
                    {
                        if (objId >= 1_000_000 && objId < 2_000_000)
                        {
                            _blueprintSystem.PlaceDisassembleBlueprint(objId);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private void TogglePanel()
        {
            _panelVisible = !_panelVisible;
            _toolbar.SetActive(_panelVisible);

            if (!_panelVisible)
                SetMode(EBuildMode.None);
        }

        private void SetMode(EBuildMode mode)
        {
            _currentMode = mode;
            UpdateModeLabel();
            UpdateButtonColors();
        }

        private void UpdateModeLabel()
        {
            if (_modeLabel == null) return;

            switch (_currentMode)
            {
                case EBuildMode.None:
                    _modeLabel.text = "建造模式 [B]";
                    break;
                case EBuildMode.Wall:
                {
                    var def = TempConfigProvider.GetStructureDef(_selectedWallStructureId);
                    _modeLabel.text = $"建墙: {def.Name} (点击格子)";
                    break;
                }
                case EBuildMode.Door:
                {
                    var def = TempConfigProvider.GetStructureDef(_selectedDoorStructureId);
                    _modeLabel.text = $"建门: {def.Name} (点击格子)";
                    break;
                }
                case EBuildMode.Window:
                {
                    var def = TempConfigProvider.GetStructureDef(_selectedWindowStructureId);
                    _modeLabel.text = $"建窗: {def.Name} (点击格子)";
                    break;
                }
                case EBuildMode.Furniture:
                {
                    var def = TempConfigProvider.GetItemDef(_selectedItemDefId);
                    _modeLabel.text = $"建家具: {def.Name} [Tab切换]";
                    break;
                }
                case EBuildMode.Demolish:
                    _modeLabel.text = "拆除 (点击结构/家具)";
                    break;
                case EBuildMode.Disassemble:
                    _modeLabel.text = "拆卸 (点击家具)";
                    break;
            }
        }

        private void UpdateButtonColors()
        {
            SetButtonHighlight(_wallBtn, _currentMode == EBuildMode.Wall);
            SetButtonHighlight(_doorBtn, _currentMode == EBuildMode.Door);
            SetButtonHighlight(_windowBtn, _currentMode == EBuildMode.Window);
            SetButtonHighlight(_furnitureBtn, _currentMode == EBuildMode.Furniture);
            SetButtonHighlight(_demolishBtn, _currentMode == EBuildMode.Demolish);
            SetButtonHighlight(_disassembleBtn, _currentMode == EBuildMode.Disassemble);
        }

        private void SetButtonHighlight(Button btn, bool active)
        {
            if (btn == null) return;
            var colors = btn.colors;
            colors.normalColor = active ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.25f, 0.25f, 0.25f);
            btn.colors = colors;
        }

        #region UI构建

        private void CreateUI()
        {
            // Canvas
            var canvasGo = new GameObject("BuildModeCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 110;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGo.AddComponent<GraphicRaycaster>();

            // 工具栏容器 (底部居中)
            _toolbar = new GameObject("Toolbar", typeof(RectTransform));
            _toolbar.transform.SetParent(canvasGo.transform, false);
            var toolbarRT = _toolbar.GetComponent<RectTransform>();
            toolbarRT.anchorMin = new Vector2(0.5f, 0);
            toolbarRT.anchorMax = new Vector2(0.5f, 0);
            toolbarRT.pivot = new Vector2(0.5f, 0);
            toolbarRT.anchoredPosition = new Vector2(0, 10);
            toolbarRT.sizeDelta = new Vector2(720, 90);

            var toolbarBg = _toolbar.AddComponent<Image>();
            toolbarBg.color = new Color(0, 0, 0, 0.75f);
            toolbarBg.raycastTarget = true;

            // 模式标签 (工具栏上方)
            var labelGo = new GameObject("ModeLabel", typeof(RectTransform));
            labelGo.transform.SetParent(_toolbar.transform, false);
            _modeLabel = labelGo.AddComponent<Text>();
            _modeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _modeLabel.fontSize = 16;
            _modeLabel.alignment = TextAnchor.MiddleCenter;
            _modeLabel.color = Color.white;
            _modeLabel.text = "建造模式 [B]";
            _modeLabel.raycastTarget = false;
            var labelRT = labelGo.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.pivot = new Vector2(0.5f, 0);
            labelRT.anchoredPosition = new Vector2(0, 2);
            labelRT.sizeDelta = new Vector2(0, 20);

            // 按钮行 (7个按钮: 墙壁/门/窗/家具/拆除/拆卸/关闭)
            float btnWidth = 85f;
            float btnHeight = 50f;
            float spacing = 8f;
            int btnCount = 7;
            float startX = -(btnWidth * btnCount + spacing * (btnCount - 1)) / 2f + btnWidth / 2f;
            float btnY = -52f;

            Text _;
            _wallBtn = CreateButton(_toolbar, "墙壁", startX, btnY, btnWidth, btnHeight, out _);
            _wallBtn.onClick.AddListener(() => SetMode(_currentMode == EBuildMode.Wall ? EBuildMode.None : EBuildMode.Wall));

            _doorBtn = CreateButton(_toolbar, "门", startX + (btnWidth + spacing), btnY, btnWidth, btnHeight, out _);
            _doorBtn.onClick.AddListener(() => SetMode(_currentMode == EBuildMode.Door ? EBuildMode.None : EBuildMode.Door));

            _windowBtn = CreateButton(_toolbar, "窗", startX + (btnWidth + spacing) * 2, btnY, btnWidth, btnHeight, out _);
            _windowBtn.onClick.AddListener(() => SetMode(_currentMode == EBuildMode.Window ? EBuildMode.None : EBuildMode.Window));

            _furnitureBtn = CreateButton(_toolbar, "家具", startX + (btnWidth + spacing) * 3, btnY, btnWidth, btnHeight, out _);
            _furnitureBtn.onClick.AddListener(() => SetMode(_currentMode == EBuildMode.Furniture ? EBuildMode.None : EBuildMode.Furniture));

            _demolishBtn = CreateButton(_toolbar, "拆除", startX + (btnWidth + spacing) * 4, btnY, btnWidth, btnHeight, out _);
            _demolishBtn.onClick.AddListener(() => SetMode(_currentMode == EBuildMode.Demolish ? EBuildMode.None : EBuildMode.Demolish));

            _disassembleBtn = CreateButton(_toolbar, "拆卸", startX + (btnWidth + spacing) * 5, btnY, btnWidth, btnHeight, out _);
            _disassembleBtn.onClick.AddListener(() => SetMode(_currentMode == EBuildMode.Disassemble ? EBuildMode.None : EBuildMode.Disassemble));

            _cancelBtn = CreateButton(_toolbar, "关闭", startX + (btnWidth + spacing) * 6, btnY, btnWidth, btnHeight, out _);
            _cancelBtn.onClick.AddListener(TogglePanel);

            // 设置关闭按钮为红色
            var cancelColors = _cancelBtn.colors;
            cancelColors.normalColor = new Color(0.6f, 0.2f, 0.2f);
            cancelColors.highlightedColor = new Color(0.8f, 0.3f, 0.3f);
            _cancelBtn.colors = cancelColors;
        }

        private Button CreateButton(GameObject parent, string label, float x, float y,
            float width, float height, out Text text)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.25f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.25f, 0.25f, 0.25f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f);
            btn.colors = colors;

            // 文字
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            text = textGo.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;

            var textRT = textGo.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return btn;
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
