using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Event;
using Core.Game.Map.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 房间检测系统 (洪水填充)
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

            // 洪水填充: 从每个未访问的室内Cell开始
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (visited[y, x]) continue;

                    var cell = map.GetCell(x, y, floor);
                    if (cell == null || !cell.IsIndoor)
                    {
                        visited[y, x] = true;
                        continue;
                    }

                    // 发现未访问的室内Cell，开始洪水填充新房间
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

                if (!cell.IsIndoor)
                {
                    // 到达非室内区域，检查是否有墙阻隔
                    // 如果没有墙阻隔就到达了室外，说明房间不封闭
                    continue;
                }

                cell.RoomId = roomId;
                room.AddCell(x, y);

                // 检查四个方向
                TryExpandRoom(map, floor, x, y, x, y + 1, true, queue, visited, ref isEnclosed);  // 北
                TryExpandRoom(map, floor, x, y, x, y - 1, false, queue, visited, ref isEnclosed); // 南
                TryExpandRoom(map, floor, x, y, x + 1, y, false, queue, visited, ref isEnclosed); // 东
                TryExpandRoom(map, floor, x, y, x - 1, y, true, queue, visited, ref isEnclosed);  // 西
            }
        }

        /// <summary>
        /// 尝试向相邻格子扩展房间
        /// </summary>
        private void TryExpandRoom(MapData map, int floor,
            int fromX, int fromY, int toX, int toY,
            bool checkFromCellWall, Queue<Vector2Int> queue, bool[,] visited,
            ref bool isEnclosed)
        {
            if (toX < 0 || toX >= map.Width || toY < 0 || toY >= map.Height)
            {
                isEnclosed = false;
                return;
            }

            if (visited[toY, toX]) return;

            // 检查两个Cell之间是否有墙阻隔
            if (IsWallBlocking(map, floor, fromX, fromY, toX, toY))
                return; // 有墙阻隔，不扩展

            queue.Enqueue(new Vector2Int(toX, toY));
        }

        /// <summary>
        /// 检查两个相邻Cell之间是否有不可通过的墙
        /// </summary>
        private bool IsWallBlocking(MapData map, int floor, int fromX, int fromY, int toX, int toY)
        {
            int dx = toX - fromX;
            int dy = toY - fromY;

            // 向北移动 (dy=+1): 检查from cell的WallNorth
            if (dy == 1 && dx == 0)
            {
                var cell = map.GetCell(fromX, fromY, floor);
                return cell != null && cell.WallNorth.HasWall && !cell.WallNorth.IsPassable;
            }

            // 向南移动 (dy=-1): 检查to cell的WallNorth
            if (dy == -1 && dx == 0)
            {
                var cell = map.GetCell(toX, toY, floor);
                return cell != null && cell.WallNorth.HasWall && !cell.WallNorth.IsPassable;
            }

            // 向西移动 (dx=-1): 检查from cell的WallWest
            if (dx == -1 && dy == 0)
            {
                var cell = map.GetCell(fromX, fromY, floor);
                return cell != null && cell.WallWest.HasWall && !cell.WallWest.IsPassable;
            }

            // 向东移动 (dx=+1): 检查to cell的WallWest
            if (dx == 1 && dy == 0)
            {
                var cell = map.GetCell(toX, toY, floor);
                return cell != null && cell.WallWest.HasWall && !cell.WallWest.IsPassable;
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
