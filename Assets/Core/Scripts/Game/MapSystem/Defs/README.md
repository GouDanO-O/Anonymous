# TycoonGame 地图系统 - Def模块

## 概述

Def（Definition）系统是游戏数据驱动设计的核心。所有游戏内容（地形、建筑、物品等）都通过Def定义，实现内容与代码的分离。

## 文件结构

```
MapSystem/
├── Defs/
│   ├── MapEnums.cs       # 核心枚举定义
│   ├── DefBase.cs        # Def基类
│   ├── TileDefs.cs       # Tile相关Def（地形/地板/墙壁/屋顶）
│   ├── EntityDef.cs      # 实体Def（建筑/物品）
│   ├── DefDatabase.cs    # Def数据库管理器
│   └── DefaultDefs.cs    # 默认Def定义
```

## 核心概念

### Def类层次

```
DefBase (所有Def的基类)
├── TileDef (Tile定义基类)
│   ├── TerrainDef    (地形：土/石/水/岩浆)
│   ├── FoundationDef (地基：影响承重)
│   ├── FloorDef      (地板：木地板/石砖)
│   ├── CoverDef      (覆盖物：血迹/污渍)
│   ├── WallDef       (墙壁：墙/门/窗)
│   └── RoofDef       (屋顶：人造/岩石)
│
└── EntityDef (实体定义基类)
    ├── BuildingDef   (建筑：家具/机器)
    └── ItemDef       (物品：资源/装备)
```

### 核心枚举

| 枚举 | 说明 | 值 |
|------|------|-----|
| `BearingCapacity` | 承重等级 | None, Light, Medium, Heavy |
| `Passability` | 可通行性 | Passable, Standable, PassThroughOnly, Impassable |
| `TileLayer` | Tile层级 | Terrain, Foundation, Floor, Cover, Wall, Roof |
| `EntityCategory` | 实体分类 | Pawn, Building, Item, Plant, Filth... |
| `FloorConnectorType` | 楼层连接器类型 | Stair, Ladder, Elevator, Hole, Ramp |

## 快速开始

### 获取Def

```csharp
using TycoonGame.MapSystem;

// 通过ID获取Def
TerrainDef terrain = DefDatabase.GetDef<TerrainDef>("Terrain_Grass");

// 使用快捷方法
TerrainDef terrain2 = DefDatabase.GetTerrainDef("Terrain_Grass");

// 获取所有某类型的Def
foreach (var floorDef in DefDatabase.AllFloorDefs)
{
    Debug.Log(floorDef.DefName);
}

// 使用泛型数据库
var allBuildings = DefDatabase<BuildingDef>.AllDefs;
int buildingCount = DefDatabase<BuildingDef>.Count;
```

### 检查Def属性

```csharp
TerrainDef terrain = DefDatabase.GetTerrainDef("Terrain_Water");

// 检查承重
if (terrain.BearingCapacity.CanSupport(BearingCapacity.Medium))
{
    // 可以建造中型建筑
}

// 检查通行性
if (terrain.Passability.CanPass())
{
    // 可以通行
}

// 检查是否可种植
if (terrain.CanPlant)
{
    // 可以种植
}
```

### 实体尺寸和占据格子

```csharp
EntityDef buildingDef = DefDatabase.GetEntityDef("Building_Table");

// 获取旋转后的尺寸
IntVec2 size = buildingDef.GetRotatedSize(Rotation.East);

// 获取占据的所有格子
CellCoord origin = new CellCoord(5, 5);
foreach (var cell in buildingDef.GetOccupiedCells(origin, Rotation.North))
{
    // 处理每个占据的格子
}

// 检查是否占据某格子
bool occupies = buildingDef.OccupiesCell(origin, Rotation.North, new CellCoord(6, 5));
```

### 楼层连接器

