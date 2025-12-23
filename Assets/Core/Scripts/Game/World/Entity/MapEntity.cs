/**
 * MapEntity.cs
 * 地图实体基础类
 * 
 * Entity 系统负责管理所有动态对象：
 * - 家具（可推动、可破坏）
 * - 容器（有库存）
 * - 门（有开关状态）
 * - 设备（有功能状态）
 * - 掉落物
 * 等等
 * 
 * 与 Tile 系统的区别：
 * - Tile: 静态、不移动、简单状态
 * - Entity: 动态、可能移动、复杂状态和行为
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 地图实体数据
    /// 这是一个纯数据类，不继承 MonoBehaviour
    /// </summary>
    [Serializable]
    public class MapEntity
    {
        #region 标识信息
        
        /// <summary>
        /// 实体唯一ID（运行时分配）
        /// </summary>
        [SerializeField]
        private int _entityId;
        
        /// <summary>
        /// 实体配置ID（对应配置表）
        /// </summary>
        [SerializeField]
        private int _configId;
        
        /// <summary>
        /// 实体类型
        /// </summary>
        [SerializeField]
        private EntityType _entityType;
        
        /// <summary>
        /// 所属地图ID
        /// </summary>
        [SerializeField]
        private string _mapId;
        
        #endregion
        
        #region 位置与方向
        
        /// <summary>
        /// 所在的 Tile 坐标
        /// </summary>
        [SerializeField]
        private TileCoord _tilePosition;
        
        /// <summary>
        /// 精确位置偏移（相对于 Tile 中心，用于平滑移动）
        /// </summary>
        [SerializeField]
        private Vector2 _positionOffset;
        
        /// <summary>
        /// 朝向/旋转
        /// </summary>
        [SerializeField]
        private Rotation _rotation;
        
        #endregion
        
        #region 状态
        
        /// <summary>
        /// 实体标志
        /// </summary>
        [SerializeField]
        private EntityFlags _flags;
        
        /// <summary>
        /// 当前生命值/耐久度
        /// </summary>
        [SerializeField]
        private int _health;
        
        /// <summary>
        /// 最大生命值/耐久度
        /// </summary>
        [SerializeField]
        private int _maxHealth;
        
        #endregion
        
        #region 运行时状态（不序列化）
        
        /// <summary>
        /// 是否需要更新
        /// </summary>
        [NonSerialized]
        private bool _isDirty;
        
        /// <summary>
        /// 所在的 Chunk 坐标（缓存，用于快速查找）
        /// </summary>
        [NonSerialized]
        private ChunkCoord _cachedChunkCoord;
        
        /// <summary>
        /// 关联的 GameObject（如果已实例化）
        /// </summary>
        [NonSerialized]
        private GameObject _gameObject;
        
        #endregion
        
        #region 属性
        
        public int EntityId => _entityId;
        public int ConfigId => _configId;
        public EntityType EntityType => _entityType;
        public string MapId => _mapId;
        
        public TileCoord TilePosition => _tilePosition;
        public Vector2 PositionOffset => _positionOffset;
        public Vector2 Offset => _positionOffset;  // 别名
        public Rotation Rotation => _rotation;
        
        public EntityFlags Flags => _flags;
        public int Health => _health;
        public int MaxHealth => _maxHealth;
        
        public bool IsDirty => _isDirty;
        public ChunkCoord ChunkCoord => _cachedChunkCoord;
        public GameObject GameObject => _gameObject;
        
        /// <summary>
        /// 获取世界坐标位置
        /// </summary>
        public Vector2 WorldPosition
        {
            get
            {
                Vector2 tileCenter = MapCoordUtility.TileToWorldCenter(_tilePosition);
                return tileCenter + _positionOffset;
            }
        }
        
        /// <summary>
        /// 生命值百分比 (0-1)
        /// </summary>
        public float HealthPercent => _maxHealth > 0 ? (float)_health / _maxHealth : 1f;
        
        /// <summary>
        /// 是否存活/未被摧毁
        /// </summary>
        public bool IsAlive => !HasFlag(EntityFlags.IsDestroyed) && _health > 0;
        
        #endregion
        
        #region 标志位快捷属性
        
        public bool IsBlocking => HasFlag(EntityFlags.Blocking);
        public bool IsInteractive => HasFlag(EntityFlags.Interactive);
        public bool IsPickupable => HasFlag(EntityFlags.Pickupable);
        public bool IsPushable => HasFlag(EntityFlags.Pushable);
        public bool IsDestructible => HasFlag(EntityFlags.Destructible);
        public bool RequiresPower => HasFlag(EntityFlags.RequiresPower);
        public bool IsOpen => HasFlag(EntityFlags.IsOpen);
        public bool IsLocked => HasFlag(EntityFlags.IsLocked);
        public bool IsPowered => HasFlag(EntityFlags.IsPowered);
        public bool IsDamaged => HasFlag(EntityFlags.IsDamaged);
        public bool IsDestroyed => HasFlag(EntityFlags.IsDestroyed);
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数（序列化需要）
        /// </summary>
        public MapEntity()
        {
            _entityId = MapConstants.INVALID_ENTITY_ID;
        }
        
        /// <summary>
        /// 创建实体
        /// </summary>
        public MapEntity(int entityId, int configId, EntityType entityType, 
            string mapId, TileCoord position)
        {
            _entityId = entityId;
            _configId = configId;
            _entityType = entityType;
            _mapId = mapId;
            _tilePosition = position;
            _positionOffset = Vector2.zero;
            _rotation = Rotation.None;
            _flags = EntityFlags.None;
            _health = 100;
            _maxHealth = 100;
            
            UpdateCachedChunkCoord();
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 设置生命值
        /// </summary>
        public MapEntity WithHealth(int current, int max)
        {
            _health = current;
            _maxHealth = max;
            return this;
        }
        
        /// <summary>
        /// 设置标志
        /// </summary>
        public MapEntity WithFlags(EntityFlags flags)
        {
            _flags = flags;
            return this;
        }
        
        /// <summary>
        /// 设置旋转
        /// </summary>
        public MapEntity WithRotation(Rotation rotation)
        {
            _rotation = rotation;
            return this;
        }
        
        #endregion
        
        #region 标志位操作
        
        /// <summary>
        /// 检查是否有指定标志
        /// </summary>
        public bool HasFlag(EntityFlags flag)
        {
            return (_flags & flag) == flag;
        }
        
        /// <summary>
        /// 设置标志位
        /// </summary>
        public void SetFlag(EntityFlags flag, bool value)
        {
            if (value)
            {
                _flags |= flag;
            }
            else
            {
                _flags &= ~flag;
            }
            MarkDirty();
        }
        
        /// <summary>
        /// 添加标志
        /// </summary>
        public void AddFlag(EntityFlags flag)
        {
            _flags |= flag;
            MarkDirty();
        }
        
        /// <summary>
        /// 移除标志
        /// </summary>
        public void RemoveFlag(EntityFlags flag)
        {
            _flags &= ~flag;
            MarkDirty();
        }
        
        /// <summary>
        /// 设置所有标志（替换现有标志）
        /// </summary>
        public void SetFlags(EntityFlags flags)
        {
            _flags = flags;
            MarkDirty();
        }
        
        /// <summary>
        /// 设置生命值
        /// </summary>
        public void SetHealth(int health, int maxHealth)
        {
            _health = health;
            _maxHealth = maxHealth;
            MarkDirty();
        }
        
        #endregion
        
        #region 位置操作
        
        /// <summary>
        /// 设置 Tile 位置
        /// </summary>
        public void SetTilePosition(TileCoord newPosition)
        {
            if (_tilePosition != newPosition)
            {
                _tilePosition = newPosition;
                _positionOffset = Vector2.zero;
                UpdateCachedChunkCoord();
                MarkDirty();
            }
        }
        
        /// <summary>
        /// 设置位置（SetTilePosition 的别名）
        /// </summary>
        public void SetPosition(TileCoord newPosition)
        {
            SetTilePosition(newPosition);
        }
        
        /// <summary>
        /// 设置精确位置偏移
        /// </summary>
        public void SetPositionOffset(Vector2 offset)
        {
            _positionOffset = offset;
            MarkDirty();
        }
        
        /// <summary>
        /// 设置偏移（SetPositionOffset 的别名）
        /// </summary>
        public void SetOffset(Vector2 offset)
        {
            SetPositionOffset(offset);
        }
        
        /// <summary>
        /// 设置世界坐标位置
        /// </summary>
        public void SetWorldPosition(Vector2 worldPos)
        {
            TileCoord newTile = MapCoordUtility.WorldToTile(worldPos);
            Vector2 tileCenter = MapCoordUtility.TileToWorldCenter(newTile);
            
            _tilePosition = newTile;
            _positionOffset = worldPos - tileCenter;
            UpdateCachedChunkCoord();
            MarkDirty();
        }
        
        /// <summary>
        /// 设置旋转
        /// </summary>
        public void SetRotation(Rotation rotation)
        {
            if (_rotation != rotation)
            {
                _rotation = rotation;
                MarkDirty();
            }
        }
        
        /// <summary>
        /// 顺时针旋转 90 度
        /// </summary>
        public void RotateClockwise()
        {
            _rotation = (Rotation)(((int)_rotation + 1) % 4);
            MarkDirty();
        }
        
        /// <summary>
        /// 更新缓存的 Chunk 坐标
        /// </summary>
        private void UpdateCachedChunkCoord()
        {
            _cachedChunkCoord = _tilePosition.ToChunkCoord();
        }
        
        #endregion
        
        #region 状态操作
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!IsDestructible || damage <= 0) return;
            
            _health = Mathf.Max(0, _health - damage);
            
            if (_health < _maxHealth)
            {
                AddFlag(EntityFlags.IsDamaged);
            }
            
            if (_health <= 0)
            {
                AddFlag(EntityFlags.IsDestroyed);
            }
            
            MarkDirty();
        }
        
        /// <summary>
        /// 修复
        /// </summary>
        public void Repair(int amount)
        {
            if (amount <= 0) return;
            
            _health = Mathf.Min(_maxHealth, _health + amount);
            
            if (_health >= _maxHealth)
            {
                RemoveFlag(EntityFlags.IsDamaged);
            }
            
            MarkDirty();
        }
        
        /// <summary>
        /// 完全修复
        /// </summary>
        public void FullRepair()
        {
            _health = _maxHealth;
            RemoveFlag(EntityFlags.IsDamaged);
            MarkDirty();
        }
        
        /// <summary>
        /// 打开（门、容器等）
        /// </summary>
        public bool TryOpen()
        {
            if (IsLocked) return false;
            
            AddFlag(EntityFlags.IsOpen);
            
            // 打开时通常不再阻挡
            if (_entityType == EntityType.Door)
            {
                RemoveFlag(EntityFlags.Blocking);
            }
            
            return true;
        }
        
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            RemoveFlag(EntityFlags.IsOpen);
            
            // 关闭时恢复阻挡
            if (_entityType == EntityType.Door)
            {
                AddFlag(EntityFlags.Blocking);
            }
        }
        
        /// <summary>
        /// 上锁
        /// </summary>
        public void Lock()
        {
            AddFlag(EntityFlags.IsLocked);
            Close(); // 上锁时自动关闭
        }
        
        /// <summary>
        /// 解锁
        /// </summary>
        public void Unlock()
        {
            RemoveFlag(EntityFlags.IsLocked);
        }
        
        #endregion
        
        #region GameObject 关联
        
        /// <summary>
        /// 绑定 GameObject
        /// </summary>
        public void BindGameObject(GameObject go)
        {
            _gameObject = go;
        }
        
        /// <summary>
        /// 解除 GameObject 绑定
        /// </summary>
        public void UnbindGameObject()
        {
            _gameObject = null;
        }
        
        /// <summary>
        /// 同步位置到 GameObject
        /// </summary>
        public void SyncToGameObject()
        {
            if (_gameObject != null)
            {
                _gameObject.transform.position = new Vector3(
                    WorldPosition.x, 
                    WorldPosition.y, 
                    0
                );
                
                // 同步旋转
                float angle = (int)_rotation * 90f;
                _gameObject.transform.rotation = Quaternion.Euler(0, 0, -angle);
            }
        }
        
        #endregion
        
        #region 脏标记
        
        /// <summary>
        /// 标记为已修改
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// 清除脏标记
        /// </summary>
        public void ClearDirty()
        {
            _isDirty = false;
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Entity({_entityId}, Type:{_entityType}, Pos:{_tilePosition}, " +
                   $"HP:{_health}/{_maxHealth}, Flags:{_flags})";
        }
    }
}
