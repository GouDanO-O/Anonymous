using System;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 地图数据
    /// </summary>
    [Serializable]
    public class MapData
    {
        #region 基础信息

        /// <summary>
        /// 地图名称
        /// </summary>
        public string MapName;

        /// <summary>
        /// 地图宽度（格子数）
        /// </summary>
        public int Width;

        /// <summary>
        /// 地图高度（格子数）
        /// </summary>
        public int Height;

        /// <summary>
        /// 楼层数
        /// </summary>
        public int FloorCount;

        /// <summary>
        /// 随机种子
        /// </summary>
        public int Seed;

        #endregion

        #region Chunk 数据

        /// <summary>
        /// Chunk 宽度数量
        /// </summary>
        public int ChunkCountX;

        /// <summary>
        /// Chunk 高度数量
        /// </summary>
        public int ChunkCountY;

        /// <summary>
        /// 所有 Chunk 数据
        /// 三维数组：[floor, chunkY, chunkX]
        /// </summary>
        public ChunkData[,,] Chunks;

        #endregion

        #region 构造函数

        public MapData()
        {
        }

        public MapData(string mapName, int width, int height, int floorCount, int seed = 0)
        {
            MapName = mapName;
            Width = width;
            Height = height;
            FloorCount = floorCount;
            Seed = seed;

            // 计算 Chunk 数量（向上取整）
            ChunkCountX = (width + MapDefine.ChunkWidth - 1) / MapDefine.ChunkWidth;
            ChunkCountY = (height + MapDefine.ChunkHeight - 1) / MapDefine.ChunkHeight;

            // 初始化 Chunk 数组
            Chunks = new ChunkData[floorCount, ChunkCountY, ChunkCountX];
        }

        #endregion

        #region Chunk 操作

        /// <summary>
        /// 获取 Chunk
        /// </summary>
        /// <param name="chunkX">Chunk X 索引</param>
        /// <param name="chunkY">Chunk Y 索引</param>
        /// <param name="floor">楼层</param>
        public ChunkData GetChunk(int chunkX, int chunkY, int floor)
        {
            if (!IsValidChunkPos(chunkX, chunkY, floor))
                return null;

            return Chunks[floor, chunkY, chunkX];
        }

        /// <summary>
        /// 设置 Chunk
        /// </summary>
        public void SetChunk(int chunkX, int chunkY, int floor, ChunkData chunkData)
        {
            if (!IsValidChunkPos(chunkX, chunkY, floor))
                return;

            Chunks[floor, chunkY, chunkX] = chunkData;
        }

        /// <summary>
        /// 创建并设置 Chunk
        /// </summary>
        public ChunkData CreateChunk(int chunkX, int chunkY, int floor)
        {
            if (!IsValidChunkPos(chunkX, chunkY, floor))
                return null;

            var chunk = new ChunkData(chunkX, chunkY, floor);
            Chunks[floor, chunkY, chunkX] = chunk;
            return chunk;
        }

        /// <summary>
        /// 检查 Chunk 坐标是否有效
        /// </summary>
        public bool IsValidChunkPos(int chunkX, int chunkY, int floor)
        {
            return chunkX >= 0 && chunkX < ChunkCountX &&
                   chunkY >= 0 && chunkY < ChunkCountY &&
                   floor >= 0 && floor < FloorCount;
        }

        #endregion

        #region Cell 操作

        /// <summary>
        /// 获取格子
        /// </summary>
        /// <param name="x">世界 X 坐标</param>
        /// <param name="y">世界 Y 坐标</param>
        /// <param name="floor">楼层</param>
        public CellData GetCell(int x, int y, int floor)
        {
            if (!IsValidCellPos(x, y, floor))
                return null;

            int chunkX = x / MapDefine.ChunkWidth;
            int chunkY = y / MapDefine.ChunkHeight;

            var chunk = GetChunk(chunkX, chunkY, floor);
            return chunk?.GetCellByWorldPos(x, y);
        }

        /// <summary>
        /// 检查格子坐标是否有效
        /// </summary>
        public bool IsValidCellPos(int x, int y, int floor)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height &&
                   floor >= 0 && floor < FloorCount;
        }

        /// <summary>
        /// 世界坐标转 Chunk 索引
        /// </summary>
        public (int chunkX, int chunkY) WorldToChunkIndex(int worldX, int worldY)
        {
            int chunkX = worldX / MapDefine.ChunkWidth;
            int chunkY = worldY / MapDefine.ChunkHeight;
            return (chunkX, chunkY);
        }

        /// <summary>
        /// 世界坐标转 Chunk 内局部坐标
        /// </summary>
        public (int localX, int localY) WorldToLocalPos(int worldX, int worldY)
        {
            int localX = worldX % MapDefine.ChunkWidth;
            int localY = worldY % MapDefine.ChunkHeight;
            return (localX, localY);
        }

        #endregion

        #region 遍历

        /// <summary>
        /// 遍历所有 Chunk
        /// </summary>
        public void ForeachChunk(Action<ChunkData, int, int, int> action)
        {
            for (int floor = 0; floor < FloorCount; floor++)
            {
                for (int cy = 0; cy < ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < ChunkCountX; cx++)
                    {
                        var chunk = Chunks[floor, cy, cx];
                        if (chunk != null)
                        {
                            action?.Invoke(chunk, cx, cy, floor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 遍历指定楼层的所有 Chunk
        /// </summary>
        public void ForeachChunkInFloor(int floor, Action<ChunkData, int, int> action)
        {
            if (floor < 0 || floor >= FloorCount)
                return;

            for (int cy = 0; cy < ChunkCountY; cy++)
            {
                for (int cx = 0; cx < ChunkCountX; cx++)
                {
                    var chunk = Chunks[floor, cy, cx];
                    if (chunk != null)
                    {
                        action?.Invoke(chunk, cx, cy);
                    }
                }
            }
        }

        #endregion
    }
}
