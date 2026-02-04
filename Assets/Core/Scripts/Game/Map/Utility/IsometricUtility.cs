using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Utility
{
    /// <summary>
    /// 等轴测坐标转换工具
    /// </summary>
    public static class IsometricUtility
    {
        #region 格子坐标 -> 屏幕坐标

        /// <summary>
        /// 格子坐标转屏幕坐标（格子左下角）
        /// </summary>
        public static Vector3 CellToScreen(int cellX, int cellY, int floor)
        {
            float screenX = (cellX - cellY) * MapDefine.TileHalfWidth / MapDefine.PixelsPerUnit;
            float screenY = (cellX + cellY) * MapDefine.TileHalfHeight / MapDefine.PixelsPerUnit;
            screenY += floor * MapDefine.FloorWorldHeight;

            return new Vector3(screenX, screenY, 0);
        }

        /// <summary>
        /// 格子坐标转屏幕坐标（格子中心）
        /// </summary>
        public static Vector3 CellCenterToScreen(int cellX, int cellY, int floor)
        {
            // 格子中心 = 格子左下角 + 半个格子偏移
            // 在等轴测中，中心点的偏移是 (0.5, 0.5) 格子单位
            float screenX = (cellX - cellY + 0.5f - 0.5f) * MapDefine.TileHalfWidth / MapDefine.PixelsPerUnit;
            float screenY = (cellX + cellY + 0.5f + 0.5f) * MapDefine.TileHalfHeight / MapDefine.PixelsPerUnit;
            screenY += floor * MapDefine.FloorWorldHeight;

            return new Vector3(screenX, screenY, 0);
        }

        /// <summary>
        /// 格子坐标转屏幕坐标（浮点精度）
        /// </summary>
        public static Vector3 CellToScreen(float cellX, float cellY, int floor)
        {
            float screenX = (cellX - cellY) * MapDefine.TileHalfWidth / MapDefine.PixelsPerUnit;
            float screenY = (cellX + cellY) * MapDefine.TileHalfHeight / MapDefine.PixelsPerUnit;
            screenY += floor * MapDefine.FloorWorldHeight;

            return new Vector3(screenX, screenY, 0);
        }

        #endregion

        #region 屏幕坐标 -> 格子坐标

        /// <summary>
        /// 屏幕坐标转格子坐标（返回整数）
        /// </summary>
        public static Vector2Int ScreenToCellInt(Vector3 screenPos, int floor)
        {
            Vector2 cell = ScreenToCell(screenPos, floor);
            return new Vector2Int(Mathf.FloorToInt(cell.x), Mathf.FloorToInt(cell.y));
        }

        /// <summary>
        /// 屏幕坐标转格子坐标（返回浮点）
        /// </summary>
        public static Vector2 ScreenToCell(Vector3 screenPos, int floor)
        {
            // 减去楼层高度偏移
            float adjustedY = screenPos.y - floor * MapDefine.FloorWorldHeight;

            float halfW = MapDefine.TileHalfWidth / MapDefine.PixelsPerUnit;
            float halfH = MapDefine.TileHalfHeight / MapDefine.PixelsPerUnit;

            // 反向计算
            // screenX = (cellX - cellY) * halfW
            // screenY = (cellX + cellY) * halfH
            //
            // screenX / halfW = cellX - cellY
            // screenY / halfH = cellX + cellY
            //
            // cellX = (screenX/halfW + screenY/halfH) / 2
            // cellY = (screenY/halfH - screenX/halfW) / 2

            float cellX = (screenPos.x / halfW + adjustedY / halfH) / 2f;
            float cellY = (adjustedY / halfH - screenPos.x / halfW) / 2f;

            return new Vector2(cellX, cellY);
        }

        /// <summary>
        /// 世界坐标转格子坐标（通过Camera）
        /// </summary>
        public static Vector2Int WorldToCellInt(Vector3 worldPos, int floor, Camera camera)
        {
            Vector3 screenPos = camera.WorldToScreenPoint(worldPos);
            Vector3 viewportPos = camera.ScreenToViewportPoint(screenPos);

            // 转换为以摄像机为中心的坐标
            float x = (viewportPos.x - 0.5f) * camera.orthographicSize * 2f * camera.aspect;
            float y = (viewportPos.y - 0.5f) * camera.orthographicSize * 2f;

            return ScreenToCellInt(new Vector3(x, y, 0) + camera.transform.position, floor);
        }

        #endregion

        #region 深度计算

        /// <summary>
        /// 计算渲染深度（用于排序）
        /// </summary>
        /// <param name="cellX">格子X坐标</param>
        /// <param name="cellY">格子Y坐标</param>
        /// <param name="floor">楼层</param>
        /// <param name="layerOffset">层级偏移（0=地面, 0.1=地板, 0.2=墙体, 等）</param>
        /// <returns>深度值（越小越靠前）</returns>
        public static float CalculateDepth(int cellX, int cellY, int floor, float layerOffset = 0f)
        {
            // 深度计算：
            // 1. 基础深度 = (cellX + cellY)，使得靠近摄像机的物体深度值更大
            // 2. 楼层偏移，高楼层深度值更大
            // 3. 层级偏移，用于同一格子内的不同层（地面、墙、屋顶等）
            // 注意：在Unity中，Z轴负方向是朝向摄像机的，所以我们需要负值

            float baseDepth = (cellX + cellY) * MapDefine.DepthPerTile;
            float floorDepth = floor * MapDefine.DepthPerFloor;

            return -(baseDepth + floorDepth + layerOffset);
        }

        /// <summary>
        /// 计算渲染排序值（用于SpriteRenderer.sortingOrder）
        /// </summary>
        public static int CalculateSortingOrder(int cellX, int cellY, int floor, int layerBase = 0)
        {
            // sortingOrder是整数，所以需要放大
            int baseOrder = (cellX + cellY) * 10;
            int floorOrder = floor * 10000;

            return layerBase + baseOrder + floorOrder;
        }

        #endregion

        #region 方向计算

        /// <summary>
        /// 计算从一个格子到另一个格子的方向（8方向）
        /// </summary>
        public static Vector2Int GetDirection(Vector2Int from, Vector2Int to)
        {
            int dx = to.x - from.x;
            int dy = to.y - from.y;

            // 归一化到-1, 0, 1
            int dirX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            int dirY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

            return new Vector2Int(dirX, dirY);
        }

        /// <summary>
        /// 获取两个格子之间的曼哈顿距离
        /// </summary>
        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 获取两个格子之间的切比雪夫距离（对角距离）
        /// </summary>
        public static int ChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        /// <summary>
        /// 获取两个格子之间的欧几里得距离
        /// </summary>
        public static float EuclideanDistance(Vector2Int a, Vector2Int b)
        {
            return Vector2Int.Distance(a, b);
        }

        #endregion

        #region 范围检查

        /// <summary>
        /// 检查格子坐标是否在指定范围内
        /// </summary>
        public static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// 检查格子坐标是否在指定范围内
        /// </summary>
        public static bool IsInBounds(Vector2Int pos, int width, int height)
        {
            return IsInBounds(pos.x, pos.y, width, height);
        }

        /// <summary>
        /// 限制格子坐标在指定范围内
        /// </summary>
        public static Vector2Int ClampToBounds(Vector2Int pos, int width, int height)
        {
            return new Vector2Int(
                Mathf.Clamp(pos.x, 0, width - 1),
                Mathf.Clamp(pos.y, 0, height - 1)
            );
        }

        #endregion

        #region 邻居获取

        /// <summary>
        /// 获取四方向邻居坐标
        /// </summary>
        public static readonly Vector2Int[] FourDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 北
            new Vector2Int(1, 0),   // 东
            new Vector2Int(0, -1),  // 南
            new Vector2Int(-1, 0)   // 西
        };

        /// <summary>
        /// 获取八方向邻居坐标
        /// </summary>
        public static readonly Vector2Int[] EightDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 北
            new Vector2Int(1, 1),   // 东北
            new Vector2Int(1, 0),   // 东
            new Vector2Int(1, -1),  // 东南
            new Vector2Int(0, -1),  // 南
            new Vector2Int(-1, -1), // 西南
            new Vector2Int(-1, 0),  // 西
            new Vector2Int(-1, 1)   // 西北
        };

        #endregion
    }
}
