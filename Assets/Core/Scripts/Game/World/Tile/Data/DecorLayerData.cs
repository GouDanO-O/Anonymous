using System;
using System.Runtime.InteropServices;
using Core.Game.World.Tile.Data.Enums;

namespace Core.Game.World.Tile.Data
{
    /// <summary>
    /// 地面装饰层
    /// 用于落叶、血迹、弹壳等地面装饰物
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DecorLayerData
    {
        /// <summary>
        /// 装饰配置ID
        /// 0 = 空
        /// </summary>
        public ushort decorId;
        
        /// <summary>
        /// 装饰渲染基底类型
        /// 决定装饰渲染在地形层还是地板层之上
        /// </summary>
        public EDecorRenderBaseType decorRenderBaseType;
        
        #region 属性
        
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => decorId == 0;
        
        /// <summary>
        /// 是否基于地形层渲染
        /// </summary>
        public bool IsGroundBased => decorRenderBaseType == EDecorRenderBaseType.Ground;
        
        /// <summary>
        /// 是否基于地板层渲染
        /// </summary>
        public bool IsFloorBased => decorRenderBaseType == EDecorRenderBaseType.Floor;
        
        #endregion
        
        #region 静态方法
        
        /// <summary>
        /// 空数据
        /// </summary>
        public static DecorLayerData Empty => default;
        
        /// <summary>
        /// 创建装饰层数据
        /// </summary>
        /// <param name="decorId">装饰配置 ID</param>
        /// <param name="baseType">渲染基底类型</param>
        public static DecorLayerData Create(ushort decorId, EDecorRenderBaseType baseType = EDecorRenderBaseType.Ground)
        {
            return new DecorLayerData
            {
                decorId = decorId,
                decorRenderBaseType = baseType
            };
        }
        
        /// <summary>
        /// 创建基于地形的装饰
        /// </summary>
        public static DecorLayerData CreateGroundBased(ushort decorId)
        {
            return Create(decorId, EDecorRenderBaseType.Ground);
        }
        
        /// <summary>
        /// 创建基于地板的装饰
        /// </summary>
        public static DecorLayerData CreateFloorBased(ushort decorId)
        {
            return Create(decorId, EDecorRenderBaseType.Floor);
        }
        
        #endregion
    }
}