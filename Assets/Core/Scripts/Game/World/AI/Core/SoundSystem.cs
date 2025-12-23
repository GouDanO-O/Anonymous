/**
 * SoundSystem.cs
 * 声音系统
 * 
 * 管理游戏世界中的声音传播
 * AI 可以通过此系统感知声音
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.AI
{
    /// <summary>
    /// 声音系统
    /// </summary>
    public class SoundSystem : MonoBehaviour
    {
        #region 单例
        
        private static SoundSystem _instance;
        public static SoundSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SoundSystem");
                    _instance = go.AddComponent<SoundSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 已注册的 AI 代理
        /// </summary>
        private List<AIAgent> _registeredAgents;
        
        /// <summary>
        /// 活跃的声音事件
        /// </summary>
        private List<SoundEvent> _activeSounds;
        
        /// <summary>
        /// 声音衰减系数
        /// </summary>
        [SerializeField]
        private float _attenuationFactor = 0.1f;
        
        /// <summary>
        /// 墙壁衰减系数
        /// </summary>
        [SerializeField]
        private float _wallAttenuationFactor = 0.5f;
        
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
            _registeredAgents = new List<AIAgent>();
            _activeSounds = new List<SoundEvent>();
        }
        
        void Update()
        {
            // 清理过期的声音
            float currentTime = Time.time;
            for (int i = _activeSounds.Count - 1; i >= 0; i--)
            {
                if (currentTime - _activeSounds[i].EmitTime > 1f)
                {
                    _activeSounds.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region 代理注册
        
        /// <summary>
        /// 注册 AI 代理
        /// </summary>
        public void RegisterAgent(AIAgent agent)
        {
            if (!_registeredAgents.Contains(agent))
            {
                _registeredAgents.Add(agent);
            }
        }
        
        /// <summary>
        /// 注销 AI 代理
        /// </summary>
        public void UnregisterAgent(AIAgent agent)
        {
            _registeredAgents.Remove(agent);
        }
        
        #endregion
        
        #region 声音发射
        
        /// <summary>
        /// 发射声音（静态方法）
        /// </summary>
        public static void EmitSound(Vector2 position, float volume, SoundType type, object source = null)
        {
            Instance.EmitSoundInternal(position, volume, type, source);
        }
        
        /// <summary>
        /// 发射声音（内部方法）
        /// </summary>
        private void EmitSoundInternal(Vector2 position, float volume, SoundType type, object source)
        {
            var soundEvent = new SoundEvent
            {
                Position = position,
                Volume = Mathf.Clamp01(volume),
                Type = type,
                Source = source,
                EmitTime = Time.time
            };
            
            _activeSounds.Add(soundEvent);
            
            // 通知所有 AI 代理
            foreach (var agent in _registeredAgents)
            {
                if (agent == null || !agent.IsAlive) continue;
                if (source is AIAgent sourceAgent && sourceAgent == agent) continue;
                
                // 计算有效音量（考虑距离和遮挡）
                float effectiveVolume = CalculateEffectiveVolume(
                    position, agent.Position, volume);
                
                if (effectiveVolume > 0.1f)
                {
                    agent.Perception?.HearSound(position, effectiveVolume, type, source);
                }
            }
        }
        
        /// <summary>
        /// 计算有效音量
        /// </summary>
        private float CalculateEffectiveVolume(Vector2 soundPos, Vector2 listenerPos, float baseVolume)
        {
            float distance = Vector2.Distance(soundPos, listenerPos);
            
            // 距离衰减
            float distanceAttenuation = 1f / (1f + _attenuationFactor * distance);
            
            // 墙壁遮挡（简化处理，实际应该检测射线穿过的墙壁数量）
            float wallAttenuation = 1f;
            if (!HasLineOfSight(soundPos, listenerPos))
            {
                wallAttenuation = _wallAttenuationFactor;
            }
            
            return baseVolume * distanceAttenuation * wallAttenuation;
        }
        
        /// <summary>
        /// 检查视线
        /// </summary>
        private bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            TileCoord fromTile = MapCoordUtility.WorldToTile(from);
            TileCoord toTile = MapCoordUtility.WorldToTile(to);
            
            return Pathfinding.PathfindingManager.Instance?.HasLineOfSight(fromTile, toTile) ?? true;
        }
        
        #endregion
        
        #region 预定义声音
        
        /// <summary>
        /// 脚步声
        /// </summary>
        public static void EmitFootstep(Vector2 position, bool running = false)
        {
            float volume = running ? 0.4f : 0.2f;
            EmitSound(position, volume, SoundType.Footstep);
        }
        
        /// <summary>
        /// 枪声
        /// </summary>
        public static void EmitGunshot(Vector2 position, float caliber = 1f)
        {
            EmitSound(position, Mathf.Clamp(caliber, 0.5f, 1f), SoundType.Gunshot);
        }
        
        /// <summary>
        /// 开门声
        /// </summary>
        public static void EmitDoorOpen(Vector2 position)
        {
            EmitSound(position, 0.3f, SoundType.DoorOpen);
        }
        
        /// <summary>
        /// 破门声
        /// </summary>
        public static void EmitDoorBreak(Vector2 position)
        {
            EmitSound(position, 0.8f, SoundType.DoorBreak);
        }
        
        /// <summary>
        /// 窗户破碎声
        /// </summary>
        public static void EmitWindowBreak(Vector2 position)
        {
            EmitSound(position, 0.7f, SoundType.WindowBreak);
        }
        
        /// <summary>
        /// 爆炸声
        /// </summary>
        public static void EmitExplosion(Vector2 position)
        {
            EmitSound(position, 1f, SoundType.Explosion);
        }
        
        /// <summary>
        /// 警报声
        /// </summary>
        public static void EmitAlarm(Vector2 position)
        {
            EmitSound(position, 1f, SoundType.Alert);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 声音事件
    /// </summary>
    public class SoundEvent
    {
        public Vector2 Position;
        public float Volume;
        public SoundType Type;
        public object Source;
        public float EmitTime;
    }
}
