# TycoonGame 地图系统 - Region/Room/Pathfinding模块

## 概述

本模块提供区域划分、房间检测和寻路功能，是地图系统的高级组件。

## 文件结构

```
MapSystem/
└── Region/
    ├── Region.cs         # 区域类和区域连接
    ├── RegionGrid.cs     # 区域网格管理
    ├── Room.cs           # 房间类
    ├── RoomManager.cs    # 房间管理器
    ├── Pathfinder.cs     # A*寻路算法
    └── README.md
```

## 核心概念

### Region（区域）

区域是一小块连通的格子集合（最多25格），用于优化寻路和可达性判断。

```csharp
Region region = regionGrid.GetRegionAt(cell);

// 属性
int id = region.RegionId;
RegionType type = region.Type;      // Normal, Portal, Impassable
int cellCount = region.CellCount;
CellCoord center = region.GetCenter();

// 格子操作
bool contains = region.ContainsCell(cell);
CellCoord random = region.GetRandomCell();

// 邻居
foreach (var neighbor in region.GetNeighborRegions())
{
    // 处理相邻区域
}
```

### RegionLink（区域连接）

连接两个相邻区域的边界。

```csharp
RegionLink link = ...;

Region otherRegion = link.GetOtherRegion(currentRegion);
CellCoord closest = link.GetClosestCell(myPosition);
RegionLinkType type = link.LinkType;  // Normal, Door, FloorConnection
```

### Room（房间）

房间是由墙壁包围的封闭区域，包含一个或多个Region。

```csharp
Room room = roomManager.GetRoomAt(cell);

// 基本属性
bool indoors = room.IsIndoors;
bool outdoors = room.IsOutdoors;
RoomRole role = room.Role;  // Bedroom, Kitchen, Storage, etc.

// 统计属性
int cellCount = room.CellCount;
float beauty = room.Beauty;
float cleanliness = room.Cleanliness;
float wealth = room.Wealth;
float temperature = room.Temperature;
bool hasRoof = room.HasRoof;

// 遍历
foreach (var cell in room.GetAllCells())
foreach (var borderCell in room.GetBorderCells())
CellCoord random = room.GetRandomCell();

// 邻居房间
foreach (var neighbor in room.GetNeighborRooms())
```

### Pathfinder（寻路器）

A*算法寻路实现。

```csharp
Pathfinder pathfinder = new Pathfinder(site);

// 单层寻路
PathResult result = pathfinder.FindPath(floor, startCell, goalCell);
if (result.Success)
{
    foreach (var cell in result.Path)
    {
        // 沿路径移动
    }
}

// 跨楼层寻路
PathResult result = pathfinder.FindPath(startGlobal, goalGlobal);
if (result.Success)
{
    foreach (var coord in result.GlobalPath)
    {
        // coord.y 可能在不同楼层
    }
}

// 快速可达性检查
bool canReach = pathfinder.CanReach(floor, from, to);
bool canReachGlobal = pathfinder.CanReach(fromGlobal, toGlobal);

// 寻路选项
var options = new PathfindingOptions
{
    AllowDiagonal = true,
    IgnoreGoalPassability = false,
    CanOpenDoors = true,
    CanUseElevators = true
};
PathResult result = pathfinder.FindPath(floor, start, goal, options);
```

## 使用流程

### 初始化

```csharp
// 在Floor中创建区域系统
RegionGrid regionGrid = new RegionGrid(floor);
RoomManager roomManager = new RoomManager(floor, regionGrid);

// 初始化
roomManager.Initialize();

// 首次构建
regionGrid.RebuildAll();
roomManager.RebuildIfNeeded();
```

### 动态更新

```csharp
// 当地形/墙壁变化时
regionGrid.MarkDirty(changedCell);

// 当门状态变化时
roomManager.NotifyDoorStateChanged(doorCell);

// 每帧或定期更新
regionGrid.RebuildIfNeeded();
roomManager.RebuildIfNeeded();
```

### 寻路示例

```csharp
// 简单寻路
var result = pathfinder.FindPath(floor, pawnPos, targetPos);
if (result.Success)
{
    pawn.SetPath(result.Path);
}
else
{
    Debug.Log($"Cannot reach target: {result.FailReason}");
}

// 跨楼层寻路（例如从地下室到2楼）
var from = new GlobalCoord(pawnPos, -1);  // 地下1层
var to = new GlobalCoord(targetPos, 2);   // 2楼
var result = pathfinder.FindPath(from, to);

if (result.Success)
{
    // 路径可能包含楼层变化
    foreach (var coord in result.GlobalPath)
    {
        if (coord.y != currentFloor)
        {
            // 使用楼梯/电梯
        }
    }
}
```

## 区域类型

| 类型 | 说明 |
|------|------|
| Normal | 普通可通行区域 |
| Portal | 门/通道区域（连接室内外或不同房间） |
| Impassable | 不可通行区域（墙内等） |

## 房间角色

| 角色 | 说明 |
|------|------|
| None | 无特定用途 |
| Bedroom | 卧室 |
| Hospital | 病房 |
| Prison | 监狱 |
| DiningRoom | 餐厅 |
| RecRoom | 娱乐室 |
| Kitchen | 厨房 |
| Storage | 仓库 |
| Research | 研究室 |
| Workshop | 工作间 |
| Barracks | 兵营 |
| Hallway | 大厅/走廊 |
| Temple | 寺庙/祈祷室 |

## 性能优化

### Region系统优势

1. **快速可达性判断**：通过区域连通性，O(区域数)而非O(格子数)
2. **分层寻路**：先找区域路径，再找格子路径
3. **增量更新**：只重建变化的区域

### 建议

- 区域变化时使用`MarkDirty`而非`RebuildAll`
- 缓存常用路径
- 对长距离路径使用区域级预判
- 可达性检查优先使用Region系统

## 层级关系

```
Site
└── Floors[]
    ├── RegionGrid
    │   ├── Regions[] (小块连通区域)
    │   └── RegionLinks[] (区域连接)
    └── RoomManager
        └── Rooms[] (封闭房间)
            └── Regions[] (房间包含的区域)

Pathfinder
├── 单层寻路 (使用Floor)
└── 跨楼层寻路 (使用Site + FloorConnections)
```

---

*TycoonGame MapSystem v1.0*
