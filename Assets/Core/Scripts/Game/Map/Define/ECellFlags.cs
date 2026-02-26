using System;

namespace Core.Game.Map.Define
{
    /// <summary>
    /// 单元格位标志
    /// </summary>
    [Flags]
    public enum ECellFlags : ushort
    {
        None       = 0,
        Walkable   = 1 << 0,
        Indoor     = 1 << 1,
        HasRoof    = 1 << 2,
        Explored   = 1 << 3,
        Buildable  = 1 << 4,
        HasPower   = 1 << 5,
        HasWater   = 1 << 6,
        Occupied   = 1 << 7,
        Forbidden  = 1 << 8,
        HomeArea   = 1 << 9,
    }
}
