using System;

namespace Core.Game.Map.Define
{
    /// <summary>
    /// 格子标记（位掩码）
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

        /// <summary>
        /// 有水源
        /// </summary>
        HasWater = 1 << 6,

        /// <summary>
        /// 被占用（有Pawn或物体）
        /// </summary>
        Occupied = 1 << 7,

        /// <summary>
        /// 禁止进入
        /// </summary>
        Forbidden = 1 << 8,

        /// <summary>
        /// 家居区域
        /// </summary>
        HomeArea = 1 << 9
    }
}
