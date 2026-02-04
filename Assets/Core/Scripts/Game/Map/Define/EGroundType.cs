namespace Core.Game.Map.Define
{
    /// <summary>
    /// 地面类型（自然地形）
    /// </summary>
    public enum EGroundType : byte
    {
        /// <summary>
        /// 无地面
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
        /// 石地
        /// </summary>
        Stone = 4,

        /// <summary>
        /// 水面
        /// </summary>
        Water = 5,

        /// <summary>
        /// 雪地
        /// </summary>
        Snow = 6,

        /// <summary>
        /// 沼泽
        /// </summary>
        Swamp = 7
    }
}
