using System;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 墙体数据
    /// </summary>
    [Serializable]
    public struct WallData
    {
        /// <summary>
        /// 墙体类型
        /// </summary>
        public EWallType WallType;

        /// <summary>
        /// 耐久度（0-100）
        /// </summary>
        public byte Health;

        /// <summary>
        /// 门状态
        /// </summary>
        public EDoorState DoorState;

        /// <summary>
        /// 窗户状态
        /// </summary>
        public EWindowState WindowState;

        /// <summary>
        /// 是否有墙
        /// </summary>
        public bool HasWall => WallType != EWallType.None;

        /// <summary>
        /// 是否有门
        /// </summary>
        public bool HasDoor => DoorState != EDoorState.None;

        /// <summary>
        /// 是否有窗
        /// </summary>
        public bool HasWindow => WindowState != EWindowState.None;

        /// <summary>
        /// 门是否打开
        /// </summary>
        public bool IsDoorOpen => DoorState == EDoorState.Open;

        /// <summary>
        /// 是否可通行（无墙/门打开）
        /// </summary>
        public bool IsPassable => !HasWall || IsDoorOpen;

        /// <summary>
        /// 创建空墙
        /// </summary>
        public static WallData Empty => new WallData
        {
            WallType = EWallType.None,
            Health = 0,
            DoorState = EDoorState.None,
            WindowState = EWindowState.None
        };

        /// <summary>
        /// 创建指定类型的墙
        /// </summary>
        public static WallData Create(EWallType wallType, byte health = 100)
        {
            return new WallData
            {
                WallType = wallType,
                Health = health,
                DoorState = EDoorState.None,
                WindowState = EWindowState.None
            };
        }

        /// <summary>
        /// 创建带门的墙
        /// </summary>
        public static WallData CreateWithDoor(EWallType wallType, EDoorState doorState = EDoorState.Closed,
            byte health = 100)
        {
            return new WallData
            {
                WallType = wallType,
                Health = health,
                DoorState = doorState,
                WindowState = EWindowState.None
            };
        }

        /// <summary>
        /// 创建带窗的墙
        /// </summary>
        public static WallData CreateWithWindow(EWallType wallType, EWindowState windowState = EWindowState.Closed,
            byte health = 100)
        {
            return new WallData
            {
                WallType = wallType,
                Health = health,
                DoorState = EDoorState.None,
                WindowState = windowState
            };
        }
    }
}
