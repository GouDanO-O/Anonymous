using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 房间检测系统 (洪水填充)
    /// Indoor标志由本系统自动推导: 被结构围合的区域 = Indoor
    /// HasRoof由玩家手动建造, 本系统不管理
    /// </summary>
    public class RoomSystem : AbstractSystem
    {
        private MapDataModel _mapDataModel;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
        }

        /// <summary>
        /// 重新计算指定楼层的所有房间
        /// </summary>
        public void RecalculateRooms(int floor)
        {
            var map = _mapDataModel.CurrentMap;
            if (map == null) return;

            var floorRoomData = map.GetFloorRoomData(floor);
            if (floorRoomData == null) return;

            floorRoomData.ClearAllRooms();

            // 1. 清除所有Cell的Indoor标志和RoomId (不动HasRoof)
            var visited = new bool[map.Height, map.Width];
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, floor);
                    if (cell != null)
                    {
                        cell.SetFlag(ECellFlags.Indoor, false);
                        cell.RoomId = MapConst.InvalidRoomId;
                    }
                }
            }

            // 2. 洪水填充: 从每个未访问的无结构Cell开始
            // 收集所有区域用于后处理
            var enclosedRoomCells = new List<List<Vector2Int>>();

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (visited[y, x]) continue;

                    var cell = map.GetCell(x, y, floor);
                    if (cell == null || cell.HasStructure)
                    {
                        visited[y, x] = true;
                        continue;
                    }

                    // 开始洪水填充
                    int roomId = floorRoomData.CreateRoom(floor);
                    var room = floorRoomData.GetRoom(roomId);
                    bool isEnclosed = true;
                    var roomCells = new List<Vector2Int>();

                    FloodFillRoom(map, floor, x, y, roomId, room, visited, ref isEnclosed, roomCells);

                    room.IsEnclosed = isEnclosed;
                    room.RecalculateBounds();

                    if (room.CellCount == 0)
                    {
                        floorRoomData.RemoveRoom(roomId);
                    }
                    else if (isEnclosed)
                    {
                        // 3. 封闭区域 → 设置Indoor标志
                        foreach (var pos in roomCells)
                        {
                            var c = map.GetCell(pos.x, pos.y, floor);
                            if (c != null)
                                c.SetFlag(ECellFlags.Indoor, true);
                        }
                        enclosedRoomCells.Add(roomCells);
                    }
                }
            }

            // 4. 后处理: 结构Cell如果相邻有enclosed房间 → 也设Indoor
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, floor);
                    if (cell == null || !cell.HasStructure) continue;

                    // 检查4邻居是否有Indoor的Cell
                    if (HasIndoorNeighbor(map, x, y, floor))
                    {
                        cell.SetFlag(ECellFlags.Indoor, true);
                    }
                }
            }

            LogKit.Log($"楼层 {floor} 房间检测完成: {floorRoomData.RoomCount} 个房间");
        }

        /// <summary>
        /// 洪水填充单个房间
        /// 有结构的格子是边界不可扩展, 到达地图边界则不封闭
        /// </summary>
        private void FloodFillRoom(MapData map, int floor, int startX, int startY,
            int roomId, RoomData room, bool[,] visited, ref bool isEnclosed,
            List<Vector2Int> roomCells)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                int x = pos.x, y = pos.y;

                if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
                {
                    isEnclosed = false;
                    continue;
                }

                if (visited[y, x]) continue;
                visited[y, x] = true;

                var cell = map.GetCell(x, y, floor);
                if (cell == null) continue;

                // 有结构的格子是边界, 不扩展
                if (cell.HasStructure) continue;

                cell.RoomId = roomId;
                room.AddCell(x, y);
                roomCells.Add(new Vector2Int(x, y));

                // 向四个方向扩展
                queue.Enqueue(new Vector2Int(x, y + 1));
                queue.Enqueue(new Vector2Int(x, y - 1));
                queue.Enqueue(new Vector2Int(x + 1, y));
                queue.Enqueue(new Vector2Int(x - 1, y));
            }
        }

        /// <summary>
        /// 检查4邻居是否有Indoor的Cell
        /// </summary>
        private bool HasIndoorNeighbor(MapData map, int x, int y, int floor)
        {
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                var neighbor = map.GetCell(x + dx[i], y + dy[i], floor);
                if (neighbor != null && neighbor.IsIndoor)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取指定位置的房间ID
        /// </summary>
        public int GetRoomId(int x, int y, int floor)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            return cell?.RoomId ?? MapConst.InvalidRoomId;
        }
    }
}
