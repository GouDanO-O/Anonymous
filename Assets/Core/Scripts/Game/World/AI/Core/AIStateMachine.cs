/**
 * AIStateMachine.cs
 * AI 状态机系统
 * 
 * 简单而强大的有限状态机实现
 * 用于管理 AI 的行为状态
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// AI 状态基类
    /// </summary>
    public abstract class AIState
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        public virtual string Name => GetType().Name;
        
        /// <summary>
        /// 所属状态机
        /// </summary>
        protected AIStateMachine StateMachine { get; private set; }
        
        /// <summary>
        /// 所属 AI 代理
        /// </summary>
        protected AIAgent Agent => StateMachine?.Agent;
        
        /// <summary>
        /// 黑板
        /// </summary>
        protected AIBlackboard Blackboard => Agent?.Blackboard;
        
        /// <summary>
        /// 进入此状态的时间
        /// </summary>
        public float EnterTime { get; private set; }
        
        /// <summary>
        /// 在此状态停留的时间
        /// </summary>
        public float TimeInState => Time.time - EnterTime;
        
        /// <summary>
        /// 初始化（由状态机调用）
        /// </summary>
        internal void Initialize(AIStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }
        
        /// <summary>
        /// 进入状态
        /// </summary>
        public virtual void OnEnter()
        {
            EnterTime = Time.time;
        }
        
        /// <summary>
        /// 退出状态
        /// </summary>
        public virtual void OnExit() { }
        
        /// <summary>
        /// 状态更新
        /// </summary>
        public virtual void OnUpdate(float deltaTime) { }
        
        /// <summary>
        /// 固定更新（物理相关）
        /// </summary>
        public virtual void OnFixedUpdate() { }
        
        /// <summary>
        /// 检查是否应该转换到其他状态
        /// </summary>
        /// <returns>目标状态类型，返回 null 表示保持当前状态</returns>
        public virtual Type CheckTransition()
        {
            return null;
        }
    }
    
    /// <summary>
    /// AI 状态机
    /// </summary>
    public class AIStateMachine
    {
        #region 字段
        
        private Dictionary<Type, AIState> _states;
        private AIState _currentState;
        private AIState _previousState;
        private AIAgent _agent;
        private bool _isRunning;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 所属 AI 代理
        /// </summary>
        public AIAgent Agent => _agent;
        
        /// <summary>
        /// 当前状态
        /// </summary>
        public AIState CurrentState => _currentState;
        
        /// <summary>
        /// 前一个状态
        /// </summary>
        public AIState PreviousState => _previousState;
        
        /// <summary>
        /// 当前状态名称
        /// </summary>
        public string CurrentStateName => _currentState?.Name ?? "None";
        
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<AIState, AIState> OnStateChanged;
        
        #endregion
        
        #region 构造函数
        
        public AIStateMachine(AIAgent agent)
        {
            _agent = agent;
            _states = new Dictionary<Type, AIState>();
        }
        
        #endregion
        
        #region 状态管理
        
        /// <summary>
        /// 添加状态
        /// </summary>
        public void AddState<T>() where T : AIState, new()
        {
            AddState(new T());
        }
        
        /// <summary>
        /// 添加状态实例
        /// </summary>
        public void AddState(AIState state)
        {
            var type = state.GetType();
            if (_states.ContainsKey(type))
            {
                Debug.LogWarning($"[AIStateMachine] State {type.Name} already exists");
                return;
            }
            
            state.Initialize(this);
            _states[type] = state;
        }
        
        /// <summary>
        /// 获取状态
        /// </summary>
        public T GetState<T>() where T : AIState
        {
            if (_states.TryGetValue(typeof(T), out var state))
            {
                return state as T;
            }
            return null;
        }
        
        /// <summary>
        /// 设置初始状态
        /// </summary>
        public void SetInitialState<T>() where T : AIState
        {
            ChangeState<T>();
        }
        
        /// <summary>
        /// 切换状态
        /// </summary>
        public bool ChangeState<T>() where T : AIState
        {
            return ChangeState(typeof(T));
        }
        
        /// <summary>
        /// 切换状态（通过类型）
        /// </summary>
        public bool ChangeState(Type stateType)
        {
            if (stateType == null) return false;
            
            if (!_states.TryGetValue(stateType, out var newState))
            {
                Debug.LogWarning($"[AIStateMachine] State {stateType.Name} not found");
                return false;
            }
            
            // 如果是相同状态，不切换
            if (_currentState?.GetType() == stateType)
            {
                return false;
            }
            
            // 退出当前状态
            _previousState = _currentState;
            _currentState?.OnExit();
            
            // 进入新状态
            _currentState = newState;
            _currentState.OnEnter();
            
            // 触发事件
            OnStateChanged?.Invoke(_previousState, _currentState);
            
            Debug.Log($"[AIStateMachine] {_agent?.name}: {_previousState?.Name ?? "None"} -> {_currentState.Name}");
            
            return true;
        }
        
        /// <summary>
        /// 返回前一个状态
        /// </summary>
        public bool ReturnToPreviousState()
        {
            if (_previousState == null) return false;
            return ChangeState(_previousState.GetType());
        }
        
        #endregion
        
        #region 运行控制
        
        /// <summary>
        /// 开始运行
        /// </summary>
        public void Start()
        {
            _isRunning = true;
        }
        
        /// <summary>
        /// 停止运行
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }
        
        /// <summary>
        /// 更新
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isRunning || _currentState == null) return;
            
            // 检查状态转换
            var nextStateType = _currentState.CheckTransition();
            if (nextStateType != null)
            {
                ChangeState(nextStateType);
            }
            
            // 更新当前状态
            _currentState.OnUpdate(deltaTime);
        }
        
        /// <summary>
        /// 固定更新
        /// </summary>
        public void FixedUpdate()
        {
            if (!_isRunning || _currentState == null) return;
            _currentState.OnFixedUpdate();
        }
        
        #endregion
    }
    
    #region 通用状态
    
    /// <summary>
    /// 空闲状态
    /// </summary>
    public class IdleState : AIState
    {
        public override string Name => "Idle";
        
        protected float _idleTime;
        protected float _maxIdleTime = 5f;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _idleTime = 0f;
            _maxIdleTime = UnityEngine.Random.Range(3f, 7f);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _idleTime += deltaTime;
        }
    }
    
    /// <summary>
    /// 巡逻状态
    /// </summary>
    public class PatrolState : AIState
    {
        public override string Name => "Patrol";
        
        protected int _currentWaypointIndex;
        protected List<TileCoord> _waypoints;
        protected float _waitTime;
        protected float _waitDuration = 2f;
        
        public override void OnEnter()
        {
            base.OnEnter();
            _waypoints = Blackboard?.Get<List<TileCoord>>(AIBlackboard.KEY_PATROL_POINTS);
            _currentWaypointIndex = 0;
            _waitTime = 0f;
        }
    }
    
    /// <summary>
    /// 追逐状态
    /// </summary>
    public class ChaseState : AIState
    {
        public override string Name => "Chase";
        
        protected float _lostTargetTime;
        protected float _maxLostTime = 5f;
    }
    
    /// <summary>
    /// 攻击状态
    /// </summary>
    public class AttackState : AIState
    {
        public override string Name => "Attack";
        
        protected float _attackCooldown;
        protected float _attackInterval = 1f;
    }
    
    /// <summary>
    /// 逃跑状态
    /// </summary>
    public class FleeState : AIState
    {
        public override string Name => "Flee";
        
        protected float _fleeTime;
        protected float _maxFleeTime = 10f;
    }
    
    /// <summary>
    /// 死亡状态
    /// </summary>
    public class DeadState : AIState
    {
        public override string Name => "Dead";
        
        public override Type CheckTransition()
        {
            // 死亡状态不能转换
            return null;
        }
    }
    
    #endregion
}
