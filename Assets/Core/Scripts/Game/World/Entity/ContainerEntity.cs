/**
 * ContainerEntity.cs
 * 容器实体
 * 
 * 继承自 MapEntity，增加库存管理功能
 * 用于：冰箱、柜子、箱子、背包等可存放物品的对象
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 容器内物品数据
    /// </summary>
    [Serializable]
    public struct ContainerSlot
    {
        /// <summary>
        /// 物品ID（对应物品配置表）
        /// </summary>
        public int itemId;
        
        /// <summary>
        /// 数量
        /// </summary>
        public int count;
        
        /// <summary>
        /// 物品耐久度/状态（0-100）
        /// </summary>
        public byte condition;
        
        /// <summary>
        /// 是否为空槽位
        /// </summary>
        public bool IsEmpty => itemId <= 0 || count <= 0;
        
        /// <summary>
        /// 创建物品槽
        /// </summary>
        public static ContainerSlot Create(int itemId, int count, byte condition = 100)
        {
            return new ContainerSlot
            {
                itemId = itemId,
                count = count,
                condition = condition
            };
        }
        
        /// <summary>
        /// 空槽位
        /// </summary>
        public static readonly ContainerSlot Empty = new ContainerSlot();
    }
    
    /// <summary>
    /// 容器实体
    /// </summary>
    [Serializable]
    public class ContainerEntity : MapEntity
    {
        #region 字段
        
        /// <summary>
        /// 容器容量（槽位数量）
        /// </summary>
        [SerializeField]
        private int _capacity;
        
        /// <summary>
        /// 物品槽位列表
        /// </summary>
        [SerializeField]
        private List<ContainerSlot> _slots;
        
        /// <summary>
        /// 是否已被搜索过
        /// </summary>
        [SerializeField]
        private bool _hasBeenSearched;
        
        /// <summary>
        /// 最后访问时间（游戏时间，用于物品腐烂等）
        /// </summary>
        [SerializeField]
        private float _lastAccessTime;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => _capacity;
        
        /// <summary>
        /// 已使用槽位数
        /// </summary>
        public int UsedSlots
        {
            get
            {
                int count = 0;
                foreach (var slot in _slots)
                {
                    if (!slot.IsEmpty) count++;
                }
                return count;
            }
        }
        
        /// <summary>
        /// 剩余空槽位数
        /// </summary>
        public int FreeSlots => _capacity - UsedSlots;
        
        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => FreeSlots <= 0;
        
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsContainerEmpty => UsedSlots == 0;
        
        /// <summary>
        /// 是否已被搜索
        /// </summary>
        public bool HasBeenSearched => _hasBeenSearched;
        
        /// <summary>
        /// 是否已被搜索（HasBeenSearched 的别名）
        /// </summary>
        public bool IsSearched => _hasBeenSearched;
        
        /// <summary>
        /// 所有槽位（只读）
        /// </summary>
        public IReadOnlyList<ContainerSlot> Slots => _slots;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ContainerEntity() : base()
        {
            _slots = new List<ContainerSlot>();
        }
        
        /// <summary>
        /// 创建容器实体
        /// </summary>
        public ContainerEntity(int entityId, int configId, string mapId, 
            TileCoord position, int capacity) 
            : base(entityId, configId, EntityType.Container, mapId, position)
        {
            _capacity = capacity;
            _slots = new List<ContainerSlot>(capacity);
            
            // 初始化空槽位
            for (int i = 0; i < capacity; i++)
            {
                _slots.Add(ContainerSlot.Empty);
            }
            
            // 容器默认属性
            AddFlag(EntityFlags.Interactive);
            AddFlag(EntityFlags.Blocking);
        }
        
        #endregion
        
        #region 物品操作
        
        /// <summary>
        /// 添加物品到容器
        /// </summary>
        /// <returns>实际添加的数量</returns>
        public int AddItem(int itemId, int count, byte condition = 100)
        {
            if (itemId <= 0 || count <= 0) return 0;
            
            int remaining = count;
            
            // 先尝试堆叠到已有的同类物品
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                if (_slots[i].itemId == itemId && _slots[i].condition == condition)
                {
                    // TODO: 这里可以添加堆叠上限检查
                    var slot = _slots[i];
                    slot.count += remaining;
                    _slots[i] = slot;
                    remaining = 0;
                }
            }
            
            // 再放入空槽位
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i] = ContainerSlot.Create(itemId, remaining, condition);
                    remaining = 0;
                }
            }
            
            if (remaining < count)
            {
                MarkDirty();
            }
            
            return count - remaining;
        }
        
        /// <summary>
        /// 从容器移除物品
        /// </summary>
        /// <returns>实际移除的数量</returns>
        public int RemoveItem(int itemId, int count)
        {
            if (itemId <= 0 || count <= 0) return 0;
            
            int remaining = count;
            
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                if (_slots[i].itemId == itemId)
                {
                    var slot = _slots[i];
                    int toRemove = Mathf.Min(slot.count, remaining);
                    slot.count -= toRemove;
                    remaining -= toRemove;
                    
                    if (slot.count <= 0)
                    {
                        _slots[i] = ContainerSlot.Empty;
                    }
                    else
                    {
                        _slots[i] = slot;
                    }
                }
            }
            
            if (remaining < count)
            {
                MarkDirty();
            }
            
            return count - remaining;
        }
        
        /// <summary>
        /// 获取指定槽位的物品
        /// </summary>
        public ContainerSlot GetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count)
            {
                return ContainerSlot.Empty;
            }
            return _slots[slotIndex];
        }
        
        /// <summary>
        /// 设置指定槽位的物品
        /// </summary>
        public void SetSlot(int slotIndex, ContainerSlot slot)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return;
            
            _slots[slotIndex] = slot;
            MarkDirty();
        }
        
        /// <summary>
        /// 设置指定槽位的物品（通过参数）
        /// </summary>
        public void SetSlot(int slotIndex, int itemId, int count, float condition)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return;
            
            _slots[slotIndex] = new ContainerSlot
            {
                itemId = itemId,
                count = count,
                condition = (byte)Mathf.Clamp(condition, 0, 100)
            };
            MarkDirty();
        }
        
        /// <summary>
        /// 清空指定槽位
        /// </summary>
        public ContainerSlot ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count)
            {
                return ContainerSlot.Empty;
            }
            
            var removed = _slots[slotIndex];
            _slots[slotIndex] = ContainerSlot.Empty;
            MarkDirty();
            return removed;
        }
        
        /// <summary>
        /// 清空所有物品
        /// </summary>
        public void ClearAllItems()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i] = ContainerSlot.Empty;
            }
            MarkDirty();
        }
        
        /// <summary>
        /// 清空所有物品（ClearAllItems 的别名）
        /// </summary>
        public void ClearAll()
        {
            ClearAllItems();
        }
        
        /// <summary>
        /// 检查是否包含指定物品
        /// </summary>
        public bool ContainsItem(int itemId)
        {
            foreach (var slot in _slots)
            {
                if (slot.itemId == itemId && slot.count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取指定物品的总数量
        /// </summary>
        public int GetItemCount(int itemId)
        {
            int total = 0;
            foreach (var slot in _slots)
            {
                if (slot.itemId == itemId)
                {
                    total += slot.count;
                }
            }
            return total;
        }
        
        /// <summary>
        /// 查找第一个包含指定物品的槽位索引
        /// </summary>
        public int FindItemSlot(int itemId)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].itemId == itemId && _slots[i].count > 0)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// 查找第一个空槽位索引
        /// </summary>
        public int FindEmptySlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }
        
        #endregion
        
        #region 搜索状态
        
        /// <summary>
        /// 标记为已搜索
        /// </summary>
        public void MarkSearched()
        {
            _hasBeenSearched = true;
            _lastAccessTime = Time.time; // 或使用游戏内时间
            MarkDirty();
        }
        
        /// <summary>
        /// 重置搜索状态（用于物品重生等）
        /// </summary>
        public void ResetSearched()
        {
            _hasBeenSearched = false;
            MarkDirty();
        }
        
        #endregion
        
        #region 预设物品
        
        /// <summary>
        /// 用预设物品列表填充容器
        /// </summary>
        public void FillWithItems(IEnumerable<ContainerSlot> items)
        {
            ClearAllItems();
            
            int slotIndex = 0;
            foreach (var item in items)
            {
                if (slotIndex >= _capacity) break;
                
                _slots[slotIndex] = item;
                slotIndex++;
            }
            
            MarkDirty();
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Container({EntityId}, Pos:{TilePosition}, " +
                   $"Capacity:{UsedSlots}/{_capacity}, Searched:{_hasBeenSearched})";
        }
    }
}
