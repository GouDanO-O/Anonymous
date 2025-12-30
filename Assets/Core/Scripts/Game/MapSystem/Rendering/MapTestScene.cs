/*******************************************************************************
 * 文件名:    MapTestScene.cs
 * 描述:      地图系统测试场景，快速创建测试环境
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   将此脚本挂载到空GameObject上即可快速测试地图系统：
 *   1. 自动创建Site、Floor、Entity
 *   2. 自动设置渲染系统
 *   3. 提供测试UI
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// 地图测试场景
    /// </summary>
    public class MapTestScene : MonoBehaviour
    {
        #region 序列化字段

        [Header("地图设置")]
        [SerializeField]
        private int _mapSizeX = 64;

        [SerializeField]
        private int _mapSizeZ = 64;

        [SerializeField]
        private int _floorCount = 3;

        [SerializeField]
        private string _siteName = "TestSite";

        [Header("生成设置")]
        [SerializeField]
        private bool _generateTerrain = true;

        [SerializeField]
        private bool _generateBuildings = true;

        [SerializeField]
        private bool _generateItems = true;

        [SerializeField]
        private int _buildingCount = 10;

        [SerializeField]
        private int _itemCount = 20;

        [Header("调试")]
        [SerializeField]
        private bool _showTestUI = true;

        #endregion

        #region 字段

        private Site _site;
        private MapRenderer _mapRenderer;
        private CameraController _cameraController;

        // 测试用
        private CellCoord _pathStart;
        private CellCoord _pathEnd;
        private bool _selectingPathStart = false;
        private bool _selectingPathEnd = false;

        #endregion

        #region 属性

        public Site Site => _site;
        public MapRenderer MapRenderer => _mapRenderer;

        #endregion

        #region Unity生命周期

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnGUI()
        {
            if (_showTestUI)
            {
                DrawTestUI();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化测试场景
        /// </summary>
        private void Initialize()
        {
            // 注册默认Def
            DefaultDefs.RegisterAll();

            // 创建Site
            CreateSite();

            // 生成内容
            if (_generateTerrain)
                GenerateTerrain();
            if (_generateBuildings)
                GenerateBuildings();
            if (_generateItems)
                GenerateItems();

            // 构建区域和房间
            foreach (var floor in _site.AllFloors)
            {
                floor.RebuildRegionsAndRooms();
            }

            // 创建渲染系统
            CreateRenderingSystem();

            Debug.Log($"MapTestScene initialized: {_siteName} ({_mapSizeX}x{_mapSizeZ}, {_floorCount} floors)");
        }

        /// <summary>
        /// 创建Site
        /// </summary>
        private void CreateSite()
        {
            // 计算楼层范围
            // 例如：_floorCount=3, basementCount=1 → MinFloor=-1, MaxFloor=1 (共3层: -1, 0, 1)
            int basementCount = 1;
            int minFloor = -basementCount;
            int maxFloor = _floorCount - basementCount - 1;
            
            var config = new SiteConfig
            {
                SiteName = _siteName,
                SizeX = _mapSizeX,
                SizeZ = _mapSizeZ,
                MinFloor = minFloor,
                MaxFloor = maxFloor,
                CellSize = 1f,
                FloorHeight = 3f
            };

            Debug.Log($"[MapTestScene] Creating site: {_mapSizeX}x{_mapSizeZ}, floors {minFloor} to {maxFloor} (total {config.FloorCount})");

            _site = new Site(config);
            _site.Initialize();
        }

        /// <summary>
        /// 创建渲染系统
        /// </summary>
        private void CreateRenderingSystem()
        {
            // 创建相机
            var cameraGO = new GameObject("MainCamera");
            cameraGO.tag = "MainCamera";
            var camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 15;
            camera.transform.position = new Vector3(_mapSizeX / 2f, 50, _mapSizeZ / 2f);
            camera.transform.rotation = Quaternion.Euler(90, 0, 0);
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            // 添加相机控制器
            _cameraController = cameraGO.AddComponent<CameraController>();
            _cameraController.SetSite(_site);

            // 创建MapRenderer
            var rendererGO = new GameObject("MapRenderer");
            _mapRenderer = rendererGO.AddComponent<MapRenderer>();
            _mapRenderer.SetSite(_site);
        }

        #endregion

        #region 地形生成

        /// <summary>
        /// 生成地形
        /// </summary>
        private void GenerateTerrain()
        {
            var floor = _site.GroundFloor;
            if (floor == null)
                return;

            var terrainGrid = floor.TerrainGrid;
            var floorGrid = floor.FloorGrid;
            var wallGrid = floor.WallGrid;
            var roofGrid = floor.RoofGrid;

            // 使用Perlin噪声生成地形
            float scale = 0.1f;
            float offsetX = Random.Range(0f, 1000f);
            float offsetZ = Random.Range(0f, 1000f);

            for (int z = 0; z < _mapSizeZ; z++)
            {
                for (int x = 0; x < _mapSizeX; x++)
                {
                    var cell = new CellCoord(x, z);
                    float noise = Mathf.PerlinNoise(x * scale + offsetX, z * scale + offsetZ);

                    // 根据噪声值设置地形
                    string terrainId;
                    if (noise < 0.2f)
                        terrainId = "Terrain_Water";
                    else if (noise < 0.35f)
                        terrainId = "Terrain_Sand";
                    else if (noise < 0.7f)
                        terrainId = "Terrain_Grass";
                    else if (noise < 0.85f)
                        terrainId = "Terrain_Dirt";
                    else
                        terrainId = "Terrain_Rock";

                    terrainGrid.SetTile(cell, terrainId);
                }
            }

            // 生成一个简单的房子
            GenerateHouse(floor, new CellCoord(20, 20), 10, 8);
            GenerateHouse(floor, new CellCoord(35, 25), 8, 6);
        }

        /// <summary>
        /// 生成房子
        /// </summary>
        private void GenerateHouse(Floor floor, CellCoord origin, int width, int height)
        {
            var floorGrid = floor.FloorGrid;
            var wallGrid = floor.WallGrid;
            var roofGrid = floor.RoofGrid;

            // 铺地板
            for (int z = origin.z; z < origin.z + height; z++)
            {
                for (int x = origin.x; x < origin.x + width; x++)
                {
                    var cell = new CellCoord(x, z);
                    floorGrid.SetTile(cell, "Floor_Wood");
                    roofGrid.SetTile(cell, "Roof_Basic");
                }
            }

            // 建墙
            for (int x = origin.x; x < origin.x + width; x++)
            {
                wallGrid.SetTile(new CellCoord(x, origin.z), "Wall_Stone");
                wallGrid.SetTile(new CellCoord(x, origin.z + height - 1), "Wall_Stone");
            }
            for (int z = origin.z; z < origin.z + height; z++)
            {
                wallGrid.SetTile(new CellCoord(origin.x, z), "Wall_Stone");
                wallGrid.SetTile(new CellCoord(origin.x + width - 1, z), "Wall_Stone");
            }

            // 门
            int doorX = origin.x + width / 2;
            wallGrid.SetTile(new CellCoord(doorX, origin.z), "Wall_WoodDoor");
        }

        #endregion

        #region 建筑生成

        /// <summary>
        /// 生成建筑
        /// </summary>
        private void GenerateBuildings()
        {
            var entityManager = _site.EntityManager;
            var floor = _site.GroundFloor;
            
            if (floor == null)
            {
                Debug.LogError($"[MapTestScene] GenerateBuildings: GroundFloor is null! MinFloor={_site.MinFloor}, MaxFloor={_site.MaxFloor}");
                return;
            }
            
            if (entityManager == null)
            {
                Debug.LogError("[MapTestScene] GenerateBuildings: EntityManager is null!");
                return;
            }

            // 在房子内生成一些建筑
            var buildingDefs = new string[]
            {
                "Building_Bed",
                "Building_Table",
                "Building_Chair",
                "Building_Lamp"
            };

            int spawnedCount = 0;
            for (int i = 0; i < _buildingCount; i++)
            {
                int attempts = 0;
                while (attempts < 50)
                {
                    int x = Random.Range(22, 28);
                    int z = Random.Range(22, 26);
                    var pos = new CellCoord(x, z);
                    string defId = buildingDefs[Random.Range(0, buildingDefs.Length)];
                    var def = DefDatabase.GetDef<EntityDef>(defId);

                    if (def != null && entityManager.CanPlaceAt(def, floor, pos, Rotation.North))
                    {
                        var building = entityManager.SpawnEntity(defId, floor, pos);
                        if (building is Building b)
                        {
                            b.CompleteConstruction();
                            spawnedCount++;
                        }
                        break;
                    }
                    attempts++;
                }
            }
            
            Debug.Log($"[MapTestScene] GenerateBuildings: spawned {spawnedCount} buildings");
        }

        #endregion

        #region 物品生成

        /// <summary>
        /// 生成物品
        /// </summary>
        private void GenerateItems()
        {
            var entityManager = _site.EntityManager;
            var floor = _site.GroundFloor;
            
            if (floor == null)
            {
                Debug.LogError($"[MapTestScene] GenerateItems: GroundFloor is null! MinFloor={_site.MinFloor}, MaxFloor={_site.MaxFloor}");
                return;
            }
            
            if (entityManager == null)
            {
                Debug.LogError("[MapTestScene] GenerateItems: EntityManager is null!");
                return;
            }

            var itemDefs = new string[]
            {
                "Item_Wood",
                "Item_Steel",
                "Item_Stone",
                "Item_Food"
            };

            int spawnedCount = 0;
            for (int i = 0; i < _itemCount; i++)
            {
                int attempts = 0;
                while (attempts < 50)
                {
                    int x = Random.Range(5, _mapSizeX - 5);
                    int z = Random.Range(5, _mapSizeZ - 5);
                    var pos = new CellCoord(x, z);

                    // 检查地形是否可通行
                    var passability = floor.GetPassability(pos);
                    if (passability == Passability.Passable)
                    {
                        string defId = itemDefs[Random.Range(0, itemDefs.Length)];
                        var item = entityManager.SpawnEntity(defId, floor, pos) as Item;
                        if (item != null)
                        {
                            item.StackCount = Random.Range(1, 50);
                            spawnedCount++;
                        }
                        break;
                    }
                    attempts++;
                }
            }
            
            Debug.Log($"[MapTestScene] GenerateItems: spawned {spawnedCount} items");
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            // 楼层切换
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                _mapRenderer.GoUpFloor();
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                _mapRenderer.GoDownFloor();
            }

            // 调试显示切换
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _mapRenderer.ShowRegionOverlay = !_mapRenderer.ShowRegionOverlay;
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _mapRenderer.ShowRoomOverlay = !_mapRenderer.ShowRoomOverlay;
            }

            // 寻路测试
            if (_selectingPathStart || _selectingPathEnd)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var cell = _cameraController.GetMouseCell();
                    if (cell.IsValid)
                    {
                        if (_selectingPathStart)
                        {
                            _pathStart = cell;
                            _selectingPathStart = false;
                            Debug.Log($"Path start: {_pathStart}");
                        }
                        else if (_selectingPathEnd)
                        {
                            _pathEnd = cell;
                            _selectingPathEnd = false;
                            Debug.Log($"Path end: {_pathEnd}");

                            // 执行寻路
                            TestPathfinding();
                        }
                    }
                }
            }

            // 刷新
            if (Input.GetKeyDown(KeyCode.F5))
            {
                _mapRenderer.RefreshAll();
            }
        }

        /// <summary>
        /// 测试寻路
        /// </summary>
        private void TestPathfinding()
        {
            if (!_pathStart.IsValid || !_pathEnd.IsValid)
                return;

            var floor = _mapRenderer.CurrentFloor;
            var result = _site.Pathfinder.FindPath(floor, _pathStart, _pathEnd);

            if (result.success)
            {
                Debug.Log($"Path found: {result.Length} steps");
                
                // 显示路径
                var debugRenderer = _mapRenderer.GetComponentInChildren<DebugRenderer>();
                debugRenderer?.ShowPath(result.Path);
            }
            else
            {
                Debug.Log($"No path: {result.FailReason}");
            }
        }

        #endregion

        #region 测试UI

        /// <summary>
        /// 绘制测试UI
        /// </summary>
        private void DrawTestUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== 测试控制 ===");

            // 楼层控制
            GUILayout.Label($"当前楼层: {_mapRenderer?.CurrentFloorIndex ?? 0}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("上楼 (PgUp)"))
                _mapRenderer?.GoUpFloor();
            if (GUILayout.Button("下楼 (PgDn)"))
                _mapRenderer?.GoDownFloor();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 调试显示
            GUILayout.Label("调试显示:");
            if (GUILayout.Button($"Region叠加 (F1): {(_mapRenderer?.ShowRegionOverlay ?? false ? "开" : "关")}"))
                if (_mapRenderer != null) _mapRenderer.ShowRegionOverlay = !_mapRenderer.ShowRegionOverlay;
            if (GUILayout.Button($"Room叠加 (F2): {(_mapRenderer?.ShowRoomOverlay ?? false ? "开" : "关")}"))
                if (_mapRenderer != null) _mapRenderer.ShowRoomOverlay = !_mapRenderer.ShowRoomOverlay;

            GUILayout.Space(10);

            // 寻路测试
            GUILayout.Label("寻路测试:");
            if (GUILayout.Button("设置起点"))
            {
                _selectingPathStart = true;
                _selectingPathEnd = false;
            }
            if (GUILayout.Button("设置终点"))
            {
                _selectingPathStart = false;
                _selectingPathEnd = true;
            }
            GUILayout.Label($"起点: {_pathStart}");
            GUILayout.Label($"终点: {_pathEnd}");

            if (_selectingPathStart)
                GUILayout.Label("点击地图选择起点...");
            else if (_selectingPathEnd)
                GUILayout.Label("点击地图选择终点...");

            GUILayout.Space(10);

            // 刷新
            if (GUILayout.Button("刷新渲染 (F5)"))
                _mapRenderer?.RefreshAll();

            // 重建区域
            if (GUILayout.Button("重建区域/房间"))
            {
                var floor = _mapRenderer?.CurrentFloor;
                floor?.RebuildRegionsAndRooms();
                _mapRenderer?.RefreshAll();
            }

            GUILayout.Space(10);

            // 鼠标位置
            var mouseCell = _cameraController?.GetMouseCell() ?? CellCoord.Invalid;
            GUILayout.Label($"鼠标位置: {mouseCell}");

            if (mouseCell.IsValid && _mapRenderer?.CurrentFloor != null)
            {
                var floor = _mapRenderer.CurrentFloor;
                var room = floor.RoomManager?.GetRoomAt(mouseCell);
                if (room != null)
                {
                    GUILayout.Label($"房间: {room.RoomId} ({room.Role})");
                    GUILayout.Label($"  室内: {room.IsIndoors}");
                    GUILayout.Label($"  格子数: {room.CellCount}");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
