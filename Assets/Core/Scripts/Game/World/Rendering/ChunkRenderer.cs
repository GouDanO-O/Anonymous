/**
 * ChunkRenderer.cs
 * 单个 Chunk 的渲染器
 * 
 * 负责：
 * - 渲染 Chunk 内所有 Tile 的所有层
 * - 每层使用独立的 Tilemap 或 SpriteRenderer 组
 * - 脏标记检测，按需重建渲染
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
        
        #endregion
        
        #region 属性
        
        public ChunkCoord ChunkCoord => _chunkCoord;
        public bool IsActive => _isActive;
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化渲染器
        /// </summary>
        public void Initialize(TileRenderer tileRenderer)
        {
            if (_isInitialized) return;
            
            _tileRenderer = tileRenderer;
            _layerVisibility = new bool[MapConstants.TILE_LAYER_COUNT];
            
            // 创建各层的 Tilemap
            CreateLayerTilemaps();
            
            // 默认所有层可见
            for (int i = 0; i < _layerVisibility.Length; i++)
            {
                _layerVisibility[i] = true;
            }
            
            _isInitialized = true;
            _isActive = false;
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
                GameObject layerGo = new GameObject($"Layer_{config.LayerName}");
                layerGo.transform.SetParent(transform);
                layerGo.transform.localPosition = Vector3.zero;
                
                // 添加 Tilemap 组件
                Tilemap tilemap = layerGo.AddComponent<Tilemap>();
                TilemapRenderer renderer = layerGo.AddComponent<TilemapRenderer>();
                
                // 设置排序
                renderer.sortingLayerName = config.SortingLayerName;
                renderer.sortingOrder = config.BaseSortingOrder;
                
                _tilemaps[i] = tilemap;
                _tilemapRenderers[i] = renderer;
            }
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
                    
                    // 创建 Tile 并设置
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    UnityEngine.Tilemaps.Tile tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                    tile.sprite = sprite;
                    
                    tilemap.SetTile(cellPos, tile);
                }
            }
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
                        UnityEngine.Tilemaps.Tile tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                        tile.sprite = sprite;
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
        }
        
        #endregion
    }
}
