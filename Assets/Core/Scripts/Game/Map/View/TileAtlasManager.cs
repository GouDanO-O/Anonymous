using System.Collections.Generic;
using Core.Game.Config;
using Core.Game.Map.Define;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// Tile Atlas 管理器
    /// 从 Resources/Tiles/ 文件夹加载贴图, 按渲染层打包为 Atlas, 提供 UV 查询
    /// 无贴图时返回白像素 FallbackUV, 保持原顶点色显示效果
    /// </summary>
    public static class TileAtlasManager
    {
        // 每层独立 Material
        public static Material TerrainMaterial { get; private set; }
        public static Material FloorMaterial { get; private set; }
        public static Material WallMaterial { get; private set; }
        public static Material RoofMaterial { get; private set; }
        public static Material ObjectMaterial { get; private set; }
        public static Material BlueprintMaterial { get; private set; }

        // 白像素 UV (无贴图时的 fallback)
        public static Rect FallbackUV { get; private set; }

        public static bool IsInitialized { get; private set; }

        // Rimworld Atlas 布局重映射: bitmask(N=1,E=2,S=4,W=8) → Atlas位置
        // Atlas布局: [0]无 [1]E [2]ES [3]S [4]ESW [5]EW [6]SW [7]W
        //            [8]NE [9]N [10]NEW [11]NW [12]NESW [13]NES [14]NSW [15]NS
        private static readonly int[] BitmaskToAtlasIndex =
        {
            0, 9, 1, 8, 3, 15, 2, 13, 7, 11, 5, 10, 6, 14, 4, 12
        };

        // UV 查询表
        private static Dictionary<int, Rect> _terrainUVs = new();
        private static Dictionary<int, Rect> _floorUVs = new();
        private static Dictionary<int, Rect[]> _structureUVs = new(); // Wall: Rect[16], 其他: Rect[1]
        private static Rect _roofUV;
        private static Dictionary<int, Rect> _itemUVs = new();

        // 各层 fallback (每张 atlas 自己的白像素区域)
        private static Rect _terrainFallback;
        private static Rect _floorFallback;
        private static Rect _wallFallback;
        private static Rect _roofFallback;
        private static Rect _objectFallback;
        private static Rect _blueprintFallback;

        /// <summary>
        /// 初始化: 加载所有贴图, 打包 Atlas, 创建 Material
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized) return;

            var shader = Shader.Find("Sprites/Default");

            // 构建各层 Atlas
            TerrainMaterial = BuildLayerAtlas("Terrain", shader, LoadTerrainTextures(), out _terrainUVs, out _terrainFallback);
            FloorMaterial = BuildLayerAtlas("Floor", shader, LoadFloorTextures(), out _floorUVs, out _floorFallback);
            ObjectMaterial = BuildLayerAtlas("Object", shader, LoadItemTextures(), out _itemUVs, out _objectFallback);
            RoofMaterial = BuildRoofAtlas(shader, out _roofUV, out _roofFallback);
            WallMaterial = BuildStructureAtlas(shader);

            // Blueprint 层: 纯白像素材质 (只用顶点色)
            BlueprintMaterial = CreateWhitePixelMaterial(shader, out _blueprintFallback);

            // 全局 FallbackUV 使用 blueprint 层的 (任意白像素区域都行)
            FallbackUV = _blueprintFallback;

            IsInitialized = true;
            LogKit.Log("TileAtlasManager: 初始化完成");
        }

        #region UV 查询 API

        public static Rect GetTerrainUV(int terrainDefId)
        {
            return _terrainUVs.GetValueOrDefault(terrainDefId, _terrainFallback);
        }

        public static Rect GetFloorUV(int floorDefId)
        {
            return _floorUVs.GetValueOrDefault(floorDefId, _floorFallback);
        }

        /// <summary>
        /// 获取结构 UV
        /// Wall 类型传入 autotileBitmask (0-15), 其他类型传 0
        /// </summary>
        public static Rect GetStructureUV(int structureDefId, int autotileBitmask = 0)
        {
            if (!_structureUVs.TryGetValue(structureDefId, out var rects))
                return _wallFallback;

            if (rects.Length == 1)
                return rects[0]; // 非 Wall 类型, 单帧

            // Wall autotile: bitmask → Rimworld Atlas 位置重映射
            int atlasIdx = BitmaskToAtlasIndex[Mathf.Clamp(autotileBitmask, 0, 15)];
            return rects[atlasIdx];
        }

        public static Rect GetRoofUV()
        {
            return _roofUV;
        }

        public static Rect GetItemUV(int itemDefId)
        {
            return _itemUVs.GetValueOrDefault(itemDefId, _objectFallback);
        }

        #endregion

        #region Atlas 构建

        /// <summary>
        /// 通用层 Atlas 构建: 加载贴图列表 → PackTextures → 生成 Material + UV 映射
        /// </summary>
        private static Material BuildLayerAtlas(string layerName, Shader shader,
            List<(int defId, Texture2D tex)> textures,
            out Dictionary<int, Rect> uvMap, out Rect fallbackUV)
        {
            uvMap = new Dictionary<int, Rect>();

            // 创建白像素纹理作为 fallback
            var whiteTex = CreateWhitePixelTexture();
            var texList = new List<Texture2D> { whiteTex };
            var idList = new List<int> { -1 }; // -1 = fallback

            foreach (var (defId, tex) in textures)
            {
                texList.Add(tex);
                idList.Add(defId);
            }

            // 如果没有任何贴图, 直接返回白像素材质
            if (textures.Count == 0)
            {
                fallbackUV = new Rect(0, 0, 1, 1);
                var mat = new Material(shader);
                mat.mainTexture = whiteTex;
                return mat;
            }

            // 打包 Atlas
            var atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point; // 像素风格
            var rects = atlas.PackTextures(texList.ToArray(), 1, 2048);

            // 记录 UV 映射
            for (int i = 0; i < idList.Count; i++)
            {
                if (idList[i] == -1)
                    continue; // fallback 单独处理
                uvMap[idList[i]] = rects[i];
            }

            fallbackUV = rects[0]; // 第一个是白像素

            var material = new Material(shader);
            material.mainTexture = atlas;

            LogKit.Log($"TileAtlasManager: {layerName} Atlas 打包完成, " +
                       $"{textures.Count} 贴图, Atlas 尺寸 {atlas.width}x{atlas.height}");

            return material;
        }

        /// <summary>
        /// Structure 层特殊处理: Wall 类型的 autotile sheet 需要拆分为 16 子贴图
        /// </summary>
        private static Material BuildStructureAtlas(Shader shader)
        {
            _structureUVs = new Dictionary<int, Rect[]>();

            var whiteTex = CreateWhitePixelTexture();
            var texList = new List<Texture2D> { whiteTex };
            // (defId, subIndex) 列表, subIndex=-1 表示单帧, 0-15 表示 autotile 变体
            var indexList = new List<(int defId, int subIndex)> { (-1, -1) };

            foreach (int defId in ConfigManager.GetAllStructureDefIds())
            {
                var def = ConfigManager.GetStructureDef(defId);
                var tex = Resources.Load<Texture2D>($"Tiles/Structure/{def.TexturePath}");
                if (tex == null) continue;

                if ((EStructureType)def.StructureType == EStructureType.Wall)
                {
                    // Autotile sheet: 拆成 16 个子贴图 (4x4 grid)
                    int tileSize = tex.width / 4;
                    for (int i = 0; i < 16; i++)
                    {
                        int col = i % 4;
                        int row = i / 4;
                        // Unity Texture2D 坐标: 左下角为原点
                        // sheet 行从上到下: row 0 在最上面 → y = height - (row+1)*tileSize
                        int px = col * tileSize;
                        int py = tex.height - (row + 1) * tileSize;

                        var subTex = new Texture2D(tileSize, tileSize, TextureFormat.RGBA32, false);
                        subTex.filterMode = FilterMode.Point;
                        subTex.SetPixels(tex.GetPixels(px, py, tileSize, tileSize));
                        subTex.Apply();

                        texList.Add(subTex);
                        indexList.Add((defId, i));
                    }
                }
                else
                {
                    // 非 Wall: 单帧
                    texList.Add(tex);
                    indexList.Add((defId, -1));
                }
            }

            // 没有任何结构贴图
            if (texList.Count == 1)
            {
                _wallFallback = new Rect(0, 0, 1, 1);
                var mat = new Material(shader);
                mat.mainTexture = whiteTex;
                return mat;
            }

            var atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;
            var rects = atlas.PackTextures(texList.ToArray(), 1, 4096);

            _wallFallback = rects[0]; // 白像素

            // 构建 UV 映射
            // 先收集每个 defId 的 autotile 变体 rects
            var wallRectTemp = new Dictionary<int, Rect[]>();

            for (int i = 1; i < indexList.Count; i++)
            {
                var (defId, subIndex) = indexList[i];

                if (subIndex == -1)
                {
                    // 单帧
                    _structureUVs[defId] = new Rect[] { rects[i] };
                }
                else
                {
                    // Autotile 变体
                    if (!wallRectTemp.ContainsKey(defId))
                        wallRectTemp[defId] = new Rect[16];
                    wallRectTemp[defId][subIndex] = rects[i];
                }
            }

            foreach (var kvp in wallRectTemp)
            {
                _structureUVs[kvp.Key] = kvp.Value;
            }

            var material = new Material(shader);
            material.mainTexture = atlas;

            LogKit.Log($"TileAtlasManager: Structure Atlas 打包完成, Atlas 尺寸 {atlas.width}x{atlas.height}");

            return material;
        }

        /// <summary>
        /// Roof 层: 单张贴图
        /// </summary>
        private static Material BuildRoofAtlas(Shader shader, out Rect roofUV, out Rect fallbackUV)
        {
            var whiteTex = CreateWhitePixelTexture();
            var roofTex = Resources.Load<Texture2D>("Tiles/Roof/Roof");

            if (roofTex == null)
            {
                roofUV = new Rect(0, 0, 1, 1);
                fallbackUV = roofUV;
                var mat = new Material(shader);
                mat.mainTexture = whiteTex;
                return mat;
            }

            var atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point;
            var rects = atlas.PackTextures(new[] { whiteTex, roofTex }, 1, 512);

            fallbackUV = rects[0];
            roofUV = rects[1];

            var material = new Material(shader);
            material.mainTexture = atlas;

            return material;
        }

        /// <summary>
        /// 创建纯白像素材质 (用于 Blueprint 层)
        /// </summary>
        private static Material CreateWhitePixelMaterial(Shader shader, out Rect fallbackUV)
        {
            var whiteTex = CreateWhitePixelTexture();
            fallbackUV = new Rect(0, 0, 1, 1);

            var mat = new Material(shader);
            mat.mainTexture = whiteTex;
            return mat;
        }

        #endregion

        #region 贴图加载

        private static List<(int defId, Texture2D tex)> LoadTerrainTextures()
        {
            var result = new List<(int, Texture2D)>();
            foreach (int defId in ConfigManager.GetAllTerrainDefIds())
            {
                var def = ConfigManager.GetTerrainDef(defId);
                var tex = Resources.Load<Texture2D>($"Tiles/Terrain/{def.TexturePath}");
                if (tex != null)
                    result.Add((defId, tex));
            }
            return result;
        }

        private static List<(int defId, Texture2D tex)> LoadFloorTextures()
        {
            var result = new List<(int, Texture2D)>();
            foreach (int defId in ConfigManager.GetAllFloorDefIds())
            {
                var def = ConfigManager.GetFloorDef(defId);
                var tex = Resources.Load<Texture2D>($"Tiles/Floor/{def.TexturePath}");
                if (tex != null)
                    result.Add((defId, tex));
            }
            return result;
        }

        private static List<(int defId, Texture2D tex)> LoadItemTextures()
        {
            var result = new List<(int, Texture2D)>();
            foreach (int defId in ConfigManager.GetAllItemDefIds())
            {
                var def = ConfigManager.GetItemDef(defId);
                var tex = Resources.Load<Texture2D>($"Tiles/Item/{def.TexturePath}");
                if (tex != null)
                    result.Add((defId, tex));
            }
            return result;
        }

        #endregion

        #region 工具

        private static Texture2D CreateWhitePixelTexture()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        #endregion
    }
}
