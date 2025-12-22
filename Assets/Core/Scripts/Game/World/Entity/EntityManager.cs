/**
 * EntityManager.cs
 * 实体管理器
 * 
 * 负责管理地图中所有动态实体的生命周期：
 * - 创建/销毁实体
 * - 按位置查询实体
 * - 按类型查询实体
 * - 实体的增删改查
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 实体管理器
    /// </summary>
    public class EntityManager
    {
        #region 字段
        
        /// <summary>
        /// 所属地图ID
        /// </summary>
        private string _mapId;
        
        /// <summary>
        /// 所有实体（按 ID 索引）
        /// </summary>
        private Dictionary<int, MapEntity> _entities;
        
        /// <summary>
        /// 按 Tile 位置索引的实体（一个位置可能有多个实体）
        /// </summary>
        private Dictionary<TileCoord, List<int>> _entitiesByTile;
        
        /// <summary>
        /// 按 Chunk 索引的实体（用于快速获取某 Chunk 内的所有实体）
        /// </summary>
        private Dictionary<ChunkCoord, HashSet<int>> _entitiesByChunk;
        
        /// <summary>
        /// 下一个可用的实体ID
        /// </summary>
        private int _nextEntityId;
        
        /// <summary>
        /// 脏实体列表（需要保存的）
        /// </summary>
        private HashSet<int> _dirtyEntities;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 所属地图ID
        /// </summary>
        public string MapId => _mapId;
        
        /// <summary>
        /// 实体总数
        /// </summary>
        public int EntityCount => _entities.Count;
        
        /// <summary>
        /// 所有实体（只读）
        /// </summary>
        public IEnumerable<MapEntity> AllEntities => _entities.Values;
        
        /// <summary>
        /// 脏实体数量
        /// </summary>
        public int DirtyCount => _dirtyEntities.Count;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 创建实体管理器
        /// </summary>
        public EntityManager(string mapId)
        {
            _mapId = mapId;
            _entities = new Dictionary<int, MapEntity>();
            _entitiesByTile = new Dictionary<TileCoord, List<int>>();
            _entitiesByChunk = new Dictionary<ChunkCoord, HashSet<int>>();
            _nextEntityId = MapConstants.ENTITY_ID_START;
            _dirtyEntities = new HashSet<int>();
        }
        
        #endregion
        
        #region 实体创建
        
        /// <summary>
        /// 分配新的实体ID
        /// </summary>
        private int AllocateEntityId()
        {
            return _nextEntityId++;
        }
        
        /// <summary>
        /// 创建基础实体
        /// </summary>
        public MapEntity CreateEntity(int configId, EntityType type, TileCoord position)
        {
            int entityId = AllocateEntityId();
            var entity = new MapEntity(entityId, configId, type, _mapId, position);
            
            RegisterEntity(entity);
            return entity;
        }
        
        /// <summary>
        /// 创建容器实体
        /// </summary>
        public ContainerEntity CreateContainer(int configId, TileCoord position, int capacity)
        {
            int entityId = AllocateEntityId();
            var entity = new ContainerEntity(entityId, configId, _mapId, position, capacity);
            
            RegisterEntity(entity);
            return entity;
        }
        
        /// <summary>
        /// 创建门实体
        /// </summary>
        public DoorEntity CreateDoor(int configId, TileCoord position, 
            DoorType doorType = DoorType.Wooden)
        {
            int entityId = AllocateEntityId();
            var entity = new DoorEntity(entityId, configId, _mapId, position, doorType);
            
            RegisterEntity(entity);
            return entity;
        }
        
        /// <summary>
        /// 注册实体到管理器
        /// </summary>
        private void RegisterEntity(MapEntity entity)
        {
            _entities[entity.EntityId] = entity;
            
            // 添加到位置索引
            AddToTileIndex(entity);
            AddToChunkIndex(entity);
            
            // 标记为脏
            _dirtyEntities.Add(entity.EntityId);
        }
        
        /// <summary>
        /// 添加已有实体（从存档加载时使用）
        /// </summary>
        public void AddExistingEntity(MapEntity entity)
        {
            if (_entities.ContainsKey(entity.EntityId))
            {
                Debug.LogWarning($"Entity {entity.EntityId} already exists, replacing...");
                RemoveEntity(entity.EntityId);
            }
            
            _entities[entity.EntityId] = entity;
            AddToTileIndex(entity);
            AddToChunkIndex(entity);
            
            // 更新 nextEntityId
            if (entity.EntityId >= _nextEntityId)
            {
                _nextEntityId = entity.EntityId + 1;
            }
        }
        
        #endregion
        
        #region 实体删除
        
        /// <summary>
        /// 移除实体
        /// </summary>
        public bool RemoveEntity(int entityId)
        {
            if (!_entities.TryGetValue(entityId, out var entity))
            {
                return false;
            }
            
            // 从索引中移除
            RemoveFromTileIndex(entity);
            RemoveFromChunkIndex(entity);
            
            // 从主字典移除
            _entities.Remove(entityId);
            _dirtyEntities.Remove(entityId);
            
            return true;
        }
        
        /// <summary>
        /// 移除指定位置的所有实体
        /// </summary>
        public int RemoveEntitiesAt(TileCoord position)
        {
            if (!_entitiesByTile.TryGetValue(position, out var entityIds))
            {
                return 0;
            }
            
            // 复制列表，因为会在遍历中修改
            var toRemove = new List<int>(entityIds);
            
            foreach (var id in toRemove)
            {
                RemoveEntity(id);
            }
            
            return toRemove.Count;
        }
        
        /// <summary>
        /// 清空所有实体
        /// </summary>
        public void Clear()
        {
            _entities.Clear();
            _entitiesByTile.Clear();
            _entitiesByChunk.Clear();
            _dirtyEntities.Clear();
            _nextEntityId = MapConstants.ENTITY_ID_START;
        }
        
        #endregion
        
        #region 索引管理
        
        /// <summary>
        /// 添加到 Tile 索引
        /// </summary>
        private void AddToTileIndex(MapEntity entity)
        {
            var pos = entity.TilePosition;
            
            if (!_entitiesByTile.TryGetValue(pos, out var list))
            {
                list = new List<int>();
                _entitiesByTile[pos] = list;
            }
            
            if (!list.Contains(entity.EntityId))
            {
                list.Add(entity.EntityId);
            }
        }
        
        /// <summary>
        /// 从 Tile 索引移除
        /// </summary>
        private void RemoveFromTileIndex(MapEntity entity)
        {
            var pos = entity.TilePosition;
            
            if (_entitiesByTile.TryGetValue(pos, out var list))
            {
                list.Remove(entity.EntityId);
                
                if (list.Count == 0)
                {
                    _entitiesByTile.Remove(pos);
                }
            }
        }
        
        /// <summary>
        /// 添加到 Chunk 索引
        /// </summary>
        private void AddToChunkIndex(MapEntity entity)
        {
            var chunkCoord = entity.ChunkCoord;
            
            if (!_entitiesByChunk.TryGetValue(chunkCoord, out var set))
            {
                set = new HashSet<int>();
                _entitiesByChunk[chunkCoord] = set;
            }
            
            set.Add(entity.EntityId);
        }
        
        /// <summary>
        /// 从 Chunk 索引移除
        /// </summary>
        private void RemoveFromChunkIndex(MapEntity entity)
        {
            var chunkCoord = entity.ChunkCoord;
            
            if (_entitiesByChunk.TryGetValue(chunkCoord, out var set))
            {
                set.Remove(entity.EntityId);
                
                if (set.Count == 0)
                {
                    _entitiesByChunk.Remove(chunkCoord);
                }
            }
        }
        
        /// <summary>
        /// 更新实体位置索引（实体移动后调用）
        /// </summary>
        public void UpdateEntityPosition(MapEntity entity, TileCoord oldPosition)
        {
            // 从旧位置的索引中移除
            if (_entitiesByTile.TryGetValue(oldPosition, out var oldList))
            {
                oldList.Remove(entity.EntityId);
                if (oldList.Count == 0)
                {
                    _entitiesByTile.Remove(oldPosition);
                }
            }
            
            // 添加到新位置的索引
            AddToTileIndex(entity);
            
            // 检查是否跨 Chunk
            ChunkCoord oldChunk = oldPosition.ToChunkCoord();
            ChunkCoord newChunk = entity.ChunkCoord;
            
            if (oldChunk != newChunk)
            {
                // 从旧 Chunk 索引移除
                if (_entitiesByChunk.TryGetValue(oldChunk, out var oldChunkSet))
                {
                    oldChunkSet.Remove(entity.EntityId);
                    if (oldChunkSet.Count == 0)
                    {
                        _entitiesByChunk.Remove(oldChunk);
                    }
                }
                
                // 添加到新 Chunk 索引
                AddToChunkIndex(entity);
            }
            
            _dirtyEntities.Add(entity.EntityId);
        }
        
        #endregion
        
        #region 查询 - 按 ID
        
        /// <summary>
        /// 通过 ID 获取实体
        /// </summary>
        public MapEntity GetEntity(int entityId)
        {
            _entities.TryGetValue(entityId, out var entity);
            return entity;
        }
        
        /// <summary>
        /// 通过 ID 获取指定类型的实体
        /// </summary>
        public T GetEntity<T>(int entityId) where T : MapEntity
        {
            if (_entities.TryGetValue(entityId, out var entity))
            {
                return entity as T;
            }
            return null;
        }
        
        public MapEntity GetNextEntity()
        {
            
        }
        
        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        public bool HasEntity(int entityId)
        {
            return _entities.ContainsKey(entityId);
        }
        
        #endregion
        
        #region 查询 - 按位置
        
        /// <summary>
        /// 获取指定位置的所有实体
        /// </summary>
        public List<MapEntity> GetEntitiesAt(TileCoord position)
        {
            var result = new List<MapEntity>();
            
            if (_entitiesByTile.TryGetValue(position, out var entityIds))
            {
                foreach (var id in entityIds)
                {
                    if (_entities.TryGetValue(id, out var entity))
                    {
                        result.Add(entity);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取指定位置的第一个实体
        /// </summary>
        public MapEntity GetFirstEntityAt(TileCoord position)
        {
            if (_entitiesByTile.TryGetValue(position, out var entityIds) && entityIds.Count > 0)
            {
                if (_entities.TryGetValue(entityIds[0], out var entity))
                {
                    return entity;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 获取指定位置的指定类型实体
        /// </summary>
        public T GetEntityAt<T>(TileCoord position) where T : MapEntity
        {
            if (_entitiesByTile.TryGetValue(position, out var entityIds))
            {
                foreach (var id in entityIds)
                {
                    if (_entities.TryGetValue(id, out var entity) && entity is T typed)
                    {
                        return typed;
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// 检查指定位置是否有实体
        /// </summary>
        public bool HasEntityAt(TileCoord position)
        {
            return _entitiesByTile.ContainsKey(position) && _entitiesByTile[position].Count > 0;
        }
        
        /// <summary>
        /// 检查指定位置是否有阻挡实体
        /// </summary>
        public bool HasBlockingEntityAt(TileCoord position)
        {
            if (_entitiesByTile.TryGetValue(position, out var entityIds))
            {
                foreach (var id in entityIds)
                {
                    if (_entities.TryGetValue(id, out var entity) && entity.IsBlocking)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        #endregion
        
        #region 查询 - 按 Chunk
        
        /// <summary>
        /// 获取指定 Chunk 内的所有实体
        /// </summary>
        public List<MapEntity> GetEntitiesInChunk(ChunkCoord chunkCoord)
        {
            var result = new List<MapEntity>();
            
            if (_entitiesByChunk.TryGetValue(chunkCoord, out var entityIds))
            {
                foreach (var id in entityIds)
                {
                    if (_entities.TryGetValue(id, out var entity))
                    {
                        result.Add(entity);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取指定 Chunk 内的实体数量
        /// </summary>
        public int GetEntityCountInChunk(ChunkCoord chunkCoord)
        {
            if (_entitiesByChunk.TryGetValue(chunkCoord, out var entityIds))
            {
                return entityIds.Count;
            }
            return 0;
        }
        
        #endregion
        
        #region 查询 - 按类型
        
        /// <summary>
        /// 获取指定类型的所有实体
        /// </summary>
        public List<T> GetEntitiesByType<T>() where T : MapEntity
        {
            var result = new List<T>();
            
            foreach (var entity in _entities.Values)
            {
                if (entity is T typed)
                {
                    result.Add(typed);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取指定 EntityType 的所有实体
        /// </summary>
        public List<MapEntity> GetEntitiesByType(EntityType type)
        {
            var result = new List<MapEntity>();
            
            foreach (var entity in _entities.Values)
            {
                if (entity.EntityType == type)
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region 查询 - 范围
        
        /// <summary>
        /// 获取矩形范围内的所有实体
        /// </summary>
        public List<MapEntity> GetEntitiesInRect(TileCoord min, TileCoord max)
        {
            var result = new List<MapEntity>();
            
            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    var pos = new TileCoord(x, y);
                    if (_entitiesByTile.TryGetValue(pos, out var entityIds))
                    {
                        foreach (var id in entityIds)
                        {
                            if (_entities.TryGetValue(id, out var entity))
                            {
                                result.Add(entity);
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取圆形范围内的所有实体
        /// </summary>
        public List<MapEntity> GetEntitiesInRadius(TileCoord center, int radius)
        {
            var result = new List<MapEntity>();
            int radiusSqr = radius * radius;
            
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    var pos = new TileCoord(x, y);
                    
                    // 检查是否在圆形范围内
                    if (pos.SqrDistance(center) > radiusSqr) continue;
                    
                    if (_entitiesByTile.TryGetValue(pos, out var entityIds))
                    {
                        foreach (var id in entityIds)
                        {
                            if (_entities.TryGetValue(id, out var entity))
                            {
                                result.Add(entity);
                            }
                        }
                    }
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region 脏数据管理
        
        /// <summary>
        /// 标记实体为脏
        /// </summary>
        public void MarkDirty(int entityId)
        {
            if (_entities.ContainsKey(entityId))
            {
                _dirtyEntities.Add(entityId);
            }
        }
        
        /// <summary>
        /// 获取所有脏实体
        /// </summary>
        public List<MapEntity> GetDirtyEntities()
        {
            var result = new List<MapEntity>();
            
            foreach (var id in _dirtyEntities)
            {
                if (_entities.TryGetValue(id, out var entity))
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 清除所有脏标记
        /// </summary>
        public void ClearAllDirty()
        {
            _dirtyEntities.Clear();
            
            foreach (var entity in _entities.Values)
            {
                entity.ClearDirty();
            }
        }
        
        #endregion
        
        #region 更新
        
        /// <summary>
        /// 更新所有实体（每帧调用）
        /// </summary>
        public void Update(float deltaTime)
        {
            // 更新门动画
            foreach (var entity in _entities.Values)
            {
                if (entity is DoorEntity door && door.IsAnimating)
                {
                    door.UpdateAnimation(deltaTime);
                }
            }
        }
        
        #endregion
    }
}
