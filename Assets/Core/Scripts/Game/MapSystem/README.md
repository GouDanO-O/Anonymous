# TycoonGame 地图系统 - 坐标模块

## 概述

本模块提供了类RimWorld游戏的多楼层地图系统所需的坐标系统实现。

## 文件结构

```
MapSystem/
├── Coords/
│   ├── CellCoord.cs      # 单层格子坐标 (X, Z)
│   ├── GlobalCoord.cs    # 全局坐标 (X, Y楼层, Z)
│   ├── Direction.cs      # 方向枚举 (4方向/8方向)
│   ├── Rotation.cs       # 旋转结构体 (0°/90°/180°/270°)
│   ├── IntVec2.cs        # 二维整数向量 (用于尺寸)
│   └── CoordUtility.cs   # 坐标工具类
├── Defs/
│   ├── MapEnums.cs       # 核心枚举 (承重/通行性/层级等)
│   ├── DefBase.cs        # Def基类
│   ├── TileDefs.cs       # Tile定义 (地形/地板/墙壁/屋顶)
│   ├── EntityDef.cs      # 实体定义 (建筑/物品)
│   ├── DefDatabase.cs    # Def数据库管理器
│   ├── DefaultDefs.cs    # 默认Def定义
│   └── README.md         # Def系统文档
└── TycoonGame.MapSystem.asmdef
```

## 模块说明

### Phase 1.1: 坐标系统 ✅

详见 `Coords/` 目录下的文件。

提供：
- CellCoord: 单层格子坐标
- GlobalCoord: 跨楼层全局坐标
- Direction/Rotation: 方向和旋转
- CoordUtility: 坐标工具方法

### Phase 1.2: Def系统 ✅

详见 `Defs/README.md`。

提供：
- DefBase: 所有定义的基类
- TileDef: 六层Tile定义（Terrain/Foundation/Floor/Cover/Wall/Roof）
- EntityDef: 实体定义（Building/Item）
- DefDatabase: Def管理器，支持Luban集成
- DefaultDefs: 预设的默认定义

### Phase 1.3: Site/Floor基础结构 ✅

详见 `Site/README.md`。

提供：
- SiteConfig: 场景配置（尺寸、楼层范围、种子等）
- Site: 场景容器（管理多楼层）
- Floor: 单层楼层（包含六层TileGrid）
- TileGrid: Tile数据网格
- FloorConnectionManager: 楼层连接管理（楼梯/电梯/梯子）

### Phase 2: Entity系统 ✅

详见 `Entity/README.md`。

提供：
- Entity: 实体基类（位置、生命值、组件）
- EntityComp: 组件系统（CompPower/CompStorage/CompFlickable等）
- Building: 建筑实体（电力、建造、开关）
- Item: 物品实体（堆叠、品质、腐烂）
- EntityGrid: 空间索引（按位置查询）
- EntityLister: 分类索引（按类型查询）
- EntityManager: 实体管理器（创建、生成、销毁）

### Phase 3: Region/Room/Pathfinding系统 ✅

详见 `Region/README.md`。

提供：
- Region: 区域划分（寻路优化）
- RegionGrid: 区域网格管理
- RegionLink: 区域连接
- Room: 房间检测（美观度、清洁度、温度等）
- RoomManager: 房间管理器
- Pathfinder: A*寻路算法（支持单层和跨楼层）

### Phase 4: Rendering渲染系统 ✅

详见 `Rendering/README.md`。

提供：
- MapRenderer: 地图渲染管理器
- TileRenderer: Tile渲染器（六层支持）
- EntityRenderer: 实体渲染器
- DebugRenderer: 调试渲染器（Region/Room/Path可视化）
- SpriteManager: 精灵资源管理
- CameraController: 俯视角相机控制
- MapTestScene: 测试场景快速启动

## 快速开始

