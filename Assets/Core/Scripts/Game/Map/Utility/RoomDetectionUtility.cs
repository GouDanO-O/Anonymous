using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Utility
{
    /// <summary>
    /// 房间检测工具
    /// 使用Flood Fill算法识别被墙壁包围的封闭空间
    /// </summary>
    public static class RoomDetectionUtility
    {
        /// <summary>
        /// 检测地图中所有楼层的房间
        /// </summary>
        public static Dictionary<int, FloorRoomData> DetectAllRooms(MapData mapData)
        {
            var result = new Dictionary<int, FloorRoomData>();

            for (int floor = 0; floor < mapData.FloorCount; floor++)
            {
                var floorRoomData = DetectRoomsInFloor(mapData, floor);
                result[floor] = floorRoomData;
                mapData.SetFloorRoomData(floor, floorRoomData);
            }

            return result;
        }

        /// <summary>
        /// 检测单层楼的所有房间
        /// </summary>
        public static FloorRoomData DetectRoomsInFloor(MapData mapData, int floor)
        {
            var floorRoomData = new FloorRoomData(floor);
            var visited = new HashSet<Vector2Int>();
            int nextRoomId = floor * 1000; // 每层楼的房间ID从 floor*1000 开始，避免冲突

            // 遍历所有格子
            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    var pos = new Vector2Int(x, y);

                    // 跳过已访问的格子
                    if (visited.Contains(pos))
                        continue;

                    var cell = mapData.GetCell(x, y, floor);
                    if (cell == null)
                        continue;

                    // 只处理有屋顶的格子（室内）
                    if (!cell.HasFlag(ECellFlags.HasRoof))
                    {
                        visited.Add(pos);
                        cell.RoomId = -1; // 室外
                        cell.RemoveFlag(ECellFlags.Indoor);
                        continue;
                    }

                    // 使用Flood Fill找到连通的室内区域
                    var room = FloodFillRoom(mapData, floor, x, y, nextRoomId, visited);

                    if (room != null && room.Area > 0)
                    {
                        floorRoomData.AddRoom(room);
                        nextRoomId++;
                    }
                }
            }

            Debug.Log($"[RoomDetection] Floor {floor}: Found {floorRoomData.Rooms.Count} rooms");

            return floorRoomData;
        }

        /// <summary>
        /// Flood Fill算法找到连通的室内区域
        /// </summary>
        private static RoomData FloodFillRoom(MapData mapData, int floor, int startX, int startY,
            int roomId, HashSet<Vector2Int> globalVisited)
        {
            var room = new RoomData(roomId, floor);
            var queue = new Queue<Vector2Int>();
            var localVisited = new HashSet<Vector2Int>();

            var startPos = new Vector2Int(startX, startY);
            queue.Enqueue(startPos);
            localVisited.Add(startPos);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var cell = mapData.GetCell(current.x, current.y, floor);

                if (cell == null)
                    continue;

                // 只添加有屋顶的格子
                if (!cell.HasFlag(ECellFlags.HasRoof))
                    continue;

                // 添加到房间
                room.AddCell(current);
                globalVisited.Add(current);

                // 更新格子的房间ID和室内标记
                cell.RoomId = roomId;
                cell.AddFlag(ECellFlags.Indoor);

                // 检查四个方向的邻居
                TryAddNeighbor(mapData, floor, current, 0, 1, queue, localVisited, globalVisited);  // 北
                TryAddNeighbor(mapData, floor, current, 1, 0, queue, localVisited, globalVisited);  // 东
                TryAddNeighbor(mapData, floor, current, 0, -1, queue, localVisited, globalVisited); // 南
                TryAddNeighbor(mapData, floor, current, -1, 0, queue, localVisited, globalVisited); // 西
            }

            return room;
        }

        /// <summary>
        /// 尝试添加邻居到队列
        /// </summary>
        private static void TryAddNeighbor(MapData mapData, int floor, Vector2Int current,
            int dx, int dy, Queue<Vector2Int> queue,
            HashSet<Vector2Int> localVisited, HashSet<Vector2Int> globalVisited)
        {
            var neighbor = new Vector2Int(current.x + dx, current.y + dy);

            // 检查是否已访问
            if (localVisited.Contains(neighbor) || globalVisited.Contains(neighbor))
                return;

            // 检查是否在地图范围内
            if (!mapData.IsValidCellPos(neighbor.x, neighbor.y, floor))
                return;

            // 检查是否有墙阻挡
            if (IsWallBlocking(mapData, floor, current, neighbor))
                return;

            var neighborCell = mapData.GetCell(neighbor.x, neighbor.y, floor);
            if (neighborCell == null)
                return;

            // 只添加有屋顶的格子
            if (!neighborCell.HasFlag(ECellFlags.HasRoof))
                return;

            localVisited.Add(neighbor);
            queue.Enqueue(neighbor);
        }

        /// <summary>
        /// 检查两个相邻格子之间是否有墙阻挡
        /// </summary>
        public static bool IsWallBlocking(MapData mapData, int floor, Vector2Int from, Vector2Int to)
        {
            int dx = to.x - from.x;
            int dy = to.y - from.y;

            var fromCell = mapData.GetCell(from.x, from.y, floor);
            var toCell = mapData.GetCell(to.x, to.y, floor);

            if (fromCell == null || toCell == null)
                return true; // 无效格子视为阻挡

            // 北墙/西墙系统：
            // - 每个格子拥有其北边和西边的墙
            // - 向北移动：检查当前格子(from)的北墙
            // - 向南移动：检查目标格子(to)的北墙
            // - 向东移动：检查目标格子(to)的西墙
            // - 向西移动：检查当前格子(from)的西墙

            if (dy > 0) // 向北移动
            {
                if (fromCell.WallNorth.HasWall && !fromCell.WallNorth.IsPassable)
                    return true;
            }
            else if (dy < 0) // 向南移动
            {
                if (toCell.WallNorth.HasWall && !toCell.WallNorth.IsPassable)
                    return true;
            }

            if (dx > 0) // 向东移动
            {
                if (toCell.WallWest.HasWall && !toCell.WallWest.IsPassable)
                    return true;
            }
            else if (dx < 0) // 向西移动
            {
                if (fromCell.WallWest.HasWall && !fromCell.WallWest.IsPassable)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查从一个格子到另一个格子是否可通行（考虑墙体）
        /// </summary>
        public static bool CanPass(MapData mapData, int floor, Vector2Int from, Vector2Int to)
        {
            // 首先检查目标格子是否可行走
            var toCell = mapData.GetCell(to.x, to.y, floor);
            if (toCell == null || !toCell.IsWalkable)
                return false;

            // 然后检查是否有墙阻挡
            return !IsWallBlocking(mapData, floor, from, to);
        }

        /// <summary>
        /// 检查格子是否在室内
        /// </summary>
        public static bool IsCellIndoor(MapData mapData, int x, int y, int floor)
        {
            var cell = mapData.GetCell(x, y, floor);
            return cell != null && cell.HasFlag(ECellFlags.Indoor);
        }

        /// <summary>
        /// 获取格子所属的房间ID
        /// </summary>
        public static int GetRoomId(MapData mapData, int x, int y, int floor)
        {
            var cell = mapData.GetCell(x, y, floor);
            return cell?.RoomId ?? -1;
        }

        /// <summary>
        /// 重新检测指定区域的房间（增量更新）
        /// </summary>
        public static void RefreshRoomsInArea(MapData mapData, int floor, int minX, int minY, int maxX, int maxY)
        {
            // 简化实现：重新检测整层
            // 后续可以优化为只检测受影响的区域
            var floorRoomData = DetectRoomsInFloor(mapData, floor);
            mapData.SetFloorRoomData(floor, floorRoomData);
        }
    }
}
