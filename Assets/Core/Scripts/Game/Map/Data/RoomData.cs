using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 房间数据
    /// 表示一个被墙壁包围的封闭空间
    /// </summary>
    [Serializable]
    public class RoomData
    {
        #region 基础信息

        /// <summary>
        /// 房间唯一 ID
        /// </summary>
        public int RoomId;

        /// <summary>
        /// 所在楼层
        /// </summary>
        public int Floor;

        /// <summary>
        /// 房间名称（可选）
        /// </summary>
        public string RoomName;

        #endregion

        #region 范围数据

        /// <summary>
        /// 房间包含的所有格子坐标
        /// </summary>
        public List<Vector2Int> Cells;

        /// <summary>
        /// 房间边界（最小 X）
        /// </summary>
        public int MinX;

        /// <summary>
        /// 房间边界（最大 X）
        /// </summary>
        public int MaxX;

        /// <summary>
        /// 房间边界（最小 Y）
        /// </summary>
        public int MinY;

        /// <summary>
        /// 房间边界（最大 Y）
        /// </summary>
        public int MaxY;

        #endregion

        #region 属性

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
        /// 房间中心点
        /// </summary>
        public Vector2 Center => new Vector2((MinX + MaxX) / 2f, (MinY + MaxY) / 2f);

        #endregion

        #region 构造函数

        public RoomData()
        {
            Cells = new List<Vector2Int>();
            RoomId = -1;
        }

        public RoomData(int roomId, int floor) : this()
        {
            RoomId = roomId;
            Floor = floor;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加格子到房间
        /// </summary>
        public void AddCell(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            if (!Cells.Contains(pos))
            {
                Cells.Add(pos);
                UpdateBounds(x, y);
            }
        }

        /// <summary>
        /// 添加格子到房间
        /// </summary>
        public void AddCell(Vector2Int pos)
        {
            AddCell(pos.x, pos.y);
        }

        /// <summary>
        /// 检查格子是否在房间内
        /// </summary>
        public bool ContainsCell(int x, int y)
        {
            return Cells.Contains(new Vector2Int(x, y));
        }

        /// <summary>
        /// 检查格子是否在房间内
        /// </summary>
        public bool ContainsCell(Vector2Int pos)
        {
            return Cells.Contains(pos);
        }

        /// <summary>
        /// 更新边界
        /// </summary>
        private void UpdateBounds(int x, int y)
        {
            if (Cells.Count == 1)
            {
                MinX = MaxX = x;
                MinY = MaxY = y;
            }
            else
            {
                MinX = Mathf.Min(MinX, x);
                MaxX = Mathf.Max(MaxX, x);
                MinY = Mathf.Min(MinY, y);
                MaxY = Mathf.Max(MaxY, y);
            }
        }

        /// <summary>
        /// 重新计算边界
        /// </summary>
        public void RecalculateBounds()
        {
            if (Cells == null || Cells.Count == 0)
                return;

            MinX = MinY = int.MaxValue;
            MaxX = MaxY = int.MinValue;

            foreach (var cell in Cells)
            {
                MinX = Mathf.Min(MinX, cell.x);
                MaxX = Mathf.Max(MaxX, cell.x);
                MinY = Mathf.Min(MinY, cell.y);
                MaxY = Mathf.Max(MaxY, cell.y);
            }
        }

        #endregion
    }

    /// <summary>
    /// 楼层房间数据
    /// 管理单层楼的所有房间
    /// </summary>
    [Serializable]
    public class FloorRoomData
    {
        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor;

        /// <summary>
        /// 该楼层的所有房间
        /// </summary>
        public List<RoomData> Rooms;

        /// <summary>
        /// 格子到房间ID的映射（快速查找）
        /// </summary>
        [NonSerialized]
        public Dictionary<Vector2Int, int> CellToRoomMap;

        public FloorRoomData()
        {
            Rooms = new List<RoomData>();
            CellToRoomMap = new Dictionary<Vector2Int, int>();
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
            Rooms.Add(room);

            // 更新映射
            foreach (var cell in room.Cells)
            {
                CellToRoomMap[cell] = room.RoomId;
            }
        }

        /// <summary>
        /// 获取格子所属的房间ID
        /// </summary>
        /// <returns>房间ID，如果不在任何房间内则返回 -1</returns>
        public int GetRoomId(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            return CellToRoomMap.TryGetValue(pos, out int roomId) ? roomId : -1;
        }

        /// <summary>
        /// 获取格子所属的房间
        /// </summary>
        public RoomData GetRoom(int x, int y)
        {
            int roomId = GetRoomId(x, y);
            if (roomId < 0)
                return null;

            return Rooms.Find(r => r.RoomId == roomId);
        }

        /// <summary>
        /// 重建映射
        /// </summary>
        public void RebuildCellToRoomMap()
        {
            CellToRoomMap.Clear();
            foreach (var room in Rooms)
            {
                foreach (var cell in room.Cells)
                {
                    CellToRoomMap[cell] = room.RoomId;
                }
            }
        }
    }
}
