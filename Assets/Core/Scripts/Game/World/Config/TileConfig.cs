/**
 * TileConfig.cs
 * 静态瓦片配置（用于 Luban 配置表）
 * 
 * 混合系统中，TileConfig 只定义静态内容：
 * - 地形、地板、墙壁、屋顶等
 * 
 * 动态对象（家具、容器、门等）使用 EntityConfig
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 静态瓦片配置
    /// </summary>
    [Serializable]
    public class TileConfig
    {
        #region 基础属性
        
        /// <summary>
        /// 瓦片类型ID（主键）
        /// </summary>
        public int TileId;
        
        /// <summary>
        /// 瓦片名称
        /// </summary>
        public string TileName;
        
        /// <summary>
        /// 分类
        /// </summary>
        public TileCategory Category;
        
        /// <summary>
        /// 所属渲染层
        /// </summary>
        public int Layer;
        
        #endregion
        
        #region 精灵资源
        
        /// <summary>
        /// 图集名称
        /// </summary>
        public string SpriteAtlas;
        
        /// <summary>
        /// 精灵名称列表（支持多个变体）
        /// </summary>
        public string[] SpriteNames;
        
        /// <summary>
        /// 是否随机选择精灵变体
        /// </summary>
        public bool RandomSprite;
        
        #endregion
        
        #region 游戏属性
        
        /// <summary>
        /// 默认标志位
        /// </summary>
        public TileFlags DefaultFlags;
        
        /// <summary>
        /// 移动消耗（1.0 正常，>1 减速，0 不可通行）
        /// </summary>
        public float MoveCost;
        
        /// <summary>
        /// 最大耐久度（0 表示不可破坏）
        /// </summary>
        public int MaxHealth;
        
        #endregion
        
        #region 视觉效果
        
        /// <summary>
        /// 是否投射阴影
        /// </summary>
        public bool CastShadow;
        
        /// <summary>
        /// 渲染排序偏移
        /// </summary>
        public int SortingOrderOffset;
        
        #endregion
        
        #region 音效
        
        /// <summary>
        /// 行走音效ID
        /// </summary>
        public string FootstepSoundId;
        
        #endregion
        
        #region 构造函数
        
        public TileConfig()
        {
            MoveCost = 1.0f;
            SpriteNames = Array.Empty<string>();
        }
        
        #endregion
        
        #region 工厂方法
        
        /// <summary>
        /// 创建地形配置
        /// </summary>
        public static TileConfig CreateTerrain(int id, string name, string spriteName, 
            float moveCost = 1.0f)
        {
            return new TileConfig
            {
                TileId = id,
                TileName = name,
                Category = TileCategory.Terrain,
                Layer = MapConstants.LAYER_GROUND,
                SpriteNames = new[] { spriteName },
                MoveCost = moveCost,
                DefaultFlags = TileFlags.None
            };
        }
        
        /// <summary>
        /// 创建地板配置
        /// </summary>
        public static TileConfig CreateFloor(int id, string name, string spriteName)
        {
            return new TileConfig
            {
                TileId = id,
                TileName = name,
                Category = TileCategory.Floor,
                Layer = MapConstants.LAYER_FLOOR,
                SpriteNames = new[] { spriteName },
                MoveCost = 1.0f,
                DefaultFlags = TileFlags.None
            };
        }
        
        /// <summary>
        /// 创建墙壁配置
        /// </summary>
        public static TileConfig CreateWall(int id, string name, string spriteName, 
            int health = 100)
        {
            return new TileConfig
            {
                TileId = id,
                TileName = name,
                Category = TileCategory.Wall,
                Layer = MapConstants.LAYER_WALL,
                SpriteNames = new[] { spriteName },
                MoveCost = 0f,
                MaxHealth = health,
                DefaultFlags = TileFlags.Blocking | TileFlags.BlockSight,
                CastShadow = true
            };
        }
        
        /// <summary>
        /// 创建屋顶配置
        /// </summary>
        public static TileConfig CreateRoof(int id, string name, string spriteName)
        {
            return new TileConfig
            {
                TileId = id,
                TileName = name,
                Category = TileCategory.Roof,
                Layer = MapConstants.LAYER_ROOF,
                SpriteNames = new[] { spriteName },
                DefaultFlags = TileFlags.None
            };
        }
        
        /// <summary>
        /// 创建水/液体配置
        /// </summary>
        public static TileConfig CreateWater(int id, string name, string spriteName)
        {
            return new TileConfig
            {
                TileId = id,
                TileName = name,
                Category = TileCategory.Terrain,
                Layer = MapConstants.LAYER_GROUND,
                SpriteNames = new[] { spriteName },
                MoveCost = 2.0f, // 水中移动减速
                DefaultFlags = TileFlags.IsLiquid
            };
        }
        
        #endregion
        
        #region 属性
        
        public bool IsBlocking => (DefaultFlags & TileFlags.Blocking) != 0;
        public bool IsDestructible => MaxHealth > 0;
        public int SpriteVariantCount => SpriteNames?.Length ?? 0;
        
        #endregion
        
        /// <summary>
        /// 获取随机精灵变体索引
        /// </summary>
        public byte GetRandomSpriteVariant()
        {
            if (!RandomSprite || SpriteNames == null || SpriteNames.Length <= 1)
                return 0;
            return (byte)UnityEngine.Random.Range(0, Mathf.Min(SpriteNames.Length, 16));
        }
        
        /// <summary>
        /// 创建默认的 TileLayerData
        /// </summary>
        public TileLayerData CreateLayerData()
        {
            return new TileLayerData(
                (ushort)TileId,
                GetRandomSpriteVariant(),
                DefaultFlags
            );
        }
        
        public override string ToString()
        {
            return $"TileConfig({TileId}: {TileName}, {Category}, Layer:{Layer})";
        }
    }
}
