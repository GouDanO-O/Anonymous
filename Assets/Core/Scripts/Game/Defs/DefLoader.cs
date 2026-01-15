using System;
using Core.Game.Defs.Map;
using Core.Game.Defs.Pawn;
using Core.Game.Defs.Thing;
using UnityEngine;

namespace Core.Game.Defs
{
    // <summary>
    /// 定义加载器 - 从Luban配置加载所有Def到DefDatabase
    /// 这是连接Luban配置系统和Def系统的桥梁
    /// </summary>
    public static class DefLoader
    {
        private static bool _initialized = false;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool Initialized => _initialized;

        /// <summary>
        /// 初始化所有定义
        /// 应在游戏启动时调用，Luban配置加载完成后
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                Debug.LogWarning("[DefLoader] 已经初始化过了");
                return;
            }

            Debug.Log("[DefLoader] 开始加载定义...");

            try
            {
                // 注册所有Def类型
                RegisterDefTypes();

                // 加载各类型的定义
                LoadTerrainDefs();
                LoadBuildingDefs();
                LoadItemDefs();
                LoadPawnKindDefs();

                // 标记各DefDatabase为已初始化
                SetAllInitialized();

                // 解析引用关系
                DefDataBaseManager.ResolveAllReferences();

                // 绑定DefOf引用
                BindAllDefOfReferences();

                // 验证配置
                DefDataBaseManager.ValidateAll();

                _initialized = true;
                Debug.Log("[DefLoader] 定义加载完成");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DefLoader] 加载定义时发生错误: {e}");
                throw;
            }
        }

        /// <summary>
        /// 注册所有Def类型
        /// </summary>
        private static void RegisterDefTypes()
        {
            DefDataBaseManager.Register<TerrainDef>();
            DefDataBaseManager.Register<BuildingDef>();
            DefDataBaseManager.Register<ItemDef>();
            DefDataBaseManager.Register<PawnKindDef>();
            // 后续可以添加更多类型
        }

        /// <summary>
        /// 标记所有DefDatabase为已初始化
        /// </summary>
        private static void SetAllInitialized()
        {
            DefDataBase<TerrainDef>.SetInitialized();
            DefDataBase<BuildingDef>.SetInitialized();
            DefDataBase<ItemDef>.SetInitialized();
            DefDataBase<PawnKindDef>.SetInitialized();
        }

        /// <summary>
        /// 绑定所有DefOf静态引用
        /// </summary>
        private static void BindAllDefOfReferences()
        {
            TerrainDefOf.BindReferences();
            BuildingDefOf.BindReferences();
            ItemDefOf.BindReferences();
            PawnKindDefOf.BindReferences();
        }

        #region 加载各类型定义

        /// <summary>
        /// 加载地形定义
        /// TODO: 从Luban配置加载，目前使用硬编码示例
        /// </summary>
        private static void LoadTerrainDefs()
        {
            AddSampleTerrainDefs();
        }

        /// <summary>
        /// 加载建筑定义
        /// </summary>
        private static void LoadBuildingDefs()
        {
            // TODO: 从Luban加载
            AddSampleBuildingDefs();
        }

        /// <summary>
        /// 加载物品定义
        /// </summary>
        private static void LoadItemDefs()
        {
            // TODO: 从Luban加载
            AddSampleItemDefs();
        }

        /// <summary>
        /// 加载角色种类定义
        /// </summary>
        private static void LoadPawnKindDefs()
        {
            // TODO: 从Luban加载
            AddSamplePawnKindDefs();
        }

        #endregion

        #region 示例数据（开发测试用）

        private static void AddSampleTerrainDefs()
        {
            DefDataBase<TerrainDef>.Add(new TerrainDef
            {
                defName = "Terrain_Grass",
                label = "草地",
                description = "普通的草地",
                passable = true,
                pathSpeed = 1f,
                natural = true,
                texturePath = "Textures/Terrain/Grass",
                tileIndex = 0,
                fertility = 1f,
                supportPlant = true,
            });

            DefDataBase<TerrainDef>.Add(new TerrainDef
            {
                defName = "Terrain_Soil",
                label = "泥土",
                description = "肥沃的泥土",
                passable = true,
                pathSpeed = 1f,
                natural = true,
                texturePath = "Textures/Terrain/Soil",
                tileIndex = 1,
                fertility = 1.4f,
                supportPlant = true,
            });

            DefDataBase<TerrainDef>.Add(new TerrainDef
            {
                defName = "Terrain_Stone",
                label = "石头地面",
                description = "坚硬的石头地面",
                passable = true,
                pathSpeed = 1f,
                natural = true,
                texturePath = "Textures/Terrain/Stone",
                tileIndex = 2,
                fertility = 0f,
                supportPlant = false,
            });

            DefDataBase<TerrainDef>.Add(new TerrainDef
            {
                defName = "Terrain_Water",
                label = "水",
                description = "浅水区域",
                passable = false,
                pathSpeed = 999f,
                natural = true,
                texturePath = "Textures/Terrain/Water",
                tileIndex = 3,
                isWater = true,
                waterDepth = 0.5f,
            });

            DefDataBase<TerrainDef>.Add(new TerrainDef
            {
                defName = "Terrain_WoodFloor",
                label = "木地板",
                description = "简单的木地板",
                passable = true,
                pathSpeed = 0.8f,
                natural = false,
                texturePath = "Textures/Terrain/WoodFloor",
                tileIndex = 10,
                beauty = 1,
                cleanlinessModifier = 0.1f,
                costDefNames = new[] { "Item_Wood" },
                costAmounts = new[] { 3 },
                workToBuild = 50f,
            });
        }

        private static void AddSampleBuildingDefs()
        {
            DefDataBase<BuildingDef>.Add(new BuildingDef
            {
                defName = "Building_Wall_Wood",
                label = "木墙",
                description = "简单的木质墙壁",
                buildingType = EBuildingType.Wall,
                texturePath = "Textures/Buildings/WallWood",
                maxHitPoints = 150,
                flammable = true,
                flammability = 1f,
                costDefNames = new[] { "Item_Wood" },
                costAmounts = new[] { 5 },
                workToBuild = 100f,
            });

            DefDataBase<BuildingDef>.Add(new BuildingDef
            {
                defName = "Building_Wall_Stone",
                label = "石墙",
                description = "坚固的石质墙壁",
                buildingType = EBuildingType.Wall,
                texturePath = "Textures/Buildings/WallStone",
                maxHitPoints = 400,
                flammable = false,
                costDefNames = new[] { "Item_Stone" },
                costAmounts = new[] { 5 },
                workToBuild = 150f,
            });

            DefDataBase<BuildingDef>.Add(new BuildingDef
            {
                defName = "Building_Door_Wood",
                label = "木门",
                description = "简单的木门",
                buildingType = EBuildingType.Door,
                texturePath = "Textures/Buildings/DoorWood",
                maxHitPoints = 100,
                flammable = true,
                blockPass = true, // 关闭时阻挡
                doorOpenPassable = true,
                costDefNames = new[] { "Item_Wood" },
                costAmounts = new[] { 25 },
                workToBuild = 80f,
            });
        }

        private static void AddSampleItemDefs()
        {
            DefDataBase<ItemDef>.Add(new ItemDef
            {
                defName = "Item_Wood",
                label = "木材",
                description = "基础建筑材料",
                itemType = EItemType.Resource,
                texturePath = "Textures/Items/Wood",
                stackLimit = 75,
                mass = 0.5f,
                baseMarketValue = 1.2f,
                materialTags = new[] { "Wood", "Burnable" },
            });

            DefDataBase<ItemDef>.Add(new ItemDef
            {
                defName = "Item_Stone",
                label = "石块",
                description = "坚硬的石块",
                itemType = EItemType.Resource,
                texturePath = "Textures/Items/Stone",
                stackLimit = 75,
                mass = 1f,
                baseMarketValue = 1f,
                materialTags = new[] { "Stone" },
            });

            DefDataBase<ItemDef>.Add(new ItemDef
            {
                defName = "Item_Steel",
                label = "钢铁",
                description = "精炼的钢铁",
                itemType = EItemType.Resource,
                texturePath = "Textures/Items/Steel",
                stackLimit = 75,
                mass = 0.8f,
                baseMarketValue = 2f,
                materialTags = new[] { "Metal" },
            });

            DefDataBase<ItemDef>.Add(new ItemDef
            {
                defName = "Item_MealSimple",
                label = "简单餐食",
                description = "简单烹饪的食物",
                itemType = EItemType.MealSimple,
                texturePath = "Textures/Items/MealSimple",
                stackLimit = 10,
                mass = 0.3f,
                baseMarketValue = 15f,
                nutrition = 0.9f,
                daysToRotStart = 4f,
                foodTaste = 0,
            });
        }

        private static void AddSamplePawnKindDefs()
        {
            DefDataBase<PawnKindDef>.Add(new PawnKindDef
            {
                defName = "PawnKind_Colonist",
                label = "殖民者",
                description = "玩家控制的殖民者",
                raceType = ERaceType.Human,
                playerControlled = true,
                hostile = false,
                bodyTexturePath = "Textures/Pawns/Human/Body",
                headTexturePath = "Textures/Pawns/Human/Head",
                baseMaxHealth = 100,
                baseMoveSpeed = 4.5f,
                hasMood = true,
                needsFood = true,
                needsRest = true,
                combatPower = 50f,
            });

            DefDataBase<PawnKindDef>.Add(new PawnKindDef
            {
                defName = "PawnKind_Raider",
                label = "劫掠者",
                description = "敌对的劫掠者",
                raceType = ERaceType.Human,
                playerControlled = false,
                hostile = true,
                bodyTexturePath = "Textures/Pawns/Human/Body",
                headTexturePath = "Textures/Pawns/Human/Head",
                baseMaxHealth = 100,
                baseMoveSpeed = 4.5f,
                hasMood = false,
                needsFood = false,
                needsRest = false,
                combatPower = 50f,
            });
        }

        #endregion

        /// <summary>
        /// 重新加载所有定义
        /// 开发调试用
        /// </summary>
        public static void Reload()
        {
            Debug.Log("[DefLoader] 重新加载定义...");
            DefDataBaseManager.ClearAll();
            _initialized = false;
            Initialize();
        }
    }
}