/**
 * AIAgent.cs
 * AI 代理基类
 * 
 * 所有 AI 控制角色的基类
 * 整合状态机、感知、寻路等系统
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using GDFramework.MapSystem.Pathfinding;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// AI 代理基类
    /// </summary>
    public abstract class AIAgent : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Agent Settings")]
        [SerializeField]
        protected float _moveSpeed = 3f;
        
        [SerializeField]
        protected float _turnSpeed = 360f;
        
        [SerializeField]
        protected float _stoppingDistance = 0.1f;
        
        [Header("Combat")]
        [SerializeField]
        protected float _attackRange = 1.5f;
        
        [SerializeField]
        protected float _attackDamage = 10f;
        
        [SerializeField]
        protected float _attackCooldown = 1f;
        
        [Header("Perception")]
        [SerializeField]
        protected float _visionRange = 10f;
        
        [SerializeField]
        protected float _visionAngle = 120f;
        
        [SerializeField]
        protected float _hearingRange = 15f;
        
        [Header("Debug")]
        [SerializeField]
        protected bool _showDebugGizmos = true;
        
        #endregion
        
        #region 组件引用
        
        /// <summary>
        /// 关联的实体
        /// </summary>
        protected MapEntity _entity;
        
        /// <summary>
        /// 状态机
        /// </summary>
        protected AIStateMachine _stateMachine;
        
        /// <summary>
        /// 黑板
        /// </summary>
        protected AIBlackboard _blackboard;
        
        /// <summary>
        /// 感知系统
        /// </summary>
        protected PerceptionSystem _perception;
        
        /// <summary>
        /// 当前路径
        /// </summary>
        protected List<TileCoord> _currentPath;
        
        /// <summary>
        /// 当前路径索引
        /// </summary>
        protected int _pathIndex;
        
        #endregion
        
        #region 运行时状态
        
        protected Vector2 _targetPosition;
        protected float _lastAttackTime;
        protected bool _isMoving;
        protected Vector2 _facingDirection = Vector2.down;
        
        #endregion
        
        #region 属性
        
        public MapEntity Entity => _entity;
        public AIStateMachine StateMachine => _stateMachine;
        public AIBlackboard Blackboard => _blackboard;
        public PerceptionSystem Perception => _perception;
        
        public float MoveSpeed => _moveSpeed;
        public float AttackRange => _attackRange;
        public float VisionRange => _visionRange;
        
        public Vector2 Position => _entity?.WorldPosition ?? (Vector2)transform.position;
        public TileCoord TilePosition => _entity?.TilePosition ?? MapCoordUtility.WorldToTile(transform.position);
        
        public bool IsAlive => _entity == null || !_entity.IsDestroyed;
        public bool IsMoving => _isMoving;
        public Vector2 FacingDirection => _facingDirection;
        
        #endregion
        
        #region 事件
        
        public event Action<AIAgent, MapEntity> OnTargetAcquired;
        public event Action<AIAgent> OnTargetLost;
        public event Action<AIAgent, MapEntity, float> OnAttack;
        public event Action<AIAgent, float> OnDamaged;
        public event Action<AIAgent> OnDeath;
        
        #endregion
        
        #region Unity 生命周期
        
        protected virtual void Awake()
        {
            InitializeSystems();
        }
        
        protected virtual void Start()
        {
            SetupStateMachine();
            _stateMachine.Start();
        }
        
        protected virtual void Update()
        {
            if (!IsAlive) return;
            
            float deltaTime = Time.deltaTime;
            
            // 更新感知
            _perception?.Update(deltaTime);
            
            // 更新状态机
            _stateMachine?.Update(deltaTime);
            
            // 更新移动
            UpdateMovement(deltaTime);
        }
        
        protected virtual void FixedUpdate()
        {
            _stateMachine?.FixedUpdate();
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化系统
        /// </summary>
        protected virtual void InitializeSystems()
        {
            // 创建黑板
            _blackboard = new AIBlackboard();
            
            // 创建状态机
            _stateMachine = new AIStateMachine(this);
            
            // 创建感知系统
            _perception = new PerceptionSystem(this);
            _perception.VisionRange = _visionRange;
            _perception.VisionAngle = _visionAngle;
            _perception.HearingRange = _hearingRange;
        }
        
        /// <summary>
        /// 设置状态机（子类实现）
        /// </summary>
        protected abstract void SetupStateMachine();
        
        /// <summary>
        /// 绑定实体
        /// </summary>
        public void BindEntity(MapEntity entity)
        {
            _entity = entity;
            _blackboard.Set(AIBlackboard.KEY_SELF_POSITION, entity.WorldPosition);
        }
        
        #endregion
        
        #region 移动
        
        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public void MoveTo(TileCoord target)
        {
            // 请求寻路
            var result = PathfindingManager.Instance.FindPath(TilePosition, target);
            
            if (result.Success)
            {
                _currentPath = result.Path;
                _pathIndex = 0;
                _isMoving = true;
                _blackboard.Set(AIBlackboard.KEY_CURRENT_PATH, _currentPath);
            }
            else
            {
                _currentPath = null;
                _isMoving = false;
            }
        }
        
        /// <summary>
        /// 移动到世界坐标
        /// </summary>
        public void MoveTo(Vector2 worldPosition)
        {
            MoveTo(MapCoordUtility.WorldToTile(worldPosition));
        }
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            _currentPath = null;
            _pathIndex = 0;
            _isMoving = false;
        }
        
        /// <summary>
        /// 更新移动
        /// </summary>
        protected virtual void UpdateMovement(float deltaTime)
        {
            if (!_isMoving || _currentPath == null || _pathIndex >= _currentPath.Count)
            {
                _isMoving = false;
                return;
            }
            
            // 获取下一个路径点
            TileCoord nextTile = _currentPath[_pathIndex];
            Vector2 nextPos = MapCoordUtility.TileToWorldCenter(nextTile);
            
            // 计算方向和距离
            Vector2 currentPos = Position;
            Vector2 direction = (nextPos - currentPos).normalized;
            float distance = Vector2.Distance(currentPos, nextPos);
            
            // 更新朝向
            if (direction != Vector2.zero)
            {
                _facingDirection = direction;
            }
            
            // 移动
            if (distance > _stoppingDistance)
            {
                Vector2 newPos = currentPos + direction * _moveSpeed * deltaTime;
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
                
                // 更新实体位置
                _entity?.SetWorldPosition(newPos);
            }
            else
            {
                // 到达当前路径点
                _pathIndex++;
                
                if (_pathIndex >= _currentPath.Count)
                {
                    // 到达终点
                    _isMoving = false;
                }
            }
        }
        
        /// <summary>
        /// 向目标方向移动（不使用寻路）
        /// </summary>
        public void MoveTowards(Vector2 target, float deltaTime)
        {
            Vector2 currentPos = Position;
            Vector2 direction = (target - currentPos).normalized;
            
            if (direction != Vector2.zero)
            {
                _facingDirection = direction;
                Vector2 newPos = currentPos + direction * _moveSpeed * deltaTime;
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
                _entity?.SetWorldPosition(newPos);
            }
        }
        
        #endregion
        
        #region 战斗
        
        /// <summary>
        /// 是否可以攻击
        /// </summary>
        public bool CanAttack()
        {
            return Time.time - _lastAttackTime >= _attackCooldown;
        }
        
        /// <summary>
        /// 是否在攻击范围内
        /// </summary>
        public bool IsInAttackRange(Vector2 targetPos)
        {
            return Vector2.Distance(Position, targetPos) <= _attackRange;
        }
        
        /// <summary>
        /// 执行攻击
        /// </summary>
        public virtual void Attack(MapEntity target)
        {
            if (!CanAttack()) return;
            if (target == null || target.IsDestroyed) return;
            
            _lastAttackTime = Time.time;
            
            // 造成伤害
            target.TakeDamage((int)_attackDamage);
            
            // 面向目标
            Vector2 direction = (target.WorldPosition - Position).normalized;
            if (direction != Vector2.zero)
            {
                _facingDirection = direction;
            }
            
            OnAttack?.Invoke(this, target, _attackDamage);
            
            Debug.Log($"[AIAgent] {name} attacks {target} for {_attackDamage} damage");
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(float damage, AIAgent attacker = null)
        {
            _entity?.TakeDamage((int)damage);
            
            OnDamaged?.Invoke(this, damage);
            
            // 如果被攻击，设置攻击者为目标
            if (attacker != null && !_blackboard.HasValidTarget())
            {
                SetTarget(attacker.Entity);
            }
            
            // 检查死亡
            if (_entity != null && _entity.IsDestroyed)
            {
                Die();
            }
        }
        
        /// <summary>
        /// 死亡
        /// </summary>
        protected virtual void Die()
        {
            _stateMachine.ChangeState<DeadState>();
            OnDeath?.Invoke(this);
            Debug.Log($"[AIAgent] {name} died");
        }
        
        #endregion
        
        #region 目标管理
        
        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(MapEntity target)
        {
            var previousTarget = _blackboard.GetTarget();
            _blackboard.SetTarget(target);
            
            if (target != null && previousTarget != target)
            {
                OnTargetAcquired?.Invoke(this, target);
            }
        }
        
        /// <summary>
        /// 清除目标
        /// </summary>
        public void ClearTarget()
        {
            if (_blackboard.HasValidTarget())
            {
                _blackboard.ClearTarget();
                OnTargetLost?.Invoke(this);
            }
        }
        
        /// <summary>
        /// 获取目标
        /// </summary>
        public MapEntity GetTarget()
        {
            return _blackboard.GetTarget();
        }
        
        /// <summary>
        /// 获取目标距离
        /// </summary>
        public float GetTargetDistance()
        {
            var target = GetTarget();
            if (target == null) return float.MaxValue;
            return Vector2.Distance(Position, target.WorldPosition);
        }
        
        #endregion
        
        #region 工具方法
        
        /// <summary>
        /// 是否能看到目标
        /// </summary>
        public bool CanSee(MapEntity target)
        {
            return _perception?.CanSee(target) ?? false;
        }
        
        /// <summary>
        /// 是否能看到位置
        /// </summary>
        public bool CanSeePosition(Vector2 position)
        {
            return _perception?.CanSeePosition(position) ?? false;
        }
        
        /// <summary>
        /// 获取到目标的方向
        /// </summary>
        public Vector2 GetDirectionTo(Vector2 target)
        {
            return (target - Position).normalized;
        }
        
        /// <summary>
        /// 面向目标
        /// </summary>
        public void LookAt(Vector2 target)
        {
            Vector2 direction = GetDirectionTo(target);
            if (direction != Vector2.zero)
            {
                _facingDirection = direction;
            }
        }
        
        #endregion
        
        #region 调试
        
        protected virtual void OnDrawGizmos()
        {
            if (!_showDebugGizmos) return;
            
            Vector3 pos = transform.position;
            
            // 绘制视野范围
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(pos, _visionRange);
            
            // 绘制攻击范围
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(pos, _attackRange);
            
            // 绘制朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + (Vector3)_facingDirection * 1f);
            
            // 绘制路径
            if (_currentPath != null && _currentPath.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = _pathIndex; i < _currentPath.Count - 1; i++)
                {
                    Vector3 from = (Vector3)(Vector2)MapCoordUtility.TileToWorldCenter(_currentPath[i]);
                    Vector3 to = (Vector3)(Vector2)MapCoordUtility.TileToWorldCenter(_currentPath[i + 1]);
                    Gizmos.DrawLine(from, to);
                }
            }
            
            // 绘制目标
            var target = GetTarget();
            if (target != null)
            {
                Gizmos.color = Color.red;
                Vector3 targetPos = (Vector3)target.WorldPosition;
                Gizmos.DrawLine(pos, targetPos);
                Gizmos.DrawWireSphere(targetPos, 0.3f);
            }
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;
            
            // 绘制视野锥
            Vector3 pos = transform.position;
            float halfAngle = _visionAngle * 0.5f * Mathf.Deg2Rad;
            
            Vector2 forward = _facingDirection;
            if (forward == Vector2.zero) forward = Vector2.down;
            
            float forwardAngle = Mathf.Atan2(forward.y, forward.x);
            
            Vector3 leftDir = new Vector3(
                Mathf.Cos(forwardAngle + halfAngle),
                Mathf.Sin(forwardAngle + halfAngle),
                0
            ) * _visionRange;
            
            Vector3 rightDir = new Vector3(
                Mathf.Cos(forwardAngle - halfAngle),
                Mathf.Sin(forwardAngle - halfAngle),
                0
            ) * _visionRange;
            
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(pos, pos + leftDir);
            Gizmos.DrawLine(pos, pos + rightDir);
        }
        
        #endregion
    }
}
