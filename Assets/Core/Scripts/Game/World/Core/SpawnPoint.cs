/**
 * SpawnPoint.cs
 * 出生点/重生点数据结构
 * 
 * SpawnPoint 定义玩家或敌人的出生位置
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 出生点类型
    /// </summary>
    public enum SpawnPointType : byte
    {
        /// <summary>
        /// 玩家出生点
        /// </summary>
        Player = 0,
        
        /// <summary>
        /// 敌人出生点
        /// </summary>
        Enemy = 1,
        
        /// <summary>
        /// NPC 出生点
        /// </summary>
        NPC = 2,
        
        /// <summary>
        /// 物品刷新点
        /// </summary>
        Item = 3
    }
    
    /// <summary>
    /// 出生点数据
    /// </summary>
    [Serializable]
    public class SpawnPoint
    {
        #region 字段
        
        /// <summary>
        /// 出生点唯一标识
        /// </summary>
        [SerializeField]
        private string _spawnPointId;
        
        /// <summary>
        /// 出生点名称
        /// </summary>
        [SerializeField]
        private string _spawnPointName;
        
        /// <summary>
        /// 出生点类型
        /// </summary>
        [SerializeField]
        private SpawnPointType _spawnType;
        
        /// <summary>
        /// 位置（Tile 坐标）
        /// </summary>
        [SerializeField]
        private TileCoord _position;
        
        /// <summary>
        /// 初始朝向
        /// </summary>
        [SerializeField]
        private Direction _facing;
        
        /// <summary>
        /// 是否为默认出生点
        /// </summary>
        [SerializeField]
        private bool _isDefault;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        [SerializeField]
        private bool _isEnabled;
        
        /// <summary>
        /// 关联的实体预制体ID（用于敌人/NPC出生点）
        /// </summary>
        [SerializeField]
        private string _entityPrefabId;
        
        /// <summary>
        /// 最大同时存在数量（用于敌人出生点）
        /// </summary>
        [SerializeField]
        private int _maxSpawnCount;
        
        /// <summary>
        /// 重生间隔（秒）
        /// </summary>
        [SerializeField]
        private float _respawnInterval;
        
        /// <summary>
        /// 出生区域半径（Tile 数量）
        /// 0 表示精确位置，大于0表示在范围内随机
        /// </summary>
        [SerializeField]
        private int _spawnRadius;
        
        #endregion
        
        #region 属性
        
        public string SpawnPointId => _spawnPointId;
        public string SpawnPointName => _spawnPointName;
        public SpawnPointType SpawnType => _spawnType;
        public TileCoord Position => _position;
        public Direction Facing => _facing;
        public bool IsDefault => _isDefault;
        public bool IsEnabled => _isEnabled;
        public string EntityPrefabId => _entityPrefabId;
        public int MaxSpawnCount => _maxSpawnCount;
        public float RespawnInterval => _respawnInterval;
        public int SpawnRadius => _spawnRadius;
        
        /// <summary>
        /// 世界坐标位置
        /// </summary>
        public Vector2 WorldPosition => _position.ToWorldPosition() + 
            new Vector2(MapConstants.TILE_SIZE * 0.5f, MapConstants.TILE_SIZE * 0.5f);
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数（序列化需要）
        /// </summary>
        public SpawnPoint()
        {
            _facing = Direction.South;
            _isEnabled = true;
            _maxSpawnCount = 1;
            _respawnInterval = 0;
            _spawnRadius = 0;
        }
        
        /// <summary>
        /// 创建玩家出生点
        /// </summary>
        public static SpawnPoint CreatePlayerSpawn(string id, TileCoord position, 
            Direction facing = Direction.South, bool isDefault = false)
        {
            return new SpawnPoint
            {
                _spawnPointId = id,
                _spawnPointName = $"Player Spawn {id}",
                _spawnType = SpawnPointType.Player,
                _position = position,
                _facing = facing,
                _isDefault = isDefault,
                _isEnabled = true,
                _maxSpawnCount = 1
            };
        }
        
        /// <summary>
        /// 创建敌人出生点
        /// </summary>
        public static SpawnPoint CreateEnemySpawn(string id, TileCoord position, 
            string entityPrefabId, int maxCount = 1, float respawnInterval = 60f)
        {
            return new SpawnPoint
            {
                _spawnPointId = id,
                _spawnPointName = $"Enemy Spawn {id}",
                _spawnType = SpawnPointType.Enemy,
                _position = position,
                _entityPrefabId = entityPrefabId,
                _maxSpawnCount = maxCount,
                _respawnInterval = respawnInterval,
                _isEnabled = true
            };
        }
        
        /// <summary>
        /// 创建NPC出生点
        /// </summary>
        public static SpawnPoint CreateNPCSpawn(string id, TileCoord position,
            string entityPrefabId, Direction facing = Direction.South)
        {
            return new SpawnPoint
            {
                _spawnPointId = id,
                _spawnPointName = $"NPC Spawn {id}",
                _spawnType = SpawnPointType.NPC,
                _position = position,
                _facing = facing,
                _entityPrefabId = entityPrefabId,
                _maxSpawnCount = 1,
                _isEnabled = true
            };
        }
        
        #endregion
        
        #region Builder 模式
        
        /// <summary>
        /// 设置出生区域半径
        /// </summary>
        public SpawnPoint WithRadius(int radius)
        {
            _spawnRadius = Mathf.Max(0, radius);
            return this;
        }
        
        /// <summary>
        /// 设置朝向
        /// </summary>
        public SpawnPoint WithFacing(Direction facing)
        {
            _facing = facing;
            return this;
        }
        
        /// <summary>
        /// 设置为默认出生点
        /// </summary>
        public SpawnPoint AsDefault()
        {
            _isDefault = true;
            return this;
        }
        
        /// <summary>
        /// 设置名称
        /// </summary>
        public SpawnPoint WithName(string name)
        {
            _spawnPointName = name;
            return this;
        }
        
        /// <summary>
        /// 设置重生参数
        /// </summary>
        public SpawnPoint WithRespawn(int maxCount, float interval)
        {
            _maxSpawnCount = maxCount;
            _respawnInterval = interval;
            return this;
        }
        
        #endregion
        
        #region 状态操作
        
        /// <summary>
        /// 启用出生点
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
        }
        
        /// <summary>
        /// 禁用出生点
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
        }
        
        /// <summary>
        /// 设置为默认出生点
        /// </summary>
        public void SetAsDefault(bool isDefault)
        {
            _isDefault = isDefault;
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 获取一个实际出生位置（考虑出生半径）
        /// </summary>
        public TileCoord GetSpawnPosition()
        {
            if (_spawnRadius <= 0)
            {
                return _position;
            }
            
            // 在半径范围内随机
            int offsetX = UnityEngine.Random.Range(-_spawnRadius, _spawnRadius + 1);
            int offsetY = UnityEngine.Random.Range(-_spawnRadius, _spawnRadius + 1);
            
            return new TileCoord(_position.x + offsetX, _position.y + offsetY);
        }
        
        /// <summary>
        /// 检查指定位置是否在出生区域内
        /// </summary>
        public bool IsInSpawnArea(TileCoord coord)
        {
            if (_spawnRadius <= 0)
            {
                return coord == _position;
            }
            
            return coord.ManhattanDistance(_position) <= _spawnRadius;
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"SpawnPoint({_spawnPointId}, Type:{_spawnType}, Pos:{_position}, " +
                   $"Default:{_isDefault}, Enabled:{_isEnabled})";
        }
    }
}
