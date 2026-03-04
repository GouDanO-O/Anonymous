using System.Collections.Generic;
using System.Linq;
using Core.Game.Map.Define;
using Core.Game.Resource.Data;
using Core.Game.Resource.Define;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 临时地形定义 (后续由Luban生成替代)
    /// </summary>
    public class TerrainDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public byte MoveCost;
        public float Fertility;
        public bool CanBuild;
    }

    /// <summary>
    /// 临时地板定义
    /// </summary>
    public class FloorDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public byte MoveCost;
        public int Beauty;
        public MaterialCost[] BuildCosts;
        public MaterialCost[] DemolishReturns;
    }

    /// <summary>
    /// 结构定义 (墙/门/窗 — 独占一格的建筑)
    /// </summary>
    public class StructureDef
    {
        public int Id;
        public string Name;
        public EStructureType StructureType;
        public Color Color;
        public int MaxHealth;
        public bool IsTransparent;
        public bool BlocksMovement;
        public MaterialCost[] BuildCosts;
        public MaterialCost[] DemolishReturns;
    }

    /// <summary>
    /// 临时物品/家具定义
    /// </summary>
    public class ItemDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public int Width;
        public int Height;
        public bool BlocksMovement;
        public int MaxHealth;
        public MaterialCost[] BuildCosts;
        public MaterialCost[] DemolishReturns;
    }

    /// <summary>
    /// 临时Pawn定义
    /// </summary>
    public class PawnDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public float MoveSpeed;
        public float SpriteSize;
    }

    /// <summary>
    /// 临时配置数据提供器
    /// 硬编码配置数据，模拟Luban生成的结构
    /// </summary>
    public static class TempConfigProvider
    {
        private static Dictionary<int, TerrainDef> _terrainDefs;
        private static Dictionary<int, FloorDef> _floorDefs;
        private static Dictionary<int, StructureDef> _structureDefs;
        private static Dictionary<int, PawnDef> _pawnDefs;
        private static Dictionary<int, ItemDef> _itemDefs;

        private static TerrainDef _defaultTerrain;
        private static FloorDef _defaultFloor;
        private static StructureDef _defaultStructure;
        private static PawnDef _defaultPawn;
        private static ItemDef _defaultItem;

        static TempConfigProvider()
        {
            InitTerrainDefs();
            InitFloorDefs();
            InitStructureDefs();
            InitPawnDefs();
            InitItemDefs();
        }

        private static void InitTerrainDefs()
        {
            _defaultTerrain = new TerrainDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta, MoveCost = 0, Fertility = 0f
            };

            _terrainDefs = new Dictionary<int, TerrainDef>
            {
                {
                    1, new TerrainDef
                    {
                        Id = 1, Name = "Grass",
                        Color = new Color(0.42f, 0.60f, 0.30f),
                        MoveCost = 1, Fertility = 1f, CanBuild = true
                    }
                },
                {
                    2, new TerrainDef
                    {
                        Id = 2, Name = "Dirt",
                        Color = new Color(0.55f, 0.40f, 0.25f),
                        MoveCost = 1, Fertility = 0.5f, CanBuild = true
                    }
                },
                {
                    3, new TerrainDef
                    {
                        Id = 3, Name = "Sand",
                        Color = new Color(0.85f, 0.80f, 0.60f),
                        MoveCost = 1, Fertility = 0.1f, CanBuild = true
                    }
                },
                {
                    4, new TerrainDef
                    {
                        Id = 4, Name = "Rock",
                        Color = new Color(0.50f, 0.50f, 0.50f),
                        MoveCost = 0, Fertility = 0f, CanBuild = false
                    }
                },
                {
                    5, new TerrainDef
                    {
                        Id = 5, Name = "ShallowWater",
                        Color = new Color(0.35f, 0.55f, 0.75f),
                        MoveCost = 3, Fertility = 0f, CanBuild = false
                    }
                },
                {
                    6, new TerrainDef
                    {
                        Id = 6, Name = "DeepWater",
                        Color = new Color(0.15f, 0.30f, 0.55f),
                        MoveCost = 0, Fertility = 0f, CanBuild = false
                    }
                },
                {
                    7, new TerrainDef
                    {
                        Id = 7, Name = "Foundation",
                        Color = new Color(0.60f, 0.55f, 0.45f),
                        MoveCost = 1, Fertility = 0f, CanBuild = true
                    }
                },
            };
        }

        private static void InitFloorDefs()
        {
            _defaultFloor = new FloorDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta, MoveCost = 1, Beauty = 0
            };

            _floorDefs = new Dictionary<int, FloorDef>
            {
                {
                    1, new FloorDef
                    {
                        Id = 1, Name = "WoodFloor",
                        Color = new Color(0.65f, 0.50f, 0.30f),
                        MoveCost = 1, Beauty = 1,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 2) }
                    }
                },
                {
                    2, new FloorDef
                    {
                        Id = 2, Name = "StoneFloor",
                        Color = new Color(0.70f, 0.70f, 0.70f),
                        MoveCost = 1, Beauty = 2,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 2) }
                    }
                },
                {
                    3, new FloorDef
                    {
                        Id = 3, Name = "TileFloor",
                        Color = new Color(0.80f, 0.75f, 0.65f),
                        MoveCost = 1, Beauty = 3,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 3), new MaterialCost(EMaterialType.Wood, 2) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 2) }
                    }
                },
            };
        }

        private static void InitStructureDefs()
        {
            _defaultStructure = new StructureDef
            {
                Id = 0, Name = "Unknown",
                StructureType = EStructureType.Wall,
                Color = Color.magenta,
                MaxHealth = 100, IsTransparent = false, BlocksMovement = true
            };

            _structureDefs = new Dictionary<int, StructureDef>
            {
                {
                    1, new StructureDef
                    {
                        Id = 1, Name = "WoodWall",
                        StructureType = EStructureType.Wall,
                        Color = new Color(0.75f, 0.75f, 0.70f),
                        MaxHealth = 100, IsTransparent = false, BlocksMovement = true,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 5) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 3) }
                    }
                },
                {
                    2, new StructureDef
                    {
                        Id = 2, Name = "StoneWall",
                        StructureType = EStructureType.Wall,
                        Color = new Color(0.60f, 0.58f, 0.55f),
                        MaxHealth = 200, IsTransparent = false, BlocksMovement = true,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 5) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 3) }
                    }
                },
                {
                    3, new StructureDef
                    {
                        Id = 3, Name = "WoodDoor",
                        StructureType = EStructureType.Door,
                        Color = new Color(0.50f, 0.35f, 0.20f),
                        MaxHealth = 80, IsTransparent = false, BlocksMovement = false,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 5) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 3) }
                    }
                },
                {
                    4, new StructureDef
                    {
                        Id = 4, Name = "StoneDoor",
                        StructureType = EStructureType.Door,
                        Color = new Color(0.45f, 0.30f, 0.15f),
                        MaxHealth = 150, IsTransparent = false, BlocksMovement = false,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 5) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 3) }
                    }
                },
                {
                    5, new StructureDef
                    {
                        Id = 5, Name = "WoodWindow",
                        StructureType = EStructureType.Window,
                        Color = new Color(0.60f, 0.80f, 0.90f),
                        MaxHealth = 60, IsTransparent = true, BlocksMovement = true,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 2) }
                    }
                },
                {
                    6, new StructureDef
                    {
                        Id = 6, Name = "StoneWindow",
                        StructureType = EStructureType.Window,
                        Color = new Color(0.55f, 0.75f, 0.85f),
                        MaxHealth = 100, IsTransparent = true, BlocksMovement = true,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 2) }
                    }
                },
                {
                    7, new StructureDef
                    {
                        Id = 7, Name = "WoodPillar",
                        StructureType = EStructureType.Pillar,
                        Color = new Color(0.65f, 0.50f, 0.35f),
                        MaxHealth = 80, IsTransparent = true, BlocksMovement = false,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 2) }
                    }
                },
                {
                    8, new StructureDef
                    {
                        Id = 8, Name = "StonePillar",
                        StructureType = EStructureType.Pillar,
                        Color = new Color(0.55f, 0.55f, 0.55f),
                        MaxHealth = 150, IsTransparent = true, BlocksMovement = false,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 2) }
                    }
                },
                {
                    9, new StructureDef
                    {
                        Id = 9, Name = "WoodStair",
                        StructureType = EStructureType.Stair,
                        Color = new Color(0.70f, 0.55f, 0.30f),
                        MaxHealth = 100, IsTransparent = true, BlocksMovement = false,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 8) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 5) }
                    }
                },
                {
                    10, new StructureDef
                    {
                        Id = 10, Name = "StoneStair",
                        StructureType = EStructureType.Stair,
                        Color = new Color(0.60f, 0.60f, 0.60f),
                        MaxHealth = 180, IsTransparent = true, BlocksMovement = false,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Stone, 8) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Stone, 5) }
                    }
                },
            };
        }

        public static TerrainDef GetTerrainDef(int id)
        {
            return _terrainDefs.GetValueOrDefault(id, _defaultTerrain);
        }

        public static FloorDef GetFloorDef(int id)
        {
            return _floorDefs.GetValueOrDefault(id, _defaultFloor);
        }

        public static StructureDef GetStructureDef(int id)
        {
            return _structureDefs.GetValueOrDefault(id, _defaultStructure);
        }

        // 地基建造/拆除成本 (不走StructureDef, 地基改变地形而非放置结构)
        public static readonly MaterialCost[] FoundationBuildCosts =
            { new MaterialCost(EMaterialType.Stone, 3) };
        public static readonly MaterialCost[] FoundationDemolishReturns =
            { new MaterialCost(EMaterialType.Stone, 2) };

        // 屋顶建造/拆除成本
        public static readonly MaterialCost[] RoofBuildCosts =
            { new MaterialCost(EMaterialType.Wood, 3) };
        public static readonly MaterialCost[] RoofDemolishReturns =
            { new MaterialCost(EMaterialType.Wood, 1) };

        private static void InitPawnDefs()
        {
            _defaultPawn = new PawnDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta, MoveSpeed = 2f, SpriteSize = 0.6f
            };

            _pawnDefs = new Dictionary<int, PawnDef>
            {
                {
                    1, new PawnDef
                    {
                        Id = 1, Name = "Colonist",
                        Color = new Color(0.3f, 0.5f, 0.9f), // 蓝
                        MoveSpeed = 2f, SpriteSize = 0.6f
                    }
                },
                {
                    2, new PawnDef
                    {
                        Id = 2, Name = "Worker",
                        Color = new Color(0.3f, 0.8f, 0.4f), // 绿
                        MoveSpeed = 1.8f, SpriteSize = 0.6f
                    }
                },
                {
                    3, new PawnDef
                    {
                        Id = 3, Name = "Scout",
                        Color = new Color(0.9f, 0.8f, 0.2f), // 黄
                        MoveSpeed = 2.5f, SpriteSize = 0.55f
                    }
                },
            };
        }

        public static PawnDef GetPawnDef(int id)
        {
            return _pawnDefs.GetValueOrDefault(id, _defaultPawn);
        }

        private static void InitItemDefs()
        {
            _defaultItem = new ItemDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta, Width = 1, Height = 1,
                BlocksMovement = false, MaxHealth = 100
            };

            _itemDefs = new Dictionary<int, ItemDef>
            {
                {
                    1, new ItemDef
                    {
                        Id = 1, Name = "Bed",
                        Color = new Color(0.55f, 0.35f, 0.20f),
                        Width = 1, Height = 2, BlocksMovement = true, MaxHealth = 100,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 10) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 6) }
                    }
                },
                {
                    2, new ItemDef
                    {
                        Id = 2, Name = "Table",
                        Color = new Color(0.45f, 0.30f, 0.15f),
                        Width = 2, Height = 2, BlocksMovement = true, MaxHealth = 150,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 15) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 9) }
                    }
                },
                {
                    3, new ItemDef
                    {
                        Id = 3, Name = "Chair",
                        Color = new Color(0.70f, 0.55f, 0.35f),
                        Width = 1, Height = 1, BlocksMovement = false, MaxHealth = 80,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 5) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 3) }
                    }
                },
                {
                    4, new ItemDef
                    {
                        Id = 4, Name = "StorageCrate",
                        Color = new Color(0.55f, 0.55f, 0.55f),
                        Width = 1, Height = 1, BlocksMovement = true, MaxHealth = 200,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 8) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 5) }
                    }
                },
                {
                    5, new ItemDef
                    {
                        Id = 5, Name = "Rug",
                        Color = new Color(0.70f, 0.25f, 0.20f),
                        Width = 2, Height = 2, BlocksMovement = false, MaxHealth = 50,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Wood, 4) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Wood, 2) }
                    }
                },
                {
                    6, new ItemDef
                    {
                        Id = 6, Name = "Lamp",
                        Color = new Color(0.90f, 0.85f, 0.40f),
                        Width = 1, Height = 1, BlocksMovement = false, MaxHealth = 60,
                        BuildCosts = new[] { new MaterialCost(EMaterialType.Steel, 3) },
                        DemolishReturns = new[] { new MaterialCost(EMaterialType.Steel, 2) }
                    }
                },
            };
        }

        public static ItemDef GetItemDef(int id)
        {
            return _itemDefs.GetValueOrDefault(id, _defaultItem);
        }

        // ---- 遍历所有已注册DefId (供TileAtlasManager使用) ----
        public static IEnumerable<int> GetAllTerrainDefIds() => _terrainDefs.Keys;
        public static IEnumerable<int> GetAllFloorDefIds() => _floorDefs.Keys;
        public static IEnumerable<int> GetAllStructureDefIds() => _structureDefs.Keys;
        public static IEnumerable<int> GetAllItemDefIds() => _itemDefs.Keys;
        public static IEnumerable<int> GetAllPawnDefIds() => _pawnDefs.Keys;
    }
}
