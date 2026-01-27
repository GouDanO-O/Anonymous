using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Utility
{
    /// <summary>
    /// 地图坐标转换工具
    /// 处理格子坐标、Chunk 坐标之间的转换
    /// </summary>
    public static class MapCoordUtility
    {
        #region 格子坐标 <-> Chunk 坐标

        /// <summary>
        /// 世界格子坐标转 Chunk 索引
        /// </summary>
        public static Vector2Int CellToChunkIndex(int cellX, int cellY)
        {
            int chunkX = cellX / MapDefine.ChunkWidth;
            int chunkY = cellY / MapDefine.ChunkHeight;

            // 处理负数情况
            if (cellX < 0 && cellX % MapDefine.ChunkWidth != 0)
                chunkX--;
            if (cellY < 0 && cellY % MapDefine.ChunkHeight != 0)
                chunkY--;

            return new Vector2Int(chunkX, chunkY);
        }

        /// <summary>
        /// 世界格子坐标转 Chunk 内局部坐标
        /// </summary>
        public static Vector2Int CellToLocalPos(int cellX, int cellY)
        {
            int localX = cellX % MapDefine.ChunkWidth;
            int localY = cellY % MapDefine.ChunkHeight;

            // 处理负数情况
            if (localX < 0)
                localX += MapDefine.ChunkWidth;
            if (localY < 0)
                localY += MapDefine.ChunkHeight;

            return new Vector2Int(localX, localY);
        }

        /// <summary>
        /// Chunk 索引 + 局部坐标转世界格子坐标
        /// </summary>
        public static Vector2Int ChunkLocalToCell(int chunkX, int chunkY, int localX, int localY)
        {
            int cellX = chunkX * MapDefine.ChunkWidth + localX;
            int cellY = chunkY * MapDefine.ChunkHeight + localY;
            return new Vector2Int(cellX, cellY);
        }

        #endregion

        #region 范围计算

        /// <summary>
        /// 获取屏幕可见区域内的 Chunk 范围
        /// </summary>
        /// <param name="cameraPos">摄像机中心位置</param>
        /// <param name="viewSize">视野大小（半宽，半高）</param>
        /// <param name="floor">当前楼层</param>
        /// <returns>(minChunkX, minChunkY, maxChunkX, maxChunkY)</returns>
        public static (int, int, int, int) GetVisibleChunkRange(Vector3 cameraPos, Vector2 viewSize, int floor = 0)
        {
            // 计算视野四角的格子坐标
            Vector3 bottomLeft = cameraPos + new Vector3(-viewSize.x, -viewSize.y, 0);
            Vector3 topRight = cameraPos + new Vector3(viewSize.x, viewSize.y, 0);

            Vector2Int minCell = IsometricUtility.ScreenToCellInt(bottomLeft, floor);
            Vector2Int maxCell = IsometricUtility.ScreenToCellInt(topRight, floor);

            // 由于等轴测的特殊性，需要扩大范围
            minCell -= new Vector2Int(2, 2);
            maxCell += new Vector2Int(2, 2);

            Vector2Int minChunk = CellToChunkIndex(minCell.x, minCell.y);
            Vector2Int maxChunk = CellToChunkIndex(maxCell.x, maxCell.y);

            return (minChunk.x, minChunk.y, maxChunk.x, maxChunk.y);
        }

        /// <summary>
        /// 获取指定中心点周围的 Chunk 列表
        /// </summary>
        public static void GetSurroundingChunks(int centerChunkX, int centerChunkY, int radius,
            List<Vector2Int> result)
        {
            result.Clear();

            for (int y = centerChunkY - radius; y <= centerChunkY + radius; y++)
            {
                for (int x = centerChunkX - radius; x <= centerChunkX + radius; x++)
                {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }

        #endregion

        #region 邻居计算

        /// <summary>
        /// 获取格子的四方向邻居坐标
        /// </summary>
        public static Vector2Int[] GetNeighbors4(int cellX, int cellY)
        {
            return new Vector2Int[]
            {
                new Vector2Int(cellX, cellY + 1), // 北
                new Vector2Int(cellX + 1, cellY), // 东
                new Vector2Int(cellX, cellY - 1), // 南
                new Vector2Int(cellX - 1, cellY), // 西
            };
        }

        /// <summary>
        /// 获取格子的八方向邻居坐标
        /// </summary>
        public static Vector2Int[] GetNeighbors8(int cellX, int cellY)
        {
            return new Vector2Int[]
            {
                new Vector2Int(cellX, cellY + 1), // 北
                new Vector2Int(cellX + 1, cellY + 1), // 东北
                new Vector2Int(cellX + 1, cellY), // 东
                new Vector2Int(cellX + 1, cellY - 1), // 东南
                new Vector2Int(cellX, cellY - 1), // 南
                new Vector2Int(cellX - 1, cellY - 1), // 西南
                new Vector2Int(cellX - 1, cellY), // 西
                new Vector2Int(cellX - 1, cellY + 1), // 西北
            };
        }

        #endregion

        #region 边界检查

        /// <summary>
        /// 检查格子坐标是否在地图范围内
        /// </summary>
        public static bool IsValidCell(int cellX, int cellY, int mapWidth, int mapHeight)
        {
            return cellX >= 0 && cellX < mapWidth && cellY >= 0 && cellY < mapHeight;
        }

        /// <summary>
        /// 检查 Chunk 坐标是否在地图范围内
        /// </summary>
        public static bool IsValidChunk(int chunkX, int chunkY, int chunkCountX, int chunkCountY)
        {
            return chunkX >= 0 && chunkX < chunkCountX && chunkY >= 0 && chunkY < chunkCountY;
        }

        /// <summary>
        /// 将格子坐标限制在地图范围内
        /// </summary>
        public static Vector2Int ClampCell(int cellX, int cellY, int mapWidth, int mapHeight)
        {
            return new Vector2Int(
                Mathf.Clamp(cellX, 0, mapWidth - 1),
                Mathf.Clamp(cellY, 0, mapHeight - 1)
            );
        }

        #endregion
    }
}
