/*******************************************************************************
 * 文件名:    SpriteManager.cs
 * 描述:      精灵资源管理器，管理Tile和Entity的精灵资源
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   SpriteManager 提供：
 *   - 精灵资源加载和缓存
 *   - 支持YooAsset异步加载
 *   - 支持精灵图集
 *   - 运行时精灵生成（调试用）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// 精灵管理器
    /// </summary>
    public class SpriteManager : MonoBehaviour
    {
        #region 单例

        private static SpriteManager _instance;
        public static SpriteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SpriteManager");
                    _instance = go.AddComponent<SpriteManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #endregion

        #region 字段

        /// <summary>
        /// 精灵缓存
        /// </summary>
        private Dictionary<string, Sprite> _spriteCache;

        /// <summary>
        /// 图集缓存
        /// </summary>
        private Dictionary<string, SpriteAtlas> _atlasCache;

        /// <summary>
        /// 生成的调试精灵缓存
        /// </summary>
        private Dictionary<string, Sprite> _generatedSprites;

        /// <summary>
        /// 默认精灵
        /// </summary>
        private Sprite _defaultSprite;

        /// <summary>
        /// 错误精灵
        /// </summary>
        private Sprite _errorSprite;

        #endregion

        #region 初始化

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _spriteCache = new Dictionary<string, Sprite>();
            _atlasCache = new Dictionary<string, SpriteAtlas>();
            _generatedSprites = new Dictionary<string, Sprite>();

            CreateBuiltInSprites();
        }

        /// <summary>
        /// 创建内置精灵
        /// </summary>
        private void CreateBuiltInSprites()
        {
            // 默认精灵（白色方块）
            _defaultSprite = CreateSolidSprite("default", Color.white, 16);

            // 错误精灵（粉红色方块）
            _errorSprite = CreateSolidSprite("error", Color.magenta, 16);
        }

        #endregion

        #region 加载精灵

        /// <summary>
        /// 获取精灵
        /// </summary>
        public Sprite GetSprite(string spritePath)
        {
            if (string.IsNullOrEmpty(spritePath))
                return _defaultSprite;

            // 检查缓存
            if (_spriteCache.TryGetValue(spritePath, out var cached))
                return cached;

            // 尝试从Resources加载
            var sprite = LoadFromResources(spritePath);
            if (sprite != null)
            {
                _spriteCache[spritePath] = sprite;
                return sprite;
            }

            // 尝试从YooAsset加载
            sprite = LoadFromYooAsset(spritePath);
            if (sprite != null)
            {
                _spriteCache[spritePath] = sprite;
                return sprite;
            }

            // 返回错误精灵
            Debug.LogWarning($"Failed to load sprite: {spritePath}");
            return _errorSprite;
        }

        /// <summary>
        /// 从Resources加载
        /// </summary>
        private Sprite LoadFromResources(string path)
        {
            return Resources.Load<Sprite>(path);
        }

        /// <summary>
        /// 从YooAsset加载
        /// </summary>
        private Sprite LoadFromYooAsset(string path)
        {
            // TODO: 实现YooAsset加载
            // var handle = YooAssets.LoadAssetSync<Sprite>(path);
            // return handle.AssetObject as Sprite;
            return null;
        }

        /// <summary>
        /// 异步获取精灵
        /// </summary>
        public void GetSpriteAsync(string spritePath, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(spritePath))
            {
                callback?.Invoke(_defaultSprite);
                return;
            }

            // 检查缓存
            if (_spriteCache.TryGetValue(spritePath, out var cached))
            {
                callback?.Invoke(cached);
                return;
            }

            // 异步加载
            StartCoroutine(LoadSpriteAsync(spritePath, callback));
        }

        private System.Collections.IEnumerator LoadSpriteAsync(string path, Action<Sprite> callback)
        {
            // 先尝试Resources
            var request = Resources.LoadAsync<Sprite>(path);
            yield return request;

            if (request.asset != null)
            {
                var sprite = request.asset as Sprite;
                _spriteCache[path] = sprite;
                callback?.Invoke(sprite);
                yield break;
            }

            // TODO: YooAsset异步加载

            callback?.Invoke(_errorSprite);
        }

        #endregion

        #region 图集

        /// <summary>
        /// 加载图集
        /// </summary>
        public SpriteAtlas LoadAtlas(string atlasPath)
        {
            if (_atlasCache.TryGetValue(atlasPath, out var cached))
                return cached;

            // TODO: 实现图集加载
            return null;
        }

        /// <summary>
        /// 从图集获取精灵
        /// </summary>
        public Sprite GetSpriteFromAtlas(string atlasPath, string spriteName)
        {
            var atlas = LoadAtlas(atlasPath);
            return atlas?.GetSprite(spriteName);
        }

        #endregion

        #region 动态生成精灵

        /// <summary>
        /// 创建纯色精灵
        /// </summary>
        public Sprite CreateSolidSprite(string name, Color color, int size = 16)
        {
            string key = $"solid_{name}_{ColorToHex(color)}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _generatedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 创建渐变精灵
        /// </summary>
        public Sprite CreateGradientSprite(string name, Color colorA, Color colorB, 
            GradientDirection direction = GradientDirection.Vertical, int size = 16)
        {
            string key = $"gradient_{name}_{direction}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = direction == GradientDirection.Vertical 
                        ? (float)y / size 
                        : (float)x / size;
                    texture.SetPixel(x, y, Color.Lerp(colorA, colorB, t));
                }
            }
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _generatedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 创建带边框的精灵
        /// </summary>
        public Sprite CreateBorderedSprite(string name, Color fillColor, Color borderColor, 
            int size = 16, int borderWidth = 1)
        {
            string key = $"bordered_{name}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < borderWidth || x >= size - borderWidth ||
                                   y < borderWidth || y >= size - borderWidth;
                    texture.SetPixel(x, y, isBorder ? borderColor : fillColor);
                }
            }
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _generatedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 创建图案精灵
        /// </summary>
        public Sprite CreatePatternSprite(string name, Color colorA, Color colorB, 
            PatternType pattern, int size = 16)
        {
            string key = $"pattern_{name}_{pattern}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool useColorA = pattern switch
                    {
                        PatternType.Checker => (x + y) % 2 == 0,
                        PatternType.VerticalStripes => x % 2 == 0,
                        PatternType.HorizontalStripes => y % 2 == 0,
                        PatternType.DiagonalStripes => (x + y) % 4 < 2,
                        PatternType.Dots => (x % 4 == 0) && (y % 4 == 0),
                        _ => true
                    };
                    texture.SetPixel(x, y, useColorA ? colorA : colorB);
                }
            }
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _generatedSprites[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// 创建圆形精灵
        /// </summary>
        public Sprite CreateCircleSprite(string name, Color color, int size = 16)
        {
            string key = $"circle_{name}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Bilinear;

            float center = size / 2f;
            float radius = size / 2f - 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _generatedSprites[key] = sprite;
            return sprite;
        }

        #endregion

        #region Tile精灵生成

        /// <summary>
        /// 为TileDef生成精灵
        /// </summary>
        public Sprite GenerateTileSprite(TileDef def)
        {
            if (def == null)
                return _defaultSprite;

            string key = $"tile_{def.DefId}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            // 根据Def类型生成不同样式的精灵
            Sprite sprite = def switch
            {
                TerrainDef terrain => GenerateTerrainSprite(terrain),
                FloorDef floor => GenerateFloorSprite(floor),
                WallDef wall => GenerateWallSprite(wall),
                _ => CreateSolidSprite(def.DefId, def.TileColor, 32)
            };

            _generatedSprites[key] = sprite;
            return sprite;
        }

        private Sprite GenerateTerrainSprite(TerrainDef terrain)
        {
            // 地形使用带噪声的纹理
            int size = 32;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            Color baseColor = terrain.TileColor;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.3f, y * 0.3f);
                    float variation = (noise - 0.5f) * 0.2f;
                    Color pixelColor = new Color(
                        Mathf.Clamp01(baseColor.r + variation),
                        Mathf.Clamp01(baseColor.g + variation),
                        Mathf.Clamp01(baseColor.b + variation),
                        baseColor.a
                    );
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite GenerateFloorSprite(FloorDef floor)
        {
            // 地板使用带格子的纹理
            return CreatePatternSprite(floor.DefId, floor.TileColor, 
                floor.TileColor * 0.9f, PatternType.Checker, 32);
        }

        private Sprite GenerateWallSprite(WallDef wall)
        {
            // 墙壁使用带边框的纹理
            return CreateBorderedSprite(wall.DefId, wall.TileColor, 
                wall.TileColor * 0.7f, 32, 2);
        }

        #endregion

        #region Entity精灵生成

        /// <summary>
        /// 为EntityDef生成精灵
        /// </summary>
        public Sprite GenerateEntitySprite(EntityDef def)
        {
            if (def == null)
                return _defaultSprite;

            string key = $"entity_{def.DefId}";
            if (_generatedSprites.TryGetValue(key, out var cached))
                return cached;

            int sizeX = Mathf.Max(1, def.Size.x) * 32;
            int sizeY = Mathf.Max(1, def.Size.y) * 32;

            Sprite sprite = def.Category switch
            {
                EntityCategory.Building => GenerateBuildingSprite(def, sizeX, sizeY),
                EntityCategory.Item => GenerateItemSprite(def),
                EntityCategory.Pawn => GeneratePawnSprite(def),
                EntityCategory.Plant => GeneratePlantSprite(def),
                _ => CreateSolidSprite(def.DefId, def.DefaultColor, 32)
            };

            _generatedSprites[key] = sprite;
            return sprite;
        }

        private Sprite GenerateBuildingSprite(EntityDef def, int sizeX, int sizeY)
        {
            var texture = new Texture2D(sizeX, sizeY);
            texture.filterMode = FilterMode.Point;

            Color baseColor = def.DefaultColor;
            Color borderColor = baseColor * 0.6f;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    bool isBorder = x < 2 || x >= sizeX - 2 || y < 2 || y >= sizeY - 2;
                    texture.SetPixel(x, y, isBorder ? borderColor : baseColor);
                }
            }
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, sizeX, sizeY), 
                new Vector2(0.5f, 0.5f), 32);
        }

        private Sprite GenerateItemSprite(EntityDef def)
        {
            return CreateCircleSprite(def.DefId, def.DefaultColor, 24);
        }

        private Sprite GeneratePawnSprite(EntityDef def)
        {
            // Pawn使用带头部的简单人形
            int size = 32;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            Color bodyColor = def.DefaultColor;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            // 身体
            for (int y = 4; y < 20; y++)
            {
                for (int x = 10; x < 22; x++)
                {
                    texture.SetPixel(x, y, bodyColor);
                }
            }

            // 头部
            float centerX = 16, centerY = 24;
            for (int y = 18; y < 30; y++)
            {
                for (int x = 10; x < 22; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (dist < 6)
                    {
                        texture.SetPixel(x, y, bodyColor * 1.1f);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Sprite GeneratePlantSprite(EntityDef def)
        {
            // 植物使用简单的树形
            int size = 32;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            Color leafColor = def.DefaultColor;
            Color trunkColor = new Color(0.4f, 0.25f, 0.1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            // 树干
            for (int y = 0; y < 12; y++)
            {
                for (int x = 14; x < 18; x++)
                {
                    texture.SetPixel(x, y, trunkColor);
                }
            }

            // 树冠
            float centerX = 16, centerY = 20;
            for (int y = 10; y < 30; y++)
            {
                for (int x = 6; x < 26; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (dist < 10)
                    {
                        float variation = Mathf.PerlinNoise(x * 0.5f, y * 0.5f) * 0.2f;
                        Color c = new Color(
                            leafColor.r + variation,
                            leafColor.g + variation,
                            leafColor.b + variation,
                            1
                        );
                        texture.SetPixel(x, y, c);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 颜色转十六进制
        /// </summary>
        private string ColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        /// <summary>
        /// 获取默认精灵
        /// </summary>
        public Sprite GetDefaultSprite()
        {
            return _defaultSprite;
        }

        /// <summary>
        /// 获取错误精灵
        /// </summary>
        public Sprite GetErrorSprite()
        {
            return _errorSprite;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
            // 不清除生成的精灵
        }

        #endregion
    }

    /// <summary>
    /// 简单图集类
    /// </summary>
    public class SpriteAtlas
    {
        private Dictionary<string, Sprite> _sprites;
        private Texture2D _texture;

        public SpriteAtlas(Texture2D texture)
        {
            _texture = texture;
            _sprites = new Dictionary<string, Sprite>();
        }

        public Sprite GetSprite(string name)
        {
            _sprites.TryGetValue(name, out var sprite);
            return sprite;
        }

        public void AddSprite(string name, Rect rect, Vector2 pivot)
        {
            var sprite = Sprite.Create(_texture, rect, pivot, 32);
            _sprites[name] = sprite;
        }
    }

    /// <summary>
    /// 渐变方向
    /// </summary>
    public enum GradientDirection
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// 图案类型
    /// </summary>
    public enum PatternType
    {
        Checker,
        VerticalStripes,
        HorizontalStripes,
        DiagonalStripes,
        Dots
    }
}
