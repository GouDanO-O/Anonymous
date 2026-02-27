using System;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 墙体数据 (值类型，内存高效)
    /// </summary>
    [Serializable]
    public struct WallData
    {
        /// <summary>
        /// Luban WallDef ID, 0 = 无墙
        /// </summary>
        public int WallDefId;

        public EDoorState DoorState;
        public EWindowState WindowState;
        public byte Health;

        public bool HasWall => WallDefId != MapConst.InvalidDefId;
        public bool HasDoor => DoorState != EDoorState.None;
        public bool HasWindow => WindowState != EWindowState.None;
        public bool IsDoorOpen => DoorState == EDoorState.Open;

        /// <summary>
        /// 是否可通过 (无墙/门开着/窗户破损)
        /// </summary>
        public bool IsPassable
        {
            get
            {
                if (!HasWall) return true;
                if (HasDoor && DoorState != EDoorState.Locked) return true;
                if (HasWindow && WindowState == EWindowState.Broken) return true;
                return false;
            }
        }

        public static WallData Empty => new WallData
        {
            WallDefId = MapConst.InvalidDefId,
            DoorState = EDoorState.None,
            WindowState = EWindowState.None,
            Health = 0
        };

        public static WallData Create(int wallDefId, byte health = 100)
        {
            return new WallData
            {
                WallDefId = wallDefId,
                DoorState = EDoorState.None,
                WindowState = EWindowState.None,
                Health = health
            };
        }
    }
}
