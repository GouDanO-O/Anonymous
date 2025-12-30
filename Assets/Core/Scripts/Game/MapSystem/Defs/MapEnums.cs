/*******************************************************************************
 * 文件名:    MapEnums.cs
 * 描述:      地图系统核心枚举定义
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 包含枚举:
 *   - BearingCapacity: 承重等级（地形/地板能承载什么建筑）
 *   - Passability: 可通行性（实体是否阻挡移动）
 *   - TileLayer: Tile层级（六层Tile系统）
 *   - AltitudeLayer: 渲染高度层级
 *   - FloorType: 楼层类型
 ******************************************************************************/

using System;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 承重等级
    /// 决定地形/地板能够支撑什么类型的建筑
    /// </summary>
    public enum BearingCapacity : byte
    {
        /// <summary>
        /// 无承重（水、岩浆等，不能放置任何东西）
        /// </summary>
        None = 0,

        /// <summary>
        /// 轻型承重（软土、沙地等，只能放置轻型物品）
        /// </summary>
        Light = 1,

        /// <summary>
        /// 中型承重（普通地面、木地板等，可放置大部分建筑）
        /// </summary>
        Medium = 2,

        /// <summary>
        /// 重型承重（石地板、钢地板等，可放置所有建筑）
        /// </summary>
        Heavy = 3
    }

    /// <summary>
    /// 可通行性
    /// 决定实体是否阻挡移动
    /// </summary>
    public enum Passability : byte
    {
        /// <summary>
        /// 可通行（不阻挡移动，如地毯、血迹）
        /// </summary>
        Passable = 0,

        /// <summary>
        /// 可站立（可以站在上面，但占据空间，如家具）
        /// </summary>
        Standable = 1,

        /// <summary>
        /// 仅穿过（可以穿过但不能停留，如门框）
        /// </summary>
        PassThroughOnly = 2,

        /// <summary>
        /// 不可通行（完全阻挡，如墙壁、大型机器）
        /// </summary>
        Impassable = 3
    }

    /// <summary>
    /// Tile层级（六层Tile系统）
    /// 从下到上排列
    /// </summary>
    public enum TileLayer : byte
    {
        /// <summary>
        /// 第0层：地形层（土、石、水、岩浆等自然地形）
        /// </summary>
        Terrain = 0,

        /// <summary>
        /// 第1层：地基层（建筑地基，影响承重）
        /// </summary>
        Foundation = 1,

        /// <summary>
        /// 第2层：地板层（木地板、石砖、地毯等）
        /// </summary>
        Floor = 2,

        /// <summary>
        /// 第3层：覆盖层（血迹、污渍、积雪、脚印等）
        /// </summary>
        Cover = 3,

        /// <summary>
        /// 第4层：墙壁层（墙、门、窗等垂直结构）
        /// </summary>
        Wall = 4,

        /// <summary>
        /// 第5层：屋顶/天花板层
        /// </summary>
        Roof = 5
    }

    /// <summary>
    /// 实体渲染高度层级
    /// 用于控制实体的渲染顺序
    /// </summary>
    public enum AltitudeLayer : byte
    {
        /// <summary>
        /// 地形层
        /// </summary>
        Terrain = 0,

        /// <summary>
        /// 地板层
        /// </summary>
        Floor = 1,

        /// <summary>
        /// 地板装饰层（地毯边缘等）
        /// </summary>
        FloorDecor = 2,

        /// <summary>
        /// 覆盖物层（血迹、污渍）
        /// </summary>
        FloorCoverings = 3,

        /// <summary>
        /// 管线层（电线、管道）
        /// </summary>
        Conduits = 4,

        /// <summary>
        /// 地面物品层
        /// </summary>
        Item = 5,

        /// <summary>
        /// 重要物品层（高亮显示）
        /// </summary>
        ItemImportant = 6,

        /// <summary>
        /// 低矮建筑层（花盆、路灯底座）
        /// </summary>
        BuildingLow = 7,

        /// <summary>
        /// 建筑层（主体建筑）
        /// </summary>
        Building = 8,

        /// <summary>
        /// 建筑顶部装饰层
        /// </summary>
        BuildingTop = 9,

        /// <summary>
        /// 植物层
        /// </summary>
        Plant = 10,

        /// <summary>
        /// 生物层（Pawn）
        /// </summary>
        Pawn = 11,

        /// <summary>
        /// 生物状态层（血条、状态图标）
        /// </summary>
        PawnState = 12,

        /// <summary>
        /// 投射物层
        /// </summary>
        Projectile = 13,

        /// <summary>
        /// 特效层
        /// </summary>
        Effect = 14,

        /// <summary>
        /// 天气层
        /// </summary>
        Weather = 15,

        /// <summary>
        /// UI层（世界空间UI）
        /// </summary>
        WorldUI = 16
    }

    /// <summary>
    /// 楼层类型
    /// </summary>
    public enum FloorType : byte
    {
        /// <summary>
        /// 地下层
        /// </summary>
        Underground = 0,

        /// <summary>
        /// 地面层
        /// </summary>
        Ground = 1,

        /// <summary>
        /// 地上层
        /// </summary>
        Aboveground = 2
    }

    /// <summary>
    /// 实体分类
    /// </summary>
    public enum EntityCategory : byte
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None = 0,

        /// <summary>
        /// 生物（人、动物、怪物）
        /// </summary>
        Pawn = 1,

        /// <summary>
        /// 建筑（墙、门、机器、家具）
        /// </summary>
        Building = 2,

        /// <summary>
        /// 物品（资源、装备、消耗品）
        /// </summary>
        Item = 3,

        /// <summary>
        /// 植物
        /// </summary>
        Plant = 4,

        /// <summary>
        /// 污物（血迹、呕吐物、垃圾）
        /// </summary>
        Filth = 5,

        /// <summary>
        /// 投射物（子弹、箭矢）
        /// </summary>
        Projectile = 6,

        /// <summary>
        /// 视觉特效
        /// </summary>
        Mote = 7,

        /// <summary>
        /// 蓝图（待建造）
        /// </summary>
        Blueprint = 8,

        /// <summary>
        /// 建造框架（建造中）
        /// </summary>
        Frame = 9
    }

    /// <summary>
    /// 楼层连接器类型
    /// </summary>
    public enum FloorConnectorType : byte
    {
        /// <summary>
        /// 非连接器
        /// </summary>
        None = 0,

        /// <summary>
        /// 楼梯（占据多格，双向）
        /// </summary>
        Stair = 1,

        /// <summary>
        /// 梯子（单格，双向，较慢）
        /// </summary>
        Ladder = 2,

        /// <summary>
        /// 电梯（可跨多层，需要电力）
        /// </summary>
        Elevator = 3,

        /// <summary>
        /// 洞/天井（单向下落，或需要绳索）
        /// </summary>
        Hole = 4,

        /// <summary>
        /// 斜坡（占据多格，适合推车）
        /// </summary>
        Ramp = 5,

        /// <summary>
        /// 传送门（特殊科技）
        /// </summary>
        Teleporter = 6
    }

    /// <summary>
    /// 墙壁类型
    /// </summary>
    public enum WallType : byte
    {
        /// <summary>
        /// 无墙
        /// </summary>
        None = 0,

        /// <summary>
        /// 普通墙壁（完全阻挡）
        /// </summary>
        Solid = 1,

        /// <summary>
        /// 门（可开关）
        /// </summary>
        Door = 2,

        /// <summary>
        /// 窗户（透光，部分阻挡）
        /// </summary>
        Window = 3,

        /// <summary>
        /// 栅栏（透视，部分阻挡）
        /// </summary>
        Fence = 4,

        /// <summary>
        /// 半墙（低矮墙壁）
        /// </summary>
        HalfWall = 5
    }

    /// <summary>
    /// 屋顶类型
    /// </summary>
    public enum RoofType : byte
    {
        /// <summary>
        /// 无屋顶（露天）
        /// </summary>
        None = 0,

        /// <summary>
        /// 人造屋顶（可拆除）
        /// </summary>
        Constructed = 1,

        /// <summary>
        /// 薄岩石顶（山洞边缘）
        /// </summary>
        RockThin = 2,

        /// <summary>
        /// 厚岩石顶（深山，不可拆）
        /// </summary>
        RockThick = 3,

        /// <summary>
        /// 透明屋顶（玻璃，透光）
        /// </summary>
        Transparent = 4
    }

    /// <summary>
    /// 地形类型
    /// </summary>
    public enum TerrainType : byte
    {
        /// <summary>
        /// 普通地面
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 水域
        /// </summary>
        Water = 1,

        /// <summary>
        /// 深水
        /// </summary>
        DeepWater = 2,

        /// <summary>
        /// 岩浆
        /// </summary>
        Lava = 3,

        /// <summary>
        /// 沼泽
        /// </summary>
        Marsh = 4,

        /// <summary>
        /// 冰面
        /// </summary>
        Ice = 5,

        /// <summary>
        /// 岩石（可开采）
        /// </summary>
        Rock = 6,

        /// <summary>
        /// 矿脉
        /// </summary>
        Ore = 7
    }

    /// <summary>
    /// 品质等级
    /// </summary>
    public enum QualityLevel : byte
    {
        /// <summary>
        /// 无品质（不适用）
        /// </summary>
        None = 0,

        /// <summary>
        /// 劣质
        /// </summary>
        Awful = 1,

        /// <summary>
        /// 粗糙
        /// </summary>
        Poor = 2,

        /// <summary>
        /// 普通
        /// </summary>
        Normal = 3,

        /// <summary>
        /// 良好
        /// </summary>
        Good = 4,

        /// <summary>
        /// 优秀
        /// </summary>
        Excellent = 5,

        /// <summary>
        /// 精品
        /// </summary>
        Masterwork = 6,

        /// <summary>
        /// 传奇
        /// </summary>
        Legendary = 7
    }

    #region 枚举扩展方法

    /// <summary>
    /// BearingCapacity 扩展方法
    /// </summary>
    public static class BearingCapacityExtensions
    {
        /// <summary>
        /// 检查是否能承载指定等级
        /// </summary>
        public static bool CanSupport(this BearingCapacity capacity, BearingCapacity required)
        {
            return (int)capacity >= (int)required;
        }

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public static string ToDisplayName(this BearingCapacity capacity)
        {
            return capacity switch
            {
                BearingCapacity.None => "无",
                BearingCapacity.Light => "轻型",
                BearingCapacity.Medium => "中型",
                BearingCapacity.Heavy => "重型",
                _ => "未知"
            };
        }
    }

    /// <summary>
    /// Passability 扩展方法
    /// </summary>
    public static class PassabilityExtensions
    {
        /// <summary>
        /// 是否可以通行
        /// </summary>
        public static bool CanPass(this Passability passability)
        {
            return passability != Passability.Impassable;
        }

        /// <summary>
        /// 是否可以站立
        /// </summary>
        public static bool CanStand(this Passability passability)
        {
            return passability == Passability.Passable || passability == Passability.Standable;
        }

        /// <summary>
        /// 是否完全阻挡
        /// </summary>
        public static bool IsBlocking(this Passability passability)
        {
            return passability == Passability.Impassable;
        }
    }

    /// <summary>
    /// TileLayer 扩展方法
    /// </summary>
    public static class TileLayerExtensions
    {
        /// <summary>
        /// 层级数量
        /// </summary>
        public const int LayerCount = 6;

        /// <summary>
        /// 获取层级索引
        /// </summary>
        public static int ToIndex(this TileLayer layer)
        {
            return (int)layer;
        }

        /// <summary>
        /// 是否是结构层（影响通行和建造）
        /// </summary>
        public static bool IsStructural(this TileLayer layer)
        {
            return layer == TileLayer.Wall || layer == TileLayer.Roof;
        }

        /// <summary>
        /// 是否是地面层（影响移动和放置）
        /// </summary>
        public static bool IsGround(this TileLayer layer)
        {
            return layer == TileLayer.Terrain || 
                   layer == TileLayer.Foundation || 
                   layer == TileLayer.Floor;
        }
    }

    /// <summary>
    /// AltitudeLayer 扩展方法
    /// </summary>
    public static class AltitudeLayerExtensions
    {
        /// <summary>
        /// 高度层级间距（用于渲染排序）
        /// </summary>
        private const float LayerSpacing = 0.1f;

        /// <summary>
        /// 获取渲染用的Y坐标偏移
        /// </summary>
        public static float ToYOffset(this AltitudeLayer layer)
        {
            return (int)layer * LayerSpacing;
        }

        /// <summary>
        /// 获取渲染排序值
        /// </summary>
        public static int ToSortingOrder(this AltitudeLayer layer)
        {
            return (int)layer * 10;
        }
    }

    /// <summary>
    /// QualityLevel 扩展方法
    /// </summary>
    public static class QualityLevelExtensions
    {
        /// <summary>
        /// 获取品质乘数（影响物品属性）
        /// </summary>
        public static float ToMultiplier(this QualityLevel quality)
        {
            return quality switch
            {
                QualityLevel.None => 1.0f,
                QualityLevel.Awful => 0.5f,
                QualityLevel.Poor => 0.75f,
                QualityLevel.Normal => 1.0f,
                QualityLevel.Good => 1.25f,
                QualityLevel.Excellent => 1.5f,
                QualityLevel.Masterwork => 2.0f,
                QualityLevel.Legendary => 3.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 获取品质颜色（用于UI显示）
        /// </summary>
        public static UnityEngine.Color ToColor(this QualityLevel quality)
        {
            return quality switch
            {
                QualityLevel.None => UnityEngine.Color.white,
                QualityLevel.Awful => new UnityEngine.Color(0.5f, 0.5f, 0.5f),    // 灰色
                QualityLevel.Poor => new UnityEngine.Color(0.7f, 0.7f, 0.7f),     // 浅灰
                QualityLevel.Normal => UnityEngine.Color.white,                    // 白色
                QualityLevel.Good => new UnityEngine.Color(0.2f, 0.8f, 0.2f),     // 绿色
                QualityLevel.Excellent => new UnityEngine.Color(0.2f, 0.6f, 1.0f), // 蓝色
                QualityLevel.Masterwork => new UnityEngine.Color(0.8f, 0.4f, 1.0f), // 紫色
                QualityLevel.Legendary => new UnityEngine.Color(1.0f, 0.8f, 0.0f), // 金色
                _ => UnityEngine.Color.white
            };
        }

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public static string ToDisplayName(this QualityLevel quality)
        {
            return quality switch
            {
                QualityLevel.None => "",
                QualityLevel.Awful => "劣质",
                QualityLevel.Poor => "粗糙",
                QualityLevel.Normal => "普通",
                QualityLevel.Good => "良好",
                QualityLevel.Excellent => "优秀",
                QualityLevel.Masterwork => "精品",
                QualityLevel.Legendary => "传奇",
                _ => "未知"
            };
        }
    }

    #endregion
}
