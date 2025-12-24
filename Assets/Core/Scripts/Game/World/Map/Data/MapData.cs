using System;
using System.Collections.Generic;
using Core.Game.World.Chunk;
using Core.Game.World.Chunk.Data;
using Core.Game.World.Tile;
using Core.Game.World.Tile.Data;

namespace Core.Game.World.Map.Data
{
    public class MapData
    {
        public MapMetaData metadata;
        public bool IsDirty { get; private set; }
        
        private Dictionary<ChunkCoord, ChunkData> chunks = new Dictionary<ChunkCoord, ChunkData>();
        
        public MapData(string mapId, string mapName, int widthInChunks, int heightInChunks)
        {
            metadata = new MapMetaData
            {
                mapId = mapId,
                mapName = mapName,
                widthInChunks = widthInChunks,
                heightInChunks = heightInChunks
            };
        }
        
        #region 边界检查
        
        public bool IsChunkInBounds(ChunkCoord coord) =>
            coord.x >= 0 && coord.x < metadata.widthInChunks &&
            coord.y >= 0 && coord.y < metadata.heightInChunks;
        
        public bool IsTileInBounds(int x, int y) =>
            x >= 0 && x < metadata.WidthInTiles &&
            y >= 0 && y < metadata.HeightInTiles;
        
        public bool IsTileInBounds(TileCoord coord) => IsTileInBounds(coord.x, coord.y);
        
        #endregion
        
        #region Chunk 管理
        
        public ChunkData GetOrCreateChunk(ChunkCoord coord)
        {
            if (!IsChunkInBounds(coord))
                throw new ArgumentOutOfRangeException(nameof(coord));
            
            if (!chunks.TryGetValue(coord, out var chunk))
            {
                chunk = new ChunkData(coord, metadata.defaultMinFloor, metadata.defaultMaxFloor);
                chunks[coord] = chunk;
                IsDirty = true;
            }
            return chunk;
        }
        
        public bool TryGetChunk(ChunkCoord coord, out ChunkData chunk) =>
            chunks.TryGetValue(coord, out chunk);
        
        public IEnumerable<ChunkData> GetAllChunks() => chunks.Values;
        public int LoadedChunkCount => chunks.Count;
        
        #endregion
        
        #region Tile 访问
        
        public ref TileData GetTile(TileCoord coord)
        {
            if (!IsTileInBounds(coord))
                throw new ArgumentOutOfRangeException(nameof(coord));
            
            var chunk = GetOrCreateChunk(coord.ToChunkCoord());
            var local = coord.ToLocalCoord();
            return ref chunk.GetTile(local, coord.z);
        }
        
        public ref TileData GetTile(int x, int y, int z) => ref GetTile(new TileCoord(x, y, z));
        
        public bool TryGetTile(TileCoord coord, out TileData tile)
        {
            if (!IsTileInBounds(coord))
            {
                tile = TileData.Empty;
                return false;
            }
            
            if (TryGetChunk(coord.ToChunkCoord(), out var chunk))
            {
                var local = coord.ToLocalCoord();
                return chunk.TryGetTile(local.x, local.y, coord.z, out tile);
            }
            
            tile = TileData.Empty;
            return false;
        }
        
        public void SetTile(TileCoord coord, TileData tile)
        {
            if (!IsTileInBounds(coord))
                throw new ArgumentOutOfRangeException(nameof(coord));
            
            var chunk = GetOrCreateChunk(coord.ToChunkCoord());
            var local = coord.ToLocalCoord();
            chunk.SetTile(local.x, local.y, coord.z, tile);
            IsDirty = true;
        }
        
        #endregion
        
        #region 便捷方法
        
        /// <summary>
        /// 设置地形（仅第一层有效）
        /// </summary>
        public void SetGround(int x, int y, ushort tileId)
        {
            ref var tile = ref GetTile(x, y, MapConstants.GROUND_FLOOR);
            tile.SetGround(tileId);
            IsDirty = true;
        }
        
        /// <summary>
        /// 设置地板
        /// </summary>
        public void SetFloor(TileCoord coord, ushort tileId)
        {
            ref var tile = ref GetTile(coord);
            tile.SetFloor(tileId);
            IsDirty = true;
        }
        
        /// <summary>
        /// 检查是否有天花板（上层有地板）
        /// </summary>
        public bool HasCeiling(TileCoord coord)
        {
            if (TryGetTile(coord.Above(), out var tile))
                return tile.HasFloor;
            return false;
        }
        
        /// <summary>
        /// 检查地形是否可承重
        /// </summary>
        public bool CanGroundBearWeight(int x, int y)
        {
            if (TryGetTile(new TileCoord(x, y, MapConstants.GROUND_FLOOR), out var tile))
                return tile.CanBearingEntity();
            return false;
        }
        
        #endregion
        
        #region 状态管理
        
        public void ClearDirty()
        {
            IsDirty = false;
            foreach (var chunk in chunks.Values)
                chunk.ClearDirty();
        }
        
        public IEnumerable<ChunkData> GetDirtyChunks()
        {
            foreach (var chunk in chunks.Values)
                if (chunk.IsDirty) yield return chunk;
        }
        
        /// <summary>
        /// 预创建所有 Chunk
        /// </summary>
        public void InitializeAllChunks()
        {
            for (int x = 0; x < metadata.widthInChunks; x++)
                for (int y = 0; y < metadata.heightInChunks; y++)
                    GetOrCreateChunk(new ChunkCoord(x, y));
        }
        
        #endregion
    }
}