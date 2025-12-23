/**
 * SaveData.cs
 * 存档数据结构
 * 
 * 设计原则：
 * - 差异化保存：只保存修改过的 Tile
 * - 完整保存 Entity 状态
 * - 支持 Easy Save 2 序列化
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Saving
{
    #region Tile 存档数据
    
    /// <summary>
    /// 单个 Tile 层的存档数据
    /// </summary>
    [Serializable]
    public struct TileLayerSaveData
    {
        public ushort tileId;
        public byte spriteAndFlags;
        public byte damage;
        
        public TileLayerSaveData(TileLayerData data)
        {
            tileId = data.tileId;
            spriteAndFlags = data.spriteAndFlags;
            damage = data.damage;
        }
        
        public TileLayerData ToLayerData()
        {
            return new TileLayerData(tileId, spriteAndFlags, damage);
        }
    }
    
    /// <summary>
    /// 单个 Tile 的存档数据
    /// </summary>
    [Serializable]
    public struct TileSaveData
    {
        public int x;
        public int y;
        public TileLayerSaveData[] layers;
        
        public TileSaveData(int x, int y, TileData data)
        {
            this.x = x;
            this.y = y;
            this.layers = new TileLayerSaveData[MapConstants.TILE_LAYER_COUNT];
            
            for (int i = 0; i < MapConstants.TILE_LAYER_COUNT; i++)
            {
                this.layers[i] = new TileLayerSaveData(data.GetLayer(i));
            }
        }
        
        public TileData ToTileData()
        {
            TileData data = TileData.Empty;
            
            for (int i = 0; i < layers.Length && i < MapConstants.TILE_LAYER_COUNT; i++)
            {
                data = data.WithLayer(i, layers[i].ToLayerData());
            }
            
            return data;
        }
    }
    
    /// <summary>
    /// Chunk 存档数据（差异化）
    /// </summary>
    [Serializable]
    public class ChunkSaveData
    {
        public int chunkX;
        public int chunkY;
        
        /// <summary>
        /// 修改过的 Tile 列表（差异化保存）
        /// </summary>
        public List<TileSaveData> modifiedTiles;
        
        public ChunkSaveData()
        {
            modifiedTiles = new List<TileSaveData>();
        }
        
        public ChunkSaveData(int x, int y)
        {
            chunkX = x;
            chunkY = y;
            modifiedTiles = new List<TileSaveData>();
        }
    }
    
    #endregion
    
    #region Entity 存档数据
    
    /// <summary>
    /// 基础实体存档数据
    /// </summary>
    [Serializable]
    public class EntitySaveData
    {
        public int entityId;
        public int configId;
        public int entityType;
        public string mapId;
        
        // 位置
        public int tileX;
        public int tileY;
        public float offsetX;
        public float offsetY;
        public int rotation;
        
        // 状态
        public int flags;
        public int health;
        public int maxHealth;
        
        // 扩展数据（子类使用）
        public string extraData;
        
        public EntitySaveData() { }
        
        public EntitySaveData(MapEntity entity)
        {
            entityId = entity.EntityId;
            configId = entity.ConfigId;
            entityType = (int)entity.EntityType;
            mapId = entity.MapId;
            
            tileX = entity.TilePosition.x;
            tileY = entity.TilePosition.y;
            offsetX = entity.Offset.x;
            offsetY = entity.Offset.y;
            rotation = (int)entity.Rotation;
            
            flags = (int)entity.Flags;
            health = entity.Health;
            maxHealth = entity.MaxHealth;
        }
        
        public virtual void ApplyTo(MapEntity entity)
        {
            entity.SetPosition(new TileCoord(tileX, tileY));
            entity.SetOffset(new Vector2(offsetX, offsetY));
            entity.SetRotation((Rotation)rotation);
            entity.SetFlags((EntityFlags)flags);
            entity.SetHealth(health, maxHealth);
        }
    }
    
    /// <summary>
    /// 容器实体存档数据
    /// </summary>
    [Serializable]
    public class ContainerSaveData : EntitySaveData
    {
        /// <summary>
        /// 库存槽位
        /// </summary>
        public List<ContainerSlotData> slots;
        
        /// <summary>
        /// 是否已被搜索
        /// </summary>
        public bool isSearched;
        
        public ContainerSaveData() 
        {
            slots = new List<ContainerSlotData>();
        }
        
        public ContainerSaveData(ContainerEntity entity) : base(entity)
        {
            slots = new List<ContainerSlotData>();
            isSearched = entity.IsSearched;
            
            for (int i = 0; i < entity.Capacity; i++)
            {
                var slot = entity.GetSlot(i);
                if (slot.itemId != 0)
                {
                    slots.Add(new ContainerSlotData
                    {
                        slotIndex = i,
                        itemId = slot.itemId,
                        count = slot.count,
                        condition = slot.condition
                    });
                }
            }
        }
        
        public void ApplyTo(ContainerEntity entity)
        {
            base.ApplyTo(entity);
            
            // 清空现有物品
            entity.ClearAll();
            
            // 恢复物品
            foreach (var slot in slots)
            {
                entity.SetSlot(slot.slotIndex, slot.itemId, slot.count, slot.condition);
            }
            
            // 恢复搜索状态
            if (isSearched)
            {
                entity.MarkSearched();
            }
        }
    }
    
    /// <summary>
    /// 容器槽位数据
    /// </summary>
    [Serializable]
    public struct ContainerSlotData
    {
        public int slotIndex;
        public int itemId;
        public int count;
        public float condition;
    }
    
    /// <summary>
    /// 门实体存档数据
    /// </summary>
    [Serializable]
    public class DoorSaveData : EntitySaveData
    {
        public int doorType;
        public bool isLocked;
        public string requiredKeyId;
        public float openProgress;
        
        public DoorSaveData() { }
        
        public DoorSaveData(DoorEntity entity) : base(entity)
        {
            doorType = (int)entity.DoorType;
            isLocked = entity.IsLocked;
            requiredKeyId = entity.RequiredKeyId;
            openProgress = entity.OpenProgress;
        }
        
        public void ApplyTo(DoorEntity entity)
        {
            base.ApplyTo(entity);
            
            // 恢复门状态
            if (!string.IsNullOrEmpty(requiredKeyId))
            {
                entity.ChangeLock(requiredKeyId);
            }
            
            if (isLocked)
            {
                entity.Lock();
            }
            
            // 恢复开关状态
            if (openProgress > 0.5f)
            {
                entity.ForceOpen();
            }
            else
            {
                entity.ForceClose();
            }
        }
    }
    
    #endregion
    
    #region 地图存档数据
    
    /// <summary>
    /// 完整地图存档数据
    /// </summary>
    [Serializable]
    public class MapSaveData
    {
        #region 元数据
        
        /// <summary>
        /// 存档版本
        /// </summary>
        public int saveVersion;
        
        /// <summary>
        /// 保存时间（Unix 时间戳）
        /// </summary>
        public long saveTimestamp;
        
        /// <summary>
        /// 地图 ID
        /// </summary>
        public string mapId;
        
        /// <summary>
        /// 地图名称
        /// </summary>
        public string mapName;
        
        /// <summary>
        /// 地图尺寸
        /// </summary>
        public int widthInChunks;
        public int heightInChunks;
        
        /// <summary>
        /// 地图类型
        /// </summary>
        public int mapType;
        
        #endregion
        
        #region Tile 数据
        
        /// <summary>
        /// 修改过的 Chunk 数据
        /// </summary>
        public List<ChunkSaveData> modifiedChunks;
        
        #endregion
        
        #region Entity 数据
        
        /// <summary>
        /// 下一个实体 ID
        /// </summary>
        public int nextEntityId;
        
        /// <summary>
        /// 普通实体
        /// </summary>
        public List<EntitySaveData> entities;
        
        /// <summary>
        /// 容器实体
        /// </summary>
        public List<ContainerSaveData> containers;
        
        /// <summary>
        /// 门实体
        /// </summary>
        public List<DoorSaveData> doors;
        
        #endregion
        
        #region 构造函数
        
        public MapSaveData()
        {
            saveVersion = MapSaveSystem.SAVE_VERSION;
            modifiedChunks = new List<ChunkSaveData>();
            entities = new List<EntitySaveData>();
            containers = new List<ContainerSaveData>();
            doors = new List<DoorSaveData>();
        }
        
        #endregion
    }
    
    #endregion
    
    #region 游戏存档数据
    
    /// <summary>
    /// 游戏整体存档数据（包含所有地图）
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 存档版本
        /// </summary>
        public int saveVersion;
        
        /// <summary>
        /// 保存时间
        /// </summary>
        public long saveTimestamp;
        
        /// <summary>
        /// 存档名称
        /// </summary>
        public string saveName;
        
        /// <summary>
        /// 当前所在地图 ID
        /// </summary>
        public string currentMapId;
        
        /// <summary>
        /// 游戏时间（秒）
        /// </summary>
        public float playTime;
        
        /// <summary>
        /// 所有地图的存档数据（按 MapId 索引）
        /// </summary>
        public Dictionary<string, MapSaveData> maps;
        
        public GameSaveData()
        {
            saveVersion = MapSaveSystem.SAVE_VERSION;
            maps = new Dictionary<string, MapSaveData>();
        }
    }
    
    #endregion
}
