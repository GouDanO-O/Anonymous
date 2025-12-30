/*******************************************************************************
 * 文件名:    Direction.cs
 * 描述:      方向枚举及相关工具方法
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Direction 用于表示四个基本方向（北东南西），
 *   Direction8 用于表示八个方向（包含对角线）。
 ******************************************************************************/

using System;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 四方向枚举（北东南西）
    /// 顺时针排列，值为0-3
    /// </summary>
    public enum Direction : byte
    {
        /// <summary>北 (Z+)</summary>
        North = 0,
        /// <summary>东 (X+)</summary>
        East = 1,
        /// <summary>南 (Z-)</summary>
        South = 2,
        /// <summary>西 (X-)</summary>
        West = 3
    }

    /// <summary>
    /// 八方向枚举（包含对角线）
    /// 顺时针排列，值为0-7
    /// </summary>
    public enum Direction8 : byte
    {
        /// <summary>北 (Z+)</summary>
        North = 0,
        /// <summary>东北 (X+, Z+)</summary>
        NorthEast = 1,
        /// <summary>东 (X+)</summary>
        East = 2,
        /// <summary>东南 (X+, Z-)</summary>
        SouthEast = 3,
        /// <summary>南 (Z-)</summary>
        South = 4,
        /// <summary>西南 (X-, Z-)</summary>
        SouthWest = 5,
        /// <summary>西 (X-)</summary>
        West = 6,
        /// <summary>西北 (X-, Z+)</summary>
        NorthWest = 7
    }

    /// <summary>
    /// 垂直方向枚举
    /// </summary>
    public enum VerticalDirection : byte
    {
        /// <summary>无垂直移动</summary>
        None = 0,
        /// <summary>向上</summary>
        Up = 1,
        /// <summary>向下</summary>
        Down = 2
    }

    /// <summary>
    /// Direction 扩展方法
    /// </summary>
    public static class DirectionExtensions
    {
        #region 方向偏移量

        /// <summary>
        /// 四方向数组（北、东、南、西）
        /// </summary>
        public static readonly Direction[] CardinalDirections = new Direction[]
        {
            Direction.North, Direction.East, Direction.South, Direction.West
        };

        /// <summary>
        /// 对角线方向数组（东北、东南、西南、西北）
        /// </summary>
        public static readonly Direction8[] DiagonalDirections = new Direction8[]
        {
            Direction8.NorthEast, Direction8.SouthEast, Direction8.SouthWest, Direction8.NorthWest
        };

        /// <summary>
        /// 四方向对应的CellCoord偏移量
        /// </summary>
        private static readonly CellCoord[] DirectionOffsets = new CellCoord[]
        {
            new CellCoord(0, 1),   // North
            new CellCoord(1, 0),   // East
            new CellCoord(0, -1),  // South
            new CellCoord(-1, 0)   // West
        };

        /// <summary>
        /// 八方向对应的CellCoord偏移量
        /// </summary>
        private static readonly CellCoord[] Direction8Offsets = new CellCoord[]
        {
            new CellCoord(0, 1),    // North
            new CellCoord(1, 1),    // NorthEast
            new CellCoord(1, 0),    // East
            new CellCoord(1, -1),   // SouthEast
            new CellCoord(0, -1),   // South
            new CellCoord(-1, -1),  // SouthWest
            new CellCoord(-1, 0),   // West
            new CellCoord(-1, 1)    // NorthWest
        };

        /// <summary>
        /// 四方向对应的Vector2Int偏移量
        /// </summary>
        private static readonly Vector2Int[] DirectionVector2Offsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // North
            new Vector2Int(1, 0),   // East
            new Vector2Int(0, -1),  // South
            new Vector2Int(-1, 0)   // West
        };

        /// <summary>
        /// 四方向对应的旋转角度
        /// </summary>
        private static readonly float[] DirectionAngles = new float[]
        {
            0f,    // North
            90f,   // East
            180f,  // South
            270f   // West
        };

        /// <summary>
        /// 八方向对应的旋转角度
        /// </summary>
        private static readonly float[] Direction8Angles = new float[]
        {
            0f,    // North
            45f,   // NorthEast
            90f,   // East
            135f,  // SouthEast
            180f,  // South
            225f,  // SouthWest
            270f,  // West
            315f   // NorthWest
        };

        #endregion

        #region Direction 扩展

        /// <summary>
        /// 获取方向对应的偏移量
        /// </summary>
        public static CellCoord ToOffset(this Direction dir)
        {
            return DirectionOffsets[(int)dir];
        }

        /// <summary>
        /// 获取方向对应的Vector2Int偏移量
        /// </summary>
        public static Vector2Int ToVector2Int(this Direction dir)
        {
            return DirectionVector2Offsets[(int)dir];
        }

        /// <summary>
        /// 获取方向对应的旋转角度（顺时针，0度=北）
        /// </summary>
        public static float ToAngle(this Direction dir)
        {
            return DirectionAngles[(int)dir];
        }

        /// <summary>
        /// 获取方向对应的Quaternion旋转（绕Y轴）
        /// </summary>
        public static Quaternion ToRotation(this Direction dir)
        {
            return Quaternion.Euler(0, DirectionAngles[(int)dir], 0);
        }

        /// <summary>
        /// 获取相反方向
        /// </summary>
        public static Direction Opposite(this Direction dir)
        {
            return (Direction)(((int)dir + 2) % 4);
        }

        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        public static Direction RotateCW(this Direction dir)
        {
            return (Direction)(((int)dir + 1) % 4);
        }

        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        public static Direction RotateCCW(this Direction dir)
        {
            return (Direction)(((int)dir + 3) % 4);
        }

        /// <summary>
        /// 旋转指定次数（正数顺时针，负数逆时针）
        /// </summary>
        public static Direction Rotate(this Direction dir, int times)
        {
            int result = ((int)dir + times) % 4;
            if (result < 0) result += 4;
            return (Direction)result;
        }

        /// <summary>
        /// 是否是水平方向（东或西）
        /// </summary>
        public static bool IsHorizontal(this Direction dir)
        {
            return dir == Direction.East || dir == Direction.West;
        }

        /// <summary>
        /// 是否是垂直方向（北或南）
        /// </summary>
        public static bool IsVertical(this Direction dir)
        {
            return dir == Direction.North || dir == Direction.South;
        }

        /// <summary>
        /// 转换为八方向
        /// </summary>
        public static Direction8 ToDirection8(this Direction dir)
        {
            return (Direction8)((int)dir * 2);
        }

        /// <summary>
        /// 获取方向的显示名称
        /// </summary>
        public static string ToDisplayName(this Direction dir)
        {
            return dir switch
            {
                Direction.North => "北",
                Direction.East => "东",
                Direction.South => "南",
                Direction.West => "西",
                _ => "未知"
            };
        }

        #endregion

        #region Direction8 扩展

        /// <summary>
        /// 获取八方向对应的偏移量
        /// </summary>
        public static CellCoord ToOffset(this Direction8 dir)
        {
            return Direction8Offsets[(int)dir];
        }

        /// <summary>
        /// 获取八方向对应的旋转角度
        /// </summary>
        public static float ToAngle(this Direction8 dir)
        {
            return Direction8Angles[(int)dir];
        }

        /// <summary>
        /// 获取八方向对应的Quaternion旋转
        /// </summary>
        public static Quaternion ToRotation(this Direction8 dir)
        {
            return Quaternion.Euler(0, Direction8Angles[(int)dir], 0);
        }

        /// <summary>
        /// 获取相反方向
        /// </summary>
        public static Direction8 Opposite(this Direction8 dir)
        {
            return (Direction8)(((int)dir + 4) % 8);
        }

        /// <summary>
        /// 顺时针旋转45度
        /// </summary>
        public static Direction8 RotateCW45(this Direction8 dir)
        {
            return (Direction8)(((int)dir + 1) % 8);
        }

        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        public static Direction8 RotateCW90(this Direction8 dir)
        {
            return (Direction8)(((int)dir + 2) % 8);
        }

        /// <summary>
        /// 逆时针旋转45度
        /// </summary>
        public static Direction8 RotateCCW45(this Direction8 dir)
        {
            return (Direction8)(((int)dir + 7) % 8);
        }

        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        public static Direction8 RotateCCW90(this Direction8 dir)
        {
            return (Direction8)(((int)dir + 6) % 8);
        }

        /// <summary>
        /// 是否是对角线方向
        /// </summary>
        public static bool IsDiagonal(this Direction8 dir)
        {
            return ((int)dir % 2) == 1;
        }

        /// <summary>
        /// 是否是基本方向（非对角线）
        /// </summary>
        public static bool IsCardinal(this Direction8 dir)
        {
            return ((int)dir % 2) == 0;
        }

        /// <summary>
        /// 转换为四方向（对角线方向返回null）
        /// </summary>
        public static Direction? ToDirection4(this Direction8 dir)
        {
            if (dir.IsDiagonal())
                return null;
            return (Direction)((int)dir / 2);
        }

        /// <summary>
        /// 获取对角线方向相邻的两个基本方向
        /// </summary>
        public static (Direction, Direction)? GetAdjacentCardinals(this Direction8 dir)
        {
            if (!dir.IsDiagonal())
                return null;

            return dir switch
            {
                Direction8.NorthEast => (Direction.North, Direction.East),
                Direction8.SouthEast => (Direction.East, Direction.South),
                Direction8.SouthWest => (Direction.South, Direction.West),
                Direction8.NorthWest => (Direction.West, Direction.North),
                _ => null
            };
        }

        #endregion

        #region 从角度/向量创建方向

        /// <summary>
        /// 从角度创建四方向（四舍五入到最近的方向）
        /// </summary>
        /// <param name="angle">角度（0=北，顺时针）</param>
        public static Direction DirectionFromAngle(float angle)
        {
            // 标准化到0-360
            angle = angle % 360;
            if (angle < 0) angle += 360;

            // 四舍五入到最近的90度
            int index = Mathf.RoundToInt(angle / 90f) % 4;
            return (Direction)index;
        }

        /// <summary>
        /// 从角度创建八方向
        /// </summary>
        /// <param name="angle">角度（0=北，顺时针）</param>
        public static Direction8 Direction8FromAngle(float angle)
        {
            angle = angle % 360;
            if (angle < 0) angle += 360;

            int index = Mathf.RoundToInt(angle / 45f) % 8;
            return (Direction8)index;
        }

        /// <summary>
        /// 从向量创建四方向
        /// </summary>
        public static Direction DirectionFromVector(Vector2 vector)
        {
            if (vector.sqrMagnitude < 0.001f)
                return Direction.North;

            float angle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
            return DirectionFromAngle(angle);
        }

        /// <summary>
        /// 从向量创建八方向
        /// </summary>
        public static Direction8 Direction8FromVector(Vector2 vector)
        {
            if (vector.sqrMagnitude < 0.001f)
                return Direction8.North;

            float angle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
            return Direction8FromAngle(angle);
        }

        /// <summary>
        /// 从两个坐标计算方向
        /// </summary>
        public static Direction DirectionFromTo(CellCoord from, CellCoord to)
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
        /// 从两个坐标计算八方向
        /// </summary>
        public static Direction8 Direction8FromTo(CellCoord from, CellCoord to)
        {
            int dx = Math.Sign(to.x - from.x);
            int dz = Math.Sign(to.z - from.z);

            // 使用查找表
            // dx: -1, 0, 1 → 索引 0, 1, 2
            // dz: -1, 0, 1 → 索引 0, 1, 2
            int[,] directionLookup = new int[,]
            {
                // dz = -1      dz = 0       dz = 1
                { 5, 6, 7 },  // dx = -1 (SW, W, NW)
                { 4, -1, 0 }, // dx = 0  (S, -, N)
                { 3, 2, 1 }   // dx = 1  (SE, E, NE)
            };

            int result = directionLookup[dx + 1, dz + 1];
            return result >= 0 ? (Direction8)result : Direction8.North;
        }

        #endregion

        #region VerticalDirection 扩展

        /// <summary>
        /// 获取垂直方向对应的楼层偏移
        /// </summary>
        public static int ToFloorOffset(this VerticalDirection dir)
        {
            return dir switch
            {
                VerticalDirection.Up => 1,
                VerticalDirection.Down => -1,
                _ => 0
            };
        }

        /// <summary>
        /// 获取相反的垂直方向
        /// </summary>
        public static VerticalDirection Opposite(this VerticalDirection dir)
        {
            return dir switch
            {
                VerticalDirection.Up => VerticalDirection.Down,
                VerticalDirection.Down => VerticalDirection.Up,
                _ => VerticalDirection.None
            };
        }

        #endregion
    }
}
