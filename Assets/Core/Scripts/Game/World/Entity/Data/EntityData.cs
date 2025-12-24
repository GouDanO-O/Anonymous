using System;
using Core.Game.World.Entity.Data.Enums;
using Core.Game.World.Tile;
using Core.Game.World.Tile.Data.Enums;

namespace Core.Game.World.Entity.Data
{
    /// <summary>
    /// 实体基础数据
    /// </summary>
    [Serializable]
    public class EntityData
    {
        /// <summary>
        /// 实体唯一 ID（运行时分配）
        /// </summary>
        public int entityId;
        
        /// <summary>
        /// 实体配置 ID - 对应 Luban 配置表
        /// </summary>
        public int configId;
        
        /// <summary>
        /// 实体类别
        /// </summary>
        public EEntityCategory category;
        
        /// <summary>
        /// 实体位置（Tile 坐标）
        /// </summary>
        public TileCoord position;
        
        /// <summary>
        /// 实体朝向
        /// </summary>
        public EEntityRotation rotation;
        
        /// <summary>
        /// 实体属性（标志位）
        /// </summary>
        public EEntityProperty properties;
        
        /// <summary>
        /// 实体所需的承重等级
        /// 放置时检查地面承重是否满足
        /// </summary>
        public EBearingType requiredBearingType;
        
        /// <summary>
        /// 当前耐久度
        /// </summary>
        public int durability;
        
        /// <summary>
        /// 最大耐久度
        /// </summary>
        public int maxDurability;
        
        #region 构造函数
        
        public EntityData()
        {
            entityId = 0;
            configId = 0;
            category = EEntityCategory.None;
            position = new TileCoord(0, 0, 1);
            rotation = EEntityRotation.North;
            properties = EEntityProperty.None;
            requiredBearingType = EBearingType.Light;
            durability = 100;
            maxDurability = 100;
        }
        
        public EntityData(int configId, EEntityCategory category, TileCoord position)
        {
            this.entityId = 0;
            this.configId = configId;
            this.category = category;
            this.position = position;
            this.rotation = EEntityRotation.North;
            this.properties = EEntityProperty.None;
            this.requiredBearingType = EBearingType.Light;
            this.durability = 100;
            this.maxDurability = 100;
        }
        
        #endregion
        
        #region 属性操作
        
        public bool HasProperty(EEntityProperty property) => (properties & property) != 0;
        public void AddProperty(EEntityProperty property) => properties |= property;
        public void RemoveProperty(EEntityProperty property) => properties &= ~property;
        
        /// <summary>
        /// 阻碍移动
        /// </summary>
        public bool BlocksMovement => HasProperty(EEntityProperty.BlocksMovement);
        
        /// <summary>
        /// 阻碍视线
        /// </summary>
        public bool BlocksVision => HasProperty(EEntityProperty.BlocksVision);
        
        /// <summary>
        /// 可互动
        /// </summary>
        public bool IsInteractable => HasProperty(EEntityProperty.Interactable);
        
        /// <summary>
        /// 可被破坏
        /// </summary>
        public bool IsDestructible => HasProperty(EEntityProperty.Destructible);
        
        /// <summary>
        /// 可被放置（可拆卸又可再次安装）
        /// </summary>
        public bool IsPlaceable => HasProperty(EEntityProperty.Placeable);
        
        /// <summary>
        /// 是承重结构
        /// </summary>
        public bool IsBearingStructure => HasProperty(EEntityProperty.IsBearingStructure);
        
        /// <summary>
        /// 无法再放置其他物体
        /// </summary>
        public bool CantPlaceOther => HasProperty(EEntityProperty.CantPlaceOtherEntity);
        
        #endregion
        
        #region 虚方法
        
        /// <summary>
        /// 交互
        /// </summary>
        public virtual void Interact(object interactor) { }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (!IsDestructible) return;
            
            durability = Math.Max(0, durability - damage);
            if (durability == 0)
            {
                OnDestroyed();
            }
        }
        
        /// <summary>
        /// 被摧毁时调用
        /// </summary>
        protected virtual void OnDestroyed() { }
        
        /// <summary>
        /// 检查是否可以被移除
        /// </summary>
        public virtual bool CanBeRemoved() => true;
        
        #endregion
    }
}