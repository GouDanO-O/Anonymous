using System;
using Core.Game.World.Map.Data;
using Core.Game.World.Tile;

namespace Core.Game.World.Chunk
{
    /// <summary>
    /// Chunk 坐标
    /// </summary>
    [Serializable]
    public struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public int x, y;

        public ChunkCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public TileCoord GetOriginTile(int floor = 1) =>
            new TileCoord(x * MapConstants.CHUNK_SIZE, y * MapConstants.CHUNK_SIZE, floor);

        public TileCoord LocalToGlobal(LocalTileCoord local, int floor = 1) =>
            new TileCoord(x * MapConstants.CHUNK_SIZE + local.x, y * MapConstants.CHUNK_SIZE + local.y, floor);

        public bool Equals(ChunkCoord other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is ChunkCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
        public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
        public static bool operator !=(ChunkCoord a, ChunkCoord b) => !a.Equals(b);
        public override string ToString() => $"Chunk({x}, {y})";
    }
}