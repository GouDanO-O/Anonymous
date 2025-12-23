/**
 * TileData.cs
 * 完整瓦片数据结构（6层）
 * 
 * 混合系统中，TileData 只存储静态建筑结构：
 *   Layer 0: Ground    - 地形
 *   Layer 1: Floor     - 地板  
 *   Layer 2: FloorDecor- 地面装饰
 *   Layer 3: Wall      - 墙壁
 *   Layer 4: WallDecor - 墙壁装饰/门窗框
 *   Layer 5: Roof      - 屋顶
 * 
 * 家具、容器、门等动态对象由 Entity 系统管理
 * 
 * 结构大小：24 字节（6层 × 4字节）
 */

using System;
using System.Runtime.InteropServices;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 完整瓦片数据（6层静态内容）
    /// 结构大小：24 字节
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileData : IEquatable<TileData>
    {
        #region 层级数据
        
        /// <summary>Layer 0: 地形层（草地、泥土、水等）</summary>
        public TileLayerData ground;
        
        /// <summary>Layer 1: 地板层（木板、瓷砖等）</summary>
        public TileLayerData floor;
        
        /// <summary>Layer 2: 地面装饰层（血迹、裂缝等）</summary>
        public TileLayerData floorDecor;
        
        /// <summary>Layer 3: 墙壁层</summary>
        public TileLayerData wall;
        
        /// <summary>Layer 4: 墙壁装饰层（窗框、门框等）</summary>
        public TileLayerData wallDecor;
        
        /// <summary>Layer 5: 屋顶层</summary>
        public TileLayerData roof;
        
        #endregion
        
        #region 静态值
        
        /// <summary>
        /// 空瓦片数据（所有层都为空）
        /// </summary>
        public static readonly TileData Empty = new TileData();
        
        #endregion
        
        #region 工厂方法
        
        /// <summary>
        /// 创建只有地形层的瓦片
        /// </summary>
        public static TileData CreateGround(ushort groundTileId)
        {
            var data = new TileData();
            data.ground = TileLayerData.Create(groundTileId);
            return data;
        }
        
        /// <summary>
        /// 创建有地形和地板的瓦片（室内地面）
        /// </summary>
        public static TileData CreateFloor(ushort groundTileId, ushort floorTileId)
        {
            var data = new TileData();
            data.ground = TileLayerData.Create(groundTileId);
            data.floor = TileLayerData.Create(floorTileId);
            return data;
        }
        
        /// <summary>
        /// 创建墙壁瓦片
        /// </summary>
        public static TileData CreateWall(ushort groundTileId, ushort wallTileId)
        {
            var data = new TileData();
            data.ground = TileLayerData.Create(groundTileId);
            data.wall = TileLayerData.CreateBlocking(wallTileId);
            return data;
        }
        
        /// <summary>
        /// 创建完整的室内瓦片（地形+地板+墙+屋顶）
        /// </summary>
        public static TileData CreateIndoor(ushort groundId, ushort floorId, 
            ushort wallId, ushort roofId)
        {
            var data = new TileData();
            data.ground = TileLayerData.Create(groundId);
            data.floor = TileLayerData.Create(floorId);
            data.wall = TileLayerData.CreateBlocking(wallId);
            data.roof = TileLayerData.Create(roofId);
            return data;
        }
        
        #endregion
        
        #region 层级访问（通过索引）
        
        /// <summary>
        /// 通过层级索引获取层数据
        /// </summary>
        public TileLayerData GetLayer(int layerIndex)
        {
            switch (layerIndex)
            {
                case MapConstants.LAYER_GROUND:      return ground;
                case MapConstants.LAYER_FLOOR:       return floor;
                case MapConstants.LAYER_FLOOR_DECOR: return floorDecor;
                case MapConstants.LAYER_WALL:        return wall;
                case MapConstants.LAYER_WALL_DECOR:  return wallDecor;
                case MapConstants.LAYER_ROOF:        return roof;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layerIndex), 
                        $"Layer index must be 0-{MapConstants.TILE_LAYER_COUNT - 1}");
            }
        }
        
        /// <summary>
        /// 通过层级索引设置层数据
        /// </summary>
        public void SetLayer(int layerIndex, TileLayerData layerData)
        {
            switch (layerIndex)
            {
                case MapConstants.LAYER_GROUND:      ground = layerData; break;
                case MapConstants.LAYER_FLOOR:       floor = layerData; break;
                case MapConstants.LAYER_FLOOR_DECOR: floorDecor = layerData; break;
                case MapConstants.LAYER_WALL:        wall = layerData; break;
                case MapConstants.LAYER_WALL_DECOR:  wallDecor = layerData; break;
                case MapConstants.LAYER_ROOF:        roof = layerData; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layerIndex), 
                        $"Layer index must be 0-{MapConstants.TILE_LAYER_COUNT - 1}");
            }
        }
        
        /// <summary>
        /// 返回设置指定层后的新 TileData（不可变方式）
        /// </summary>
        public TileData WithLayer(int layerIndex, TileLayerData layerData)
        {
            TileData result = this;
            result.SetLayer(layerIndex, layerData);
            return result;
        }
        
        /// <summary>
        /// 索引器访问
        /// </summary>
        public TileLayerData this[int layerIndex]
        {
            get => GetLayer(layerIndex);
            set => SetLayer(layerIndex, value);
        }
        
        #endregion
        
        #region 属性查询
        
        /// <summary>
        /// 是否完全为空（所有层都没有瓦片）
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return ground.IsEmpty 
                    && floor.IsEmpty 
                    && floorDecor.IsEmpty 
                    && wall.IsEmpty 
                    && wallDecor.IsEmpty 
                    && roof.IsEmpty;
            }
        }
        
        /// <summary>
        /// 地形是否阻挡移动（墙壁或其他阻挡物）
        /// 注意：这只检查静态 Tile，Entity 的阻挡需要另外检查
        /// </summary>
        public bool IsBlocking => wall.IsBlocking;
        
        /// <summary>
        /// 是否阻挡视线
        /// </summary>
        public bool BlocksSight => wall.BlocksSight;
        
        /// <summary>
        /// 是否有墙壁
        /// </summary>
        public bool HasWall => !wall.IsEmpty;
        
        /// <summary>
        /// 是否有屋顶
        /// </summary>
        public bool HasRoof => !roof.IsEmpty;
        
        /// <summary>
        /// 是否是室内（有屋顶）
        /// </summary>
        public bool IsIndoor => HasRoof;
        
        /// <summary>
        /// 是否有地板（人造地面）
        /// </summary>
        public bool HasFloor => !floor.IsEmpty;
        
        /// <summary>
        /// 地形是否是水/液体
        /// </summary>
        public bool IsWater => ground.IsLiquid;
        
        /// <summary>
        /// 获取非空层的数量
        /// </summary>
        public int NonEmptyLayerCount
        {
            get
            {
                int count = 0;
                if (!ground.IsEmpty) count++;
                if (!floor.IsEmpty) count++;
                if (!floorDecor.IsEmpty) count++;
                if (!wall.IsEmpty) count++;
                if (!wallDecor.IsEmpty) count++;
                if (!roof.IsEmpty) count++;
                return count;
            }
        }
        
        #endregion
        
        #region 层级操作
        
        /// <summary>
        /// 清空指定层
        /// </summary>
        public void ClearLayer(int layerIndex)
        {
            SetLayer(layerIndex, TileLayerData.Empty);
        }
        
        /// <summary>
        /// 清空所有层
        /// </summary>
        public void Clear()
        {
            ground = TileLayerData.Empty;
            floor = TileLayerData.Empty;
            floorDecor = TileLayerData.Empty;
            wall = TileLayerData.Empty;
            wallDecor = TileLayerData.Empty;
            roof = TileLayerData.Empty;
        }
        
        /// <summary>
        /// 合并另一个 TileData（非空层覆盖）
        /// </summary>
        public void MergeFrom(TileData other)
        {
            if (!other.ground.IsEmpty) ground = other.ground;
            if (!other.floor.IsEmpty) floor = other.floor;
            if (!other.floorDecor.IsEmpty) floorDecor = other.floorDecor;
            if (!other.wall.IsEmpty) wall = other.wall;
            if (!other.wallDecor.IsEmpty) wallDecor = other.wallDecor;
            if (!other.roof.IsEmpty) roof = other.roof;
        }
        
        #endregion
        
        #region Builder 模式（链式调用）
        
        /// <summary>
        /// 设置地形层
        /// </summary>
        public TileData WithGround(ushort tileId)
        {
            var copy = this;
            copy.ground = TileLayerData.Create(tileId);
            return copy;
        }
        
        /// <summary>
        /// 设置地板层
        /// </summary>
        public TileData WithFloor(ushort tileId)
        {
            var copy = this;
            copy.floor = TileLayerData.Create(tileId);
            return copy;
        }
        
        /// <summary>
        /// 设置地面装饰
        /// </summary>
        public TileData WithFloorDecor(ushort tileId)
        {
            var copy = this;
            copy.floorDecor = TileLayerData.Create(tileId);
            return copy;
        }
        
        /// <summary>
        /// 设置墙壁（自动添加阻挡标志）
        /// </summary>
        public TileData WithWall(ushort tileId)
        {
            var copy = this;
            copy.wall = TileLayerData.CreateBlocking(tileId);
            return copy;
        }
        
        /// <summary>
        /// 设置墙壁装饰（如门框）
        /// </summary>
        public TileData WithWallDecor(ushort tileId)
        {
            var copy = this;
            copy.wallDecor = TileLayerData.Create(tileId);
            return copy;
        }
        
        /// <summary>
        /// 设置屋顶
        /// </summary>
        public TileData WithRoof(ushort tileId)
        {
            var copy = this;
            copy.roof = TileLayerData.Create(tileId);
            return copy;
        }
        
        #endregion
        
        #region 遍历
        
        /// <summary>
        /// 遍历所有层（包括空层）
        /// </summary>
        public void ForEachLayer(Action<int, TileLayerData> action)
        {
            action(MapConstants.LAYER_GROUND, ground);
            action(MapConstants.LAYER_FLOOR, floor);
            action(MapConstants.LAYER_FLOOR_DECOR, floorDecor);
            action(MapConstants.LAYER_WALL, wall);
            action(MapConstants.LAYER_WALL_DECOR, wallDecor);
            action(MapConstants.LAYER_ROOF, roof);
        }
        
        /// <summary>
        /// 遍历非空层
        /// </summary>
        public void ForEachNonEmptyLayer(Action<int, TileLayerData> action)
        {
            if (!ground.IsEmpty) action(MapConstants.LAYER_GROUND, ground);
            if (!floor.IsEmpty) action(MapConstants.LAYER_FLOOR, floor);
            if (!floorDecor.IsEmpty) action(MapConstants.LAYER_FLOOR_DECOR, floorDecor);
            if (!wall.IsEmpty) action(MapConstants.LAYER_WALL, wall);
            if (!wallDecor.IsEmpty) action(MapConstants.LAYER_WALL_DECOR, wallDecor);
            if (!roof.IsEmpty) action(MapConstants.LAYER_ROOF, roof);
        }
        
        #endregion
        
        #region IEquatable 实现
        
        public bool Equals(TileData other)
        {
            return ground.Equals(other.ground)
                && floor.Equals(other.floor)
                && floorDecor.Equals(other.floorDecor)
                && wall.Equals(other.wall)
                && wallDecor.Equals(other.wallDecor)
                && roof.Equals(other.roof);
        }
        
        public override bool Equals(object obj)
        {
            return obj is TileData other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ground.GetHashCode();
                hash = (hash * 397) ^ floor.GetHashCode();
                hash = (hash * 397) ^ floorDecor.GetHashCode();
                hash = (hash * 397) ^ wall.GetHashCode();
                hash = (hash * 397) ^ wallDecor.GetHashCode();
                hash = (hash * 397) ^ roof.GetHashCode();
                return hash;
            }
        }
        
        public static bool operator ==(TileData a, TileData b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(TileData a, TileData b)
        {
            return !a.Equals(b);
        }
        
        #endregion
        
        public override string ToString()
        {
            if (IsEmpty)
            {
                return "Tile(Empty)";
            }
            return $"Tile(Layers:{NonEmptyLayerCount}, Wall:{HasWall}, Roof:{HasRoof})";
        }
    }
}
