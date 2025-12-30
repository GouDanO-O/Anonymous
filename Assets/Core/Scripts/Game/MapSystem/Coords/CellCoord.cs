/*******************************************************************************
 * 文件名:    CellCoord.cs
 * 描述:      单层格子坐标结构体，用于表示楼层内的二维格子位置
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   CellCoord 是地图系统的基础坐标类型，表示单个楼层内的格子位置。
 *   采用 X-Z 坐标系（俯视角，Y轴向上），与Unity的3D坐标系兼容。
 *   
 * 坐标系说明:
 *   - X轴: 水平方向（屏幕左右）
 *   - Z轴: 垂直方向（屏幕上下，俯视角）
 *   - 原点(0,0)在地图左下角
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 单层格子坐标（二维）
    /// 用于表示楼层内的格子位置
    /// </summary>
    [Serializable]
    public struct CellCoord : IEquatable<CellCoord>, IComparable<CellCoord>
    {
        #region 字段

        /// <summary>
        /// X坐标（水平方向）
        /// </summary>
        public int x;

        /// <summary>
        /// Z坐标（垂直方向，俯视角上下）
        /// </summary>
        public int z;

        #endregion

        #region 静态常量

        /// <summary>
        /// 无效坐标（用于表示空或错误状态）
        /// </summary>
        public static readonly CellCoord Invalid = new CellCoord(-1, -1);

        /// <summary>
        /// 零坐标（原点）
        /// </summary>
        public static readonly CellCoord Zero = new CellCoord(0, 0);

        /// <summary>
        /// 单位坐标
        /// </summary>
        public static readonly CellCoord One = new CellCoord(1, 1);

        /// <summary>
        /// 北方（Z+）
        /// </summary>
        public static readonly CellCoord North = new CellCoord(0, 1);

        /// <summary>
        /// 南方（Z-）
        /// </summary>
        public static readonly CellCoord South = new CellCoord(0, -1);

        /// <summary>
        /// 东方（X+）
        /// </summary>
        public static readonly CellCoord East = new CellCoord(1, 0);

        /// <summary>
        /// 西方（X-）
        /// </summary>
        public static readonly CellCoord West = new CellCoord(-1, 0);

        /// <summary>
        /// 四方向偏移数组（北、东、南、西）
        /// </summary>
        public static readonly CellCoord[] Cardinals = new CellCoord[]
        {
            North, East, South, West
        };

        /// <summary>
        /// 八方向偏移数组（包含对角线）
        /// </summary>
        public static readonly CellCoord[] AllDirections = new CellCoord[]
        {
            North,
            new CellCoord(1, 1),   // 东北
            East,
            new CellCoord(1, -1),  // 东南
            South,
            new CellCoord(-1, -1), // 西南
            West,
            new CellCoord(-1, 1)   // 西北
        };

        /// <summary>
        /// 对角线方向偏移数组
        /// </summary>
        public static readonly CellCoord[] Diagonals = new CellCoord[]
        {
            new CellCoord(1, 1),   // 东北
            new CellCoord(1, -1),  // 东南
            new CellCoord(-1, -1), // 西南
            new CellCoord(-1, 1)   // 西北
        };

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="z">Z坐标</param>
        public CellCoord(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        /// <summary>
        /// 从Vector2Int构造
        /// </summary>
        public CellCoord(Vector2Int v)
        {
            this.x = v.x;
            this.z = v.y;
        }

        /// <summary>
        /// 从Vector3Int构造（忽略Y分量）
        /// </summary>
        public CellCoord(Vector3Int v)
        {
            this.x = v.x;
            this.z = v.z;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 是否是有效坐标（非负）
        /// </summary>
        public bool IsValid => x >= 0 && z >= 0;

        /// <summary>
        /// 曼哈顿距离到原点
        /// </summary>
        public int ManhattanMagnitude => Mathf.Abs(x) + Mathf.Abs(z);

        /// <summary>
        /// 切比雪夫距离到原点（棋盘距离）
        /// </summary>
        public int ChebyshevMagnitude => Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));

        /// <summary>
        /// 欧几里得距离的平方到原点
        /// </summary>
        public int SqrMagnitude => x * x + z * z;

        /// <summary>
        /// 欧几里得距离到原点
        /// </summary>
        public float Magnitude => Mathf.Sqrt(SqrMagnitude);

        #endregion

        #region 索引转换

        /// <summary>
        /// 转换为一维数组索引
        /// </summary>
        /// <param name="sizeX">地图X方向尺寸</param>
        /// <returns>一维索引</returns>
        public int ToIndex(int sizeX)
        {
            return z * sizeX + x;
        }

        /// <summary>
        /// 从一维索引创建坐标
        /// </summary>
        /// <param name="index">一维索引</param>
        /// <param name="sizeX">地图X方向尺寸</param>
        /// <returns>格子坐标</returns>
        public static CellCoord FromIndex(int index, int sizeX)
        {
            return new CellCoord(index % sizeX, index / sizeX);
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 转换为Vector2Int
        /// </summary>
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, z);
        }

        /// <summary>
        /// 转换为Vector3Int（Y分量为0）
        /// </summary>
        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, 0, z);
        }

        /// <summary>
        /// 转换为世界坐标（格子中心）
        /// </summary>
        /// <param name="cellSize">格子尺寸，默认为1</param>
        /// <returns>世界坐标Vector2（X, Z平面）</returns>
        public Vector2 ToWorldPosition2D(float cellSize = 1f)
        {
            return new Vector2(
                (x + 0.5f) * cellSize,
                (z + 0.5f) * cellSize
            );
        }

        /// <summary>
        /// 转换为世界坐标（格子中心，3D）
        /// </summary>
        /// <param name="cellSize">格子尺寸，默认为1</param>
        /// <param name="y">Y坐标（高度），默认为0</param>
        /// <returns>世界坐标Vector3</returns>
        public Vector3 ToWorldPosition3D(float cellSize = 1f, float y = 0f)
        {
            return new Vector3(
                (x + 0.5f) * cellSize,
                y,
                (z + 0.5f) * cellSize
            );
        }

        /// <summary>
        /// 转换为世界坐标（格子左下角）
        /// </summary>
        /// <param name="cellSize">格子尺寸，默认为1</param>
        /// <returns>世界坐标Vector2</returns>
        public Vector2 ToWorldPositionCorner2D(float cellSize = 1f)
        {
            return new Vector2(x * cellSize, z * cellSize);
        }

        /// <summary>
        /// 从世界坐标转换（向下取整）
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="cellSize">格子尺寸，默认为1</param>
        /// <returns>格子坐标</returns>
        public static CellCoord FromWorldPosition(Vector2 worldPos, float cellSize = 1f)
        {
            return new CellCoord(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.y / cellSize)
            );
        }

        /// <summary>
        /// 从世界坐标转换（3D，使用X和Z分量）
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="cellSize">格子尺寸，默认为1</param>
        /// <returns>格子坐标</returns>
        public static CellCoord FromWorldPosition(Vector3 worldPos, float cellSize = 1f)
        {
            return new CellCoord(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }

        #endregion

        #region 邻居获取

        /// <summary>
        /// 获取四方向邻居（上下左右）
        /// </summary>
        public IEnumerable<CellCoord> GetNeighbors4()
        {
            yield return new CellCoord(x, z + 1);     // 北
            yield return new CellCoord(x + 1, z);     // 东
            yield return new CellCoord(x, z - 1);     // 南
            yield return new CellCoord(x - 1, z);     // 西
        }

        /// <summary>
        /// 获取八方向邻居（包含对角线）
        /// </summary>
        public IEnumerable<CellCoord> GetNeighbors8()
        {
            yield return new CellCoord(x, z + 1);     // 北
            yield return new CellCoord(x + 1, z + 1); // 东北
            yield return new CellCoord(x + 1, z);     // 东
            yield return new CellCoord(x + 1, z - 1); // 东南
            yield return new CellCoord(x, z - 1);     // 南
            yield return new CellCoord(x - 1, z - 1); // 西南
            yield return new CellCoord(x - 1, z);     // 西
            yield return new CellCoord(x - 1, z + 1); // 西北
        }

        /// <summary>
        /// 获取指定范围内的所有邻居（曼哈顿距离）
        /// </summary>
        /// <param name="radius">范围半径</param>
        /// <param name="includeSelf">是否包含自身</param>
        public IEnumerable<CellCoord> GetNeighborsInRange(int radius, bool includeSelf = false)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    if (!includeSelf && dx == 0 && dz == 0)
                        continue;

                    if (Mathf.Abs(dx) + Mathf.Abs(dz) <= radius)
                    {
                        yield return new CellCoord(x + dx, z + dz);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定范围内的所有格子（正方形范围）
        /// </summary>
        /// <param name="radius">范围半径</param>
        /// <param name="includeSelf">是否包含自身</param>
        public IEnumerable<CellCoord> GetCellsInSquare(int radius, bool includeSelf = false)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    if (!includeSelf && dx == 0 && dz == 0)
                        continue;

                    yield return new CellCoord(x + dx, z + dz);
                }
            }
        }

        #endregion

        #region 距离计算

        /// <summary>
        /// 计算到另一坐标的曼哈顿距离
        /// </summary>
        public int ManhattanDistance(CellCoord other)
        {
            return Mathf.Abs(x - other.x) + Mathf.Abs(z - other.z);
        }

        /// <summary>
        /// 计算到另一坐标的切比雪夫距离（棋盘距离）
        /// </summary>
        public int ChebyshevDistance(CellCoord other)
        {
            return Mathf.Max(Mathf.Abs(x - other.x), Mathf.Abs(z - other.z));
        }

        /// <summary>
        /// 计算到另一坐标的欧几里得距离的平方
        /// </summary>
        public int SqrDistance(CellCoord other)
        {
            int dx = x - other.x;
            int dz = z - other.z;
            return dx * dx + dz * dz;
        }

        /// <summary>
        /// 计算到另一坐标的欧几里得距离
        /// </summary>
        public float Distance(CellCoord other)
        {
            return Mathf.Sqrt(SqrDistance(other));
        }

        #endregion

        #region 边界检查

        /// <summary>
        /// 检查是否在指定范围内
        /// </summary>
        /// <param name="sizeX">X方向尺寸</param>
        /// <param name="sizeZ">Z方向尺寸</param>
        public bool InBounds(int sizeX, int sizeZ)
        {
            return x >= 0 && x < sizeX && z >= 0 && z < sizeZ;
        }

        /// <summary>
        /// 检查是否在指定范围内
        /// </summary>
        /// <param name="minX">最小X</param>
        /// <param name="minZ">最小Z</param>
        /// <param name="maxX">最大X（不包含）</param>
        /// <param name="maxZ">最大Z（不包含）</param>
        public bool InBounds(int minX, int minZ, int maxX, int maxZ)
        {
            return x >= minX && x < maxX && z >= minZ && z < maxZ;
        }

        /// <summary>
        /// 将坐标限制在指定范围内
        /// </summary>
        /// <param name="sizeX">X方向尺寸</param>
        /// <param name="sizeZ">Z方向尺寸</param>
        public CellCoord Clamp(int sizeX, int sizeZ)
        {
            return new CellCoord(
                Mathf.Clamp(x, 0, sizeX - 1),
                Mathf.Clamp(z, 0, sizeZ - 1)
            );
        }

        /// <summary>
        /// 将坐标限制在指定范围内
        /// </summary>
        public CellCoord Clamp(int minX, int minZ, int maxX, int maxZ)
        {
            return new CellCoord(
                Mathf.Clamp(x, minX, maxX - 1),
                Mathf.Clamp(z, minZ, maxZ - 1)
            );
        }

        #endregion

        #region 运算符重载

        public static CellCoord operator +(CellCoord a, CellCoord b)
        {
            return new CellCoord(a.x + b.x, a.z + b.z);
        }

        public static CellCoord operator -(CellCoord a, CellCoord b)
        {
            return new CellCoord(a.x - b.x, a.z - b.z);
        }

        public static CellCoord operator *(CellCoord a, int scale)
        {
            return new CellCoord(a.x * scale, a.z * scale);
        }

        public static CellCoord operator *(int scale, CellCoord a)
        {
            return new CellCoord(a.x * scale, a.z * scale);
        }

        public static CellCoord operator /(CellCoord a, int divisor)
        {
            return new CellCoord(a.x / divisor, a.z / divisor);
        }

        public static CellCoord operator -(CellCoord a)
        {
            return new CellCoord(-a.x, -a.z);
        }

        public static bool operator ==(CellCoord a, CellCoord b)
        {
            return a.x == b.x && a.z == b.z;
        }

        public static bool operator !=(CellCoord a, CellCoord b)
        {
            return a.x != b.x || a.z != b.z;
        }

        #endregion

        #region 隐式/显式转换

        public static implicit operator Vector2Int(CellCoord coord)
        {
            return new Vector2Int(coord.x, coord.z);
        }

        public static implicit operator CellCoord(Vector2Int v)
        {
            return new CellCoord(v.x, v.y);
        }

        public static explicit operator Vector3Int(CellCoord coord)
        {
            return new Vector3Int(coord.x, 0, coord.z);
        }

        public static explicit operator CellCoord(Vector3Int v)
        {
            return new CellCoord(v.x, v.z);
        }

        #endregion

        #region IEquatable, IComparable 实现

        public bool Equals(CellCoord other)
        {
            return x == other.x && z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is CellCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            // 使用位运算优化哈希计算
            unchecked
            {
                return (x * 397) ^ z;
            }
        }

        /// <summary>
        /// 比较顺序：先Z后X（从下到上，从左到右）
        /// </summary>
        public int CompareTo(CellCoord other)
        {
            int zCompare = z.CompareTo(other.z);
            if (zCompare != 0)
                return zCompare;
            return x.CompareTo(other.x);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"({x}, {z})";
        }

        /// <summary>
        /// 格式化输出
        /// </summary>
        public string ToString(string format)
        {
            return $"({x.ToString(format)}, {z.ToString(format)})";
        }

        #endregion

        #region 静态工具方法

        /// <summary>
        /// 线性插值（返回浮点坐标）
        /// </summary>
        public static Vector2 Lerp(CellCoord a, CellCoord b, float t)
        {
            return new Vector2(
                Mathf.Lerp(a.x, b.x, t),
                Mathf.Lerp(a.z, b.z, t)
            );
        }

        /// <summary>
        /// 获取两点之间的所有格子（Bresenham直线算法）
        /// </summary>
        public static IEnumerable<CellCoord> GetCellsOnLine(CellCoord from, CellCoord to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dz = Mathf.Abs(to.z - from.z);
            int sx = from.x < to.x ? 1 : -1;
            int sz = from.z < to.z ? 1 : -1;
            int err = dx - dz;

            int currentX = from.x;
            int currentZ = from.z;

            while (true)
            {
                yield return new CellCoord(currentX, currentZ);

                if (currentX == to.x && currentZ == to.z)
                    break;

                int e2 = 2 * err;

                if (e2 > -dz)
                {
                    err -= dz;
                    currentX += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    currentZ += sz;
                }
            }
        }

        /// <summary>
        /// 获取矩形区域内的所有格子
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
        /// 获取圆形区域内的所有格子
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
        /// 获取最小包围矩形
        /// </summary>
        public static (CellCoord min, CellCoord max) GetBounds(IEnumerable<CellCoord> cells)
        {
            int minX = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxZ = int.MinValue;

            foreach (var cell in cells)
            {
                if (cell.x < minX) minX = cell.x;
                if (cell.z < minZ) minZ = cell.z;
                if (cell.x > maxX) maxX = cell.x;
                if (cell.z > maxZ) maxZ = cell.z;
            }

            return (new CellCoord(minX, minZ), new CellCoord(maxX, maxZ));
        }

        #endregion
    }
}
