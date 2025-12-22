/**
 * PathfindingManager.cs
 * 寻路管理器
 * 
 * 提供便捷的寻路接口：
 * - 异步寻路
 * - 路径缓存
 * - 调试可视化
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Pathfinding
{
    /// <summary>
    /// 寻路请求
    /// </summary>
    public class PathRequest
    {
        public TileCoord Start;
        public TileCoord End;
        public Action<PathResult> Callback;
        public PathfindingConfig Config;
        public bool SmoothPath;
        
        public PathRequest(TileCoord start, TileCoord end, Action<PathResult> callback)
        {
            Start = start;
            End = end;
            Callback = callback;
            SmoothPath = true;
        }
    }
    
    /// <summary>
    /// 寻路管理器
    /// </summary>
    public class PathfindingManager : MonoBehaviour
    {
        #region 单例
        
        private static PathfindingManager _instance;
        public static PathfindingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PathfindingManager");
                    _instance = go.AddComponent<PathfindingManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 序列化字段
        
        [Header("Settings")]
        [SerializeField]
        [Tooltip("每帧处理的最大请求数")]
        private int _maxRequestsPerFrame = 3;
        
        [SerializeField]
        [Tooltip("是否启用路径缓存")]
        private bool _enablePathCache = true;
        
        [SerializeField]
        [Tooltip("路径缓存时间（秒）")]
        private float _pathCacheTime = 1.0f;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showDebugGizmos = false;
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 当前地图
        /// </summary>
        private Map _currentMap;
        
        /// <summary>
        /// 寻路器
        /// </summary>
        private AStarPathfinder _pathfinder;
        
        /// <summary>
        /// 默认配置
        /// </summary>
        private PathfindingConfig _defaultConfig;
        
        /// <summary>
        /// 请求队列
        /// </summary>
        private Queue<PathRequest> _requestQueue;
        
        /// <summary>
        /// 路径缓存
        /// </summary>
        private Dictionary<string, CachedPath> _pathCache;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 最后一次寻路结果（用于调试）
        /// </summary>
        private PathResult _lastResult;
        
        #endregion
        
        #region 属性
        
        public Map CurrentMap => _currentMap;
        public bool IsInitialized => _isInitialized;
        public int PendingRequests => _requestQueue?.Count ?? 0;
        
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
            _requestQueue = new Queue<PathRequest>();
            _pathCache = new Dictionary<string, CachedPath>();
            _defaultConfig = new PathfindingConfig();
        }
        
        void Update()
        {
            ProcessRequests();
            CleanupCache();
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化寻路管理器
        /// </summary>
        public void Initialize(Map map, PathfindingConfig config = null)
        {
            _currentMap = map;
            _defaultConfig = config ?? new PathfindingConfig();
            _pathfinder = new AStarPathfinder(map, _defaultConfig);
            _isInitialized = true;
            
            // 清理缓存
            _pathCache.Clear();
            _requestQueue.Clear();
            
            Debug.Log($"[PathfindingManager] 初始化完成: Map={map.MapId}");
        }
        
        /// <summary>
        /// 更新地图（当地图变化时调用）
        /// </summary>
        public void UpdateMap(Map map)
        {
            if (map != _currentMap)
            {
                Initialize(map, _defaultConfig);
            }
            else
            {
                // 清理缓存
                _pathCache.Clear();
            }
        }
        
        #endregion
        
        #region 同步寻路
        
        /// <summary>
        /// 同步寻路（立即返回结果）
        /// </summary>
        public PathResult FindPath(TileCoord start, TileCoord end, bool smooth = true)
        {
            if (!_isInitialized)
            {
                return PathResult.Failed("寻路管理器未初始化");
            }
            
            // 检查缓存
            string cacheKey = GetCacheKey(start, end);
            if (_enablePathCache && _pathCache.TryGetValue(cacheKey, out var cached))
            {
                if (Time.time - cached.CacheTime < _pathCacheTime)
                {
                    return cached.Result;
                }
            }
            
            // 执行寻路
            var result = _pathfinder.FindPath(start, end);
            
            // 路径平滑
            if (result.Success && smooth && result.Path.Count > 2)
            {
                result.Path = _pathfinder.SmoothPath(result.Path);
            }
            
            // 缓存结果
            if (_enablePathCache && result.Success)
            {
                _pathCache[cacheKey] = new CachedPath
                {
                    Result = result,
                    CacheTime = Time.time
                };
            }
            
            _lastResult = result;
            return result;
        }
        
        /// <summary>
        /// 同步寻路（使用自定义配置）
        /// </summary>
        public PathResult FindPath(TileCoord start, TileCoord end, PathfindingConfig config, bool smooth = true)
        {
            if (!_isInitialized)
            {
                return PathResult.Failed("寻路管理器未初始化");
            }
            
            var pathfinder = new AStarPathfinder(_currentMap, config);
            var result = pathfinder.FindPath(start, end);
            
            if (result.Success && smooth && result.Path.Count > 2)
            {
                result.Path = pathfinder.SmoothPath(result.Path);
            }
            
            _lastResult = result;
            return result;
        }
        
        #endregion
        
        #region 异步寻路
        
        /// <summary>
        /// 异步寻路（通过回调返回结果）
        /// </summary>
        public void RequestPath(TileCoord start, TileCoord end, Action<PathResult> callback, bool smooth = true)
        {
            if (!_isInitialized)
            {
                callback?.Invoke(PathResult.Failed("寻路管理器未初始化"));
                return;
            }
            
            // 检查缓存
            string cacheKey = GetCacheKey(start, end);
            if (_enablePathCache && _pathCache.TryGetValue(cacheKey, out var cached))
            {
                if (Time.time - cached.CacheTime < _pathCacheTime)
                {
                    callback?.Invoke(cached.Result);
                    return;
                }
            }
            
            // 添加到请求队列
            _requestQueue.Enqueue(new PathRequest(start, end, callback)
            {
                SmoothPath = smooth
            });
        }
        
        /// <summary>
        /// 异步寻路（协程版本）
        /// </summary>
        public IEnumerator FindPathCoroutine(TileCoord start, TileCoord end, 
            Action<PathResult> callback, bool smooth = true)
        {
            PathResult result = null;
            bool completed = false;
            
            RequestPath(start, end, r =>
            {
                result = r;
                completed = true;
            }, smooth);
            
            while (!completed)
            {
                yield return null;
            }
            
            callback?.Invoke(result);
        }
        
        /// <summary>
        /// 处理请求队列
        /// </summary>
        private void ProcessRequests()
        {
            if (!_isInitialized) return;
            
            int processed = 0;
            
            while (_requestQueue.Count > 0 && processed < _maxRequestsPerFrame)
            {
                var request = _requestQueue.Dequeue();
                
                var pathfinder = request.Config != null 
                    ? new AStarPathfinder(_currentMap, request.Config)
                    : _pathfinder;
                
                var result = pathfinder.FindPath(request.Start, request.End);
                
                // 路径平滑
                if (result.Success && request.SmoothPath && result.Path.Count > 2)
                {
                    result.Path = pathfinder.SmoothPath(result.Path);
                }
                
                // 缓存
                if (_enablePathCache && result.Success)
                {
                    string cacheKey = GetCacheKey(request.Start, request.End);
                    _pathCache[cacheKey] = new CachedPath
                    {
                        Result = result,
                        CacheTime = Time.time
                    };
                }
                
                _lastResult = result;
                request.Callback?.Invoke(result);
                
                processed++;
            }
        }
        
        #endregion
        
        #region 工具方法
        
        /// <summary>
        /// 检查位置是否可行走
        /// </summary>
        public bool IsWalkable(TileCoord coord)
        {
            if (!_isInitialized || _currentMap == null)
            {
                return false;
            }
            
            return _currentMap.IsWalkable(coord);
        }
        
        /// <summary>
        /// 检查两点之间是否有直线路径
        /// </summary>
        public bool HasLineOfSight(TileCoord start, TileCoord end)
        {
            if (!_isInitialized)
            {
                return false;
            }
            
            return _pathfinder.HasLineOfSight(start, end);
        }
        
        /// <summary>
        /// 获取距离指定位置最近的可行走格子
        /// </summary>
        public TileCoord? GetNearestWalkable(TileCoord target, int maxRadius = 5)
        {
            if (!_isInitialized || _currentMap == null)
            {
                return null;
            }
            
            if (IsWalkable(target))
            {
                return target;
            }
            
            // 螺旋搜索
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                        {
                            continue; // 只检查边缘
                        }
                        
                        var coord = new TileCoord(target.x + dx, target.y + dy);
                        if (IsWalkable(coord))
                        {
                            return coord;
                        }
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 清除路径缓存
        /// </summary>
        public void ClearCache()
        {
            _pathCache.Clear();
        }
        
        /// <summary>
        /// 当地图数据改变时调用
        /// </summary>
        public void OnMapChanged()
        {
            _pathCache.Clear();
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 生成缓存键
        /// </summary>
        private string GetCacheKey(TileCoord start, TileCoord end)
        {
            return $"{start.x},{start.y}-{end.x},{end.y}";
        }
        
        /// <summary>
        /// 清理过期缓存
        /// </summary>
        private void CleanupCache()
        {
            if (!_enablePathCache) return;
            
            var keysToRemove = new List<string>();
            float currentTime = Time.time;
            
            foreach (var kvp in _pathCache)
            {
                if (currentTime - kvp.Value.CacheTime > _pathCacheTime * 2)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _pathCache.Remove(key);
            }
        }
        
        #endregion
        
        #region 调试
        
        void OnDrawGizmos()
        {
            if (!_showDebugGizmos || _lastResult == null || !_lastResult.Success)
            {
                return;
            }
            
            Gizmos.color = Color.green;
            
            for (int i = 0; i < _lastResult.Path.Count - 1; i++)
            {
                Vector3 from = MapCoordUtility.TileToWorld(_lastResult.Path[i]);
                Vector3 to = MapCoordUtility.TileToWorld(_lastResult.Path[i + 1]);
                
                Gizmos.DrawLine(from, to);
                Gizmos.DrawWireSphere(from, 0.1f);
            }
            
            if (_lastResult.Path.Count > 0)
            {
                Vector3 end = MapCoordUtility.TileToWorld(_lastResult.Path[_lastResult.Path.Count - 1]);
                Gizmos.DrawWireSphere(end, 0.1f);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 缓存的路径
    /// </summary>
    internal class CachedPath
    {
        public PathResult Result;
        public float CacheTime;
    }
}
