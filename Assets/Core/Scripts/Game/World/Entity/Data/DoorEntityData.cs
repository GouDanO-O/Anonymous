using System;
using Core.Game.World.Entity.Data.Enums;
using Core.Game.World.Tile;

namespace Core.Game.World.Entity.Data
{
/// <summary>
    /// 门实体数据
    /// </summary>
    [Serializable]
    public class DoorEntityData : EntityData
    {
        /// <summary>
        /// 门状态
        /// </summary>
        public EDoorState doorState;
        
        /// <summary>
        /// 锁等级（0 = 无锁）
        /// </summary>
        public int lockLevel;
        
        public DoorEntityData() : base()
        {
            category = EEntityCategory.Furniture;
            doorState = EDoorState.Closed;
            lockLevel = 0;
            
            AddProperty(EEntityProperty.BlocksMovement);
            AddProperty(EEntityProperty.BlocksVision);
            AddProperty(EEntityProperty.Destructible);
            AddProperty(EEntityProperty.Placeable);
            AddProperty(EEntityProperty.Interactable);
        }
        
        public DoorEntityData(int configId, TileCoord position) 
            : base(configId, EEntityCategory.Furniture, position)
        {
            doorState = EDoorState.Closed;
            lockLevel = 0;
            
            AddProperty(EEntityProperty.BlocksMovement);
            AddProperty(EEntityProperty.BlocksVision);
            AddProperty(EEntityProperty.Destructible);
            AddProperty(EEntityProperty.Placeable);
            AddProperty(EEntityProperty.Interactable);
        }
        
        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpen => doorState == EDoorState.Open;
        
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked => doorState == EDoorState.Locked;
        
        /// <summary>
        /// 是否损坏
        /// </summary>
        public bool IsBroken => doorState == EDoorState.Broken;
        
        /// <summary>
        /// 交互（开/关门）
        /// </summary>
        public override void Interact(object interactor)
        {
            switch (doorState)
            {
                case EDoorState.Closed:
                    Open();
                    break;
                case EDoorState.Open:
                    Close();
                    break;
                case EDoorState.Locked:
                    // 需要钥匙或撬锁
                    break;
                case EDoorState.Broken:
                    // 已损坏，无法交互
                    break;
            }
        }
        
        /// <summary>
        /// 打开门
        /// </summary>
        public void Open()
        {
            doorState = EDoorState.Open;
            RemoveProperty(EEntityProperty.BlocksMovement);
            RemoveProperty(EEntityProperty.BlocksVision);
        }
        
        /// <summary>
        /// 关闭门
        /// </summary>
        public void Close()
        {
            doorState = EDoorState.Closed;
            AddProperty(EEntityProperty.BlocksMovement);
            AddProperty(EEntityProperty.BlocksVision);
        }
        
        /// <summary>
        /// 锁门
        /// </summary>
        public void Lock(int level = 1)
        {
            if (doorState == EDoorState.Open)
                Close();
            
            doorState = EDoorState.Locked;
            lockLevel = level;
        }
        
        /// <summary>
        /// 尝试解锁
        /// </summary>
        public bool TryUnlock(int keyLevel)
        {
            if (doorState != EDoorState.Locked)
                return false;
            
            if (keyLevel >= lockLevel)
            {
                doorState = EDoorState.Closed;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 破坏时调用
        /// </summary>
        protected override void OnDestroyed()
        {
            doorState = EDoorState.Broken;
            RemoveProperty(EEntityProperty.BlocksMovement);
            RemoveProperty(EEntityProperty.BlocksVision);
        }
    }
}