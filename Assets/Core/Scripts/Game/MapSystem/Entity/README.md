# TycoonGame 地图系统 - Entity模块

## 概述

Entity模块是游戏实体系统的核心，管理所有动态游戏对象（建筑、物品、生物等）。

## 文件结构

```
MapSystem/
└── Entity/
    ├── Entity.cs          # 实体基类
    ├── EntityComp.cs      # 组件基类及常用组件
    ├── EntityGrid.cs      # 空间索引（按位置查询）
    ├── EntityLister.cs    # 分类索引（按类型查询）
    ├── EntityManager.cs   # 实体管理器
    ├── EntityTypes.cs     # 特化实体类（Building, Item等）
    └── README.md
```

## 核心类

### Entity（实体基类）

```csharp
// 创建实体
Entity entity = new Entity(entityDef);

// 生成到地图
entity.SpawnSetup(floor, position, rotation);

// 基本属性
int id = entity.EntityId;
string name = entity.Label;
CellCoord pos = entity.Position;
Rotation rot = entity.Rotation;
IntVec2 size = entity.Size;

// 全局坐标
GlobalCoord globalPos = entity.GlobalPosition;

// 生命值
int hp = entity.HitPoints;
int maxHp = entity.MaxHitPoints;
entity.TakeDamage(10);
entity.Heal(5);

// 占据格子
foreach (var cell in entity.OccupiedCells())
bool occupies = entity.OccupiesCell(targetCell);

// 组件访问
var power = entity.GetComp<CompPower>();
bool hasPower = entity.HasComp<CompPower>();

// 销毁
entity.Destroy();
```

### EntityComp（组件系统）

```csharp
// 添加组件
var power = entity.AddComp<CompPower>();
var storage = entity.AddComp<CompStorage>();

// 获取组件
var flickable = entity.GetComp<CompFlickable>();
if (entity.TryGetComp<CompBreakdown>(out var breakdown))
{
    if (breakdown.BrokenDown) breakdown.Repair();
}

// 常用组件
CompPower      // 电力（消耗/产生）
CompStorage    // 存储容器
CompFlickable  // 开关
CompRefuelable // 燃料
CompBreakdown  // 故障
CompLinkable   // 连接（墙壁等）
```

### Building（建筑实体）

```csharp
// 创建建筑
var building = new Building(buildingDef);

// 建造系统
building.StartConstruction();
building.AddConstructionWork(10f);
bool complete = building.ConstructionComplete;
float progress = building.ConstructionProgress;

// 电力系统
bool needsPower = building.RequiresPower;
bool hasPower = building.HasPower;
bool isGenerator = building.IsPowerGenerator;

// 开关
building.ToggleSwitch();
building.SetSwitched(true);
bool isOn = building.SwitchedOn;

// 工作状态
bool working = building.IsWorking; // 完成 && 开启 && 有电

// 拆除
building.Deconstruct(dropItems: true);
```

### Item（物品实体）

```csharp
// 创建物品
var item = new Item(itemDef);
item.StackCount = 10;
item.Quality = QualityLevel.Good;

// 堆叠
int stacked = item.TryStackWith(otherItem);
bool canStack = item.CanStackWith(otherItem);
Item split = item.SplitOff(5);

// 属性
int count = item.StackCount;
int max = item.MaxStackCount;
bool full = item.IsFullStack;
float mass = item.TotalMass;
float value = item.TotalValue;

// 品质
QualityLevel quality = item.Quality;
bool hasQuality = item.HasQuality;

// 腐烂
bool canRot = item.CanRot;
bool rotten = item.IsRotten;
float daysLeft = item.RotDaysRemaining;

// 消耗
int consumed = item.Consume(5);
```

### EntityGrid（空间索引）

```csharp
EntityGrid grid = floor.EntityGrid;

// 按位置查询
var entities = grid.GetEntitiesAt(cell);
var first = grid.GetFirstEntityAt(cell);
var blocking = grid.GetBlockingEntityAt(cell);
bool hasEntity = grid.HasEntityAt(cell);
bool blocked = grid.IsBlockedAt(cell);

// 按类型查询
var building = grid.GetEntityAt<Building>(cell);

// 区域查询
var inRect = grid.GetEntitiesInRect(min, max);
var inRadius = grid.GetEntitiesInRadius(center, radius);
var nearest = grid.GetNearestEntity(from, maxRadius);
var nearestBuilding = grid.GetNearestEntity<Building>(from);

// 放置检查
bool canPlace = grid.CanPlaceAt(def, position, rotation);
string reason = grid.GetPlaceFailReason(def, position, rotation);

// 通行性
Passability pass = grid.GetEntityPassability(cell);
Passability combined = grid.GetCombinedPassability(cell);
int pathCost = grid.GetCombinedPathCost(cell);
```

