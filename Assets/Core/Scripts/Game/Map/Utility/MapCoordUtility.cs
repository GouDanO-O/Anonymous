using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Utility
{
    /// <summary>
    /// 地图坐标转换工具
    /// </summary>
    public static class MapCoordUtility
    {
        /// <summary>
        /// 世界坐标 → Tile坐标
        /// </summary>
        public static Vector2Int WorldToTile(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / MapConst.TileWorldSize),
                Mathf.FloorToInt(worldPos.y / MapConst.TileWorldSize)
            );
        }

        /// <summary>
        /// Tile坐标 → 世界坐标 (Tile中心)
        /// </summary>
        public static Vector3 TileToWorld(int tileX, int tileY)
        {
            return new Vector3(
                (tileX + 0.5f) * MapConst.TileWorldSize,
                (tileY + 0.5f) * MapConst.TileWorldSize,
                0f
            );
        }

        /// <summary>
        /// Tile坐标 → 世界坐标 (Tile左下角)
        /// </summary>
        public static Vector3 TileToWorldCorner(int tileX, int tileY)
        {
            return new Vector3(
                tileX * MapConst.TileWorldSize,
                tileY * MapConst.TileWorldSize,
                0f
            );
        }

        /// <summary>
        /// 世界坐标 → Chunk坐标
        /// </summary>
        public static Vector2Int WorldToChunk(Vector3 worldPos)
        {
            var tile = WorldToTile(worldPos);
            return TileToChunk(tile.x, tile.y);
        }

        /// <summary>
        /// Tile坐标 → Chunk坐标
        /// </summary>
        public static Vector2Int TileToChunk(int tileX, int tileY)
        {
            return new Vector2Int(
                tileX / MapConst.ChunkSize,
                tileY / MapConst.ChunkSize
            );
        }

        /// <summary>
        /// Tile坐标 → Chunk内局部坐标
        /// </summary>
        public static Vector2Int TileToLocal(int tileX, int tileY)
        {
            return new Vector2Int(
                tileX % MapConst.ChunkSize,
                tileY % MapConst.ChunkSize
            );
        }

        /// <summary>
        /// 计算排序顺序 (Y越大越靠后渲染 = 在更远处)
        /// </summary>
        public static int CalculateSortingOrder(int y, int floor, int mapHeight)
        {
            return (mapHeight - y) + (floor * mapHeight * 2);
        }

        /// <summary>
        /// Chunk左下角的世界坐标
        /// </summary>
        public static Vector3 ChunkToWorldCorner(int chunkX, int chunkY)
        {
            return new Vector3(
                chunkX * MapConst.ChunkSize * MapConst.TileWorldSize,
                chunkY * MapConst.ChunkSize * MapConst.TileWorldSize,
                0f
            );
        }
    }
}
