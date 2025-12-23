using System;
using Core.Game.World.Chunk;
using Core.Game.World.Map.Data;
using UnityEngine;

namespace Core.Game.World.Tile
{
    /// <summary>
    /// Tile 全局坐标（含楼层）
    /// </summary>
    [Serializable]
    public struct TileCoord : IEquatable<TileCoord>
    {
        public int x, y, z;

        public TileCoord(int x, int y, int z = 1)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public ChunkCoord ToChunkCoord()
        {
            int cx = x >= 0 ? x / MapConstants.CHUNK_SIZE : (x - MapConstants.CHUNK_SIZE + 1) / MapConstants.CHUNK_SIZE;
            int cy = y >= 0 ? y / MapConstants.CHUNK_SIZE : (y - MapConstants.CHUNK_SIZE + 1) / MapConstants.CHUNK_SIZE;
            return new ChunkCoord(cx, cy);
        }

        public LocalTileCoord ToLocalCoord()
        {
            int lx = ((x % MapConstants.CHUNK_SIZE) + MapConstants.CHUNK_SIZE) % MapConstants.CHUNK_SIZE;
            int ly = ((y % MapConstants.CHUNK_SIZE) + MapConstants.CHUNK_SIZE) % MapConstants.CHUNK_SIZE;
            return new LocalTileCoord(lx, ly);
        }

        public Vector3 ToWorldPosition() => new Vector3(x * MapConstants.TILE_SIZE, y * MapConstants.TILE_SIZE, 0);

        public static TileCoord FromWorldPosition(Vector3 pos, int floor = 1) =>
            new TileCoord(Mathf.FloorToInt(pos.x / MapConstants.TILE_SIZE),
                Mathf.FloorToInt(pos.y / MapConstants.TILE_SIZE), floor);

        public TileCoord Above() => new TileCoord(x, y, z + 1);
        public TileCoord Below() => new TileCoord(x, y, z - 1);
        public TileCoord Offset(int dx, int dy) => new TileCoord(x + dx, y + dy, z);

        public bool Equals(TileCoord other) => x == other.x && y == other.y && z == other.z;
        public override bool Equals(object obj) => obj is TileCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y, z);
        public static bool operator ==(TileCoord a, TileCoord b) => a.Equals(b);
        public static bool operator !=(TileCoord a, TileCoord b) => !a.Equals(b);
        public override string ToString() => $"({x}, {y}, z={z})";
    }
}