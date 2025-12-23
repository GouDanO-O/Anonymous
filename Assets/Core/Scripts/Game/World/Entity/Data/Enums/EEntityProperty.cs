using System;

namespace Core.Game.World.Entity.Data.Enums
{
    /// <summary>
    /// 实体属性
    /// </summary>
    [Flags]
    public enum EEntityProperty : ushort
    {
        /// <summary>
        /// 无特殊属性
        /// </summary>
        None = 0,

        /// <summary>
        /// 阻碍移动
        /// </summary>
        BlocksMovement = 1 << 0,

        /// <summary>
        /// 阻碍视线
        /// </summary>
        BlocksVision = 1 << 1,

        /// <summary>
        /// 可互动
        /// </summary>
        Interactable = 1 << 2,

        /// <summary>
        /// 可被破坏
        /// </summary>
        Destructible = 1 << 3,

        /// <summary>
        /// 可被放置(既可拆卸又可再次被安装)
        /// </summary>
        Placeable = 1 << 4,

        /// <summary>
        /// 可承重
        /// </summary>
        IsBearingStructure = 1 << 5,

        /// <summary>
        /// 无法再放置其他物体
        /// </summary>
        CantPlaceOtherEntity = 1 << 6,
    }
}