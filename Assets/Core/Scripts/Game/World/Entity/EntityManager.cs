using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game.World.Entity.Data.Enums;
using Core.Game.World.Tile;

namespace Core.Game.World.Entity.Data
{
    /// <summary>
    /// 实体管理器
    /// </summary>
    public class EntityManager
    {
        #region 字段
        
        /// <summary>
        /// 所有实体（按 ID 索引）
        /// </summary>
        private Dictionary<int, EntityData> entitiesById;
        
        /// <summary>
        /// 空间索引：按 Tile 坐标查找实体
        /// </summary>
        private Dictionary<TileCoord, List<int>> spatialIndex;
        
        /// <summary>
        /// 承重结构索引：按楼层分类
        /// </summary>
        private Dictionary<int, List<WallEntityData>> bearingStructuresByFloor;
        
        /// <summary>
        /// 下一个可用的实体 ID
        /// </summary>
        private int nextEntityId;
        
        /// <summary>
        /// 是否有未保存的修改
        /// </summary>
        public bool IsDirty { get; private set; }
        
        /// <summary>
        /// 实体总数
        /// </summary>
        public int EntityCount => entitiesById.Count;
        
        #endregion
        
        #region 构造函数
        
        public EntityManager()
        {
            entitiesById = new Dictionary<int, EntityData>();
            spatialIndex = new Dictionary<TileCoord, List<int>>();
            bearingStructuresByFloor = new Dictionary<int, List<WallEntityData>>();
            nextEntityId = 1;
        }
        
        #endregion
        
        #region 实体生命周期
        
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <returns>实体 ID</returns>
        public int AddEntity(EntityData entity)
        {
            entity.entityId = nextEntityId++;
            entitiesById[entity.entityId] = entity;
            
            // 添加到空间索引
            AddToSpatialIndex(entity);
            
            // 如果是承重结构，添加到承重索引
            if (entity is WallEntityData wall)
            {
                AddToBearingIndex(wall);
            }
            
            IsDirty = true;
            return entity.entityId;
        }
        
        /// <summary>
        /// 移除实体
        /// </summary>
        public bool RemoveEntity(int entityId)
        {
            if (!entitiesById.TryGetValue(entityId, out var entity))
                return false;
            
            // 检查是否可以移除
            if (!entity.CanBeRemoved())
                return false;
            
            // 从空间索引移除
            RemoveFromSpatialIndex(entity);
            
            // 如果是承重结构，从承重索引移除
            if (entity is WallEntityData wall)
            {
                RemoveFromBearingIndex(wall);
            }
            
            entitiesById.Remove(entityId);
            IsDirty = true;
            return true;
        }
        
