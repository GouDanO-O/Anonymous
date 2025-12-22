/**
 * MapEditorUI.cs
 * 地图编辑器 UI（简单的 IMGUI 版本）
 * 
 * 提供基本的编辑器界面：
 * - 工具选择
 * - 层级选择
 * - 瓦片/实体选择
 * - 撤销/重做按钮
 */

using UnityEngine;
using GDFramework.MapSystem.Rendering;
using GDFramework.MapSystem.Saving;

namespace GDFramework.MapSystem.Editor
{
    /// <summary>
    /// 地图编辑器 UI
    /// </summary>
    public class MapEditorUI : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("References")]
        [SerializeField]
        private RuntimeMapEditor _editor;
        
        [SerializeField]
        private MapRenderer _mapRenderer;
        
        [Header("UI Settings")]
        [SerializeField]
        private bool _showUI = true;
        
        [SerializeField]
        private Rect _windowRect = new Rect(10, 10, 250, 400);
        
        #endregion
        
        #region 字段
        
        private int _selectedModeIndex = 0;
        private int _selectedLayerIndex = 0;
        private int _selectedTileId = 1;
        private int _selectedEntityConfigId = 1;
        private int _selectedEntityTypeIndex = 0;
        private int _brushShapeIndex = 0;
        private int _brushSize = 1;
        
        private Vector2 _scrollPosition;
        
        private readonly string[] _modeNames = new string[]
        {
            "无", "绘制瓦片", "擦除瓦片", "放置实体", 
            "选择实体", "移动实体", "删除实体", "填充", "吸取"
        };
        
        private readonly string[] _layerNames = new string[]
        {
            "地形 (Ground)", "地板 (Floor)", "地面装饰",
            "墙壁 (Wall)", "墙壁装饰", "屋顶 (Roof)"
        };
        
        private readonly string[] _entityTypeNames = new string[]
        {
            "家具", "容器", "门", "窗户", "设备", "掉落物", "光源", "其他"
        };
        
        private readonly string[] _brushShapeNames = new string[]
        {
            "单格", "方形", "圆形"
        };
        
        #endregion
        
        #region Unity 生命周期
        
        void OnGUI()
        {
            if (!_showUI || _editor == null || !_editor.IsEnabled) return;
            
            _windowRect = GUILayout.Window(0, _windowRect, DrawWindow, "地图编辑器");
        }
        
        #endregion
        
        #region UI 绘制
        
        private void DrawWindow(int windowId)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            // 模式选择
            DrawModeSection();
            
            GUILayout.Space(10);
            
            // 根据模式显示不同的选项
            switch (_editor.CurrentMode)
            {
                case EditorMode.TilePaint:
                case EditorMode.TileErase:
                case EditorMode.Pick:
                case EditorMode.Fill:
                    DrawTileSection();
                    DrawBrushSection();
                    break;
                    
                case EditorMode.EntityPlace:
                    DrawEntitySection();
                    break;
                    
                case EditorMode.EntitySelect:
                    DrawSelectedEntityInfo();
                    break;
            }
            
            GUILayout.Space(10);
            
            // 撤销/重做
            DrawUndoRedoSection();
            
            GUILayout.Space(10);
            
            // 保存/加载
            DrawSaveLoadSection();
            
            GUILayout.Space(10);
            
            // 视图控制
            DrawViewSection();
            
            GUILayout.EndScrollView();
            
            // 允许拖动窗口
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        }
        
        private void DrawModeSection()
        {
            GUILayout.Label("编辑模式", GUI.skin.box);
            
            int newModeIndex = GUILayout.SelectionGrid(_selectedModeIndex, _modeNames, 3);
            
            if (newModeIndex != _selectedModeIndex)
            {
                _selectedModeIndex = newModeIndex;
                _editor.SetMode((EditorMode)newModeIndex);
            }
        }
        
        private void DrawTileSection()
        {
            GUILayout.Label("瓦片设置", GUI.skin.box);
            
            // 层级选择
            GUILayout.Label("编辑层:");
            int newLayerIndex = GUILayout.SelectionGrid(_selectedLayerIndex, _layerNames, 2);
            
            if (newLayerIndex != _selectedLayerIndex)
            {
                _selectedLayerIndex = newLayerIndex;
                _editor.SetLayer(newLayerIndex);
            }
            
            GUILayout.Space(5);
            
            // 瓦片 ID
            GUILayout.BeginHorizontal();
            GUILayout.Label("瓦片 ID:", GUILayout.Width(60));
            string idStr = GUILayout.TextField(_selectedTileId.ToString(), GUILayout.Width(60));
            if (int.TryParse(idStr, out int newId) && newId >= 0 && newId <= 65535)
            {
                _selectedTileId = newId;
                _editor.SetTileId((ushort)newId);
            }
            GUILayout.EndHorizontal();
            
            // 快捷瓦片按钮
            GUILayout.Label("常用瓦片:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("草地(1)")) SetTile(1);
            if (GUILayout.Button("泥土(2)")) SetTile(2);
            if (GUILayout.Button("水(5)")) SetTile(5);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("木板(10)")) SetTile(10);
            if (GUILayout.Button("石板(11)")) SetTile(11);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("木墙(20)")) SetTile(20);
            if (GUILayout.Button("石墙(21)")) SetTile(21);
            if (GUILayout.Button("屋顶(30)")) SetTile(30);
            GUILayout.EndHorizontal();
        }
        
        private void SetTile(int id)
        {
            _selectedTileId = id;
            _editor.SetTileId((ushort)id);
        }
        
        private void DrawBrushSection()
        {
            GUILayout.Label("笔刷设置", GUI.skin.box);
            
            // 笔刷形状
            GUILayout.Label("形状:");
            int newShapeIndex = GUILayout.SelectionGrid(_brushShapeIndex, _brushShapeNames, 3);
            
            if (newShapeIndex != _brushShapeIndex)
            {
                _brushShapeIndex = newShapeIndex;
                _editor.SetBrush((BrushShape)newShapeIndex, _brushSize);
            }
            
            // 笔刷大小
            GUILayout.BeginHorizontal();
            GUILayout.Label($"大小: {_brushSize}", GUILayout.Width(60));
            int newSize = (int)GUILayout.HorizontalSlider(_brushSize, 1, 10);
            if (newSize != _brushSize)
            {
                _brushSize = newSize;
                _editor.SetBrush((BrushShape)_brushShapeIndex, _brushSize);
            }
            GUILayout.EndHorizontal();
        }
        
        private void DrawEntitySection()
        {
            GUILayout.Label("实体设置", GUI.skin.box);
            
            // 实体类型
            GUILayout.Label("类型:");
            int newTypeIndex = GUILayout.SelectionGrid(_selectedEntityTypeIndex, _entityTypeNames, 4);
            
            if (newTypeIndex != _selectedEntityTypeIndex)
            {
                _selectedEntityTypeIndex = newTypeIndex;
            }
            
            // 配置 ID
            GUILayout.BeginHorizontal();
            GUILayout.Label("配置 ID:", GUILayout.Width(60));
            string idStr = GUILayout.TextField(_selectedEntityConfigId.ToString(), GUILayout.Width(60));
            if (int.TryParse(idStr, out int newId) && newId > 0)
            {
                _selectedEntityConfigId = newId;
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("应用"))
            {
                _editor.SetEntityConfig(_selectedEntityConfigId, (EntityType)_selectedEntityTypeIndex);
            }
            
            // 快捷实体按钮
            GUILayout.Label("常用实体:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("桌子")) SetEntity(2001, EntityType.Furniture);
            if (GUILayout.Button("椅子")) SetEntity(2002, EntityType.Furniture);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("冰箱")) SetEntity(3001, EntityType.Container);
            if (GUILayout.Button("柜子")) SetEntity(3002, EntityType.Container);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("木门")) SetEntity(1001, EntityType.Door);
            GUILayout.EndHorizontal();
        }
        
        private void SetEntity(int configId, EntityType type)
        {
            _selectedEntityConfigId = configId;
            _selectedEntityTypeIndex = (int)type;
            _editor.SetEntityConfig(configId, type);
        }
        
        private void DrawSelectedEntityInfo()
        {
            GUILayout.Label("选中实体", GUI.skin.box);
            
            var entity = _editor.SelectedEntity;
            
            if (entity != null)
            {
                GUILayout.Label($"ID: {entity.EntityId}");
                GUILayout.Label($"配置: {entity.ConfigId}");
                GUILayout.Label($"类型: {entity.EntityType}");
                GUILayout.Label($"位置: {entity.TilePosition}");
                GUILayout.Label($"生命: {entity.Health}/{entity.MaxHealth}");
                
                if (GUILayout.Button("删除"))
                {
                    _editor.SetMode(EditorMode.EntityDelete);
                }
            }
            else
            {
                GUILayout.Label("未选中任何实体");
            }
        }
        
        private void DrawUndoRedoSection()
        {
            GUILayout.Label("历史操作", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            
            GUI.enabled = _editor.CanUndo;
            if (GUILayout.Button("撤销 (Ctrl+Z)"))
            {
                _editor.Undo();
            }
            
            GUI.enabled = _editor.CanRedo;
            if (GUILayout.Button("重做 (Ctrl+Y)"))
            {
                _editor.Redo();
            }
            
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        
        private void DrawSaveLoadSection()
        {
            GUILayout.Label("保存/加载", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("保存地图"))
            {
                SaveMap();
            }
            
            if (GUILayout.Button("加载地图"))
            {
                LoadMap();
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawViewSection()
        {
            GUILayout.Label("视图控制", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("显示屋顶"))
            {
                _mapRenderer?.SetRoofVisible(true);
            }
            if (GUILayout.Button("隐藏屋顶"))
            {
                _mapRenderer?.SetRoofVisible(false);
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("刷新渲染"))
            {
                _mapRenderer?.ForceRefreshAll();
            }
        }
        
        #endregion
        
        #region 保存/加载
        
        private void SaveMap()
        {
            if (_editor.Map == null) return;
            
            string filePath = $"Maps/{_editor.Map.MapId}.json";
            MapSaveSystem.Instance.SaveMapToFile(_editor.Map, filePath);
            
            Debug.Log($"[MapEditorUI] 地图已保存: {filePath}");
        }
        
        private void LoadMap()
        {
            if (_editor.Map == null) return;
            
            string filePath = $"Maps/{_editor.Map.MapId}.json";
            
            if (!MapSaveSystem.Instance.SaveFileExists(filePath))
            {
                Debug.LogWarning($"[MapEditorUI] 存档不存在: {filePath}");
                return;
            }
            
            MapSaveSystem.Instance.LoadMapFromFile(filePath, _editor.Map);
            _mapRenderer?.ForceRefreshAll();
            
            Debug.Log($"[MapEditorUI] 地图已加载: {filePath}");
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 显示/隐藏 UI
        /// </summary>
        public void SetVisible(bool visible)
        {
            _showUI = visible;
        }
        
        /// <summary>
        /// 切换 UI 显示
        /// </summary>
        public void ToggleVisible()
        {
            _showUI = !_showUI;
        }
        
        #endregion
    }
}
