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
        /// Tile索引ID
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
        /// </summary>
        public float tileMovementEfficiency;
        
        /// <summary>
        /// 当前Tile是否能够承重
        /// </summary>
        public bool CanBearWeight => (tileBearingType) != 0;

        public bool IsEmpty()
        {
            return tileId == 0;
        }

        public static TileLayerData SetEmpty()
        {
            return default;
        }

        /// <summary>
        /// 创建瓦片
        /// </summary>
        /// <param name="spriteId"></param>
        /// <param name="tileBearingType"></param>
        /// <param name="tileMovementType"></param>
        /// <param name="tileMovementEfficiency"></param>
        /// <returns></returns>
        public static TileLayerData CreateTileLayer(ushort tileId, EBearingType tileBearingType,
            ETileMovementType tileMovementType, float? tileMovementEfficiency)
        {
            float movementEfficiency = 0;
            if (tileMovementType == ETileMovementType.Passable)
            {
                movementEfficiency = tileMovementEfficiency ?? 0;
            }
            else
            {
                movementEfficiency = 0;
            }

            return new TileLayerData()
            {
                tileId = tileId, 
                tileBearingType = tileBearingType,
                tileMovementType = tileMovementType,
                tileMovementEfficiency = movementEfficiency
            };
        }
    }
}