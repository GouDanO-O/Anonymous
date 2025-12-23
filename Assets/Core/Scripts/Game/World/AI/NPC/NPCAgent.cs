/**
 * NPCAgent.cs
 * NPC AI 代理
 * 
 * 行为模式：
 * - Idle: 站立/工作
 * - Patrol: 巡逻路线
 * - Work: 执行工作任务
 * - Talk: 与玩家对话
 * - Follow: 跟随玩家
 * - Flee: 遇到危险逃跑
 * - Hide: 躲藏
 * - Fight: 战斗（如果有武器）
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.AI.NPC
{
    /// <summary>
    /// NPC AI 代理
    /// </summary>
    public class NPCAgent : AIAgent
    {
        #region 序列化字段
        
        [Header("NPC Settings")]
        [SerializeField]
        private NPCType _npcType = NPCType.Civilian;
        
        [SerializeField]
        private string _npcName = "NPC";
        
        [SerializeField]
        private NPCPersonality _personality = NPCPersonality.Neutral;
        
        [Header("Behavior")]
        [SerializeField]
        private float _fleeHealthThreshold = 0.3f;
        
        [SerializeField]
        private bool _canFight = false;
        
        [SerializeField]
        private bool _canFollow = true;
        
        [Header("Schedule")]
        [SerializeField]
        private List<TileCoord> _patrolPoints;
        
        [SerializeField]
        private TileCoord _homePosition;
        
        [SerializeField]
        private TileCoord _workPosition;
        
        #endregion
        
        #region 运行时状态
        
        private AIAgent _followTarget;
        private bool _isFollowing;
        private float _fearLevel;
        private float _morale = 1f;
        
        #endregion
        
        #region 属性
        
        public NPCType NPCType => _npcType;
        public string NPCName => _npcName;
        public NPCPersonality Personality => _personality;
        public bool CanFight => _canFight;
        public bool CanFollow => _canFollow;
        public bool IsFollowing => _isFollowing;
        public float FearLevel => _fearLevel;
        public float Morale => _morale;
        public TileCoord HomePosition => _homePosition;
        public TileCoord WorkPosition => _workPosition;
        
        #endregion
        
        #region 初始化
        
        protected override void Awake()
        {
            base.Awake();
            
            // 根据 NPC 类型调整属性
            ApplyNPCTypeModifiers();
        }
        
        private void ApplyNPCTypeModifiers()
        {
            switch (_npcType)
            {
                case NPCType.Guard:
                    _canFight = true;
                    _attackDamage *= 1.5f;
                    _visionRange *= 1.3f;
                    break;
                    
                case NPCType.Doctor:
                    _canFight = false;
                    _moveSpeed *= 0.9f;
                    break;
                    
                case NPCType.Trader:
                    _canFight = false;
                    break;
                    
                case NPCType.Survivor:
                    _canFight = true;
                    break;
            }
            
            // 根据性格调整
            switch (_personality)
            {
                case NPCPersonality.Brave:
                    _fleeHealthThreshold = 0.15f;
                    break;
                    
                case NPCPersonality.Coward:
                    _fleeHealthThreshold = 0.5f;
                    break;
                    
                case NPCPersonality.Aggressive:
                    _attackDamage *= 1.2f;
                    break;
            }
        }
        
        protected override void SetupStateMachine()
        {
            // 添加状态
            _stateMachine.AddState(new NPCIdleState());
            _stateMachine.AddState(new NPCPatrolState());
            _stateMachine.AddState(new NPCWorkState());
            _stateMachine.AddState(new NPCFollowState());
            _stateMachine.AddState(new NPCFleeState());
            _stateMachine.AddState(new NPCHideState());
            _stateMachine.AddState(new NPCFightState());
            _stateMachine.AddState(new DeadState());
            
            // 设置初始状态
            _stateMachine.SetInitialState<NPCIdleState>();
            
            // 设置巡逻点
            Blackboard.Set(AIBlackboard.KEY_PATROL_POINTS, _patrolPoints);
            Blackboard.Set(AIBlackboard.KEY_HOME_POSITION, _homePosition);
            
            // 监听感知事件
            _perception.OnEntitySeen += OnEntitySeen;
            _perception.OnSoundHeard += OnSoundHeard;
        }
        
        #endregion
        
        #region 感知事件
        
        private void OnEntitySeen(PerceivedEntity perceived)
        {
            if (perceived.IsEnemy)
            {
                // 增加恐惧
                _fearLevel = Mathf.Min(1f, _fearLevel + 0.3f);
                
                // 根据性格决定行为
                if (_canFight && _personality == NPCPersonality.Aggressive)
                {
                    SetTarget(perceived.Entity);
                }
            }
        }
        
        private void OnSoundHeard(PerceivedSound sound)
        {
            // 听到危险声音增加恐惧
            if (sound.Type == SoundType.Gunshot || 
                sound.Type == SoundType.Explosion ||
                sound.Type == SoundType.Scream)
            {
                _fearLevel = Mathf.Min(1f, _fearLevel + sound.Volume * 0.5f);
            }
        }
        
        #endregion
        
        #region NPC 特殊行为
        
        /// <summary>
        /// 开始跟随
        /// </summary>
        public void StartFollow(AIAgent target)
        {
            if (!_canFollow || target == null) return;
            
            _followTarget = target;
            _isFollowing = true;
            Blackboard.Set("FollowTarget", target);
            
            _stateMachine.ChangeState<NPCFollowState>();
        }
        
        /// <summary>
        /// 停止跟随
        /// </summary>
        public void StopFollow()
        {
            _followTarget = null;
            _isFollowing = false;
            Blackboard.Remove("FollowTarget");
        }
        
        /// <summary>
        /// 回家
        /// </summary>
        public void GoHome()
        {
            StopFollow();
            MoveTo(_homePosition);
        }
        
        /// <summary>
        /// 去工作
        /// </summary>
        public void GoToWork()
        {
            StopFollow();
            MoveTo(_workPosition);
            _stateMachine.ChangeState<NPCWorkState>();
        }
        
        /// <summary>
        /// 检查是否应该逃跑
        /// </summary>
        public bool ShouldFlee()
        {
            // 生命值低
            if (_entity != null && 
                (float)_entity.Health / _entity.MaxHealth < _fleeHealthThreshold)
            {
                return true;
            }
            
            // 恐惧过高
            if (_fearLevel > 0.7f && !_canFight)
            {
                return true;
            }
            
            // 不能战斗且有威胁
            if (!_canFight && Blackboard.Has(AIBlackboard.KEY_VISIBLE_ENEMIES))
            {
                var enemies = Blackboard.Get<List<PerceivedEntity>>(AIBlackboard.KEY_VISIBLE_ENEMIES);
                if (enemies != null && enemies.Count > 0)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取逃跑方向
        /// </summary>
        public Vector2 GetFleeDirection()
        {
            var enemies = Blackboard.Get<List<PerceivedEntity>>(AIBlackboard.KEY_VISIBLE_ENEMIES);
            if (enemies == null || enemies.Count == 0)
            {
                // 随机方向
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
            
            // 计算平均威胁方向的反方向
            Vector2 threatDir = Vector2.zero;
            foreach (var enemy in enemies)
            {
                threatDir += (enemy.LastKnownPosition - Position).normalized;
            }
            
            return -threatDir.normalized;
        }
        
        /// <summary>
        /// 恢复恐惧
        /// </summary>
        public void RecoverFear(float deltaTime)
        {
            // 没有威胁时逐渐恢复
            var threat = Blackboard.Get<ThreatLevel>(AIBlackboard.KEY_THREAT_LEVEL);
            if (threat == ThreatLevel.None)
            {
                _fearLevel = Mathf.Max(0f, _fearLevel - deltaTime * 0.1f);
            }
        }
        
        #endregion
        
        #region 重写方法
        
        protected override void Update()
        {
            base.Update();
            RecoverFear(Time.deltaTime);
        }
        
        public override void TakeDamage(float damage, AIAgent attacker = null)
        {
            base.TakeDamage(damage, attacker);
            
            // 受伤增加恐惧
            _fearLevel = Mathf.Min(1f, _fearLevel + 0.2f);
            
            // 可能触发尖叫
            if (_fearLevel > 0.5f && UnityEngine.Random.value < 0.3f)
            {
                SoundSystem.EmitSound(Position, 0.6f, SoundType.Scream, this);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// NPC 类型
    /// </summary>
    public enum NPCType
    {
        Civilian,   // 平民
        Guard,      // 守卫
        Doctor,     // 医生
        Trader,     // 商人
        Survivor    // 幸存者
    }
    
    /// <summary>
    /// NPC 性格
    /// </summary>
    public enum NPCPersonality
    {
        Neutral,    // 中立
        Brave,      // 勇敢
        Coward,     // 胆小
        Aggressive, // 好斗
        Friendly,   // 友善
        Hostile     // 敌对
    }
    
    #region NPC 状态
    
    /// <summary>
    /// NPC 空闲状态
    /// </summary>
    public class NPCIdleState : AIState
    {
        public override string Name => "NPCIdle";
        
        private float _idleTime;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _idleTime = 0f;
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _idleTime += deltaTime;
        }
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return null;
            
            // 应该逃跑
            if (npc.ShouldFlee())
            {
                return typeof(NPCFleeState);
            }
            
            // 正在跟随
            if (npc.IsFollowing)
            {
                return typeof(NPCFollowState);
            }
            
            // 有目标且可战斗
            if (npc.CanFight && Blackboard.HasValidTarget())
            {
                return typeof(NPCFightState);
            }
            
            // 空闲一段时间后开始巡逻
            if (_idleTime > 5f)
            {
                var patrolPoints = Blackboard.Get<List<TileCoord>>(AIBlackboard.KEY_PATROL_POINTS);
                if (patrolPoints != null && patrolPoints.Count > 0)
                {
                    return typeof(NPCPatrolState);
                }
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// NPC 巡逻状态
    /// </summary>
    public class NPCPatrolState : AIState
    {
        public override string Name => "NPCPatrol";
        
        private List<TileCoord> _waypoints;
        private int _currentIndex;
        private float _waitTime;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            _waypoints = Blackboard.Get<List<TileCoord>>(AIBlackboard.KEY_PATROL_POINTS);
            _currentIndex = 0;
            _waitTime = 0f;
            
            if (_waypoints != null && _waypoints.Count > 0)
            {
                Agent.MoveTo(_waypoints[_currentIndex]);
            }
        }
        
        public override void OnUpdate(float deltaTime)
        {
            if (_waypoints == null || _waypoints.Count == 0) return;
            
            if (!Agent.IsMoving)
            {
                _waitTime += deltaTime;
                
                if (_waitTime > 2f)
                {
                    _waitTime = 0f;
                    _currentIndex = (_currentIndex + 1) % _waypoints.Count;
                    Agent.MoveTo(_waypoints[_currentIndex]);
                }
            }
        }
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return typeof(NPCIdleState);
            
            if (npc.ShouldFlee())
            {
                return typeof(NPCFleeState);
            }
            
            if (npc.IsFollowing)
            {
                return typeof(NPCFollowState);
            }
            
            if (npc.CanFight && Blackboard.HasValidTarget())
            {
                return typeof(NPCFightState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// NPC 工作状态
    /// </summary>
    public class NPCWorkState : AIState
    {
        public override string Name => "NPCWork";
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return typeof(NPCIdleState);
            
            if (npc.ShouldFlee())
            {
                return typeof(NPCFleeState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// NPC 跟随状态
    /// </summary>
    public class NPCFollowState : AIState
    {
        public override string Name => "NPCFollow";
        
        private float _followDistance = 2f;
        private float _pathUpdateTime;
        
        public override void OnUpdate(float deltaTime)
        {
            var npc = Agent as NPCAgent;
            if (npc == null || !npc.IsFollowing) return;
            
            if (!Blackboard.TryGet<AIAgent>("FollowTarget", out var target)) return;
            if (target == null) return;
            
            float distance = Vector2.Distance(npc.Position, target.Position);
            
            if (distance > _followDistance)
            {
                _pathUpdateTime += deltaTime;
                if (_pathUpdateTime > 0.5f)
                {
                    _pathUpdateTime = 0f;
                    npc.MoveTo(target.Position);
                }
            }
            else
            {
                npc.StopMoving();
            }
        }
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return typeof(NPCIdleState);
            
            if (npc.ShouldFlee())
            {
                return typeof(NPCFleeState);
            }
            
            if (!npc.IsFollowing)
            {
                return typeof(NPCIdleState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// NPC 逃跑状态
    /// </summary>
    public class NPCFleeState : AIState
    {
        public override string Name => "NPCFlee";
        
        private float _fleeTime;
        private float _maxFleeTime = 10f;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _fleeTime = 0f;
            
            var npc = Agent as NPCAgent;
            if (npc != null)
            {
                // 向逃跑方向移动
                Vector2 fleeDir = npc.GetFleeDirection();
                Vector2 fleeTarget = npc.Position + fleeDir * 10f;
                npc.MoveTo(fleeTarget);
            }
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _fleeTime += deltaTime;
            
            var npc = Agent as NPCAgent;
            if (npc == null) return;
            
            // 如果停止移动，继续逃跑
            if (!npc.IsMoving && _fleeTime < _maxFleeTime)
            {
                Vector2 fleeDir = npc.GetFleeDirection();
                Vector2 fleeTarget = npc.Position + fleeDir * 10f;
                npc.MoveTo(fleeTarget);
            }
        }
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return typeof(NPCIdleState);
            
            // 不再需要逃跑
            if (!npc.ShouldFlee() && _fleeTime > 3f)
            {
                return typeof(NPCIdleState);
            }
            
            // 逃跑超时
            if (_fleeTime > _maxFleeTime)
            {
                return typeof(NPCHideState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// NPC 躲藏状态
    /// </summary>
    public class NPCHideState : AIState
    {
        public override string Name => "NPCHide";
        
        private float _hideTime;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _hideTime = 0f;
            Agent.StopMoving();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _hideTime += deltaTime;
        }
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return typeof(NPCIdleState);
            
            // 威胁消除后出来
            var threat = Blackboard.Get<ThreatLevel>(AIBlackboard.KEY_THREAT_LEVEL);
            if (threat == ThreatLevel.None && _hideTime > 5f)
            {
                return typeof(NPCIdleState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// NPC 战斗状态
    /// </summary>
    public class NPCFightState : AIState
    {
        public override string Name => "NPCFight";
        
        private float _lastPathTime;
        
        public override void OnUpdate(float deltaTime)
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return;
            
            var target = npc.GetTarget();
            if (target == null || target.IsDestroyed) return;
            
            // 在攻击范围内攻击
            if (npc.IsInAttackRange(target.WorldPosition))
            {
                npc.StopMoving();
                npc.LookAt(target.WorldPosition);
                
                if (npc.CanAttack())
                {
                    npc.Attack(target);
                }
            }
            else
            {
                // 追逐目标
                _lastPathTime += deltaTime;
                if (_lastPathTime > 0.5f)
                {
                    _lastPathTime = 0f;
                    npc.MoveTo(target.WorldPosition);
                }
            }
        }
        
        public override Type CheckTransition()
        {
            var npc = Agent as NPCAgent;
            if (npc == null) return typeof(NPCIdleState);
            
            // 应该逃跑
            if (npc.ShouldFlee())
            {
                return typeof(NPCFleeState);
            }
            
            // 目标消失
            if (!Blackboard.HasValidTarget())
            {
                return typeof(NPCIdleState);
            }
            
            return null;
        }
    }
    
    #endregion
}
