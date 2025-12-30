/*******************************************************************************
 * 文件名:    CoordUtility.cs
 * 描述:      坐标系统工具类，提供各种坐标转换、查询和计算的静态方法
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   CoordUtility 是坐标系统的核心工具类，提供：
 *   - 坐标转换（世界坐标 ↔ 格子坐标 ↔ 索引）
 *   - 范围查询（矩形、圆形、环形）
 *   - 路径计算（直线、Bresenham）
 *   - 边界检查和裁剪
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 坐标系统工具类
    /// </summary>
    public static class CoordUtility
    {
        #region 常量

        /// <summary>
        /// 默认格子尺寸（世界单位）
        /// </summary>
        public const float DefaultCellSize = 1f;

        /// <summary>
        /// 默认楼层高度（世界单位）
        /// </summary>
        public const float DefaultFloorHeight = 3f;

        #endregion

        #region 索引转换

        /// <summary>
        /// CellCoord转一维索引
        /// </summary>
        public static int CellToIndex(CellCoord cell, int sizeX)
        {
            return cell.z * sizeX + cell.x;
        }

        /// <summary>
        /// XZ坐标转一维索引
        /// </summary>
        public static int CellToIndex(int x, int z, int sizeX)
        {
            return z * sizeX + x;
        }

        /// <summary>
        /// 一维索引转CellCoord
        /// </summary>
        public static CellCoord IndexToCell(int index, int sizeX)
        {
            return new CellCoord(index % sizeX, index / sizeX);
        }

        /// <summary>
        /// GlobalCoord转全局一维索引
        /// </summary>
        public static int GlobalToIndex(GlobalCoord coord, int sizeX, int sizeZ, int minFloor)
        {
            return coord.ToGlobalIndex(sizeX, sizeZ, minFloor);
        }

        /// <summary>
        /// 全局一维索引转GlobalCoord
        /// </summary>
        public static GlobalCoord IndexToGlobal(int index, int sizeX, int sizeZ, int minFloor)
        {
            return GlobalCoord.FromGlobalIndex(index, sizeX, sizeZ, minFloor);
        }

        /// <summary>
        /// 检查索引是否有效
        /// </summary>
        public static bool IsValidIndex(int index, int sizeX, int sizeZ)
        {
            return index >= 0 && index < sizeX * sizeZ;
        }

        #endregion

        #region 世界坐标转换

        /// <summary>
        /// 世界坐标转CellCoord
        /// </summary>
        public static CellCoord WorldToCell(Vector2 worldPos, float cellSize = DefaultCellSize)
        {
            return new CellCoord(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.y / cellSize)
            );
        }

        /// <summary>
        /// 世界坐标转CellCoord（3D版本，使用XZ平面）
        /// </summary>
        public static CellCoord WorldToCell(Vector3 worldPos, float cellSize = DefaultCellSize)
        {
            return new CellCoord(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }

        /// <summary>
        /// 世界坐标转GlobalCoord
        /// </summary>
        public static GlobalCoord WorldToGlobal(Vector3 worldPos, float cellSize = DefaultCellSize, float floorHeight = DefaultFloorHeight)
        {
            return new GlobalCoord(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.RoundToInt(worldPos.y / floorHeight),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }

        /// <summary>
        /// CellCoord转世界坐标（格子中心）
        /// </summary>
        public static Vector2 CellToWorld2D(CellCoord cell, float cellSize = DefaultCellSize)
        {
            return new Vector2(
                (cell.x + 0.5f) * cellSize,
                (cell.z + 0.5f) * cellSize
            );
        }

        /// <summary>
        /// CellCoord转世界坐标（3D，格子中心）
        /// </summary>
        public static Vector3 CellToWorld3D(CellCoord cell, float y = 0f, float cellSize = DefaultCellSize)
        {
            return new Vector3(
                (cell.x + 0.5f) * cellSize,
                y,
                (cell.z + 0.5f) * cellSize
            );
        }

        /// <summary>
        /// GlobalCoord转世界坐标
        /// </summary>
        public static Vector3 GlobalToWorld(GlobalCoord coord, float cellSize = DefaultCellSize, float floorHeight = DefaultFloorHeight)
        {
            return new Vector3(
                (coord.x + 0.5f) * cellSize,
                coord.y * floorHeight,
                (coord.z + 0.5f) * cellSize
            );
        }

        /// <summary>
        /// 获取格子的世界坐标边界
        /// </summary>
        public static Rect GetCellWorldBounds(CellCoord cell, float cellSize = DefaultCellSize)
        {
            return new Rect(
                cell.x * cellSize,
                cell.z * cellSize,
                cellSize,
                cellSize
            );
        }

        /// <summary>
        /// 获取格子的世界坐标四角
        /// </summary>
        public static Vector2[] GetCellCorners2D(CellCoord cell, float cellSize = DefaultCellSize)
        {
            float minX = cell.x * cellSize;
            float minZ = cell.z * cellSize;
            float maxX = minX + cellSize;
            float maxZ = minZ + cellSize;

            return new Vector2[]
            {
                new Vector2(minX, minZ), // 左下
                new Vector2(maxX, minZ), // 右下
                new Vector2(maxX, maxZ), // 右上
                new Vector2(minX, maxZ)  // 左上
            };
        }

        #endregion

        #region 边界检查

        /// <summary>
        /// 检查CellCoord是否在地图范围内
        /// </summary>
        public static bool InBounds(CellCoord cell, int sizeX, int sizeZ)
        {
            return cell.x >= 0 && cell.x < sizeX && cell.z >= 0 && cell.z < sizeZ;
        }

        /// <summary>
        /// 检查GlobalCoord是否在地图范围内
        /// </summary>
        public static bool InBounds(GlobalCoord coord, int sizeX, int sizeZ, int minFloor, int maxFloor)
        {
            return coord.x >= 0 && coord.x < sizeX &&
                   coord.z >= 0 && coord.z < sizeZ &&
                   coord.y >= minFloor && coord.y <= maxFloor;
        }

        /// <summary>
        /// 将CellCoord裁剪到地图范围内
        /// </summary>
        public static CellCoord ClampToMap(CellCoord cell, int sizeX, int sizeZ)
        {
            return new CellCoord(
                Mathf.Clamp(cell.x, 0, sizeX - 1),
                Mathf.Clamp(cell.z, 0, sizeZ - 1)
            );
        }

        /// <summary>
        /// 将GlobalCoord裁剪到地图范围内
        /// </summary>
        public static GlobalCoord ClampToMap(GlobalCoord coord, int sizeX, int sizeZ, int minFloor, int maxFloor)
        {
            return new GlobalCoord(
                Mathf.Clamp(coord.x, 0, sizeX - 1),
                Mathf.Clamp(coord.y, minFloor, maxFloor),
                Mathf.Clamp(coord.z, 0, sizeZ - 1)
            );
        }

        /// <summary>
        /// 检查矩形区域是否完全在地图范围内
        /// </summary>
        public static bool RectInBounds(CellCoord min, CellCoord max, int sizeX, int sizeZ)
        {
            return min.x >= 0 && min.z >= 0 && max.x < sizeX && max.z < sizeZ;
        }

        #endregion

        #region 范围查询

        /// <summary>
        /// 获取矩形范围内所有格子
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsInRect(CellCoord min, CellCoord max)
        {
            for (int z = min.z; z <= max.z; z++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    yield return new CellCoord(x, z);
                }
            }
        }

        /// <summary>
        /// 获取矩形范围内所有格子（带边界检查）
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsInRectClamped(CellCoord min, CellCoord max, int sizeX, int sizeZ)
        {
            int minX = Mathf.Max(0, min.x);
            int minZ = Mathf.Max(0, min.z);
            int maxX = Mathf.Min(sizeX - 1, max.x);
            int maxZ = Mathf.Min(sizeZ - 1, max.z);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    yield return new CellCoord(x, z);
                }
            }
        }

        /// <summary>
        /// 获取圆形范围内所有格子
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsInCircle(CellCoord center, float radius)
        {
            int radiusCeil = Mathf.CeilToInt(radius);
            float radiusSqr = radius * radius;

            for (int dz = -radiusCeil; dz <= radiusCeil; dz++)
            {
                for (int dx = -radiusCeil; dx <= radiusCeil; dx++)
                {
                    if (dx * dx + dz * dz <= radiusSqr)
                    {
                        yield return new CellCoord(center.x + dx, center.z + dz);
                    }
                }
            }
        }

        /// <summary>
        /// 获取环形范围内所有格子（内半径到外半径）
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsInRing(CellCoord center, float innerRadius, float outerRadius)
        {
            int radiusCeil = Mathf.CeilToInt(outerRadius);
            float innerSqr = innerRadius * innerRadius;
            float outerSqr = outerRadius * outerRadius;

            for (int dz = -radiusCeil; dz <= radiusCeil; dz++)
            {
                for (int dx = -radiusCeil; dx <= radiusCeil; dx++)
                {
                    float distSqr = dx * dx + dz * dz;
                    if (distSqr >= innerSqr && distSqr <= outerSqr)
                    {
                        yield return new CellCoord(center.x + dx, center.z + dz);
                    }
                }
            }
        }

        /// <summary>
        /// 获取曼哈顿距离范围内的格子（菱形范围）
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsInManhattanRange(CellCoord center, int range, bool includeSelf = true)
        {
            for (int dz = -range; dz <= range; dz++)
            {
                int xRange = range - Mathf.Abs(dz);
                for (int dx = -xRange; dx <= xRange; dx++)
                {
                    if (!includeSelf && dx == 0 && dz == 0)
                        continue;
                    yield return new CellCoord(center.x + dx, center.z + dz);
                }
            }
        }

        /// <summary>
        /// 获取切比雪夫距离范围内的格子（正方形范围）
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsInChebyshevRange(CellCoord center, int range, bool includeSelf = true)
        {
            for (int dz = -range; dz <= range; dz++)
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    if (!includeSelf && dx == 0 && dz == 0)
                        continue;
                    yield return new CellCoord(center.x + dx, center.z + dz);
                }
            }
        }

        #endregion

        #region 路径和直线

        /// <summary>
        /// Bresenham直线算法 - 获取两点之间的所有格子
        /// </summary>
        public static List<CellCoord> GetCellsOnLine(CellCoord from, CellCoord to)
        {
            List<CellCoord> result = new List<CellCoord>();

            int dx = Mathf.Abs(to.x - from.x);
            int dz = Mathf.Abs(to.z - from.z);
            int sx = from.x < to.x ? 1 : -1;
            int sz = from.z < to.z ? 1 : -1;
            int err = dx - dz;

            int x = from.x;
            int z = from.z;

            while (true)
            {
                result.Add(new CellCoord(x, z));

                if (x == to.x && z == to.z)
                    break;

                int e2 = 2 * err;

                if (e2 > -dz)
                {
                    err -= dz;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    z += sz;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取两点之间的直线格子（迭代器版本）
        /// </summary>
        public static IEnumerable<CellCoord> EnumerateCellsOnLine(CellCoord from, CellCoord to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dz = Mathf.Abs(to.z - from.z);
            int sx = from.x < to.x ? 1 : -1;
            int sz = from.z < to.z ? 1 : -1;
            int err = dx - dz;

            int x = from.x;
            int z = from.z;

            while (true)
            {
                yield return new CellCoord(x, z);

                if (x == to.x && z == to.z)
                    break;

                int e2 = 2 * err;

                if (e2 > -dz)
                {
                    err -= dz;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    z += sz;
                }
            }
        }

        /// <summary>
        /// 检查两点之间是否有直线视野（无阻挡）
        /// </summary>
        /// <param name="from">起点</param>
        /// <param name="to">终点</param>
        /// <param name="isBlocked">判断格子是否阻挡的函数</param>
        /// <param name="ignoreStart">是否忽略起点的阻挡检查</param>
        /// <param name="ignoreEnd">是否忽略终点的阻挡检查</param>
        public static bool HasLineOfSight(CellCoord from, CellCoord to, Func<CellCoord, bool> isBlocked, 
            bool ignoreStart = true, bool ignoreEnd = false)
        {
            bool isFirst = true;
            CellCoord lastCell = from;

            foreach (var cell in EnumerateCellsOnLine(from, to))
            {
                if (isFirst && ignoreStart)
                {
                    isFirst = false;
                    continue;
                }

                if (cell == to && ignoreEnd)
                    continue;

                if (isBlocked(cell))
                    return false;

                lastCell = cell;
            }

            return true;
        }

        #endregion

        #region 多格子实体

        /// <summary>
        /// 获取多格子实体占据的所有格子
        /// </summary>
        /// <param name="origin">原点（左下角）</param>
        /// <param name="size">尺寸</param>
        /// <param name="rotation">旋转</param>
        public static IEnumerable<CellCoord> GetOccupiedCells(CellCoord origin, Vector2Int size, Rotation rotation)
        {
            // 根据旋转调整尺寸
            Vector2Int rotatedSize = rotation.RotateSize(size);

            for (int dz = 0; dz < rotatedSize.y; dz++)
            {
                for (int dx = 0; dx < rotatedSize.x; dx++)
                {
                    yield return new CellCoord(origin.x + dx, origin.z + dz);
                }
            }
        }

        /// <summary>
        /// 获取多格子实体的中心坐标
        /// </summary>
        public static Vector2 GetEntityCenter(CellCoord origin, Vector2Int size, Rotation rotation, float cellSize = DefaultCellSize)
        {
            Vector2Int rotatedSize = rotation.RotateSize(size);
            return new Vector2(
                (origin.x + rotatedSize.x * 0.5f) * cellSize,
                (origin.z + rotatedSize.y * 0.5f) * cellSize
            );
        }

        /// <summary>
        /// 获取多格子实体的边界
        /// </summary>
        public static (CellCoord min, CellCoord max) GetEntityBounds(CellCoord origin, Vector2Int size, Rotation rotation)
        {
            Vector2Int rotatedSize = rotation.RotateSize(size);
            return (origin, new CellCoord(origin.x + rotatedSize.x - 1, origin.z + rotatedSize.y - 1));
        }

        /// <summary>
        /// 检查两个矩形区域是否重叠
        /// </summary>
        public static bool RectsOverlap(CellCoord minA, CellCoord maxA, CellCoord minB, CellCoord maxB)
        {
            return minA.x <= maxB.x && maxA.x >= minB.x &&
                   minA.z <= maxB.z && maxA.z >= minB.z;
        }

        #endregion

        #region 跨楼层

        /// <summary>
        /// 从CellCoord和楼层创建GlobalCoord
        /// </summary>
        public static GlobalCoord ToGlobal(CellCoord cell, int floorIndex)
        {
            return new GlobalCoord(cell.x, floorIndex, cell.z);
        }

        /// <summary>
        /// 从GlobalCoord提取CellCoord
        /// </summary>
        public static CellCoord ToCell(GlobalCoord coord)
        {
            return new CellCoord(coord.x, coord.z);
        }

        /// <summary>
        /// 获取两个全局坐标之间的楼层差
        /// </summary>
        public static int GetFloorDifference(GlobalCoord a, GlobalCoord b)
        {
            return Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 检查两个全局坐标是否在同一楼层
        /// </summary>
        public static bool SameFloor(GlobalCoord a, GlobalCoord b)
        {
            return a.y == b.y;
        }

        /// <summary>
        /// 检查两个全局坐标是否在同一垂直线上（可能不同楼层）
        /// </summary>
        public static bool SameVerticalLine(GlobalCoord a, GlobalCoord b)
        {
            return a.x == b.x && a.z == b.z;
        }

        #endregion

        #region 方向计算

        /// <summary>
        /// 计算从一个格子到另一个格子的四方向
        /// </summary>
        public static Direction GetDirection4(CellCoord from, CellCoord to)
        {
            int dx = to.x - from.x;
            int dz = to.z - from.z;

            if (Mathf.Abs(dx) > Mathf.Abs(dz))
            {
                return dx > 0 ? Direction.East : Direction.West;
            }
            else
            {
                return dz > 0 ? Direction.North : Direction.South;
            }
        }

        /// <summary>
        /// 计算从一个格子到另一个格子的八方向
        /// </summary>
        public static Direction8 GetDirection8(CellCoord from, CellCoord to)
        {
            return DirectionExtensions.Direction8FromTo(from, to);
        }

        /// <summary>
        /// 获取朝向指定方向移动后的坐标
        /// </summary>
        public static CellCoord MoveInDirection(CellCoord from, Direction dir, int distance = 1)
        {
            return from + dir.ToOffset() * distance;
        }

        /// <summary>
        /// 获取朝向指定八方向移动后的坐标
        /// </summary>
        public static CellCoord MoveInDirection8(CellCoord from, Direction8 dir, int distance = 1)
        {
            return from + dir.ToOffset() * distance;
        }

        #endregion

        #region 随机

        /// <summary>
        /// 在指定范围内获取随机格子
        /// </summary>
        public static CellCoord RandomInRect(CellCoord min, CellCoord max)
        {
            return new CellCoord(
                UnityEngine.Random.Range(min.x, max.x + 1),
                UnityEngine.Random.Range(min.z, max.z + 1)
            );
        }

        /// <summary>
        /// 在地图范围内获取随机格子
        /// </summary>
        public static CellCoord RandomInMap(int sizeX, int sizeZ)
        {
            return new CellCoord(
                UnityEngine.Random.Range(0, sizeX),
                UnityEngine.Random.Range(0, sizeZ)
            );
        }

        /// <summary>
        /// 在圆形范围内获取随机格子
        /// </summary>
        public static CellCoord RandomInCircle(CellCoord center, float radius)
        {
            int maxAttempts = 100;
            int radiusCeil = Mathf.CeilToInt(radius);
            float radiusSqr = radius * radius;

            for (int i = 0; i < maxAttempts; i++)
            {
                int dx = UnityEngine.Random.Range(-radiusCeil, radiusCeil + 1);
                int dz = UnityEngine.Random.Range(-radiusCeil, radiusCeil + 1);

                if (dx * dx + dz * dz <= radiusSqr)
                {
                    return new CellCoord(center.x + dx, center.z + dz);
                }
            }

            // 失败时返回中心
            return center;
        }

        /// <summary>
        /// 使用System.Random在地图范围内获取随机格子
        /// </summary>
        public static CellCoord RandomInMap(int sizeX, int sizeZ, System.Random random)
        {
            return new CellCoord(
                random.Next(0, sizeX),
                random.Next(0, sizeZ)
            );
        }

        #endregion
    }
}
