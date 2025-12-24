using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game.World.Chunk;
using Core.Game.World.Chunk.Data;
using Core.Game.World.Entity.Data;
using Core.Game.World.Map.Data;
using Core.Game.World.Tile;
using Core.Game.World.Tile.Data;
using Core.Game.World.Tile.Data.Enums;
using GDFrameworkCore;

namespace Core.Game.World.System
{
    #region 检查结果
    
    /// <summary>
    /// 地板放置检查结果
    /// </summary>
    public struct FloorPlacementResult
    {
        public bool CanPlace;
        public string FailReason;
        public List<WallEntityData> SupportingStructures;
        
        public static FloorPlacementResult Success(List<WallEntityData> supports = null) =>
            new FloorPlacementResult 
            { 
                CanPlace = true, 
                SupportingStructures = supports ?? new List<WallEntityData>() 
            };
        
        public static FloorPlacementResult Failure(string reason) =>
            new FloorPlacementResult 
            { 
                CanPlace = false, 
                FailReason = reason 
            };
    }
    
    /// <summary>
    /// 实体放置检查结果
    /// </summary>
    public struct EntityPlacementResult
    {
        public bool CanPlace;
        public string FailReason;
        
        public static EntityPlacementResult Success() =>
            new EntityPlacementResult { CanPlace = true };
        
        public static EntityPlacementResult Failure(string reason) =>
            new EntityPlacementResult { CanPlace = false, FailReason = reason };
    }
    
    /// <summary>
    /// 承重结构移除检查结果
    /// </summary>
    public struct StructureRemovalResult
    {
        public bool CanRemove;
        
        public string FailReason;
        
        public List<TileCoord> AffectedFloors;
        
        public static StructureRemovalResult Success() =>
            new StructureRemovalResult { CanRemove = true };
        
        public static StructureRemovalResult Failure(string reason, List<TileCoord> floors) =>
            new StructureRemovalResult 
            { 
                CanRemove = false, 
                FailReason = reason, 
                AffectedFloors = floors 
            };
    }
    
    #endregion
    
    /// <summary>
    /// 世界系统
    /// 管理地图、实体、承重等核心逻辑
    /// </summary>
    public class WorldSystem : AbstractSystem
    {
        #region 数据
        
        /// <summary>
        /// 当前地图数据
        /// </summary>
        public MapData CurrentMap { get; private set; }
        
        /// <summary>
        /// 实体管理器
        /// </summary>
        public EntityManager EntityManager { get; private set; }
        
        /// <summary>
        /// 是否已加载地图
        /// </summary>
        public bool IsMapLoaded => CurrentMap != null;
        
        #endregion
        
        #region 初始化
        
        protected override void OnInit()
        {
            EntityManager = new EntityManager();
        }
        
        /// <summary>
        /// 创建新地图
        /// </summary>
        public void CreateNewMap(string mapId, string mapName, int widthInChunks, int heightInChunks)
        {
            CurrentMap = new MapData(mapId, mapName, widthInChunks, heightInChunks);
            EntityManager.Clear();
        }
        
        #endregion
        
        #region 地板放置检查
        
        /// <summary>
        /// 检查是否可以放置地板
        /// </summary>
        public FloorPlacementResult CanPlaceFloor(TileCoord position)
        {
            if (CurrentMap == null)
                return FloorPlacementResult.Failure("地图未加载");
            
            if (position.z == MapConstants.GROUND_FLOOR)
            {
                return CheckGroundBearing(position);
            }
            
            return CheckStructureBearing(position);
        }
        
        /// <summary>
        /// 检查地面层是否可以承重
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private FloorPlacementResult CheckGroundBearing(TileCoord position)
        {
            if (!CurrentMap.TryGetTile(position, out var tile))
                return FloorPlacementResult.Failure("位置超出地图范围");
            
            // 地形可承重
            if (tile.GroundCanBearWeight)
                return FloorPlacementResult.Success();
            
            // 地形不可承重，检查人工承重结构
            var supports = EntityManager.FindBearingStructuresFor(position).ToList();
            if (supports.Count > 0)
                return FloorPlacementResult.Success(supports);
            
            return FloorPlacementResult.Failure("地形无法承重，需要建造承重结构");
        }
        
        /// <summary>
        /// 检查高层是否可以进行承重
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private FloorPlacementResult CheckStructureBearing(TileCoord position)
        {
            var supports = EntityManager.FindBearingStructuresFor(position).ToList();
            
            if (supports.Count > 0)
                return FloorPlacementResult.Success(supports);
            
            return FloorPlacementResult.Failure("需要下层承重结构（墙或柱子）支撑");
        }
        
        #endregion
        
        #region 实体放置检查
        
        /// <summary>
        /// 检查是否可以放置实体
        /// </summary>
        public EntityPlacementResult CanPlaceEntity(EntityData entity, TileCoord position)
        {
            if (CurrentMap == null)
                return EntityPlacementResult.Failure("地图未加载");
            
            // 检查位置是否已被完全占用
            if (EntityManager.IsPositionFullyOccupied(position))
                return EntityPlacementResult.Failure("该位置已被占用");
            
            // 获取 Tile 数据
            if (!CurrentMap.TryGetTile(position, out var tile))
                return EntityPlacementResult.Failure("位置超出地图范围");
            
            // 检查承重等级
            if (!tile.CanBearingEntity(entity.requiredBearingType))
            {
                return EntityPlacementResult.Failure(
                    $"地面承重不足（需要 {entity.requiredBearingType}，当前 {tile.GetBearingType()}）");
            }
            
            return EntityPlacementResult.Success();
        }
        
