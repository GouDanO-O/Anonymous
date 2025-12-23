/**
 * AIBlackboard.cs
 * AI 黑板系统
 * 
 * 黑板是 AI 系统中用于存储和共享数据的中央存储器
 * 所有 AI 组件都可以读写黑板上的数据
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// AI 黑板 - 存储 AI 决策所需的所有数据
    /// </summary>
    [Serializable]
    public class AIBlackboard
    {
        #region 存储
        
        private Dictionary<string, object> _data;
        private Dictionary<string, float> _dataTimestamps;
        
        #endregion
        
        #region 构造函数
        
        public AIBlackboard()
        {
            _data = new Dictionary<string, object>();
            _dataTimestamps = new Dictionary<string, float>();
        }
        
        #endregion
        
        #region 通用数据访问
        
        /// <summary>
        /// 设置数据
        /// </summary>
        public void Set<T>(string key, T value)
        {
            _data[key] = value;
            _dataTimestamps[key] = Time.time;
        }
        
        /// <summary>
        /// 获取数据
        /// </summary>
        public T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 尝试获取数据
        /// </summary>
        public bool TryGet<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }
            value = default;
            return false;
        }
        
        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }
        
        /// <summary>
        /// 移除数据
        /// </summary>
        public bool Remove(string key)
        {
            _dataTimestamps.Remove(key);
            return _data.Remove(key);
        }
        
        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _dataTimestamps.Clear();
        }
        
        /// <summary>
        /// 获取数据的最后更新时间
        /// </summary>
        public float GetTimestamp(string key)
        {
            return _dataTimestamps.TryGetValue(key, out var time) ? time : 0f;
        }
        
        /// <summary>
        /// 数据是否过期
        /// </summary>
        public bool IsExpired(string key, float maxAge)
        {
            if (!_dataTimestamps.TryGetValue(key, out var time))
            {
                return true;
            }
            return Time.time - time > maxAge;
        }
        
        #endregion
        
        #region 常用数据键
        
        // === 目标相关 ===
        public const string KEY_TARGET = "Target";
        public const string KEY_TARGET_POSITION = "TargetPosition";
        public const string KEY_TARGET_LAST_SEEN = "TargetLastSeen";
        public const string KEY_TARGET_DISTANCE = "TargetDistance";
        
        // === 自身状态 ===
        public const string KEY_SELF_POSITION = "SelfPosition";
        public const string KEY_SELF_HEALTH = "SelfHealth";
        public const string KEY_SELF_STATE = "SelfState";
        
        // === 感知相关 ===
        public const string KEY_VISIBLE_ENEMIES = "VisibleEnemies";
        public const string KEY_HEARD_SOUNDS = "HeardSounds";
        public const string KEY_THREAT_LEVEL = "ThreatLevel";
        
        // === 导航相关 ===
        public const string KEY_CURRENT_PATH = "CurrentPath";
        public const string KEY_PATROL_POINTS = "PatrolPoints";
        public const string KEY_HOME_POSITION = "HomePosition";
        
        // === 行为相关 ===
        public const string KEY_CURRENT_ACTION = "CurrentAction";
        public const string KEY_LAST_ACTION_TIME = "LastActionTime";
        
        #endregion
        
        #region 便捷方法
        
        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(MapEntity target)
        {
            Set(KEY_TARGET, target);
            if (target != null)
            {
                Set(KEY_TARGET_POSITION, target.WorldPosition);
                Set(KEY_TARGET_LAST_SEEN, Time.time);
            }
        }
        
        /// <summary>
        /// 获取目标
        /// </summary>
        public MapEntity GetTarget()
        {
            return Get<MapEntity>(KEY_TARGET);
        }
        
        /// <summary>
        /// 是否有有效目标
        /// </summary>
        public bool HasValidTarget()
        {
            var target = GetTarget();
            return target != null && !target.IsDestroyed;
        }
        
        /// <summary>
        /// 清除目标
        /// </summary>
        public void ClearTarget()
        {
            Remove(KEY_TARGET);
            Remove(KEY_TARGET_POSITION);
            Remove(KEY_TARGET_LAST_SEEN);
            Remove(KEY_TARGET_DISTANCE);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 威胁等级
    /// </summary>
    public enum ThreatLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
