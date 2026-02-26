namespace Core.Game.Map.Define
{
    /// <summary>
    /// 地图系统常量定义
    /// </summary>
    public static class MapConst
    {
        // 瓦片尺寸 (俯视角，像素)
        public const int TilePixelSize = 32;
        public const float PixelsPerUnit = 32f;
        public const float TileWorldSize = 1f; // 1 tile = 1 Unity unit

        // Chunk配置
        public const int ChunkSize = 16; // 16x16 tiles per chunk
        public const int CellsPerChunk = ChunkSize * ChunkSize; // 256

        // 地图默认值
        public const int DefaultMapWidth = 250;
        public const int DefaultMapHeight = 250;
        public const int DefaultFloorCount = 3; // 地面层0 + 2层楼
        public const int MaxFloorCount = 8;

        // 无效ID
        public const int InvalidDefId = 0;
        public const int InvalidRoomId = -1;
    }
}
