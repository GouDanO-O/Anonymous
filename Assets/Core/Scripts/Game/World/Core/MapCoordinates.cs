/**
 * MapCoordinates.cs
 * 地图坐标结构体定义
 * 
 * 包含三种坐标类型：
 * - TileCoord: Tile 坐标（逻辑坐标）
 * - ChunkCoord: Chunk 坐标
 * - LocalTileCoord: Chunk 内的局部 Tile 坐标
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// Tile 坐标（地图逻辑坐标）
    /// 这是游戏逻辑中最常用的坐标类型
    /// </summary>
    [Serializable]
    public struct TileCoord : IEquatable<TileCoord>
    {
        public int x;
        public int y;
        
        public TileCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        #region 常用静态值
        
        public static readonly TileCoord Zero = new TileCoord(0, 0);
        public static readonly TileCoord One = new TileCoord(1, 1);
        public static readonly TileCoord Invalid = new TileCoord(-1, -1);
        
        // 四方向偏移
        public static readonly TileCoord Up = new TileCoord(0, 1);
        public static readonly TileCoord Down = new TileCoord(0, -1);
        public static readonly TileCoord Left = new TileCoord(-1, 0);
        public static readonly TileCoord Right = new TileCoord(1, 0);
        
        #endregion
        
        #region 坐标转换
        
        /// <summary>
        /// 转换为 Chunk 坐标
        /// </summary>
        public ChunkCoord ToChunkCoord()
        {
            // 使用位运算加速：右移4位等于除以16
            // 注意：对于负数坐标需要特殊处理
            int chunkX = x >= 0 
                ? x >> MapConstants.CHUNK_SIZE_SHIFT 
                : ((x + 1) >> MapConstants.CHUNK_SIZE_SHIFT) - 1;
            int chunkY = y >= 0 
                ? y >> MapConstants.CHUNK_SIZE_SHIFT 
                : ((y + 1) >> MapConstants.CHUNK_SIZE_SHIFT) - 1;
            
            return new ChunkCoord(chunkX, chunkY);
        }
        
        /// <summary>
        /// 获取在 Chunk 内的局部坐标
        /// </summary>
        public LocalTileCoord ToLocalCoord()
        {
            // 使用位运算：与 15(0b1111) 做与运算等于取模16
            // 对于负数需要特殊处理，确保结果为正
            int localX = x >= 0 
                ? x & MapConstants.CHUNK_LOCAL_MASK 
                : MapConstants.CHUNK_SIZE - 1 - ((-x - 1) & MapConstants.CHUNK_LOCAL_MASK);
            int localY = y >= 0 
                ? y & MapConstants.CHUNK_LOCAL_MASK 
                : MapConstants.CHUNK_SIZE - 1 - ((-y - 1) & MapConstants.CHUNK_LOCAL_MASK);
            
            return new LocalTileCoord(localX, localY);
        }
        
        /// <summary>
        /// 转换为 Unity 世界坐标
        /// </summary>
        public Vector2 ToWorldPosition()
        {
            return new Vector2(
                x * MapConstants.TILE_SIZE,
                y * MapConstants.TILE_SIZE
            );
        }
        
        /// <summary>
        /// 从 Unity 世界坐标转换
        /// </summary>
        public static TileCoord FromWorldPosition(Vector2 worldPos)
        {
            return new TileCoord(
                Mathf.FloorToInt(worldPos.x / MapConstants.TILE_SIZE),
                Mathf.FloorToInt(worldPos.y / MapConstants.TILE_SIZE)
            );
        }
        
        /// <summary>
        /// 从 Unity 世界坐标转换（Vector3版本，忽略z）
        /// </summary>
        public static TileCoord FromWorldPosition(Vector3 worldPos)
        {
            return FromWorldPosition(new Vector2(worldPos.x, worldPos.y));
        }
        
        #endregion
        
        #region 运算符重载
        
        public static TileCoord operator +(TileCoord a, TileCoord b)
        {
            return new TileCoord(a.x + b.x, a.y + b.y);
        }
        
        public static TileCoord operator -(TileCoord a, TileCoord b)
        {
            return new TileCoord(a.x - b.x, a.y - b.y);
        }
        
        public static TileCoord operator *(TileCoord a, int scalar)
        {
            return new TileCoord(a.x * scalar, a.y * scalar);
        }
        
        public static bool operator ==(TileCoord a, TileCoord b)
        {
            return a.x == b.x && a.y == b.y;
        }
        
        public static bool operator !=(TileCoord a, TileCoord b)
        {
            return !(a == b);
        }
        
        #endregion
        
        #region 实用方法
        
        /// <summary>
        /// 计算到另一个坐标的曼哈顿距离
        /// </summary>
        public int ManhattanDistance(TileCoord other)
        {
            return Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);
        }
        
        /// <summary>
        /// 计算到另一个坐标的欧几里得距离（平方）
        /// 避免开方运算，用于距离比较
        /// </summary>
        public int SqrDistance(TileCoord other)
        {
            int dx = x - other.x;
            int dy = y - other.y;
            return dx * dx + dy * dy;
        }
        
        /// <summary>
        /// 获取相邻的四个坐标
        /// </summary>
        public TileCoord[] GetNeighbors4()
        {
            return new TileCoord[]
            {
                this + Up,
                this + Down,
                this + Left,
                this + Right
            };
        }
        
        /// <summary>
        /// 获取相邻的八个坐标（包括对角线）
        /// </summary>
        public TileCoord[] GetNeighbors8()
        {
            return new TileCoord[]
            {
                this + Up,
                this + Down,
                this + Left,
                this + Right,
                this + Up + Left,
                this + Up + Right,
                this + Down + Left,
                this + Down + Right
            };
        }
        
        #endregion
        
        #region IEquatable 实现
        
        public bool Equals(TileCoord other)
        {
            return x == other.x && y == other.y;
        }
        
        public override bool Equals(object obj)
        {
            return obj is TileCoord other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            // 使用位移组合来生成哈希码
            return (x * 397) ^ y;
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Tile({x}, {y})";
        }
    }
    
    /// <summary>
    /// Chunk 坐标
    /// </summary>
    [Serializable]
    public struct ChunkCoord : IEquatable<ChunkCoord>
    {
        public int x;
        public int y;
        
        public ChunkCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        #region 常用静态值
        
        public static readonly ChunkCoord Zero = new ChunkCoord(0, 0);
        public static readonly ChunkCoord Invalid = new ChunkCoord(-1, -1);
        
        #endregion
        
        #region 坐标转换
        
        /// <summary>
        /// 获取该 Chunk 左下角的 Tile 坐标
        /// </summary>
        public TileCoord ToTileCoord()
        {
            return new TileCoord(
                x << MapConstants.CHUNK_SIZE_SHIFT,  // x * 16
                y << MapConstants.CHUNK_SIZE_SHIFT   // y * 16
            );
        }
        
        /// <summary>
        /// 获取 Chunk 中心的 Tile 坐标
        /// </summary>
        public TileCoord GetCenterTileCoord()
        {
            int baseX = x << MapConstants.CHUNK_SIZE_SHIFT;
            int baseY = y << MapConstants.CHUNK_SIZE_SHIFT;
            return new TileCoord(
                baseX + MapConstants.CHUNK_SIZE / 2,
                baseY + MapConstants.CHUNK_SIZE / 2
            );
        }
        
        /// <summary>
        /// 转换为一维数组索引
        /// 用于在 Map 的 Chunk 数组中定位
        /// </summary>
        public int ToIndex(int mapWidthInChunks)
        {
            return y * mapWidthInChunks + x;
        }
        
        /// <summary>
        /// 从一维索引还原坐标
        /// </summary>
        public static ChunkCoord FromIndex(int index, int mapWidthInChunks)
        {
            return new ChunkCoord(
                index % mapWidthInChunks,
                index / mapWidthInChunks
            );
        }
        
        #endregion
        
        #region 运算符重载
        
        public static ChunkCoord operator +(ChunkCoord a, ChunkCoord b)
        {
            return new ChunkCoord(a.x + b.x, a.y + b.y);
        }
        
        public static ChunkCoord operator -(ChunkCoord a, ChunkCoord b)
        {
            return new ChunkCoord(a.x - b.x, a.y - b.y);
        }
        
        public static bool operator ==(ChunkCoord a, ChunkCoord b)
        {
            return a.x == b.x && a.y == b.y;
        }
        
        public static bool operator !=(ChunkCoord a, ChunkCoord b)
        {
            return !(a == b);
        }
        
        #endregion
        
        #region IEquatable 实现
        
        public bool Equals(ChunkCoord other)
        {
            return x == other.x && y == other.y;
        }
        
        public override bool Equals(object obj)
        {
            return obj is ChunkCoord other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (x * 397) ^ y;
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Chunk({x}, {y})";
        }
    }
    
    /// <summary>
    /// Chunk 内的局部 Tile 坐标
    /// 范围：0-15
    /// </summary>
    [Serializable]
    public struct LocalTileCoord : IEquatable<LocalTileCoord>
    {
        public int x;
        public int y;
        
        public LocalTileCoord(int x, int y)
        {
            // 确保值在有效范围内
            this.x = x & MapConstants.CHUNK_LOCAL_MASK;
            this.y = y & MapConstants.CHUNK_LOCAL_MASK;
        }
        
        #region 索引转换
        
        /// <summary>
        /// 转换为一维数组索引
        /// 用于在 Chunk 的 Tile 数组中定位
        /// </summary>
        public int ToIndex()
        {
            return y * MapConstants.CHUNK_SIZE + x;
        }
        
        /// <summary>
        /// 从一维索引还原坐标
        /// </summary>
        public static LocalTileCoord FromIndex(int index)
        {
            return new LocalTileCoord(
                index & MapConstants.CHUNK_LOCAL_MASK,       // index % 16
                index >> MapConstants.CHUNK_SIZE_SHIFT       // index / 16
            );
        }
        
        #endregion
        
        #region 验证
        
        /// <summary>
        /// 检查坐标是否在有效范围内
        /// </summary>
        public bool IsValid()
        {
            return x >= 0 && x < MapConstants.CHUNK_SIZE 
                && y >= 0 && y < MapConstants.CHUNK_SIZE;
        }
        
        #endregion
        
        #region IEquatable 实现
        
        public bool Equals(LocalTileCoord other)
        {
            return x == other.x && y == other.y;
        }
        
        public override bool Equals(object obj)
        {
            return obj is LocalTileCoord other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (x * 397) ^ y;
        }
        
        public static bool operator ==(LocalTileCoord a, LocalTileCoord b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(LocalTileCoord a, LocalTileCoord b)
        {
            return !a.Equals(b);
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Local({x}, {y})";
        }
    }
}
