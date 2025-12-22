/**
 * MapCoordUtility.cs
 * 地图坐标转换工具类
 * 
 * 提供各种坐标系统之间的转换方法
 * 作为 QFramework 的 Utility 使用
 */

using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 地图坐标转换工具
    /// </summary>
    public static class MapCoordUtility
    {
        #region World <-> Tile 转换
        
        /// <summary>
        /// 世界坐标转 Tile 坐标
        /// </summary>
        public static TileCoord WorldToTile(Vector2 worldPos)
        {
            return new TileCoord(
                Mathf.FloorToInt(worldPos.x / MapConstants.TILE_SIZE),
                Mathf.FloorToInt(worldPos.y / MapConstants.TILE_SIZE)
            );
        }
        
        /// <summary>
        /// 世界坐标转 Tile 坐标 (Vector3)
        /// </summary>
        public static TileCoord WorldToTile(Vector3 worldPos)
        {
            return WorldToTile(new Vector2(worldPos.x, worldPos.y));
        }
        
        /// <summary>
        /// Tile 坐标转世界坐标（Tile 左下角）
        /// </summary>
        public static Vector2 TileToWorld(TileCoord tileCoord)
        {
            return new Vector2(
                tileCoord.x * MapConstants.TILE_SIZE,
                tileCoord.y * MapConstants.TILE_SIZE
            );
        }
        
        /// <summary>
        /// Tile 坐标转世界坐标（Tile 中心）
        /// </summary>
        public static Vector2 TileToWorldCenter(TileCoord tileCoord)
        {
            return new Vector2(
                (tileCoord.x + 0.5f) * MapConstants.TILE_SIZE,
                (tileCoord.y + 0.5f) * MapConstants.TILE_SIZE
            );
        }
        
        /// <summary>
        /// Tile 坐标转世界坐标 (Vector3，z=0)
        /// </summary>
        public static Vector3 TileToWorld3D(TileCoord tileCoord, float z = 0)
        {
            Vector2 pos2D = TileToWorld(tileCoord);
            return new Vector3(pos2D.x, pos2D.y, z);
        }
        
        /// <summary>
        /// Tile 坐标转世界坐标中心 (Vector3)
        /// </summary>
        public static Vector3 TileToWorldCenter3D(TileCoord tileCoord, float z = 0)
        {
            Vector2 pos2D = TileToWorldCenter(tileCoord);
            return new Vector3(pos2D.x, pos2D.y, z);
        }
        
        #endregion
        
        #region Tile <-> Chunk 转换
        
        /// <summary>
        /// Tile 坐标转 Chunk 坐标
        /// </summary>
        public static ChunkCoord TileToChunk(TileCoord tileCoord)
        {
            return tileCoord.ToChunkCoord();
        }
        
        /// <summary>
        /// Tile 坐标转 Chunk 坐标（整数版本）
        /// </summary>
        public static ChunkCoord TileToChunk(int tileX, int tileY)
        {
            // 使用位运算加速
            int chunkX = tileX >= 0 
                ? tileX >> MapConstants.CHUNK_SIZE_SHIFT 
                : ((tileX + 1) >> MapConstants.CHUNK_SIZE_SHIFT) - 1;
            int chunkY = tileY >= 0 
                ? tileY >> MapConstants.CHUNK_SIZE_SHIFT 
                : ((tileY + 1) >> MapConstants.CHUNK_SIZE_SHIFT) - 1;
            
            return new ChunkCoord(chunkX, chunkY);
        }
        
        /// <summary>
        /// Tile 坐标转 Chunk 内局部坐标
        /// </summary>
        public static LocalTileCoord TileToLocal(TileCoord tileCoord)
        {
            return tileCoord.ToLocalCoord();
        }
        
        /// <summary>
        /// Tile 坐标转 Chunk 内局部坐标（整数版本）
        /// </summary>
        public static LocalTileCoord TileToLocal(int tileX, int tileY)
        {
            int localX = tileX >= 0 
                ? tileX & MapConstants.CHUNK_LOCAL_MASK 
                : MapConstants.CHUNK_SIZE - 1 - ((-tileX - 1) & MapConstants.CHUNK_LOCAL_MASK);
            int localY = tileY >= 0 
                ? tileY & MapConstants.CHUNK_LOCAL_MASK 
                : MapConstants.CHUNK_SIZE - 1 - ((-tileY - 1) & MapConstants.CHUNK_LOCAL_MASK);
            
            return new LocalTileCoord(localX, localY);
        }
        
        /// <summary>
        /// Chunk 坐标 + 局部坐标 -> Tile 坐标
        /// </summary>
        public static TileCoord ChunkLocalToTile(ChunkCoord chunkCoord, LocalTileCoord localCoord)
        {
            return new TileCoord(
                (chunkCoord.x << MapConstants.CHUNK_SIZE_SHIFT) + localCoord.x,
                (chunkCoord.y << MapConstants.CHUNK_SIZE_SHIFT) + localCoord.y
            );
        }
        
        /// <summary>
        /// Chunk 坐标转该 Chunk 左下角的 Tile 坐标
        /// </summary>
        public static TileCoord ChunkToTileOrigin(ChunkCoord chunkCoord)
        {
            return new TileCoord(
                chunkCoord.x << MapConstants.CHUNK_SIZE_SHIFT,
                chunkCoord.y << MapConstants.CHUNK_SIZE_SHIFT
            );
        }
        
        /// <summary>
        /// Chunk 坐标转该 Chunk 中心的 Tile 坐标
        /// </summary>
        public static TileCoord ChunkToTileCenter(ChunkCoord chunkCoord)
        {
            return new TileCoord(
                (chunkCoord.x << MapConstants.CHUNK_SIZE_SHIFT) + MapConstants.CHUNK_SIZE / 2,
                (chunkCoord.y << MapConstants.CHUNK_SIZE_SHIFT) + MapConstants.CHUNK_SIZE / 2
            );
        }
        
        #endregion
        
        #region World <-> Chunk 转换
        
        /// <summary>
        /// 世界坐标转 Chunk 坐标
        /// </summary>
        public static ChunkCoord WorldToChunk(Vector2 worldPos)
        {
            TileCoord tileCoord = WorldToTile(worldPos);
            return TileToChunk(tileCoord);
        }
        
        /// <summary>
        /// Chunk 坐标转世界坐标（Chunk 左下角）
        /// </summary>
        public static Vector2 ChunkToWorld(ChunkCoord chunkCoord)
        {
            return new Vector2(
                chunkCoord.x * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE,
                chunkCoord.y * MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE
            );
        }
        
        /// <summary>
        /// Chunk 坐标转世界坐标（Chunk 中心）
        /// </summary>
        public static Vector2 ChunkToWorldCenter(ChunkCoord chunkCoord)
        {
            float chunkWorldSize = MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE;
            return new Vector2(
                (chunkCoord.x + 0.5f) * chunkWorldSize,
                (chunkCoord.y + 0.5f) * chunkWorldSize
            );
        }
        
        /// <summary>
        /// 获取 Chunk 的世界坐标边界
        /// </summary>
        public static Rect GetChunkWorldBounds(ChunkCoord chunkCoord)
        {
            Vector2 origin = ChunkToWorld(chunkCoord);
            float size = MapConstants.CHUNK_SIZE * MapConstants.TILE_SIZE;
            return new Rect(origin.x, origin.y, size, size);
        }
        
        #endregion
        
        #region 索引转换
        
        /// <summary>
        /// Chunk 坐标转一维数组索引
        /// </summary>
        public static int ChunkToIndex(ChunkCoord chunkCoord, int mapWidthInChunks)
        {
            return chunkCoord.y * mapWidthInChunks + chunkCoord.x;
        }
        
        /// <summary>
        /// 一维索引转 Chunk 坐标
        /// </summary>
        public static ChunkCoord IndexToChunk(int index, int mapWidthInChunks)
        {
            return new ChunkCoord(
                index % mapWidthInChunks,
                index / mapWidthInChunks
            );
        }
        
        /// <summary>
        /// 局部 Tile 坐标转一维数组索引
        /// </summary>
        public static int LocalToIndex(LocalTileCoord localCoord)
        {
            return localCoord.y * MapConstants.CHUNK_SIZE + localCoord.x;
        }
        
        /// <summary>
        /// 局部 Tile 坐标转一维数组索引（整数版本）
        /// </summary>
        public static int LocalToIndex(int localX, int localY)
        {
            return localY * MapConstants.CHUNK_SIZE + localX;
        }
        
        /// <summary>
        /// 一维索引转局部 Tile 坐标
        /// </summary>
        public static LocalTileCoord IndexToLocal(int index)
        {
            return new LocalTileCoord(
                index % MapConstants.CHUNK_SIZE,
                index / MapConstants.CHUNK_SIZE
            );
        }
        
        #endregion
        
        #region 距离计算
        
        /// <summary>
        /// 计算两个 Tile 之间的曼哈顿距离
        /// </summary>
        public static int ManhattanDistance(TileCoord a, TileCoord b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        
        /// <summary>
        /// 计算两个 Tile 之间的切比雪夫距离（对角线距离）
        /// </summary>
        public static int ChebyshevDistance(TileCoord a, TileCoord b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
        
        /// <summary>
        /// 计算两个 Tile 之间的欧几里得距离平方
        /// </summary>
        public static int SqrDistance(TileCoord a, TileCoord b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return dx * dx + dy * dy;
        }
        
        /// <summary>
        /// 计算两个 Tile 之间的欧几里得距离
        /// </summary>
        public static float Distance(TileCoord a, TileCoord b)
        {
            return Mathf.Sqrt(SqrDistance(a, b));
        }
        
        #endregion
        
        #region 范围查询
        
        /// <summary>
        /// 获取指定中心点周围的 Tile 坐标（曼哈顿距离）
        /// </summary>
        public static TileCoord[] GetTilesInManhattanRange(TileCoord center, int range)
        {
            // 预计算数量：2*range*(range+1)+1
            int count = 2 * range * (range + 1) + 1;
            TileCoord[] result = new TileCoord[count];
            
            int index = 0;
            for (int dy = -range; dy <= range; dy++)
            {
                int xRange = range - Mathf.Abs(dy);
                for (int dx = -xRange; dx <= xRange; dx++)
                {
                    result[index++] = new TileCoord(center.x + dx, center.y + dy);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取指定中心点周围的 Tile 坐标（矩形范围）
        /// </summary>
        public static TileCoord[] GetTilesInRect(TileCoord center, int halfWidth, int halfHeight)
        {
            int width = halfWidth * 2 + 1;
            int height = halfHeight * 2 + 1;
            TileCoord[] result = new TileCoord[width * height];
            
            int index = 0;
            for (int dy = -halfHeight; dy <= halfHeight; dy++)
            {
                for (int dx = -halfWidth; dx <= halfWidth; dx++)
                {
                    result[index++] = new TileCoord(center.x + dx, center.y + dy);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取视野范围内的 Chunk 坐标
        /// </summary>
        public static ChunkCoord[] GetChunksInViewport(Vector2 center, float viewWidth, float viewHeight)
        {
            float halfWidth = viewWidth * 0.5f;
            float halfHeight = viewHeight * 0.5f;
            
            ChunkCoord minChunk = WorldToChunk(new Vector2(center.x - halfWidth, center.y - halfHeight));
            ChunkCoord maxChunk = WorldToChunk(new Vector2(center.x + halfWidth, center.y + halfHeight));
            
            int width = maxChunk.x - minChunk.x + 1;
            int height = maxChunk.y - minChunk.y + 1;
            ChunkCoord[] result = new ChunkCoord[width * height];
            
            int index = 0;
            for (int y = minChunk.y; y <= maxChunk.y; y++)
            {
                for (int x = minChunk.x; x <= maxChunk.x; x++)
                {
                    result[index++] = new ChunkCoord(x, y);
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region 方向辅助
        
        /// <summary>
        /// 获取方向对应的 Tile 偏移
        /// </summary>
        public static TileCoord GetDirectionOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return new TileCoord(0, 1);
                case Direction.South: return new TileCoord(0, -1);
                case Direction.East: return new TileCoord(1, 0);
                case Direction.West: return new TileCoord(-1, 0);
                case Direction.NorthEast: return new TileCoord(1, 1);
                case Direction.NorthWest: return new TileCoord(-1, 1);
                case Direction.SouthEast: return new TileCoord(1, -1);
                case Direction.SouthWest: return new TileCoord(-1, -1);
                default: return TileCoord.Zero;
            }
        }
        
        /// <summary>
        /// 根据两点计算方向
        /// </summary>
        public static Direction GetDirection(TileCoord from, TileCoord to)
        {
            int dx = to.x - from.x;
            int dy = to.y - from.y;
            
            // 标准化
            if (dx != 0) dx = dx > 0 ? 1 : -1;
            if (dy != 0) dy = dy > 0 ? 1 : -1;
            
            if (dx == 0 && dy == 1) return Direction.North;
            if (dx == 0 && dy == -1) return Direction.South;
            if (dx == 1 && dy == 0) return Direction.East;
            if (dx == -1 && dy == 0) return Direction.West;
            if (dx == 1 && dy == 1) return Direction.NorthEast;
            if (dx == -1 && dy == 1) return Direction.NorthWest;
            if (dx == 1 && dy == -1) return Direction.SouthEast;
            if (dx == -1 && dy == -1) return Direction.SouthWest;
            
            return Direction.None;
        }
        
        /// <summary>
        /// 方向转角度（0度为北，顺时针）
        /// </summary>
        public static float DirectionToAngle(Direction direction)
        {
            switch (direction)
            {
                case Direction.North: return 0f;
                case Direction.NorthEast: return 45f;
                case Direction.East: return 90f;
                case Direction.SouthEast: return 135f;
                case Direction.South: return 180f;
                case Direction.SouthWest: return 225f;
                case Direction.West: return 270f;
                case Direction.NorthWest: return 315f;
                default: return 0f;
            }
        }
        
        #endregion
    }
}
