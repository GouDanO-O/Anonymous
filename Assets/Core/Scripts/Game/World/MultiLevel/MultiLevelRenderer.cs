/**
 * MultiLevelRenderer.cs
 * 多层地图渲染器
 * 
 * 负责：
 * - 渲染当前激活层
 * - 可选渲染相邻层（半透明）
 * - 处理层级切换动画
 * - 管理每层的独立渲染器
 */

using System.Collections.Generic;
using UnityEngine;
using GDFramework.MapSystem.Rendering;

namespace GDFramework.MapSystem.MultiLevel
{
    /// <summary>
    /// 多层地图渲染器
    /// </summary>
    public class MultiLevelRenderer : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Settings")]
        [SerializeField]
        [Tooltip("是否渲染相邻层（透视效果）")]
        private bool _renderAdjacentLevels = true;
        
        [SerializeField]
        [Tooltip("相邻层的透明度")]
        [Range(0f, 1f)]
        private float _adjacentLevelAlpha = 0.3f;
        
        [SerializeField]
        [Tooltip("渲染的相邻层数量")]
        private int _adjacentLevelCount = 1;
        
        [SerializeField]
        [Tooltip("层级切换动画时间")]
        private float _transitionDuration = 0.3f;
        
        [SerializeField]
        [Tooltip("使用 URP 光照")]
        private bool _useLighting = true;
        
        [Header("Visual Effects")]
        [SerializeField]
        [Tooltip("地下层颜色调整")]
        private Color _undergroundTint = new Color(0.7f, 0.7f, 0.8f, 1f);
        
        [SerializeField]
        [Tooltip("上层颜色调整")]
        private Color _upperLevelTint = new Color(1f, 1f, 0.95f, 1f);
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 当前多层地图
        /// </summary>
        private MultiLevelMap _map;
        
        /// <summary>
        /// 每层的渲染器
        /// </summary>
        private Dictionary<int, LevelRendererData> _levelRenderers;
        
        /// <summary>
        /// 当前激活的层级
        /// </summary>
        private int _activeLevel;
        
        /// <summary>
        /// 层级容器
        /// </summary>
        private Transform _levelsContainer;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 正在进行的层级切换
        /// </summary>
        private LevelTransitionState _transitionState;
        
        #endregion
        
        #region 属性
        
        public MultiLevelMap Map => _map;
        public int ActiveLevel => _activeLevel;
        public bool IsInitialized => _isInitialized;
        public bool IsTransitioning => _transitionState != null && _transitionState.IsActive;
        
