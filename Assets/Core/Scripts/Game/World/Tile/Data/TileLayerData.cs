using System;
using System.Runtime.InteropServices;
using Core.Game.World.Tile.Data.Enums;

namespace Core.Game.World.Tile.Data
{
    /// <summary>
    /// 地形层--部分地形可以直接放置物体,部分地形需要在基础之上放置承重层
    /// 地板层--可以放置任何物体
    /// 地面装饰层
    /// 实体层
    /// 天花板层
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TileLayerData
    {
        /// <summary>
        /// Tile索引ID - 对应 Luban 配置表
        /// 0 = 空
        /// </summary>
        public ushort tileId;
        
        /// <summary>
        /// 瓦片的承重能力
        /// </summary>
        public EBearingType tileBearingType;
        
        /// <summary>
        /// 瓦片是否可通行
        /// </summary>
        public ETileMovementType tileMovementType;

        /// <summary>
        /// 瓦片的行动效率
        /// 如果不可通行,则行动效率为0
        /// 1.0 = 正常速度, 0.5 = 半速
        /// </summary>
        public float tileMovementEfficiency;
        
        #region 属性
        
        /// <summary>
        /// 当前Tile是否能够承重
        /// </summary>
        public bool CanBearWeight => !IsEmpty && tileBearingType != EBearingType.None;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => tileId == 0;
        
        /// <summary>
        /// 是否可通行
        /// </summary>
        public bool IsPassable => tileMovementType == ETileMovementType.Passable;
        
        /// <summary>
        /// 获取实际移动效率（不可通行返回0）
        /// </summary>
        public float ActualMovementEfficiency => IsPassable ? tileMovementEfficiency : 0f;
        
        #endregion
        
        #region 静态方法

        /// <summary>
        /// 设置为空
        /// </summary>
        public static TileLayerData Empty => default;

        /// <summary>
        /// 创建瓦片
        /// </summary>
        /// <param name="tileId">配置表ID</param>
        /// <param name="tileBearingType">承重类型</param>
        /// <param name="tileMovementType">移动类型</param>
        /// <param name="tileMovementEfficiency">移动效率（可选）</param>
        public static TileLayerData Create(
            ushort tileId, 
            EBearingType tileBearingType,
            ETileMovementType tileMovementType, 
            float? tileMovementEfficiency = null)
        {
            float movementEfficiency = 0f;
            if (tileMovementType == ETileMovementType.Passable)
            {
                movementEfficiency = tileMovementEfficiency ?? 1.0f;
            }

            return new TileLayerData
            {
                tileId = tileId, 
                tileBearingType = tileBearingType,
                tileMovementType = tileMovementType,
                tileMovementEfficiency = movementEfficiency
            };
        }
        
        /// <summary>
        /// 快速创建可通行、可承重的 Tile
        /// </summary>
        public static TileLayerData CreatePassable(
            ushort tileId, 
            EBearingType bearingType = EBearingType.Heavy, 
            float efficiency = 1.0f)
        {
            return Create(tileId, bearingType, ETileMovementType.Passable, efficiency);
        }
        
        /// <summary>
        /// 快速创建不可通行的 Tile
        /// </summary>
        public static TileLayerData CreateImpassable(
            ushort tileId, 
            EBearingType bearingType = EBearingType.None)
        {
            return Create(tileId, bearingType, ETileMovementType.UnPassable, 0f);
        }
        
        #endregion
        
        #region 承重检查
        
        /// <summary>
        /// 检查是否能承载指定重量等级的物体
        /// </summary>
        /// <param name="requiredBearing">物体所需的承重等级</param>
        /// <returns>当前承重等级 >= 所需等级</returns>
        public bool CanBear(EBearingType requiredBearing)
        {
            if (requiredBearing == EBearingType.None)
                return false;
            
            return (int)tileBearingType >= (int)requiredBearing;
        }
        
        #endregion
    }
}