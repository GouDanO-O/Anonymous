/**
 * URPRenderingExample.cs
 * URP 渲染系统使用示例
 * 
 * 展示如何在 URP 2D 管线下使用 MapRenderer
 */

using UnityEngine;

namespace GDFramework.MapSystem.Rendering.Examples
{
    /// <summary>
    /// URP 渲染系统使用示例
    /// </summary>
    public class URPRenderingExample : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField]
        private int _mapWidthInChunks = 4;
        
        [SerializeField]
        private int _mapHeightInChunks = 4;
        
        [Header("Rendering Settings")]
        [SerializeField]
        [Tooltip("是否使用 2D 光照（需要 Global Light 2D）")]
        private bool _useLighting = true;
        
        [Header("References")]
        [SerializeField]
        private Camera _camera;
        
        private Map _map;
        private MapRenderer _mapRenderer;
        
        void Start()
        {
            Debug.Log("=== URP 渲染系统示例 ===\n");
            
            // 验证 URP 设置
            ValidateSetup();
            
            // 1. 初始化 SpriteManager（通常从 Luban 加载配置）
            InitializeSpriteManager();
            
            // 2. 创建测试地图
            CreateTestMap();
            
            // 3. 设置渲染器
            SetupRenderer();
            
            // 4. 设置相机
            SetupCamera();
            
            PrintControls();
        }
        
        /// <summary>
        /// 验证 URP 设置
        /// </summary>
        void ValidateSetup()
        {
            // 检查 Sorting Layers
            if (!URPSetupGuide.ValidateSortingLayers())
            {
                Debug.LogError("Sorting Layers 设置不完整！请查看设置指南。");
                URPSetupGuide.PrintSetupInstructions();
            }
            
            // 检查 URP
            URPSetupGuide.ValidateURPSetup();
        }
        
        /// <summary>
        /// 初始化精灵管理器（示例配置）
        /// </summary>
        void InitializeSpriteManager()
        {
            // 创建一些测试配置
            var tileConfigs = new TileConfig[]
            {
                TileConfig.CreateTerrain(1, "草地", "terrain_grass"),
                TileConfig.CreateTerrain(2, "泥土", "terrain_dirt"),
                TileConfig.CreateFloor(10, "木地板", "floor_wood"),
                TileConfig.CreateFloor(11, "石地板", "floor_stone"),
                TileConfig.CreateWall(20, "木墙", "wall_wood", 100),
                TileConfig.CreateWall(21, "石墙", "wall_stone", 200),
                TileConfig.CreateRoof(30, "木屋顶", "roof_wood"),
                TileConfig.CreateWater(5, "水", "terrain_water"),
            };
            
            var entityConfigs = new EntityConfig[]
            {
                EntityConfig.CreateFurniture(2001, "木桌", "furniture_table"),
                EntityConfig.CreateFurniture(2002, "椅子", "furniture_chair", false),
                EntityConfig.CreateContainer(3001, "冰箱", "container_fridge", 30),
                EntityConfig.CreateContainer(3002, "柜子", "container_cabinet", 20),
                EntityConfig.CreateDoor(1001, "木门", new[] { "door_wood_closed", "door_wood_open" }),
            };
            
            SpriteManager.Instance.Initialize(tileConfigs, entityConfigs);
        }
        
        /// <summary>
        /// 创建测试地图
        /// </summary>
        void CreateTestMap()
        {
            _map = new Map("test_map", "测试地图", _mapWidthInChunks, _mapHeightInChunks);
            
            // 填充地形
            _map.FillLayer(MapConstants.LAYER_GROUND, TileLayerData.Create(1)); // 草地
            
            // 建造一些建筑
            BuildHouse(10, 10, 8, 6);
            BuildHouse(25, 15, 6, 5);
            BuildHouse(5, 25, 10, 8);
            
            // 建造道路
            for (int x = 0; x < _map.WidthInTiles; x++)
            {
                _map.SetTileLayer(new TileCoord(x, 8), MapConstants.LAYER_FLOOR, 
                    TileLayerData.Create(11));
            }
            
            // 放置一些实体
            var entities = _map.Entities;
            
            // 房间1内的家具
            entities.CreateEntity(2001, EntityType.Furniture, new TileCoord(12, 12))
                .AddFlag(EntityFlags.Blocking);
            entities.CreateEntity(2002, EntityType.Furniture, new TileCoord(13, 12));
            entities.CreateEntity(2002, EntityType.Furniture, new TileCoord(14, 12));
            
            // 容器
            entities.CreateContainer(3001, new TileCoord(16, 14), 30);
            
            // 门
            entities.CreateDoor(1001, new TileCoord(14, 10), DoorType.Wooden);
            
            Debug.Log($"地图创建完成: {_map}");
            Debug.Log($"实体数量: {entities.EntityCount}");
        }
        
        /// <summary>
        /// 建造简单房屋
        /// </summary>
        void BuildHouse(int startX, int startY, int width, int height)
        {
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    var coord = new TileCoord(x, y);
                    bool isEdge = x == startX || x == startX + width - 1 ||
                                  y == startY || y == startY + height - 1;
                    
                    if (isEdge)
                    {
                        bool isDoor = y == startY && x == startX + width / 2;
                        if (!isDoor)
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
                    else
                    {
                        _map.SetTile(coord, TileData.Empty
                            .WithFloor(10)
                            .WithRoof(30));
                    }
                }
            }
        }
        
        /// <summary>
        /// 设置渲染器
        /// </summary>
        void SetupRenderer()
        {
            // 创建 MapRenderer
            GameObject rendererGo = new GameObject("MapRenderer");
            _mapRenderer = rendererGo.AddComponent<MapRenderer>();
            
            // 设置光照模式
            _mapRenderer.UseLighting = _useLighting;
            
            // 初始化
            _mapRenderer.Initialize(_map);
        }
        
        /// <summary>
        /// 设置相机
        /// </summary>
        void SetupCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            if (_camera == null)
            {
                Debug.LogError("No camera found!");
                return;
            }
            
            // 确保相机是正交模式
            _camera.orthographic = true;
            _camera.orthographicSize = 10f;
            
            // 设置背景色
            _camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            
            // 居中到地图中央
            float centerX = _mapWidthInChunks * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE * 0.5f;
            float centerY = _mapHeightInChunks * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE * 0.5f;
            _camera.transform.position = new Vector3(centerX, centerY, -10);
        }
        
        void PrintControls()
        {
            Debug.Log(@"
=== 控制说明 ===
WASD / 方向键: 移动相机
鼠标滚轮: 缩放
I: 切换室内模式（隐藏/显示屋顶）
L: 切换光照模式
P: 暂停/恢复渲染
R: 强制刷新
鼠标左键: 显示 Tile 信息
================
");
        }
        
        void Update()
        {
            HandleCameraMovement();
            HandleInput();
        }
        
        /// <summary>
        /// 相机移动控制
        /// </summary>
        void HandleCameraMovement()
        {
            if (_camera == null) return;
            
            float speed = 10f * Time.deltaTime;
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
        
        /// <summary>
        /// 输入处理
        /// </summary>
        void HandleInput()
        {
            // I 键 - 切换室内模式（隐藏/显示屋顶）
            if (UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                bool currentRoofVisible = _mapRenderer.TileRenderer.IsGlobalLayerVisible(MapConstants.LAYER_ROOF);
                _mapRenderer.SetRoofVisible(!currentRoofVisible);
                Debug.Log($"屋顶可见性: {!currentRoofVisible}");
            }
            
            // L 键 - 切换光照模式
            if (UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                _useLighting = !_useLighting;
                _mapRenderer.UseLighting = _useLighting;
                Debug.Log($"光照模式: {_useLighting}");
            }
            
            // P 键 - 暂停/恢复渲染更新
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                if (_mapRenderer.IsPaused)
                {
                    _mapRenderer.Resume();
                    Debug.Log("渲染恢复");
                }
                else
                {
                    _mapRenderer.Pause();
                    Debug.Log("渲染暂停");
                }
            }
            
            // R 键 - 强制刷新
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                _mapRenderer.ForceRefreshAll();
                Debug.Log("强制刷新完成");
            }
            
            // 鼠标点击 - 显示 Tile 信息
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = _camera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                TileCoord tileCoord = MapCoordUtility.WorldToTile(worldPos);
                
                if (_map.IsTileCoordValid(tileCoord))
                {
                    TileData tile = _map.GetTile(tileCoord);
                    var entities = _map.Entities.GetEntitiesAt(tileCoord);
                    
                    Debug.Log($"点击位置: {tileCoord}");
                    Debug.Log($"  Tile: {tile}");
                    Debug.Log($"  可行走: {_map.IsWalkable(tileCoord)}");
                    Debug.Log($"  实体数量: {entities.Count}");
                    foreach (var e in entities)
                    {
                        Debug.Log($"    - {e}");
                    }
                }
            }
        }
        
        void OnDestroy()
        {
            if (_mapRenderer != null)
            {
                _mapRenderer.Cleanup();
            }
        }
        
        /// <summary>
        /// 在编辑器中绘制 Gizmos
        /// </summary>
        void OnDrawGizmos()
        {
            if (_map == null) return;
            
            // 绘制地图边界
            Gizmos.color = Color.yellow;
            Vector3 size = new Vector3(
                _map.WidthInTiles * MapConstants.TILE_SIZE,
                _map.HeightInTiles * MapConstants.TILE_SIZE,
                0
            );
            Vector3 center = size * 0.5f;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