        /// <summary>
        /// 当前层的渲染器
        /// </summary>
        public MapRenderer ActiveRenderer
        {
            get
            {
                if (_levelRenderers.TryGetValue(_activeLevel, out var data))
                {
                    return data.Renderer;
                }
                return null;
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化多层渲染器
        /// </summary>
        public void Initialize(MultiLevelMap map)
        {
            _map = map;
            _levelRenderers = new Dictionary<int, LevelRendererData>();
            _activeLevel = map.ActiveLevel;
            
            // 创建容器
            _levelsContainer = new GameObject("Levels").transform;
            _levelsContainer.SetParent(transform);
            _levelsContainer.localPosition = Vector3.zero;
            
            // 为每个楼层创建渲染器
            foreach (var level in map.GetAllLevels())
            {
                CreateLevelRenderer(level);
            }
            
            // 设置初始可见性
            UpdateLevelVisibility();
            
            _isInitialized = true;
            
            Debug.Log($"[MultiLevelRenderer] 初始化完成: {map.LevelCount} 个楼层");
        }
        
        /// <summary>
        /// 为单个楼层创建渲染器
        /// </summary>
        private void CreateLevelRenderer(MapLevel level)
        {
            // 创建楼层容器
            var levelGo = new GameObject($"Level_{level.LevelIndex}_{level.LevelName}");
            levelGo.transform.SetParent(_levelsContainer);
            levelGo.transform.localPosition = Vector3.zero;
            
            // 创建适配器 Map（将 MapLevel 适配为 Map 接口）
            var adaptedMap = new MapLevelAdapter(level, _map.MapId);
            
            // 创建 MapRenderer
            var renderer = levelGo.AddComponent<MapRenderer>();
            renderer.UseLighting = _useLighting;
            renderer.Initialize(adaptedMap);
            
            // 创建 CanvasGroup 用于透明度控制
            // 注意：对于 SpriteRenderer/Tilemap，我们需要其他方式控制透明度
            
            // 存储渲染器数据
            var data = new LevelRendererData
            {
                Level = level,
                Renderer = renderer,
                Container = levelGo,
                CurrentAlpha = level.LevelIndex == _activeLevel ? 1f : 0f
            };
            
            _levelRenderers[level.LevelIndex] = data;
            
            // 设置初始状态
            levelGo.SetActive(level.LevelIndex == _activeLevel);
        }
        
        #endregion
        
        #region 层级切换
        
        /// <summary>
        /// 切换到指定层级
        /// </summary>
        public void SetActiveLevel(int levelIndex)
        {
            if (!_map.HasLevel(levelIndex))
            {
                Debug.LogWarning($"[MultiLevelRenderer] Level {levelIndex} does not exist");
                return;
            }
            
            if (levelIndex == _activeLevel && !IsTransitioning)
            {
                return;
            }
            
            int previousLevel = _activeLevel;
            _activeLevel = levelIndex;
            _map.SetActiveLevel(levelIndex);
            
            // 开始切换动画
            if (_transitionDuration > 0)
            {
                StartTransition(previousLevel, levelIndex);
            }
            else
            {
                // 立即切换
                UpdateLevelVisibility();
            }
        }
        
        /// <summary>
        /// 上一层
        /// </summary>
        public bool GoUp()
        {
            int nextLevel = _activeLevel + 1;
            if (_map.HasLevel(nextLevel))
            {
                SetActiveLevel(nextLevel);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 下一层
        /// </summary>
        public bool GoDown()
        {
            int nextLevel = _activeLevel - 1;
            if (_map.HasLevel(nextLevel))
            {
                SetActiveLevel(nextLevel);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 开始层级切换动画
        /// </summary>
        private void StartTransition(int fromLevel, int toLevel)
        {
            _transitionState = new LevelTransitionState
            {
                FromLevel = fromLevel,
                ToLevel = toLevel,
                Progress = 0f,
                Duration = _transitionDuration,
                IsActive = true
            };
        }
        
        /// <summary>
        /// 更新层级可见性
        /// </summary>
        private void UpdateLevelVisibility()
        {
            foreach (var kvp in _levelRenderers)
            {
                int levelIndex = kvp.Key;
                var data = kvp.Value;
                
                int distance = Mathf.Abs(levelIndex - _activeLevel);
                
                if (distance == 0)
                {
                    // 当前层：完全可见
                    data.Container.SetActive(true);
                    data.TargetAlpha = 1f;
                    ApplyLevelTint(data, GetLevelTint(levelIndex));
                }
                else if (_renderAdjacentLevels && distance <= _adjacentLevelCount)
                {
                    // 相邻层：半透明
                    data.Container.SetActive(true);
                    data.TargetAlpha = _adjacentLevelAlpha / distance;
                    ApplyLevelTint(data, GetLevelTint(levelIndex) * new Color(1, 1, 1, data.TargetAlpha));
                }
                else
                {
                    // 其他层：隐藏
                    data.Container.SetActive(false);
                    data.TargetAlpha = 0f;
                }
            }
        }
        
        /// <summary>
        /// 获取层级颜色调整
        /// </summary>
        private Color GetLevelTint(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return _undergroundTint;
            }
            else if (levelIndex > 0)
            {
                return _upperLevelTint;
            }
            return Color.white;
        }
        
        /// <summary>
        /// 应用层级颜色
        /// </summary>
        private void ApplyLevelTint(LevelRendererData data, Color tint)
        {
            // 设置环境光强度
            // TODO: 通过 Shader 或材质属性控制整体颜色
            
            // 如果有 TileRenderer，可以设置全局颜色
            // data.Renderer.TileRenderer?.SetGlobalTint(tint);
        }
        
        #endregion
        
        #region Update
        
        void Update()
        {
            if (!_isInitialized) return;
            
            // 更新层级切换动画
            if (_transitionState != null && _transitionState.IsActive)
            {
                UpdateTransition();
            }
        }
        
        /// <summary>
        /// 更新切换动画
        /// </summary>
        private void UpdateTransition()
        {
            _transitionState.Progress += Time.deltaTime / _transitionState.Duration;
            
            if (_transitionState.Progress >= 1f)
            {
                // 动画完成
                _transitionState.Progress = 1f;
                _transitionState.IsActive = false;
                UpdateLevelVisibility();
            }
            else
            {
                // 插值更新
                float t = Mathf.SmoothStep(0, 1, _transitionState.Progress);
                
                // 淡出旧层
                if (_levelRenderers.TryGetValue(_transitionState.FromLevel, out var fromData))
                {
                    fromData.CurrentAlpha = 1f - t;
                    UpdateLevelAlpha(fromData);
                }
                
                // 淡入新层
                if (_levelRenderers.TryGetValue(_transitionState.ToLevel, out var toData))
                {
                    toData.Container.SetActive(true);
                    toData.CurrentAlpha = t;
                    UpdateLevelAlpha(toData);
                }
            }
        }
        
        /// <summary>
        /// 更新层级透明度
        /// </summary>
        private void UpdateLevelAlpha(LevelRendererData data)
        {
            // 通过调整 SpriteRenderer/Tilemap 的颜色 alpha 来实现
            // 或者使用 Shader 的全局属性
            
            // 简单实现：调整容器缩放（不推荐，仅作为示例）
            // 更好的方案是使用自定义 Shader 或后处理
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 刷新指定层级
        /// </summary>
        public void RefreshLevel(int levelIndex)
        {
            if (_levelRenderers.TryGetValue(levelIndex, out var data))
            {
                data.Renderer.ForceRefreshAll();
            }
        }
        
        /// <summary>
        /// 刷新所有层级
        /// </summary>
        public void RefreshAllLevels()
        {
            foreach (var data in _levelRenderers.Values)
            {
                data.Renderer.ForceRefreshAll();
            }
        }
        
        /// <summary>
        /// 设置是否渲染相邻层
        /// </summary>
        public void SetRenderAdjacentLevels(bool render)
        {
            _renderAdjacentLevels = render;
            UpdateLevelVisibility();
        }
        
        /// <summary>
        /// 设置相邻层透明度
        /// </summary>
        public void SetAdjacentLevelAlpha(float alpha)
        {
            _adjacentLevelAlpha = Mathf.Clamp01(alpha);
            UpdateLevelVisibility();
        }
        
        /// <summary>
        /// 获取指定层的渲染器
        /// </summary>
        public MapRenderer GetLevelRenderer(int levelIndex)
        {
            if (_levelRenderers.TryGetValue(levelIndex, out var data))
            {
                return data.Renderer;
            }
            return null;
        }
        
        #endregion
        
        #region 清理
        
        public void Cleanup()
        {
            foreach (var data in _levelRenderers.Values)
            {
                data.Renderer?.Cleanup();
                if (data.Container != null)
                {
                    Destroy(data.Container);
                }
            }
            
            _levelRenderers.Clear();
            _isInitialized = false;
        }
        
        void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 层级渲染器数据
    /// </summary>
    internal class LevelRendererData
    {
        public MapLevel Level;
        public MapRenderer Renderer;
        public GameObject Container;
        public float CurrentAlpha;
        public float TargetAlpha;
    }
    
    /// <summary>
    /// 层级切换状态
    /// </summary>
    internal class LevelTransitionState
    {
        public int FromLevel;
        public int ToLevel;
        public float Progress;
        public float Duration;
        public bool IsActive;
    }
    
    /// <summary>
    /// MapLevel 到 Map 的适配器
    /// 让 MapLevel 可以被 MapRenderer 使用
    /// </summary>
    public class MapLevelAdapter : Map
    {
        private MapLevel _level;
        
        public MapLevelAdapter(MapLevel level, string mapId) 
            : base(mapId + "_L" + level.LevelIndex, 
                   level.LevelName, 
                   level.WidthInChunks, 
                   level.HeightInChunks,
                   level.IsOutdoor ? MapType.Outdoor : MapType.Indoor)
        {
            _level = level;
        }
        
        /// <summary>
        /// 重写 GetChunk 以使用 MapLevel 的数据
        /// </summary>
        public new Chunk GetChunk(ChunkCoord coord)
        {
            return _level.GetChunk(coord);
        }
        
        /// <summary>
        /// 重写实体管理器
        /// </summary>
        public new EntityManager Entities => _level.Entities;
    }
}
