/**
 * AIManager.cs
 * AI 管理器
 * 
 * 负责：
 * - 管理所有 AI 代理
 * - 更新 AI（可分帧更新）
 * - 提供 AI 查询接口
 * - 管理 AI 生成和销毁
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// AI 管理器
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        #region 单例
        
        private static AIManager _instance;
        public static AIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AIManager");
                    _instance = go.AddComponent<AIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 序列化字段
        
        [Header("Update Settings")]
        [SerializeField]
        [Tooltip("每帧最多更新的 AI 数量（0 = 全部更新）")]
        private int _maxUpdatesPerFrame = 0;
        
        [SerializeField]
        [Tooltip("AI 更新间隔（秒）")]
        private float _updateInterval = 0.05f;
        
        [Header("Spawning")]
        [SerializeField]
        [Tooltip("最大同时存在的 AI 数量")]
        private int _maxActiveAI = 100;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showDebugInfo = false;
        
        #endregion
        
        #region 字段
        
        private List<AIAgent> _agents;
        private Dictionary<Type, List<AIAgent>> _agentsByType;
        private int _currentUpdateIndex;
        private float _lastUpdateTime;
        private Transform _aiContainer;
        private Map _currentMap;
        
        #endregion
        
        #region 属性
        
        public int ActiveAgentCount => _agents.Count;
        public IReadOnlyList<AIAgent> Agents => _agents;
        
        #endregion
        
        #region 事件
        
        public event Action<AIAgent> OnAgentSpawned;
        public event Action<AIAgent> OnAgentDespawned;
        
        #endregion
        
        #region Unity 生命周期
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            _agents = new List<AIAgent>();
            _agentsByType = new Dictionary<Type, List<AIAgent>>();
            
            _aiContainer = new GameObject("AI_Agents").transform;
            _aiContainer.SetParent(transform);
        }
        
        void Update()
        {
            if (Time.time - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = Time.time;
            
            UpdateAgents();
            CleanupDeadAgents();
        }
        
        #endregion
        
        #region 初始化
        
        public void Initialize(Map map)
        {
            _currentMap = map;
            foreach (var agent in _agents)
            {
                SoundSystem.Instance.RegisterAgent(agent);
            }
            Debug.Log("[AIManager] 初始化完成");
        }
        
        #endregion
        
        #region AI 更新
        
        private void UpdateAgents()
        {
            if (_agents.Count == 0) return;
            
            int updateCount = _maxUpdatesPerFrame > 0 
                ? Mathf.Min(_maxUpdatesPerFrame, _agents.Count) 
                : _agents.Count;
            
            for (int i = 0; i < updateCount; i++)
            {
                if (_currentUpdateIndex >= _agents.Count)
                    _currentUpdateIndex = 0;
                _currentUpdateIndex++;
            }
        }
        
        private void CleanupDeadAgents()
        {
            for (int i = _agents.Count - 1; i >= 0; i--)
            {
                var agent = _agents[i];
                if (agent == null || !agent.IsAlive)
                {
                    RemoveAgent(agent, i);
                }
            }
        }
        
        #endregion
        
        #region 代理管理
        
        public void RegisterAgent(AIAgent agent)
        {
            if (agent == null || _agents.Contains(agent)) return;
            
            _agents.Add(agent);
            
            var type = agent.GetType();
            if (!_agentsByType.TryGetValue(type, out var list))
            {
                list = new List<AIAgent>();
                _agentsByType[type] = list;
            }
            list.Add(agent);
            
            SoundSystem.Instance.RegisterAgent(agent);
            OnAgentSpawned?.Invoke(agent);
        }
        
        public void UnregisterAgent(AIAgent agent)
        {
            if (agent == null) return;
            int index = _agents.IndexOf(agent);
            if (index >= 0) RemoveAgent(agent, index);
        }
        
        private void RemoveAgent(AIAgent agent, int index)
        {
            _agents.RemoveAt(index);
            if (_currentUpdateIndex > index) _currentUpdateIndex--;
            
            var type = agent?.GetType();
            if (type != null && _agentsByType.TryGetValue(type, out var list))
                list.Remove(agent);
            
            if (agent != null)
                SoundSystem.Instance.UnregisterAgent(agent);
            
            OnAgentDespawned?.Invoke(agent);
        }
        
        public T SpawnAgent<T>(Vector2 position) where T : AIAgent
        {
            if (_agents.Count >= _maxActiveAI)
            {
                Debug.LogWarning($"[AIManager] 达到最大 AI 数量: {_maxActiveAI}");
                return null;
            }
            
            var go = new GameObject($"AI_{typeof(T).Name}_{_agents.Count}");
            go.transform.SetParent(_aiContainer);
            go.transform.position = new Vector3(position.x, position.y, 0);
            
            var agent = go.AddComponent<T>();
            RegisterAgent(agent);
            return agent;
        }
        
        public Zombie.ZombieAgent SpawnZombie(Vector2 position, Zombie.ZombieType type = Zombie.ZombieType.Walker)
        {
            var zombie = SpawnAgent<Zombie.ZombieAgent>(position);
            if (zombie != null)
                SetPrivateField(zombie, "_zombieType", type);
            return zombie;
        }
        
        public NPC.NPCAgent SpawnNPC(Vector2 position, NPC.NPCType type = NPC.NPCType.Civilian)
        {
            var npc = SpawnAgent<NPC.NPCAgent>(position);
            if (npc != null)
                SetPrivateField(npc, "_npcType", type);
            return npc;
        }
        
        public void DespawnAgent(AIAgent agent)
        {
            if (agent == null) return;
            UnregisterAgent(agent);
            if (agent.gameObject != null)
                Destroy(agent.gameObject);
        }
        
        public void DespawnAll()
        {
            for (int i = _agents.Count - 1; i >= 0; i--)
                DespawnAgent(_agents[i]);
        }
        
        #endregion
        
        #region 查询
        
        public List<T> GetAgentsOfType<T>() where T : AIAgent
        {
            var result = new List<T>();
            if (_agentsByType.TryGetValue(typeof(T), out var list))
            {
                foreach (var agent in list)
                    if (agent is T typed)
                        result.Add(typed);
            }
            return result;
        }
        
        public List<AIAgent> GetAgentsInRange(Vector2 position, float range)
        {
            var result = new List<AIAgent>();
            float rangeSqr = range * range;
            
            foreach (var agent in _agents)
            {
                if (agent == null || !agent.IsAlive) continue;
                if ((agent.Position - position).sqrMagnitude <= rangeSqr)
                    result.Add(agent);
            }
            return result;
        }
        
        public AIAgent GetNearestAgent(Vector2 position, float maxRange = float.MaxValue)
        {
            AIAgent nearest = null;
            float nearestDistSqr = maxRange * maxRange;
            
            foreach (var agent in _agents)
            {
                if (agent == null || !agent.IsAlive) continue;
                float distSqr = (agent.Position - position).sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearest = agent;
                    nearestDistSqr = distSqr;
                }
            }
            return nearest;
        }
        
        public Zombie.ZombieAgent GetNearestZombie(Vector2 position, float maxRange = float.MaxValue)
        {
            var zombies = GetAgentsOfType<Zombie.ZombieAgent>();
            Zombie.ZombieAgent nearest = null;
            float nearestDistSqr = maxRange * maxRange;
            
            foreach (var zombie in zombies)
            {
                if (!zombie.IsAlive) continue;
                float distSqr = (zombie.Position - position).sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearest = zombie;
                    nearestDistSqr = distSqr;
                }
            }
            return nearest;
        }
        
        public int GetZombieCountInRange(Vector2 position, float range)
        {
            int count = 0;
            float rangeSqr = range * range;
            var zombies = GetAgentsOfType<Zombie.ZombieAgent>();
            
            foreach (var zombie in zombies)
            {
                if (!zombie.IsAlive) continue;
                if ((zombie.Position - position).sqrMagnitude <= rangeSqr)
                    count++;
            }
            return count;
        }
        
        #endregion
        
        #region 辅助
        
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
        
        #endregion
        
        #region 调试
        
        void OnGUI()
        {
            if (!_showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            GUILayout.BeginVertical("box");
            GUILayout.Label("=== AI Manager ===");
            GUILayout.Label($"Active: {_agents.Count}/{_maxActiveAI}");
            foreach (var kvp in _agentsByType)
                GUILayout.Label($"  {kvp.Key.Name}: {kvp.Value.Count}");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
