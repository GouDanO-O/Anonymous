using System;

namespace Core.Game.Defs.Thing
{
    public enum EBuildingType
    {
        None = 0,
        Wall,           // 墙壁
        Door,           // 门
        Floor,          // 地板（特殊建筑）
        Furniture,      // 家具
        Production,     // 生产建筑（工作台）
        Storage,        // 存储建筑
        Power,          // 电力建筑
        Security,       // 安防建筑（炮塔等）
        Misc,           // 其他
    }
    
    /// <summary>
    /// 建筑定义 - 定义建筑物的属性
    /// 继承自ThingDef，添加建筑特有的属性
    /// </summary>
    [Serializable]
    public class BuildingDef : ThingDef
    {
        public override EDefType DefType => EDefType.Building;

        public BuildingDef()
        {
            // 建筑的默认值
            category = EThingCategory.Building;
            blockPass = true;
            destroyable = true;
        }

        #region 建筑特有属性

        /// <summary>
        /// 建筑类型
        /// </summary>
        public EBuildingType buildingType = EBuildingType.None;

        /// <summary>
        /// 是否是墙（会触发屋顶生成）
        /// </summary>
        public bool isWall = false;

        /// <summary>
        /// 是否是门（可开关）
        /// </summary>
        public bool isDoor = false;

        /// <summary>
        /// 门开启后是否允许通行
        /// </summary>
        public bool doorOpenPassable = true;

        /// <summary>
        /// 是否支持屋顶
        /// </summary>
        public bool holdsRoof = false;

        /// <summary>
        /// 是否可以在其下方放置物品
        /// </summary>
        public bool canPlaceUnder = false;

        #endregion

        #region 建造属性

        /// <summary>
        /// 建造所需材料（defName列表）
        /// </summary>
        public string[] costDefNames;

        /// <summary>
        /// 建造所需材料数量（与costDefNames对应）
        /// </summary>
        public int[] costAmounts;

        /// <summary>
        /// 建造工作量
        /// </summary>
        public float workToBuild = 100f;

        /// <summary>
        /// 建造所需技能类型
        /// </summary>
        public string constructionSkill = "Construction";

        /// <summary>
        /// 建造所需最低技能等级
        /// </summary>
        public int minSkillLevel = 0;

        /// <summary>
        /// 是否可被玩家建造
        /// </summary>
        public bool playerBuildable = true;

        /// <summary>
        /// 建造时的蓝图贴图路径
        /// </summary>
        public string blueprintTexturePath;

        #endregion

        #region 功能属性

        /// <summary>
        /// 提供的舒适度
        /// </summary>
        public float comfort = 0f;

        /// <summary>
        /// 美观度
        /// </summary>
        public int beauty = 0;

        /// <summary>
        /// 是否提供休息功能
        /// </summary>
        public bool isRest = false;

        /// <summary>
        /// 休息效率
        /// </summary>
        public float restEfficiency = 1f;

        /// <summary>
        /// 是否是工作站
        /// </summary>
        public bool isWorkStation = false;

        /// <summary>
        /// 支持的配方defName列表（如果是工作站）
        /// </summary>
        public string[] recipeDefNames;

        /// <summary>
        /// 存储容量（如果是存储建筑）
        /// </summary>
        public int storageCapacity = 0;

        /// <summary>
        /// 可存储的物品类型过滤
        /// </summary>
        public string[] storageFilter;

        #endregion

        #region 电力属性

        /// <summary>
        /// 电力消耗（正数消耗，负数产生）
        /// </summary>
        public float powerConsumption = 0f;

        /// <summary>
        /// 是否需要电力才能工作
        /// </summary>
        public bool requiresPower = false;

        #endregion

        #region 多楼层属性

        /// <summary>
        /// 是否支持多层（如楼梯）
        /// </summary>
        public bool isMultiFloor = false;

        /// <summary>
        /// 连接到的楼层偏移（-1下层，+1上层）
        /// </summary>
        public int floorConnection = 0;

        #endregion

        public override void PostLoad()
        {
            base.PostLoad();

            // 根据建筑类型自动设置一些属性
            if (buildingType == EBuildingType.Wall)
            {
                isWall = true;
                holdsRoof = true;
                blockLight = true;
            }
            else if (buildingType == EBuildingType.Door)
            {
                isDoor = true;
                holdsRoof = true;
            }

            // 蓝图贴图默认使用主贴图
            if (string.IsNullOrEmpty(blueprintTexturePath))
            {
                blueprintTexturePath = texturePath;
            }
        }

        public override string[] ConfigErrors()
        {
            var baseErrors = base.ConfigErrors();
            var errors = new System.Collections.Generic.List<string>();

            if (baseErrors != null)
            {
                errors.AddRange(baseErrors);
            }

            if (buildingType == EBuildingType.None)
            {
                errors.Add("buildingType未设置");
            }

            if (costDefNames != null && costAmounts != null && costDefNames.Length != costAmounts.Length)
            {
                errors.Add("costDefNames和costAmounts长度不匹配");
            }

            if (workToBuild <= 0 && playerBuildable)
            {
                errors.Add("playerBuildable为true但workToBuild <= 0");
            }

            return errors.Count > 0 ? errors.ToArray() : null;
        }
    }
    
    
    /// <summary>
    /// 预定义的常用建筑
    /// </summary>
    public static class BuildingDefOf
    {
        public static BuildingDef Wall_Wood;
        public static BuildingDef Wall_Stone;
        public static BuildingDef Door_Wood;
        public static BuildingDef Door_Auto;

        public static void BindReferences()
        {
            Wall_Wood = DefDataBase<BuildingDef>.GetNamed("Building_Wall_Wood");
            Wall_Stone = DefDataBase<BuildingDef>.GetNamed("Building_Wall_Stone");
            Door_Wood = DefDataBase<BuildingDef>.GetNamed("Building_Door_Wood");
            Door_Auto = DefDataBase<BuildingDef>.GetNamed("Building_Door_Auto");
        }
    }
}