using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Utility;
using Core.Game.Navigation.Define;
using UnityEngine;

namespace Core.Game.Navigation.Utility
{
    /// <summary>
    /// 寻路工具类
    /// 处理墙体阻挡检测、对角移动验证、邻居可达性判断
    /// </summary>
    public static class NavigationUtility
    {
        #region 可通行检测

        /// <summary>
        /// 检查从from到to是否可通行（支持对角移动的墙角检测）
        /// </summary>
        public static bool CanMoveTo(MapData mapData, int fromX, int fromY, int floor, int toX, int toY)
        {
            // 目标格子有效性检查
            if (!mapData.IsValidCellPos(toX, toY, floor))
                return false;

            var toCell = mapData.GetCell(toX, toY, floor);
            if (toCell == null || !toCell.IsWalkable)
                return false;

            // 检查目标是否被占用（有不可通行的物体）
            if ((toCell.Flags & ECellFlags.Occupied) != 0)
                return false;

            int dx = toX - fromX;
            int dy = toY - fromY;

            // 直线移动：只检查两格之间的墙
            if (dx == 0 || dy == 0)
            {
                return !RoomDetectionUtility.IsWallBlocking(
                    mapData, floor,
                    new Vector2Int(fromX, fromY),
                    new Vector2Int(toX, toY));
            }

            // 对角移动：需要检查墙角
            // 从(fx,fy)到(fx+dx, fy+dy)对角移动时，
            // 必须确保两条相邻直线路径都不被墙体阻挡
            // 即：(fx,fy)->(fx+dx,fy) 和 (fx,fy)->(fx,fy+dy) 都不能有阻挡的墙
            // 同时：(fx+dx,fy)->(fx+dx,fy+dy) 和 (fx,fy+dy)->(fx+dx,fy+dy) 也不能有阻挡的墙

            var from = new Vector2Int(fromX, fromY);
            var to = new Vector2Int(toX, toY);
            var midH = new Vector2Int(fromX + dx, fromY); // 水平中间点
            var midV = new Vector2Int(fromX, fromY + dy); // 垂直中间点

            // 路径1: from -> midH -> to
            // 路径2: from -> midV -> to
            // 两条路径中至少有一条的两段都不被墙挡住即可（宽松模式）
            // 但为了防止穿墙角，两条路径的第一段都不能被阻挡

            // 严格模式：from到两个中间点都不能被墙挡
            bool wallBlockH = RoomDetectionUtility.IsWallBlocking(mapData, floor, from, midH);
            bool wallBlockV = RoomDetectionUtility.IsWallBlocking(mapData, floor, from, midV);

            // 如果两个方向都被墙挡住，不能对角移动
            if (wallBlockH && wallBlockV)
                return false;

            // 检查中间点到目标的墙
            bool wallBlockHToEnd = RoomDetectionUtility.IsWallBlocking(mapData, floor, midH, to);
            bool wallBlockVToEnd = RoomDetectionUtility.IsWallBlocking(mapData, floor, midV, to);

            // 如果两条路径的第二段都被挡，也不能走
            if (wallBlockHToEnd && wallBlockVToEnd)
                return false;

            // 至少有一条完整路径可通行
            bool path1Clear = !wallBlockH && !wallBlockHToEnd;
            bool path2Clear = !wallBlockV && !wallBlockVToEnd;

            if (!path1Clear && !path2Clear)
                return false;

            // 检查中间格子的可行走性（对角移动时中间格子也要能走）
            if (!wallBlockH)
            {
                var midHCell = mapData.GetCell(midH.x, midH.y, floor);
                if (midHCell == null || !midHCell.IsWalkable)
                    path1Clear = false;
            }

            if (!wallBlockV)
            {
                var midVCell = mapData.GetCell(midV.x, midV.y, floor);
                if (midVCell == null || !midVCell.IsWalkable)
                    path2Clear = false;
            }

            return path1Clear || path2Clear;
        }

        /// <summary>
        /// 检查格子是否可作为寻路目标（可行走且未被占用）
        /// </summary>
        public static bool IsCellPassable(MapData mapData, int x, int y, int floor)
        {
            var cell = mapData.GetCell(x, y, floor);
            if (cell == null)
                return false;

            return cell.IsWalkable && (cell.Flags & ECellFlags.Occupied) == 0;
        }

        #endregion

        #region 移动代价

        /// <summary>
        /// 获取移动到目标格子的代价
        /// </summary>
        public static int GetMoveCost(MapData mapData, int toX, int toY, int floor, bool isDiagonal)
        {
            var cell = mapData.GetCell(toX, toY, floor);
            if (cell == null)
                return int.MaxValue;

            int baseCost = isDiagonal ? NavigationDefine.CostDiagonal : NavigationDefine.CostStraight;

            // MoveCost为0视为不可通行
            if (cell.MoveCost == 0)
                return int.MaxValue;

            return baseCost * cell.MoveCost;
        }

        #endregion

        #region 启发式计算

        /// <summary>
        /// 计算启发式代价（Octile距离，适配对角移动）
        /// </summary>
        public static int CalculateHeuristic(int fromX, int fromY, int fromFloor,
            int toX, int toY, int toFloor)
        {
            int dx = Mathf.Abs(toX - fromX);
            int dy = Mathf.Abs(toY - fromY);

            // Octile距离：min(dx,dy) * 14 + |dx-dy| * 10
            int diag = Mathf.Min(dx, dy);
            int straight = Mathf.Abs(dx - dy);
            int heuristic = diag * NavigationDefine.CostDiagonal + straight * NavigationDefine.CostStraight;

            // 跨楼层额外代价
            int floorDiff = Mathf.Abs(toFloor - fromFloor);
            heuristic += floorDiff * NavigationDefine.CostStairs;

            return heuristic;
        }

        #endregion

        #region 楼梯检测

        /// <summary>
        /// 检查格子是否是楼梯（通过ObjectIds判断）
        /// 约定：ObjectId为负数时代表楼梯连接
        /// StairUp = -1, StairDown = -2
        /// </summary>
        public static bool IsStairUp(MapData mapData, int x, int y, int floor)
        {
            var cell = mapData.GetCell(x, y, floor);
            return cell != null && cell.ContainsObject(StairUpId);
        }

        public static bool IsStairDown(MapData mapData, int x, int y, int floor)
        {
            var cell = mapData.GetCell(x, y, floor);
            return cell != null && cell.ContainsObject(StairDownId);
        }

        /// <summary>
        /// 楼梯上行标记ID
        /// </summary>
        public const int StairUpId = -1;

        /// <summary>
        /// 楼梯下行标记ID
        /// </summary>
        public const int StairDownId = -2;

        #endregion
    }
}
