# TycoonGame 地图系统 - Site模块

## 概述

Site模块是地图系统的核心容器层，管理多楼层场景结构和楼层间的连接关系。

## 文件结构

```
MapSystem/
└── Site/
    ├── SiteConfig.cs              # 场景配置（尺寸、楼层、种子等）
    ├── Site.cs                    # 场景容器（管理多楼层）
    ├── Floor.cs                   # 单层楼层
    ├── TileGrid.cs                # Tile数据网格
    ├── FloorConnectionManager.cs  # 楼层连接管理器
    └── README.md
```

## 核心类

### SiteConfig（场景配置）

```csharp
// 创建配置
var config = new SiteConfig(100, 100, -1, 2);  // 100x100, 地下1层到地上2层
config.SiteId = "my_site";
config.SiteName = "我的基地";
config.Seed = 12345;

// 使用预设
var smallConfig = SiteConfig.CreateSmall();    // 50x50, 0-1层
var mediumConfig = SiteConfig.CreateMedium();  // 100x100, -1到2层
var largeConfig = SiteConfig.CreateLarge();    // 200x200, -2到3层

// 派生属性
int floorCount = config.FloorCount;      // 楼层总数
int cellCount = config.CellCount;        // 单层格子数
bool hasBasement = config.HasUnderground; // 是否有地下层
```

### Site（场景容器）

```csharp
// 创建场景
var site = new Site(config);
site.Initialize();
site.FillWithDefaults();

// 访问楼层
Floor groundFloor = site.GroundFloor;           // 地面层
Floor basement = site.GetFloor(-1);             // 地下1层
Floor floor2 = site[2];                         // 索引器访问

// 遍历楼层
foreach (var floor in site.FloorsAscending)     // 从低到高
foreach (var floor in site.FloorsDescending)    // 从高到低

// 全局坐标访问
var terrain = site.GetTerrain(new GlobalCoord(5, 0, 5));
site.SetWall(new GlobalCoord(10, 1, 10), "Wall_Stone");
bool passable = site.IsPassable(coord);
int pathCost = site.GetPathCost(coord);

// 世界坐标转换
GlobalCoord coord = site.WorldToGlobal(worldPosition);
Vector3 worldPos = site.GlobalToWorld(coord);
```

### Floor（单层楼层）

```csharp
Floor floor = site.GetFloor(0);

// 基本属性
int sizeX = floor.SizeX;
int sizeZ = floor.SizeZ;
bool isUnderground = floor.IsUnderground;
bool isGround = floor.IsGroundFloor;

// Tile访问
TerrainDef terrain = floor.GetTerrain(cell);
floor.SetTerrain(cell, "Terrain_Grass");
FloorDef floorTile = floor.GetFloor(cell);
WallDef wall = floor.GetWall(cell);
RoofDef roof = floor.GetRoof(cell);

// 综合查询
BearingCapacity bearing = floor.GetBearingCapacity(cell);
Passability pass = floor.GetPassability(cell);
int pathCost = floor.GetPathCost(cell);
bool hasRoof = floor.HasRoof(cell);
bool indoors = floor.IsIndoors(cell);
bool canBuild = floor.CanBuildAt(cell, BearingCapacity.Medium);

// 遍历格子
foreach (var cell in floor.AllCells())
foreach (var cell in floor.CellsInRect(min, max))
foreach (var cell in floor.EdgeCells())
```

### TileGrid（Tile数据网格）

```csharp
TileGrid terrainGrid = floor.TerrainGrid;
TileGrid wallGrid = floor.GetTileGrid(TileLayer.Wall);

// 数据访问
string defId = terrainGrid.GetDefId(cell);
TerrainDef def = terrainGrid.GetDef<TerrainDef>(cell);

// 设置数据
terrainGrid.SetTile(cell, "Terrain_Water");

// 批量操作
terrainGrid.Fill("Terrain_Grass");
terrainGrid.FillRect(min, max, "Terrain_Stone");
terrainGrid.Clear();

// 查询
var grassCells = terrainGrid.FindCellsWithDefId("Terrain_Grass");
int waterCount = terrainGrid.CountDefId("Terrain_Water");
var usedDefs = terrainGrid.GetUsedDefIds();

// 邻居遮罩（用于自动Tile）
int mask4 = terrainGrid.GetSameNeighborMask4(cell);  // 4方向
int mask8 = terrainGrid.GetSameNeighborMask8(cell);  // 8方向
```

### FloorConnectionManager（楼层连接）

