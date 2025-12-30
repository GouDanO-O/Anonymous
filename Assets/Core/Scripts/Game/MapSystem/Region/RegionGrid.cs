/*******************************************************************************
 * 文件名:    RegionGrid.cs
 * 描述:      区域网格，管理楼层的区域划分
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   RegionGrid 为每个楼层管理区域划分：
 *   - 使用flood fill算法划分区域
 *   - 维护格子到区域的映射
 *   - 支持区域的动态更新
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 区域网格
    /// </summary>
    public class RegionGrid
    {
        #region 字段

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 尺寸
        /// </summary>
        private int _sizeX;
        private int _sizeZ;

        /// <summary>
        /// 格子到区域的映射
        /// </summary>
        private Region[] _regionGrid;

        /// <summary>
        /// 所有区域
        /// </summary>
        private List<Region> _allRegions;

        /// <summary>
        /// 所有区域连接
        /// </summary>
        private List<RegionLink> _allLinks;

        /// <summary>
        /// 下一个区域ID
        /// </summary>
        private int _nextRegionId = 1;

        /// <summary>
        /// 需要重建的格子
        /// </summary>
        private HashSet<CellCoord> _dirtyCells;

        /// <summary>
        /// 是否需要完全重建
        /// </summary>
        private bool _needsFullRebuild = true;

        #endregion

        #region 属性

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// 所有区域
        /// </summary>
        public IReadOnlyList<Region> AllRegions => _allRegions;

        /// <summary>
        /// 区域数量
        /// </summary>
        public int RegionCount => _allRegions.Count;

        /// <summary>
        /// 所有连接
        /// </summary>
        public IReadOnlyList<RegionLink> AllLinks => _allLinks;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public RegionGrid(Floor floor)
        {
            _floor = floor;
            _sizeX = floor.SizeX;
            _sizeZ = floor.SizeZ;
            _regionGrid = new Region[_sizeX * _sizeZ];
            _allRegions = new List<Region>();
            _allLinks = new List<RegionLink>();
            _dirtyCells = new HashSet<CellCoord>();
        }

        #endregion

        #region 索引转换

        /// <summary>
        /// 坐标转索引
        /// </summary>
        private int ToIndex(CellCoord cell)
        {
            return cell.z * _sizeX + cell.x;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        private int ToIndex(int x, int z)
        {
            return z * _sizeX + x;
        }

        /// <summary>
        /// 检查坐标有效性
        /// </summary>
        private bool InBounds(CellCoord cell)
        {
            return cell.x >= 0 && cell.x < _sizeX && 
                   cell.z >= 0 && cell.z < _sizeZ;
        }

        #endregion

        #region 区域查询

        /// <summary>
        /// 获取格子所在区域
        /// </summary>
        public Region GetRegionAt(CellCoord cell)
        {
            if (!InBounds(cell))
                return null;
            return _regionGrid[ToIndex(cell)];
        }

        /// <summary>
        /// 获取格子所在区域
        /// </summary>
        public Region GetRegionAt(int x, int z)
        {
            if (x < 0 || x >= _sizeX || z < 0 || z >= _sizeZ)
                return null;
            return _regionGrid[ToIndex(x, z)];
        }

        /// <summary>
        /// 获取区域（通过ID）
        /// </summary>
        public Region GetRegionById(int regionId)
        {
            return _allRegions.FirstOrDefault(r => r.RegionId == regionId);
        }

        /// <summary>
        /// 检查两个格子是否在同一区域
        /// </summary>
        public bool InSameRegion(CellCoord a, CellCoord b)
        {
            var regionA = GetRegionAt(a);
            var regionB = GetRegionAt(b);
            return regionA != null && regionA == regionB;
        }

        #endregion

        #region 可达性检查

        /// <summary>
        /// 检查两个格子是否可达（同楼层）
        /// </summary>
        public bool CanReach(CellCoord from, CellCoord to)
        {
            var regionFrom = GetRegionAt(from);
            var regionTo = GetRegionAt(to);

            if (regionFrom == null || regionTo == null)
                return false;

            if (regionFrom == regionTo)
                return true;

            // BFS搜索区域连通性
            return AreRegionsConnected(regionFrom, regionTo);
        }

        /// <summary>
        /// 检查两个区域是否连通
        /// </summary>
        public bool AreRegionsConnected(Region from, Region to)
        {
            if (from == null || to == null)
                return false;

            if (from == to)
                return true;

            var visited = new HashSet<Region> { from };
            var queue = new Queue<Region>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in current.GetNeighborRegions())
                {
                    if (neighbor == to)
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
        /// 获取可达的所有区域
        /// </summary>
        public HashSet<Region> GetReachableRegions(Region from)
        {
            var result = new HashSet<Region>();
            if (from == null)
                return result;

            var queue = new Queue<Region>();
            queue.Enqueue(from);
            result.Add(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in current.GetNeighborRegions())
                {
                    if (result.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        #endregion

        #region 脏标记

        /// <summary>
        /// 标记格子需要重建
        /// </summary>
        public void MarkDirty(CellCoord cell)
        {
            if (InBounds(cell))
            {
                _dirtyCells.Add(cell);
            }
        }

        /// <summary>
        /// 标记区域需要重建
        /// </summary>
        public void MarkDirty(CellCoord min, CellCoord max)
        {
            for (int z = min.z; z <= max.z; z++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    MarkDirty(new CellCoord(x, z));
                }
            }
        }

        /// <summary>
        /// 标记需要完全重建
        /// </summary>
        public void MarkFullRebuild()
        {
            _needsFullRebuild = true;
        }

        #endregion

        #region 区域构建

        /// <summary>
        /// 重建区域（如果需要）
        /// </summary>
        public void RebuildIfNeeded()
        {
            if (_needsFullRebuild)
            {
                RebuildAll();
                return;
            }

            if (_dirtyCells.Count > 0)
            {
                RebuildDirty();
            }
        }

        /// <summary>
        /// 完全重建所有区域
        /// </summary>
        public void RebuildAll()
        {
            // 清除所有区域
            foreach (var region in _allRegions)
            {
                region.Invalidate();
            }
            _allRegions.Clear();
            _allLinks.Clear();
            Array.Clear(_regionGrid, 0, _regionGrid.Length);
            _nextRegionId = 1;

            // Flood fill构建区域
            for (int z = 0; z < _sizeZ; z++)
            {
                for (int x = 0; x < _sizeX; x++)
                {
                    var cell = new CellCoord(x, z);
                    if (_regionGrid[ToIndex(cell)] == null)
                    {
                        FloodFillRegion(cell);
                    }
                }
            }

            // 构建区域连接
            BuildRegionLinks();

            _needsFullRebuild = false;
            _dirtyCells.Clear();
        }

        /// <summary>
        /// 重建脏区域
        /// </summary>
        private void RebuildDirty()
        {
            // 收集需要重建的区域
            var regionsToRebuild = new HashSet<Region>();
            foreach (var cell in _dirtyCells)
            {
                var region = GetRegionAt(cell);
                if (region != null)
                {
                    regionsToRebuild.Add(region);
                }
            }

            // 失效这些区域
            foreach (var region in regionsToRebuild)
            {
                InvalidateRegion(region);
            }

            // 重建脏格子的区域
            foreach (var cell in _dirtyCells)
            {
                if (_regionGrid[ToIndex(cell)] == null)
                {
                    FloodFillRegion(cell);
                }
            }

            // 重建受影响区域的连接
            RebuildLinksForDirtyRegions(regionsToRebuild);

            _dirtyCells.Clear();
        }

        /// <summary>
        /// 失效区域
        /// </summary>
        private void InvalidateRegion(Region region)
        {
            if (region == null)
                return;

            // 清除格子映射
            foreach (var cell in region.Cells)
            {
                if (InBounds(cell))
                {
                    _regionGrid[ToIndex(cell)] = null;
                }
            }

            // 移除连接
            foreach (var link in region.Links.ToList())
            {
                _allLinks.Remove(link);
                link.Deregister();
            }

            region.Invalidate();
            _allRegions.Remove(region);
        }

        /// <summary>
        /// Flood fill构建单个区域
        /// </summary>
        private void FloodFillRegion(CellCoord start)
        {
            // 获取起始格子的通行性
            var passability = GetCellPassability(start);
            var regionType = passability == Passability.Impassable 
                ? RegionType.Impassable 
                : RegionType.Normal;

            // 检查是否是门
            if (IsDoorCell(start))
            {
                regionType = RegionType.Portal;
            }

            var region = new Region(_nextRegionId++, _floor, regionType);
            _allRegions.Add(region);

            var queue = new Queue<CellCoord>();
            queue.Enqueue(start);
            _regionGrid[ToIndex(start)] = region;
            region.AddCell(start);

            while (queue.Count > 0 && region.CellCount < Region.MaxCellCount)
            {
                var current = queue.Dequeue();

                // 检查四个邻居
                foreach (var dir in DirectionExtensions.CardinalDirections)
                {
                    var neighbor = current + dir.ToOffset();

                    if (!InBounds(neighbor))
                        continue;

                    if (_regionGrid[ToIndex(neighbor)] != null)
                        continue;

                    // 检查是否可以扩展到这个格子
                    if (CanExpandTo(current, neighbor, regionType))
                    {
                        _regionGrid[ToIndex(neighbor)] = region;
                        region.AddCell(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否可以扩展到邻居格子
        /// </summary>
        private bool CanExpandTo(CellCoord from, CellCoord to, RegionType regionType)
        {
            var toPassability = GetCellPassability(to);
            bool toIsDoor = IsDoorCell(to);

            // 不可通行区域只扩展到不可通行格子
            if (regionType == RegionType.Impassable)
            {
                return toPassability == Passability.Impassable && !toIsDoor;
            }

            // 门区域不扩展
            if (regionType == RegionType.Portal)
            {
                return false;
            }

            // 普通区域不扩展到不可通行或门
            if (toPassability == Passability.Impassable || toIsDoor)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取格子通行性
        /// </summary>
        private Passability GetCellPassability(CellCoord cell)
        {
            // 组合Tile和Entity的通行性
            var tilePass = _floor?.GetPassability(cell) ?? Passability.Passable;
            var entityGrid = _floor?.EntityGrid;
            var entityPass = entityGrid?.GetEntityPassability(cell) ?? Passability.Passable;

            return (Passability)Mathf.Max((int)tilePass, (int)entityPass);
        }

        /// <summary>
        /// 检查是否是门格子
        /// </summary>
        private bool IsDoorCell(CellCoord cell)
        {
            var wall = _floor?.GetWall(cell);
            return wall != null && wall.IsDoor;
        }

        #endregion

        #region 区域连接构建

        /// <summary>
        /// 构建所有区域连接
        /// </summary>
        private void BuildRegionLinks()
        {
            _allLinks.Clear();

            // 遍历所有格子，找边界
            for (int z = 0; z < _sizeZ; z++)
            {
                for (int x = 0; x < _sizeX; x++)
                {
                    var cell = new CellCoord(x, z);
                    var region = _regionGrid[ToIndex(cell)];

                    if (region == null || region.Type == RegionType.Impassable)
                        continue;

                    // 检查东边和北边的邻居（避免重复）
                    CheckAndCreateLink(cell, new CellCoord(x + 1, z), region);
                    CheckAndCreateLink(cell, new CellCoord(x, z + 1), region);
                }
            }
        }

        /// <summary>
        /// 检查并创建连接
        /// </summary>
        private void CheckAndCreateLink(CellCoord cellA, CellCoord cellB, Region regionA)
        {
            if (!InBounds(cellB))
                return;

            var regionB = _regionGrid[ToIndex(cellB)];
            if (regionB == null || regionB == regionA)
                return;

            if (regionB.Type == RegionType.Impassable)
                return;

            // 检查是否已有连接
            var existingLink = _allLinks.FirstOrDefault(l =>
                (l.RegionA == regionA && l.RegionB == regionB) ||
                (l.RegionA == regionB && l.RegionB == regionA));

            if (existingLink != null)
            {
                existingLink.AddCell(cellA);
                existingLink.AddCell(cellB);
            }
            else
            {
                var linkType = (regionA.IsDoorway || regionB.IsDoorway) 
                    ? RegionLinkType.Door 
                    : RegionLinkType.Normal;

                var link = new RegionLink(regionA, regionB, linkType);
                link.AddCell(cellA);
                link.AddCell(cellB);
                link.Register();
                _allLinks.Add(link);
            }
        }

        /// <summary>
        /// 重建脏区域的连接
        /// </summary>
        private void RebuildLinksForDirtyRegions(HashSet<Region> affectedRegions)
        {
            // 移除相关连接
            var linksToRemove = _allLinks.Where(l =>
                affectedRegions.Contains(l.RegionA) || affectedRegions.Contains(l.RegionB)).ToList();

            foreach (var link in linksToRemove)
            {
                _allLinks.Remove(link);
                link.Deregister();
            }

            // 重建连接
            foreach (var region in _allRegions)
            {
                if (region.Type == RegionType.Impassable)
                    continue;

                foreach (var cell in region.Cells)
                {
                    CheckAndCreateLink(cell, new CellCoord(cell.x + 1, cell.z), region);
                    CheckAndCreateLink(cell, new CellCoord(cell.x, cell.z + 1), region);
                }
            }
        }

        #endregion

        #region 调试

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStats()
        {
            return new Dictionary<string, int>
            {
                ["TotalRegions"] = _allRegions.Count,
                ["NormalRegions"] = _allRegions.Count(r => r.Type == RegionType.Normal),
                ["PortalRegions"] = _allRegions.Count(r => r.Type == RegionType.Portal),
                ["ImpassableRegions"] = _allRegions.Count(r => r.Type == RegionType.Impassable),
                ["TotalLinks"] = _allLinks.Count,
                ["DirtyCells"] = _dirtyCells.Count
            };
        }

        #endregion
    }
}