        /// <summary>
        /// 移动实体到新位置
        /// </summary>
        public bool MoveEntity(int entityId, TileCoord newPosition)
        {
            if (!entitiesById.TryGetValue(entityId, out var entity))
                return false;
            
            if (!entity.IsPlaceable)
                return false;
            
            // 从旧位置索引移除
            RemoveFromSpatialIndex(entity);
            
            if (entity is WallEntityData wall1)
                RemoveFromBearingIndex(wall1);
            
            // 更新位置
            entity.position = newPosition;
            
            // 添加到新位置索引
            AddToSpatialIndex(entity);
            
            if (entity is WallEntityData wall2)
                AddToBearingIndex(wall2);
            
            IsDirty = true;
            return true;
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 按 ID 获取实体
        /// </summary>
        public EntityData GetEntity(int entityId)
        {
            entitiesById.TryGetValue(entityId, out var entity);
            return entity;
        }
        
        /// <summary>
        /// 按 ID 获取指定类型的实体
        /// </summary>
        public T GetEntity<T>(int entityId) where T : EntityData
        {
            if (entitiesById.TryGetValue(entityId, out var entity))
                return entity as T;
            return null;
        }
        
        /// <summary>
        /// 获取指定位置的所有实体
        /// </summary>
        public IEnumerable<EntityData> GetEntitiesAt(TileCoord position)
        {
            if (spatialIndex.TryGetValue(position, out var entityIds))
            {
                foreach (var id in entityIds)
                {
                    if (entitiesById.TryGetValue(id, out var entity))
                        yield return entity;
                }
            }
        }
        
        /// <summary>
        /// 获取指定位置的指定类型实体
        /// </summary>
        public IEnumerable<T> GetEntitiesAt<T>(TileCoord position) where T : EntityData
        {
            return GetEntitiesAt(position).OfType<T>();
        }
        
        /// <summary>
        /// 检查指定位置是否有实体
        /// </summary>
        public bool HasEntityAt(TileCoord position)
        {
            return spatialIndex.ContainsKey(position) && spatialIndex[position].Count > 0;
        }
        
        /// <summary>
        /// 检查指定位置是否无法再放置其他实体
        /// </summary>
        public bool IsPositionFullyOccupied(TileCoord position)
        {
            return GetEntitiesAt(position).Any(e => e.CantPlaceOther);
        }
        
        /// <summary>
        /// 检查指定位置是否阻挡移动
        /// </summary>
        public bool IsPositionBlocked(TileCoord position)
        {
            return GetEntitiesAt(position).Any(e => e.BlocksMovement);
        }
        
        /// <summary>
        /// 获取指定类别的所有实体
        /// </summary>
        public IEnumerable<EntityData> GetEntitiesByCategory(EEntityCategory category)
        {
            return entitiesById.Values.Where(e => e.category == category);
        }
        
        /// <summary>
        /// 获取所有实体
        /// </summary>
        public IEnumerable<EntityData> GetAllEntities()
        {
            return entitiesById.Values;
        }
        
        #endregion
        
        #region 承重相关
        
        /// <summary>
        /// 获取指定楼层的所有承重结构
        /// </summary>
        public IEnumerable<WallEntityData> GetBearingStructuresOnFloor(int floor)
        {
            if (bearingStructuresByFloor.TryGetValue(floor, out var list))
                return list;
            return Enumerable.Empty<WallEntityData>();
        }
        
        /// <summary>
        /// 查找能够支撑指定位置的承重结构
        /// </summary>
        public IEnumerable<WallEntityData> FindBearingStructuresFor(TileCoord floorPosition)
        {
            // 承重结构在下一层
            int structureFloor = floorPosition.z - 1;
            
            foreach (var wall in GetBearingStructuresOnFloor(structureFloor))
            {
                if (wall.IsInBearingRange(floorPosition))
                    yield return wall;
            }
        }
        
        /// <summary>
        /// 检查指定位置是否有足够的承重支撑
        /// </summary>
        public bool HasBearingSupport(TileCoord floorPosition)
        {
            return FindBearingStructuresFor(floorPosition).Any();
        }
        
        #endregion
        
        #region 索引管理
        
        private void AddToSpatialIndex(EntityData entity)
        {
            if (!spatialIndex.TryGetValue(entity.position, out var list))
            {
                list = new List<int>();
                spatialIndex[entity.position] = list;
            }
            list.Add(entity.entityId);
        }
        
        private void RemoveFromSpatialIndex(EntityData entity)
        {
            if (spatialIndex.TryGetValue(entity.position, out var list))
            {
                list.Remove(entity.entityId);
                if (list.Count == 0)
                    spatialIndex.Remove(entity.position);
            }
        }
        
        private void AddToBearingIndex(WallEntityData wall)
        {
            int floor = wall.position.z;
            if (!bearingStructuresByFloor.TryGetValue(floor, out var list))
            {
                list = new List<WallEntityData>();
                bearingStructuresByFloor[floor] = list;
            }
            list.Add(wall);
        }
        
        private void RemoveFromBearingIndex(WallEntityData wall)
        {
            int floor = wall.position.z;
            if (bearingStructuresByFloor.TryGetValue(floor, out var list))
            {
                list.Remove(wall);
                if (list.Count == 0)
                    bearingStructuresByFloor.Remove(floor);
            }
        }
        
        #endregion
        
        #region 状态管理
        
        public void ClearDirty() => IsDirty = false;
        
        public void Clear()
        {
            entitiesById.Clear();
            spatialIndex.Clear();
            bearingStructuresByFloor.Clear();
            nextEntityId = 1;
            IsDirty = true;
        }
        
        #endregion
    }
}