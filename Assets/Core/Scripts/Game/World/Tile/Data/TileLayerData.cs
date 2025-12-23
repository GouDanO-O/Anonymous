using System;
using System.Runtime.InteropServices;
using Core.Game.World.Map.Data;
using Core.Game.World.Tile.Data.Enums;

namespace Core.Game.World.Tile.Data
{
    /// <summary>
    /// 单层 Tile 数据（4 字节）
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TileLayerData
    {
        /// <summary>
        /// Tile配置ID
        /// </summary>
        public ushort tileId;        
        
        /// <summary>
        /// 精灵变体
        /// </summary>
        public byte spriteVariant;

        /// <summary>
        /// Tile承重能力
        /// </summary>
        public EBearingType tileBearingType;
        
        public bool CanBearWeight => (tileBearingType) != 0;
        
        public static TileLayerData Empty => default;
        
    }
}