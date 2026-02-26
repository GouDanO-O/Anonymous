using System;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 16x16 块数据
    /// </summary>
    [Serializable]
    public class ChunkData
    {
        public int ChunkX;
        public int ChunkY;
        public int Floor;

        private CellData[] _cells;

        public bool IsDirty { get; private set; }

        /// <summary>
        /// 块左下角的世界坐标X
        /// </summary>
        public int WorldStartX => ChunkX * MapConst.ChunkSize;

        /// <summary>
        /// 块左下角的世界坐标Y
        /// </summary>
        public int WorldStartY => ChunkY * MapConst.ChunkSize;

        public ChunkData(int chunkX, int chunkY, int floor)
        {
            ChunkX = chunkX;
            ChunkY = chunkY;
            Floor = floor;
            _cells = new CellData[MapConst.CellsPerChunk];
            IsDirty = true;
        }

        /// <summary>
        /// 初始化所有Cell
        /// </summary>
        public void InitializeCells(int mapWidth, int mapHeight)
        {
            for (int ly = 0; ly < MapConst.ChunkSize; ly++)
            {
                for (int lx = 0; lx < MapConst.ChunkSize; lx++)
                {
                    int worldX = WorldStartX + lx;
                    int worldY = WorldStartY + ly;

                    if (worldX < mapWidth && worldY < mapHeight)
                    {
                        _cells[ly * MapConst.ChunkSize + lx] = new CellData(worldX, worldY, Floor);
                    }
                }
            }
        }

        /// <summary>
        /// 通过局部坐标获取Cell
        /// </summary>
        public CellData GetCell(int localX, int localY)
        {
            if (localX < 0 || localX >= MapConst.ChunkSize ||
                localY < 0 || localY >= MapConst.ChunkSize)
                return null;

            return _cells[localY * MapConst.ChunkSize + localX];
        }

        /// <summary>
        /// 通过世界坐标获取Cell
        /// </summary>
        public CellData GetCellByWorld(int worldX, int worldY)
        {
            int localX = worldX - WorldStartX;
            int localY = worldY - WorldStartY;
            return GetCell(localX, localY);
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }

        /// <summary>
        /// 遍历所有Cell
        /// </summary>
        public void ForEachCell(Action<CellData, int, int> action)
        {
            for (int ly = 0; ly < MapConst.ChunkSize; ly++)
            {
                for (int lx = 0; lx < MapConst.ChunkSize; lx++)
                {
                    var cell = _cells[ly * MapConst.ChunkSize + lx];
                    if (cell != null)
                        action(cell, lx, ly);
                }
            }
        }
    }
}
