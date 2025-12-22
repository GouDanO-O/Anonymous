/**
 * Chunk.cs
 * 区块数据类
 * 
 * 混合系统中的 Chunk 只负责管理静态 Tile 数据
 * Entity 由独立的 EntityManager 管理
 * 
 * 每个 Chunk 包含 16×16 = 256 个 TileData
 * 每个 TileData 24 字节，整个 Chunk 约 6KB
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 区块数据类
    /// </summary>
    [Serializable]
    public class Chunk
    {
        #region 字段
        
        /// <summary>
        /// 区块坐标
        /// </summary>
        [SerializeField]
        private ChunkCoord _coord;
        
        /// <summary>
        /// 所属地图ID
        /// </summary>
        [SerializeField]
        private string _mapId;
        
        /// <summary>
        /// 瓦片数据数组（一维，256个元素）
        /// </summary>
        [SerializeField]
        private TileData[] _tiles;
        
        /// <summary>
        /// 脏标记
        /// </summary>
        [NonSerialized]
        private bool _isDirty;
        
        /// <summary>
        /// 渲染数据需要重建标记
        /// </summary>
        [NonSerialized]
        private bool _needsRenderRebuild;
        
        /// <summary>
        /// 是否已加载
        /// </summary>
        [NonSerialized]
        private bool _isLoaded;
        
        #endregion
        
        #region 属性
        
        public ChunkCoord Coord => _coord;
        public string MapId => _mapId;
        public bool IsDirty => _isDirty;
        public bool NeedsRenderRebuild => _needsRenderRebuild;
        public bool IsLoaded => _isLoaded;
        
        /// <summary>
        /// 该 Chunk 左下角的 Tile 坐标
        /// </summary>
        public TileCoord OriginTileCoord => _coord.ToTileCoord();
        
        /// <summary>
        /// 该 Chunk 的世界坐标边界
        /// </summary>
        public Rect WorldBounds => MapCoordUtility.GetChunkWorldBounds(_coord);
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数（序列化需要）
        /// </summary>
        public Chunk()
        {
            _tiles = new TileData[MapConstants.TILES_PER_CHUNK];
        }
        
        /// <summary>
        /// 创建新的区块
        /// </summary>
        public Chunk(ChunkCoord coord, string mapId)
        {
            _coord = coord;
            _mapId = mapId;
            _tiles = new TileData[MapConstants.TILES_PER_CHUNK];
            _isLoaded = true;
            _needsRenderRebuild = true;
        }
        
        /// <summary>
        /// 从已有数据创建区块
        /// </summary>
        public Chunk(ChunkCoord coord, string mapId, TileData[] tiles)
        {
            _coord = coord;
            _mapId = mapId;
            
            if (tiles == null || tiles.Length != MapConstants.TILES_PER_CHUNK)
            {
                throw new ArgumentException(
                    $"Tiles array must have exactly {MapConstants.TILES_PER_CHUNK} elements");
            }
            
            _tiles = tiles;
            _isLoaded = true;
            _needsRenderRebuild = true;
        }
        
        #endregion
        
        #region Tile 访问 - 局部坐标
        
        /// <summary>
        /// 获取指定局部坐标的 Tile 数据
        /// </summary>
        public TileData GetTile(int localX, int localY)
        {
            int index = LocalToIndex(localX, localY);
            return _tiles[index];
        }
        
        /// <summary>
        /// 获取指定局部坐标的 Tile 数据
        /// </summary>
        public TileData GetTile(LocalTileCoord localCoord)
        {
            return GetTile(localCoord.x, localCoord.y);
        }
        
        /// <summary>
        /// 设置指定局部坐标的 Tile 数据
        /// </summary>
        public void SetTile(int localX, int localY, TileData tileData)
        {
            int index = LocalToIndex(localX, localY);
            _tiles[index] = tileData;
            MarkDirty();
        }
        
        /// <summary>
        /// 设置指定局部坐标的 Tile 数据
        /// </summary>
        public void SetTile(LocalTileCoord localCoord, TileData tileData)
        {
            SetTile(localCoord.x, localCoord.y, tileData);
        }
        
        #endregion
        
        #region Tile 层级访问
        
        /// <summary>
        /// 获取指定位置和层级的层数据
        /// </summary>
        public TileLayerData GetTileLayer(int localX, int localY, int layerIndex)
        {
            return GetTile(localX, localY).GetLayer(layerIndex);
        }
        
        /// <summary>
        /// 设置指定位置和层级的层数据
        /// </summary>
        public void SetTileLayer(int localX, int localY, int layerIndex, TileLayerData layerData)
        {
            int index = LocalToIndex(localX, localY);
            TileData tile = _tiles[index];
            tile.SetLayer(layerIndex, layerData);
            _tiles[index] = tile;
            MarkDirty();
        }
        
        /// <summary>
        /// 清空指定位置的特定层
        /// </summary>
        public void ClearTileLayer(int localX, int localY, int layerIndex)
        {
            SetTileLayer(localX, localY, layerIndex, TileLayerData.Empty);
        }
        
        #endregion
        
        #region 批量操作
        
        /// <summary>
        /// 填充整个区块
        /// </summary>
        public void Fill(TileData tileData)
        {
            for (int i = 0; i < MapConstants.TILES_PER_CHUNK; i++)
            {
                _tiles[i] = tileData;
            }
            MarkDirty();
        }
        
        /// <summary>
        /// 填充指定层
        /// </summary>
        public void FillLayer(int layerIndex, TileLayerData layerData)
        {
            for (int i = 0; i < MapConstants.TILES_PER_CHUNK; i++)
            {
                TileData tile = _tiles[i];
                tile.SetLayer(layerIndex, layerData);
                _tiles[i] = tile;
            }
            MarkDirty();
        }
        
        /// <summary>
        /// 清空整个区块
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < MapConstants.TILES_PER_CHUNK; i++)
            {
                _tiles[i] = TileData.Empty;
            }
            MarkDirty();
        }
        
        /// <summary>
        /// 填充矩形区域
        /// </summary>
        public void FillRect(int startX, int startY, int width, int height, TileData tileData)
        {
            int endX = Mathf.Min(startX + width, MapConstants.CHUNK_SIZE);
            int endY = Mathf.Min(startY + height, MapConstants.CHUNK_SIZE);
            startX = Mathf.Max(startX, 0);
            startY = Mathf.Max(startY, 0);
            
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int index = y * MapConstants.CHUNK_SIZE + x;
                    _tiles[index] = tileData;
                }
            }
            MarkDirty();
        }
        
        #endregion
        
        #region 查询
        
        /// <summary>
        /// 检查指定位置是否阻挡移动（只检查 Tile，不检查 Entity）
        /// </summary>
        public bool IsTileBlocking(int localX, int localY)
        {
            return GetTile(localX, localY).IsBlocking;
        }
        
        /// <summary>
        /// 检查指定位置是否为空
        /// </summary>
        public bool IsTileEmpty(int localX, int localY)
        {
            return GetTile(localX, localY).IsEmpty;
        }
        
        /// <summary>
        /// 获取非空瓦片数量
        /// </summary>
        public int GetNonEmptyTileCount()
        {
            int count = 0;
            for (int i = 0; i < MapConstants.TILES_PER_CHUNK; i++)
            {
                if (!_tiles[i].IsEmpty) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 检查区块是否完全为空
        /// </summary>
        public bool IsCompletelyEmpty()
        {
            for (int i = 0; i < MapConstants.TILES_PER_CHUNK; i++)
            {
                if (!_tiles[i].IsEmpty) return false;
            }
            return true;
        }
        
        /// <summary>
        /// 检查世界坐标是否属于该 Chunk
        /// </summary>
        public bool ContainsWorldCoord(TileCoord worldCoord)
        {
            return worldCoord.ToChunkCoord() == _coord;
        }
        
        #endregion
        
        #region 遍历
        
        /// <summary>
        /// 遍历所有瓦片
        /// </summary>
        public void ForEachTile(Action<int, int, TileData> action)
        {
            for (int y = 0; y < MapConstants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < MapConstants.CHUNK_SIZE; x++)
                {
                    int index = y * MapConstants.CHUNK_SIZE + x;
                    action(x, y, _tiles[index]);
                }
            }
        }
        
        /// <summary>
        /// 遍历非空瓦片
        /// </summary>
        public void ForEachNonEmptyTile(Action<int, int, TileData> action)
        {
            for (int y = 0; y < MapConstants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < MapConstants.CHUNK_SIZE; x++)
                {
                    int index = y * MapConstants.CHUNK_SIZE + x;
                    if (!_tiles[index].IsEmpty)
                    {
                        action(x, y, _tiles[index]);
                    }
                }
            }
        }
        
        #endregion
        
        #region 状态管理
        
        /// <summary>
        /// 标记为已修改
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
            _needsRenderRebuild = true;
        }
        
        /// <summary>
        /// 清除脏标记
        /// </summary>
        public void ClearDirty()
        {
            _isDirty = false;
        }
        
        /// <summary>
        /// 清除渲染重建标记
        /// </summary>
        public void ClearRenderRebuildFlag()
        {
            _needsRenderRebuild = false;
        }
        
        /// <summary>
        /// 标记为已加载
        /// </summary>
        public void MarkLoaded()
        {
            _isLoaded = true;
        }
        
        /// <summary>
        /// 标记为已卸载
        /// </summary>
        public void MarkUnloaded()
        {
            _isLoaded = false;
        }
        
        #endregion
        
        #region 数据获取
        
        /// <summary>
        /// 获取原始瓦片数据数组（用于序列化）
        /// </summary>
        public TileData[] GetRawTileData()
        {
            return _tiles;
        }
        
        /// <summary>
        /// 获取瓦片数据的副本
        /// </summary>
        public TileData[] CopyTileData()
        {
            TileData[] copy = new TileData[MapConstants.TILES_PER_CHUNK];
            Array.Copy(_tiles, copy, MapConstants.TILES_PER_CHUNK);
            return copy;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 局部坐标转一维索引
        /// </summary>
        private int LocalToIndex(int localX, int localY)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localX < 0 || localX >= MapConstants.CHUNK_SIZE ||
                localY < 0 || localY >= MapConstants.CHUNK_SIZE)
            {
                throw new ArgumentOutOfRangeException(
                    $"Local coordinates ({localX}, {localY}) out of range [0, {MapConstants.CHUNK_SIZE})");
            }
#endif
            return localY * MapConstants.CHUNK_SIZE + localX;
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Chunk({_coord}, Map:{_mapId}, NonEmpty:{GetNonEmptyTileCount()}, Dirty:{_isDirty})";
        }
    }
}
