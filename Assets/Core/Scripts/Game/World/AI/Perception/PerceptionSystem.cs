/**
 * PerceptionSystem.cs
 * AI 感知系统
 * 
 * 负责：
 * - 视觉感知（视野范围、视野角度、遮挡检测）
 * - 听觉感知（声音检测）
 * - 记忆系统（记住看到/听到的目标）
 */

using System;
using System.Collections.Generic;
using GDFramework.MapSystem.Pathfinding;
using UnityEngine;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// 感知系统
    /// </summary>
    public class PerceptionSystem
    {
        #region 配置
        
        /// <summary>
        /// 视野范围
        /// </summary>
        public float VisionRange = 10f;
        
        /// <summary>
        /// 视野角度（度）
        /// </summary>
        public float VisionAngle = 120f;
        
        /// <summary>
        /// 听觉范围
        /// </summary>
        public float HearingRange = 15f;
        
        /// <summary>
        /// 记忆持续时间（秒）
        /// </summary>
        public float MemoryDuration = 10f;
        
        /// <summary>
        /// 感知更新间隔（秒）
        /// </summary>
        public float UpdateInterval = 0.2f;
        
        #endregion
        
        #region 字段
        
        private AIAgent _owner;
        private List<PerceivedEntity> _perceivedEntities;
        private List<PerceivedSound> _heardSounds;
        private float _lastUpdateTime;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 所有感知到的实体
        /// </summary>
        public IReadOnlyList<PerceivedEntity> PerceivedEntities => _perceivedEntities;
        
        /// <summary>
        /// 所有听到的声音
        /// </summary>
        public IReadOnlyList<PerceivedSound> HeardSounds => _heardSounds;
        
        /// <summary>
        /// 是否有可见的敌人
        /// </summary>
        public bool HasVisibleEnemies
        {
            get
            {
                foreach (var perceived in _perceivedEntities)
                {
                    if (perceived.IsVisible && perceived.IsEnemy)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        #endregion
        
        #region 事件
        
        public event Action<PerceivedEntity> OnEntitySeen;
        public event Action<PerceivedEntity> OnEntityLost;
        public event Action<PerceivedSound> OnSoundHeard;
        
        #endregion
        
        #region 构造函数
        
        public PerceptionSystem(AIAgent owner)
        {
            _owner = owner;
            _perceivedEntities = new List<PerceivedEntity>();
            _heardSounds = new List<PerceivedSound>();
        }
        
        #endregion
        
        #region 更新
        
        /// <summary>
        /// 更新感知
        /// </summary>
        public void Update(float deltaTime)
        {
            // 限制更新频率
            if (Time.time - _lastUpdateTime < UpdateInterval)
            {
                return;
            }
            _lastUpdateTime = Time.time;
            
            // 更新视觉感知
            UpdateVision();
            
            // 更新记忆（移除过期的）
            UpdateMemory();
            
            // 更新黑板
            UpdateBlackboard();
        }
        
        /// <summary>
        /// 更新视觉感知
        /// </summary>
        private void UpdateVision()
        {
            Vector2 ownerPos = _owner.Position;
            Vector2 forward = _owner.FacingDirection;
            if (forward == Vector2.zero) forward = Vector2.down;
            
            // 获取所有潜在目标（这里简化处理，实际应该从 EntityManager 获取）
            // TODO: 从当前地图获取附近的实体
            var nearbyEntities = GetNearbyEntities(ownerPos, VisionRange);
            
            foreach (var entity in nearbyEntities)
            {
                if (entity == _owner.Entity) continue;
                
                // 检查是否在视野内
                bool isVisible = CanSee(entity);
                
                // 查找或创建感知记录
                var perceived = FindPerceived(entity);
                
                if (isVisible)
                {
                    if (perceived == null)
                    {
                        // 新发现的实体
                        perceived = new PerceivedEntity
                        {
                            Entity = entity,
                            IsEnemy = IsEnemy(entity),
                            FirstSeenTime = Time.time
                        };
                        _perceivedEntities.Add(perceived);
                        OnEntitySeen?.Invoke(perceived);
                    }
                    
                    // 更新感知信息
                    perceived.IsVisible = true;
                    perceived.LastSeenTime = Time.time;
                    perceived.LastKnownPosition = entity.WorldPosition;
                    perceived.Distance = Vector2.Distance(ownerPos, entity.WorldPosition);
                }
                else if (perceived != null)
                {
                    perceived.IsVisible = false;
                }
            }
        }
        
        /// <summary>
        /// 更新记忆
        /// </summary>
        private void UpdateMemory()
        {
            for (int i = _perceivedEntities.Count - 1; i >= 0; i--)
            {
                var perceived = _perceivedEntities[i];
                
                // 检查实体是否还存在
                if (perceived.Entity == null || perceived.Entity.IsDestroyed)
                {
                    _perceivedEntities.RemoveAt(i);
                    continue;
                }
                
                // 检查记忆是否过期
                if (!perceived.IsVisible && 
                    Time.time - perceived.LastSeenTime > MemoryDuration)
                {
                    _perceivedEntities.RemoveAt(i);
                    OnEntityLost?.Invoke(perceived);
                }
            }
            
            // 清理过期的声音
            for (int i = _heardSounds.Count - 1; i >= 0; i--)
            {
                if (Time.time - _heardSounds[i].HeardTime > MemoryDuration)
                {
                    _heardSounds.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// 更新黑板
        /// </summary>
        private void UpdateBlackboard()
        {
            var blackboard = _owner.Blackboard;
            if (blackboard == null) return;
            
            // 更新可见敌人列表
            var visibleEnemies = new List<PerceivedEntity>();
            foreach (var perceived in _perceivedEntities)
            {
                if (perceived.IsVisible && perceived.IsEnemy)
                {
                    visibleEnemies.Add(perceived);
                }
            }
            blackboard.Set(AIBlackboard.KEY_VISIBLE_ENEMIES, visibleEnemies);
            
            // 更新威胁等级
            ThreatLevel threat = CalculateThreatLevel(visibleEnemies);
            blackboard.Set(AIBlackboard.KEY_THREAT_LEVEL, threat);
        }
        
        #endregion
        
        #region 视觉检测
        
        /// <summary>
        /// 检查是否能看到实体
        /// </summary>
        public bool CanSee(MapEntity entity)
        {
            if (entity == null || entity.IsDestroyed) return false;
            return CanSeePosition(entity.WorldPosition);
        }
        
        /// <summary>
        /// 检查是否能看到位置
        /// </summary>
        public bool CanSeePosition(Vector2 targetPos)
        {
            Vector2 ownerPos = _owner.Position;
            Vector2 forward = _owner.FacingDirection;
            if (forward == Vector2.zero) forward = Vector2.down;
            
            // 检查距离
            float distance = Vector2.Distance(ownerPos, targetPos);
            if (distance > VisionRange)
            {
                return false;
            }
            
            // 检查角度
            Vector2 toTarget = (targetPos - ownerPos).normalized;
            float angle = Vector2.Angle(forward, toTarget);
            if (angle > VisionAngle * 0.5f)
            {
                return false;
            }
            
            // 检查遮挡（射线检测）
            if (!HasLineOfSight(ownerPos, targetPos))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查视线是否被遮挡
        /// </summary>
        private bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            // 使用寻路系统的视线检测
            TileCoord fromTile = MapCoordUtility.WorldToTile(from);
            TileCoord toTile = MapCoordUtility.WorldToTile(to);
            
            return PathfindingManager.Instance?.HasLineOfSight(fromTile, toTile) ?? true;
        }
        
        #endregion
        
        #region 听觉检测
        
        /// <summary>
        /// 接收声音
        /// </summary>
        public void HearSound(Vector2 position, float volume, SoundType type, object source = null)
        {
            Vector2 ownerPos = _owner.Position;
            float distance = Vector2.Distance(ownerPos, position);
            
            // 根据音量和距离计算是否能听到
            float effectiveRange = HearingRange * volume;
            if (distance > effectiveRange)
            {
                return;
            }
            
            var sound = new PerceivedSound
            {
                Position = position,
                Type = type,
                Volume = volume,
                Distance = distance,
                HeardTime = Time.time,
                Source = source
            };
            
            _heardSounds.Add(sound);
            OnSoundHeard?.Invoke(sound);
            
            // 更新黑板
            _owner.Blackboard?.Set(AIBlackboard.KEY_HEARD_SOUNDS, _heardSounds);
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 获取最近的可见敌人
        /// </summary>
        public PerceivedEntity GetNearestVisibleEnemy()
        {
            PerceivedEntity nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var perceived in _perceivedEntities)
            {
                if (perceived.IsVisible && perceived.IsEnemy && perceived.Distance < nearestDist)
                {
                    nearest = perceived;
                    nearestDist = perceived.Distance;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 获取所有可见敌人
        /// </summary>
        public List<PerceivedEntity> GetVisibleEnemies()
        {
            var enemies = new List<PerceivedEntity>();
            
            foreach (var perceived in _perceivedEntities)
            {
                if (perceived.IsVisible && perceived.IsEnemy)
                {
                    enemies.Add(perceived);
                }
            }
            
            return enemies;
        }
        
        /// <summary>
        /// 获取最近听到的声音
        /// </summary>
        public PerceivedSound GetMostRecentSound()
        {
            if (_heardSounds.Count == 0) return null;
            return _heardSounds[_heardSounds.Count - 1];
        }
        
        /// <summary>
        /// 查找已感知的实体
        /// </summary>
        private PerceivedEntity FindPerceived(MapEntity entity)
        {
            foreach (var perceived in _perceivedEntities)
            {
                if (perceived.Entity == entity)
                {
                    return perceived;
                }
            }
            return null;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取附近的实体
        /// </summary>
        private List<MapEntity> GetNearbyEntities(Vector2 position, float range)
        {
            // TODO: 从 EntityManager 获取
            // 这里返回空列表，实际使用时需要接入 EntityManager
            return new List<MapEntity>();
        }
        
        /// <summary>
        /// 判断是否为敌人
        /// </summary>
        private bool IsEnemy(MapEntity entity)
        {
            // TODO: 根据实体类型和阵营判断
            // 这里简单实现：僵尸对玩家是敌人，玩家对僵尸是敌人
            
            var ownerType = _owner.Entity?.EntityType ?? EntityType.None;
            var targetType = entity.EntityType;
            
            // 僵尸 vs 玩家/NPC
            if (ownerType == EntityType.Zombie)
            {
                return targetType == EntityType.Player || targetType == EntityType.NPC;
            }
            
            // 玩家/NPC vs 僵尸
            if (ownerType == EntityType.Player || ownerType == EntityType.NPC)
            {
                return targetType == EntityType.Zombie;
            }
            
            return false;
        }
        
        /// <summary>
        /// 计算威胁等级
        /// </summary>
        private ThreatLevel CalculateThreatLevel(List<PerceivedEntity> visibleEnemies)
        {
            if (visibleEnemies.Count == 0)
            {
                return ThreatLevel.None;
            }
            
            // 根据敌人数量和距离计算威胁等级
            int closeEnemies = 0;
            foreach (var enemy in visibleEnemies)
            {
                if (enemy.Distance < _owner.AttackRange * 2)
                {
                    closeEnemies++;
                }
            }
            
            if (closeEnemies >= 3) return ThreatLevel.Critical;
            if (closeEnemies >= 2) return ThreatLevel.High;
            if (closeEnemies >= 1) return ThreatLevel.Medium;
            if (visibleEnemies.Count > 0) return ThreatLevel.Low;
            
            return ThreatLevel.None;
        }
        
        #endregion
        
        #region 清理
        
        /// <summary>
        /// 清除所有感知
        /// </summary>
        public void Clear()
        {
            _perceivedEntities.Clear();
            _heardSounds.Clear();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 感知到的实体
    /// </summary>
    public class PerceivedEntity
    {
        /// <summary>
        /// 实体引用
        /// </summary>
        public MapEntity Entity;
        
        /// <summary>
        /// 是否当前可见
        /// </summary>
        public bool IsVisible;
        
        /// <summary>
        /// 是否为敌人
        /// </summary>
        public bool IsEnemy;
        
        /// <summary>
        /// 最后已知位置
        /// </summary>
        public Vector2 LastKnownPosition;
        
        /// <summary>
        /// 距离
        /// </summary>
        public float Distance;
        
        /// <summary>
        /// 首次看到的时间
        /// </summary>
        public float FirstSeenTime;
        
        /// <summary>
        /// 最后看到的时间
        /// </summary>
        public float LastSeenTime;
        
        /// <summary>
        /// 自最后看到以来的时间
        /// </summary>
        public float TimeSinceLastSeen => Time.time - LastSeenTime;
    }
    
    /// <summary>
    /// 感知到的声音
    /// </summary>
    public class PerceivedSound
    {
        /// <summary>
        /// 声音位置
        /// </summary>
        public Vector2 Position;
        
        /// <summary>
        /// 声音类型
        /// </summary>
        public SoundType Type;
        
        /// <summary>
        /// 音量 (0-1)
        /// </summary>
        public float Volume;
        
        /// <summary>
        /// 距离
        /// </summary>
        public float Distance;
        
        /// <summary>
        /// 听到的时间
        /// </summary>
        public float HeardTime;
        
        /// <summary>
        /// 声源
        /// </summary>
        public object Source;
    }
    
    /// <summary>
    /// 声音类型
    /// </summary>
    public enum SoundType
    {
        Footstep,       // 脚步声
        Gunshot,        // 枪声
        Explosion,      // 爆炸
        DoorOpen,       // 开门
        DoorBreak,      // 破门
        WindowBreak,    // 打破窗户
        Scream,         // 尖叫
        Alert,          // 警报
        Vehicle,        // 载具
        Other           // 其他
    }
}
