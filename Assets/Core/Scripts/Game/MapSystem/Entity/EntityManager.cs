/*******************************************************************************
 * 文件名:    EntityManager.cs
 * 描述:      实体管理器，统一管理实体的创建、生成、销毁
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   EntityManager 是实体系统的中央管理器，负责：
 *   - 实体的创建和销毁
 *   - 实体的生成和移除
 *   - 全局实体索引
 *   - Tick更新调度
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 实体管理器
    /// </summary>
    public class EntityManager
    {
        #region 字段

        /// <summary>
        /// 所属Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 全局实体列表
        /// </summary>
        private GlobalEntityLister _globalLister;

        /// <summary>
        /// 每层的EntityGrid
        /// </summary>
        private Dictionary<int, EntityGrid> _entityGrids;

        /// <summary>
        /// 每层的EntityLister
        /// </summary>
        private Dictionary<int, EntityLister> _entityListers;

        /// <summary>
        /// 待销毁的实体
        /// </summary>
        private List<Entity> _pendingDestroy;

        /// <summary>
        /// 实体工厂
        /// </summary>
        private Dictionary<EntityCategory, Func<EntityDef, Entity>> _entityFactories;

        #endregion

        #region 属性

        /// <summary>
        /// 所属Site
        /// </summary>
        public Site Site => _site;

        /// <summary>
        /// 全局实体列表
        /// </summary>
        public GlobalEntityLister GlobalLister => _globalLister;

        /// <summary>
        /// 实体总数
        /// </summary>
        public int TotalEntityCount => _globalLister.Count;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityManager(Site site)
        {
            _site = site;
            _globalLister = new GlobalEntityLister(site);
            _entityGrids = new Dictionary<int, EntityGrid>();
            _entityListers = new Dictionary<int, EntityLister>();
            _pendingDestroy = new List<Entity>();
            _entityFactories = new Dictionary<EntityCategory, Func<EntityDef, Entity>>();

            // 注册默认工厂
            RegisterDefaultFactories();
        }

        /// <summary>
        /// 注册默认实体工厂
        /// </summary>
        private void RegisterDefaultFactories()
        {
            _entityFactories[EntityCategory.None] = def => new Entity(def);
            _entityFactories[EntityCategory.Building] = def => new Building(def as BuildingDef ?? new BuildingDef { _defId = def?.DefId });
            _entityFactories[EntityCategory.Item] = def => new Item(def as ItemDef ?? new ItemDef { _defId = def?.DefId });
            _entityFactories[EntityCategory.Pawn] = def => new Entity(def);
            _entityFactories[EntityCategory.Plant] = def => new Entity(def);
            _entityFactories[EntityCategory.Filth] = def => new Entity(def);
            _entityFactories[EntityCategory.Projectile] = def => new Entity(def);
            _entityFactories[EntityCategory.Mote] = def => new Entity(def);
            _entityFactories[EntityCategory.Blueprint] = def => new Entity(def);
            _entityFactories[EntityCategory.Frame] = def => new Entity(def);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 为每个楼层创建EntityGrid和EntityLister
            for (int floorIndex = _site.MinFloor; floorIndex <= _site.MaxFloor; floorIndex++)
            {
                var floor = _site.GetFloor(floorIndex);
                if (floor != null)
                {
                    _entityGrids[floorIndex] = new EntityGrid(floor);
                    _entityListers[floorIndex] = new EntityLister(floor);
                }
            }
        }

        #endregion

        #region 工厂注册

        /// <summary>
        /// 注册实体工厂
        /// </summary>
        public void RegisterFactory(EntityCategory category, Func<EntityDef, Entity> factory)
        {
            _entityFactories[category] = factory;
        }

        /// <summary>
        /// 注册泛型实体工厂
        /// </summary>
        public void RegisterFactory<T>(EntityCategory category) where T : Entity, new()
        {
            _entityFactories[category] = def =>
            {
                var entity = new T();
                entity.SetDef(def);
                return entity;
            };
        }

        #endregion

        #region 实体创建

        /// <summary>
        /// 创建实体（不生成到地图）
        /// </summary>
        public Entity CreateEntity(string defId)
        {
            var def = DefDatabase.GetDef<EntityDef>(defId);
            return CreateEntity(def);
        }

        /// <summary>
        /// 创建实体（不生成到地图）
        /// </summary>
        public Entity CreateEntity(EntityDef def)
        {
            if (def == null)
                return null;

            Entity entity;
            if (_entityFactories.TryGetValue(def.Category, out var factory))
            {
                entity = factory(def);
            }
            else
            {
                entity = new Entity(def);
            }

            return entity;
        }

        /// <summary>
        /// 创建并生成实体
        /// </summary>
        public Entity SpawnEntity(string defId, Floor floor, CellCoord position, 
            Rotation rotation = default)
        {
            var entity = CreateEntity(defId);
            if (entity != null)
            {
                SpawnEntity(entity, floor, position, rotation);
            }
            return entity;
        }

        /// <summary>
        /// 创建并生成实体
        /// </summary>
        public Entity SpawnEntity(EntityDef def, Floor floor, CellCoord position, 
            Rotation rotation = default)
        {
            var entity = CreateEntity(def);
            if (entity != null)
            {
                SpawnEntity(entity, floor, position, rotation);
            }
            return entity;
        }

        #endregion

        #region 实体生成/移除

        /// <summary>
        /// 生成实体到地图
        /// </summary>
        public bool SpawnEntity(Entity entity, Floor floor, CellCoord position, 
            Rotation rotation = default)
        {
            if (entity == null || floor == null)
                return false;

            if (entity.Spawned)
            {
                Debug.LogWarning($"[EntityManager] Entity {entity} is already spawned");
                return false;
            }

            int floorIndex = floor.FloorIndex;

            // 检查是否可以放置
            if (!CanPlaceAt(entity.Def, floor, position, rotation))
            {
                Debug.LogWarning($"[EntityManager] Cannot place {entity} at {position}");
                return false;
            }

            // 生成实体
            entity.SpawnSetup(floor, position, rotation);

            // 注册到各个索引
            _globalLister.Register(entity);

            if (_entityGrids.TryGetValue(floorIndex, out var grid))
            {
                grid.Register(entity);
            }

            if (_entityListers.TryGetValue(floorIndex, out var lister))
            {
                lister.Register(entity);
            }

            return true;
        }

        /// <summary>
        /// 从地图移除实体
        /// </summary>
        public void DeSpawnEntity(Entity entity)
        {
            if (entity == null || !entity.Spawned)
                return;

            int floorIndex = entity.FloorIndex;

            // 从索引中移除
            if (_entityGrids.TryGetValue(floorIndex, out var grid))
            {
                grid.Unregister(entity);
            }

            if (_entityListers.TryGetValue(floorIndex, out var lister))
            {
                lister.Unregister(entity);
            }

            _globalLister.Unregister(entity);

            // 执行DeSpawn
            entity.DeSpawn();
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        public void DestroyEntity(Entity entity)
        {
            if (entity == null)
                return;

            if (entity.Spawned)
            {
                DeSpawnEntity(entity);
            }

            entity.Destroy();
        }

        /// <summary>
        /// 延迟销毁实体（在Tick结束后处理）
        /// </summary>
        public void DestroyEntityDeferred(Entity entity)
        {
            if (entity != null && !_pendingDestroy.Contains(entity))
            {
                _pendingDestroy.Add(entity);
            }
        }

        /// <summary>
        /// 处理待销毁实体
        /// </summary>
        public void ProcessPendingDestroy()
        {
            foreach (var entity in _pendingDestroy)
            {
                DestroyEntity(entity);
            }
            _pendingDestroy.Clear();
        }

        #endregion

        #region 实体移动

        /// <summary>
        /// 移动实体到新位置（同层）
        /// </summary>
        public bool MoveEntity(Entity entity, CellCoord newPosition)
        {
            if (entity == null || !entity.Spawned)
                return false;

            var floor = entity.Floor;
            var oldPosition = entity.Position;

            // 检查新位置
            if (!CanPlaceAt(entity.Def, floor, newPosition, entity.Rotation))
                return false;

            // 更新位置
            entity.SetPosition(newPosition);

            // 更新空间索引
            if (_entityGrids.TryGetValue(floor.FloorIndex, out var grid))
            {
                grid.UpdatePosition(entity, oldPosition, newPosition);
            }

            return true;
        }

        /// <summary>
        /// 移动实体到新楼层
        /// </summary>
        public bool MoveEntityToFloor(Entity entity, Floor newFloor, CellCoord newPosition)
        {
            if (entity == null || !entity.Spawned || newFloor == null)
                return false;

            var oldFloor = entity.Floor;
            var oldPosition = entity.Position;

            // 检查新位置
            if (!CanPlaceAt(entity.Def, newFloor, newPosition, entity.Rotation))
                return false;

            int oldFloorIndex = oldFloor.FloorIndex;
            int newFloorIndex = newFloor.FloorIndex;

            // 从旧楼层移除
            if (_entityGrids.TryGetValue(oldFloorIndex, out var oldGrid))
            {
                oldGrid.Unregister(entity);
            }
            if (_entityListers.TryGetValue(oldFloorIndex, out var oldLister))
            {
                oldLister.Unregister(entity);
            }

            // 执行楼层移动
            entity.MoveToFloor(newFloor, newPosition);

            // 注册到新楼层
            if (_entityGrids.TryGetValue(newFloorIndex, out var newGrid))
            {
                newGrid.Register(entity);
            }
            if (_entityListers.TryGetValue(newFloorIndex, out var newLister))
            {
                newLister.Register(entity);
            }

            return true;
        }

        #endregion

        #region 放置检查

        /// <summary>
        /// 检查是否可以放置
        /// </summary>
        public bool CanPlaceAt(EntityDef def, Floor floor, CellCoord position, Rotation rotation)
        {
            if (def == null || floor == null)
                return false;

            if (_entityGrids.TryGetValue(floor.FloorIndex, out var grid))
            {
                return grid.CanPlaceAt(def, position, rotation);
            }

            return false;
        }

        /// <summary>
        /// 获取放置失败原因
        /// </summary>
        public string GetPlaceFailReason(EntityDef def, Floor floor, CellCoord position, Rotation rotation)
        {
            if (def == null)
                return "无效的定义";
            if (floor == null)
                return "无效的楼层";

            if (_entityGrids.TryGetValue(floor.FloorIndex, out var grid))
            {
                return grid.GetPlaceFailReason(def, position, rotation);
            }

            return "楼层索引无效";
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取楼层的EntityGrid
        /// </summary>
        public EntityGrid GetEntityGrid(int floorIndex)
        {
            _entityGrids.TryGetValue(floorIndex, out var grid);
            return grid;
        }

        /// <summary>
        /// 获取楼层的EntityLister
        /// </summary>
        public EntityLister GetEntityLister(int floorIndex)
        {
            _entityListers.TryGetValue(floorIndex, out var lister);
            return lister;
        }

        /// <summary>
        /// 通过ID获取实体
        /// </summary>
        public Entity GetEntityById(int entityId)
        {
            return _globalLister.GetById(entityId);
        }

        /// <summary>
        /// 获取指定位置的实体
        /// </summary>
        public IReadOnlyList<Entity> GetEntitiesAt(GlobalCoord coord)
        {
            if (_entityGrids.TryGetValue(coord.y, out var grid))
            {
                return grid.GetEntitiesAt(coord.ToCellCoord());
            }
            return Array.Empty<Entity>();
        }

        /// <summary>
        /// 获取指定位置的阻挡实体
        /// </summary>
        public Entity GetBlockingEntityAt(GlobalCoord coord)
        {
            if (_entityGrids.TryGetValue(coord.y, out var grid))
            {
                return grid.GetBlockingEntityAt(coord.ToCellCoord());
            }
            return null;
        }

        #endregion

        #region Tick

        /// <summary>
        /// Tick更新
        /// </summary>
        public void Tick()
        {
            // Tick所有楼层的实体
            foreach (var lister in _entityListers.Values)
            {
                lister.TickAll();
            }

            // 处理待销毁
            ProcessPendingDestroy();
        }

        /// <summary>
        /// 稀有Tick
        /// </summary>
        public void TickRare()
        {
            foreach (var lister in _entityListers.Values)
            {
                lister.TickRare();
            }
        }

        /// <summary>
        /// 长周期Tick
        /// </summary>
        public void TickLong()
        {
            foreach (var lister in _entityListers.Values)
            {
                lister.TickLong();
            }
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清空所有实体
        /// </summary>
        public void Clear()
        {
            // 销毁所有实体
            foreach (var entity in _globalLister.AllEntities.ToArray())
            {
                DestroyEntity(entity);
            }

            _pendingDestroy.Clear();

            foreach (var grid in _entityGrids.Values)
            {
                grid.Clear();
            }

            foreach (var lister in _entityListers.Values)
            {
                lister.Clear();
            }

            _globalLister.Clear();
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
                ["TotalEntities"] = _globalLister.Count,
                ["PendingDestroy"] = _pendingDestroy.Count
            };

            foreach (EntityCategory category in Enum.GetValues(typeof(EntityCategory)))
            {
                var list = _globalLister.GetByCategory(category);
                if (list.Count > 0)
                {
                    stats[$"Category_{category}"] = list.Count;
                }
            }

            return stats;
        }

        #endregion
    }

    /// <summary>
    /// 使EntityManager需要的扩展
    /// </summary>
    internal static class EntityManagerExtensions
    {
        public static T[] ToArray<T>(this IEnumerable<T> source)
        {
            if (source is T[] array)
                return array;
            return System.Linq.Enumerable.ToArray(source);
        }
    }
}
