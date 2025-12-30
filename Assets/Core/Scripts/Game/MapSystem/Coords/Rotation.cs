/*******************************************************************************
 * 文件名:    Rotation.cs
 * 描述:      旋转结构体，用于表示建筑物和实体的朝向（0/90/180/270度）
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Rotation 表示实体的旋转状态，只支持四个方向（0°, 90°, 180°, 270°）。
 *   主要用于建筑物放置和渲染时的朝向控制。
 *   
 * 旋转规则:
 *   - 旋转值为0-3，表示顺时针旋转的90度次数
 *   - Rot0 (0°)  = 朝北
 *   - Rot1 (90°) = 朝东
 *   - Rot2 (180°) = 朝南
 *   - Rot3 (270°) = 朝西
 ******************************************************************************/

using System;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 旋转结构体（0/90/180/270度）
    /// 用于表示建筑物和实体的朝向
    /// </summary>
    [Serializable]
    public struct Rotation : IEquatable<Rotation>
    {
        #region 字段

        /// <summary>
        /// 旋转值（0-3），表示顺时针旋转90度的次数
        /// </summary>
        [SerializeField]
        private byte _value;

        #endregion

        #region 静态常量

        /// <summary>
        /// 朝北（0度）
        /// </summary>
        public static readonly Rotation North = new Rotation(0);

        /// <summary>
        /// 朝东（90度）
        /// </summary>
        public static readonly Rotation East = new Rotation(1);

        /// <summary>
        /// 朝南（180度）
        /// </summary>
        public static readonly Rotation South = new Rotation(2);

        /// <summary>
        /// 朝西（270度）
        /// </summary>
        public static readonly Rotation West = new Rotation(3);

        /// <summary>
        /// 所有旋转值
        /// </summary>
        public static readonly Rotation[] All = new Rotation[]
        {
            North, East, South, West
        };

        /// <summary>
        /// 无效旋转
        /// </summary>
        public static readonly Rotation Invalid = new Rotation(255);

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">旋转值（0-3）</param>
        public Rotation(int value)
        {
            _value = (byte)(value & 3); // 只保留0-3
        }

        /// <summary>
        /// 私有构造函数（用于Invalid）
        /// </summary>
        private Rotation(byte rawValue)
        {
            _value = rawValue;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 旋转值（0-3）
        /// </summary>
        public int Value => _value & 3;

        /// <summary>
        /// 旋转角度（0, 90, 180, 270）
        /// </summary>
        public float Angle => (_value & 3) * 90f;

        /// <summary>
        /// 弧度值
        /// </summary>
        public float Radians => (_value & 3) * Mathf.PI * 0.5f;

        /// <summary>
        /// 是否是有效旋转
        /// </summary>
        public bool IsValid => _value <= 3;

        /// <summary>
        /// 是否是水平朝向（东或西）
        /// </summary>
        public bool IsHorizontal => (_value & 3) == 1 || (_value & 3) == 3;

        /// <summary>
        /// 是否是垂直朝向（北或南）
        /// </summary>
        public bool IsVertical => (_value & 3) == 0 || (_value & 3) == 2;

        /// <summary>
        /// 对应的方向
        /// </summary>
        public Direction FacingDirection => (Direction)(_value & 3);

        #endregion

        #region 旋转操作

        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        public Rotation RotateCW()
        {
            return new Rotation((_value + 1) & 3);
        }

        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        public Rotation RotateCCW()
        {
            return new Rotation((_value + 3) & 3);
        }

        /// <summary>
        /// 旋转180度
        /// </summary>
        public Rotation Rotate180()
        {
            return new Rotation((_value + 2) & 3);
        }

        /// <summary>
        /// 获取相反朝向
        /// </summary>
        public Rotation Opposite => Rotate180();

        /// <summary>
        /// 旋转指定次数（每次90度）
        /// 正数顺时针，负数逆时针
        /// </summary>
        public Rotation Rotate(int times)
        {
            int result = (_value + times) % 4;
            if (result < 0) result += 4;
            return new Rotation(result);
        }

        #endregion

        #region 坐标变换

        /// <summary>
        /// 旋转CellCoord坐标
        /// 将坐标绕原点按当前旋转值旋转
        /// </summary>
        /// <param name="coord">原始坐标</param>
        /// <returns>旋转后的坐标</returns>
        public CellCoord RotateCoord(CellCoord coord)
        {
            return (_value & 3) switch
            {
                0 => coord,                                    // 0°:   (x, z)
                1 => new CellCoord(coord.z, -coord.x),         // 90°:  (z, -x)
                2 => new CellCoord(-coord.x, -coord.z),        // 180°: (-x, -z)
                3 => new CellCoord(-coord.z, coord.x),         // 270°: (-z, x)
                _ => coord
            };
        }

        /// <summary>
        /// 逆向旋转CellCoord坐标
        /// </summary>
        public CellCoord UnrotateCoord(CellCoord coord)
        {
            return Opposite.RotateCoord(coord);
        }

        /// <summary>
        /// 旋转尺寸
        /// 90度和270度时交换宽高
        /// </summary>
        /// <param name="size">原始尺寸</param>
        /// <returns>旋转后的尺寸</returns>
        public Vector2Int RotateSize(Vector2Int size)
        {
            return IsHorizontal ? new Vector2Int(size.y, size.x) : size;
        }

        /// <summary>
        /// 获取方向偏移量
        /// 相当于向"前方"移动一格的偏移
        /// </summary>
        public CellCoord FacingOffset => (_value & 3) switch
        {
            0 => new CellCoord(0, 1),   // 北
            1 => new CellCoord(1, 0),   // 东
            2 => new CellCoord(0, -1),  // 南
            3 => new CellCoord(-1, 0),  // 西
            _ => CellCoord.Zero
        };

        /// <summary>
        /// 获取右侧偏移量
        /// </summary>
        public CellCoord RightOffset => RotateCW().FacingOffset;

        /// <summary>
        /// 获取左侧偏移量
        /// </summary>
        public CellCoord LeftOffset => RotateCCW().FacingOffset;

        /// <summary>
        /// 获取后方偏移量
        /// </summary>
        public CellCoord BackOffset => Opposite.FacingOffset;

        #endregion

        #region Unity转换

        /// <summary>
        /// 转换为Quaternion（绕Y轴旋转）
        /// </summary>
        public Quaternion ToQuaternion()
        {
            return Quaternion.Euler(0, Angle, 0);
        }

        /// <summary>
        /// 转换为2D旋转的Quaternion（绕Z轴旋转）
        /// </summary>
        public Quaternion ToQuaternion2D()
        {
            return Quaternion.Euler(0, 0, -Angle); // 2D中顺时针需要负角度
        }

        /// <summary>
        /// 获取朝向的单位向量（3D，XZ平面）
        /// </summary>
        public Vector3 ToVector3()
        {
            return (_value & 3) switch
            {
                0 => Vector3.forward,  // 北 (0, 0, 1)
                1 => Vector3.right,    // 东 (1, 0, 0)
                2 => Vector3.back,     // 南 (0, 0, -1)
                3 => Vector3.left,     // 西 (-1, 0, 0)
                _ => Vector3.forward
            };
        }

        /// <summary>
        /// 获取朝向的单位向量（2D）
        /// </summary>
        public Vector2 ToVector2()
        {
            return (_value & 3) switch
            {
                0 => Vector2.up,       // 北
                1 => Vector2.right,    // 东
                2 => Vector2.down,     // 南
                3 => Vector2.left,     // 西
                _ => Vector2.up
            };
        }

        /// <summary>
        /// 从角度创建Rotation
        /// </summary>
        /// <param name="angle">角度（会四舍五入到最近的90度）</param>
        public static Rotation FromAngle(float angle)
        {
            angle = angle % 360;
            if (angle < 0) angle += 360;
            int value = Mathf.RoundToInt(angle / 90f) % 4;
            return new Rotation(value);
        }

        /// <summary>
        /// 从方向创建Rotation
        /// </summary>
        public static Rotation FromDirection(Direction dir)
        {
            return new Rotation((int)dir);
        }

        /// <summary>
        /// 从向量创建Rotation
        /// </summary>
        public static Rotation FromVector(Vector2 vector)
        {
            if (vector.sqrMagnitude < 0.001f)
                return North;

            float angle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
            return FromAngle(angle);
        }

        /// <summary>
        /// 从向量创建Rotation（3D，使用XZ平面）
        /// </summary>
        public static Rotation FromVector(Vector3 vector)
        {
            return FromVector(new Vector2(vector.x, vector.z));
        }

        #endregion

        #region 运算符重载

        public static Rotation operator +(Rotation a, Rotation b)
        {
            return new Rotation((a._value + b._value) & 3);
        }

        public static Rotation operator -(Rotation a, Rotation b)
        {
            return new Rotation((a._value - b._value + 4) & 3);
        }

        public static Rotation operator +(Rotation rot, int times)
        {
            return rot.Rotate(times);
        }

        public static Rotation operator -(Rotation rot, int times)
        {
            return rot.Rotate(-times);
        }

        public static Rotation operator ++(Rotation rot)
        {
            return rot.RotateCW();
        }

        public static Rotation operator --(Rotation rot)
        {
            return rot.RotateCCW();
        }

        public static bool operator ==(Rotation a, Rotation b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(Rotation a, Rotation b)
        {
            return a._value != b._value;
        }

        #endregion

        #region 隐式转换

        public static implicit operator int(Rotation rot)
        {
            return rot.Value;
        }

        public static implicit operator Rotation(int value)
        {
            return new Rotation(value);
        }

        public static implicit operator Direction(Rotation rot)
        {
            return rot.FacingDirection;
        }

        public static implicit operator Rotation(Direction dir)
        {
            return new Rotation((int)dir);
        }

        #endregion

        #region IEquatable 实现

        public bool Equals(Rotation other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is Rotation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            if (!IsValid)
                return "Invalid";

            return (_value & 3) switch
            {
                0 => "North (0°)",
                1 => "East (90°)",
                2 => "South (180°)",
                3 => "West (270°)",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 简短格式
        /// </summary>
        public string ToShortString()
        {
            return (_value & 3) switch
            {
                0 => "N",
                1 => "E",
                2 => "S",
                3 => "W",
                _ => "?"
            };
        }

        #endregion

        #region 随机

        /// <summary>
        /// 获取随机旋转
        /// </summary>
        public static Rotation Random()
        {
            return new Rotation(UnityEngine.Random.Range(0, 4));
        }

        /// <summary>
        /// 获取随机旋转（使用指定随机数生成器）
        /// </summary>
        public static Rotation Random(System.Random random)
        {
            return new Rotation(random.Next(0, 4));
        }

        #endregion
    }
}
