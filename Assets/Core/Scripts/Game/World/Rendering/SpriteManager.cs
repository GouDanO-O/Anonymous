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
            
            CreateDefaultSprites();
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
            if (config == null)
            {
                Debug.LogWarning($"[SpriteManager] 未找到 TileConfig: {tileId}");
                return _errorSprite;
            }
            
            return GetSpriteFromConfig(config.SpriteAtlas, config.SpriteNames, spriteVariant);
        }
        
        /// <summary>
        /// 获取 Entity 精灵
        /// </summary>
        public Sprite GetEntitySprite(int configId, int spriteIndex = 0)
        {
            var config = GetEntityConfig(configId);
            if (config == null)
            {
                Debug.LogWarning($"[SpriteManager] 未找到 EntityConfig: {configId}");
                return _errorSprite;
            }
            
            return GetSpriteFromConfig(config.SpriteAtlas, config.SpriteNames, spriteIndex);
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
                return _defaultSprite;
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
            
            // 缓存结果
            _spriteCache[cacheKey] = sprite;
            
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
            // 这需要改为异步接口，这里先返回占位符
            
            Debug.LogWarning($"[SpriteManager] 无法加载精灵: {atlasName}/{spriteName}");
            return _errorSprite;
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
