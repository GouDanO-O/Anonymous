namespace Core.Game.Map.Define
{
    /// <summary>
    /// 地面类型（自然地形）
    /// </summary>
    public enum EGroundType : byte
    {
        /// <summary>
        /// 无/空
        /// </summary>
        None = 0,

        /// <summary>
        /// 草地
        /// </summary>
        Grass = 1,

        /// <summary>
        /// 泥土
        /// </summary>
        Dirt = 2,

        /// <summary>
        /// 沙地
        /// </summary>
        Sand = 3,

        /// <summary>
        /// 石头地面
        /// </summary>
        Stone = 4,

        /// <summary>
        /// 水
        /// </summary>
        Water = 5,
    }

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
        /// 瓷砖
        /// </summary>
        Tile = 2,

        /// <summary>
        /// 混凝土
        /// </summary>
        Concrete = 3,

        /// <summary>
        /// 地毯
        /// </summary>
        Carpet = 4,
    }
}
