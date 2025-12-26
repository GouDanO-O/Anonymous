using System;
using System.Collections.Generic;
using Core.Game.Map.Tile.Data;
using UnityEngine;

namespace Core.Game.Map.Tile
{
    /// <summary>
    /// 连通区域 - 一组相互连通的cell集合
    /// 参照RimWorld的Region设计,用于优化寻路
    /// </summary>
    public class Region
    {
        public int RegionId { get; private set; }
        public HashSet<Vector2Int> Cells { get; private set; }
        public HashSet<Region> Neighbors { get; private set; }
        
        // 区域类型
        public RegionType Type { get; set; }
        
        // 是否有效
        public bool Valid { get; private set; }

        public Region(int regionId)
        {
            RegionId = regionId;
            Cells = new HashSet<Vector2Int>();
            Neighbors = new HashSet<Region>();
            Type = RegionType.Normal;
            Valid = true;
        }

        /// <summary>
        /// 添加cell到区域
        /// </summary>
        public void AddCell(Vector2Int cell)
        {
            Cells.Add(cell);
        }

        /// <summary>
        /// 添加邻居区域
        /// </summary>
        public void AddNeighbor(Region neighbor)
        {
            if (neighbor != null && neighbor != this)
            {
                Neighbors.Add(neighbor);
            }
        }

        /// <summary>
        /// 检查是否与目标区域连通
        /// </summary>
        public bool IsConnectedTo(Region target)
        {
            if (target == this) return true;
            if (Neighbors.Contains(target)) return true;
            
            // BFS搜索连通性
            HashSet<Region> visited = new HashSet<Region> { this };
            Queue<Region> queue = new Queue<Region>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                Region current = queue.Dequeue();
                
                foreach (Region neighbor in current.Neighbors)
                {
                    if (neighbor == target)
                        return true;
                    
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 使区域无效
        /// </summary>
        public void Invalidate()
        {
            Valid = false;
        }
    }

    /// <summary>
    /// 区域类型
    /// </summary>
    public enum RegionType
    {
        Normal,      // 正常可通行区域
        Impassable,  // 不可通行区域
        Door         // 门(特殊处理)
    }

    /// <summary>
    /// 连通区域网格 - 管理整个楼层的Region划分
    /// 参照RimWorld的RegionGrid设计
    /// </summary>
    public class RegionGrid
    {
        private readonly int _width;
        private readonly int _height;
        
        // 每个cell所属的Region
        private Region[] _cellRegions;
        
        // 所有Region列表
        private List<Region> _allRegions;
        
        // Region ID计数器
        private int _nextRegionId;
        
        // 脏标记 - 需要重建
        private bool _dirty;

        #region 构造与初始化

        public RegionGrid(int width, int height)
        {
            _width = width;
            _height = height;
            
            int cellCount = width * height;
            _cellRegions = new Region[cellCount];
            _allRegions = new List<Region>();
            _nextRegionId = 1;
            _dirty = true;
        }

        #endregion

        #region Region查询

        /// <summary>
        /// 获取cell所属的Region
        /// </summary>
        public Region GetRegionAt(int x, int z)
        {
            if (!IsValid(x, z)) return null;
            return _cellRegions[CellIndex(x, z)];
        }

        /// <summary>
        /// 获取所有有效Region
        /// </summary>
        public List<Region> GetAllValidRegions()
        {
            return _allRegions.FindAll(r => r.Valid);
        }

        #endregion

        #region 可达性查询

        /// <summary>
        /// 检查两个位置是否可达
        /// </summary>
        public bool IsReachable(int fromX, int fromZ, int toX, int toZ)
        {
            Region fromRegion = GetRegionAt(fromX, fromZ);
            Region toRegion = GetRegionAt(toX, toZ);
            
            if (fromRegion == null || toRegion == null)
                return false;
            
            return fromRegion.IsConnectedTo(toRegion);
        }

        #endregion

        #region Region重建

        /// <summary>
        /// 标记为脏,需要重建
        /// </summary>
        public void MarkDirty()
        {
            _dirty = true;
        }

        /// <summary>
        /// 重建所有Region
        /// </summary>
        public void RebuildAllRegions(PathGrid pathGrid)
        {
            if (!_dirty) return;

            // 清空现有Region
            _allRegions.Clear();
            Array.Clear(_cellRegions, 0, _cellRegions.Length);
            _nextRegionId = 1;

            // 使用Flood Fill算法构建Region
            bool[] visited = new bool[_width * _height];

            for (int z = 0; z < _height; z++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = CellIndex(x, z);
                    
                    if (visited[index])
                        continue;
                    
                    // 如果cell不可通行,跳过
                    if (!pathGrid.IsWalkable(x, z))
                    {
                        visited[index] = true;
                        continue;
                    }

                    // 创建新Region并Flood Fill
                    Region newRegion = new Region(_nextRegionId++);
                    FloodFillRegion(x, z, newRegion, pathGrid, visited);
                    
                    _allRegions.Add(newRegion);
                }
            }

            // 建立Region之间的邻接关系
            BuildRegionNeighbors();

            _dirty = false;
            
            Debug.Log($"RegionGrid rebuilt: {_allRegions.Count} regions created");
        }

        /// <summary>
        /// Flood Fill构建单个Region
        /// </summary>
        private void FloodFillRegion(int startX, int startZ, Region region, PathGrid pathGrid, bool[] visited)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startZ));
            visited[CellIndex(startX, startZ)] = true;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                // 添加到Region
                region.AddCell(current);
                _cellRegions[CellIndex(current.x, current.y)] = region;

