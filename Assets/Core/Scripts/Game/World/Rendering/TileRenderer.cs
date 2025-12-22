/**
 * TileRenderer.cs
 * Tile 层总渲染器
 * 
 * 负责：
 * - 管理所有 ChunkRenderer
 * - 视野剔除，只渲染可见 Chunk
 * - ChunkRenderer 对象池
 * - 全局层可见性控制
 */

using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// Tile 层渲染器
    /// </summary>
    public class TileRenderer : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 当前地图
        /// </summary>
        private Map _map;
        
        /// <summary>
        /// 活跃的 ChunkRenderer（按坐标索引）
        /// </summary>
        private Dictionary<ChunkCoord, ChunkRenderer> _activeChunkRenderers;
        
        /// <summary>
        /// ChunkRenderer 对象池
        /// </summary>
        private Queue<ChunkRenderer> _chunkRendererPool;
        
        /// <summary>
        /// ChunkRenderer 预制体/容器
        /// </summary>
        private Transform _chunkContainer;
        
        /// <summary>
        /// 当前可见的 Chunk 集合
        /// </summary>
        private HashSet<ChunkCoord> _visibleChunks;
        
        /// <summary>
        /// 上一帧可见的 Chunk 集合（用于差异对比）
        /// </summary>
        private HashSet<ChunkCoord> _previousVisibleChunks;
        
        /// <summary>
        /// 跟踪的相机
        /// </summary>
        private Camera _mainCamera;
        
        /// <summary>
        /// 全局层可见性
        /// </summary>
        private bool[] _globalLayerVisibility;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        #endregion
        
        #region 属性
        
        public Map Map => _map;
        public int ActiveChunkCount => _activeChunkRenderers?.Count ?? 0;
        public int PooledChunkCount => _chunkRendererPool?.Count ?? 0;
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化渲染器
        /// </summary>
        public void Initialize(Map map, Camera mainCamera = null)
        {
            if (_isInitialized)
            {
                Cleanup();
            }
            
            _map = map;
            _mainCamera = mainCamera ?? Camera.main;
            
            // 初始化集合
            _activeChunkRenderers = new Dictionary<ChunkCoord, ChunkRenderer>();
            _chunkRendererPool = new Queue<ChunkRenderer>();
            _visibleChunks = new HashSet<ChunkCoord>();
            _previousVisibleChunks = new HashSet<ChunkCoord>();
            
            // 初始化层可见性
            _globalLayerVisibility = new bool[MapConstants.TILE_LAYER_COUNT];
            for (int i = 0; i < _globalLayerVisibility.Length; i++)
            {
                _globalLayerVisibility[i] = true;
            }
            
            // 创建 Chunk 容器
            CreateChunkContainer();
            
            // 预创建对象池
            PrewarmPool(RenderingConstants.CHUNK_RENDERER_POOL_SIZE);
            
            _isInitialized = true;
            
            Debug.Log($"[TileRenderer] 初始化完成: Map={map.MapId}");
        }
        
        /// <summary>
        /// 创建 Chunk 容器
        /// </summary>
        private void CreateChunkContainer()
        {
            if (_chunkContainer != null)
            {
                Destroy(_chunkContainer.gameObject);
            }
            
            GameObject container = new GameObject("ChunkContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            _chunkContainer = container.transform;
        }
        
        /// <summary>
        /// 预热对象池
        /// </summary>
        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ChunkRenderer renderer = CreateChunkRenderer();
                renderer.gameObject.SetActive(false);
                _chunkRendererPool.Enqueue(renderer);
            }
        }
        
        /// <summary>
        /// 创建新的 ChunkRenderer
        /// </summary>
        private ChunkRenderer CreateChunkRenderer()
        {
            GameObject go = new GameObject("ChunkRenderer");
            go.transform.SetParent(_chunkContainer);
            
            // 添加 Grid 组件（Tilemap 需要）
            Grid grid = go.AddComponent<Grid>();
            grid.cellSize = new Vector3(MapConstants.TILE_SIZE, MapConstants.TILE_SIZE, 0);
            
            ChunkRenderer renderer = go.AddComponent<ChunkRenderer>();
            renderer.Initialize(this);
            
            return renderer;
        }
        
        #endregion
        
        #region 对象池管理
        
        /// <summary>
        /// 从对象池获取 ChunkRenderer
        /// </summary>
        private ChunkRenderer GetChunkRenderer()
        {
            if (_chunkRendererPool.Count > 0)
            {
                return _chunkRendererPool.Dequeue();
            }
            
            // 池为空，创建新的
            return CreateChunkRenderer();
        }
        
        /// <summary>
        /// 归还 ChunkRenderer 到对象池
        /// </summary>
        private void ReturnChunkRenderer(ChunkRenderer renderer)
        {
            renderer.Deactivate();
            _chunkRendererPool.Enqueue(renderer);
        }
        
        #endregion
        
        #region 更新逻辑
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public void UpdateRendering()
        {
            if (!_isInitialized || _map == null || _mainCamera == null) return;
            
            // 计算当前可见的 Chunk
            UpdateVisibleChunks();
            
            // 激活新可见的 Chunk
            ActivateVisibleChunks();
            
            // 停用不再可见的 Chunk
            DeactivateInvisibleChunks();
            
            // 刷新脏 Chunk
            RefreshDirtyChunks();
            
            // 交换可见集合
            SwapVisibleSets();
        }
        
        /// <summary>
        /// 计算当前可见的 Chunk
        /// </summary>
        private void UpdateVisibleChunks()
        {
            _visibleChunks.Clear();
            
            // 获取相机视野范围
            float height = _mainCamera.orthographicSize * 2f;
            float width = height * _mainCamera.aspect;
            Vector2 center = _mainCamera.transform.position;
            
            // 扩展范围（预加载）
            float extendX = RenderingConstants.VIEWPORT_EXTEND_CHUNKS * 
                           MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE;
            float extendY = extendX;
            
            // 计算 Chunk 范围
            ChunkCoord[] chunks = MapCoordUtility.GetChunksInViewport(
                center, 
                width + extendX * 2, 
                height + extendY * 2
            );
            
            // 过滤有效 Chunk
            foreach (var coord in chunks)
            {
                if (_map.IsChunkCoordValid(coord))
                {
                    _visibleChunks.Add(coord);
                }
            }
        }
        
        /// <summary>
        /// 激活可见 Chunk
        /// </summary>
        private void ActivateVisibleChunks()
        {
            foreach (var coord in _visibleChunks)
            {
                if (!_activeChunkRenderers.ContainsKey(coord))
                {
                    Chunk chunk = _map.GetChunk(coord);
                    if (chunk != null)
                    {
                        ActivateChunk(chunk);
                    }
                }
            }
        }
        
        /// <summary>
        /// 激活单个 Chunk
        /// </summary>
        private void ActivateChunk(Chunk chunk)
        {
            ChunkRenderer renderer = GetChunkRenderer();
            renderer.Activate(chunk);
            
            // 应用全局层可见性
            ApplyGlobalVisibilityToRenderer(renderer);
            
            _activeChunkRenderers[chunk.Coord] = renderer;
        }
        
        /// <summary>
        /// 停用不可见 Chunk
        /// </summary>
        private void DeactivateInvisibleChunks()
        {
            // 找出不再可见的 Chunk
            List<ChunkCoord> toRemove = new List<ChunkCoord>();
            
            foreach (var kvp in _activeChunkRenderers)
            {
                if (!_visibleChunks.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            // 停用并回收
            foreach (var coord in toRemove)
            {
                if (_activeChunkRenderers.TryGetValue(coord, out var renderer))
                {
                    ReturnChunkRenderer(renderer);
                    _activeChunkRenderers.Remove(coord);
                }
            }
        }
        
        /// <summary>
        /// 刷新脏 Chunk
        /// </summary>
        private void RefreshDirtyChunks()
        {
            foreach (var kvp in _activeChunkRenderers)
            {
                kvp.Value.Refresh();
            }
        }
        
        /// <summary>
        /// 交换可见集合
        /// </summary>
        private void SwapVisibleSets()
        {
            var temp = _previousVisibleChunks;
            _previousVisibleChunks = _visibleChunks;
            _visibleChunks = temp;
            _visibleChunks.Clear();
        }
        
        #endregion
        
        #region 层可见性控制
        
        /// <summary>
        /// 设置全局层可见性
        /// </summary>
        public void SetGlobalLayerVisible(int layerIndex, bool visible)
        {
            if (layerIndex < 0 || layerIndex >= _globalLayerVisibility.Length) return;
            
            _globalLayerVisibility[layerIndex] = visible;
            
            // 应用到所有活跃的 ChunkRenderer
            foreach (var kvp in _activeChunkRenderers)
            {
                kvp.Value.SetLayerVisible(layerIndex, visible);
            }
        }
        
        /// <summary>
        /// 获取全局层可见性
        /// </summary>
        public bool IsGlobalLayerVisible(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= _globalLayerVisibility.Length) return false;
            return _globalLayerVisibility[layerIndex];
        }
        
        /// <summary>
        /// 设置屋顶全局可见性
        /// </summary>
        public void SetRoofVisible(bool visible)
        {
            SetGlobalLayerVisible(MapConstants.LAYER_ROOF, visible);
        }
        
        /// <summary>
        /// 应用全局可见性到渲染器
        /// </summary>
        private void ApplyGlobalVisibilityToRenderer(ChunkRenderer renderer)
        {
            for (int i = 0; i < _globalLayerVisibility.Length; i++)
            {
                renderer.SetLayerVisible(i, _globalLayerVisibility[i]);
            }
        }
        
        #endregion
        
        #region 手动刷新
        
        /// <summary>
        /// 强制刷新所有可见 Chunk
        /// </summary>
        public void ForceRefreshAll()
        {
            foreach (var kvp in _activeChunkRenderers)
            {
                kvp.Value.RebuildAllLayers();
            }
        }
        
        /// <summary>
        /// 刷新指定 Chunk
        /// </summary>
        public void RefreshChunk(ChunkCoord coord)
        {
            if (_activeChunkRenderers.TryGetValue(coord, out var renderer))
            {
                renderer.RebuildAllLayers();
            }
        }
        
        /// <summary>
        /// 更新指定 Tile
        /// </summary>
        public void UpdateTile(TileCoord tileCoord)
        {
            ChunkCoord chunkCoord = tileCoord.ToChunkCoord();
            
            if (_activeChunkRenderers.TryGetValue(chunkCoord, out var renderer))
            {
                LocalTileCoord local = tileCoord.ToLocalCoord();
                renderer.UpdateTile(local.x, local.y);
            }
        }
        
        #endregion
        
        #region 清理
        
        /// <summary>
        /// 清理所有渲染器
        /// </summary>
        public void Cleanup()
        {
            // 停用所有活跃的渲染器
            if (_activeChunkRenderers != null)
            {
                foreach (var kvp in _activeChunkRenderers)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value.gameObject);
                    }
                }
                _activeChunkRenderers.Clear();
            }
            
            // 清理对象池
            if (_chunkRendererPool != null)
            {
                while (_chunkRendererPool.Count > 0)
                {
                    var renderer = _chunkRendererPool.Dequeue();
                    if (renderer != null)
                    {
                        Destroy(renderer.gameObject);
                    }
                }
            }
            
            // 销毁容器
            if (_chunkContainer != null)
            {
                Destroy(_chunkContainer.gameObject);
                _chunkContainer = null;
            }
            
            _map = null;
            _isInitialized = false;
        }
        
        #endregion
        
        #region 生命周期
        
        void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
    }
}
