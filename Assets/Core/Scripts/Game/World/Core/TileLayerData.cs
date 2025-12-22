/**
 * TileLayerData.cs
 * 单层瓦片数据结构
 * 
 * 混合系统中，TileLayerData 只用于存储静态内容：
 * - 地形、地板、墙壁、屋顶等
 * - 不包含家具、容器等动态对象（那些由 Entity 系统管理）
 * 
 * 设计为 4 字节，更加紧凑
 */

using System;
using System.Runtime.InteropServices;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 单层瓦片数据
    /// 结构大小：4 字节
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TileLayerData : IEquatable<TileLayerData>
    {
        #region 字段定义（共 4 字节）
        
        /// <summary>
        /// 瓦片类型 ID，对应配置表中的 TileId
        /// 0 表示空（无瓦片）
        /// </summary>
        public ushort tileId;           // 2 字节
        
        /// <summary>
        /// 精灵变体索引 + 状态标志
        /// 高 4 位：spriteVariant (0-15)
        /// 低 4 位：flags
        /// </summary>
        public byte spriteAndFlags;     // 1 字节
        
        /// <summary>
        /// 耐久度/损坏程度 (0-255)
        /// 0 = 完好，255 = 完全损坏
        /// </summary>
        public byte damage;             // 1 字节
        
        #endregion
        
        #region 常量
        
        private const byte SPRITE_MASK = 0xF0;      // 高4位
        private const byte FLAGS_MASK = 0x0F;       // 低4位
        private const int SPRITE_SHIFT = 4;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 创建瓦片层数据
        /// </summary>
        public TileLayerData(ushort tileId, byte spriteVariant = 0, TileFlags flags = TileFlags.None)
        {
            this.tileId = tileId;
            this.spriteAndFlags = (byte)((spriteVariant << SPRITE_SHIFT) | ((byte)flags & FLAGS_MASK));
            this.damage = 0;
        }
        
        #endregion
        
        #region 静态工厂方法
        
        /// <summary>
        /// 空瓦片数据（默认值）
        /// </summary>
        public static readonly TileLayerData Empty = new TileLayerData(MapConstants.EMPTY_TILE_ID);
        
        /// <summary>
        /// 快速创建简单瓦片
        /// </summary>
        public static TileLayerData Create(ushort tileId)
        {
            return new TileLayerData(tileId);
        }
        
        /// <summary>
        /// 创建带标志的瓦片（如阻挡的墙壁）
        /// </summary>
        public static TileLayerData CreateBlocking(ushort tileId)
        {
            return new TileLayerData(tileId, 0, TileFlags.Blocking | TileFlags.BlockSight);
        }
        
        /// <summary>
        /// 创建带随机变体的瓦片
        /// </summary>
        public static TileLayerData CreateWithVariant(ushort tileId, byte variant)
        {
            return new TileLayerData(tileId, variant);
        }
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否为空瓦片
        /// </summary>
        public bool IsEmpty => tileId == MapConstants.EMPTY_TILE_ID;
        
        /// <summary>
        /// 精灵变体索引 (0-15)
        /// </summary>
        public byte SpriteVariant
        {
            get => (byte)((spriteAndFlags & SPRITE_MASK) >> SPRITE_SHIFT);
            set => spriteAndFlags = (byte)((value << SPRITE_SHIFT) | (spriteAndFlags & FLAGS_MASK));
        }
        
        /// <summary>
        /// 标志位（只有低4位有效）
        /// </summary>
        public TileFlags Flags
        {
            get => (TileFlags)(spriteAndFlags & FLAGS_MASK);
            set => spriteAndFlags = (byte)((spriteAndFlags & SPRITE_MASK) | ((byte)value & FLAGS_MASK));
        }
        
        /// <summary>
        /// 是否阻挡移动
        /// </summary>
        public bool IsBlocking => (Flags & TileFlags.Blocking) != 0;
        
        /// <summary>
        /// 是否阻挡视线
        /// </summary>
        public bool BlocksSight => (Flags & TileFlags.BlockSight) != 0;
        
        /// <summary>
        /// 是否已损坏
        /// </summary>
        public bool IsDamaged => (Flags & TileFlags.Damaged) != 0 || damage > 0;
        
        /// <summary>
        /// 是否可燃
        /// </summary>
        public bool IsFlammable => (Flags & TileFlags.Flammable) != 0;
        
        /// <summary>
        /// 是否是液体
        /// </summary>
        public bool IsLiquid => (Flags & TileFlags.IsLiquid) != 0;
        
        /// <summary>
        /// 损坏百分比 (0-1)
        /// </summary>
        public float DamagePercent => damage / 255f;
        
        #endregion
        
        #region 标志位操作
        
        /// <summary>
        /// 设置标志位
        /// </summary>
        public void SetFlag(TileFlags flag, bool value)
        {
            if (value)
            {
                Flags |= flag;
            }
            else
            {
                Flags &= ~flag;
            }
        }
        
        /// <summary>
        /// 检查是否有指定标志
        /// </summary>
        public bool HasFlag(TileFlags flag)
        {
            return (Flags & flag) == flag;
        }
        
        #endregion
        
        #region 修改方法
        
        /// <summary>
        /// 添加损坏值
        /// </summary>
        public void AddDamage(byte amount)
        {
            int newDamage = damage + amount;
            damage = (byte)Math.Min(newDamage, 255);
            if (damage > 0)
            {
                SetFlag(TileFlags.Damaged, true);
            }
        }
        
        /// <summary>
        /// 修复损坏
        /// </summary>
        public void Repair(byte amount)
        {
            int newDamage = damage - amount;
            damage = (byte)Math.Max(newDamage, 0);
            if (damage == 0)
            {
                SetFlag(TileFlags.Damaged, false);
            }
        }
        
        /// <summary>
        /// 完全修复
        /// </summary>
        public void FullRepair()
        {
            damage = 0;
            SetFlag(TileFlags.Damaged, false);
        }
        
        /// <summary>
        /// 清空此层
        /// </summary>
        public void Clear()
        {
            this = Empty;
        }
        
        #endregion
        
        #region IEquatable 实现
        
        public bool Equals(TileLayerData other)
        {
            return tileId == other.tileId 
                && spriteAndFlags == other.spriteAndFlags 
                && damage == other.damage;
        }
        
        public override bool Equals(object obj)
        {
            return obj is TileLayerData other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = tileId.GetHashCode();
                hash = (hash * 397) ^ spriteAndFlags.GetHashCode();
                hash = (hash * 397) ^ damage.GetHashCode();
                return hash;
            }
        }
        
        public static bool operator ==(TileLayerData a, TileLayerData b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(TileLayerData a, TileLayerData b)
        {
            return !a.Equals(b);
        }
        
        #endregion
        
        public override string ToString()
        {
            if (IsEmpty)
            {
                return "TileLayer(Empty)";
            }
            return $"TileLayer(Id:{tileId}, Var:{SpriteVariant}, Dmg:{damage})";
        }
    }
}
