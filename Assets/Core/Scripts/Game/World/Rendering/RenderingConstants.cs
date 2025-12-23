/**
 * RenderingConstants.cs
 * 渲染系统常量和配置
 * 
 * 支持 URP (Universal Render Pipeline) 2D Renderer
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// 渲染层级配置
    /// </summary>
    [Serializable]
    public class RenderLayerConfig
    {
        /// <summary>
        /// 层级索引（对应 MapConstants.LAYER_XXX）
        /// </summary>
        public int LayerIndex;
        
        /// <summary>
        /// 层级名称
        /// </summary>
        public string LayerName;
        
        /// <summary>
        /// Unity Sorting Layer 名称
        /// </summary>
        public string SortingLayerName;
        
        /// <summary>
        /// 基础排序值
        /// </summary>
        public int BaseSortingOrder;
        
        /// <summary>
        /// 是否默认可见
        /// </summary>
        public bool DefaultVisible;
        
        public RenderLayerConfig(int index, string name, string sortingLayer, 
            int baseSortingOrder, bool visible = true)
        {
            LayerIndex = index;
            LayerName = name;
            SortingLayerName = sortingLayer;
            BaseSortingOrder = baseSortingOrder;
            DefaultVisible = visible;
        }
    }
    
    /// <summary>
    /// 渲染系统常量
    /// </summary>
    public static class RenderingConstants
    {
        #region URP 材质路径
        
        /// <summary>
        /// URP 2D Sprite Lit 默认材质路径
        /// </summary>
        public const string URP_SPRITE_LIT_MATERIAL = "Sprite-Lit-Default";
        
        /// <summary>
        /// URP 2D Sprite Unlit 默认材质路径
        /// </summary>
        public const string URP_SPRITE_UNLIT_MATERIAL = "Sprite-Unlit-Default";
        
        /// <summary>
        /// URP 2D Sprite Mask 材质路径
        /// </summary>
        public const string URP_SPRITE_MASK_MATERIAL = "Sprite-Mask-Default";
        
        #endregion
        
        #region Sorting Layer 名称
        // 需要在 Unity 中预先创建这些 Sorting Layer
        // Edit -> Project Settings -> Tags and Layers -> Sorting Layers
        
        public const string SORTING_LAYER_GROUND = "Ground";
        public const string SORTING_LAYER_FLOOR = "Floor";
        public const string SORTING_LAYER_FLOOR_DECOR = "FloorDecor";
        public const string SORTING_LAYER_WALL = "Wall";
        public const string SORTING_LAYER_WALL_DECOR = "WallDecor";
        public const string SORTING_LAYER_ENTITY = "Entity";
        public const string SORTING_LAYER_ROOF = "Roof";
        public const string SORTING_LAYER_UI = "UI";
        
        #endregion
        
        #region 排序相关
        
        /// <summary>
        /// Y 坐标对排序的影响因子
        /// 用于实现"越靠下的物体渲染在越上面"的效果
        /// </summary>
        public const int SORTING_ORDER_PER_Y = 100;
        
        /// <summary>
        /// Entity 基础排序值
        /// </summary>
        public const int ENTITY_BASE_SORTING_ORDER = 0;
        
        #endregion
        
        #region 渲染配置
        
        /// <summary>
        /// 视野外扩展 Chunk 数量（用于预加载）
        /// </summary>
        public const int VIEWPORT_EXTEND_CHUNKS = 1;
        
        /// <summary>
        /// Chunk 渲染器对象池初始大小
        /// </summary>
        public const int CHUNK_RENDERER_POOL_SIZE = 16;
        
        /// <summary>
        /// Entity View 对象池初始大小
        /// </summary>
        public const int ENTITY_VIEW_POOL_SIZE = 100;
        
        #endregion
        
        #region 默认层级配置
        
        /// <summary>
        /// 获取默认的层级配置
        /// </summary>
        public static RenderLayerConfig[] GetDefaultLayerConfigs()
        {
            return new RenderLayerConfig[]
            {
                new RenderLayerConfig(
                    MapConstants.LAYER_GROUND,
                    "Ground",
                    SORTING_LAYER_GROUND,
                    0,
                    true
                ),
                new RenderLayerConfig(
                    MapConstants.LAYER_FLOOR,
                    "Floor",
                    SORTING_LAYER_FLOOR,
                    0,
                    true
                ),
                new RenderLayerConfig(
                    MapConstants.LAYER_FLOOR_DECOR,
                    "FloorDecor",
                    SORTING_LAYER_FLOOR_DECOR,
                    0,
                    true
                ),
                new RenderLayerConfig(
                    MapConstants.LAYER_WALL,
                    "Wall",
                    SORTING_LAYER_WALL,
                    0,
                    true
                ),
                new RenderLayerConfig(
                    MapConstants.LAYER_WALL_DECOR,
                    "WallDecor",
                    SORTING_LAYER_WALL_DECOR,
                    0,
                    true
                ),
                new RenderLayerConfig(
                    MapConstants.LAYER_ROOF,
                    "Roof",
                    SORTING_LAYER_ROOF,
                    0,
                    true  // 屋顶默认可见，进入室内时隐藏
                )
            };
        }
        
        #endregion
    }
}
