using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 房间数据
    /// </summary>
    [Serializable]
    public class RoomData
    {
        #region 基础属性

        /// <summary>
        /// 房间ID
        /// </summary>
        public int RoomId;

        /// <summary>
        /// 所在楼层
        /// </summary>
        public int Floor;

        /// <summary>
        /// 房间名称
        /// </summary>
        public string Name;

        #endregion

        #region 格子数据

        /// <summary>
        /// 房间包含的所有格子坐标
        /// </summary>
        public List<Vector2Int> Cells;

        /// <summary>
        /// 房间边界的最小X
        /// </summary>
        public int MinX { get; private set; }

        /// <summary>
        /// 房间边界的最大X
        /// </summary>
        public int MaxX { get; private set; }

        /// <summary>
        /// 房间边界的最小Y
        /// </summary>
        public int MinY { get; private set; }

        /// <summary>
        /// 房间边界的最大Y
        /// </summary>
        public int MaxY { get; private set; }

        #endregion

        #region 属性访问器

        /// <summary>
        /// 房间面积（格子数）
        /// </summary>
        public int Area => Cells?.Count ?? 0;

        /// <summary>
        /// 房间宽度
        /// </summary>
        public int Width => MaxX - MinX + 1;

        /// <summary>
        /// 房间高度
        /// </summary>
        public int Height => MaxY - MinY + 1;

        /// <summary>
        /// 房间中心坐标
        /// </summary>
        public Vector2 Center => new Vector2((MinX + MaxX) / 2f, (MinY + MaxY) / 2f);

        #endregion

        #region 构造函数

        public RoomData()
        {
            Cells = new List<Vector2Int>();
            MinX = int.MaxValue;
            MaxX = int.MinValue;
            MinY = int.MaxValue;
            MaxY = int.MinValue;
        }

        public RoomData(int roomId, int floor) : this()
        {
            RoomId = roomId;
            Floor = floor;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加格子
        /// </summary>
        public void AddCell(Vector2Int pos)
        {
            Cells ??= new List<Vector2Int>();

            if (!Cells.Contains(pos))
            {
                Cells.Add(pos);
                UpdateBounds(pos);
            }
        }

        /// <summary>
        /// 添加格子
        /// </summary>
        public void AddCell(int x, int y)
        {
            AddCell(new Vector2Int(x, y));
        }

        /// <summary>
        /// 移除格子
        /// </summary>
        public bool RemoveCell(Vector2Int pos)
        {
            bool removed = Cells?.Remove(pos) ?? false;
            if (removed)
            {
                RecalculateBounds();
            }
            return removed;
        }

        /// <summary>
        /// 检查坐标是否在房间内
        /// </summary>
        public bool ContainsCell(Vector2Int pos)
        {
            return Cells?.Contains(pos) ?? false;
        }

        /// <summary>
        /// 检查坐标是否在房间内
        /// </summary>
        public bool ContainsCell(int x, int y)
        {
            return ContainsCell(new Vector2Int(x, y));
        }

        /// <summary>
        /// 清空房间
        /// </summary>
        public void Clear()
        {
            Cells?.Clear();
            MinX = int.MaxValue;
            MaxX = int.MinValue;
            MinY = int.MaxValue;
            MaxY = int.MinValue;
        }

        /// <summary>
        /// 更新边界
        /// </summary>
        private void UpdateBounds(Vector2Int pos)
        {
            MinX = Math.Min(MinX, pos.x);
            MaxX = Math.Max(MaxX, pos.x);
            MinY = Math.Min(MinY, pos.y);
            MaxY = Math.Max(MaxY, pos.y);
        }

        /// <summary>
        /// 重新计算边界
        /// </summary>
        private void RecalculateBounds()
        {
            MinX = int.MaxValue;
            MaxX = int.MinValue;
            MinY = int.MaxValue;
            MaxY = int.MinValue;

            if (Cells != null)
            {
                foreach (var pos in Cells)
                {
                    UpdateBounds(pos);
                }
            }
        }

        public override string ToString()
        {
            return $"Room({RoomId}, Floor={Floor}, Area={Area})";
        }

        #endregion
    }

    /// <summary>
    /// 单层楼的房间数据集合
    /// </summary>
    [Serializable]
    public class FloorRoomData
    {
        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor;

        /// <summary>
        /// 房间列表
        /// </summary>
        public List<RoomData> Rooms;

        /// <summary>
        /// 房间ID到房间数据的映射
        /// </summary>
        [NonSerialized]
        private Dictionary<int, RoomData> _roomMap;

        public FloorRoomData()
        {
            Rooms = new List<RoomData>();
            _roomMap = new Dictionary<int, RoomData>();
        }

        public FloorRoomData(int floor) : this()
        {
            Floor = floor;
        }

        /// <summary>
        /// 添加房间
        /// </summary>
        public void AddRoom(RoomData room)
        {
            if (room == null) return;

            Rooms ??= new List<RoomData>();
            _roomMap ??= new Dictionary<int, RoomData>();

            if (!_roomMap.ContainsKey(room.RoomId))
            {
                Rooms.Add(room);
                _roomMap[room.RoomId] = room;
            }
        }

        /// <summary>
        /// 获取房间
        /// </summary>
        public RoomData GetRoom(int roomId)
        {
            _roomMap ??= new Dictionary<int, RoomData>();

            // 如果映射表为空，重建
            if (_roomMap.Count == 0 && Rooms != null && Rooms.Count > 0)
            {
                foreach (var room in Rooms)
                {
                    _roomMap[room.RoomId] = room;
                }
            }

            return _roomMap.TryGetValue(roomId, out var result) ? result : null;
        }

        /// <summary>
        /// 移除房间
        /// </summary>
        public bool RemoveRoom(int roomId)
        {
            var room = GetRoom(roomId);
            if (room != null)
            {
                Rooms?.Remove(room);
                _roomMap?.Remove(roomId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空所有房间
        /// </summary>
        public void Clear()
        {
            Rooms?.Clear();
            _roomMap?.Clear();
        }
    }
}