```csharp
var connManager = site.ConnectionManager;

// 创建连接器
var stairs = connManager.CreateStairs(position, 0, new IntVec2(2, 3), Rotation.North);
var ladder = connManager.CreateLadder(position, 0);
var elevator = connManager.CreateElevator(position, -1, 3, new IntVec2(2, 2));
var hole = connManager.CreateHole(position, 1);

// 查询连接器
var connector = connManager.GetConnectorAt(position, floorIndex);
var floorConnectors = connManager.GetConnectorsOnFloor(0);
var elevators = connManager.GetConnectorsByType(FloorConnectorType.Elevator);

// 连通性检查
bool connected = connManager.AreFloorsConnected(0, 2);
var floorPath = connManager.FindFloorPath(-1, 2);  // [-1, 0, 1, 2]
bool canReach = connManager.CanReach(from, to);
```

## 数据结构

### 楼层布局

```
Site
├── Config (SiteConfig)
├── Floors[] (按数组索引)
│   ├── Floor[-1] -> _floors[0] (地下1层)
│   ├── Floor[0]  -> _floors[1] (地面层)
│   ├── Floor[1]  -> _floors[2] (1楼)
│   └── Floor[2]  -> _floors[3] (2楼)
└── ConnectionManager
    └── Connectors[]

Floor
├── TileGrids[6]
│   ├── [0] TerrainGrid   (地形)
│   ├── [1] FoundationGrid (地基)
│   ├── [2] FloorGrid     (地板)
│   ├── [3] CoverGrid     (覆盖物)
│   ├── [4] WallGrid      (墙壁)
│   └── [5] RoofGrid      (屋顶)
└── (Future: EntityGrid, RegionGrid, RoomTracker)
```

### 六层Tile系统

| 层级 | 枚举 | 说明 | Def类型 |
|------|------|------|---------|
| 0 | Terrain | 自然地形 | TerrainDef |
| 1 | Foundation | 地基 | FoundationDef |
| 2 | Floor | 人造地板 | FloorDef |
| 3 | Cover | 覆盖物 | CoverDef |
| 4 | Wall | 墙壁/门/窗 | WallDef |
| 5 | Roof | 屋顶 | RoofDef |

### 承重等级计算

```
最终承重 = max(地形承重, 地基提供, 地板提供)

示例：
- 水域(None) = None（不可建造）
- 沙地(Light) = Light
- 沙地(Light) + 石地基(Medium) = Medium
- 沙地(Light) + 石地基(Medium) + 钢地板(Heavy) = Heavy
```

## 使用流程

### 创建新场景

```csharp
// 1. 创建配置
var config = new SiteConfig(100, 100, -1, 2)
{
    SiteId = "base_001",
    SiteName = "主基地",
    Seed = 12345,
    BiomeId = "temperate_forest"
};

// 2. 添加预设建筑
config.PresetBuildings.Add(new PresetBuildingConfig
{
    PresetDefId = "StartingHouse",
    Position = new CellCoord(50, 50),
    FloorIndex = 0,
    Required = true
});

// 3. 创建场景
var site = new Site(config);

// 4. 初始化
site.Initialize();

// 5. 生成内容（或填充默认值）
site.FillWithDefaults();  // 简单填充
// 或使用地图生成器
// MapGenerator.Generate(site, config);
```

### 修改Tile

```csharp
// 方法1：通过Floor
var floor = site.GetFloor(0);
floor.SetTerrain(new CellCoord(10, 10), "Terrain_Water");
floor.SetWall(new CellCoord(20, 20), "Wall_Stone");

// 方法2：通过Site（全局坐标）
site.SetTerrain(new GlobalCoord(10, 0, 10), "Terrain_Water");
site.SetWall(new GlobalCoord(20, 0, 20), "Wall_Stone");

// 方法3：直接操作TileGrid
floor.TerrainGrid.SetTile(cell, "Terrain_Grass");
floor.TerrainGrid.FillRect(min, max, "Terrain_Stone");
```

### 添加楼层连接

```csharp
var connManager = site.ConnectionManager;

// 在(50,50)位置添加连接地面层和1楼的楼梯
connManager.CreateStairs(
    new CellCoord(50, 50), 
    lowerFloor: 0, 
    size: new IntVec2(2, 3), 
    rotation: Rotation.North
);

// 添加电梯连接所有楼层
connManager.CreateElevator(
    new CellCoord(45, 45),
    bottomFloor: -1,
    topFloor: 2,
    size: new IntVec2(2, 2)
);
```

## 性能说明

- TileGrid使用一维数组存储，O(1)访问
- Def查询带缓存，避免重复查找
- 连接器使用多重索引（楼层、位置）
- 楼层连通性使用BFS，结果可缓存

## 下一步

Site/Floor基础结构完成后，接下来将实现：
1. Entity系统（游戏实体）
2. EntityGrid（实体空间索引）
3. Region系统（寻路优化）
4. Room系统（房间检测）

---

*TycoonGame MapSystem v1.0*