                // 检查四个方向的邻居
                Vector2Int[] neighbors = new[]
                {
                    new Vector2Int(current.x, current.y + 1), // North
                    new Vector2Int(current.x, current.y - 1), // South
                    new Vector2Int(current.x + 1, current.y), // East
                    new Vector2Int(current.x - 1, current.y)  // West
                };

                foreach (Vector2Int neighbor in neighbors)
                {
                    if (!IsValid(neighbor.x, neighbor.y))
                        continue;

                    int neighborIndex = CellIndex(neighbor.x, neighbor.y);
                    
                    if (visited[neighborIndex])
                        continue;
                    
                    if (!pathGrid.IsWalkable(neighbor.x, neighbor.y))
                        continue;

                    visited[neighborIndex] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        /// <summary>
        /// 建立Region之间的邻接关系
        /// </summary>
        private void BuildRegionNeighbors()
        {
            foreach (Region region in _allRegions)
            {
                HashSet<Region> neighborRegions = new HashSet<Region>();

                foreach (Vector2Int cell in region.Cells)
                {
                    // 检查四个方向
                    Vector2Int[] neighbors = new[]
                    {
                        new Vector2Int(cell.x, cell.y + 1),
                        new Vector2Int(cell.x, cell.y - 1),
                        new Vector2Int(cell.x + 1, cell.y),
                        new Vector2Int(cell.x - 1, cell.y)
                    };

                    foreach (Vector2Int neighbor in neighbors)
                    {
                        Region neighborRegion = GetRegionAt(neighbor.x, neighbor.y);
                        
                        if (neighborRegion != null && neighborRegion != region)
                        {
                            neighborRegions.Add(neighborRegion);
                        }
                    }
                }

                // 添加所有邻居
                foreach (Region neighbor in neighborRegions)
                {
                    region.AddNeighbor(neighbor);
                }
            }
        }

        #endregion

        #region 局部更新

        /// <summary>
        /// 更新指定区域的Region (当地形改变时)
        /// </summary>
        public void UpdateRegionsInRect(FloorRect rect, PathGrid pathGrid)
        {
            // 简化版本: 直接标记为脏
            // 完整版本应该只重建受影响的Region
            MarkDirty();
        }

        #endregion

        #region 工具方法

        private int CellIndex(int x, int z) => z * _width + x;

        private bool IsValid(int x, int z)
        {
            return x >= 0 && x < _width && z >= 0 && z < _height;
        }

        #endregion

        #region 调试

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            int validRegions = GetAllValidRegions().Count;
            int totalCells = 0;
            
            foreach (Region region in _allRegions)
            {
                if (region.Valid)
                    totalCells += region.Cells.Count;
            }

            return $"RegionGrid: {validRegions} valid regions, {totalCells} total cells";
        }

        #endregion
    }
}