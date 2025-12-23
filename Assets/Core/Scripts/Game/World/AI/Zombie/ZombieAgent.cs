/**
 * ZombieAgent.cs
 * 僵尸 AI 代理
 * 
 * 行为模式：
 * - Idle: 站立不动，偶尔转向
 * - Wander: 缓慢随机移动
 * - Investigate: 听到声音后前往调查
 * - Chase: 发现目标后追逐
 * - Attack: 接近目标后攻击
 * - Feed: 击杀目标后进食
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.AI.Zombie
{
    /// <summary>
    /// 僵尸 AI 代理
    /// </summary>
    public class ZombieAgent : AIAgent
    {
        #region 序列化字段
        
        [Header("Zombie Settings")]
        [SerializeField]
        private ZombieType _zombieType = ZombieType.Walker;
        
        [SerializeField]
        private float _wanderSpeed = 1f;
        
        [SerializeField]
        private float _chaseSpeed = 3f;
        
        [SerializeField]
        private float _investigateTime = 10f;
        
        [SerializeField]
        private float _feedingTime = 5f;
        
        [SerializeField]
        private float _loseTargetTime = 10f;
        
        [Header("Zombie Perception")]
        [SerializeField]
        private float _alertRange = 5f; // 警戒范围（不需要视线）
        
        [SerializeField]
        private float _soundAttractionMultiplier = 1.5f;
        
        #endregion
        
        #region 属性
        
        public ZombieType ZombieType => _zombieType;
        public float WanderSpeed => _wanderSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float InvestigateTime => _investigateTime;
        public float FeedingTime => _feedingTime;
        public float LoseTargetTime => _loseTargetTime;
        public float AlertRange => _alertRange;
        
        #endregion
        
        #region 初始化
        
        protected override void Awake()
        {
            base.Awake();
            
            // 根据僵尸类型调整属性
            ApplyZombieTypeModifiers();
        }
        
        /// <summary>
        /// 应用僵尸类型修正
        /// </summary>
        private void ApplyZombieTypeModifiers()
        {
            switch (_zombieType)
            {
                case ZombieType.Crawler:
                    _wanderSpeed *= 0.5f;
                    _chaseSpeed *= 0.7f;
                    _visionRange *= 0.5f;
                    _attackDamage *= 0.7f;
                    break;
                    
                case ZombieType.Runner:
                    _wanderSpeed *= 1.5f;
                    _chaseSpeed *= 2f;
                    _visionRange *= 1.2f;
                    break;
                    
                case ZombieType.Brute:
                    _wanderSpeed *= 0.8f;
                    _chaseSpeed *= 0.9f;
                    _attackDamage *= 2f;
                    _attackRange *= 1.3f;
                    break;
                    
                case ZombieType.Screamer:
                    _attackDamage *= 0.5f;
                    _hearingRange *= 2f;
                    break;
            }
        }
        
        protected override void SetupStateMachine()
        {
            // 添加状态
            _stateMachine.AddState(new ZombieIdleState());
            _stateMachine.AddState(new ZombieWanderState());
            _stateMachine.AddState(new ZombieInvestigateState());
            _stateMachine.AddState(new ZombieChaseState());
            _stateMachine.AddState(new ZombieAttackState());
            _stateMachine.AddState(new ZombieFeedState());
            _stateMachine.AddState(new DeadState());
            
            // 设置初始状态
            _stateMachine.SetInitialState<ZombieIdleState>();
            
            // 监听感知事件
            _perception.OnEntitySeen += OnEntitySeen;
            _perception.OnSoundHeard += OnSoundHeard;
        }
        
        #endregion
        
        #region 感知事件处理
        
        private void OnEntitySeen(PerceivedEntity perceived)
        {
            if (perceived.IsEnemy)
            {
                // 发现敌人，设置目标
                SetTarget(perceived.Entity);
                
                // 如果是 Screamer，发出尖叫吸引其他僵尸
                if (_zombieType == ZombieType.Screamer)
                {
                    EmitScream();
                }
            }
        }
        
        private void OnSoundHeard(PerceivedSound sound)
        {
            // 如果没有目标，前往调查
            if (!Blackboard.HasValidTarget())
            {
                Blackboard.Set("InvestigatePosition", sound.Position);
                Blackboard.Set("InvestigateTime", Time.time);
            }
        }
        
        #endregion
        
        #region 僵尸特殊行为
        
        /// <summary>
        /// 尖叫（Screamer 特殊能力）
        /// </summary>
        public void EmitScream()
        {
            if (_zombieType != ZombieType.Screamer) return;
            
            // 发出声音吸引其他僵尸
            SoundSystem.EmitSound(Position, 1f, SoundType.Scream, this);
            
            Debug.Log($"[ZombieAgent] {name} screams!");
        }
        
        /// <summary>
        /// 检查是否在警戒范围内（不需要视线）
        /// </summary>
        public bool IsInAlertRange(Vector2 targetPos)
        {
            return Vector2.Distance(Position, targetPos) <= _alertRange;
        }
        
        /// <summary>
        /// 获取随机游荡目标
        /// </summary>
        public TileCoord GetRandomWanderTarget()
        {
            // 在附近随机选择一个可行走的位置
            int range = 5;
            TileCoord current = TilePosition;
            
            for (int attempts = 0; attempts < 10; attempts++)
            {
                int dx = UnityEngine.Random.Range(-range, range + 1);
                int dy = UnityEngine.Random.Range(-range, range + 1);
                
                TileCoord target = new TileCoord(current.x + dx, current.y + dy);
                
                if (Pathfinding.PathfindingManager.Instance?.IsWalkable(target) ?? false)
                {
                    return target;
                }
            }
            
            return current;
        }
        
        #endregion
        
        #region 重写方法
        
        public override void Attack(MapEntity target)
        {
            base.Attack(target);
            
            // 播放攻击动画/音效
            SoundSystem.EmitSound(Position, 0.5f, SoundType.Other, this);
        }
        
        protected override void Die()
        {
            base.Die();
            
            // 僵尸死亡特殊处理
            // 例如：掉落物品、播放死亡动画等
        }
        
        #endregion
    }
    
    /// <summary>
    /// 僵尸类型
    /// </summary>
    public enum ZombieType
    {
        /// <summary>普通行走僵尸</summary>
        Walker,
        
        /// <summary>爬行僵尸（腿部受伤）</summary>
        Crawler,
        
        /// <summary>奔跑僵尸（新鲜僵尸）</summary>
        Runner,
        
        /// <summary>重型僵尸（高伤害、高血量）</summary>
        Brute,
        
        /// <summary>尖叫僵尸（会呼叫其他僵尸）</summary>
        Screamer
    }
    
    #region 僵尸状态
    
    /// <summary>
    /// 僵尸空闲状态
    /// </summary>
    public class ZombieIdleState : AIState
    {
        public override string Name => "ZombieIdle";
        
        private float _idleTime;
        private float _maxIdleTime;
        private float _turnTimer;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _idleTime = 0f;
            _maxIdleTime = UnityEngine.Random.Range(3f, 8f);
            _turnTimer = 0f;
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _idleTime += deltaTime;
            _turnTimer += deltaTime;
            
            // 偶尔转向
            if (_turnTimer >= 2f)
            {
                _turnTimer = 0f;
                if (UnityEngine.Random.value < 0.3f)
                {
                    // 随机转向
                    float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    // Agent._facingDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
            }
        }
        
        public override Type CheckTransition()
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return null;
            
            // 发现敌人 -> 追逐
            if (Blackboard.HasValidTarget())
            {
                return typeof(ZombieChaseState);
            }
            
            // 听到声音 -> 调查
            if (Blackboard.Has("InvestigatePosition"))
            {
                return typeof(ZombieInvestigateState);
            }
            
            // 空闲时间结束 -> 游荡
            if (_idleTime >= _maxIdleTime)
            {
                return typeof(ZombieWanderState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// 僵尸游荡状态
    /// </summary>
    public class ZombieWanderState : AIState
    {
        public override string Name => "ZombieWander";
        
        private float _wanderTime;
        private float _maxWanderTime;
        private bool _hasTarget;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _wanderTime = 0f;
            _maxWanderTime = UnityEngine.Random.Range(5f, 15f);
            
            // 设置游荡目标
            var zombie = Agent as ZombieAgent;
            if (zombie != null)
            {
                var target = zombie.GetRandomWanderTarget();
                zombie.MoveTo(target);
                _hasTarget = true;
            }
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _wanderTime += deltaTime;
            
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return;
            
            // 如果到达目标或卡住，选择新目标
            if (!zombie.IsMoving && _hasTarget)
            {
                if (_wanderTime < _maxWanderTime)
                {
                    var target = zombie.GetRandomWanderTarget();
                    zombie.MoveTo(target);
                }
            }
        }
        
        public override void OnExit()
        {
            Agent.StopMoving();
        }
        
        public override Type CheckTransition()
        {
            // 发现敌人 -> 追逐
            if (Blackboard.HasValidTarget())
            {
                return typeof(ZombieChaseState);
            }
            
            // 听到声音 -> 调查
            if (Blackboard.Has("InvestigatePosition"))
            {
                return typeof(ZombieInvestigateState);
            }
            
            // 游荡时间结束 -> 空闲
            if (_wanderTime >= _maxWanderTime)
            {
                return typeof(ZombieIdleState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// 僵尸调查状态
    /// </summary>
    public class ZombieInvestigateState : AIState
    {
        public override string Name => "ZombieInvestigate";
        
        private Vector2 _investigatePosition;
        private float _investigateStartTime;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            if (Blackboard.TryGet<Vector2>("InvestigatePosition", out var pos))
            {
                _investigatePosition = pos;
                _investigateStartTime = Time.time;
                Agent.MoveTo(pos);
            }
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 到达调查位置后四处张望
            if (!Agent.IsMoving)
            {
                // 可以添加转向行为
            }
        }
        
        public override void OnExit()
        {
            Agent.StopMoving();
            Blackboard.Remove("InvestigatePosition");
            Blackboard.Remove("InvestigateTime");
        }
        
        public override Type CheckTransition()
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return typeof(ZombieIdleState);
            
            // 发现敌人 -> 追逐
            if (Blackboard.HasValidTarget())
            {
                return typeof(ZombieChaseState);
            }
            
            // 调查超时 -> 空闲
            if (Time.time - _investigateStartTime > zombie.InvestigateTime)
            {
                return typeof(ZombieIdleState);
            }
            
            // 到达调查位置且等待一段时间 -> 空闲
            if (!Agent.IsMoving && TimeInState > 3f)
            {
                return typeof(ZombieIdleState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// 僵尸追逐状态
    /// </summary>
    public class ZombieChaseState : AIState
    {
        public override string Name => "ZombieChase";
        
        private float _lastPathTime;
        private float _pathUpdateInterval = 0.5f;
        private float _lostTargetTimer;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _lastPathTime = 0f;
            _lostTargetTimer = 0f;
            
            // 切换到追逐速度
            var zombie = Agent as ZombieAgent;
            if (zombie != null)
            {
                // zombie._moveSpeed = zombie.ChaseSpeed;
            }
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return;
            
            var target = zombie.GetTarget();
            if (target == null || target.IsDestroyed)
            {
                _lostTargetTimer += deltaTime;
                return;
            }
            
            // 检查是否能看到目标
            if (zombie.CanSee(target))
            {
                _lostTargetTimer = 0f;
                Blackboard.Set(AIBlackboard.KEY_TARGET_LAST_SEEN, Time.time);
                Blackboard.Set(AIBlackboard.KEY_TARGET_POSITION, target.WorldPosition);
            }
            else
            {
                _lostTargetTimer += deltaTime;
            }
            
            // 更新路径
            if (Time.time - _lastPathTime > _pathUpdateInterval)
            {
                _lastPathTime = Time.time;
                
                Vector2 targetPos = Blackboard.Get<Vector2>(AIBlackboard.KEY_TARGET_POSITION);
                zombie.MoveTo(targetPos);
            }
        }
        
        public override void OnExit()
        {
            Agent.StopMoving();
        }
        
        public override Type CheckTransition()
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return typeof(ZombieIdleState);
            
            var target = zombie.GetTarget();
            
            // 目标无效
            if (target == null || target.IsDestroyed)
            {
                zombie.ClearTarget();
                return typeof(ZombieIdleState);
            }
            
            // 丢失目标太久
            if (_lostTargetTimer > zombie.LoseTargetTime)
            {
                zombie.ClearTarget();
                return typeof(ZombieIdleState);
            }
            
            // 在攻击范围内 -> 攻击
            if (zombie.IsInAttackRange(target.WorldPosition))
            {
                return typeof(ZombieAttackState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// 僵尸攻击状态
    /// </summary>
    public class ZombieAttackState : AIState
    {
        public override string Name => "ZombieAttack";
        
        public override void OnEnter()
        {
            base.OnEnter();
            Agent.StopMoving();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return;
            
            var target = zombie.GetTarget();
            if (target == null || target.IsDestroyed) return;
            
            // 面向目标
            zombie.LookAt(target.WorldPosition);
            
            // 执行攻击
            if (zombie.CanAttack())
            {
                zombie.Attack(target);
            }
        }
        
        public override Type CheckTransition()
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return typeof(ZombieIdleState);
            
            var target = zombie.GetTarget();
            
            // 目标死亡 -> 进食
            if (target == null || target.IsDestroyed)
            {
                return typeof(ZombieFeedState);
            }
            
            // 目标逃出攻击范围 -> 追逐
            if (!zombie.IsInAttackRange(target.WorldPosition))
            {
                return typeof(ZombieChaseState);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// 僵尸进食状态
    /// </summary>
    public class ZombieFeedState : AIState
    {
        public override string Name => "ZombieFeed";
        
        private float _feedTimer;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _feedTimer = 0f;
            Agent.ClearTarget();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _feedTimer += deltaTime;
            
            // 进食动画/行为
        }
        
        public override Type CheckTransition()
        {
            var zombie = Agent as ZombieAgent;
            if (zombie == null) return typeof(ZombieIdleState);
            
            // 发现新目标 -> 追逐
            if (Blackboard.HasValidTarget())
            {
                return typeof(ZombieChaseState);
            }
            
            // 进食完成 -> 游荡
            if (_feedTimer >= zombie.FeedingTime)
            {
                return typeof(ZombieWanderState);
            }
            
            return null;
        }
    }
    
    #endregion
}
