namespace Core.Game.Map.Define
{
    /// <summary>
    /// 地图系统常量定义
    /// </summary>
    public static class MapDefine
    {
        #region Tile尺寸（像素）

        /// <summary>
        /// Tile宽度（像素）- 等轴测菱形的水平宽度
        /// </summary>
        public const float TileWidth = 64f;

        /// <summary>
        /// Tile高度（像素）- 等轴测菱形的垂直高度
        /// </summary>
        public const float TileHeight = 32f;

        /// <summary>
        /// Tile半宽
        /// </summary>
        public const float TileHalfWidth = TileWidth / 2f;

        /// <summary>
        /// Tile半高
        /// </summary>
        public const float TileHalfHeight = TileHeight / 2f;

        /// <summary>
        /// 墙体高度（像素）
        /// </summary>
        public const float WallHeight = 48f;

        /// <summary>
        /// 楼层高度（像素）
        /// </summary>
        public const float FloorHeight = 64f;

        #endregion

        #region Chunk配置

        /// <summary>
        /// Chunk尺寸（格子数，Chunk为正方形）
        /// </summary>
        public const int ChunkSize = 32;

        /// <summary>
        /// 每个Chunk的格子总数
        /// </summary>
        public const int CellsPerChunk = ChunkSize * ChunkSize;

        #endregion

        #region 地图配置

        /// <summary>
        /// 默认地图宽度（格子数）
        /// </summary>
        public const int DefaultMapWidth = 300;

        /// <summary>
        /// 默认地图高度（格子数）
        /// </summary>
        public const int DefaultMapHeight = 300;

        /// <summary>
        /// 默认楼层数
        /// </summary>
        public const int DefaultFloorCount = 2;

        /// <summary>
        /// 最大楼层数
        /// </summary>
        public const int MaxFloorCount = 8;

        #endregion

        #region 像素到世界单位转换

        /// <summary>
        /// 像素转世界单位比例（100像素 = 1单位）
        /// </summary>
        public const float PixelsPerUnit = 100f;

        /// <summary>
        /// Tile世界宽度
        /// </summary>
        public const float TileWorldWidth = TileWidth / PixelsPerUnit;

        /// <summary>
        /// Tile世界高度
        /// </summary>
        public const float TileWorldHeight = TileHeight / PixelsPerUnit;

        /// <summary>
        /// 墙体世界高度
        /// </summary>
        public const float WallWorldHeight = WallHeight / PixelsPerUnit;

        /// <summary>
        /// 楼层世界高度
        /// </summary>
        public const float FloorWorldHeight = FloorHeight / PixelsPerUnit;

        #endregion

        #region 渲染排序

        /// <summary>
        /// 每个Tile的深度增量
        /// </summary>
        public const float DepthPerTile = 0.001f;

        /// <summary>
        /// 每层楼的深度增量
        /// </summary>
        public const float DepthPerFloor = 10f;

        /// <summary>
        /// 地面层深度偏移
        /// </summary>
        public const float DepthOffsetGround = 0f;

        /// <summary>
        /// 地板层深度偏移
        /// </summary>
        public const float DepthOffsetFloor = 0.1f;

        /// <summary>
        /// 墙体层深度偏移
        /// </summary>
        public const float DepthOffsetWall = 0.2f;

        /// <summary>
        /// 物体层深度偏移
        /// </summary>
        public const float DepthOffsetObject = 0.3f;

        /// <summary>
        /// Pawn层深度偏移
        /// </summary>
        public const float DepthOffsetPawn = 0.4f;

        /// <summary>
        /// 屋顶层深度偏移
        /// </summary>
        public const float DepthOffsetRoof = 0.5f;

        #endregion
    }
}
