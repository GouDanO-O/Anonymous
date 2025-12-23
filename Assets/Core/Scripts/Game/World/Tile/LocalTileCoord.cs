using System;
using Core.Game.World.Map.Data;

namespace Core.Game.World.Tile
{
    /// <summary>
    /// Chunk 内局部坐标 (0 ~ CHUNK_SIZE-1)
    /// </summary>
    [Serializable]
    public struct LocalTileCoord : IEquatable<LocalTileCoord>
    {
        public int x, y;

        public LocalTileCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int ToIndex() => y * MapConstants.CHUNK_SIZE + x;

        public static LocalTileCoord FromIndex(int idx) =>
            new LocalTileCoord(idx % MapConstants.CHUNK_SIZE, idx / MapConstants.CHUNK_SIZE);

        public bool IsValid() => x >= 0 && x < MapConstants.CHUNK_SIZE && y >= 0 && y < MapConstants.CHUNK_SIZE;

        public bool Equals(LocalTileCoord other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is LocalTileCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y);
        public static bool operator ==(LocalTileCoord a, LocalTileCoord b) => a.Equals(b);
        public static bool operator !=(LocalTileCoord a, LocalTileCoord b) => !a.Equals(b);
        public override string ToString() => $"Local({x}, {y})";
    }
}