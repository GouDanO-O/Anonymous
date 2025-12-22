/**
 * EntityConfig.cs
 * 动态实体配置（用于 Luban 配置表）
 * 
 * 混合系统中，EntityConfig 定义动态对象：
 * - 家具、容器、门、设备等
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 动态实体配置
    /// </summary>
    [Serializable]
    public class EntityConfig
    {
        #region 基础属性
        
        /// <summary>
        /// 实体配置ID（主键）
        /// </summary>
        public int ConfigId;
        
        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName;
        
        /// <summary>
        /// 实体类型
        /// </summary>
        public EntityType EntityType;
        
        /// <summary>
        /// 预制体路径（用于实例化 GameObject）
        /// </summary>
        public string PrefabPath;
        
        #endregion
        
        #region 精灵资源
        
        /// <summary>
        /// 图集名称
        /// </summary>
        public string SpriteAtlas;
        
        /// <summary>
        /// 精灵名称（支持多方向：North, East, South, West）
        /// </summary>
        public string[] SpriteNames;
        
        #endregion
        
        #region 游戏属性
        
        /// <summary>
        /// 默认标志
        /// </summary>
        public EntityFlags DefaultFlags;
        
        /// <summary>
        /// 最大生命值/耐久度
        /// </summary>
        public int MaxHealth;
        
        /// <summary>
        /// 占用的 Tile 尺寸（大多数为 1x1）
        /// </summary>
        public Vector2Int Size;
        
        #endregion
        
        #region 碰撞属性
        
        /// <summary>
        /// 碰撞体类型
        /// </summary>
        public ColliderType ColliderType;
        
        /// <summary>
        /// 碰撞体尺寸
        /// </summary>
        public Vector2 ColliderSize;
        
        /// <summary>
        /// 碰撞体偏移
        /// </summary>
        public Vector2 ColliderOffset;
        
        #endregion
        
        #region 容器属性（仅容器类型使用）
        
        /// <summary>
        /// 容器容量（仅 Container 类型）
        /// </summary>
        public int ContainerCapacity;
        
        /// <summary>
        /// 预设物品列表ID（用于生成初始物品）
        /// </summary>
        public int LootTableId;
        
        #endregion
        
        #region 门属性（仅门类型使用）
        
        /// <summary>
        /// 门类型
        /// </summary>
        public DoorType DoorType;
        
        /// <summary>
        /// 开门动画时长
        /// </summary>
        public float DoorAnimationDuration;
        
        #endregion
        
        #region 音效
        
        /// <summary>
        /// 交互音效
        /// </summary>
        public string InteractSoundId;
        
        /// <summary>
        /// 破坏音效
        /// </summary>
        public string DestroySoundId;
        
        #endregion
        
        #region 构造函数
        
        public EntityConfig()
        {
            Size = Vector2Int.one;
            ColliderSize = Vector2.one;
            MaxHealth = 100;
            DoorAnimationDuration = 0.3f;
            SpriteNames = Array.Empty<string>();
        }
        
        #endregion
        
        #region 工厂方法
        
        /// <summary>
        /// 创建家具配置
        /// </summary>
        public static EntityConfig CreateFurniture(int id, string name, string spriteName,
            bool blocking = true, int health = 50)
        {
            var flags = EntityFlags.Destructible;
            if (blocking) flags |= EntityFlags.Blocking;
            
            return new EntityConfig
            {
                ConfigId = id,
                EntityName = name,
                EntityType = EntityType.Furniture,
                SpriteNames = new[] { spriteName },
                DefaultFlags = flags,
                MaxHealth = health,
                ColliderType = blocking ? ColliderType.Box : ColliderType.None,
                ColliderSize = new Vector2(0.8f, 0.8f)
            };
        }
        
        /// <summary>
        /// 创建容器配置
        /// </summary>
        public static EntityConfig CreateContainer(int id, string name, string spriteName,
            int capacity = 20, int lootTableId = 0)
        {
            return new EntityConfig
            {
                ConfigId = id,
                EntityName = name,
                EntityType = EntityType.Container,
                SpriteNames = new[] { spriteName },
                DefaultFlags = EntityFlags.Blocking | EntityFlags.Interactive | EntityFlags.Destructible,
                MaxHealth = 80,
                ColliderType = ColliderType.Box,
                ColliderSize = new Vector2(0.9f, 0.9f),
                ContainerCapacity = capacity,
                LootTableId = lootTableId
            };
        }
        
        /// <summary>
        /// 创建门配置
        /// </summary>
        public static EntityConfig CreateDoor(int id, string name, string[] spriteNames,
            DoorType doorType = DoorType.Wooden, int health = 100)
        {
            return new EntityConfig
            {
                ConfigId = id,
                EntityName = name,
                EntityType = EntityType.Door,
                SpriteNames = spriteNames, // 通常包含 Open/Closed 状态
                DefaultFlags = EntityFlags.Blocking | EntityFlags.Interactive | EntityFlags.Destructible,
                MaxHealth = health,
                ColliderType = ColliderType.Box,
                ColliderSize = new Vector2(1f, 0.3f),
                DoorType = doorType,
                DoorAnimationDuration = 0.3f
            };
        }
        
        /// <summary>
        /// 创建掉落物配置
        /// </summary>
        public static EntityConfig CreateDroppedItem(int id, string name, string spriteName)
        {
            return new EntityConfig
            {
                ConfigId = id,
                EntityName = name,
                EntityType = EntityType.DroppedItem,
                SpriteNames = new[] { spriteName },
                DefaultFlags = EntityFlags.Pickupable,
                MaxHealth = 0, // 不可破坏
                ColliderType = ColliderType.None
            };
        }
        
        #endregion
        
        #region 属性
        
        public bool IsBlocking => (DefaultFlags & EntityFlags.Blocking) != 0;
        public bool IsInteractive => (DefaultFlags & EntityFlags.Interactive) != 0;
        public bool IsDestructible => (DefaultFlags & EntityFlags.Destructible) != 0;
        public bool IsContainer => EntityType == EntityType.Container;
        public bool IsDoor => EntityType == EntityType.Door;
        
        #endregion
        
        /// <summary>
        /// 创建实体实例
        /// </summary>
        public MapEntity CreateEntity(int entityId, string mapId, TileCoord position)
        {
            MapEntity entity;
            
            switch (EntityType)
            {
                case EntityType.Container:
                    entity = new ContainerEntity(entityId, ConfigId, mapId, position, ContainerCapacity);
                    break;
                    
                case EntityType.Door:
                    entity = new DoorEntity(entityId, ConfigId, mapId, position, DoorType)
                        .WithAnimationDuration(DoorAnimationDuration);
                    break;
                    
                default:
                    entity = new MapEntity(entityId, ConfigId, EntityType, mapId, position);
                    break;
            }
            
            entity.WithFlags(DefaultFlags)
                  .WithHealth(MaxHealth, MaxHealth);
            
            return entity;
        }
        
        public override string ToString()
        {
            return $"EntityConfig({ConfigId}: {EntityName}, {EntityType})";
        }
    }
}
