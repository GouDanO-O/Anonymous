using System;
using Core.Game.World.Map.Data;
using Core.Game.World.Tile;
using Core.Game.World.Tile.Data;

namespace Core.Game.World.Chunk.Data
{
    [Serializable]
    public class ChunkData
    {
        public ChunkCoord coord;
        
        public int MinFloor { get; private set; }
        
        public int MaxFloor { get; private set; }
        
        public bool IsDirty { get; private set; }
        
        // floors[floorIndex][tileIndex]
        private TileData[][] floors;

        public ChunkData(ChunkCoord coord, int minFloor = 0, int maxFloor = 1)
        {
            this.coord = coord;
            MinFloor = minFloor;
            MaxFloor = maxFloor;
            InitializeFloors();
        }
        
        private void InitializeFloors()
        {
            int count = MaxFloor - MinFloor + 1;
            floors = new TileData[count][];
            for (int i = 0; i < count; i++)
                floors[i] = new TileData[MapConstants.TILES_PER_CHUNK];
        }
        
        private int FloorToIndex(int floor) => floor - MinFloor;
        public bool IsFloorValid(int floor) => floor >= MinFloor && floor <= MaxFloor;

        /// <summary>
        /// 扩展楼层范围
        /// </summary>
        public void EnsureFloor(int floor)
        {
            if (floor >= MinFloor && floor <= MaxFloor) return;
            
            int newMin = Math.Min(MinFloor, floor);
            int newMax = Math.Max(MaxFloor, floor);
            var newFloors = new TileData[newMax - newMin + 1][];
            
            for (int f = newMin; f <= newMax; f++)
            {
                int newIdx = f - newMin;
                if (f >= MinFloor && f <= MaxFloor)
                    newFloors[newIdx] = floors[FloorToIndex(f)];
                else
                    newFloors[newIdx] = new TileData[MapConstants.TILES_PER_CHUNK];
            }
            
            floors = newFloors;
            MinFloor = newMin;
            MaxFloor = newMax;
            IsDirty = true;
        }
        
        /// <summary>
        /// 获取 Tile（引用）
        /// </summary>
        public ref TileData GetTile(int localX, int localY, int floor)
        {
            if (!IsFloorValid(floor))
                throw new ArgumentOutOfRangeException(nameof(floor));
            
            int idx = localY * MapConstants.CHUNK_SIZE + localX;
            return ref floors[FloorToIndex(floor)][idx];
        }
        
        public ref TileData GetTile(LocalTileCoord local, int floor) => ref GetTile(local.x, local.y, floor);
        
        /// <summary>
        /// 安全获取 Tile
        /// </summary>
        public bool TryGetTile(int localX, int localY, int floor, out TileData tile)
        {
            if (!IsFloorValid(floor) || localX < 0 || localX >= MapConstants.CHUNK_SIZE ||
                localY < 0 || localY >= MapConstants.CHUNK_SIZE)
            {
                tile = TileData.Empty;
                return false;
            }
            
            int idx = localY * MapConstants.CHUNK_SIZE + localX;
            tile = floors[FloorToIndex(floor)][idx];
            return true;
        }
        
        /// <summary>
        /// 设置 Tile
        /// </summary>
        public void SetTile(int localX, int localY, int floor, TileData tile)
        {
            EnsureFloor(floor);
            int idx = localY * MapConstants.CHUNK_SIZE + localX;
            floors[FloorToIndex(floor)][idx] = tile;
            IsDirty = true;
        }
        
        /// <summary>
        /// 获取楼层所有 Tiles（只读）
        /// </summary>
        public ReadOnlySpan<TileData> GetFloorTiles(int floor)
        {
            if (!IsFloorValid(floor)) return ReadOnlySpan<TileData>.Empty;
            return floors[FloorToIndex(floor)];
        }
        
        /// <summary>
        /// 检查指定位置是否有地板
        /// </summary>
        public bool HasFloorAt(int localX, int localY, int floor)
        {
            if (TryGetTile(localX, localY, floor, out var tile))
                return tile.HasFloor;
            return false;
        }
        
        public void ClearDirty() => IsDirty = false;
        public void MarkDirty() => IsDirty = true;
    }
}