/*******************************************************************************
 * 文件名:    Region.cs
 * 描述:      区域系统，用于寻路优化的区域划分
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Region系统将地图划分为小区域，用于：
 *   - 加速寻路（先找区域路径，再找格子路径）
 *   - 快速判断可达性
 *   - 房间检测的基础
 *   
 *   每个Region是一块连通的可通行区域，通过RegionLink连接。
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 区域（一块连通的可通行区域）
    /// </summary>
    public class Region
    {
        #region 常量

        /// <summary>
        /// 区域最大格子数
        /// </summary>
        public const int MaxCellCount = 25;

        #endregion

        #region 字段

        /// <summary>
        /// 区域ID
        /// </summary>
        private int _regionId;

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 区域类型
        /// </summary>
        private RegionType _type;

        /// <summary>
        /// 包含的格子
        /// </summary>
        private List<CellCoord> _cells;

        /// <summary>
        /// 区域连接
        /// </summary>
        private List<RegionLink> _links;

        /// <summary>
        /// 所属房间
        /// </summary>
        private Room _room;

        /// <summary>
        /// 是否有效
        /// </summary>
        private bool _valid = true;

        /// <summary>
        /// 边界缓存
        /// </summary>
        private CellCoord _boundsMin;
        private CellCoord _boundsMax;
        private bool _boundsDirty = true;

        #endregion

        #region 属性

        /// <summary>
        /// 区域ID
        /// </summary>
        public int RegionId => _regionId;

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// 楼层索引
        /// </summary>
        public int FloorIndex => _floor?.FloorIndex ?? 0;

        /// <summary>
        /// 区域类型
        /// </summary>
        public RegionType Type => _type;

        /// <summary>
        /// 包含的格子
        /// </summary>
        public IReadOnlyList<CellCoord> Cells => _cells;

        /// <summary>
        /// 格子数量
        /// </summary>
        public int CellCount => _cells.Count;

        /// <summary>
        /// 区域连接
        /// </summary>
        public IReadOnlyList<RegionLink> Links => _links;

        /// <summary>
        /// 连接数量
        /// </summary>
        public int LinkCount => _links.Count;

        /// <summary>
        /// 所属房间
        /// </summary>
        public Room Room
        {
            get => _room;
            set => _room = value;
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Valid => _valid;

        /// <summary>
        /// 是否是门区域
        /// </summary>
        public bool IsDoorway => _type == RegionType.Portal;

        /// <summary>
        /// 边界最小点
        /// </summary>
        public CellCoord BoundsMin
        {
            get
            {
                UpdateBoundsIfNeeded();
                return _boundsMin;
            }
        }

        /// <summary>
        /// 边界最大点
        /// </summary>
        public CellCoord BoundsMax
        {
            get
            {
                UpdateBoundsIfNeeded();
                return _boundsMax;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public Region(int regionId, Floor floor, RegionType type)
        {
            _regionId = regionId;
            _floor = floor;
            _type = type;
            _cells = new List<CellCoord>();
            _links = new List<RegionLink>();
        }

        #endregion

        #region 格子管理

        /// <summary>
        /// 添加格子
        /// </summary>
        public void AddCell(CellCoord cell)
        {
            if (!_cells.Contains(cell))
            {
                _cells.Add(cell);
                _boundsDirty = true;
            }
        }

        /// <summary>
        /// 检查是否包含格子
        /// </summary>
        public bool ContainsCell(CellCoord cell)
        {
            return _cells.Contains(cell);
        }

        /// <summary>
        /// 获取随机格子
        /// </summary>
        public CellCoord GetRandomCell()
        {
            if (_cells.Count == 0)
                return CellCoord.Invalid;
            return _cells[UnityEngine.Random.Range(0, _cells.Count)];
        }

        #endregion

        #region 连接管理

        /// <summary>
        /// 添加连接
        /// </summary>
        public void AddLink(RegionLink link)
        {
            if (!_links.Contains(link))
            {
                _links.Add(link);
            }
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        public void RemoveLink(RegionLink link)
        {
            _links.Remove(link);
        }

        /// <summary>
        /// 获取相邻区域
        /// </summary>
        public IEnumerable<Region> GetNeighborRegions()
        {
            foreach (var link in _links)
            {
                var other = link.GetOtherRegion(this);
                if (other != null && other.Valid)
                {
                    yield return other;
                }
            }
        }

        /// <summary>
        /// 检查是否与另一区域相邻
        /// </summary>
        public bool IsNeighbor(Region other)
        {
            return _links.Any(l => l.GetOtherRegion(this) == other);
        }

        #endregion

        #region 边界计算

        /// <summary>
        /// 更新边界（如果需要）
        /// </summary>
        private void UpdateBoundsIfNeeded()
        {
            if (!_boundsDirty || _cells.Count == 0)
                return;

            int minX = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxZ = int.MinValue;

            foreach (var cell in _cells)
            {
                minX = Mathf.Min(minX, cell.x);
                minZ = Mathf.Min(minZ, cell.z);
                maxX = Mathf.Max(maxX, cell.x);
                maxZ = Mathf.Max(maxZ, cell.z);
            }

            _boundsMin = new CellCoord(minX, minZ);
            _boundsMax = new CellCoord(maxX, maxZ);
            _boundsDirty = false;
        }

        /// <summary>
        /// 获取中心点
        /// </summary>
        public CellCoord GetCenter()
        {
            if (_cells.Count == 0)
                return CellCoord.Invalid;

            int sumX = 0, sumZ = 0;
            foreach (var cell in _cells)
            {
                sumX += cell.x;
                sumZ += cell.z;
            }

            return new CellCoord(sumX / _cells.Count, sumZ / _cells.Count);
        }

        #endregion

        #region 失效

        /// <summary>
        /// 标记为无效
        /// </summary>
        public void Invalidate()
        {
            _valid = false;

            // 从房间移除
            _room?.RemoveRegion(this);
            _room = null;

            // 清除连接
            foreach (var link in _links.ToList())
            {
                link.Deregister();
            }
            _links.Clear();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"Region[{_regionId}] {_type}, {_cells.Count} cells, {_links.Count} links";
        }

        #endregion
    }

    /// <summary>
    /// 区域类型
    /// </summary>
    public enum RegionType
    {
        /// <summary>
        /// 普通区域
        /// </summary>
        Normal,

        /// <summary>
        /// 门/通道区域
        /// </summary>
        Portal,

        /// <summary>
        /// 不可通行区域（墙内等）
        /// </summary>
        Impassable
    }

    /// <summary>
    /// 区域连接
    /// </summary>
    public class RegionLink
    {
        #region 字段

        /// <summary>
        /// 连接的两个区域
        /// </summary>
        private Region _regionA;
        private Region _regionB;

        /// <summary>
        /// 连接点（边界格子）
        /// </summary>
        private List<CellCoord> _cells;

        /// <summary>
        /// 连接类型
        /// </summary>
        private RegionLinkType _linkType;

        /// <summary>
        /// 通过代价
        /// </summary>
        private int _traverseCost;

        #endregion

        #region 属性

        /// <summary>
        /// 区域A
        /// </summary>
        public Region RegionA => _regionA;

        /// <summary>
        /// 区域B
        /// </summary>
        public Region RegionB => _regionB;

        /// <summary>
        /// 连接点
        /// </summary>
        public IReadOnlyList<CellCoord> Cells => _cells;

        /// <summary>
        /// 连接类型
        /// </summary>
        public RegionLinkType LinkType => _linkType;

        /// <summary>
        /// 通过代价
        /// </summary>
        public int TraverseCost => _traverseCost;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Valid => _regionA != null && _regionB != null && 
                            _regionA.Valid && _regionB.Valid;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public RegionLink(Region regionA, Region regionB, RegionLinkType linkType = RegionLinkType.Normal)
        {
            _regionA = regionA;
            _regionB = regionB;
            _linkType = linkType;
            _cells = new List<CellCoord>();
            _traverseCost = 1;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加连接点
        /// </summary>
        public void AddCell(CellCoord cell)
        {
            if (!_cells.Contains(cell))
            {
                _cells.Add(cell);
            }
        }

        /// <summary>
        /// 获取另一个区域
        /// </summary>
        public Region GetOtherRegion(Region from)
        {
            if (from == _regionA) return _regionB;
            if (from == _regionB) return _regionA;
            return null;
        }

        /// <summary>
        /// 注册到两个区域
        /// </summary>
        public void Register()
        {
            _regionA?.AddLink(this);
            _regionB?.AddLink(this);
        }

        /// <summary>
        /// 从两个区域注销
        /// </summary>
        public void Deregister()
        {
            _regionA?.RemoveLink(this);
            _regionB?.RemoveLink(this);
        }

        /// <summary>
        /// 获取最近的连接点
        /// </summary>
        public CellCoord GetClosestCell(CellCoord from)
        {
            if (_cells.Count == 0)
                return CellCoord.Invalid;

            CellCoord closest = _cells[0];
            int minDist = from.ManhattanDistance(closest);

            for (int i = 1; i < _cells.Count; i++)
            {
                int dist = from.ManhattanDistance(_cells[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = _cells[i];
                }
            }

            return closest;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"RegionLink: R{_regionA?.RegionId}↔R{_regionB?.RegionId}, {_cells.Count} cells";
        }

        #endregion
    }

    /// <summary>
    /// 区域连接类型
    /// </summary>
    public enum RegionLinkType
    {
        /// <summary>
        /// 普通连接
        /// </summary>
        Normal,

        /// <summary>
        /// 通过门连接
        /// </summary>
        Door,

        /// <summary>
        /// 楼层连接（楼梯等）
        /// </summary>
        FloorConnection
    }
}
