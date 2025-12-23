/**
 * SpriteManager.cs
 * 精灵资源管理器
 * 
 * 负责：
 * - 从图集加载精灵
 * - 缓存已加载的精灵
 * - 支持 YooAsset 资源加载
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// 精灵资源管理器
    /// </summary>
    public class SpriteManager
    {
        #region 单例
        
        private static SpriteManager _instance;
        public static SpriteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SpriteManager();
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 精灵缓存 (key: "atlasName/spriteName")
        /// </summary>
        private Dictionary<string, Sprite> _spriteCache;
        
        /// <summary>
        /// 图集缓存
        /// </summary>
        private Dictionary<string, Sprite[]> _atlasCache;
        
        /// <summary>
        /// Tile 配置缓存
        /// </summary>
        private Dictionary<int, TileConfig> _tileConfigs;
        
        /// <summary>
        /// Entity 配置缓存
        /// </summary>
        private Dictionary<int, EntityConfig> _entityConfigs;
        
        /// <summary>
        /// 默认精灵（找不到资源时使用）
        /// </summary>
        private Sprite _defaultSprite;
        
        /// <summary>
        /// 错误精灵（加载失败时使用）
        /// </summary>
        private Sprite _errorSprite;
        
        #endregion
        
        #region 构造函数
        
        private SpriteManager()
        {
            _spriteCache = new Dictionary<string, Sprite>();
            _atlasCache = new Dictionary<string, Sprite[]>();
            _tileConfigs = new Dictionary<int, TileConfig>();
            _entityConfigs = new Dictionary<int, EntityConfig>();
            _tilePlaceholderCache = new Dictionary<ushort, Sprite>();
            _entityPlaceholderCache = new Dictionary<int, Sprite>();
            
            CreateDefaultSprites();
            
            Debug.Log("[SpriteManager] 初始化完成，占位精灵模式已启用");
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 创建默认的占位精灵
        /// </summary>
        private void CreateDefaultSprites()
        {
            // 创建默认精灵（白色方块）
            _defaultSprite = CreateColoredSprite(Color.white, "Default");
            
            // 创建错误精灵（品红色方块，表示资源缺失）
            _errorSprite = CreateColoredSprite(Color.magenta, "Error");
        }
        
        /// <summary>
        /// 创建纯色精灵
        /// </summary>
        private Sprite CreateColoredSprite(Color color, string name)
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = name;
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0.5f),
                32f
            );
        }
        
        /// <summary>
        /// 初始化配置数据（从 Luban 加载后调用）
        /// </summary>
        public void Initialize(IEnumerable<TileConfig> tileConfigs, 
            IEnumerable<EntityConfig> entityConfigs)
        {
            _tileConfigs.Clear();
            _entityConfigs.Clear();
            
            if (tileConfigs != null)
            {
                foreach (var config in tileConfigs)
                {
                    _tileConfigs[config.TileId] = config;
                }
            }
            
            if (entityConfigs != null)
            {
                foreach (var config in entityConfigs)
                {
                    _entityConfigs[config.ConfigId] = config;
                }
            }
            
            Debug.Log($"[SpriteManager] 初始化完成: {_tileConfigs.Count} Tile配置, " +
                      $"{_entityConfigs.Count} Entity配置");
        }
        
        #endregion
        
        #region 配置获取
        
        /// <summary>
        /// 获取 Tile 配置
        /// </summary>
        public TileConfig GetTileConfig(int tileId)
        {
            _tileConfigs.TryGetValue(tileId, out var config);
            return config;
        }
        
        /// <summary>
        /// 获取 Entity 配置
        /// </summary>
        public EntityConfig GetEntityConfig(int configId)
        {
            _entityConfigs.TryGetValue(configId, out var config);
            return config;
        }
        
        #endregion
        
        #region 占位精灵缓存
        
        /// <summary>
        /// Tile 占位精灵缓存（按 TileId）
        /// </summary>
        private Dictionary<ushort, Sprite> _tilePlaceholderCache;
        
        /// <summary>
        /// Entity 占位精灵缓存（按 ConfigId）
        /// </summary>
        private Dictionary<int, Sprite> _entityPlaceholderCache;
        
        /// <summary>
        /// 是否使用占位精灵模式（没有实际资源时自动启用）
        /// </summary>
        private bool _usePlaceholderMode = true;
        
        /// <summary>
        /// 是否使用占位精灵模式
        /// </summary>
        public bool UsePlaceholderMode
        {
            get => _usePlaceholderMode;
            set => _usePlaceholderMode = value;
        }
        
        #endregion
        
        #region 精灵获取
        
        /// <summary>
        /// 获取 Tile 精灵
        /// </summary>
        public Sprite GetTileSprite(ushort tileId, byte spriteVariant = 0)
        {
            if (tileId == MapConstants.EMPTY_TILE_ID)
            {
                return null;
            }
            
            var config = GetTileConfig(tileId);
            
            // 如果有配置，尝试加载真实精灵
            if (config != null)
            {
                Sprite realSprite = TryLoadRealSprite(config.SpriteAtlas, config.SpriteNames, spriteVariant);
                if (realSprite != null && realSprite != _errorSprite)
                {
                    return realSprite;
                }
            }
            
            // 使用占位精灵
            if (_usePlaceholderMode)
            {
                return GetTilePlaceholderSprite(tileId, config);
            }
            
            return _errorSprite;
        }
        
        /// <summary>
        /// 获取 Entity 精灵
        /// </summary>
        public Sprite GetEntitySprite(int configId, int spriteIndex = 0)
        {
            var config = GetEntityConfig(configId);
            
            // 如果有配置，尝试加载真实精灵
            if (config != null)
            {
                Sprite realSprite = TryLoadRealSprite(config.SpriteAtlas, config.SpriteNames, spriteIndex);
                if (realSprite != null && realSprite != _errorSprite)
                {
                    return realSprite;
                }
            }
            
            // 使用占位精灵
            if (_usePlaceholderMode)
            {
                return GetEntityPlaceholderSprite(configId, config);
            }
            
            return _errorSprite;
        }
        
        /// <summary>
        /// 尝试加载真实精灵
        /// </summary>
        private Sprite TryLoadRealSprite(string atlasName, string[] spriteNames, int index)
        {
            if (spriteNames == null || spriteNames.Length == 0)
            {
                return null;
            }
            
            index = Mathf.Clamp(index, 0, spriteNames.Length - 1);
            string spriteName = spriteNames[index];
            
            return GetSprite(atlasName, spriteName);
        }
        
        /// <summary>
        /// 获取 Tile 占位精灵
        /// </summary>
        private Sprite GetTilePlaceholderSprite(ushort tileId, TileConfig config)
        {
            // 检查缓存
            if (_tilePlaceholderCache.TryGetValue(tileId, out var cached))
            {
                return cached;
            }
            
            // 根据 TileId 或 Category 生成颜色
            Color color = GetTileColor(tileId, config);
            string name = config != null ? config.TileName : $"Tile_{tileId}";
            
            Sprite sprite = CreateColoredSprite(color, name);
            _tilePlaceholderCache[tileId] = sprite;
            
            return sprite;
        }
        
        /// <summary>
        /// 获取 Entity 占位精灵
        /// </summary>
        private Sprite GetEntityPlaceholderSprite(int configId, EntityConfig config)
        {
            // 检查缓存
            if (_entityPlaceholderCache.TryGetValue(configId, out var cached))
            {
                return cached;
            }
            
            // 根据 EntityType 生成颜色
            Color color = GetEntityColor(configId, config);
            string name = config != null ? config.EntityName : $"Entity_{configId}";
            
            Sprite sprite = CreateColoredSprite(color, name);
            _entityPlaceholderCache[configId] = sprite;
            
            return sprite;
        }
        
        /// <summary>
        /// 根据 Tile 类型获取颜色
        /// </summary>
        private Color GetTileColor(ushort tileId, TileConfig config)
        {
            if (config != null)
            {
                // 根据分类返回不同颜色
                switch (config.Category)
                {
                    case TileCategory.Terrain:
                        // 地形 - 绿色系
                        if (config.TileName.Contains("水") || config.TileName.Contains("Water"))
                            return new Color(0.2f, 0.4f, 0.8f); // 蓝色 - 水
                        if (config.TileName.Contains("泥") || config.TileName.Contains("Dirt"))
                            return new Color(0.6f, 0.4f, 0.2f); // 棕色 - 泥土
                        return new Color(0.3f, 0.7f, 0.3f); // 绿色 - 草地
                        
                    case TileCategory.Floor:
                        // 地板 - 棕色/灰色系
                        if (config.TileName.Contains("石") || config.TileName.Contains("Stone"))
                            return new Color(0.5f, 0.5f, 0.55f); // 灰色 - 石地板
                        return new Color(0.6f, 0.45f, 0.3f); // 棕色 - 木地板
                        
                    case TileCategory.FloorDecor:
                        // 地面装饰 - 半透明
                        return new Color(0.8f, 0.8f, 0.8f, 0.5f);
                        
                    case TileCategory.Wall:
                        // 墙壁 - 深色
                        if (config.TileName.Contains("石") || config.TileName.Contains("Stone"))
                            return new Color(0.4f, 0.4f, 0.45f); // 深灰 - 石墙
                        return new Color(0.5f, 0.35f, 0.2f); // 深棕 - 木墙
                        
                    case TileCategory.WallDecor:
                        // 墙壁装饰
                        return new Color(0.7f, 0.5f, 0.3f);
                        
                    case TileCategory.Roof:
                        // 屋顶 - 红/棕色
                        return new Color(0.6f, 0.3f, 0.2f);
                        
                    default:
                        return Color.gray;
                }
            }
            
            // 没有配置，根据 TileId 生成随机但一致的颜色
            return GenerateColorFromId(tileId);
        }
        
        /// <summary>
        /// 根据 Entity 类型获取颜色
        /// </summary>
        private Color GetEntityColor(int configId, EntityConfig config)
        {
            if (config != null)
            {
                switch (config.EntityType)
                {
                    case EntityType.Furniture:
                        return new Color(0.7f, 0.5f, 0.3f); // 棕色 - 家具
                        
                    case EntityType.Container:
                        return new Color(0.4f, 0.6f, 0.8f); // 蓝色 - 容器
                        
                    case EntityType.Door:
                        return new Color(0.5f, 0.3f, 0.15f); // 深棕 - 门
                        
                    case EntityType.Window:
                        return new Color(0.7f, 0.85f, 0.95f); // 浅蓝 - 窗
                        
                    case EntityType.DroppedItem:
                        return new Color(0.9f, 0.8f, 0.2f); // 黄色 - 掉落物
                        
                    case EntityType.LightSource:
                        return new Color(1f, 0.9f, 0.5f); // 亮黄 - 光源
                        
                    default:
                        return new Color(0.8f, 0.8f, 0.8f);
                }
            }
            
            return GenerateColorFromId(configId);
        }
        
        /// <summary>
        /// 根据 ID 生成一致的颜色
        /// </summary>
        private Color GenerateColorFromId(int id)
        {
            // 使用简单的哈希来生成一致的颜色
            float hue = (id * 0.618033988749895f) % 1f; // 黄金比例
            float saturation = 0.5f + (id % 5) * 0.1f;
            float value = 0.6f + (id % 3) * 0.1f;
            
            return Color.HSVToRGB(hue, saturation, value);
        }
        
        /// <summary>
        /// 从配置获取精灵
        /// </summary>
        private Sprite GetSpriteFromConfig(string atlasName, string[] spriteNames, int index)
        {
            if (spriteNames == null || spriteNames.Length == 0)
            {
                return _defaultSprite;
            }
            
            // 确保索引有效
            index = Mathf.Clamp(index, 0, spriteNames.Length - 1);
            string spriteName = spriteNames[index];
            
            return GetSprite(atlasName, spriteName);
        }
        
        /// <summary>
        /// 获取精灵（带缓存）
        /// </summary>
        public Sprite GetSprite(string atlasName, string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }
            
            // 构造缓存键
            string cacheKey = string.IsNullOrEmpty(atlasName) 
                ? spriteName 
                : $"{atlasName}/{spriteName}";
            
            // 检查缓存
            if (_spriteCache.TryGetValue(cacheKey, out var cachedSprite))
            {
                return cachedSprite;
            }
            
            // 加载精灵
            Sprite sprite = LoadSprite(atlasName, spriteName);
            
            // 缓存结果（只缓存成功加载的）
            if (sprite != null && sprite != _errorSprite)
            {
                _spriteCache[cacheKey] = sprite;
            }
            
            return sprite;
        }
        
        /// <summary>
        /// 实际加载精灵
        /// 这里提供多种加载方式，可以根据项目需求选择
        /// </summary>
        private Sprite LoadSprite(string atlasName, string spriteName)
        {
            Sprite sprite = null;
            
            // 方式1：从 Resources 加载（简单但不推荐用于大量资源）
            sprite = LoadFromResources(atlasName, spriteName);
            if (sprite != null) return sprite;
            
            // 方式2：从已加载的图集中查找
            sprite = LoadFromAtlas(atlasName, spriteName);
            if (sprite != null) return sprite;
            
            // TODO: 方式3：通过 YooAsset 异步加载
            // 这需要改为异步接口，这里先返回 null
            
            // 返回 null，让调用者决定是否使用占位精灵
            return null;
        }
        
        /// <summary>
        /// 从 Resources 加载
        /// </summary>
        private Sprite LoadFromResources(string atlasName, string spriteName)
        {
            string path;
            
            if (string.IsNullOrEmpty(atlasName))
            {
                path = $"Sprites/{spriteName}";
            }
            else
            {
                path = $"Sprites/{atlasName}/{spriteName}";
            }
            
            return Resources.Load<Sprite>(path);
        }
        
        /// <summary>
        /// 从已加载的图集中查找
        /// </summary>
        private Sprite LoadFromAtlas(string atlasName, string spriteName)
        {
            if (string.IsNullOrEmpty(atlasName))
            {
                return null;
            }
            
            if (!_atlasCache.TryGetValue(atlasName, out var sprites))
            {
                return null;
            }
            
            foreach (var sprite in sprites)
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region 图集管理
        
        /// <summary>
        /// 注册图集（预加载时调用）
        /// </summary>
        public void RegisterAtlas(string atlasName, Sprite[] sprites)
        {
            if (string.IsNullOrEmpty(atlasName) || sprites == null)
            {
                return;
            }
            
            _atlasCache[atlasName] = sprites;
            
            // 同时缓存单个精灵
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    string cacheKey = $"{atlasName}/{sprite.name}";
                    _spriteCache[cacheKey] = sprite;
                }
            }
            
            Debug.Log($"[SpriteManager] 注册图集: {atlasName}, {sprites.Length} 个精灵");
        }
        
        /// <summary>
        /// 卸载图集
        /// </summary>
        public void UnloadAtlas(string atlasName)
        {
            if (_atlasCache.TryGetValue(atlasName, out var sprites))
            {
                // 从精灵缓存中移除
                foreach (var sprite in sprites)
                {
                    if (sprite != null)
                    {
                        string cacheKey = $"{atlasName}/{sprite.name}";
                        _spriteCache.Remove(cacheKey);
                    }
                }
                
                _atlasCache.Remove(atlasName);
            }
        }
        
        #endregion
        
        #region 清理
        
        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
            _atlasCache.Clear();
            _tilePlaceholderCache.Clear();
            _entityPlaceholderCache.Clear();
        }
        
        /// <summary>
        /// 清空配置
        /// </summary>
        public void ClearConfigs()
        {
            _tileConfigs.Clear();
            _entityConfigs.Clear();
        }
        
        #endregion
    }
}
