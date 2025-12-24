using System;
using System.Runtime.InteropServices;
using Core.Game.World.Tile.Data.Enums;

namespace Core.Game.World.Tile.Data
{
    /// <summary>
    /// 完整的 Tile 数据
    /// 包含地形层、地板层、装饰层
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileData
    {
        /// <summary>
        /// 地形层 - 仅 z=1 有效
        /// </summary>
        public TileLayerData groundLayerData;   
        
        /// <summary>
        /// 地板层 - 需承重，也是下层天花板
        /// </summary>
        public TileLayerData floorLayerData;
        
        /// <summary>
        /// 地面装饰层
        /// </summary>
        public DecorLayerData decorLayerData;

        #region 属性

        /// <summary>
        /// 是否完全为空
        /// </summary>
        public bool IsEmpty => groundLayerData.IsEmpty && floorLayerData.IsEmpty;
        
        /// <summary>
        /// 是否有地板层
        /// </summary>
        public bool HasFloor => !floorLayerData.IsEmpty;
        
        /// <summary>
        /// 是否有地形层
        /// </summary>
        public bool HasGround => !groundLayerData.IsEmpty;
        
        /// <summary>
        /// 是否有装饰层
        /// </summary>
        public bool HasDecor => !decorLayerData.IsEmpty;
        
        /// <summary>
        /// 地形是否可承重
        /// </summary>
        public bool GroundCanBearWeight => groundLayerData.CanBearWeight;
        
        /// <summary>
        /// 地板是否可承重
        /// </summary>
        public bool FloorCanBearWeight => floorLayerData.CanBearWeight;

        #endregion
        
        #region 静态方法
        
        /// <summary>
        /// 空数据
        /// </summary>
        public static TileData Empty => default;
        
        #endregion
        
        #region 承重检查
        
        /// <summary>
        /// 能否承载实体
        /// 优先检查地板层，其次检查地形层
        /// </summary>
        public bool CanBearingEntity()
        {
            // 有地板则检查地板承重
            if (HasFloor)
                return floorLayerData.CanBearWeight;
            
            // 无地板则检查地形承重
            if (HasGround)
                return groundLayerData.CanBearWeight;
            
            return false;
        }
        
        /// <summary>
        /// 检查能否承载指定重量等级的实体
        /// </summary>
        /// <param name="requiredBearing">实体所需承重等级</param>
        public bool CanBearingEntity(EBearingType requiredBearing)
        {
            if (HasFloor)
                return floorLayerData.CanBear(requiredBearing);
            
            if (HasGround)
                return groundLayerData.CanBear(requiredBearing);
            
            return requiredBearing == EBearingType.None;
        }
        
        /// <summary>
        /// 获取当前承重等级（取地板和地形中较高的）
        /// </summary>
        public EBearingType GetBearingType()
        {
            EBearingType floorBearing = HasFloor ? floorLayerData.tileBearingType : EBearingType.None;
            EBearingType groundBearing = HasGround ? groundLayerData.tileBearingType : EBearingType.None;
            
            return (EBearingType)Math.Max((int)floorBearing, (int)groundBearing);
        }
        
        #endregion
        
        #region 移动检查
        
        /// <summary>
        /// 是否可通行
        /// 优先检查地板层
        /// </summary>
        public bool IsPassable()
        {
            if (HasFloor)
                return floorLayerData.IsPassable;
            
            if (HasGround)
                return groundLayerData.IsPassable;
            
            return false;
        }
        
        /// <summary>
        /// 获取移动效率
        /// </summary>
        public float GetMovementEfficiency()
        {
            if (HasFloor)
                return floorLayerData.ActualMovementEfficiency;
            
            if (HasGround)
                return groundLayerData.ActualMovementEfficiency;
            
            return 0f;
        }
        
        #endregion
        
        #region 设置方法
        
        /// <summary>
        /// 设置地形层（完整参数）
        /// </summary>
        public void SetGround(ushort tileId, EBearingType bearingType, ETileMovementType movementType, float movementEfficiency = 1.0f)
        {
            groundLayerData = TileLayerData.Create(tileId, bearingType, movementType, movementEfficiency);
        }
        
        /// <summary>
        /// 设置地形层（简化版 - 从配置表读取属性）
        /// 需要配合 WorldSystem 使用
        /// </summary>
        public void SetGround(ushort tileId)
        {
            // 简化版本，实际使用时应从 Luban 配置表读取属性
            // 这里暂时用默认值，可通行、可承重
            groundLayerData = TileLayerData.Create(tileId, EBearingType.Heavy, ETileMovementType.Passable, 1.0f);
        }
        
        /// <summary>
        /// 设置地板层（完整参数）
        /// 会清除基于地形的装饰
        /// </summary>
        public void SetFloor(ushort tileId, EBearingType bearingType, ETileMovementType movementType, float movementEfficiency = 1.0f)
        {
            floorLayerData = TileLayerData.Create(tileId, bearingType, movementType, movementEfficiency);
            
            // 清除基于地形的装饰
            if (decorLayerData.IsGroundBased)
            {
                ClearDecor();
            }
        }
        
        /// <summary>
        /// 设置地板层（简化版）
        /// </summary>
        public void SetFloor(ushort tileId)
        {
            SetFloor(tileId, EBearingType.Heavy, ETileMovementType.Passable, 1.0f);
        }
        
        /// <summary>
        /// 设置装饰层
        /// 自动根据是否有地板决定渲染基底
        /// </summary>
        public void SetDecor(ushort decorId)
        {
            var baseType = HasFloor ? EDecorRenderBaseType.Floor : EDecorRenderBaseType.Ground;
            decorLayerData = DecorLayerData.Create(decorId, baseType);
        }
        
        /// <summary>
        /// 设置装饰层（指定基底类型）
        /// </summary>
        public void SetDecor(ushort decorId, EDecorRenderBaseType baseType)
        {
            decorLayerData = DecorLayerData.Create(decorId, baseType);
        }
        
        #endregion
        
        #region 清除方法
        
        /// <summary>
        /// 清除地形层
        /// </summary>
        public void ClearGround()
        {
            groundLayerData = TileLayerData.Empty;
        }
        
        /// <summary>
        /// 清除地板层（同时清除基于地板的装饰）
        /// </summary>
        public void ClearFloor()
        {
            floorLayerData = TileLayerData.Empty;
            
            // 清除基于地板的装饰
            if (decorLayerData.IsFloorBased)
            {
                ClearDecor();
            }
        }
        
        /// <summary>
        /// 清除装饰层
        /// </summary>
        public void ClearDecor()
        {
            decorLayerData = DecorLayerData.Empty;
        }
        
        /// <summary>
        /// 清除所有层
        /// </summary>
        public void ClearAll()
        {
            groundLayerData = TileLayerData.Empty;
            floorLayerData = TileLayerData.Empty;
            decorLayerData = DecorLayerData.Empty;
        }
        
        #endregion
    }
}