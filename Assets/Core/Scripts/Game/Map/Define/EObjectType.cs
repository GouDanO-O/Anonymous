using System;

namespace Core.Game.Map.Define
{
    /// <summary>
    /// 物体类型（家具、装饰等）
    /// </summary>
    public enum EObjectType : ushort
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        #region 家具 1-100

        /// <summary>
        /// 椅子
        /// </summary>
        Chair = 1,

        /// <summary>
        /// 桌子
        /// </summary>
        Table = 2,

        /// <summary>
        /// 床
        /// </summary>
        Bed = 3,

        /// <summary>
        /// 柜子
        /// </summary>
        Cabinet = 4,

        /// <summary>
        /// 沙发
        /// </summary>
        Sofa = 5,

        #endregion

        #region 容器 101-200

        /// <summary>
        /// 箱子
        /// </summary>
        Crate = 101,

        /// <summary>
        /// 冰箱
        /// </summary>
        Fridge = 102,

        #endregion

        #region 装饰 201-300

        /// <summary>
        /// 植物盆栽
        /// </summary>
        PlantPot = 201,

        #endregion

        #region 功能性 301-400

        /// <summary>
        /// 炉灶
        /// </summary>
        Stove = 301,

        /// <summary>
        /// 工作台
        /// </summary>
        Workbench = 302,

        #endregion
    }

    /// <summary>
    /// Cell 标记位
    /// </summary>
    [Flags]
    public enum ECellFlags : ushort
    {
        /// <summary>
        /// 无标记
        /// </summary>
        None = 0,

        /// <summary>
        /// 可通行
        /// </summary>
        Walkable = 1 << 0,

        /// <summary>
        /// 室内
        /// </summary>
        Indoor = 1 << 1,

        /// <summary>
        /// 有屋顶
        /// </summary>
        HasRoof = 1 << 2,

        /// <summary>
        /// 已探索
        /// </summary>
        Explored = 1 << 3,

        /// <summary>
        /// 可建造
        /// </summary>
        Buildable = 1 << 4,

        /// <summary>
        /// 有电力
        /// </summary>
        HasPower = 1 << 5,
    }
}
