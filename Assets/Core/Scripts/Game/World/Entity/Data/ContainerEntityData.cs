using System;
using System.Collections.Generic;
using Core.Game.World.Tile;

namespace Core.Game.World.Entity.Data
{
    /// <summary>
    /// 容器实体数据
    /// </summary>
    [Serializable]
    public class ContainerEntityData : FurnitureEntityData
    {
        /// <summary>
        /// 容量（槽位数）
        /// </summary>
        public int capacity;
        
        /// <summary>
        /// 存储的物品 ID 列表
        /// 实际项目中应该是 List<ItemStack>
        /// </summary>
        public List<int> itemIds;
        
        public ContainerEntityData() : base()
        {
            capacity = 10;
            itemIds = new List<int>();
        }
        
        public ContainerEntityData(int configId, TileCoord position, int capacity = 10) 
            : base(configId, position)
        {
            this.capacity = capacity;
            this.itemIds = new List<int>();
        }
        
        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => itemIds.Count >= capacity;
        
        /// <summary>
        /// 剩余空间
        /// </summary>
        public int FreeSlots => capacity - itemIds.Count;
        
        /// <summary>
        /// 尝试添加物品
        /// </summary>
        public bool TryAddItem(int itemId)
        {
            if (IsFull) return false;
            itemIds.Add(itemId);
            return true;
        }
        
        /// <summary>
        /// 尝试移除物品
        /// </summary>
        public bool TryRemoveItem(int itemId)
        {
            return itemIds.Remove(itemId);
        }
    }
}