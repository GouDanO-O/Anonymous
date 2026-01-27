namespace Core.Game.Map.Define
{
    /// <summary>
    /// 地图系统常量定义
    /// </summary>
    public static class MapDefine
    {
        #region Tile 尺寸

        /// <summary>
        /// Tile 宽度（像素）- 等轴测菱形的水平宽度
        /// </summary>
        public const float TileWidth = 128f;

        /// <summary>
        /// Tile 高度（像素）- 等轴测菱形的垂直高度
        /// </summary>
        public const float TileHeight = 64f;

        /// <summary>
        /// Tile 半宽
        /// </summary>
        public const float TileHalfWidth = TileWidth / 2f;

        /// <summary>
        /// Tile 半高
        /// </summary>
        public const float TileHalfHeight = TileHeight / 2f;

        /// <summary>
        /// 墙体高度（像素）
        /// </summary>
        public const float WallHeight = 96f;

        /// <summary>
        /// 楼层高度（像素）
        /// </summary>
        public const float FloorHeight = 128f;

        #endregion

        #region Chunk 配置

        /// <summary>
        /// Chunk 宽度（格子数）
        /// </summary>
        public const int ChunkWidth = 8;

        /// <summary>
        /// Chunk 高度（格子数）
        /// </summary>
        public const int ChunkHeight = 8;

        /// <summary>
        /// 每个 Chunk 的格子总数
        /// </summary>
        public const int CellsPerChunk = ChunkWidth * ChunkHeight;

        #endregion

        #region Map 配置

        /// <summary>
        /// 默认地图宽度（格子数）
        /// </summary>
        public const int DefaultMapWidth = 32;

        /// <summary>
        /// 默认地图高度（格子数）
        /// </summary>
        public const int DefaultMapHeight = 32;

        /// <summary>
        /// 默认楼层数
        /// </summary>
        public const int DefaultFloorCount = 2;

        /// <summary>
        /// 最大楼层数
        /// </summary>
        public const int MaxFloorCount = 8;

        #endregion

        #region 渲染排序

        /// <summary>
        /// 每个 Tile 的深度增量
        /// </summary>
        public const float DepthPerTile = 0.01f;

        /// <summary>
        /// 每层楼的深度增量
        /// </summary>
        public const float DepthPerFloor = 10f;

        /// <summary>
        /// 地面层排序基础值
        /// </summary>
        public const int SortingOrderGround = 0;

        /// <summary>
        /// 地板层排序基础值
        /// </summary>
        public const int SortingOrderFloor = 1000;

        /// <summary>
        /// 墙体层排序基础值
        /// </summary>
        public const int SortingOrderWall = 2000;

        /// <summary>
        /// 物体层排序基础值
        /// </summary>
        public const int SortingOrderObject = 3000;

        /// <summary>
        /// 角色层排序基础值
        /// </summary>
        public const int SortingOrderPawn = 4000;

        #endregion

        #region 像素到世界单位转换

        /// <summary>
        /// 像素转世界单位比例（100像素 = 1单位）
        /// </summary>
        public const float PixelsPerUnit = 100f;

        /// <summary>
        /// Tile 世界宽度
        /// </summary>
        public const float TileWorldWidth = TileWidth / PixelsPerUnit;

        /// <summary>
        /// Tile 世界高度
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
    }
}
