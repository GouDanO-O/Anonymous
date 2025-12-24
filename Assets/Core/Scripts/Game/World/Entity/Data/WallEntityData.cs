using System;
using Core.Game.World.Entity.Data.Enums;
using Core.Game.World.Tile;

namespace Core.Game.World.Entity.Data
{
/// <summary>
    /// 墙实体数据
    /// </summary>
    [Serializable]
    public class WallEntityData : EntityData
    {
        /// <summary>
        /// 承重半径
        /// 支撑 (2*radius+1)² - 1 个格子的上层地板（不含本格）
        /// 例如 radius=1 表示 3×3 范围，支撑周围 8 格
        /// </summary>
        public int bearingRadius;
        
        /// <summary>
        /// 墙体材质类型
        /// 影响耐久度和外观
        /// </summary>
        public EWallMaterialType materialType;
        
        /// <summary>
        /// 依赖此墙的上层地板数量（运行时计算，不序列化）
        /// </summary>
        [NonSerialized]
        public int dependentFloorCount;
        
        #region 构造函数
        
        public WallEntityData() : base()
        {
            category = EEntityCategory.Wall;
            bearingRadius = 1;
            materialType = EWallMaterialType.Wood;
            dependentFloorCount = 0;
            
            // 默认属性
            AddProperty(EEntityProperty.BlocksMovement);
            AddProperty(EEntityProperty.BlocksVision);
            AddProperty(EEntityProperty.Destructible);
            AddProperty(EEntityProperty.Placeable);
            AddProperty(EEntityProperty.IsBearingStructure);
        }
        
        public WallEntityData(int configId, TileCoord position, int bearingRadius = 1) 
            : base(configId, EEntityCategory.Wall, position)
        {
            this.bearingRadius = bearingRadius;
            this.materialType = EWallMaterialType.Wood;
            this.dependentFloorCount = 0;
            
            AddProperty(EEntityProperty.BlocksMovement);
            AddProperty(EEntityProperty.BlocksVision);
            AddProperty(EEntityProperty.Destructible);
            AddProperty(EEntityProperty.Placeable);
            AddProperty(EEntityProperty.IsBearingStructure);
        }
        
        #endregion
        
        #region 承重相关
        
        /// <summary>
        /// 获取承重范围的边长
        /// </summary>
        public int BearingDiameter => bearingRadius * 2 + 1;
        
        /// <summary>
        /// 检查目标位置是否在承重范围内
        /// </summary>
        /// <param name="targetPos">上层地板位置</param>
        /// <returns>是否在承重范围内</returns>
        public bool IsInBearingRange(TileCoord targetPos)
        {
            // 必须是上一层
            if (targetPos.z != position.z + 1)
                return false;
            
            // 承重点本身不算
            if (targetPos.x == position.x && targetPos.y == position.y)
                return false;
            
            int dx = Math.Abs(targetPos.x - position.x);
            int dy = Math.Abs(targetPos.y - position.y);
            
            return dx <= bearingRadius && dy <= bearingRadius;
        }
        
        /// <summary>
        /// 检查 2D 坐标是否在承重范围内（同层）
        /// </summary>
        public bool IsInBearingRange2D(int x, int y)
        {
            if (x == position.x && y == position.y)
                return false;
            
            return Math.Abs(x - position.x) <= bearingRadius &&
                   Math.Abs(y - position.y) <= bearingRadius;
        }
        
        /// <summary>
        /// 更新承重状态
        /// </summary>
        public void UpdateBearingStatus(int dependentCount)
        {
            dependentFloorCount = dependentCount;
        }
        
        /// <summary>
        /// 是否正在承重
        /// </summary>
        public bool IsCurrentlyBearing => dependentFloorCount > 0;
        
        #endregion
        
        #region 重写方法
        
        /// <summary>
        /// 检查是否可以被移除
        /// 如果有上层地板依赖，则不能移除
        /// </summary>
        public override bool CanBeRemoved()
        {
            return dependentFloorCount == 0;
        }
        
        #endregion
    }
}