/**
 * Map.cs
 * 地图数据类
 * 
 * 混合系统中的 Map 协调两个子系统：
 * - Tile 系统：静态地形/建筑（通过 Chunk 管理）
 * - Entity 系统：动态对象（通过 EntityManager 管理）
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 地图数据类
    /// </summary>
    [Serializable]
    public class Map
    {
        #region 基础字段
        
        [SerializeField] private string _mapId;
        [SerializeField] private string _mapName;
        [SerializeField] private MapType _mapType;
        [SerializeField] private int _widthInChunks;
        [SerializeField] private int _heightInChunks;
        
        #endregion
        
        #region Tile 系统
        
        /// <summary>
        /// Chunk 数组（一维存储）
        /// </summary>
        [SerializeField]
        private Chunk[] _chunks;
        
        #endregion
        
        #region Entity 系统
        
        /// <summary>
        /// 实体管理器（不序列化，需要单独处理）
        /// </summary>
        [NonSerialized]
        private EntityManager _entityManager;
        
        #endregion
        
        #region 传送门和出生点
        
        [SerializeField] private List<Portal> _portals;
        [SerializeField] private List<SpawnPoint> _spawnPoints;
        
        #endregion
        
        #region 运行时状态
        
        [NonSerialized] private bool _isLoaded;
        [NonSerialized] private bool _isDirty;
        
        #endregion
        
        #region 属性
        
        public string MapId => _mapId;
        public string MapName => _mapName;
        public MapType MapType => _mapType;
        
        public int WidthInChunks => _widthInChunks;
        public int HeightInChunks => _heightInChunks;
        public int WidthInTiles => _widthInChunks * MapConstants.CHUNK_SIZE;
        public int HeightInTiles => _heightInChunks * MapConstants.CHUNK_SIZE;
        public int TotalChunks => _widthInChunks * _heightInChunks;
        
        public bool IsLoaded => _isLoaded;
        public bool IsDirty => _isDirty;
        
        /// <summary>
        /// 实体管理器
        /// </summary>
        public EntityManager Entities => _entityManager;
        
        /// <summary>
        /// 传送门列表
        /// </summary>
        public IReadOnlyList<Portal> Portals => _portals;
        
        /// <summary>
        /// 出生点列表
        /// </summary>
        public IReadOnlyList<SpawnPoint> SpawnPoints => _spawnPoints;
        
        /// <summary>
        /// 地图世界坐标边界
        /// </summary>
        public Rect WorldBounds => new Rect(0, 0, 
            WidthInTiles * MapConstants.TILE_SIZE,
            HeightInTiles * MapConstants.TILE_SIZE);
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数（序列化需要）
        /// </summary>
        public Map()
        {
            _portals = new List<Portal>();
            _spawnPoints = new List<SpawnPoint>();
        }
        
        /// <summary>
        /// 创建新地图
        /// </summary>
        public Map(string mapId, string mapName, int widthInChunks, int heightInChunks,
            MapType mapType = MapType.Town)
        {
            if (string.IsNullOrEmpty(mapId))
                throw new ArgumentException("Map ID cannot be null or empty");
            if (widthInChunks <= 0 || heightInChunks <= 0)
                throw new ArgumentException("Map dimensions must be positive");
            
            _mapId = mapId;
            _mapName = mapName;
            _mapType = mapType;
            _widthInChunks = widthInChunks;
            _heightInChunks = heightInChunks;
            
            // 初始化 Chunk 数组
            int totalChunks = widthInChunks * heightInChunks;
            _chunks = new Chunk[totalChunks];
            
            for (int y = 0; y < heightInChunks; y++)
            {
                for (int x = 0; x < widthInChunks; x++)
                {
                    int index = y * widthInChunks + x;
                    _chunks[index] = new Chunk(new ChunkCoord(x, y), mapId);
                }
            }
            
            // 初始化 Entity 管理器
            _entityManager = new EntityManager(mapId);
            
            _portals = new List<Portal>();
            _spawnPoints = new List<SpawnPoint>();
            _isLoaded = true;
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化 EntityManager（反序列化后调用）
        /// </summary>
        public void InitializeEntityManager()
        {
            if (_entityManager == null)
            {
                _entityManager = new EntityManager(_mapId);
            }
        }
        
        #endregion
        
        #region Chunk 访问
        
        /// <summary>
        /// 获取指定坐标的 Chunk
        /// </summary>
        public Chunk GetChunk(ChunkCoord coord)
        {
            if (!IsChunkCoordValid(coord)) return null;
            return _chunks[coord.ToIndex(_widthInChunks)];
        }
        
        /// <summary>
        /// 获取指定坐标的 Chunk
        /// </summary>
        public Chunk GetChunk(int chunkX, int chunkY)
        {
            return GetChunk(new ChunkCoord(chunkX, chunkY));
        }
        
        /// <summary>
        /// 通过 Tile 坐标获取所在的 Chunk
        /// </summary>
        public Chunk GetChunkAtTile(TileCoord tileCoord)
        {
            return GetChunk(tileCoord.ToChunkCoord());
        }
        
        /// <summary>
        /// 检查 Chunk 坐标是否有效
        /// </summary>
        public bool IsChunkCoordValid(ChunkCoord coord)
        {
            return coord.x >= 0 && coord.x < _widthInChunks
                && coord.y >= 0 && coord.y < _heightInChunks;
        }
        
        #endregion
        
        #region Tile 访问
        
        /// <summary>
        /// 获取指定坐标的瓦片数据
        /// </summary>
        public TileData GetTile(TileCoord coord)
        {
            if (!IsTileCoordValid(coord)) return TileData.Empty;
            
            Chunk chunk = GetChunkAtTile(coord);
            if (chunk == null) return TileData.Empty;
            
            LocalTileCoord local = coord.ToLocalCoord();
            return chunk.GetTile(local);
        }
        
        /// <summary>
        /// 获取指定坐标的瓦片数据
        /// </summary>
        public TileData GetTile(int tileX, int tileY)
        {
            return GetTile(new TileCoord(tileX, tileY));
        }
        
        /// <summary>
        /// 设置指定坐标的瓦片数据
        /// </summary>
        public bool SetTile(TileCoord coord, TileData tileData)
        {
            if (!IsTileCoordValid(coord)) return false;
            
            Chunk chunk = GetChunkAtTile(coord);
            if (chunk == null) return false;
            
            LocalTileCoord local = coord.ToLocalCoord();
            chunk.SetTile(local, tileData);
            _isDirty = true;
            
            return true;
        }
        
        /// <summary>
        /// 设置指定坐标的瓦片数据
        /// </summary>
        public bool SetTile(int tileX, int tileY, TileData tileData)
        {
            return SetTile(new TileCoord(tileX, tileY), tileData);
        }
        
        /// <summary>
        /// 检查 Tile 坐标是否有效
        /// </summary>
        public bool IsTileCoordValid(TileCoord coord)
        {
            return coord.x >= 0 && coord.x < WidthInTiles
                && coord.y >= 0 && coord.y < HeightInTiles;
        }
        
        #endregion
        
        #region Tile 层级访问
        
        /// <summary>
        /// 获取指定位置和层级的层数据
        /// </summary>
        public TileLayerData GetTileLayer(TileCoord coord, int layerIndex)
        {
            return GetTile(coord).GetLayer(layerIndex);
        }
        
        /// <summary>
        /// 设置指定位置和层级的层数据
        /// </summary>
        public bool SetTileLayer(TileCoord coord, int layerIndex, TileLayerData layerData)
        {
            if (!IsTileCoordValid(coord)) return false;
            
            Chunk chunk = GetChunkAtTile(coord);
            if (chunk == null) return false;
            
            LocalTileCoord local = coord.ToLocalCoord();
            chunk.SetTileLayer(local.x, local.y, layerIndex, layerData);
            _isDirty = true;
            
            return true;
        }
        
        #endregion
        
        #region 阻挡检查（综合 Tile + Entity）
        
        /// <summary>
        /// 检查指定位置是否阻挡移动（综合检查 Tile 和 Entity）
        /// </summary>
        public bool IsBlocking(TileCoord coord)
        {
            // 检查 Tile 阻挡
            TileData tile = GetTile(coord);
            if (tile.IsBlocking) return true;
            
            // 检查 Entity 阻挡
            if (_entityManager != null && _entityManager.HasBlockingEntityAt(coord))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 检查指定位置是否可行走
        /// </summary>
        public bool IsWalkable(TileCoord coord)
        {
            if (!IsTileCoordValid(coord)) return false;
            return !IsBlocking(coord);
        }
        
        /// <summary>
        /// 检查从一点到另一点是否可以直线行走
        /// </summary>
        public bool CanWalkDirectly(TileCoord from, TileCoord to)
        {
            // Bresenham 算法
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            int sx = from.x < to.x ? 1 : -1;
            int sy = from.y < to.y ? 1 : -1;
            int err = dx - dy;
            
            int x = from.x;
            int y = from.y;
            
            while (x != to.x || y != to.y)
            {
                if (IsBlocking(new TileCoord(x, y))) return false;
                
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
            
            return !IsBlocking(to);
        }
        
        #endregion
        
        #region 坐标转换
        
        /// <summary>
        /// 世界坐标转 Tile 坐标
        /// </summary>
        public TileCoord WorldToTile(Vector2 worldPos)
        {
            return MapCoordUtility.WorldToTile(worldPos);
        }
        
        /// <summary>
        /// Tile 坐标转世界坐标（中心）
        /// </summary>
        public Vector2 TileToWorld(TileCoord tileCoord)
        {
            return MapCoordUtility.TileToWorldCenter(tileCoord);
        }
        
        /// <summary>
        /// 检查世界坐标是否在地图范围内
        /// </summary>
        public bool IsWorldPosInBounds(Vector2 worldPos)
        {
            return WorldBounds.Contains(worldPos);
        }
        
        #endregion
        
        #region 批量操作
        
        /// <summary>
        /// 填充整个地图
        /// </summary>
        public void Fill(TileData tileData)
        {
            foreach (var chunk in _chunks)
            {
                chunk?.Fill(tileData);
            }
            _isDirty = true;
        }
        
        /// <summary>
        /// 填充指定层
        /// </summary>
        public void FillLayer(int layerIndex, TileLayerData layerData)
        {
            foreach (var chunk in _chunks)
            {
                chunk?.FillLayer(layerIndex, layerData);
            }
            _isDirty = true;
        }
        
        /// <summary>
        /// 清空整个地图
        /// </summary>
        public void Clear()
        {
            foreach (var chunk in _chunks)
            {
                chunk?.Clear();
            }
            _entityManager?.Clear();
            _isDirty = true;
        }
        
        #endregion
        
        #region 传送门管理
        
        public void AddPortal(Portal portal)
        {
            if (portal == null) throw new ArgumentNullException(nameof(portal));
            _portals.Add(portal);
            _isDirty = true;
        }
        
        public bool RemovePortal(string portalId)
        {
            int index = _portals.FindIndex(p => p.PortalId == portalId);
            if (index >= 0)
            {
                _portals.RemoveAt(index);
                _isDirty = true;
                return true;
            }
            return false;
        }
        
        public Portal GetPortal(string portalId)
        {
            return _portals.Find(p => p.PortalId == portalId);
        }
        
        public Portal GetPortalAtPosition(TileCoord coord)
        {
            return _portals.Find(p => p.ContainsPosition(coord));
        }
        
        #endregion
        
        #region 出生点管理
        
        public void AddSpawnPoint(SpawnPoint spawnPoint)
        {
            if (spawnPoint == null) throw new ArgumentNullException(nameof(spawnPoint));
            _spawnPoints.Add(spawnPoint);
            _isDirty = true;
        }
        
        public SpawnPoint GetDefaultSpawnPoint()
        {
            return _spawnPoints.Find(s => s.IsDefault)
                ?? (_spawnPoints.Count > 0 ? _spawnPoints[0] : null);
        }
        
        public SpawnPoint GetSpawnPoint(string spawnPointId)
        {
            return _spawnPoints.Find(s => s.SpawnPointId == spawnPointId);
        }
        
        #endregion
        
        #region 遍历
        
        /// <summary>
        /// 遍历所有 Chunk
        /// </summary>
        public void ForEachChunk(Action<ChunkCoord, Chunk> action)
        {
            for (int y = 0; y < _heightInChunks; y++)
            {
                for (int x = 0; x < _widthInChunks; x++)
                {
                    int index = y * _widthInChunks + x;
                    action(new ChunkCoord(x, y), _chunks[index]);
                }
            }
        }
        
        /// <summary>
        /// 遍历视野范围内的 Chunk
        /// </summary>
        public void ForEachChunkInViewport(Vector2 center, float viewWidth, float viewHeight,
            Action<ChunkCoord, Chunk> action)
        {
            ChunkCoord[] visibleChunks = MapCoordUtility.GetChunksInViewport(center, viewWidth, viewHeight);
            
            foreach (var chunkCoord in visibleChunks)
            {
                Chunk chunk = GetChunk(chunkCoord);
                if (chunk != null)
                {
                    action(chunkCoord, chunk);
                }
            }
        }
        
        #endregion
        
        #region 状态管理
        
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        public void ClearDirty()
        {
            _isDirty = false;
            foreach (var chunk in _chunks)
            {
                chunk?.ClearDirty();
            }
            _entityManager?.ClearAllDirty();
        }
        
        public void MarkLoaded()
        {
            _isLoaded = true;
            foreach (var chunk in _chunks)
            {
                chunk?.MarkLoaded();
            }
        }
        
        public void MarkUnloaded()
        {
            _isLoaded = false;
            foreach (var chunk in _chunks)
            {
                chunk?.MarkUnloaded();
            }
        }
        
        /// <summary>
        /// 获取所有脏的 Chunk
        /// </summary>
        public List<Chunk> GetDirtyChunks()
        {
            var result = new List<Chunk>();
            foreach (var chunk in _chunks)
            {
                if (chunk != null && chunk.IsDirty)
                {
                    result.Add(chunk);
                }
            }
            return result;
        }
        
        #endregion
        
        #region 更新
        
        /// <summary>
        /// 每帧更新（更新 Entity 等）
        /// </summary>
        public void Update(float deltaTime)
        {
            _entityManager?.Update(deltaTime);
        }
        
        #endregion
        
        public override string ToString()
        {
            int entityCount = _entityManager?.EntityCount ?? 0;
            return $"Map({_mapId}, {_mapName}, {_widthInChunks}x{_heightInChunks} chunks, " +
                   $"Entities:{entityCount}, Portals:{_portals.Count})";
        }
    }
}