```csharp
using TycoonGame.MapSystem;

// 1. 初始化Def系统
DefaultDefs.RegisterAll();
DefDatabase.InitializeAll();

// 2. 创建场景配置
var config = SiteConfig.CreateMedium();  // 100x100, -1到2层
config.Seed = 12345;

// 3. 创建场景
var site = new Site(config);
site.Initialize();
site.FillWithDefaults();

// 4. 访问数据
Floor ground = site.GroundFloor;
TerrainDef terrain = ground.GetTerrain(new CellCoord(50, 50));
ground.SetWall(new CellCoord(10, 10), DefaultDefs.WallStone);

// 5. 添加楼层连接
site.ConnectionManager.CreateStairs(
    new CellCoord(50, 50), 
    lowerFloor: 0, 
    size: new IntVec2(2, 3), 
    rotation: Rotation.North
);

// 6. 生成实体
var table = site.EntityManager.SpawnEntity(
    "Building_Table",
    site.GroundFloor,
    new CellCoord(20, 20),
    Rotation.North
);

// 7. 查询实体
var buildings = site.EntityManager.GetEntityLister(0).Buildings;
var atPos = site.EntityManager.GetEntitiesAt(new GlobalCoord(20, 0, 20));

// 8. 构建区域和房间
site.GroundFloor.RebuildRegionsAndRooms();
Room room = site.GroundFloor.RoomManager.GetRoomAt(new CellCoord(25, 25));
bool isIndoors = room?.IsIndoors ?? false;

// 9. 寻路
var pathResult = site.Pathfinder.FindPath(
    site.GroundFloor,
    new CellCoord(10, 10),
    new CellCoord(50, 50)
);

if (pathResult.Success)
{
    foreach (var cell in pathResult.Path)
    {
        // 沿路径移动
    }
}

// 10. 跨楼层寻路
var globalPath = site.Pathfinder.FindPath(
    new GlobalCoord(10, 0, 10),   // 地面层
    new GlobalCoord(50, 2, 50)    // 2楼
);

// 11. 创建渲染系统（需要在MonoBehaviour中）
// 方法1：使用测试场景（最简单）
// - 创建空GameObject，挂载MapTestScene脚本，运行即可

// 方法2：手动设置
var cameraGO = new GameObject("Camera");
var camera = cameraGO.AddComponent<Camera>();
camera.orthographic = true;
camera.transform.position = new Vector3(32, 50, 32);
camera.transform.rotation = Quaternion.Euler(90, 0, 0);

var cameraController = cameraGO.AddComponent<CameraController>();
cameraController.SetSite(site);

var rendererGO = new GameObject("MapRenderer");
var mapRenderer = rendererGO.AddComponent<MapRenderer>();
mapRenderer.SetSite(site);

// 12. 切换楼层、调试显示
mapRenderer.GoUpFloor();
mapRenderer.ShowRegionOverlay = true;
mapRenderer.ShowRoomOverlay = true;
```

## 坐标系说明

### 坐标系定义
- **X轴**: 水平方向（屏幕左→右）
- **Y轴**: 楼层索引（非高度），0=地面层，正=地上，负=地下
- **Z轴**: 垂直方向（屏幕下→上，俯视角）
- **原点**: 地图左下角

### 坐标类型

| 类型 | 用途 | 示例 |
|------|------|------|
| `CellCoord` | 单层内的格子位置 | `(5, 3)` |
| `GlobalCoord` | 跨楼层的完整位置 | `(5, F2, 3)` = 2楼的(5,3) |
| `IntVec2` | 尺寸表示 | `3x2` |

## 快速开始

### 基础使用

```csharp
using TycoonGame.MapSystem;

// 创建坐标
CellCoord cell = new CellCoord(5, 3);
GlobalCoord global = new GlobalCoord(5, 2, 3); // X=5, 楼层=2, Z=3

// 坐标转换
int index = cell.ToIndex(mapSizeX);              // 格子 → 索引
CellCoord back = CellCoord.FromIndex(index, mapSizeX); // 索引 → 格子

// 世界坐标转换
Vector2 worldPos = cell.ToWorldPosition2D();     // 格子中心
CellCoord fromWorld = CellCoord.FromWorldPosition(worldPos);
```

### 邻居查询

