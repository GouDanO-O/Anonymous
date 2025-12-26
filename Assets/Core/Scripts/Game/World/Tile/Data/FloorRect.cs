using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Game.Map.Tile.Data
{
    /// <summary>
    /// 楼层内的2D矩形区域
    /// 参照RimWorld的CellRect设计
    /// </summary>
    public struct FloorRect : IEquatable<FloorRect>, IEnumerable<Vector2Int>, IEnumerable
    {
        #region 字段

        public int minX;
        public int maxX;
        public int minZ;
        public int maxZ;

        #endregion

        #region 静态常量

        public static FloorRect Empty => new FloorRect(0, 0, 0, 0);

        #endregion

        #region 属性

        public readonly bool IsEmpty
        {
            get
            {
                if (Width > 0)
                    return Height <= 0;
                return true;
            }
        }

        public readonly int Area => Width * Height;

        public int Width
        {
            readonly get
            {
                if (minX > maxX) return 0;
                return maxX - minX + 1;
            }
            set
            {
                maxX = minX + Mathf.Max(value, 0) - 1;
            }
        }

        public int Height
        {
            readonly get
            {
                if (minZ > maxZ) return 0;
                return maxZ - minZ + 1;
            }
            set
            {
                maxZ = minZ + Mathf.Max(value, 0) - 1;
            }
        }

        public Vector2Int Size => new Vector2Int(Width, Height);

        public Vector2Int Min
        {
            get => new Vector2Int(minX, minZ);
            set
            {
                minX = value.x;
                minZ = value.y;
            }
        }

        public Vector2Int Max
        {
            get => new Vector2Int(maxX, maxZ);
            set
            {
                maxX = value.x;
                maxZ = value.y;
            }
        }

        public Vector2Int RandomCell => new Vector2Int(
            UnityEngine.Random.Range(minX, maxX + 1),
            UnityEngine.Random.Range(minZ, maxZ + 1)
        );

        public Vector2Int CenterCell => new Vector2Int(minX + Width / 2, minZ + Height / 2);

        public Vector3 CenterVector3 => new Vector3(
            (float)minX + (float)Width / 2f,
            0f,
            (float)minZ + (float)Height / 2f
        );

        #endregion

        #region 构造方法

        public FloorRect(int minX, int minZ, int width, int height)
        {
            this.minX = minX;
            this.minZ = minZ;
            this.maxX = minX + width - 1;
            this.maxZ = minZ + height - 1;
        }

        public static FloorRect FromLimits(int minX, int minZ, int maxX, int maxZ)
        {
            return new FloorRect
            {
                minX = Mathf.Min(minX, maxX),
                minZ = Mathf.Min(minZ, maxZ),
                maxX = Mathf.Max(maxX, minX),
                maxZ = Mathf.Max(maxZ, minZ)
            };
        }

        public static FloorRect FromLimits(Vector2Int first, Vector2Int second)
        {
            return new FloorRect
            {
                minX = Mathf.Min(first.x, second.x),
                minZ = Mathf.Min(first.y, second.y),
                maxX = Mathf.Max(first.x, second.x),
                maxZ = Mathf.Max(first.y, second.y)
            };
        }

        public static FloorRect CenteredOn(Vector2Int center, int radius)
        {
            return new FloorRect
            {
                minX = center.x - radius,
                maxX = center.x + radius,
                minZ = center.y - radius,
                maxZ = center.y + radius
            };
        }

        public static FloorRect CenteredOn(Vector2Int center, int width, int height)
        {
            FloorRect result = new FloorRect
            {
                minX = center.x - width / 2,
                minZ = center.y - height / 2
            };
            result.maxX = result.minX + width - 1;
            result.maxZ = result.minZ + height - 1;
            return result;
        }

        public static FloorRect SingleCell(Vector2Int c)
        {
            return new FloorRect(c.x, c.y, 1, 1);
        }

        #endregion

        #region Cell枚举

        public readonly IEnumerable<Vector2Int> Cells
        {
            get
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        yield return new Vector2Int(x, z);
                    }
                }
            }
        }

        public readonly IEnumerable<Vector2Int> EdgeCells
        {
            get
            {
                if (IsEmpty) yield break;

                int x = minX;
                int z = minZ;

                // 底边
                for (; x <= maxX; x++)
                    yield return new Vector2Int(x, z);

                // 右边
                x--;
                for (z++; z <= maxZ; z++)
                    yield return new Vector2Int(x, z);

                // 顶边
                z--;
                for (x--; x >= minX; x--)
                    yield return new Vector2Int(x, z);

                // 左边
                x++;
                for (z--; z > minZ; z--)
                    yield return new Vector2Int(x, z);
            }
        }

        public readonly IEnumerable<Vector2Int> Corners
        {
            get
            {
                if (IsEmpty) yield break;

                yield return new Vector2Int(minX, minZ);
                if (Height > 1)
                {
                    yield return new Vector2Int(minX, maxZ);
                    if (Width > 1)
                        yield return new Vector2Int(maxX, maxZ);
                }
                if (Width > 1)
                    yield return new Vector2Int(maxX, minZ);
            }
        }

        public IEnumerable<Vector2Int> AdjacentCells
        {
            get
            {
                if (IsEmpty) yield break;

                // 上下边的邻接
                for (int x = minX; x <= maxX; x++)
                {
                    yield return new Vector2Int(x, minZ - 1);
                    yield return new Vector2Int(x, maxZ + 1);
                }

                // 左右边的邻接
                for (int z = minZ; z <= maxZ; z++)
                {
                    yield return new Vector2Int(minX - 1, z);
                    yield return new Vector2Int(maxX + 1, z);
                }

                // 四个角的对角邻接
                yield return new Vector2Int(minX - 1, minZ - 1);
                yield return new Vector2Int(maxX + 1, minZ - 1);
                yield return new Vector2Int(minX - 1, maxZ + 1);
                yield return new Vector2Int(maxX + 1, maxZ + 1);
            }
        }

        #endregion

        #region 包含与重叠检测

        public readonly bool Contains(Vector2Int c)
        {
            if (c.x >= minX && c.x <= maxX && c.y >= minZ)
                return c.y <= maxZ;
            return false;
        }

        public readonly bool Contains(int x, int z)
        {
            if (x >= minX && x <= maxX && z >= minZ)
                return z <= maxZ;
            return false;
        }

        public bool InBounds(Vector2Int mapSize)
        {
            if (minX >= 0 && minZ >= 0 && maxX < mapSize.x)
                return maxZ < mapSize.y;
            return false;
        }

        public bool FullyContainedWithin(FloorRect within)
        {
            FloorRect clipped = this;
            clipped.ClipInsideRect(within);
            return this == clipped;
        }

        public bool Overlaps(FloorRect other)
        {
            if (IsEmpty || other.IsEmpty)
                return false;
            
            if (minX <= other.maxX && maxX >= other.minX && maxZ >= other.minZ)
                return minZ <= other.maxZ;
            return false;
        }

        #endregion

        #region 边缘与角落判断

        public bool IsOnEdge(Vector2Int c)
        {
            if ((c.x != minX || c.y < minZ || c.y > maxZ) &&
                (c.x != maxX || c.y < minZ || c.y > maxZ) &&
                (c.y != minZ || c.x < minX || c.x > maxX))
            {
                if (c.y == maxZ && c.x >= minX)
                    return c.x <= maxX;
                return false;
            }
            return true;
        }

        public bool IsCorner(Vector2Int c)
        {
            if ((c.x != minX || c.y != minZ) &&
                (c.x != maxX || c.y != minZ) &&
                (c.x != minX || c.y != maxZ))
            {
                if (c.x == maxX)
                    return c.y == maxZ;
                return false;
            }
            return true;
        }

        #endregion

        #region 距离计算

        public float ClosestDistanceTo(Vector2Int c)
        {
            return Vector2Int.Distance(ClosestCellTo(c), c);
        }

        public Vector2Int ClosestCellTo(Vector2Int c)
        {
            if (Contains(c))
                return c;

            if (c.x < minX)
            {
                if (c.y < minZ)
                    return new Vector2Int(minX, minZ);
                if (c.y > maxZ)
                    return new Vector2Int(minX, maxZ);
                return new Vector2Int(minX, c.y);
            }

            if (c.x > maxX)
            {
                if (c.y < minZ)
                    return new Vector2Int(maxX, minZ);
                if (c.y > maxZ)
                    return new Vector2Int(maxX, maxZ);
                return new Vector2Int(maxX, c.y);
            }

            if (c.y < minZ)
                return new Vector2Int(c.x, minZ);
            
            return new Vector2Int(c.x, maxZ);
        }

        #endregion

        #region 几何变换

        public FloorRect ClipInsideRect(FloorRect otherRect)
        {
            if (minX < otherRect.minX)
                minX = otherRect.minX;
            if (maxX > otherRect.maxX)
                maxX = otherRect.maxX;
            if (minZ < otherRect.minZ)
                minZ = otherRect.minZ;
            if (maxZ > otherRect.maxZ)
                maxZ = otherRect.maxZ;
            return this;
        }

        public readonly FloorRect ExpandedBy(int dist)
        {
            FloorRect result = this;
            result.minX -= dist;
            result.minZ -= dist;
            result.maxX += dist;
            result.maxZ += dist;
            return result;
        }

        public readonly FloorRect ContractedBy(int dist)
        {
            return ExpandedBy(-dist);
        }

        public FloorRect MovedBy(int x, int z)
        {
            FloorRect result = this;
            result.minX += x;
            result.minZ += z;
            result.maxX += x;
            result.maxZ += z;
            return result;
        }

        public FloorRect MovedBy(Vector2Int offset)
        {
            return MovedBy(offset.x, offset.y);
        }

        #endregion

        #region 分割

        public (FloorRect bottom, FloorRect up) SplitVertical(int separation = 0)
        {
            Vector2Int center = CenterCell;
            FloorRect bottom = this;
            FloorRect up = this;
            bottom.maxZ = center.y - separation;
            up.minZ = center.y + separation;
            return (bottom, up);
        }

        public (FloorRect left, FloorRect right) SplitHorizontal(int separation = 0)
        {
            Vector2Int center = CenterCell;
            FloorRect left = this;
            FloorRect right = this;
            left.maxX = center.x - separation;
            right.minX = center.x + separation;
            return (left, right);
        }

        #endregion

        #region 随机查询

        public readonly bool TryFindRandomCell(out Vector2Int cell, Predicate<Vector2Int> validator = null)
        {
            List<Vector2Int> validCells = new List<Vector2Int>();
            
            foreach (Vector2Int c in Cells)
            {
                if (validator == null || validator(c))
                {
                    validCells.Add(c);
                }
            }

            if (validCells.Count > 0)
            {
                cell = validCells[UnityEngine.Random.Range(0, validCells.Count)];
                return true;
            }

            cell = Vector2Int.zero;
            return false;
        }

        #endregion

        #region 运算符重载

        public static bool operator ==(FloorRect lhs, FloorRect rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FloorRect lhs, FloorRect rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region IEnumerable实现

        public IEnumerator<Vector2Int> GetEnumerator()
        {
            return Cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region 标准方法

        public bool Equals(FloorRect other)
        {
            if (minX == other.minX && maxX == other.maxX && minZ == other.minZ)
                return maxZ == other.maxZ;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FloorRect other))
                return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(minX, maxX, minZ, maxZ);
        }

        public override string ToString()
        {
            return $"({minX},{minZ},{maxX},{maxZ})";
        }

        #endregion
    }
}