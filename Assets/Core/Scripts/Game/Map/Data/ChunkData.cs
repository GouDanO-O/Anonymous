using System;
using Core.Game.Map.Define;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// Chunk数据（32x32格子块）
    /// </summary>
    [Serializable]
    public class ChunkData
    {
        #region 位置

        /// <summary>
        /// Chunk在地图中的X索引
        /// </summary>
        public int ChunkX;

        /// <summary>
        /// Chunk在地图中的Y索引
        /// </summary>
        public int ChunkY;

        /// <summary>
        /// 所在楼层
        /// </summary>
        public int Floor;

        #endregion

        #region 数据

        /// <summary>
        /// 格子数据数组（一维数组存储，按行优先）
        /// </summary>
        private CellData[] _cells;

        /// <summary>
        /// 是否需要重新构建Mesh
        /// </summary>
        public bool IsDirty { get; private set; }

        #endregion

        #region 属性

        /// <summary>
        /// Chunk左下角的世界格子X坐标
        /// </summary>
        public int WorldStartX => ChunkX * MapDefine.ChunkSize;

        /// <summary>
        /// Chunk左下角的世界格子Y坐标
        /// </summary>
        public int WorldStartY => ChunkY * MapDefine.ChunkSize;

        /// <summary>
        /// Chunk右上角的世界格子X坐标（不包含）
        /// </summary>
        public int WorldEndX => WorldStartX + MapDefine.ChunkSize;

        /// <summary>
        /// Chunk右上角的世界格子Y坐标（不包含）
        /// </summary>
        public int WorldEndY => WorldStartY + MapDefine.ChunkSize;

        #endregion

        #region 构造函数

        public ChunkData()
        {
            _cells = new CellData[MapDefine.CellsPerChunk];
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

        #region 初始化

        /// <summary>
        /// 初始化所有格子
        /// </summary>
        private void InitializeCells()
        {
            for (int ly = 0; ly < MapDefine.ChunkSize; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkSize; lx++)
                {
                    int worldX = WorldStartX + lx;
                    int worldY = WorldStartY + ly;
                    int index = LocalToIndex(lx, ly);
                    _cells[index] = new CellData(worldX, worldY, Floor);
                }
            }
        }

        #endregion

        #region 格子访问

        /// <summary>
        /// 获取格子（本地坐标）
        /// </summary>
        /// <param name="localX">Chunk内的X坐标（0到ChunkSize-1）</param>
        /// <param name="localY">Chunk内的Y坐标（0到ChunkSize-1）</param>
        public CellData GetCell(int localX, int localY)
        {
            if (!IsValidLocalPos(localX, localY))
                return null;

            int index = LocalToIndex(localX, localY);
            return _cells[index];
        }

        /// <summary>
        /// 获取格子（世界坐标）
        /// </summary>
        public CellData GetCellByWorld(int worldX, int worldY)
        {
            int localX = worldX - WorldStartX;
            int localY = worldY - WorldStartY;
            return GetCell(localX, localY);
        }

        /// <summary>
        /// 设置格子（本地坐标）
        /// </summary>
        public void SetCell(int localX, int localY, CellData cell)
        {
            if (!IsValidLocalPos(localX, localY))
                return;

            int index = LocalToIndex(localX, localY);
            _cells[index] = cell;
            MarkDirty();
        }

        /// <summary>
        /// 遍历所有格子
        /// </summary>
        public void ForEachCell(Action<CellData, int, int> action)
        {
            for (int ly = 0; ly < MapDefine.ChunkSize; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkSize; lx++)
                {
                    var cell = GetCell(lx, ly);
                    if (cell != null)
                    {
                        action(cell, lx, ly);
                    }
                }
            }
        }

        #endregion

        #region 脏标记

        /// <summary>
        /// 标记为需要重建
        /// </summary>
        public void MarkDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// 清除脏标记
        /// </summary>
        public void ClearDirty()
        {
            IsDirty = false;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 本地坐标转数组索引
        /// </summary>
        private int LocalToIndex(int localX, int localY)
        {
            return localY * MapDefine.ChunkSize + localX;
        }

        /// <summary>
        /// 检查本地坐标是否有效
        /// </summary>
        private bool IsValidLocalPos(int localX, int localY)
        {
            return localX >= 0 && localX < MapDefine.ChunkSize &&
                   localY >= 0 && localY < MapDefine.ChunkSize;
        }

        /// <summary>
        /// 检查世界坐标是否在此Chunk内
        /// </summary>
        public bool ContainsWorldPos(int worldX, int worldY)
        {
            return worldX >= WorldStartX && worldX < WorldEndX &&
                   worldY >= WorldStartY && worldY < WorldEndY;
        }

        public override string ToString()
        {
            return $"Chunk({ChunkX}, {ChunkY}, Floor={Floor})";
        }

        #endregion
    }
}
