/**
 * CompleteExample.cs
 * 完整示例 - 展示如何使用所有系统
 * 
 * 包含：
 * - 地图创建
 * - 渲染系统
 * - 寻路系统
 * - 存档系统
 * - 编辑器工具
 */

using UnityEngine;
using GDFramework.MapSystem.Rendering;
using GDFramework.MapSystem.Pathfinding;
using GDFramework.MapSystem.Saving;

namespace GDFramework.MapSystem.Examples
{
    /// <summary>
    /// 完整示例
    /// </summary>
    public class CompleteExample : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Map Settings")]
        [SerializeField]
        private int _mapWidth = 4;
        
        [SerializeField]
        private int _mapHeight = 4;
        
        [Header("Features")]
        [SerializeField]
        private bool _enableEditor = true;
        
        [SerializeField]
        private bool _enablePathfinding = true;
        
        [SerializeField]
        private bool _useLighting = true;
        
        [Header("References")]
        [SerializeField]
        private Camera _camera;
        
        #endregion
        
        #region 字段
        
        private Map _map;
        private MapRenderer _mapRenderer;
        private RuntimeMapEditor _mapEditor;
        private MapEditorUI _editorUI;
        
        // 寻路测试
        private TileCoord? _pathStart;
        private TileCoord? _pathEnd;
        private PathResult _currentPath;
        
        #endregion
        
        #region Unity 生命周期
        
        void Start()
        {
            Debug.Log("=== 完整地图系统示例 ===");
            
            // 初始化配置
            InitializeConfigs();
            
            // 创建地图
            CreateMap();
            
            // 初始化渲染
            InitializeRendering();
            
            // 初始化寻路
            if (_enablePathfinding)
            {
                InitializePathfinding();
            }
            
            // 初始化编辑器
            if (_enableEditor)
            {
                InitializeEditor();
            }
            
            // 设置相机
            SetupCamera();
            
            PrintInstructions();
        }
        
        void Update()
        {
            HandleInput();
            
            // 更新地图（Entity 动画等）
            _map?.Update(Time.deltaTime);
        }
        
        void OnDestroy()
        {
            _mapRenderer?.Cleanup();
        }
        
        #endregion
        
        #region 初始化
        
        private void InitializeConfigs()
        {
            // 创建瓦片配置
            var tileConfigs = new TileConfig[]
            {
                TileConfig.CreateTerrain(1, "草地", "terrain_grass"),
                TileConfig.CreateTerrain(2, "泥土", "terrain_dirt"),
                TileConfig.CreateWater(5, "水", "terrain_water"),
                TileConfig.CreateFloor(10, "木地板", "floor_wood"),
                TileConfig.CreateFloor(11, "石地板", "floor_stone"),
                TileConfig.CreateWall(20, "木墙", "wall_wood", 100),
                TileConfig.CreateWall(21, "石墙", "wall_stone", 200),
                TileConfig.CreateRoof(30, "木屋顶", "roof_wood"),
            };
            
            // 创建实体配置
            var entityConfigs = new EntityConfig[]
            {
                EntityConfig.CreateFurniture(2001, "木桌", "furniture_table"),
                EntityConfig.CreateFurniture(2002, "椅子", "furniture_chair", false),
                EntityConfig.CreateContainer(3001, "冰箱", "container_fridge", 30),
                EntityConfig.CreateContainer(3002, "柜子", "container_cabinet", 20),
                EntityConfig.CreateDoor(1001, "木门", new[] { "door_closed", "door_open" }),
            };
            
            SpriteManager.Instance.Initialize(tileConfigs, entityConfigs);
        }
        
        private void CreateMap()
        {
            _map = new Map("example_map", "示例地图", _mapWidth, _mapHeight, MapType.Town);
            
            // 填充草地
            _map.FillLayer(MapConstants.LAYER_GROUND, TileLayerData.Create(1));
            
            // 建造房屋
            BuildHouse(10, 10, 8, 6);
            BuildHouse(25, 8, 6, 5);
            BuildHouse(5, 20, 10, 7);
            
            // 建造道路
            BuildRoad(0, 5, _map.WidthInTiles, true);
            BuildRoad(15, 0, _map.HeightInTiles, false);
            
            // 放置实体
            PlaceEntities();
            
            // 缓存原始数据（用于差异化保存）
            MapSaveSystem.Instance.CacheOriginalMapData(_map);
            
            Debug.Log($"地图创建完成: {_map}");
        }
        
