/*******************************************************************************
 * 文件名:    Room.cs
 * 描述:      房间系统，检测和管理封闭房间
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Room系统用于：
 *   - 检测被墙壁包围的封闭区域
 *   - 计算房间属性（温度、美观度、清洁度等）
 *   - 室内/室外判断
 *   - 房间角色分配（卧室、厨房等）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 房间
    /// </summary>
    public class Room
    {
        #region 字段

        /// <summary>
        /// 房间ID
        /// </summary>
        private int _roomId;

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 包含的区域
        /// </summary>
        private List<Region> _regions;

        /// <summary>
        /// 是否是室外
        /// </summary>
        private bool _isOutdoors;

        /// <summary>
        /// 房间角色
        /// </summary>
        private RoomRole _role;

        /// <summary>
        /// 是否有效
        /// </summary>
        private bool _valid = true;

        /// <summary>
        /// 属性缓存是否有效
        /// </summary>
        private bool _statsCacheValid;

        // 缓存的属性
        private int _cachedCellCount;
        private float _cachedBeauty;
        private float _cachedCleanliness;
        private float _cachedWealth;
        private float _cachedSpace;
        private float _cachedTemperature;
        private bool _cachedHasRoof;

        #endregion

        #region 属性

        /// <summary>
        /// 房间ID
        /// </summary>
        public int RoomId => _roomId;

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// 楼层索引
        /// </summary>
        public int FloorIndex => _floor?.FloorIndex ?? 0;

        /// <summary>
        /// 包含的区域
        /// </summary>
        public IReadOnlyList<Region> Regions => _regions;

        /// <summary>
        /// 区域数量
        /// </summary>
        public int RegionCount => _regions.Count;

        /// <summary>
        /// 是否是室外
        /// </summary>
        public bool IsOutdoors => _isOutdoors;

        /// <summary>
        /// 是否是室内
        /// </summary>
        public bool IsIndoors => !_isOutdoors;

        /// <summary>
        /// 房间角色
        /// </summary>
        public RoomRole Role
        {
            get => _role;
            set => _role = value;
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Valid => _valid;

        /// <summary>
        /// 格子数量
        /// </summary>
        public int CellCount
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedCellCount;
            }
        }

        /// <summary>
        /// 美观度
        /// </summary>
        public float Beauty
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedBeauty;
            }
        }

        /// <summary>
        /// 清洁度
        /// </summary>
        public float Cleanliness
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedCleanliness;
            }
        }

        /// <summary>
        /// 财富值
        /// </summary>
        public float Wealth
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedWealth;
            }
        }

        /// <summary>
        /// 空间感（每格）
        /// </summary>
        public float SpacePerCell
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedSpace;
            }
        }

        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedTemperature;
            }
        }

        /// <summary>
        /// 是否有完整屋顶
        /// </summary>
        public bool HasRoof
        {
            get
            {
                EnsureStatsCacheValid();
                return _cachedHasRoof;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public Room(int roomId, Floor floor, bool isOutdoors = false)
        {
            _roomId = roomId;
            _floor = floor;
            _isOutdoors = isOutdoors;
            _regions = new List<Region>();
            _role = RoomRole.None;
        }

        #endregion

        #region 区域管理

        /// <summary>
        /// 添加区域
        /// </summary>
        public void AddRegion(Region region)
        {
            if (region == null || _regions.Contains(region))
                return;

            _regions.Add(region);
            region.Room = this;
            InvalidateStatsCache();
        }

        /// <summary>
        /// 移除区域
        /// </summary>
        public void RemoveRegion(Region region)
        {
            if (region == null)
                return;

            if (_regions.Remove(region))
            {
                if (region.Room == this)
                    region.Room = null;
                InvalidateStatsCache();
            }
        }

        /// <summary>
        /// 检查是否包含区域
        /// </summary>
        public bool ContainsRegion(Region region)
        {
            return _regions.Contains(region);
        }

        /// <summary>
        /// 检查是否包含格子
        /// </summary>
        public bool ContainsCell(CellCoord cell)
        {
            foreach (var region in _regions)
            {
                if (region.ContainsCell(cell))
                    return true;
            }
            return false;
        }

        #endregion

        #region 格子遍历

        /// <summary>
        /// 获取所有格子
        /// </summary>
        public IEnumerable<CellCoord> GetAllCells()
        {
            foreach (var region in _regions)
            {
                foreach (var cell in region.Cells)
                {
                    yield return cell;
                }
            }
        }

        /// <summary>
        /// 获取随机格子
        /// </summary>
        public CellCoord GetRandomCell()
        {
            if (_regions.Count == 0)
                return CellCoord.Invalid;

            var region = _regions[UnityEngine.Random.Range(0, _regions.Count)];
            return region.GetRandomCell();
        }

        /// <summary>
        /// 获取边界格子
        /// </summary>
        public IEnumerable<CellCoord> GetBorderCells()
        {
            var allCells = new HashSet<CellCoord>(GetAllCells());

            foreach (var cell in allCells)
            {
                // 检查是否是边界（有邻居不在房间内）
                foreach (var dir in DirectionExtensions.CardinalDirections)
                {
                    var neighbor = cell + dir.ToOffset();
                    if (!allCells.Contains(neighbor))
                    {
                        yield return cell;
                        break;
                    }
                }
            }
        }

        #endregion

        #region 属性计算

        /// <summary>
        /// 失效属性缓存
        /// </summary>
        public void InvalidateStatsCache()
        {
            _statsCacheValid = false;
        }

        /// <summary>
        /// 确保属性缓存有效
        /// </summary>
        private void EnsureStatsCacheValid()
        {
            if (_statsCacheValid)
                return;

            RecalculateStats();
            _statsCacheValid = true;
        }

        /// <summary>
        /// 重新计算所有属性
        /// </summary>
        private void RecalculateStats()
        {
            _cachedCellCount = 0;
            _cachedBeauty = 0;
            _cachedCleanliness = 0;
            _cachedWealth = 0;
            _cachedTemperature = 21f; // 默认温度
            _cachedHasRoof = true;

            if (_isOutdoors)
            {
                _cachedHasRoof = false;
                // 室外不计算详细属性
                foreach (var region in _regions)
                {
                    _cachedCellCount += region.CellCount;
                }
                return;
            }

            float totalBeauty = 0;
            float totalCleanliness = 0;
            float totalWealth = 0;
            int roofedCells = 0;

            foreach (var region in _regions)
            {
                foreach (var cell in region.Cells)
                {
                    _cachedCellCount++;

                    // 地板美观度和清洁度
                    var floorDef = _floor?.GetFloor(cell);
                    if (floorDef != null)
                    {
                        totalBeauty += floorDef.Beauty;
                        totalCleanliness += floorDef.Cleanliness;
                    }

                    // 覆盖物清洁度
                    var coverGrid = _floor?.CoverGrid;
                    var coverDef = coverGrid?.GetDef<CoverDef>(cell);
                    if (coverDef != null)
                    {
                        totalCleanliness += coverDef.Cleanliness;
                    }

                    // 检查屋顶
                    if (_floor?.HasRoof(cell) == true)
                    {
                        roofedCells++;
                    }

                    // TODO: 计算实体的美观度、财富值等
                    // var entities = _floor?.EntityGrid?.GetEntitiesAt(cell);
                    // foreach (var entity in entities) { ... }
                }
            }

            if (_cachedCellCount > 0)
            {
                _cachedBeauty = totalBeauty / _cachedCellCount;
                _cachedCleanliness = totalCleanliness / _cachedCellCount;
                _cachedWealth = totalWealth;
                _cachedSpace = CalculateSpaceScore();
                _cachedHasRoof = roofedCells >= _cachedCellCount * 0.8f; // 80%以上有屋顶
            }
        }

        /// <summary>
        /// 计算空间感评分
        /// </summary>
        private float CalculateSpaceScore()
        {
            // 基于房间大小的空间感评分
            // 小房间（<9格）感觉拥挤
            // 中等房间（9-25格）感觉正常
            // 大房间（>25格）感觉宽敞
            if (_cachedCellCount < 9)
                return 0.5f + (_cachedCellCount / 18f);
            if (_cachedCellCount < 25)
                return 1f;
            return 1f + Mathf.Min(0.5f, (_cachedCellCount - 25) / 50f);
        }

        #endregion

        #region 角色推断

        /// <summary>
        /// 自动推断房间角色
        /// </summary>
        public void InferRole()
        {
            if (_isOutdoors)
            {
                _role = RoomRole.None;
                return;
            }

            // TODO: 根据房间内的建筑推断角色
            // 例如：有床 -> 卧室，有炉灶 -> 厨房，等等

            // 简单实现：按大小判断
            if (_cachedCellCount < 6)
                _role = RoomRole.None; // 太小
            else if (_cachedCellCount < 16)
                _role = RoomRole.Bedroom; // 小房间默认卧室
            else
                _role = RoomRole.DiningRoom; // 大房间默认餐厅
        }

        #endregion

        #region 失效

        /// <summary>
        /// 标记为无效
        /// </summary>
        public void Invalidate()
        {
            _valid = false;

            foreach (var region in _regions)
            {
                if (region.Room == this)
                    region.Room = null;
            }
            _regions.Clear();
        }

        #endregion

        #region 邻居房间

        /// <summary>
        /// 获取相邻房间
        /// </summary>
        public IEnumerable<Room> GetNeighborRooms()
        {
            var found = new HashSet<Room>();

            foreach (var region in _regions)
            {
                foreach (var neighborRegion in region.GetNeighborRegions())
                {
                    var neighborRoom = neighborRegion.Room;
                    if (neighborRoom != null && neighborRoom != this && found.Add(neighborRoom))
                    {
                        yield return neighborRoom;
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否与另一房间相邻
        /// </summary>
        public bool IsNeighbor(Room other)
        {
            if (other == null || other == this)
                return false;

            foreach (var region in _regions)
            {
                foreach (var neighborRegion in region.GetNeighborRegions())
                {
                    if (neighborRegion.Room == other)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            string typeStr = _isOutdoors ? "Outdoors" : "Indoor";
            return $"Room[{_roomId}] {typeStr}, {_regions.Count} regions, {CellCount} cells, {_role}";
        }

        #endregion
    }

    /// <summary>
    /// 房间角色
    /// </summary>
    public enum RoomRole
    {
        /// <summary>
        /// 无特定用途
        /// </summary>
        None,

        /// <summary>
        /// 卧室
        /// </summary>
        Bedroom,

        /// <summary>
        /// 病房
        /// </summary>
        Hospital,

        /// <summary>
        /// 监狱
        /// </summary>
        Prison,

        /// <summary>
        /// 餐厅
        /// </summary>
        DiningRoom,

        /// <summary>
        /// 娱乐室
        /// </summary>
        RecRoom,

        /// <summary>
        /// 厨房
        /// </summary>
        Kitchen,

        /// <summary>
        /// 仓库
        /// </summary>
        Storage,

        /// <summary>
        /// 研究室
        /// </summary>
        Research,

        /// <summary>
        /// 工作间
        /// </summary>
        Workshop,

        /// <summary>
        /// 兵营
        /// </summary>
        Barracks,

        /// <summary>
        /// 大厅/走廊
        /// </summary>
        Hallway,

        /// <summary>
        /// 寺庙/祈祷室
        /// </summary>
        Temple
    }
}
