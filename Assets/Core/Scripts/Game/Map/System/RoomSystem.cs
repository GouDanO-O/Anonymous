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
    /// 格子占据式: 有结构(墙/门/窗)的格子是边界, 无结构的Indoor格子组成房间
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

            // 重置所有Cell的RoomId
            var visited = new bool[map.Height, map.Width];
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, floor);
                    if (cell != null)
                        cell.RoomId = MapConst.InvalidRoomId;
                }
            }

            // 洪水填充: 从每个未访问的室内且无结构的Cell开始
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (visited[y, x]) continue;

                    var cell = map.GetCell(x, y, floor);
                    if (cell == null || !cell.IsIndoor || cell.HasStructure)
                    {
                        visited[y, x] = true;
                        continue;
                    }

                    // 发现未访问的室内无结构Cell，开始洪水填充新房间
                    int roomId = floorRoomData.CreateRoom(floor);
                    var room = floorRoomData.GetRoom(roomId);
                    bool isEnclosed = true;

                    FloodFillRoom(map, floor, x, y, roomId, room, visited, ref isEnclosed);

                    room.IsEnclosed = isEnclosed;
                    room.RecalculateBounds();

                    if (room.CellCount == 0)
                    {
                        floorRoomData.RemoveRoom(roomId);
                    }
                }
            }

            LogKit.Log($"楼层 {floor} 房间检测完成: {floorRoomData.RoomCount} 个房间");
        }

        /// <summary>
        /// 洪水填充单个房间
        /// 规则: 有阻挡结构(墙/窗)的格子是边界不可扩展,
        ///       门也是边界(门分隔房间), 无结构的Indoor格子加入房间
        /// </summary>
        private void FloodFillRoom(MapData map, int floor, int startX, int startY,
            int roomId, RoomData room, bool[,] visited, ref bool isEnclosed)
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

                // 有结构的格子是边界, 不扩展进去
                if (cell.HasStructure) continue;

                if (!cell.IsIndoor)
                {
                    // 到达非室内区域且无结构阻隔, 房间不封闭
                    isEnclosed = false;
                    continue;
                }

                cell.RoomId = roomId;
                room.AddCell(x, y);

                // 向四个方向扩展
                queue.Enqueue(new Vector2Int(x, y + 1)); // 北
                queue.Enqueue(new Vector2Int(x, y - 1)); // 南
                queue.Enqueue(new Vector2Int(x + 1, y)); // 东
                queue.Enqueue(new Vector2Int(x - 1, y)); // 西
            }
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
