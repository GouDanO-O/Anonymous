/*******************************************************************************
 * 文件名:    EntityLister.cs
 * 描述:      实体列表管理器，按类型和分类索引实体
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   EntityLister 提供按类型和分类的实体快速查询：
 *   - 按EntityCategory分类
 *   - 按DefId分类
 *   - 按自定义标签分类
 *   - 支持快速遍历特定类型的实体
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 实体列表管理器
    /// </summary>
    public class EntityLister
    {
        #region 字段

        /// <summary>
        /// 所属楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 所有实体
        /// </summary>
        private HashSet<Entity> _allEntities;

        /// <summary>
        /// 按分类索引
        /// </summary>
        private Dictionary<EntityCategory, List<Entity>> _byCategory;

        /// <summary>
        /// 按DefId索引
        /// </summary>
        private Dictionary<string, List<Entity>> _byDefId;

        /// <summary>
        /// 按自定义标签索引
        /// </summary>
        private Dictionary<string, HashSet<Entity>> _byTag;

        /// <summary>
        /// 需要Tick的实体
        /// </summary>
        private List<Entity> _tickables;

        /// <summary>
        /// 需要稀有Tick的实体
        /// </summary>
        private List<Entity> _rareTickables;

        #endregion

        #region 属性

        /// <summary>
        /// 所属楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// 实体总数
        /// </summary>
        public int Count => _allEntities.Count;

        /// <summary>
        /// 所有实体
        /// </summary>
        public IEnumerable<Entity> AllEntities => _allEntities;

        /// <summary>
        /// 所有建筑
        /// </summary>
        public IEnumerable<Entity> Buildings => GetByCategory(EntityCategory.Building);

        /// <summary>
        /// 所有物品
        /// </summary>
        public IEnumerable<Entity> Items => GetByCategory(EntityCategory.Item);

        /// <summary>
        /// 所有生物
        /// </summary>
        public IEnumerable<Entity> Pawns => GetByCategory(EntityCategory.Pawn);

        /// <summary>
        /// 所有植物
        /// </summary>
        public IEnumerable<Entity> Plants => GetByCategory(EntityCategory.Plant);

        /// <summary>
        /// 需要Tick的实体
        /// </summary>
        public IReadOnlyList<Entity> Tickables => _tickables;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityLister(Floor floor)
        {
            _floor = floor;
            _allEntities = new HashSet<Entity>();
            _byCategory = new Dictionary<EntityCategory, List<Entity>>();
            _byDefId = new Dictionary<string, List<Entity>>();
            _byTag = new Dictionary<string, HashSet<Entity>>();
            _tickables = new List<Entity>();
            _rareTickables = new List<Entity>();

            // 初始化分类列表
            foreach (EntityCategory category in Enum.GetValues(typeof(EntityCategory)))
            {
                _byCategory[category] = new List<Entity>();
            }
        }

        #endregion

        #region 注册/注销

        /// <summary>
        /// 注册实体
        /// </summary>
        public void Register(Entity entity)
        {
            if (entity == null || _allEntities.Contains(entity))
                return;

            _allEntities.Add(entity);

            // 按分类索引
            if (_byCategory.TryGetValue(entity.Category, out var categoryList))
            {
                categoryList.Add(entity);
            }

            // 按DefId索引
            string defId = entity.DefId;
            if (!string.IsNullOrEmpty(defId))
            {
                if (!_byDefId.TryGetValue(defId, out var defList))
                {
                    defList = new List<Entity>();
                    _byDefId[defId] = defList;
                }
                defList.Add(entity);
            }

            // 添加到Tick列表
            _tickables.Add(entity);

            // TODO: 根据实体类型决定是否加入稀有Tick列表
            if (ShouldRareTick(entity))
            {
                _rareTickables.Add(entity);
            }
        }

        /// <summary>
        /// 注销实体
        /// </summary>
        public void Unregister(Entity entity)
        {
            if (entity == null || !_allEntities.Contains(entity))
                return;

            _allEntities.Remove(entity);

            // 从分类索引移除
            if (_byCategory.TryGetValue(entity.Category, out var categoryList))
            {
                categoryList.Remove(entity);
            }

            // 从DefId索引移除
            string defId = entity.DefId;
            if (!string.IsNullOrEmpty(defId) && _byDefId.TryGetValue(defId, out var defList))
            {
                defList.Remove(entity);
            }

            // 从标签索引移除
            foreach (var tagSet in _byTag.Values)
            {
                tagSet.Remove(entity);
            }

            // 从Tick列表移除
            _tickables.Remove(entity);
            _rareTickables.Remove(entity);
        }

        /// <summary>
        /// 判断是否需要稀有Tick
        /// </summary>
        private bool ShouldRareTick(Entity entity)
        {
            // 植物、建筑等通常需要稀有Tick
            return entity.Category == EntityCategory.Plant ||
                   entity.Category == EntityCategory.Building;
        }

        #endregion

        #region 标签系统

        /// <summary>
        /// 为实体添加标签
        /// </summary>
        public void AddTag(Entity entity, string tag)
        {
            if (entity == null || string.IsNullOrEmpty(tag))
                return;

            if (!_allEntities.Contains(entity))
                return;

            if (!_byTag.TryGetValue(tag, out var tagSet))
            {
                tagSet = new HashSet<Entity>();
                _byTag[tag] = tagSet;
            }
            tagSet.Add(entity);
        }

        /// <summary>
        /// 移除实体标签
        /// </summary>
        public void RemoveTag(Entity entity, string tag)
        {
            if (entity == null || string.IsNullOrEmpty(tag))
                return;

            if (_byTag.TryGetValue(tag, out var tagSet))
            {
                tagSet.Remove(entity);
            }
        }

        /// <summary>
        /// 检查实体是否有标签
        /// </summary>
        public bool HasTag(Entity entity, string tag)
        {
            if (_byTag.TryGetValue(tag, out var tagSet))
            {
                return tagSet.Contains(entity);
            }
            return false;
        }

        /// <summary>
        /// 获取有指定标签的实体
        /// </summary>
        public IEnumerable<Entity> GetByTag(string tag)
        {
            if (_byTag.TryGetValue(tag, out var tagSet))
            {
                return tagSet;
            }
            return Enumerable.Empty<Entity>();
        }

        #endregion

        #region 查询

        /// <summary>
        /// 按分类获取实体
        /// </summary>
        public IReadOnlyList<Entity> GetByCategory(EntityCategory category)
        {
            if (_byCategory.TryGetValue(category, out var list))
            {
                return list;
            }
            return Array.Empty<Entity>();
        }

        /// <summary>
        /// 按DefId获取实体
        /// </summary>
        public IReadOnlyList<Entity> GetByDefId(string defId)
        {
            if (_byDefId.TryGetValue(defId, out var list))
            {
                return list;
            }
            return Array.Empty<Entity>();
        }

        /// <summary>
        /// 获取指定类型的实体
        /// </summary>
        public IEnumerable<T> GetAll<T>() where T : Entity
        {
            foreach (var entity in _allEntities)
            {
                if (entity is T typed)
                    yield return typed;
            }
        }

        /// <summary>
        /// 获取指定分类的数量
        /// </summary>
        public int CountByCategory(EntityCategory category)
        {
            if (_byCategory.TryGetValue(category, out var list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// 获取指定DefId的数量
        /// </summary>
        public int CountByDefId(string defId)
        {
            if (_byDefId.TryGetValue(defId, out var list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// 按条件查询
        /// </summary>
        public IEnumerable<Entity> Where(Func<Entity, bool> predicate)
        {
            return _allEntities.Where(predicate);
        }

        /// <summary>
        /// 查找第一个匹配的实体
        /// </summary>
        public Entity FirstOrDefault(Func<Entity, bool> predicate)
        {
            return _allEntities.FirstOrDefault(predicate);
        }

        /// <summary>
        /// 随机获取一个实体
        /// </summary>
        public Entity GetRandom()
        {
            if (_allEntities.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, _allEntities.Count);
            return _allEntities.ElementAt(index);
        }

        /// <summary>
        /// 随机获取指定分类的实体
        /// </summary>
        public Entity GetRandom(EntityCategory category)
        {
            if (!_byCategory.TryGetValue(category, out var list) || list.Count == 0)
                return null;

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        #endregion

        #region Tick

        /// <summary>
        /// Tick所有实体
        /// </summary>
        public void TickAll()
        {
            // 使用倒序遍历，避免Tick中移除实体导致问题
            for (int i = _tickables.Count - 1; i >= 0; i--)
            {
                if (i < _tickables.Count)
                {
                    _tickables[i].Tick();
                }
            }
        }

        /// <summary>
        /// 稀有Tick
        /// </summary>
        public void TickRare()
        {
            for (int i = _rareTickables.Count - 1; i >= 0; i--)
            {
                if (i < _rareTickables.Count)
                {
                    _rareTickables[i].TickRare();
                }
            }
        }

        /// <summary>
        /// 长周期Tick
        /// </summary>
        public void TickLong()
        {
            foreach (var entity in _allEntities)
            {
                entity.TickLong();
            }
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清空所有实体
        /// </summary>
        public void Clear()
        {
            _allEntities.Clear();
            foreach (var list in _byCategory.Values)
            {
                list.Clear();
            }
            _byDefId.Clear();
            _byTag.Clear();
            _tickables.Clear();
            _rareTickables.Clear();
        }

        #endregion

        #region 统计

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStats()
        {
            var stats = new Dictionary<string, int>
            {
                ["Total"] = _allEntities.Count,
                ["Tickables"] = _tickables.Count,
                ["RareTickables"] = _rareTickables.Count
            };

            foreach (var kvp in _byCategory)
            {
                if (kvp.Value.Count > 0)
                {
                    stats[kvp.Key.ToString()] = kvp.Value.Count;
                }
            }

            return stats;
        }

        #endregion
    }

    /// <summary>
    /// 全局实体列表管理器（跨楼层）
    /// </summary>
    public class GlobalEntityLister
    {
        #region 字段

        /// <summary>
        /// 所属Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 所有实体
        /// </summary>
        private HashSet<Entity> _allEntities;

        /// <summary>
        /// 按分类索引
        /// </summary>
        private Dictionary<EntityCategory, List<Entity>> _byCategory;

        /// <summary>
        /// 按DefId索引
        /// </summary>
        private Dictionary<string, List<Entity>> _byDefId;

        /// <summary>
        /// 实体ID -> 实体映射
        /// </summary>
        private Dictionary<int, Entity> _byId;

        /// <summary>
        /// 下一个实体ID
        /// </summary>
        private int _nextEntityId = 1;

        #endregion

        #region 属性

        /// <summary>
        /// 实体总数
        /// </summary>
        public int Count => _allEntities.Count;

        /// <summary>
        /// 所有实体
        /// </summary>
        public IEnumerable<Entity> AllEntities => _allEntities;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public GlobalEntityLister(Site site)
        {
            _site = site;
            _allEntities = new HashSet<Entity>();
            _byCategory = new Dictionary<EntityCategory, List<Entity>>();
            _byDefId = new Dictionary<string, List<Entity>>();
            _byId = new Dictionary<int, Entity>();

            foreach (EntityCategory category in Enum.GetValues(typeof(EntityCategory)))
            {
                _byCategory[category] = new List<Entity>();
            }
        }

        #endregion

        #region ID分配

        /// <summary>
        /// 分配实体ID
        /// </summary>
        public int AllocateEntityId()
        {
            return _nextEntityId++;
        }

        /// <summary>
        /// 设置下一个ID（用于加载存档）
        /// </summary>
        public void SetNextEntityId(int nextId)
        {
            _nextEntityId = nextId;
        }

        #endregion

        #region 注册/注销

        /// <summary>
        /// 注册实体
        /// </summary>
        public void Register(Entity entity)
        {
            if (entity == null || _allEntities.Contains(entity))
                return;

            // 分配ID
            if (entity.EntityId <= 0)
            {
                entity.SetEntityId(AllocateEntityId());
            }

            _allEntities.Add(entity);
            _byId[entity.EntityId] = entity;

            // 按分类索引
            if (_byCategory.TryGetValue(entity.Category, out var categoryList))
            {
                categoryList.Add(entity);
            }

            // 按DefId索引
            string defId = entity.DefId;
            if (!string.IsNullOrEmpty(defId))
            {
                if (!_byDefId.TryGetValue(defId, out var defList))
                {
                    defList = new List<Entity>();
                    _byDefId[defId] = defList;
                }
                defList.Add(entity);
            }
        }

        /// <summary>
        /// 注销实体
        /// </summary>
        public void Unregister(Entity entity)
        {
            if (entity == null || !_allEntities.Contains(entity))
                return;

            _allEntities.Remove(entity);
            _byId.Remove(entity.EntityId);

            if (_byCategory.TryGetValue(entity.Category, out var categoryList))
            {
                categoryList.Remove(entity);
            }

            string defId = entity.DefId;
            if (!string.IsNullOrEmpty(defId) && _byDefId.TryGetValue(defId, out var defList))
            {
                defList.Remove(entity);
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 通过ID获取实体
        /// </summary>
        public Entity GetById(int entityId)
        {
            _byId.TryGetValue(entityId, out var entity);
            return entity;
        }

        /// <summary>
        /// 按分类获取实体
        /// </summary>
        public IReadOnlyList<Entity> GetByCategory(EntityCategory category)
        {
            if (_byCategory.TryGetValue(category, out var list))
            {
                return list;
            }
            return Array.Empty<Entity>();
        }

        /// <summary>
        /// 按DefId获取实体
        /// </summary>
        public IReadOnlyList<Entity> GetByDefId(string defId)
        {
            if (_byDefId.TryGetValue(defId, out var list))
            {
                return list;
            }
            return Array.Empty<Entity>();
        }

        /// <summary>
        /// 查找最近的实体（跨楼层）
        /// </summary>
        public Entity GetNearest(GlobalCoord from, float maxRadius = float.MaxValue)
        {
            Entity nearest = null;
            float nearestDist = maxRadius;

            foreach (var entity in _allEntities)
            {
                float dist = from.HorizontalManhattanDistance(entity.GlobalPosition) +
                            Mathf.Abs(from.y - entity.FloorIndex) * 10; // 楼层差距权重

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = entity;
                }
            }

            return nearest;
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _allEntities.Clear();
            _byId.Clear();
            foreach (var list in _byCategory.Values)
            {
                list.Clear();
            }
            _byDefId.Clear();
        }

        #endregion
    }
}
