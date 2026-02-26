namespace Core.Game.Pawn.Define
{
    /// <summary>
    /// 8方向朝向枚举
    /// 按顺时针排列，与IsometricUtility.EightDirections对应
    /// </summary>
    public enum EFacingDirection : byte
    {
        /// <summary>
        /// 北 (0, +1)
        /// </summary>
        North = 0,

        /// <summary>
        /// 东北 (+1, +1)
        /// </summary>
        NorthEast = 1,

        /// <summary>
        /// 东 (+1, 0)
        /// </summary>
        East = 2,

        /// <summary>
        /// 东南 (+1, -1)
        /// </summary>
        SouthEast = 3,

        /// <summary>
        /// 南 (0, -1)
        /// </summary>
        South = 4,

        /// <summary>
        /// 西南 (-1, -1)
        /// </summary>
        SouthWest = 5,

        /// <summary>
        /// 西 (-1, 0)
        /// </summary>
        West = 6,

        /// <summary>
        /// 西北 (-1, +1)
        /// </summary>
        NorthWest = 7
    }
}
