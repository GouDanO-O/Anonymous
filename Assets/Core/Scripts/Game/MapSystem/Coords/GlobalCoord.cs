/*******************************************************************************
 * 文件名:    GlobalCoord.cs
 * 描述:      全局坐标结构体，包含楼层索引，用于跨楼层定位
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   GlobalCoord 是包含楼层信息的完整三维坐标，用于跨楼层的位置表示。
 *   Y分量表示楼层索引：0为地面层，正数为地上层，负数为地下层。
 *   
 * 坐标系说明:
 *   - X轴: 水平方向（屏幕左右）
 *   - Y轴: 楼层索引（不是高度）
 *   - Z轴: 垂直方向（屏幕上下，俯视角）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 全局坐标（三维，包含楼层）
    /// 用于跨楼层的位置表示
    /// </summary>
    [Serializable]
    public struct GlobalCoord : IEquatable<GlobalCoord>, IComparable<GlobalCoord>
    {
        #region 字段

        /// <summary>
        /// X坐标（水平方向）
        /// </summary>
        public int x;

        /// <summary>
        /// Y坐标（楼层索引）
        /// 0 = 地面层
        /// 正数 = 地上层（1楼、2楼...）
        /// 负数 = 地下层（-1层、-2层...）
        /// </summary>
        public int y;

        /// <summary>
        /// Z坐标（垂直方向，俯视角上下）
        /// </summary>
        public int z;

        #endregion

        #region 静态常量

        /// <summary>
        /// 无效坐标
        /// </summary>
        public static readonly GlobalCoord Invalid = new GlobalCoord(-1, int.MinValue, -1);

        /// <summary>
        /// 零坐标（地面层原点）
        /// </summary>
        public static readonly GlobalCoord Zero = new GlobalCoord(0, 0, 0);

        /// <summary>
        /// 向上一层
        /// </summary>
        public static readonly GlobalCoord Up = new GlobalCoord(0, 1, 0);

        /// <summary>
        /// 向下一层
        /// </summary>
        public static readonly GlobalCoord Down = new GlobalCoord(0, -1, 0);

        /// <summary>
        /// 北方（Z+，同层）
        /// </summary>
        public static readonly GlobalCoord North = new GlobalCoord(0, 0, 1);

        /// <summary>
        /// 南方（Z-，同层）
        /// </summary>
        public static readonly GlobalCoord South = new GlobalCoord(0, 0, -1);

        /// <summary>
        /// 东方（X+，同层）
        /// </summary>
        public static readonly GlobalCoord East = new GlobalCoord(1, 0, 0);

        /// <summary>
        /// 西方（X-，同层）
        /// </summary>
        public static readonly GlobalCoord West = new GlobalCoord(-1, 0, 0);

        /// <summary>
        /// 同层四方向
        /// </summary>
        public static readonly GlobalCoord[] HorizontalCardinals = new GlobalCoord[]
        {
            North, East, South, West
        };

        /// <summary>
        /// 垂直方向（上下楼层）
        /// </summary>
        public static readonly GlobalCoord[] VerticalDirections = new GlobalCoord[]
        {
            Up, Down
        };

        /// <summary>
        /// 所有六方向（上下东西南北）
        /// </summary>
        public static readonly GlobalCoord[] AllSixDirections = new GlobalCoord[]
        {
            North, East, South, West, Up, Down
        };

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">楼层索引</param>
        /// <param name="z">Z坐标</param>
        public GlobalCoord(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 从CellCoord和楼层索引构造
        /// </summary>
        /// <param name="cell">单层坐标</param>
        /// <param name="floorIndex">楼层索引</param>
        public GlobalCoord(CellCoord cell, int floorIndex)
        {
            this.x = cell.x;
            this.y = floorIndex;
            this.z = cell.z;
        }

        /// <summary>
        /// 从Vector3Int构造
        /// </summary>
        public GlobalCoord(Vector3Int v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 是否是有效坐标
        /// </summary>
        public bool IsValid => x >= 0 && z >= 0 && y != int.MinValue;

        /// <summary>
        /// 楼层索引（y的别名，更直观）
        /// </summary>
        public int FloorIndex => y;

        /// <summary>
        /// 是否在地面层
        /// </summary>
        public bool IsGroundFloor => y == 0;

        /// <summary>
        /// 是否在地上层
        /// </summary>
        public bool IsAboveGround => y > 0;

        /// <summary>
        /// 是否在地下层
        /// </summary>
        public bool IsUnderground => y < 0;

        /// <summary>
        /// 获取单层坐标（忽略楼层）
        /// </summary>
        public CellCoord CellCoord => new CellCoord(x, z);

        /// <summary>
        /// 同层曼哈顿距离到原点
        /// </summary>
        public int HorizontalManhattanMagnitude => Mathf.Abs(x) + Mathf.Abs(z);

        /// <summary>
        /// 三维曼哈顿距离到原点（包含楼层差）
        /// </summary>
        public int TotalManhattanMagnitude => Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);

        #endregion

        #region 索引转换

        /// <summary>
        /// 转换为全局一维索引
        /// </summary>
        /// <param name="sizeX">地图X方向尺寸</param>
        /// <param name="sizeZ">地图Z方向尺寸</param>
        /// <param name="minFloor">最低楼层索引</param>
        /// <returns>全局一维索引</returns>
        public int ToGlobalIndex(int sizeX, int sizeZ, int minFloor = 0)
        {
            int floorOffset = y - minFloor;
            int floorSize = sizeX * sizeZ;
            return floorOffset * floorSize + z * sizeX + x;
        }

        /// <summary>
        /// 从全局一维索引创建坐标
        /// </summary>
        /// <param name="index">全局一维索引</param>
        /// <param name="sizeX">地图X方向尺寸</param>
        /// <param name="sizeZ">地图Z方向尺寸</param>
        /// <param name="minFloor">最低楼层索引</param>
        /// <returns>全局坐标</returns>
        public static GlobalCoord FromGlobalIndex(int index, int sizeX, int sizeZ, int minFloor = 0)
        {
            int floorSize = sizeX * sizeZ;
            int floorOffset = index / floorSize;
            int remainder = index % floorSize;
            return new GlobalCoord(
                remainder % sizeX,
                floorOffset + minFloor,
                remainder / sizeX
            );
        }

        /// <summary>
        /// 获取单层内的一维索引
        /// </summary>
        /// <param name="sizeX">地图X方向尺寸</param>
        /// <returns>单层一维索引</returns>
        public int ToCellIndex(int sizeX)
        {
            return z * sizeX + x;
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 转换为Vector3Int
        /// </summary>
        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// 转换为世界坐标（格子中心）
        /// </summary>
        /// <param name="cellSize">格子尺寸</param>
        /// <param name="floorHeight">楼层高度</param>
        /// <returns>世界坐标Vector3</returns>
        public Vector3 ToWorldPosition(float cellSize = 1f, float floorHeight = 3f)
        {
            return new Vector3(
                (x + 0.5f) * cellSize,
                y * floorHeight,
                (z + 0.5f) * cellSize
            );
        }

        /// <summary>
        /// 从世界坐标转换
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="cellSize">格子尺寸</param>
        /// <param name="floorHeight">楼层高度</param>
        /// <returns>全局坐标</returns>
        public static GlobalCoord FromWorldPosition(Vector3 worldPos, float cellSize = 1f, float floorHeight = 3f)
        {
            return new GlobalCoord(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.RoundToInt(worldPos.y / floorHeight),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }

        /// <summary>
        /// 转换为同楼层的CellCoord
        /// </summary>
        public CellCoord ToCellCoord()
        {
            return new CellCoord(x, z);
        }

        /// <summary>
        /// 从CellCoord和楼层创建
        /// </summary>
        public static GlobalCoord FromCellCoord(CellCoord cell, int floorIndex)
        {
            return new GlobalCoord(cell.x, floorIndex, cell.z);
        }

        #endregion

        #region 楼层操作

        /// <summary>
        /// 移动到指定楼层（保持XZ坐标不变）
        /// </summary>
        /// <param name="newFloorIndex">新楼层索引</param>
        /// <returns>新坐标</returns>
        public GlobalCoord WithFloor(int newFloorIndex)
        {
            return new GlobalCoord(x, newFloorIndex, z);
        }

        /// <summary>
        /// 向上移动指定层数
        /// </summary>
        /// <param name="floors">层数（正数向上，负数向下）</param>
        /// <returns>新坐标</returns>
        public GlobalCoord MoveFloors(int floors)
        {
            return new GlobalCoord(x, y + floors, z);
        }

        /// <summary>
        /// 上一层
        /// </summary>
        public GlobalCoord FloorAbove => new GlobalCoord(x, y + 1, z);

        /// <summary>
        /// 下一层
        /// </summary>
        public GlobalCoord FloorBelow => new GlobalCoord(x, y - 1, z);

        #endregion

        #region 邻居获取

        /// <summary>
        /// 获取同层四方向邻居
        /// </summary>
        public IEnumerable<GlobalCoord> GetHorizontalNeighbors4()
        {
            yield return new GlobalCoord(x, y, z + 1);     // 北
            yield return new GlobalCoord(x + 1, y, z);     // 东
            yield return new GlobalCoord(x, y, z - 1);     // 南
            yield return new GlobalCoord(x - 1, y, z);     // 西
        }

        /// <summary>
        /// 获取同层八方向邻居（包含对角线）
        /// </summary>
        public IEnumerable<GlobalCoord> GetHorizontalNeighbors8()
        {
            yield return new GlobalCoord(x, y, z + 1);         // 北
            yield return new GlobalCoord(x + 1, y, z + 1);     // 东北
            yield return new GlobalCoord(x + 1, y, z);         // 东
            yield return new GlobalCoord(x + 1, y, z - 1);     // 东南
            yield return new GlobalCoord(x, y, z - 1);         // 南
            yield return new GlobalCoord(x - 1, y, z - 1);     // 西南
            yield return new GlobalCoord(x - 1, y, z);         // 西
            yield return new GlobalCoord(x - 1, y, z + 1);     // 西北
        }

        /// <summary>
        /// 获取垂直邻居（上下楼层同位置）
        /// </summary>
        public IEnumerable<GlobalCoord> GetVerticalNeighbors()
        {
            yield return new GlobalCoord(x, y + 1, z);  // 上层
            yield return new GlobalCoord(x, y - 1, z);  // 下层
        }

        /// <summary>
        /// 获取所有六方向邻居（同层四方向 + 上下层）
        /// </summary>
        public IEnumerable<GlobalCoord> GetAllNeighbors6()
        {
            // 同层四方向
            yield return new GlobalCoord(x, y, z + 1);     // 北
            yield return new GlobalCoord(x + 1, y, z);     // 东
            yield return new GlobalCoord(x, y, z - 1);     // 南
            yield return new GlobalCoord(x - 1, y, z);     // 西
            // 垂直方向
            yield return new GlobalCoord(x, y + 1, z);     // 上
            yield return new GlobalCoord(x, y - 1, z);     // 下
        }

        #endregion

        #region 距离计算

        /// <summary>
        /// 计算同层曼哈顿距离（忽略楼层差）
        /// </summary>
        public int HorizontalManhattanDistance(GlobalCoord other)
        {
            return Mathf.Abs(x - other.x) + Mathf.Abs(z - other.z);
        }

        /// <summary>
        /// 计算楼层差
        /// </summary>
        public int FloorDifference(GlobalCoord other)
        {
            return Mathf.Abs(y - other.y);
        }

        /// <summary>
        /// 计算三维曼哈顿距离（包含楼层差）
        /// 注意：楼层差应该乘以权重，因为上下楼比平移代价更高
        /// </summary>
        /// <param name="other">目标坐标</param>
        /// <param name="floorWeight">楼层权重，默认为10（上下楼成本更高）</param>
        public int WeightedManhattanDistance(GlobalCoord other, int floorWeight = 10)
        {
            int horizontal = Mathf.Abs(x - other.x) + Mathf.Abs(z - other.z);
            int vertical = Mathf.Abs(y - other.y) * floorWeight;
            return horizontal + vertical;
        }

        /// <summary>
        /// 是否在同一楼层
        /// </summary>
        public bool SameFloor(GlobalCoord other)
        {
            return y == other.y;
        }

        /// <summary>
        /// 是否在同一位置（同楼层同格子）
        /// </summary>
        public bool SamePosition(GlobalCoord other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        /// <summary>
        /// 是否在同一垂直线上（同XZ，不同楼层）
        /// </summary>
        public bool SameVerticalLine(GlobalCoord other)
        {
            return x == other.x && z == other.z;
        }

        #endregion

        #region 边界检查

        /// <summary>
        /// 检查是否在指定范围内
        /// </summary>
        /// <param name="sizeX">X方向尺寸</param>
        /// <param name="sizeZ">Z方向尺寸</param>
        /// <param name="minFloor">最低楼层</param>
        /// <param name="maxFloor">最高楼层（包含）</param>
        public bool InBounds(int sizeX, int sizeZ, int minFloor, int maxFloor)
        {
            return x >= 0 && x < sizeX &&
                   z >= 0 && z < sizeZ &&
                   y >= minFloor && y <= maxFloor;
        }

        /// <summary>
        /// 检查同层是否在范围内
        /// </summary>
        public bool InHorizontalBounds(int sizeX, int sizeZ)
        {
            return x >= 0 && x < sizeX && z >= 0 && z < sizeZ;
        }

        /// <summary>
        /// 将坐标限制在指定范围内
        /// </summary>
        public GlobalCoord Clamp(int sizeX, int sizeZ, int minFloor, int maxFloor)
        {
            return new GlobalCoord(
                Mathf.Clamp(x, 0, sizeX - 1),
                Mathf.Clamp(y, minFloor, maxFloor),
                Mathf.Clamp(z, 0, sizeZ - 1)
            );
        }

        #endregion

        #region 运算符重载

        public static GlobalCoord operator +(GlobalCoord a, GlobalCoord b)
        {
            return new GlobalCoord(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static GlobalCoord operator -(GlobalCoord a, GlobalCoord b)
        {
            return new GlobalCoord(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static GlobalCoord operator *(GlobalCoord a, int scale)
        {
            return new GlobalCoord(a.x * scale, a.y * scale, a.z * scale);
        }

        public static GlobalCoord operator -(GlobalCoord a)
        {
            return new GlobalCoord(-a.x, -a.y, -a.z);
        }

        public static bool operator ==(GlobalCoord a, GlobalCoord b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(GlobalCoord a, GlobalCoord b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        // 与CellCoord的运算（在同层内移动）
        public static GlobalCoord operator +(GlobalCoord global, CellCoord cell)
        {
            return new GlobalCoord(global.x + cell.x, global.y, global.z + cell.z);
        }

        public static GlobalCoord operator -(GlobalCoord global, CellCoord cell)
        {
            return new GlobalCoord(global.x - cell.x, global.y, global.z - cell.z);
        }

        #endregion

        #region 隐式/显式转换

        public static implicit operator Vector3Int(GlobalCoord coord)
        {
            return new Vector3Int(coord.x, coord.y, coord.z);
        }

        public static implicit operator GlobalCoord(Vector3Int v)
        {
            return new GlobalCoord(v.x, v.y, v.z);
        }

        /// <summary>
        /// 显式转换为CellCoord（丢失楼层信息）
        /// </summary>
        public static explicit operator CellCoord(GlobalCoord coord)
        {
            return new CellCoord(coord.x, coord.z);
        }

        #endregion

        #region IEquatable, IComparable 实现

        public bool Equals(GlobalCoord other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is GlobalCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = x;
                hash = (hash * 397) ^ y;
                hash = (hash * 397) ^ z;
                return hash;
            }
        }

        /// <summary>
        /// 比较顺序：先楼层，再Z，最后X
        /// </summary>
        public int CompareTo(GlobalCoord other)
        {
            int yCompare = y.CompareTo(other.y);
            if (yCompare != 0) return yCompare;

            int zCompare = z.CompareTo(other.z);
            if (zCompare != 0) return zCompare;

            return x.CompareTo(other.x);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"({x}, F{y}, {z})";
        }

        /// <summary>
        /// 详细格式输出
        /// </summary>
        public string ToDetailedString()
        {
            string floorName = y switch
            {
                0 => "Ground",
                > 0 => $"Floor {y}",
                < 0 => $"Basement {-y}"
            };
            return $"[{floorName}] ({x}, {z})";
        }

        #endregion

        #region 静态工具方法

        /// <summary>
        /// 获取两点之间经过的所有楼层
        /// </summary>
        public static IEnumerable<int> GetFloorsBetween(GlobalCoord from, GlobalCoord to)
        {
            int minFloor = Mathf.Min(from.y, to.y);
            int maxFloor = Mathf.Max(from.y, to.y);

            for (int floor = minFloor; floor <= maxFloor; floor++)
            {
                yield return floor;
            }
        }

        /// <summary>
        /// 创建指定楼层范围内同一XZ位置的所有坐标
        /// </summary>
        public static IEnumerable<GlobalCoord> GetVerticalStack(int x, int z, int minFloor, int maxFloor)
        {
            for (int floor = minFloor; floor <= maxFloor; floor++)
            {
                yield return new GlobalCoord(x, floor, z);
            }
        }

        /// <summary>
        /// 获取最小包围盒
        /// </summary>
        public static (GlobalCoord min, GlobalCoord max) GetBounds(IEnumerable<GlobalCoord> coords)
        {
            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;

            foreach (var coord in coords)
            {
                if (coord.x < minX) minX = coord.x;
                if (coord.y < minY) minY = coord.y;
                if (coord.z < minZ) minZ = coord.z;
                if (coord.x > maxX) maxX = coord.x;
                if (coord.y > maxY) maxY = coord.y;
                if (coord.z > maxZ) maxZ = coord.z;
            }

            return (new GlobalCoord(minX, minY, minZ), new GlobalCoord(maxX, maxY, maxZ));
        }

        #endregion
    }
}
