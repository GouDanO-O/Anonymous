/**
 * MultiLevelExample.cs
 * 多层地图示例
 * 
 * 演示：
 * - 创建多层建筑
 * - 楼梯连接
 * - 层级切换
 * - 跨层寻路
 */

using UnityEngine;
using GDFramework.MapSystem.Rendering;

namespace GDFramework.MapSystem.MultiLevel
{
    /// <summary>
    /// 多层地图示例
    /// </summary>
    public class MultiLevelExample : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Map Settings")]
        [SerializeField]
        private int _mapWidth = 3;
        
        [SerializeField]
        private int _mapHeight = 3;
        
        [Header("Visual Settings")]
        [SerializeField]
        private bool _useLighting = true;
        
        [SerializeField]
        private bool _renderAdjacentLevels = true;
        
        [Header("References")]
        [SerializeField]
        private Camera _camera;
        
        #endregion
        
        #region 字段
        
        private MultiLevelMap _map;
        private MultiLevelRenderer _renderer;
        private MultiLevelPathfinder _pathfinder;
        
        // 寻路测试
        private LevelCoord? _pathStart;
        private LevelCoord? _pathEnd;
        private MultiLevelPathResult _currentPath;
        
        #endregion
        
        #region Unity 生命周期
        
        void Start()
        {
            Debug.Log("=== 多层地图示例 ===");
            
            // 初始化精灵管理器
            InitializeSprites();
            
            // 创建多层地图
            CreateMultiLevelMap();
            
            // 初始化渲染
            InitializeRendering();
            
            // 初始化寻路
            _pathfinder = new MultiLevelPathfinder(_map);
            
            // 设置相机
            SetupCamera();
            
            PrintInstructions();
        }
        
        void Update()
        {
            HandleInput();
            _map?.Update(Time.deltaTime);
        }
        
        void OnDestroy()
        {
            _renderer?.Cleanup();
        }
        
        #endregion
        
        #region 初始化
        
        private void InitializeSprites()
        {
            var tileConfigs = new TileConfig[]
            {
                TileConfig.CreateTerrain(1, "草地", "terrain_grass"),
                TileConfig.CreateTerrain(2, "泥土", "terrain_dirt"),
                TileConfig.CreateFloor(10, "木地板", "floor_wood"),
                TileConfig.CreateFloor(11, "石地板", "floor_stone"),
                TileConfig.CreateFloor(12, "地下室地板", "floor_basement"),
                TileConfig.CreateWall(20, "木墙", "wall_wood", 100),
                TileConfig.CreateWall(21, "石墙", "wall_stone", 200),
                TileConfig.CreateRoof(30, "屋顶", "roof_wood"),
            };
            
            var entityConfigs = new EntityConfig[]
            {
                EntityConfig.CreateFurniture(2001, "桌子", "furniture_table"),
                EntityConfig.CreateContainer(3001, "柜子", "container_cabinet", 20),
                EntityConfig.CreateDoor(1001, "木门", new[] { "door_closed", "door_open" }),
                new EntityConfig { ConfigId = MapConstants.STAIRS_UP_CONFIG_ID, EntityName = "上行楼梯" },
                new EntityConfig { ConfigId = MapConstants.STAIRS_DOWN_CONFIG_ID, EntityName = "下行楼梯" },
            };
            
            SpriteManager.Instance.Initialize(tileConfigs, entityConfigs);
        }
        
        private void CreateMultiLevelMap()
        {
            _map = new MultiLevelMap("multi_level_demo", "多层演示", _mapWidth, _mapHeight);
            
            // === 地面层 (Level 0) ===
            var ground = _map.GroundLevel;
            ground.FillLayer(MapConstants.LAYER_GROUND, TileLayerData.Create(1)); // 草地
            BuildHouseOnLevel(ground, 10, 10, 12, 10);
            
            // === 二楼 (Level 1) ===
            var floor1 = _map.CreateLevel(1, "二楼", LevelType.Indoor);
            floor1.FillLayer(MapConstants.LAYER_FLOOR, TileLayerData.Create(10)); // 木地板
            BuildFloorPlan(floor1, 10, 10, 12, 10);
            
            // === 地下室 (Level -1) ===
            var basement = _map.CreateLevel(-1, "地下室", LevelType.Basement);
            basement.FillLayer(MapConstants.LAYER_FLOOR, TileLayerData.Create(12)); // 地下室地板
            BuildBasement(basement, 10, 10, 12, 10);
            
            // === 添加楼梯 ===
            // 地面到二楼的楼梯
            _map.AddBidirectionalStairs(new TileCoord(15, 15), 0, 1);
            
            // 地面到地下室的楼梯
            _map.AddBidirectionalStairs(new TileCoord(18, 12), 0, -1);
            
            Debug.Log($"多层地图创建完成: {_map}");
        }
        
