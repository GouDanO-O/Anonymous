/*******************************************************************************
 * 文件名:    IntVec2.cs
 * 描述:      轻量级二维整数向量结构体，主要用于尺寸表示
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   IntVec2 是一个轻量化的二维整数向量，主要用于：
 *   - 实体尺寸（宽度×高度）
 *   - 地图尺寸
 *   - 区域范围
 *   
 *   与 Unity 的 Vector2Int 兼容，但提供了更多游戏相关的实用方法。
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 二维整数向量（用于尺寸表示）
    /// </summary>
    [Serializable]
    public struct IntVec2 : IEquatable<IntVec2>
    {
        #region 字段

        /// <summary>
        /// X分量（宽度）
        /// </summary>
        public int x;

        /// <summary>
        /// Y分量（高度/深度）
        /// </summary>
        public int y;

        #endregion

        #region 静态常量

        /// <summary>
        /// 零向量
        /// </summary>
        public static readonly IntVec2 Zero = new IntVec2(0, 0);

        /// <summary>
        /// 单位向量
        /// </summary>
        public static readonly IntVec2 One = new IntVec2(1, 1);

        /// <summary>
        /// X轴单位向量
        /// </summary>
        public static readonly IntVec2 UnitX = new IntVec2(1, 0);

        /// <summary>
        /// Y轴单位向量
        /// </summary>
        public static readonly IntVec2 UnitY = new IntVec2(0, 1);

        /// <summary>
        /// 无效尺寸
        /// </summary>
        public static readonly IntVec2 Invalid = new IntVec2(-1, -1);

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public IntVec2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 正方形构造函数
        /// </summary>
        public IntVec2(int size)
        {
            this.x = size;
            this.y = size;
        }

        /// <summary>
        /// 从Vector2Int构造
        /// </summary>
        public IntVec2(Vector2Int v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 宽度（x的别名）
        /// </summary>
        public int Width => x;

        /// <summary>
        /// 高度（y的别名）
        /// </summary>
        public int Height => y;

        /// <summary>
        /// 面积
        /// </summary>
        public int Area => x * y;

        /// <summary>
        /// 周长
        /// </summary>
        public int Perimeter => 2 * (x + y);

        /// <summary>
        /// 是否是有效尺寸（正数）
        /// </summary>
        public bool IsValid => x > 0 && y > 0;

        /// <summary>
        /// 是否是正方形
        /// </summary>
        public bool IsSquare => x == y;

        /// <summary>
        /// 是否是单格子（1x1）
        /// </summary>
        public bool IsSingleCell => x == 1 && y == 1;

        /// <summary>
        /// 对角线长度（欧几里得）
        /// </summary>
        public float Diagonal => Mathf.Sqrt(x * x + y * y);

        #endregion

        #region 尺寸操作

        /// <summary>
        /// 交换宽高（旋转90度后的尺寸）
        /// </summary>
        public IntVec2 Swapped => new IntVec2(y, x);

        /// <summary>
        /// 根据旋转获取实际尺寸
        /// </summary>
        public IntVec2 RotatedSize(Rotation rotation)
        {
            return rotation.IsHorizontal ? Swapped : this;
        }

        /// <summary>
        /// 扩展尺寸
        /// </summary>
        public IntVec2 Expand(int amount)
        {
            return new IntVec2(x + amount, y + amount);
        }

        /// <summary>
        /// 扩展尺寸（分别指定）
        /// </summary>
        public IntVec2 Expand(int expandX, int expandY)
        {
            return new IntVec2(x + expandX, y + expandY);
        }

        /// <summary>
        /// 收缩尺寸
        /// </summary>
        public IntVec2 Shrink(int amount)
        {
            return new IntVec2(Mathf.Max(1, x - amount), Mathf.Max(1, y - amount));
        }

        /// <summary>
        /// 限制尺寸在范围内
        /// </summary>
        public IntVec2 Clamp(int minSize, int maxSize)
        {
            return new IntVec2(
                Mathf.Clamp(x, minSize, maxSize),
                Mathf.Clamp(y, minSize, maxSize)
            );
        }

        /// <summary>
        /// 限制尺寸在范围内
        /// </summary>
        public IntVec2 Clamp(IntVec2 min, IntVec2 max)
        {
            return new IntVec2(
                Mathf.Clamp(x, min.x, max.x),
                Mathf.Clamp(y, min.y, max.y)
            );
        }

        #endregion

        #region 格子枚举

        /// <summary>
        /// 枚举该尺寸范围内的所有格子（从原点开始）
        /// </summary>
        public IEnumerable<CellCoord> EnumerateCells()
        {
            for (int cy = 0; cy < y; cy++)
            {
                for (int cx = 0; cx < x; cx++)
                {
                    yield return new CellCoord(cx, cy);
                }
            }
        }

        /// <summary>
        /// 枚举该尺寸范围内的所有格子（指定原点）
        /// </summary>
        public IEnumerable<CellCoord> EnumerateCells(CellCoord origin)
        {
            for (int cy = 0; cy < y; cy++)
            {
                for (int cx = 0; cx < x; cx++)
                {
                    yield return new CellCoord(origin.x + cx, origin.z + cy);
                }
            }
        }

        /// <summary>
        /// 枚举边界格子
        /// </summary>
        public IEnumerable<CellCoord> EnumerateBorder()
        {
            // 底边
            for (int cx = 0; cx < x; cx++)
                yield return new CellCoord(cx, 0);

            // 右边（不含底角）
            for (int cy = 1; cy < y; cy++)
                yield return new CellCoord(x - 1, cy);

            // 顶边（不含右角）
            if (y > 1)
            {
                for (int cx = x - 2; cx >= 0; cx--)
                    yield return new CellCoord(cx, y - 1);
            }

            // 左边（不含两角）
            if (x > 1)
            {
                for (int cy = y - 2; cy >= 1; cy--)
                    yield return new CellCoord(0, cy);
            }
        }

        /// <summary>
        /// 枚举角落格子
        /// </summary>
        public IEnumerable<CellCoord> EnumerateCorners()
        {
            yield return new CellCoord(0, 0);         // 左下
            yield return new CellCoord(x - 1, 0);     // 右下
            yield return new CellCoord(x - 1, y - 1); // 右上
            yield return new CellCoord(0, y - 1);     // 左上
        }

        /// <summary>
        /// 获取中心格子
        /// </summary>
        public CellCoord GetCenter()
        {
            return new CellCoord(x / 2, y / 2);
        }

        /// <summary>
        /// 获取中心格子（指定原点）
        /// </summary>
        public CellCoord GetCenter(CellCoord origin)
        {
            return new CellCoord(origin.x + x / 2, origin.z + y / 2);
        }

        #endregion

        #region 包含检查

        /// <summary>
        /// 检查坐标是否在范围内（0到size-1）
        /// </summary>
        public bool Contains(int cx, int cy)
        {
            return cx >= 0 && cx < x && cy >= 0 && cy < y;
        }

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool Contains(CellCoord cell)
        {
            return cell.x >= 0 && cell.x < x && cell.z >= 0 && cell.z < y;
        }

        /// <summary>
        /// 检查坐标是否在指定原点的范围内
        /// </summary>
        public bool Contains(CellCoord cell, CellCoord origin)
        {
            int localX = cell.x - origin.x;
            int localZ = cell.z - origin.z;
            return localX >= 0 && localX < x && localZ >= 0 && localZ < y;
        }

        #endregion

        #region 转换

        /// <summary>
        /// 转换为Vector2Int
        /// </summary>
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 转换为Vector2
        /// </summary>
        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        /// <summary>
        /// 转换为世界尺寸
        /// </summary>
        public Vector2 ToWorldSize(float cellSize = 1f)
        {
            return new Vector2(x * cellSize, y * cellSize);
        }

        /// <summary>
        /// 获取一维数组索引
        /// </summary>
        public int ToIndex(int cellX, int cellY)
        {
            return cellY * x + cellX;
        }

        /// <summary>
        /// 从一维索引获取坐标
        /// </summary>
        public (int x, int y) FromIndex(int index)
        {
            return (index % x, index / x);
        }

        #endregion

        #region 运算符重载

        public static IntVec2 operator +(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(a.x + b.x, a.y + b.y);
        }

        public static IntVec2 operator -(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(a.x - b.x, a.y - b.y);
        }

        public static IntVec2 operator *(IntVec2 a, int scale)
        {
            return new IntVec2(a.x * scale, a.y * scale);
        }

        public static IntVec2 operator *(int scale, IntVec2 a)
        {
            return new IntVec2(a.x * scale, a.y * scale);
        }

        public static IntVec2 operator /(IntVec2 a, int divisor)
        {
            return new IntVec2(a.x / divisor, a.y / divisor);
        }

        public static bool operator ==(IntVec2 a, IntVec2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(IntVec2 a, IntVec2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        #endregion

        #region 隐式转换

        public static implicit operator Vector2Int(IntVec2 v)
        {
            return new Vector2Int(v.x, v.y);
        }

        public static implicit operator IntVec2(Vector2Int v)
        {
            return new IntVec2(v.x, v.y);
        }

        public static implicit operator (int, int)(IntVec2 v)
        {
            return (v.x, v.y);
        }

        public static implicit operator IntVec2((int x, int y) tuple)
        {
            return new IntVec2(tuple.x, tuple.y);
        }

        #endregion

        #region IEquatable 实现

        public bool Equals(IntVec2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is IntVec2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{x}x{y}";
        }

        /// <summary>
        /// 详细格式
        /// </summary>
        public string ToDetailedString()
        {
            return $"Size({x}, {y}) Area={Area}";
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 获取两个尺寸的最大值
        /// </summary>
        public static IntVec2 Max(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        /// <summary>
        /// 获取两个尺寸的最小值
        /// </summary>
        public static IntVec2 Min(IntVec2 a, IntVec2 b)
        {
            return new IntVec2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }

        /// <summary>
        /// 解析字符串（格式："3x4" 或 "3,4"）
        /// </summary>
        public static IntVec2 Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Invalid;

            string[] parts = str.Split(new char[] { 'x', 'X', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return Invalid;

            if (int.TryParse(parts[0], out int px) && int.TryParse(parts[1], out int py))
                return new IntVec2(px, py);

            return Invalid;
        }

        /// <summary>
        /// 尝试解析字符串
        /// </summary>
        public static bool TryParse(string str, out IntVec2 result)
        {
            result = Parse(str);
            return result.IsValid;
        }

        #endregion
    }
}
