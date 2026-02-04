namespace Core.Game.Map.Define
{
    /// <summary>
    /// 地板类型（人造地板）
    /// </summary>
    public enum EFloorType : byte
    {
        /// <summary>
        /// 无地板
        /// </summary>
        None = 0,

        /// <summary>
        /// 木地板
        /// </summary>
        Wood = 1,

        /// <summary>
        /// 瓷砖地板
        /// </summary>
        Tile = 2,

        /// <summary>
        /// 混凝土地板
        /// </summary>
        Concrete = 3,

        /// <summary>
        /// 地毯
        /// </summary>
        Carpet = 4,

        /// <summary>
        /// 金属地板
        /// </summary>
        Metal = 5,

        /// <summary>
        /// 石板地板
        /// </summary>
        StoneSlab = 6
    }
}