```csharp
EntityDef stairDef = DefDatabase.GetEntityDef("Stair_Wood");

if (stairDef.IsFloorConnector)
{
    // 是楼层连接器
    FloorConnectorType type = stairDef.ConnectorType;  // Stair
    int floors = stairDef.ConnectsFloors;               // 连接楼层数
    int cost = stairDef.TraverseCost;                   // 通过代价
    bool needsPower = stairDef.ConnectorNeedsPower;     // 是否需要电力
}
```

## 与Luban集成

### 从Luban数据创建Def

```csharp
// 假设 Luban 生成了 TbTerrain 表和 Terrain 数据类
public void LoadTerrainDefs(TbTerrain table)
{
    DefDatabase.LoadFromLuban(table.DataList, data =>
    {
        var def = new TerrainDef();
        def._defId = data.Id;
        def._defName = data.Name;
        // ... 设置其他字段
        return def;
    });
}

// 或使用自动映射
DefDatabase.CreateAllFromLuban<TerrainDef, Terrain>(table.DataList);
```

### 初始化流程

```csharp
public void InitializeGame()
{
    // 1. 清空数据库
    DefDatabase.Clear();
    
    // 2. 注册默认Def
    DefaultDefs.RegisterAll();
    
    // 3. 从Luban加载Def
    LoadLubanDefs();
    
    // 4. 初始化所有Def（解析引用）
    DefDatabase.InitializeAll();
}
```

## 六层Tile系统

```
Layer 5: Roof       屋顶/天花板
Layer 4: Wall       墙壁/门/窗
Layer 3: Cover      覆盖物（血迹/积雪）
Layer 2: Floor      地板
Layer 1: Foundation 地基
Layer 0: Terrain    地形
```

### 承重等级链

```
地形(BearingCapacity) → 地基(提升) → 地板(提升) → 可建造的建筑
     None/Light/Medium/Heavy

示例:
  沙地(Light) + 石地基(+Medium) + 钢地板(+Heavy) = Heavy承重
  水域(None) = 不可建造
```

## 默认Def ID

### 地形
- `Terrain_Dirt` - 泥土
- `Terrain_Grass` - 草地
- `Terrain_Rock` - 岩石
- `Terrain_Water` - 浅水
- `Terrain_Lava` - 岩浆

### 地板
- `Floor_None` - 无
- `Floor_Wood` - 木地板
- `Floor_StoneTile` - 石砖
- `Floor_Steel` - 钢地板

### 墙壁
- `Wall_None` - 无
- `Wall_Wood` - 木墙
- `Wall_Stone` - 石墙
- `Door_Wood` - 木门

### 楼层连接器
- `Stair_Wood` - 木楼梯 (2x3)
- `Ladder_Wood` - 木梯子 (1x1)
- `Elevator_Small` - 小型电梯 (2x2)

## 性能说明

- DefDatabase使用字典存储，O(1)查询复杂度
- 支持短哈希快速查找（用于网络同步）
- 泛型DefDatabase<T>支持缓存列表
- 引用解析在初始化时一次性完成

## 扩展指南

### 添加新的Def类型

```csharp
[Serializable]
public class MyCustomDef : DefBase
{
    [SerializeField]
    internal int _customValue;
    
    public int CustomValue => _customValue;
    
    protected override void Validate()
    {
        base.Validate();
        if (_customValue < 0)
            Debug.LogWarning($"[{DefId}] CustomValue should be positive");
    }
}

// 注册
DefDatabase.Register(new MyCustomDef { _defId = "MyDef_1", _customValue = 100 });
```

### 添加Def间引用

```csharp
public class RecipeDef : DefBase
{
    [SerializeField]
    internal string _outputItemId;
    
    // 运行时解析的引用
    [NonSerialized]
    private ItemDef _outputItem;
    
    public ItemDef OutputItem => _outputItem;
    
    protected override void ResolveReferences()
    {
        base.ResolveReferences();
        _outputItem = DefDatabase.GetDef<ItemDef>(_outputItemId);
    }
}
```

## 下一步

Def系统完成后，接下来将实现：
1. Site/Floor基础结构（地图容器）
2. TileGrids六层实现（静态世界数据）
3. Tile渲染系统

---

*TycoonGame MapSystem v1.0*
