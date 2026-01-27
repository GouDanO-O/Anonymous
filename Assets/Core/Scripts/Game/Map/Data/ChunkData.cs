using System;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// Chunk 数据（8x8 格子）
    /// </summary>
    [Serializable]
    public class ChunkData
    {
        #region 位置

        /// <summary>
        /// Chunk X 坐标（Chunk 索引，非格子坐标）
        /// </summary>
        public int ChunkX;

        /// <summary>
        /// Chunk Y 坐标（Chunk 索引，非格子坐标）
        /// </summary>
        public int ChunkY;

        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor;

        #endregion

        #region 数据

        /// <summary>
        /// 格子数据数组（一维数组存储，索引 = y * ChunkWidth + x）
        /// </summary>
        public CellData[] Cells;

        /// <summary>
        /// 是否已加载
        /// </summary>
        public bool IsLoaded;

        /// <summary>
        /// 是否需要重新渲染
        /// </summary>
        public bool IsDirty;

        #endregion

        #region 属性

        /// <summary>
        /// 起始格子 X 坐标（世界坐标）
        /// </summary>
        public int StartCellX => ChunkX * MapDefine.ChunkWidth;

        /// <summary>
        /// 起始格子 Y 坐标（世界坐标）
        /// </summary>
        public int StartCellY => ChunkY * MapDefine.ChunkHeight;

        #endregion

        #region 构造函数

        public ChunkData()
        {
            Cells = new CellData[MapDefine.CellsPerChunk];
            IsDirty = true;
        }

        public ChunkData(int chunkX, int chunkY, int floor) : this()
        {
            ChunkX = chunkX;
            ChunkY = chunkY;
            Floor = floor;
            InitializeCells();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化所有格子
        /// </summary>
        private void InitializeCells()
        {
            for (int y = 0; y < MapDefine.ChunkHeight; y++)
            {
                for (int x = 0; x < MapDefine.ChunkWidth; x++)
                {
                    int worldX = StartCellX + x;
                    int worldY = StartCellY + y;
                    int index = GetCellIndex(x, y);
                    Cells[index] = new CellData(worldX, worldY, Floor);
                }
            }
        }

        /// <summary>
        /// 获取格子索引
        /// </summary>
        /// <param name="localX">Chunk 内局部 X（0-7）</param>
        /// <param name="localY">Chunk 内局部 Y（0-7）</param>
        /// <returns>数组索引</returns>
        public static int GetCellIndex(int localX, int localY)
        {
            return localY * MapDefine.ChunkWidth + localX;
        }

        /// <summary>
        /// 获取格子（通过局部坐标）
        /// </summary>
        /// <param name="localX">Chunk 内局部 X（0-7）</param>
        /// <param name="localY">Chunk 内局部 Y（0-7）</param>
        public CellData GetCell(int localX, int localY)
        {
            if (localX < 0 || localX >= MapDefine.ChunkWidth ||
                localY < 0 || localY >= MapDefine.ChunkHeight)
                return null;

            int index = GetCellIndex(localX, localY);
            return Cells[index];
        }

        /// <summary>
        /// 获取格子（通过世界坐标）
        /// </summary>
        public CellData GetCellByWorldPos(int worldX, int worldY)
        {
            int localX = worldX - StartCellX;
            int localY = worldY - StartCellY;
            return GetCell(localX, localY);
        }

        /// <summary>
        /// 设置格子数据
        /// </summary>
        public void SetCell(int localX, int localY, CellData cellData)
        {
            if (localX < 0 || localX >= MapDefine.ChunkWidth ||
                localY < 0 || localY >= MapDefine.ChunkHeight)
                return;

            int index = GetCellIndex(localX, localY);
            Cells[index] = cellData;
            IsDirty = true;
        }

        /// <summary>
        /// 标记为需要更新
        /// </summary>
        public void MarkDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// 检查世界坐标是否在此 Chunk 内
        /// </summary>
        public bool ContainsWorldPos(int worldX, int worldY)
        {
            return worldX >= StartCellX && worldX < StartCellX + MapDefine.ChunkWidth &&
                   worldY >= StartCellY && worldY < StartCellY + MapDefine.ChunkHeight;
        }

        #endregion
    }
}
