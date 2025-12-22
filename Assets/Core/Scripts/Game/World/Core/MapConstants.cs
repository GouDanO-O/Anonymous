/**
 * MapConstants.cs
 * 地图系统常量定义
 * 
 * 混合系统设计：
 * - Tile 层：只负责静态地形、建筑结构
 * - Entity 系统：负责动态对象、可交互物品
 */

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 地图系统常量
    /// </summary>
    public static class MapConstants
    {
        #region Chunk 相关常量
        
        /// <summary>
        /// Chunk 的边长（以 Tile 为单位）
        /// </summary>
        public const int CHUNK_SIZE = 16;
        
        /// <summary>
        /// 每个 Chunk 包含的 Tile 总数
        /// </summary>
        public const int TILES_PER_CHUNK = CHUNK_SIZE * CHUNK_SIZE; // 256
        
        #endregion
        
        #region Tile 相关常量
        
        /// <summary>
        /// 单个 Tile 在 Unity 世界坐标中的尺寸
        /// </summary>
        public const float TILE_SIZE = 1.0f;
        
        /// <summary>
        /// Tile 渲染层级数量（只包含静态层）
        /// 混合系统中，Tile 只负责：地形、地板、墙壁、屋顶
        /// </summary>
        public const int TILE_LAYER_COUNT = 6;
        
        /// <summary>
        /// 空 Tile ID，表示该位置没有内容
        /// </summary>
        public const ushort EMPTY_TILE_ID = 0;
        
        #endregion
        
        #region Tile 渲染层级索引
        // 
        // 设计原则：Tile 层只存储"静态"内容
        // - 不会移动
        // - 不会频繁变化
        // - 主要是建筑结构
        //
        
        /// <summary>
        /// Layer 0: 地形层
        /// 内容：草地、泥土、沙子、水、道路等基础地形
        /// 特点：几乎永远不变，是地图的最底层
        /// </summary>
        public const int LAYER_GROUND = 0;
        
        /// <summary>
        /// Layer 1: 地板层
        /// 内容：木地板、瓷砖、地毯、石板等人造地面
        /// 特点：建筑内部的地面，可能被破坏
        /// </summary>
        public const int LAYER_FLOOR = 1;
        
        /// <summary>
        /// Layer 2: 地面装饰层
        /// 内容：血迹、裂缝、落叶、脚印、污渍等
        /// 特点：纯视觉效果，不影响游戏逻辑
        /// </summary>
        public const int LAYER_FLOOR_DECOR = 2;
        
        /// <summary>
        /// Layer 3: 墙壁层
        /// 内容：墙壁、栅栏、低矮围墙
        /// 特点：阻挡移动和视线，可被破坏
        /// 说明：合并了之前的 WallBase 和 WallTop
        /// </summary>
        public const int LAYER_WALL = 3;
        
        /// <summary>
        /// Layer 4: 墙壁装饰/开口层
        /// 内容：窗框、门框（注意：门本身是 Entity）、墙上装饰
        /// 特点：依附于墙壁存在的静态装饰
        /// </summary>
        public const int LAYER_WALL_DECOR = 4;
        
        /// <summary>
        /// Layer 5: 屋顶层
        /// 内容：屋顶、天花板、遮挡物
        /// 特点：用于室内外切换时的显示/隐藏
        /// </summary>
        public const int LAYER_ROOF = 5;
        
        #endregion
        
        #region Entity 相关常量
        
        /// <summary>
        /// 无效的 Entity ID
        /// </summary>
        public const int INVALID_ENTITY_ID = -1;
        
        /// <summary>
        /// Entity ID 起始值
        /// </summary>
        public const int ENTITY_ID_START = 1;
        
        #endregion
        
        #region 位运算辅助
        
        /// <summary>
        /// 用于快速计算 Chunk 坐标的位移量
        /// CHUNK_SIZE = 16 = 2^4，所以右移4位等于除以16
        /// </summary>
        public const int CHUNK_SIZE_SHIFT = 4;
        
        /// <summary>
        /// 用于快速计算 Chunk 内局部坐标的掩码
        /// 16 - 1 = 15 = 0b1111，与运算等于取模
        /// </summary>
        public const int CHUNK_LOCAL_MASK = CHUNK_SIZE - 1; // 15
        
        #endregion
    }
}