```csharp
// 获取四方向邻居
foreach (var neighbor in cell.GetNeighbors4())
{
    // 北、东、南、西
}

// 获取八方向邻居
foreach (var neighbor in cell.GetNeighbors8())
{
    // 包含对角线
}

// 获取指定范围内的邻居
foreach (var nearby in cell.GetNeighborsInRange(3))
{
    // 曼哈顿距离<=3的所有格子
}
```

### 距离计算

```csharp
CellCoord a = new CellCoord(0, 0);
CellCoord b = new CellCoord(3, 4);

int manhattan = a.ManhattanDistance(b);   // 7 (|3| + |4|)
int chebyshev = a.ChebyshevDistance(b);   // 4 (max(3, 4))
float euclidean = a.Distance(b);          // 5 (√(3² + 4²))
```

### 方向和旋转

```csharp
// 方向操作
Direction dir = Direction.North;
CellCoord offset = dir.ToOffset();        // (0, 1)
Direction opposite = dir.Opposite();      // South
Direction rotated = dir.RotateCW();       // East

// 从两点计算方向
Direction toTarget = DirectionExtensions.DirectionFromTo(from, to);

// 旋转操作
Rotation rot = Rotation.East;             // 90°
CellCoord rotatedCoord = rot.RotateCoord(new CellCoord(1, 0)); // (0, -1)
Vector2Int rotatedSize = rot.RotateSize(new Vector2Int(3, 2)); // (2, 3)
```

### 跨楼层操作

```csharp
GlobalCoord pos = new GlobalCoord(5, 2, 3);

// 楼层操作
GlobalCoord above = pos.FloorAbove;        // (5, 3, 3)
GlobalCoord below = pos.FloorBelow;        // (5, 1, 3)
GlobalCoord moved = pos.WithFloor(0);      // (5, 0, 3) 移到地面层

// 提取单层坐标
CellCoord cell = pos.ToCellCoord();        // (5, 3)

// 检查关系
bool sameFloor = pos.SameFloor(other);
bool sameVertical = pos.SameVerticalLine(other);
```

### 范围查询

```csharp
// 矩形范围
var cells = CoordUtility.GetCellsInRect(min, max);

// 圆形范围
var cells = CoordUtility.GetCellsInCircle(center, radius);

// 环形范围
var cells = CoordUtility.GetCellsInRing(center, innerRadius, outerRadius);

// Bresenham直线
var line = CoordUtility.GetCellsOnLine(from, to);

// 视线检查
bool hasLOS = CoordUtility.HasLineOfSight(from, to, cell => IsBlocked(cell));
```

### 多格子实体

```csharp
IntVec2 size = new IntVec2(3, 2);          // 3x2 的建筑
CellCoord origin = new CellCoord(5, 5);
Rotation rotation = Rotation.East;

// 获取占据的所有格子
foreach (var cell in CoordUtility.GetOccupiedCells(origin, size, rotation))
{
    // 处理每个格子
}

// 获取边界
var (min, max) = CoordUtility.GetEntityBounds(origin, size, rotation);

// 检查重叠
bool overlaps = CoordUtility.RectsOverlap(minA, maxA, minB, maxB);
```

## 性能说明

- `CellCoord` 和 `GlobalCoord` 都是 `struct`，避免堆分配
- 大量运算时使用迭代器版本（`IEnumerable`）避免创建列表
- 哈希计算已优化，适合作为字典键

## 与Unity的集成

所有坐标类型都支持与Unity类型的隐式/显式转换：

```csharp
// CellCoord ↔ Vector2Int
Vector2Int v = cell;
CellCoord c = (CellCoord)v;

// GlobalCoord ↔ Vector3Int
Vector3Int v = global;
GlobalCoord g = v;

// Rotation ↔ Direction
Direction dir = rotation;
Rotation rot = Direction.North;

// 世界坐标
Vector3 worldPos = global.ToWorldPosition(cellSize, floorHeight);
Quaternion rotation = rot.ToQuaternion();
```

## 下一步

坐标系统完成后，接下来将实现：
1. Def系统基础（实体定义、地形定义等）
2. Site/Floor基础结构
3. TileGrids六层实现

---

*TycoonGame MapSystem v1.0*
