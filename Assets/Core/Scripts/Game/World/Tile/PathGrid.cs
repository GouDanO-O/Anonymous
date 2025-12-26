using System;
using Core.Game.Map.Tile.Data;
using UnityEngine;

namespace Core.Game.Map.Tile
{
    /// <summary>
    /// 路径成本网格 - 存储每个cell的移动成本
    /// 参照RimWorld的PathGrid设计
    /// </summary>
    public class PathGrid
    {
        private readonly int _width;
        private readonly int _height;
        
        // 路径成本数组 (0 = 无法通行, >0 = 移动成本)
        private int[] _pathCosts;
        
        // 是否可通行数组 (快速查询)
        private bool[] _walkable;

        #region 常量

        public const int ImpassableCost = 10000;  // 不可通行标记
        public const int DefaultCost = 60;        // 默认移动成本(1格=1秒的60ticks)

        #endregion

        #region 构造与初始化

        public PathGrid(int width, int height)
        {
            _width = width;
            _height = height;
            
            int cellCount = width * height;
            _pathCosts = new int[cellCount];
            _walkable = new bool[cellCount];
            
            // 默认所有cell可通行
            for (int i = 0; i < cellCount; i++)
            {
                _pathCosts[i] = DefaultCost;
                _walkable[i] = true;
            }
        }

        #endregion

        #region 访问方法

        /// <summary>
        /// 获取路径成本
        /// </summary>
        public int GetPathCost(int x, int z)
        {
            if (!IsValid(x, z))
                return ImpassableCost;
            
            return _pathCosts[CellIndex(x, z)];
        }

        /// <summary>
        /// 设置路径成本
        /// </summary>
        public void SetPathCost(int x, int z, int cost)
        {
            if (!IsValid(x, z)) return;

            int index = CellIndex(x, z);
            _pathCosts[index] = cost;
            _walkable[index] = cost < ImpassableCost;
            
            OnPathCostChanged?.Invoke(x, z, cost);
        }

        /// <summary>
        /// 检查是否可通行
        /// </summary>
        public bool IsWalkable(int x, int z)
        {
            if (!IsValid(x, z)) return false;
            return _walkable[CellIndex(x, z)];
        }

        #endregion

        #region 批量计算

        /// <summary>
        /// 重新计算指定区域的路径成本
        /// </summary>
        public void RecalculatePathCosts(FloorRect rect, Func<int, int, int> costCalculator)
        {
            foreach (var cell in rect.Cells)
            {
                if (IsValid(cell.x, cell.y))
                {
                    int cost = costCalculator(cell.x, cell.y);
                    SetPathCost(cell.x, cell.y, cost);
                }
            }
        }

        /// <summary>
        /// 重新计算所有路径成本
        /// </summary>
        public void RecalculateAllPathCosts(Func<int, int, int> costCalculator)
        {
            for (int z = 0; z < _height; z++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int cost = costCalculator(x, z);
                    SetPathCost(x, z, cost);
                }
            }
        }

        #endregion

        #region 路径成本计算辅助

        /// <summary>
        /// 计算从from到to的移动成本
        /// </summary>
        public int CalculateMoveCost(int fromX, int fromZ, int toX, int toZ)
        {
            if (!IsWalkable(toX, toZ))
                return ImpassableCost;

            int baseCost = GetPathCost(toX, toZ);

            // 对角线移动成本更高
            bool isDiagonal = fromX != toX && fromZ != toZ;
            if (isDiagonal)
            {
                baseCost = (int)(baseCost * 1.41f); // √2 ≈ 1.41
            }

            return baseCost;
        }

        #endregion

        #region 调试与可视化

        /// <summary>
        /// 导出路径成本数据(用于调试)
        /// </summary>
        public int[] ExportPathCostData()
        {
            int[] copy = new int[_pathCosts.Length];
            Array.Copy(_pathCosts, copy, _pathCosts.Length);
            return copy;
        }

        /// <summary>
        /// 获取路径成本统计
        /// </summary>
        public (int walkable, int impassable, int avgCost) GetStatistics()
        {
            int walkableCount = 0;
            int impassableCount = 0;
            int totalCost = 0;

            for (int i = 0; i < _pathCosts.Length; i++)
            {
                if (_walkable[i])
                {
                    walkableCount++;
                    totalCost += _pathCosts[i];
                }
                else
                {
                    impassableCount++;
                }
            }

            int avgCost = walkableCount > 0 ? totalCost / walkableCount : 0;
            return (walkableCount, impassableCount, avgCost);
        }

        #endregion

        #region 序列化支持

        /// <summary>
        /// 导出数据
        /// </summary>
        public PathGridSaveData SaveData()
        {
            return new PathGridSaveData
            {
                PathCosts = ExportPathCostData()
            };
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        public void LoadData(PathGridSaveData data)
        {
            if (data.PathCosts.Length != _pathCosts.Length)
            {
                Debug.LogError("PathGrid data size mismatch");
                return;
            }

            Array.Copy(data.PathCosts, _pathCosts, data.PathCosts.Length);
            
            // 重建walkable数组
            for (int i = 0; i < _pathCosts.Length; i++)
            {
                _walkable[i] = _pathCosts[i] < ImpassableCost;
            }
        }

        #endregion

        #region 工具方法

        private int CellIndex(int x, int z) => z * _width + x;

        private bool IsValid(int x, int z)
        {
            return x >= 0 && x < _width && z >= 0 && z < _height;
        }

        #endregion

        #region 事件

        /// <summary>
        /// 路径成本改变事件
        /// </summary>
        public event Action<int, int, int> OnPathCostChanged;

        #endregion
    }

    /// <summary>
    /// PathGrid存档数据
    /// </summary>
    [Serializable]
    public class PathGridSaveData
    {
        public int[] PathCosts;
    }
}