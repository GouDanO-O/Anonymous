using System;
using Newtonsoft.Json;

namespace Core.Game.World.Map.Data
{
    public static class MapConstants
    {
        #region Chunk 相关常量
        
        /// <summary>
        /// Chunk 的边长（以 Tile 为单位）
        /// 16x16 = 256 个 Tile 每 Chunk
        /// </summary>
        public const int CHUNK_SIZE = 16;
        
        /// <summary>
        /// 每个 Chunk 包含的 Tile 总数
        /// </summary>
        public const int TILES_PER_CHUNK = CHUNK_SIZE * CHUNK_SIZE;
        
        #endregion
        
        #region Tile 相关常量
        
        /// <summary>
        /// 单个 Tile 在 Unity 世界坐标中的尺寸
        /// </summary>
        public const float TILE_SIZE = 1.0f;
        
        #endregion
        
        #region 楼层相关常量
        
        /// <summary>
        /// 最大楼层数（包括地下层）
        /// 例如：0=地下一层，1=第一层，2=第二层...
        /// </summary>
        public const int MAX_FLOORS = 6;
        
        /// <summary>
        /// 地面层索引（第一层）
        /// </summary>
        public const int GROUND_FLOOR = 1;
        
        #endregion
        
        #region Tile 层级索引
        
        /// <summary>
        /// 地形层索引 - 仅在 z=0（第一层地面）有效
        /// 内容：草地、泥土、沙子、水、道路等自然地形
        /// </summary>
        public const int LAYER_GROUND = 0;
        
        /// <summary>
        /// 地板层索引 - 需要承重支撑
        /// 内容：木地板、瓷砖、石板等人造地面
        /// 特性：同时也是下层的"天花板"
        /// </summary>
        public const int LAYER_FLOOR = 1;
        
        /// <summary>
        /// 装饰层索引 - 渲染位置随基底变化
        /// 内容：落叶、血迹、裂缝等地面装饰
        /// 特性：建造地板时自动清除地形装饰
        /// </summary>
        public const int LAYER_DECOR = 2;
        
        #endregion
    }
}