        private void BuildHouse(int x, int y, int w, int h)
        {
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var coord = new TileCoord(x + dx, y + dy);
                    if (!_map.IsTileCoordValid(coord)) continue;
                    
                    bool isEdge = dx == 0 || dx == w - 1 || dy == 0 || dy == h - 1;
                    bool isDoor = dy == 0 && dx == w / 2;
                    
                    if (isDoor)
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                        
                        // 放置门
                        _map.Entities.CreateDoor(1001, coord, DoorType.Wooden);
                    }
                    else if (isEdge)
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithWall(20)
                            .WithRoof(30));
                    }
                    else
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                    }
                }
            }
        }
        
        private void BuildRoad(int start, int y, int length, bool horizontal)
        {
            for (int i = 0; i < length; i++)
            {
                TileCoord coord;
                if (horizontal)
                {
                    coord = new TileCoord(start + i, y);
                }
                else
                {
                    coord = new TileCoord(y, start + i);
                }
                
                if (_map.IsTileCoordValid(coord))
                {
                    // 只设置地板层，不覆盖墙壁
                    var tile = _map.GetTile(coord);
                    if (!tile.HasWall)
                    {
                        _map.SetTileLayer(coord, MapConstants.LAYER_FLOOR, TileLayerData.Create(11));
                    }
                }
            }
        }
        
        private void PlaceEntities()
        {
            // 在第一个房子里放置家具
            _map.Entities.CreateEntity(2001, EntityType.Furniture, new TileCoord(12, 12))
                .AddFlag(EntityFlags.Blocking);
            _map.Entities.CreateEntity(2002, EntityType.Furniture, new TileCoord(13, 12));
            _map.Entities.CreateContainer(3001, new TileCoord(16, 14), 30);
        }
        
        private void InitializeRendering()
        {
            var rendererGo = new GameObject("MapRenderer");
            _mapRenderer = rendererGo.AddComponent<MapRenderer>();
            _mapRenderer.UseLighting = _useLighting;
            _mapRenderer.Initialize(_map);
        }
        
        private void InitializePathfinding()
        {
            PathfindingManager.Instance.Initialize(_map);
        }
        
        private void InitializeEditor()
        {
            var editorGo = new GameObject("MapEditor");
            _mapEditor = editorGo.AddComponent<RuntimeMapEditor>();
            _editorUI = editorGo.AddComponent<MapEditorUI>();
            
            // 通过反射设置引用（实际项目中应该使用 SerializeField）
            var editorField = typeof(MapEditorUI).GetField("_editor", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            editorField?.SetValue(_editorUI, _mapEditor);
            
            var rendererField = typeof(MapEditorUI).GetField("_mapRenderer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rendererField?.SetValue(_editorUI, _mapRenderer);
            
            _mapEditor.Initialize(_map, _mapRenderer);
        }
        
        private void SetupCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            if (_camera == null) return;
            
            _camera.orthographic = true;
            _camera.orthographicSize = 12f;
            
            float centerX = _mapWidth * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE * 0.5f;
            float centerY = _mapHeight * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE * 0.5f;
            _camera.transform.position = new Vector3(centerX, centerY, -10);
        }
        
        #endregion
        
        #region 输入处理
        
        private void HandleInput()
        {
            // 相机移动
            HandleCameraMovement();
            
            // 快捷键
            HandleShortcuts();
            
            // 寻路测试
            if (_enablePathfinding && !_enableEditor)
            {
                HandlePathfindingInput();
            }
        }
        
        private void HandleCameraMovement()
        {
            if (_camera == null) return;
            
            float speed = 15f * Time.deltaTime;
            Vector3 move = Vector3.zero;
            
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
                move.y += speed;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
                move.y -= speed;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
                move.x -= speed;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
                move.x += speed;
            
            _camera.transform.position += move;
            
            // 缩放
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _camera.orthographicSize = Mathf.Clamp(
                    _camera.orthographicSize - scroll * 3f,
                    3f, 30f
                );
            }
        }
        
        private void HandleShortcuts()
        {
            // Tab - 切换编辑器
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) && _mapEditor != null)
            {
                _mapEditor.SetEnabled(!_mapEditor.IsEnabled);
                _editorUI?.SetVisible(_mapEditor.IsEnabled);
            }
            
            // I - 切换室内模式
            if (UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                bool visible = _mapRenderer.TileRenderer.IsGlobalLayerVisible(MapConstants.LAYER_ROOF);
                _mapRenderer.SetRoofVisible(!visible);
            }
            
            // F5 - 快速保存
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5))
            {
                QuickSave();
            }
            
            // F9 - 快速加载
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                QuickLoad();
            }
        }
        
        private void HandlePathfindingInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = _camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                TileCoord coord = MapCoordUtility.WorldToTile(worldPos);
                
                if (!_map.IsTileCoordValid(coord)) return;
                
                if (!_pathStart.HasValue)
                {
                    _pathStart = coord;
                    Debug.Log($"寻路起点: {coord}");
                }
                else
                {
                    _pathEnd = coord;
                    Debug.Log($"寻路终点: {coord}");
                    
                    // 执行寻路
                    _currentPath = PathfindingManager.Instance.FindPath(_pathStart.Value, _pathEnd.Value);
                    
                    if (_currentPath.Success)
                    {
                        Debug.Log($"找到路径: {_currentPath.Path.Count} 个节点, 消耗: {_currentPath.TotalCost:F2}");
                    }
                    else
                    {
                        Debug.Log($"寻路失败: {_currentPath.FailureReason}");
                    }
                    
                    // 重置
                    _pathStart = null;
                    _pathEnd = null;
                }
            }
            
            // 右键清除
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                _pathStart = null;
                _pathEnd = null;
                _currentPath = null;
            }
        }
        
        #endregion
        
        #region 保存/加载
        
        private void QuickSave()
        {
            MapSaveSystem.Instance.SaveMapToFile(_map, "QuickSave.json");
            Debug.Log("快速保存完成");
        }
        
        private void QuickLoad()
        {
            if (MapSaveSystem.Instance.SaveFileExists("QuickSave.json"))
            {
                MapSaveSystem.Instance.LoadMapFromFile("QuickSave.json", _map);
                _mapRenderer?.ForceRefreshAll();
                PathfindingManager.Instance.ClearCache();
                Debug.Log("快速加载完成");
            }
            else
            {
                Debug.LogWarning("没有找到快速存档");
            }
        }
        
        #endregion
        
        #region 调试绘制
        
        void OnDrawGizmos()
        {
            // 绘制寻路路径
            if (_currentPath != null && _currentPath.Success)
            {
                Gizmos.color = Color.green;
                
                for (int i = 0; i < _currentPath.Path.Count - 1; i++)
                {
                    Vector3 from = (Vector3)(Vector2)MapCoordUtility.TileToWorld(_currentPath.Path[i]);
                    Vector3 to = (Vector3)(Vector2)MapCoordUtility.TileToWorld(_currentPath.Path[i + 1]);
                    
                    Gizmos.DrawLine(from, to);
                    Gizmos.DrawWireSphere(from, 0.15f);
                }
                
                if (_currentPath.Path.Count > 0)
                {
                    Vector3 end = (Vector3)(Vector2)MapCoordUtility.TileToWorld(
                        _currentPath.Path[_currentPath.Path.Count - 1]);
                    Gizmos.DrawWireSphere(end, 0.15f);
                }
            }
            
            // 绘制寻路起点
            if (_pathStart.HasValue)
            {
                Gizmos.color = Color.blue;
                Vector3 pos = (Vector3)(Vector2)MapCoordUtility.TileToWorld(_pathStart.Value);
                Gizmos.DrawWireSphere(pos, 0.3f);
            }
        }
        
        #endregion
        
        #region 辅助
        
        private void PrintInstructions()
        {
            Debug.Log(@"
=== 控制说明 ===

【相机】
WASD / 方向键: 移动
鼠标滚轮: 缩放

【快捷键】
Tab: 切换编辑器
I: 切换室内模式（显示/隐藏屋顶）
F5: 快速保存
F9: 快速加载

【寻路测试】（编辑器关闭时）
左键: 设置起点/终点
右键: 清除路径

【编辑器】
1-6: 切换编辑层
[ ]: 调整笔刷大小
Ctrl+Z: 撤销
Ctrl+Y: 重做

================
");
        }
        
        #endregion
    }
}