        /// <summary>
        /// 在指定层构建房屋
        /// </summary>
        private void BuildHouseOnLevel(MapLevel level, int x, int y, int w, int h)
        {
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var coord = new TileCoord(x + dx, y + dy);
                    if (!level.IsTileCoordValid(coord)) continue;
                    
                    bool isEdge = dx == 0 || dx == w - 1 || dy == 0 || dy == h - 1;
                    bool isDoor = dy == 0 && dx == w / 2;
                    
                    if (isDoor)
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                        level.Entities.CreateDoor(1001, coord, DoorType.Wooden);
                    }
                    else if (isEdge)
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithWall(20)
                            .WithRoof(30));
                    }
                    else
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                    }
                }
            }
        }
        
        /// <summary>
        /// 构建楼层平面
        /// </summary>
        private void BuildFloorPlan(MapLevel level, int x, int y, int w, int h)
        {
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var coord = new TileCoord(x + dx, y + dy);
                    if (!level.IsTileCoordValid(coord)) continue;
                    
                    bool isEdge = dx == 0 || dx == w - 1 || dy == 0 || dy == h - 1;
                    
                    if (isEdge)
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithWall(20)
                            .WithRoof(30));
                    }
                    else
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                    }
                }
            }
            
            // 放置一些家具
            level.Entities.CreateEntity(2001, EntityType.Furniture, new TileCoord(x + 3, y + 3));
        }
        
        /// <summary>
        /// 构建地下室
        /// </summary>
        private void BuildBasement(MapLevel level, int x, int y, int w, int h)
        {
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    var coord = new TileCoord(x + dx, y + dy);
                    if (!level.IsTileCoordValid(coord)) continue;
                    
                    bool isEdge = dx == 0 || dx == w - 1 || dy == 0 || dy == h - 1;
                    
                    if (isEdge)
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(12)
                            .WithWall(21)); // 石墙
                    }
                    else
                    {
                        level.SetTile(coord, TileData.Empty
                            .WithFloor(12));
                    }
                }
            }
            
            // 放置储物柜
            level.Entities.CreateContainer(3001, new TileCoord(x + 2, y + 2), 30);
        }
        
        private void InitializeRendering()
        {
            var rendererGo = new GameObject("MultiLevelRenderer");
            _renderer = rendererGo.AddComponent<MultiLevelRenderer>();
            
            // 设置属性（通过反射，实际项目中使用 SerializeField）
            SetPrivateField(_renderer, "_useLighting", _useLighting);
            SetPrivateField(_renderer, "_renderAdjacentLevels", _renderAdjacentLevels);
            
            _renderer.Initialize(_map);
        }
        
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
        
        private void SetupCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            if (_camera == null) return;
            
            _camera.orthographic = true;
            _camera.orthographicSize = 15f;
            
            float centerX = _mapWidth * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE * 0.5f;
            float centerY = _mapHeight * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE * 0.5f;
            _camera.transform.position = new Vector3(centerX, centerY, -10);
        }
        
        #endregion
        
        #region 输入处理
        
        private void HandleInput()
        {
            HandleCameraMovement();
            HandleLevelSwitch();
            HandlePathfindingTest();
        }
        
        private void HandleCameraMovement()
        {
            if (_camera == null) return;
            
            float speed = 15f * Time.deltaTime;
            Vector3 move = Vector3.zero;
            
            if (UnityEngine.Input.GetKey(KeyCode.W)) move.y += speed;
            if (UnityEngine.Input.GetKey(KeyCode.S)) move.y -= speed;
            if (UnityEngine.Input.GetKey(KeyCode.A)) move.x -= speed;
            if (UnityEngine.Input.GetKey(KeyCode.D)) move.x += speed;
            
            _camera.transform.position += move;
            
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _camera.orthographicSize = Mathf.Clamp(
                    _camera.orthographicSize - scroll * 3f, 5f, 30f);
            }
        }
        
        private void HandleLevelSwitch()
        {
            // Page Up - 上一层
            if (UnityEngine.Input.GetKeyDown(KeyCode.PageUp))
            {
                if (_renderer.GoUp())
                {
                    Debug.Log($"切换到层级: {_renderer.ActiveLevel}");
                }
            }
            
            // Page Down - 下一层
            if (UnityEngine.Input.GetKeyDown(KeyCode.PageDown))
            {
                if (_renderer.GoDown())
                {
                    Debug.Log($"切换到层级: {_renderer.ActiveLevel}");
                }
            }
            
            // 数字键快速切换
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                _renderer.SetActiveLevel(-1); // 地下室
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                _renderer.SetActiveLevel(0); // 地面
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                _renderer.SetActiveLevel(1); // 二楼
            }
            
            // T - 切换相邻层显示
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                _renderAdjacentLevels = !_renderAdjacentLevels;
                _renderer.SetRenderAdjacentLevels(_renderAdjacentLevels);
                Debug.Log($"相邻层显示: {_renderAdjacentLevels}");
            }
        }
        
        private void HandlePathfindingTest()
        {
            // 左键设置寻路点
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = _camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                TileCoord tileCoord = MapCoordUtility.WorldToTile(worldPos);
                LevelCoord levelCoord = new LevelCoord(tileCoord, _renderer.ActiveLevel);
                
                if (!_pathStart.HasValue)
                {
                    _pathStart = levelCoord;
                    Debug.Log($"寻路起点: {levelCoord}");
                }
                else
                {
                    _pathEnd = levelCoord;
                    Debug.Log($"寻路终点: {levelCoord}");
                    
                    // 执行跨层寻路
                    _currentPath = _pathfinder.FindMultiLevelPath(_pathStart.Value, _pathEnd.Value);
                    
                    if (_currentPath.Success)
                    {
                        Debug.Log($"找到路径! 段数: {_currentPath.Segments.Count}, " +
                                  $"转换次数: {_currentPath.TransitionCount}, " +
                                  $"总消耗: {_currentPath.TotalCost:F2}");
                        
                        foreach (var segment in _currentPath.Segments)
                        {
                            Debug.Log($"  - 层级 {segment.Level}: {segment.Path.Count} 步");
                        }
                    }
                    else
                    {
                        Debug.Log($"寻路失败: {_currentPath.FailureReason}");
                    }
                    
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
        
        #region 调试绘制
        
        void OnDrawGizmos()
        {
            // 绘制寻路路径
            if (_currentPath != null && _currentPath.Success)
            {
                Color[] levelColors = { Color.red, Color.green, Color.blue, Color.yellow };
                
                foreach (var segment in _currentPath.Segments)
                {
                    int colorIndex = (segment.Level + 10) % levelColors.Length;
                    Gizmos.color = levelColors[colorIndex];
                    
                    for (int i = 0; i < segment.Path.Count - 1; i++)
                    {
                        Vector3 from = (Vector3)(Vector2)MapCoordUtility.TileToWorld(segment.Path[i]);
                        Vector3 to = (Vector3)(Vector2)MapCoordUtility.TileToWorld(segment.Path[i + 1]);
                        
                        // 根据层级偏移 Y（用于可视化）
                        from.y += segment.Level * 0.1f;
                        to.y += segment.Level * 0.1f;
                        
                        Gizmos.DrawLine(from, to);
                        Gizmos.DrawWireSphere(from, 0.1f);
                    }
                }
            }
            
            // 绘制起点
            if (_pathStart.HasValue)
            {
                Gizmos.color = Color.cyan;
                Vector3 pos = (Vector3)(Vector2)MapCoordUtility.TileToWorld(_pathStart.Value.ToTileCoord());
                Gizmos.DrawWireSphere(pos, 0.3f);
            }
        }
        
        #endregion
        
        #region 辅助
        
        private void PrintInstructions()
        {
            Debug.Log(@"
=== 多层地图控制说明 ===

【移动】
WASD: 移动相机
鼠标滚轮: 缩放

【楼层切换】
Page Up: 上一层
Page Down: 下一层
1: 地下室 (L-1)
2: 地面 (L0)
3: 二楼 (L1)
T: 切换相邻层显示

【寻路测试】
左键: 设置起点/终点
右键: 清除路径

=========================
");
        }
        
        #endregion
    }
}
