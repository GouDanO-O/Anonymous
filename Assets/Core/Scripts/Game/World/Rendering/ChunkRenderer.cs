/**
 * ChunkRenderer.cs
 * 单个 Chunk 的渲染器
 * 
 * 支持 URP (Universal Render Pipeline) 2D Renderer
 */

using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// Chunk 渲染器
    /// </summary>
    public class ChunkRenderer : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 当前渲染的 Chunk 坐标
        /// </summary>
        private ChunkCoord _chunkCoord;
        
        /// <summary>
        /// 当前渲染的 Chunk 数据引用
        /// </summary>
        private Chunk _chunk;
        
        /// <summary>
        /// 所属的 TileRenderer
        /// </summary>
        private TileRenderer _tileRenderer;
        
        /// <summary>
        /// 各层的 Tilemap 组件
        /// </summary>
        private Tilemap[] _tilemaps;
        
        /// <summary>
        /// 各层的 TilemapRenderer
        /// </summary>
        private TilemapRenderer[] _tilemapRenderers;
        
        /// <summary>
        /// Grid 组件
        /// </summary>
        private Grid _grid;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 是否激活
        /// </summary>
        private bool _isActive;
        
        /// <summary>
        /// 各层可见性
        /// </summary>
        private bool[] _layerVisibility;
        
        /// <summary>
        /// 是否使用光照
        /// </summary>
        private bool _useLighting = true;
        
        /// <summary>
        /// Tile 缓存（避免重复创建）
        /// </summary>
        private UnityEngine.Tilemaps.Tile[] _tileCache;
        
        #endregion
        
        #region 属性
        
        public ChunkCoord ChunkCoord => _chunkCoord;
        public bool IsActive => _isActive;
        
        /// <summary>
        /// 是否使用 2D 光照
        /// </summary>
        public bool UseLighting
        {
            get => _useLighting;
            set
            {
                _useLighting = value;
                ApplyMaterials();
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化渲染器
        /// </summary>
        public void Initialize(TileRenderer tileRenderer, bool useLighting = true)
        {
            if (_isInitialized) return;
            
            _tileRenderer = tileRenderer;
            _useLighting = useLighting;
            _layerVisibility = new bool[MapConstants.TILE_LAYER_COUNT];
            
            // 设置/获取 Grid 组件
            SetupGrid();
            
            // 创建各层的 Tilemap
            CreateLayerTilemaps();
            
            // 应用 URP 材质
            ApplyMaterials();
            
            // 默认所有层可见
            for (int i = 0; i < _layerVisibility.Length; i++)
            {
                _layerVisibility[i] = true;
            }
            
            // 初始化 Tile 缓存
            _tileCache = new UnityEngine.Tilemaps.Tile[256]; // 最多缓存 256 种 Tile
            
            _isInitialized = true;
            _isActive = false;
        }
        
        /// <summary>
        /// 设置 Grid 组件
        /// </summary>
        private void SetupGrid()
        {
            _grid = GetComponent<Grid>();
            if (_grid == null)
            {
                _grid = gameObject.AddComponent<Grid>();
            }
            
            // 设置格子大小
            _grid.cellSize = new Vector3(MapConstants.TILE_SIZE, MapConstants.TILE_SIZE, 0);
            _grid.cellGap = Vector3.zero;
            _grid.cellLayout = GridLayout.CellLayout.Rectangle;
            _grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
        }
        
        /// <summary>
        /// 创建各层的 Tilemap
        /// </summary>
        private void CreateLayerTilemaps()
        {
            var layerConfigs = RenderingConstants.GetDefaultLayerConfigs();
            _tilemaps = new Tilemap[MapConstants.TILE_LAYER_COUNT];
            _tilemapRenderers = new TilemapRenderer[MapConstants.TILE_LAYER_COUNT];
            
            for (int i = 0; i < MapConstants.TILE_LAYER_COUNT; i++)
            {
                var config = layerConfigs[i];
                
                // 创建层 GameObject
                GameObject layerGo = new GameObject($"Layer_{i}_{config.LayerName}");
                layerGo.transform.SetParent(transform);
                layerGo.transform.localPosition = Vector3.zero;
                layerGo.transform.localRotation = Quaternion.identity;
                layerGo.transform.localScale = Vector3.one;
                
                // 添加 Tilemap 组件
                Tilemap tilemap = layerGo.AddComponent<Tilemap>();
                tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0); // 中心锚点
                
                // 添加 TilemapRenderer 组件
                TilemapRenderer renderer = layerGo.AddComponent<TilemapRenderer>();
                
                // 设置排序
                renderer.sortingLayerName = config.SortingLayerName;
                renderer.sortingOrder = config.BaseSortingOrder;
                
                // URP 特定设置
                renderer.mode = TilemapRenderer.Mode.Chunk; // 使用 Chunk 模式提升性能
                
                _tilemaps[i] = tilemap;
                _tilemapRenderers[i] = renderer;
            }
        }
        
        /// <summary>
        /// 应用 URP 材质
        /// </summary>
        private void ApplyMaterials()
        {
            if (_tilemapRenderers == null) return;
            
            URPMaterialHelper.SetupTilemapRenderers(_tilemapRenderers, _useLighting);
        }
        
        #endregion
        
        #region 激活/停用
        
        /// <summary>
        /// 激活并绑定到指定 Chunk
        /// </summary>
        public void Activate(Chunk chunk)
        {
            if (chunk == null)
            {
                Debug.LogError("[ChunkRenderer] Chunk cannot be null");
                return;
            }
            
            _chunk = chunk;
            _chunkCoord = chunk.Coord;
            
            // 设置位置
            Vector2 worldPos = MapCoordUtility.ChunkToWorld(_chunkCoord);
            transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            
            // 重建渲染
            RebuildAllLayers();
            
            gameObject.SetActive(true);
            _isActive = true;
        }
        
        /// <summary>
        /// 停用并释放
        /// </summary>
        public void Deactivate()
        {
            _chunk = null;
            _isActive = false;
            
            // 清空所有 Tilemap
            ClearAllTilemaps();
            
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 清空所有 Tilemap
        /// </summary>
        private void ClearAllTilemaps()
        {
            if (_tilemaps == null) return;
            
            foreach (var tilemap in _tilemaps)
            {
                if (tilemap != null)
                {
                    tilemap.ClearAllTiles();
                }
            }
        }
        
        #endregion
        
        #region 渲染重建
        
        /// <summary>
        /// 重建所有层的渲染
        /// </summary>
        public void RebuildAllLayers()
        {
            if (_chunk == null) return;
            
            for (int layer = 0; layer < MapConstants.TILE_LAYER_COUNT; layer++)
            {
                RebuildLayer(layer);
            }
            
            _chunk.ClearRenderRebuildFlag();
        }
        
        /// <summary>
        /// 重建指定层的渲染
        /// </summary>
        public void RebuildLayer(int layerIndex)
        {
            if (_chunk == null || _tilemaps == null) return;
            if (layerIndex < 0 || layerIndex >= _tilemaps.Length) return;
            
            Tilemap tilemap = _tilemaps[layerIndex];
            if (tilemap == null) return;
            
            // 清空当前层
            tilemap.ClearAllTiles();
            
            // 准备批量设置
            var positions = new Vector3Int[MapConstants.TILES_PER_CHUNK];
            var tiles = new UnityEngine.Tilemaps.TileBase[MapConstants.TILES_PER_CHUNK];
            int tileCount = 0;
            
            // 遍历 Chunk 内所有 Tile
            for (int y = 0; y < MapConstants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < MapConstants.CHUNK_SIZE; x++)
                {
                    TileData tileData = _chunk.GetTile(x, y);
                    TileLayerData layerData = tileData.GetLayer(layerIndex);
                    
                    if (layerData.IsEmpty) continue;
                    
                    // 获取精灵
                    Sprite sprite = SpriteManager.Instance.GetTileSprite(
                        layerData.tileId, 
                        layerData.SpriteVariant
                    );
                    
                    if (sprite == null) continue;
                    
                    // 获取或创建 Tile
                    UnityEngine.Tilemaps.Tile tile = GetOrCreateTile(layerData.tileId, sprite);
                    
                    positions[tileCount] = new Vector3Int(x, y, 0);
                    tiles[tileCount] = tile;
                    tileCount++;
                }
            }
            
            // 批量设置 Tiles（提升性能）
            if (tileCount > 0)
            {
                // 创建正确大小的数组
                var finalPositions = new Vector3Int[tileCount];
                var finalTiles = new UnityEngine.Tilemaps.TileBase[tileCount];
                Array.Copy(positions, finalPositions, tileCount);
                Array.Copy(tiles, finalTiles, tileCount);
                
                tilemap.SetTiles(finalPositions, finalTiles);
            }
        }
        
        /// <summary>
        /// 获取或创建 Tile 对象
        /// </summary>
        private UnityEngine.Tilemaps.Tile GetOrCreateTile(ushort tileId, Sprite sprite)
        {
            // 检查缓存
            if (tileId < _tileCache.Length && _tileCache[tileId] != null)
            {
                var cached = _tileCache[tileId];
                if (cached.sprite == sprite)
                {
                    return cached;
                }
            }
            
            // 创建新 Tile
            var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
            
            // 缓存
            if (tileId < _tileCache.Length)
            {
                _tileCache[tileId] = tile;
            }
            
            return tile;
        }
        
        /// <summary>
        /// 刷新（如果需要重建）
        /// </summary>
        public void Refresh()
        {
            if (_chunk != null && _chunk.NeedsRenderRebuild)
            {
                RebuildAllLayers();
            }
        }
        
        /// <summary>
        /// 更新单个 Tile 的渲染
        /// </summary>
        public void UpdateTile(int localX, int localY)
        {
            if (_chunk == null || _tilemaps == null) return;
            
            TileData tileData = _chunk.GetTile(localX, localY);
            Vector3Int cellPos = new Vector3Int(localX, localY, 0);
            
            for (int layer = 0; layer < MapConstants.TILE_LAYER_COUNT; layer++)
            {
                Tilemap tilemap = _tilemaps[layer];
                if (tilemap == null) continue;
                
                TileLayerData layerData = tileData.GetLayer(layer);
                
                if (layerData.IsEmpty)
                {
                    tilemap.SetTile(cellPos, null);
                }
                else
                {
                    Sprite sprite = SpriteManager.Instance.GetTileSprite(
                        layerData.tileId,
                        layerData.SpriteVariant
                    );
                    
                    if (sprite != null)
                    {
                        var tile = GetOrCreateTile(layerData.tileId, sprite);
                        tilemap.SetTile(cellPos, tile);
                    }
                }
            }
        }
        
        #endregion
        
        #region 层级可见性
        
        /// <summary>
        /// 设置层可见性
        /// </summary>
        public void SetLayerVisible(int layerIndex, bool visible)
        {
            if (layerIndex < 0 || layerIndex >= _tilemapRenderers.Length) return;
            
            _layerVisibility[layerIndex] = visible;
            
            if (_tilemapRenderers[layerIndex] != null)
            {
                _tilemapRenderers[layerIndex].enabled = visible;
            }
        }
        
        /// <summary>
        /// 获取层可见性
        /// </summary>
        public bool IsLayerVisible(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= _layerVisibility.Length) return false;
            return _layerVisibility[layerIndex];
        }
        
        /// <summary>
        /// 设置屋顶可见性
        /// </summary>
        public void SetRoofVisible(bool visible)
        {
            SetLayerVisible(MapConstants.LAYER_ROOF, visible);
        }
        
        #endregion
        
        #region 生命周期
        
        void OnDestroy()
        {
            ClearAllTilemaps();
            
            // 清理 Tile 缓存
            if (_tileCache != null)
            {
                foreach (var tile in _tileCache)
                {
                    if (tile != null)
                    {
                        DestroyImmediate(tile);
                    }
                }
                _tileCache = null;
            }
        }
        
        #endregion
    }
}
