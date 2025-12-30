/*******************************************************************************
 * 文件名:    RoomManager.cs
 * 描述:      房间管理器，检测和管理所有房间
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   RoomManager 负责：
 *   - 基于区域构建房间
 *   - 管理室内/室外区分
 *   - 房间属性更新
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 房间管理器
    /// </summary>
    public class RoomManager
    {
        #region 字段

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 区域网格
        /// </summary>
        private RegionGrid _regionGrid;

        /// <summary>
        /// 所有房间
        /// </summary>
        private List<Room> _allRooms;

        /// <summary>
        /// 室外房间（特殊，只有一个）
        /// </summary>
        private Room _outdoorsRoom;

        /// <summary>
        /// 下一个房间ID
        /// </summary>
        private int _nextRoomId = 1;

        /// <summary>
        /// 是否需要重建
        /// </summary>
        private bool _needsRebuild = true;

        #endregion

        #region 属性

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// 区域网格
        /// </summary>
        public RegionGrid RegionGrid => _regionGrid;

        /// <summary>
        /// 所有房间
        /// </summary>
        public IReadOnlyList<Room> AllRooms => _allRooms;

        /// <summary>
        /// 室内房间
        /// </summary>
        public IEnumerable<Room> IndoorRooms => _allRooms.Where(r => !r.IsOutdoors);

        /// <summary>
        /// 室外房间
        /// </summary>
        public Room OutdoorsRoom => _outdoorsRoom;

        /// <summary>
        /// 房间数量
        /// </summary>
        public int RoomCount => _allRooms.Count;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public RoomManager(Floor floor, RegionGrid regionGrid)
        {
            _floor = floor;
            _regionGrid = regionGrid;
            _allRooms = new List<Room>();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 创建室外房间
            _outdoorsRoom = new Room(_nextRoomId++, _floor, isOutdoors: true);
            _allRooms.Add(_outdoorsRoom);

            _needsRebuild = true;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取格子所在房间
        /// </summary>
        public Room GetRoomAt(CellCoord cell)
        {
            var region = _regionGrid?.GetRegionAt(cell);
            return region?.Room;
        }

        /// <summary>
        /// 获取房间（通过ID）
        /// </summary>
        public Room GetRoomById(int roomId)
        {
            return _allRooms.FirstOrDefault(r => r.RoomId == roomId);
        }

        /// <summary>
        /// 检查格子是否在室内
        /// </summary>
        public bool IsIndoors(CellCoord cell)
        {
            var room = GetRoomAt(cell);
            return room != null && !room.IsOutdoors;
        }

        /// <summary>
        /// 检查格子是否在室外
        /// </summary>
        public bool IsOutdoors(CellCoord cell)
        {
            var room = GetRoomAt(cell);
            return room == null || room.IsOutdoors;
        }

        /// <summary>
        /// 获取指定角色的房间
        /// </summary>
        public IEnumerable<Room> GetRoomsByRole(RoomRole role)
        {
            return _allRooms.Where(r => r.Role == role);
        }

        #endregion

        #region 房间构建

        /// <summary>
        /// 标记需要重建
        /// </summary>
        public void MarkNeedsRebuild()
        {
            _needsRebuild = true;
        }

        /// <summary>
        /// 重建房间（如果需要）
        /// </summary>
        public void RebuildIfNeeded()
        {
            // 先确保区域已更新
            _regionGrid?.RebuildIfNeeded();

            if (!_needsRebuild)
                return;

            RebuildAllRooms();
            _needsRebuild = false;
        }

        /// <summary>
        /// 重建所有房间
        /// </summary>
        private void RebuildAllRooms()
        {
            // 清除现有房间（保留室外房间）
            foreach (var room in _allRooms.Where(r => !r.IsOutdoors).ToList())
            {
                room.Invalidate();
                _allRooms.Remove(room);
            }

            // 清除室外房间的区域
            foreach (var region in _outdoorsRoom.Regions.ToList())
            {
                _outdoorsRoom.RemoveRegion(region);
            }

            if (_regionGrid == null)
                return;

            // 遍历所有区域，分配到房间
            var unassignedRegions = new HashSet<Region>(
                _regionGrid.AllRegions.Where(r => r.Valid && r.Type != RegionType.Impassable));

            while (unassignedRegions.Count > 0)
            {
                var startRegion = unassignedRegions.First();

                // 检查是否应该是室外（无屋顶或连接到地图边缘）
                bool isOutdoors = ShouldBeOutdoors(startRegion);

                if (isOutdoors)
                {
                    // 分配到室外房间
                    var connectedRegions = FloodFillConnectedRegions(startRegion, unassignedRegions, 
                        r => ShouldBeOutdoors(r));
                    
                    foreach (var region in connectedRegions)
                    {
                        _outdoorsRoom.AddRegion(region);
                        unassignedRegions.Remove(region);
                    }
                }
                else
                {
                    // 创建新的室内房间
                    var room = new Room(_nextRoomId++, _floor, isOutdoors: false);
                    _allRooms.Add(room);

                    var connectedRegions = FloodFillConnectedRegions(startRegion, unassignedRegions,
                        r => !ShouldBeOutdoors(r));

                    foreach (var region in connectedRegions)
                    {
                        room.AddRegion(region);
                        unassignedRegions.Remove(region);
                    }

                    // 推断房间角色
                    room.InferRole();
                }
            }
        }

        /// <summary>
        /// 检查区域是否应该是室外
        /// </summary>
        private bool ShouldBeOutdoors(Region region)
        {
            if (region == null)
                return true;

            // 门区域不算室外
            if (region.Type == RegionType.Portal)
                return false;

            // 检查区域是否有屋顶
            foreach (var cell in region.Cells)
            {
                if (_floor?.HasRoof(cell) != true)
                {
                    return true; // 有无屋顶的格子，认为是室外
                }
            }

            // 检查是否在地图边缘
            foreach (var cell in region.Cells)
            {
                if (cell.x == 0 || cell.z == 0 || 
                    cell.x == _floor.SizeX - 1 || cell.z == _floor.SizeZ - 1)
                {
                    // 在边缘且无墙壁包围
                    // 简化处理：边缘格子如果没有完整包围，算室外
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Flood fill获取连通的区域
        /// </summary>
        private List<Region> FloodFillConnectedRegions(Region start, 
            HashSet<Region> available, Func<Region, bool> predicate)
        {
            var result = new List<Region>();
            var queue = new Queue<Region>();

            if (!available.Contains(start) || !predicate(start))
                return result;

            queue.Enqueue(start);
            result.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in current.GetNeighborRegions())
                {
                    if (available.Contains(neighbor) && !result.Contains(neighbor) && predicate(neighbor))
                    {
                        result.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        #endregion

        #region 房间合并/分割

        /// <summary>
        /// 通知区域变化
        /// </summary>
        public void NotifyRegionChanged(Region region)
        {
            // 简单实现：标记完全重建
            _needsRebuild = true;
        }

        /// <summary>
        /// 通知门状态变化
        /// </summary>
        public void NotifyDoorStateChanged(CellCoord doorCell)
        {
            // 门开关可能导致房间合并/分割
            _needsRebuild = true;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新（每帧或定期）
        /// </summary>
        public void Update()
        {
            RebuildIfNeeded();
        }

        /// <summary>
        /// 稀有更新
        /// </summary>
        public void UpdateRare()
        {
            // 定期失效房间属性缓存，强制重新计算
            foreach (var room in _allRooms)
            {
                room.InvalidateStatsCache();
            }
        }

        #endregion

        #region 调试

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStats()
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalRooms"] = _allRooms.Count,
                ["IndoorRooms"] = _allRooms.Count(r => !r.IsOutdoors),
                ["OutdoorRegions"] = _outdoorsRoom?.RegionCount ?? 0
            };

            // 按角色统计
            foreach (RoomRole role in Enum.GetValues(typeof(RoomRole)))
            {
                int count = _allRooms.Count(r => r.Role == role);
                if (count > 0)
                {
                    stats[$"Role_{role}"] = count;
                }
            }

            return stats;
        }

        /// <summary>
        /// 打印房间信息
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log($"=== RoomManager (Floor {_floor?.FloorIndex}) ===");
            Debug.Log($"Total Rooms: {_allRooms.Count}");
            Debug.Log($"Outdoor Room: {_outdoorsRoom}");

            foreach (var room in _allRooms.Where(r => !r.IsOutdoors))
            {
                Debug.Log($"  - {room}");
            }
        }

        #endregion
    }
}
