using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 单个房间数据
    /// </summary>
    [Serializable]
    public class RoomData
    {
        public int RoomId;
        public int Floor;
        public HashSet<Vector2Int> Cells;
        public RectInt Bounds;
        public bool IsEnclosed;
        public float Temperature;
        public float Lighting;

        public RoomData(int roomId, int floor)
        {
            RoomId = roomId;
            Floor = floor;
            Cells = new HashSet<Vector2Int>();
            IsEnclosed = false;
            Temperature = 20f;
            Lighting = 1f;
        }

        public int CellCount => Cells.Count;

        public void AddCell(int x, int y)
        {
            Cells.Add(new Vector2Int(x, y));
        }

        /// <summary>
        /// 重新计算包围盒
        /// </summary>
        public void RecalculateBounds()
        {
            if (Cells.Count == 0) return;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var cell in Cells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.y < minY) minY = cell.y;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.y > maxY) maxY = cell.y;
            }

            Bounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }

    /// <summary>
    /// 每层楼的房间数据管理器
    /// </summary>
    [Serializable]
    public class FloorRoomData
    {
        private Dictionary<int, RoomData> _rooms = new Dictionary<int, RoomData>();
        private int _nextRoomId;

        public int CreateRoom(int floor)
        {
            int id = _nextRoomId++;
            _rooms[id] = new RoomData(id, floor);
            return id;
        }

        public RoomData GetRoom(int roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        public void RemoveRoom(int roomId)
        {
            _rooms.Remove(roomId);
        }

        public void ClearAllRooms()
        {
            _rooms.Clear();
            _nextRoomId = 0;
        }

        public IEnumerable<RoomData> GetAllRooms()
        {
            return _rooms.Values;
        }

        public int RoomCount => _rooms.Count;
    }
}
