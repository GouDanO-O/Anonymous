/*******************************************************************************
 * 文件名:    DefaultDefs.cs
 * 描述:      默认Def定义，提供系统预设的基础Def实例
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   DefaultDefs 提供了一些系统必需的默认Def，如：
 *   - 空Def（用于表示无/空状态）
 *   - 默认地形、地板、墙壁等
 *   
 *   这些Def在游戏初始化时自动注册。
 ******************************************************************************/

using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 默认Def定义
    /// </summary>
    public static class DefaultDefs
    {
        #region Def ID 常量

        /// <summary>
        /// 空/无的Def ID前缀
        /// </summary>
        public const string EmptyPrefix = "Empty_";

        // 地形
        public const string TerrainDirt = "Terrain_Dirt";
        public const string TerrainSoil = "Terrain_Soil";
        public const string TerrainSand = "Terrain_Sand";
        public const string TerrainGrass = "Terrain_Grass";
        public const string TerrainRock = "Terrain_Rock";
        public const string TerrainWater = "Terrain_Water";
        public const string TerrainDeepWater = "Terrain_DeepWater";
        public const string TerrainLava = "Terrain_Lava";

        // 地基
        public const string FoundationNone = "Foundation_None";
        public const string FoundationWood = "Foundation_Wood";
        public const string FoundationStone = "Foundation_Stone";
        public const string FoundationSteel = "Foundation_Steel";

        // 地板
        public const string FloorNone = "Floor_None";
        public const string FloorWood = "Floor_Wood";
        public const string FloorStoneTile = "Floor_StoneTile";
        public const string FloorConcrete = "Floor_Concrete";
        public const string FloorSteel = "Floor_Steel";
        public const string FloorCarpet = "Floor_Carpet";

        // 墙壁
        public const string WallNone = "Wall_None";
        public const string WallWood = "Wall_Wood";
        public const string WallStone = "Wall_Stone";
        public const string WallSteel = "Wall_Steel";
        public const string WallGlass = "Wall_Glass";
        public const string DoorWood = "Door_Wood";
        public const string DoorSteel = "Door_Steel";

        // 屋顶
        public const string RoofNone = "Roof_None";
        public const string RoofWood = "Roof_Wood";
        public const string RoofStone = "Roof_Stone";
        public const string RoofSteel = "Roof_Steel";
        public const string RoofGlass = "Roof_Glass";
        public const string RoofRockThin = "Roof_RockThin";
        public const string RoofRockThick = "Roof_RockThick";

        // 覆盖物
        public const string CoverBlood = "Cover_Blood";
        public const string CoverDirt = "Cover_Dirt";
        public const string CoverVomit = "Cover_Vomit";
        public const string CoverSnow = "Cover_Snow";

        // 楼层连接器
        public const string StairWood = "Stair_Wood";
        public const string StairStone = "Stair_Stone";
        public const string StairSteel = "Stair_Steel";
        public const string LadderWood = "Ladder_Wood";
        public const string ElevatorSmall = "Elevator_Small";
        public const string ElevatorLarge = "Elevator_Large";

        #endregion

        #region 注册默认Def

        /// <summary>
        /// 注册所有默认Def
        /// </summary>
        public static void RegisterAll()
        {
            Debug.Log("[DefaultDefs] Registering default defs...");

            RegisterTerrainDefs();
            RegisterFoundationDefs();
            RegisterFloorDefs();
            RegisterWallDefs();
            RegisterRoofDefs();
            RegisterCoverDefs();
            RegisterConnectorDefs();

            Debug.Log($"[DefaultDefs] Registered {DefDatabase.Count} default defs");
        }

        /// <summary>
        /// 注册地形Def
        /// </summary>
        private static void RegisterTerrainDefs()
        {
            // 土地
            DefDatabase.Register(new TerrainDef
            {
                _defId = TerrainDirt,
                _defName = "泥土",
                _description = "普通的泥土地面",
                _texturePath = "Terrain/Dirt"
            });

            // 肥沃土壤
            DefDatabase.Register(TerrainDef.Create(TerrainSoil, "肥沃土壤", 
                BearingCapacity.Medium, Passability.Passable, 1));

            // 沙地
            DefDatabase.Register(new TerrainDef
            {
                _defId = TerrainSand,
                _defName = "沙地",
                _description = "松软的沙地，移动较慢"
            });

            // 草地
            DefDatabase.Register(TerrainDef.Create(TerrainGrass, "草地",
                BearingCapacity.Medium, Passability.Passable, 1));

            // 岩石
            DefDatabase.Register(new TerrainDef
            {
                _defId = TerrainRock,
                _defName = "岩石",
                _description = "坚硬的岩石，可以开采"
            });

            // 水
            DefDatabase.Register(new TerrainDef
            {
                _defId = TerrainWater,
                _defName = "浅水",
                _description = "浅水区域，移动缓慢"
            });

            // 深水
            DefDatabase.Register(new TerrainDef
            {
                _defId = TerrainDeepWater,
                _defName = "深水",
                _description = "深水区域，无法通行"
            });

            // 岩浆
            DefDatabase.Register(new TerrainDef
            {
                _defId = TerrainLava,
                _defName = "岩浆",
                _description = "危险的岩浆，接触即死"
            });
        }

        /// <summary>
        /// 注册地基Def
        /// </summary>
        private static void RegisterFoundationDefs()
        {
            // 无地基
            DefDatabase.Register(FoundationDef.Create(FoundationNone, "无", BearingCapacity.None));

            // 木地基
            DefDatabase.Register(FoundationDef.Create(FoundationWood, "木地基", BearingCapacity.Light));

            // 石地基
            DefDatabase.Register(FoundationDef.Create(FoundationStone, "石地基", BearingCapacity.Medium));

            // 钢地基
            DefDatabase.Register(FoundationDef.Create(FoundationSteel, "钢地基", BearingCapacity.Heavy));
        }

        /// <summary>
        /// 注册地板Def
        /// </summary>
        private static void RegisterFloorDefs()
        {
            // 无地板
            DefDatabase.Register(FloorDef.Create(FloorNone, "无", BearingCapacity.None));

            // 木地板
            DefDatabase.Register(FloorDef.Create(FloorWood, "木地板", BearingCapacity.Light, 1));

            // 石砖地板
            DefDatabase.Register(FloorDef.Create(FloorStoneTile, "石砖地板", BearingCapacity.Medium, 2));

            // 混凝土地板
            DefDatabase.Register(FloorDef.Create(FloorConcrete, "混凝土地板", BearingCapacity.Heavy, 0));

            // 钢地板
            DefDatabase.Register(FloorDef.Create(FloorSteel, "钢地板", BearingCapacity.Heavy, 1));

            // 地毯
            DefDatabase.Register(FloorDef.Create(FloorCarpet, "地毯", BearingCapacity.None, 3));
        }

        /// <summary>
        /// 注册墙壁Def
        /// </summary>
        private static void RegisterWallDefs()
        {
            // 无墙
            DefDatabase.Register(WallDef.Create(WallNone, "无", WallType.None, 0));

            // 木墙
            DefDatabase.Register(WallDef.Create(WallWood, "木墙", WallType.Solid, 150));

            // 石墙
            DefDatabase.Register(WallDef.Create(WallStone, "石墙", WallType.Solid, 300));

            // 钢墙
            DefDatabase.Register(WallDef.Create(WallSteel, "钢墙", WallType.Solid, 500));

            // 玻璃墙
            DefDatabase.Register(new WallDef
            {
                _defId = WallGlass,
                _defName = "玻璃墙",
                _description = "透明的玻璃墙"
            });

            // 木门
            DefDatabase.Register(WallDef.Create(DoorWood, "木门", WallType.Door, 100));

            // 钢门
            DefDatabase.Register(WallDef.Create(DoorSteel, "钢门", WallType.Door, 300));
        }

        /// <summary>
        /// 注册屋顶Def
        /// </summary>
        private static void RegisterRoofDefs()
        {
            // 无屋顶
            DefDatabase.Register(RoofDef.Create(RoofNone, "无", RoofType.None));

            // 木屋顶
            DefDatabase.Register(RoofDef.Create(RoofWood, "木屋顶", RoofType.Constructed));

            // 石屋顶
            DefDatabase.Register(RoofDef.Create(RoofStone, "石屋顶", RoofType.Constructed));

            // 钢屋顶
            DefDatabase.Register(RoofDef.Create(RoofSteel, "钢屋顶", RoofType.Constructed));

            // 玻璃屋顶
            DefDatabase.Register(RoofDef.Create(RoofGlass, "玻璃屋顶", RoofType.Transparent));

            // 薄岩顶
            DefDatabase.Register(RoofDef.Create(RoofRockThin, "薄岩顶", RoofType.RockThin));

            // 厚岩顶
            DefDatabase.Register(RoofDef.Create(RoofRockThick, "厚岩顶", RoofType.RockThick));
        }

        /// <summary>
        /// 注册覆盖物Def
        /// </summary>
        private static void RegisterCoverDefs()
        {
            // 血迹
            DefDatabase.Register(CoverDef.Create(CoverBlood, "血迹", true, -2));

            // 灰尘
            DefDatabase.Register(CoverDef.Create(CoverDirt, "灰尘", true, -1));

            // 呕吐物
            DefDatabase.Register(CoverDef.Create(CoverVomit, "呕吐物", true, -3));

            // 积雪
            DefDatabase.Register(CoverDef.Create(CoverSnow, "积雪", false, 0));
        }

        /// <summary>
        /// 注册楼层连接器Def
        /// </summary>
        private static void RegisterConnectorDefs()
        {
            // 木楼梯
            DefDatabase.Register(EntityDef.CreateStairs(StairWood, "木楼梯", new IntVec2(2, 3)));

            // 石楼梯
            DefDatabase.Register(EntityDef.CreateStairs(StairStone, "石楼梯", new IntVec2(2, 3)));

            // 钢楼梯
            DefDatabase.Register(EntityDef.CreateStairs(StairSteel, "钢楼梯", new IntVec2(2, 3)));

            // 木梯子
            DefDatabase.Register(new EntityDef
            {
                _defId = LadderWood,
                _defName = "木梯子",
                _category = EntityCategory.Building,
                _size = new IntVec2(1, 1),
                _isFloorConnector = true,
                _connectorType = FloorConnectorType.Ladder,
                _traverseCost = 20
            });

            // 小型电梯
            DefDatabase.Register(EntityDef.CreateElevator(ElevatorSmall, "小型电梯", new IntVec2(2, 2), 5));

            // 大型电梯
            DefDatabase.Register(EntityDef.CreateElevator(ElevatorLarge, "大型电梯", new IntVec2(3, 3), 10));
        }

        #endregion

        #region 快捷访问

        // 缓存的默认Def引用
        private static TerrainDef _defaultTerrain;
        private static FloorDef _emptyFloor;
        private static WallDef _emptyWall;
        private static RoofDef _emptyRoof;

        /// <summary>
        /// 默认地形（泥土）
        /// </summary>
        public static TerrainDef DefaultTerrain
        {
            get
            {
                if (_defaultTerrain == null)
                    _defaultTerrain = DefDatabase.GetTerrainDef(TerrainDirt);
                return _defaultTerrain;
            }
        }

        /// <summary>
        /// 空地板
        /// </summary>
        public static FloorDef EmptyFloor
        {
            get
            {
                if (_emptyFloor == null)
                    _emptyFloor = DefDatabase.GetFloorDef(FloorNone);
                return _emptyFloor;
            }
        }

        /// <summary>
        /// 空墙壁
        /// </summary>
        public static WallDef EmptyWall
        {
            get
            {
                if (_emptyWall == null)
                    _emptyWall = DefDatabase.GetWallDef(WallNone);
                return _emptyWall;
            }
        }

        /// <summary>
        /// 空屋顶
        /// </summary>
        public static RoofDef EmptyRoof
        {
            get
            {
                if (_emptyRoof == null)
                    _emptyRoof = DefDatabase.GetRoofDef(RoofNone);
                return _emptyRoof;
            }
        }

        /// <summary>
        /// 清除缓存（当DefDatabase重新加载时调用）
        /// </summary>
        public static void ClearCache()
        {
            _defaultTerrain = null;
            _emptyFloor = null;
            _emptyWall = null;
            _emptyRoof = null;
        }

        #endregion
    }
}
