using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.Utility
{
    /// <summary>
    /// 等轴测坐标转换工具
    /// 
    /// 坐标系说明：
    /// - 世界坐标（World）：格子坐标，X 向右，Y 向上，Z 向上层楼
    /// - 屏幕坐标（Screen）：Unity 世界坐标，用于渲染
    /// - 等轴测视角：45° 俯视角，菱形 Tile
    /// </summary>
    public static class IsometricUtility
    {
        #region 世界坐标 -> 屏幕坐标

        /// <summary>
        /// 世界格子坐标转屏幕坐标（Unity 世界坐标）
        /// </summary>
        /// <param name="cellX">格子 X</param>
        /// <param name="cellY">格子 Y</param>
        /// <param name="floor">楼层</param>
        /// <returns>Unity 世界坐标</returns>
        public static Vector3 CellToScreen(int cellX, int cellY, int floor = 0)
        {
            return CellToScreen((float)cellX, (float)cellY, floor);
        }

        /// <summary>
        /// 世界格子坐标转屏幕坐标（支持小数，用于平滑移动）
        /// </summary>
        public static Vector3 CellToScreen(float cellX, float cellY, int floor = 0)
        {
            // 等轴测投影公式
            // screenX = (cellX - cellY) * tileHalfWidth
            // screenY = (cellX + cellY) * tileHalfHeight + floor * floorHeight

            float screenX = (cellX - cellY) * (MapDefine.TileWorldWidth * 0.5f);
            float screenY = (cellX + cellY) * (MapDefine.TileWorldHeight * 0.5f);
            screenY += floor * MapDefine.FloorWorldHeight;

            return new Vector3(screenX, screenY, 0f);
        }

        /// <summary>
        /// 获取格子中心的屏幕坐标
        /// </summary>
        public static Vector3 CellCenterToScreen(int cellX, int cellY, int floor = 0)
        {
            return CellToScreen(cellX + 0.5f, cellY + 0.5f, floor);
        }

        #endregion

        #region 屏幕坐标 -> 世界坐标

        /// <summary>
        /// 屏幕坐标转世界格子坐标
        /// </summary>
        /// <param name="screenPos">Unity 世界坐标</param>
        /// <param name="floor">当前楼层</param>
        /// <returns>格子坐标（可能为小数）</returns>
        public static Vector2 ScreenToCell(Vector3 screenPos, int floor = 0)
        {
            // 先减去楼层偏移
            float adjustedY = screenPos.y - floor * MapDefine.FloorWorldHeight;

            // 逆等轴测投影
            // cellX = (screenX / halfWidth + screenY / halfHeight) / 2
            // cellY = (screenY / halfHeight - screenX / halfWidth) / 2

            float halfWidth = MapDefine.TileWorldWidth * 0.5f;
            float halfHeight = MapDefine.TileWorldHeight * 0.5f;

            float cellX = (screenPos.x / halfWidth + adjustedY / halfHeight) * 0.5f;
            float cellY = (adjustedY / halfHeight - screenPos.x / halfWidth) * 0.5f;

            return new Vector2(cellX, cellY);
        }

        /// <summary>
        /// 屏幕坐标转世界格子坐标（整数）
        /// </summary>
        public static Vector2Int ScreenToCellInt(Vector3 screenPos, int floor = 0)
        {
            Vector2 cellPos = ScreenToCell(screenPos, floor);
            return new Vector2Int(Mathf.FloorToInt(cellPos.x), Mathf.FloorToInt(cellPos.y));
        }

        #endregion

        #region 深度计算

        /// <summary>
        /// 计算渲染深度值（用于排序）
        /// 数值越小越先渲染（在后面）
        /// </summary>
        /// <param name="cellX">格子 X</param>
        /// <param name="cellY">格子 Y</param>
        /// <param name="floor">楼层</param>
        /// <param name="layerOffset">层级偏移（地面=0, 地板=1, 墙=2, 物体=3, 角色=4）</param>
        /// <returns>深度值</returns>
        public static float CalculateDepth(int cellX, int cellY, int floor, int layerOffset = 0)
        {
            // 基础深度 = 楼层 * 楼层深度 + (x + y) * 格子深度 + 层级偏移
            // 注意：Y 坐标在等轴测中，越大的 Y 应该越后渲染（depth 越大）
            float depth = floor * MapDefine.DepthPerFloor;
            depth += (cellX + cellY) * MapDefine.DepthPerTile;
            depth += layerOffset * 0.001f;

            return depth;
        }

        /// <summary>
        /// 计算 SortingOrder（用于 SpriteRenderer）
        /// </summary>
        public static int CalculateSortingOrder(int cellX, int cellY, int floor, int baseOrder)
        {
            // 基础排序 + 楼层偏移 + 位置偏移
            int order = baseOrder;
            order += floor * 10000;
            order += (cellX + cellY) * 10;
            return order;
        }

        #endregion

        #region Mesh 顶点计算

        /// <summary>
        /// 获取菱形 Tile 的四个顶点（本地坐标）
        /// </summary>
        /// <returns>顶点数组：[左, 上, 右, 下]</returns>
        public static Vector3[] GetTileVertices()
        {
            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;

            return new Vector3[]
            {
                new Vector3(-halfW, 0, 0), // 左
                new Vector3(0, halfH, 0), // 上
                new Vector3(halfW, 0, 0), // 右
                new Vector3(0, -halfH, 0) // 下
            };
        }

        /// <summary>
        /// 获取指定格子的菱形顶点（世界坐标）
        /// </summary>
        public static Vector3[] GetTileVerticesAt(int cellX, int cellY, int floor = 0)
        {
            Vector3 center = CellCenterToScreen(cellX, cellY, floor);
            Vector3[] localVerts = GetTileVertices();

            for (int i = 0; i < localVerts.Length; i++)
            {
                localVerts[i] += center;
            }

            return localVerts;
        }

        /// <summary>
        /// 获取北墙的四个顶点（世界坐标）
        /// 北墙位于格子的上边缘
        /// </summary>
        public static Vector3[] GetNorthWallVertices(int cellX, int cellY, int floor = 0)
        {
            Vector3 cellPos = CellToScreen(cellX, cellY, floor);
            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;
            float wallH = MapDefine.WallWorldHeight;

            // 北墙从格子左上角到右上角
            // 左上角偏移：(-halfW + halfW, halfH) = (0, halfH) 不对
            // 实际上，格子的顶点是：左(-halfW, 0), 上(0, halfH), 右(halfW, 0), 下(0, -halfH)
            // 北墙连接"左"和"上"两个顶点

            Vector3 leftBottom = cellPos + new Vector3(-halfW, 0, 0);
            Vector3 rightBottom = cellPos + new Vector3(0, halfH, 0);
            Vector3 leftTop = leftBottom + new Vector3(0, wallH, 0);
            Vector3 rightTop = rightBottom + new Vector3(0, wallH, 0);

            return new Vector3[] { leftBottom, rightBottom, rightTop, leftTop };
        }

        /// <summary>
        /// 获取西墙的四个顶点（世界坐标）
        /// 西墙位于格子的左边缘
        /// </summary>
        public static Vector3[] GetWestWallVertices(int cellX, int cellY, int floor = 0)
        {
            Vector3 cellPos = CellToScreen(cellX, cellY, floor);
            float halfW = MapDefine.TileWorldWidth * 0.5f;
            float halfH = MapDefine.TileWorldHeight * 0.5f;
            float wallH = MapDefine.WallWorldHeight;

            // 西墙连接"下"和"左"两个顶点
            Vector3 leftBottom = cellPos + new Vector3(0, -halfH, 0);
            Vector3 rightBottom = cellPos + new Vector3(-halfW, 0, 0);
            Vector3 leftTop = leftBottom + new Vector3(0, wallH, 0);
            Vector3 rightTop = rightBottom + new Vector3(0, wallH, 0);

            return new Vector3[] { leftBottom, rightBottom, rightTop, leftTop };
        }

        #endregion

        #region 距离计算

        /// <summary>
        /// 计算两个格子间的曼哈顿距离
        /// </summary>
        public static int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
        }

        /// <summary>
        /// 计算两个格子间的切比雪夫距离（对角线距离）
        /// </summary>
        public static int ChebyshevDistance(int x1, int y1, int x2, int y2)
        {
            return Mathf.Max(Mathf.Abs(x1 - x2), Mathf.Abs(y1 - y2));
        }

        #endregion
    }
}
