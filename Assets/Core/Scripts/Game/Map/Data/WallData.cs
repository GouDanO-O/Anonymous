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
        #region 基础属性

        /// <summary>
        /// 墙体类型
        /// </summary>
        public EWallType WallType;

        /// <summary>
        /// 门状态
        /// </summary>
        public EDoorState DoorState;

        /// <summary>
        /// 窗户状态
        /// </summary>
        public EWindowState WindowState;

        /// <summary>
        /// 耐久度（0-100）
        /// </summary>
        public byte Health;

        #endregion

        #region 属性访问器

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
        /// 门是否锁定
        /// </summary>
        public bool IsDoorLocked => DoorState == EDoorState.Locked;

        /// <summary>
        /// 窗户是否打开
        /// </summary>
        public bool IsWindowOpen => WindowState == EWindowState.Open;

        /// <summary>
        /// 窗户是否破碎
        /// </summary>
        public bool IsWindowBroken => WindowState == EWindowState.Broken;

        /// <summary>
        /// 是否可通行（无墙 或 门打开 或 窗户破碎）
        /// </summary>
        public bool IsPassable
        {
            get
            {
                if (!HasWall) return true;
                if (HasDoor && IsDoorOpen) return true;
                if (HasWindow && IsWindowBroken) return true;
                return false;
            }
        }

        /// <summary>
        /// 是否透明（玻璃墙 或 有窗户且未关闭）
        /// </summary>
        public bool IsTransparent
        {
            get
            {
                if (!HasWall) return true;
                if (WallType == EWallType.Glass) return true;
                if (HasWindow && WindowState != EWindowState.Closed) return true;
                return false;
            }
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建空墙数据
        /// </summary>
        public static WallData Empty => new WallData
        {
            WallType = EWallType.None,
            DoorState = EDoorState.None,
            WindowState = EWindowState.None,
            Health = 0
        };

        /// <summary>
        /// 创建实心墙
        /// </summary>
        public static WallData CreateSolid(EWallType wallType, byte health = 100)
        {
            return new WallData
            {
                WallType = wallType,
                DoorState = EDoorState.None,
                WindowState = EWindowState.None,
                Health = health
            };
        }

        /// <summary>
        /// 创建带门的墙
        /// </summary>
        public static WallData CreateWithDoor(EWallType wallType, EDoorState doorState = EDoorState.Closed, byte health = 100)
        {
            return new WallData
            {
                WallType = wallType,
                DoorState = doorState,
                WindowState = EWindowState.None,
                Health = health
            };
        }

        /// <summary>
        /// 创建带窗的墙
        /// </summary>
        public static WallData CreateWithWindow(EWallType wallType, EWindowState windowState = EWindowState.Closed, byte health = 100)
        {
            return new WallData
            {
                WallType = wallType,
                DoorState = EDoorState.None,
                WindowState = windowState,
                Health = health
            };
        }

        #endregion

        #region 方法

        /// <summary>
        /// 设置门状态
        /// </summary>
        public void SetDoorState(EDoorState state)
        {
            DoorState = state;
        }

        /// <summary>
        /// 设置窗户状态
        /// </summary>
        public void SetWindowState(EWindowState state)
        {
            WindowState = state;
        }

        /// <summary>
        /// 造成伤害
        /// </summary>
        public void TakeDamage(byte damage)
        {
            Health = (byte)Math.Max(0, Health - damage);
        }

        /// <summary>
        /// 修复
        /// </summary>
        public void Repair(byte amount)
        {
            Health = (byte)Math.Min(100, Health + amount);
        }

        #endregion
    }
}