        #endregion
        
        #region 承重结构移除检查
        
        /// <summary>
        /// 检查是否可以移除承重结构
        /// </summary>
        public StructureRemovalResult CanRemoveStructure(WallEntityData wall)
        {
            var dependentFloors = FindDependentFloors(wall);
            
            if (dependentFloors.Count == 0)
                return StructureRemovalResult.Success();
            
            // 检查这些地板是否有其他承重点支撑
            var unsupportedFloors = new List<TileCoord>();
            
            foreach (var floorPos in dependentFloors)
            {
                var otherSupports = EntityManager.FindBearingStructuresFor(floorPos)
                    .Where(w => w.entityId != wall.entityId)
                    .ToList();
                
                if (otherSupports.Count == 0)
                    unsupportedFloors.Add(floorPos);
            }
            
            if (unsupportedFloors.Count > 0)
            {
                return StructureRemovalResult.Failure(
                    $"有 {unsupportedFloors.Count} 块地板将失去支撑",
                    unsupportedFloors);
            }
            
            return StructureRemovalResult.Success();
        }
        
        /// <summary>
        /// 查找依赖指定承重结构的所有地板
        /// </summary>
        public List<TileCoord> FindDependentFloors(WallEntityData wall)
        {
            var result = new List<TileCoord>();
            int upperFloor = wall.position.z + 1;
            
            for (int dx = -wall.bearingRadius; dx <= wall.bearingRadius; dx++)
            {
                for (int dy = -wall.bearingRadius; dy <= wall.bearingRadius; dy++)
                {
                    // 跳过承重点本身
                    if (dx == 0 && dy == 0) continue;
                    
                    var checkPos = new TileCoord(
                        wall.position.x + dx,
                        wall.position.y + dy,
                        upperFloor);
                    
                    if (CurrentMap.TryGetTile(checkPos, out var tile) && tile.HasFloor)
                        result.Add(checkPos);
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region 承重状态更新
        
        /// <summary>
        /// 刷新所有承重结构的依赖状态
        /// </summary>
        public void RefreshAllBearingStatus()
        {
            foreach (var entity in EntityManager.GetAllEntities())
            {
                if (entity is WallEntityData wall)
                {
                    var floors = FindDependentFloors(wall);
                    wall.UpdateBearingStatus(floors.Count);
                }
            }
        }
        
        /// <summary>
        /// 放置地板后更新承重状态
        /// </summary>
        public void OnFloorPlaced(TileCoord floorPosition)
        {
            foreach (var wall in EntityManager.FindBearingStructuresFor(floorPosition))
            {
                wall.dependentFloorCount++;
            }
        }
        
        /// <summary>
        /// 移除地板后更新承重状态
        /// </summary>
        public void OnFloorRemoved(TileCoord floorPosition)
        {
            foreach (var wall in EntityManager.FindBearingStructuresFor(floorPosition))
            {
                wall.dependentFloorCount = Math.Max(0, wall.dependentFloorCount - 1);
            }
        }
        
        #endregion
        
        #region 便捷操作方法
        
        /// <summary>
        /// 放置地板（带承重检查）
        /// </summary>
        public bool TryPlaceFloor(TileCoord position, ushort floorId)
        {
            var result = CanPlaceFloor(position);
            if (!result.CanPlace)
                return false;
            
            CurrentMap.SetFloor(position, floorId);
            OnFloorPlaced(position);
            return true;
        }
        
        /// <summary>
        /// 放置墙壁
        /// </summary>
        public int PlaceWall(int configId, TileCoord position, int bearingRadius = 1)
        {
            var wall = new WallEntityData(configId, position, bearingRadius);
            return EntityManager.AddEntity(wall);
        }
        
        /// <summary>
        /// 移除墙壁（带承重检查）
        /// </summary>
        public bool TryRemoveWall(int wallEntityId)
        {
            var wall = EntityManager.GetEntity<WallEntityData>(wallEntityId);
            if (wall == null)
                return false;
            
            var result = CanRemoveStructure(wall);
            if (!result.CanRemove)
                return false;
            
            return EntityManager.RemoveEntity(wallEntityId);
        }
        
        /// <summary>
        /// 放置实体（带承重检查）
        /// </summary>
        public int TryPlaceEntity(EntityData entity, TileCoord position)
        {
            entity.position = position;
            
            var result = CanPlaceEntity(entity, position);
            if (!result.CanPlace)
                return -1;
            
            return EntityManager.AddEntity(entity);
        }
        
        #endregion
        
        #region 天花板/遮罩判断
        
        /// <summary>
        /// 检查是否有天花板（用于渲染遮罩剔除）
        /// </summary>
        public bool HasCeiling(TileCoord position)
        {
            if (CurrentMap == null)
                return false;
            
            return CurrentMap.HasCeiling(position);
        }
        
        /// <summary>
        /// 检查玩家是否在室内（用于判断渲染哪些楼层）
        /// </summary>
        public bool IsIndoor(TileCoord position)
        {
            return HasCeiling(position);
        }
        
        #endregion
        
        #region 保存/加载
        
        /// <summary>
        /// 保存当前地图
        /// </summary>
        public void SaveMap(string saveKey)
        {
            if (CurrentMap == null)
                return;
            
            
        }
        
        /// <summary>
        /// 加载地图
        /// </summary>
        public void LoadMap(string saveKey)
        {

        }
        
        #endregion
    }
}