### EntityLister（分类索引）

```csharp
EntityLister lister = floor.EntityLister;

// 按分类
var buildings = lister.Buildings;
var items = lister.Items;
var pawns = lister.Pawns;
var byCategory = lister.GetByCategory(EntityCategory.Plant);

// 按DefId
var walls = lister.GetByDefId("Wall_Stone");

// 按标签
lister.AddTag(entity, "important");
var tagged = lister.GetByTag("important");
bool hasTag = lister.HasTag(entity, "important");

// 查询
var all = lister.GetAll<Building>();
var filtered = lister.Where(e => e.HitPoints < 50);
var first = lister.FirstOrDefault(e => e.DefId == "Table");
var random = lister.GetRandom(EntityCategory.Item);

// 统计
int count = lister.Count;
int buildingCount = lister.CountByCategory(EntityCategory.Building);
```

### EntityManager（实体管理器）

```csharp
EntityManager manager = site.EntityManager;

// 创建实体
Entity entity = manager.CreateEntity("Building_Table");
Entity entity2 = manager.CreateEntity(buildingDef);

// 生成实体
bool success = manager.SpawnEntity(entity, floor, position, rotation);
Entity spawned = manager.SpawnEntity("Item_Wood", floor, position);

// 移除/销毁
manager.DeSpawnEntity(entity);
manager.DestroyEntity(entity);
manager.DestroyEntityDeferred(entity); // 延迟销毁

// 移动实体
manager.MoveEntity(entity, newPosition);
manager.MoveEntityToFloor(entity, newFloor, newPosition);

// 查询
Entity byId = manager.GetEntityById(123);
var atPos = manager.GetEntitiesAt(globalCoord);
EntityGrid grid = manager.GetEntityGrid(floorIndex);
EntityLister lister = manager.GetEntityLister(floorIndex);

// 放置检查
bool canPlace = manager.CanPlaceAt(def, floor, position, rotation);
string reason = manager.GetPlaceFailReason(def, floor, position, rotation);

// 统计
int total = manager.TotalEntityCount;
var stats = manager.GetStats();
```

## 组件模式

实体通过组件扩展功能：

```csharp
// 自定义组件
public class CompHeatable : EntityComp
{
    private float _temperature = 20f;
    
    public float Temperature => _temperature;
    
    public override void CompTick()
    {
        // 每Tick更新温度
    }
    
    public void Heat(float amount)
    {
        _temperature += amount;
    }
}

// 使用
entity.AddComp<CompHeatable>();
var heatable = entity.GetComp<CompHeatable>();
heatable.Heat(5f);
```

## 实体生命周期

```
CreateEntity() -> SetDef() -> SpawnSetup() -> Tick() -> DeSpawn() -> Destroy()
                                  ↓
                          InitializeComponents()
                          Register to Grid/Lister
                                  ↓
                              OnSpawned()
```

## 层级关系

```
Site
├── EntityManager (全局管理)
│   └── GlobalEntityLister (跨楼层索引)
│
└── Floors[]
    ├── EntityGrid (空间索引)
    └── EntityLister (分类索引)
```

## 使用示例

### 生成建筑

```csharp
// 方法1：使用EntityManager
var building = site.EntityManager.SpawnEntity(
    "Building_Table", 
    site.GroundFloor, 
    new CellCoord(10, 10),
    Rotation.North
);

// 方法2：手动创建
var building = new Building(DefDatabase.GetDef<BuildingDef>("Building_Table"));
site.EntityManager.SpawnEntity(building, floor, position, rotation);
```

### 查找最近的物品

```csharp
var lister = site.EntityManager.GetEntityLister(0);
var items = lister.Items;

Entity nearest = null;
float minDist = float.MaxValue;

foreach (var item in items)
{
    float dist = playerPos.ManhattanDistance(item.Position);
    if (dist < minDist)
    {
        minDist = dist;
        nearest = item;
    }
}
```

### 处理建筑电力

```csharp
var buildings = lister.GetByCategory(EntityCategory.Building);
foreach (Building b in buildings)
{
    if (b.RequiresPower && !b.HasPower)
    {
        // 无电建筑
    }
}
```

## 性能说明

- EntityGrid使用格子列表，O(1)位置查询
- 阻挡实体有单独缓存，通行检查高效
- EntityLister按分类预索引
- 支持延迟销毁避免遍历中修改

---

*TycoonGame MapSystem v1.0*
