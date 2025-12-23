/**
 * MapRenderer.cs
 * 渲染系统总控制器
 * 
 * 负责：
 * - 协调 TileRenderer 和 EntityRenderer
 * - 管理渲染生命周期
 * - 提供统一的渲染控制接口
 */

using UnityEngine;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// 地图渲染器（总控制器）
    /// </summary>
    public class MapRenderer : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Camera Settings")]
        [SerializeField]
        [Tooltip("主相机（留空则自动获取 Camera.main）")]
        private Camera _mainCamera;
        
        [Header("URP Settings")]
        [SerializeField]
        [Tooltip("是否使用 2D 光照")]
        private bool _useLighting = true;
        
        [Header("Debug")]
        [SerializeField]
        private bool _showDebugInfo = false;
        
        #endregion
        
        #region 子渲染器
        
        /// <summary>
        /// Tile 渲染器
        /// </summary>
        private TileRenderer _tileRenderer;
        
        /// <summary>
        /// Entity 渲染器
        /// </summary>
        private EntityRenderer _entityRenderer;
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 当前渲染的地图
        /// </summary>
        private Map _currentMap;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 是否暂停渲染更新
        /// </summary>
        private bool _isPaused;
        
        #endregion
        
        #region 属性
        
        public Map CurrentMap => _currentMap;
        public TileRenderer TileRenderer => _tileRenderer;
        public EntityRenderer EntityRenderer => _entityRenderer;
        public bool IsInitialized => _isInitialized;
        public bool IsPaused => _isPaused;
        
        public Camera MainCamera
        {
            get => _mainCamera;
            set => _mainCamera = value;
        }
        
        /// <summary>
        /// 是否使用 2D 光照
        /// </summary>
        public bool UseLighting
        {
            get => _useLighting;
            set
            {
                _useLighting = value;
                if (_tileRenderer != null) _tileRenderer.UseLighting = value;
                if (_entityRenderer != null) _entityRenderer.UseLighting = value;
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化渲染系统
        /// </summary>
        public void Initialize(Map map)
        {
            if (map == null)
            {
                Debug.LogError("[MapRenderer] Map cannot be null");
                return;
            }
            
            // 如果已初始化，先清理
            if (_isInitialized)
            {
                Cleanup();
            }
            
            _currentMap = map;
            
            // 确保相机存在
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            
            if (_mainCamera == null)
            {
                Debug.LogError("[MapRenderer] No camera found!");
                return;
            }
            
            // 初始化 URP 材质
            URPMaterialHelper.Initialize();
            
            // 创建子渲染器
            CreateSubRenderers();
            
            // 初始化子渲染器
            _tileRenderer.Initialize(map, _mainCamera);
            _tileRenderer.UseLighting = _useLighting;
            
            _entityRenderer.Initialize(map, _mainCamera);
            _entityRenderer.UseLighting = _useLighting;
            
            _isInitialized = true;
            _isPaused = false;
            
            Debug.Log($"[MapRenderer] 初始化完成: Map={map.MapId}, Camera={_mainCamera.name}, Lighting={_useLighting}");
        }
        
        /// <summary>
        /// 创建子渲染器
        /// </summary>
        private void CreateSubRenderers()
        {
            // 创建 TileRenderer
            GameObject tileRendererGo = new GameObject("TileRenderer");
            tileRendererGo.transform.SetParent(transform);
            tileRendererGo.transform.localPosition = Vector3.zero;
            _tileRenderer = tileRendererGo.AddComponent<TileRenderer>();
            
            // 创建 EntityRenderer
            GameObject entityRendererGo = new GameObject("EntityRenderer");
            entityRendererGo.transform.SetParent(transform);
            entityRendererGo.transform.localPosition = Vector3.zero;
            _entityRenderer = entityRendererGo.AddComponent<EntityRenderer>();
        }
        
        #endregion
        
        #region 更新
        
        void Update()
        {
            if (!_isInitialized || _isPaused) return;
            
            // 更新 Tile 渲染
            _tileRenderer?.UpdateRendering();
            
            // 更新 Entity 渲染
            _entityRenderer?.UpdateRendering(Time.deltaTime);
        }
        
        #endregion
        
        #region 控制接口
        
        /// <summary>
        /// 暂停渲染更新
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }
        
        /// <summary>
        /// 恢复渲染更新
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }
        
        /// <summary>
        /// 强制刷新所有渲染
        /// </summary>
        public void ForceRefreshAll()
        {
            _tileRenderer?.ForceRefreshAll();
            _entityRenderer?.RefreshAllViews();
        }
        
        #endregion
        
        #region Tile 层控制
        
        /// <summary>
        /// 设置层可见性
        /// </summary>
        public void SetLayerVisible(int layerIndex, bool visible)
        {
            _tileRenderer?.SetGlobalLayerVisible(layerIndex, visible);
        }
        
        /// <summary>
        /// 设置屋顶可见性
        /// </summary>
        public void SetRoofVisible(bool visible)
        {
            _tileRenderer?.SetRoofVisible(visible);
        }
        
        /// <summary>
        /// 更新指定 Tile
        /// </summary>
        public void UpdateTile(TileCoord coord)
        {
            _tileRenderer?.UpdateTile(coord);
        }
        
        /// <summary>
        /// 刷新指定 Chunk
        /// </summary>
        public void RefreshChunk(ChunkCoord coord)
        {
            _tileRenderer?.RefreshChunk(coord);
        }
        
        #endregion
        
        #region Entity 控制
        
        /// <summary>
        /// 刷新指定实体
        /// </summary>
        public void RefreshEntity(int entityId)
        {
            _entityRenderer?.RefreshEntity(entityId);
        }
        
        /// <summary>
        /// 获取实体视图
        /// </summary>
        public EntityView GetEntityView(int entityId)
        {
            return _entityRenderer?.GetView(entityId);
        }
        
        /// <summary>
        /// 立即创建实体视图
        /// </summary>
        public EntityView CreateEntityViewImmediate(MapEntity entity)
        {
            return _entityRenderer?.CreateViewImmediate(entity);
        }
        
        /// <summary>
        /// 立即销毁实体视图
        /// </summary>
        public void DestroyEntityViewImmediate(int entityId)
        {
            _entityRenderer?.DestroyViewImmediate(entityId);
        }
        
        #endregion
        
        #region 室内/室外处理
        
        /// <summary>
        /// 进入室内（隐藏屋顶）
        /// </summary>
        public void EnterIndoor()
        {
            SetRoofVisible(false);
        }
        
        /// <summary>
        /// 离开室内（显示屋顶）
        /// </summary>
        public void ExitIndoor()
        {
            SetRoofVisible(true);
        }
        
        /// <summary>
        /// 根据玩家位置自动切换室内/室外
        /// </summary>
        public void UpdateIndoorState(TileCoord playerPosition)
        {
            if (_currentMap == null) return;
            
            TileData tile = _currentMap.GetTile(playerPosition);
            bool isIndoor = tile.IsIndoor;
            
            SetRoofVisible(!isIndoor);
        }
        
        #endregion
        
        #region 清理
        
        /// <summary>
        /// 清理渲染系统
        /// </summary>
        public void Cleanup()
        {
            // 清理子渲染器
            if (_tileRenderer != null)
            {
                _tileRenderer.Cleanup();
                Destroy(_tileRenderer.gameObject);
                _tileRenderer = null;
            }
            
            if (_entityRenderer != null)
            {
                _entityRenderer.Cleanup();
                Destroy(_entityRenderer.gameObject);
                _entityRenderer = null;
            }
            
            _currentMap = null;
            _isInitialized = false;
        }
        
        #endregion
        
        #region Debug
        
        void OnGUI()
        {
            if (!_showDebugInfo || !_isInitialized) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"Map: {_currentMap?.MapId ?? "None"}");
            GUILayout.Label($"Active Chunks: {_tileRenderer?.ActiveChunkCount ?? 0}");
            GUILayout.Label($"Pooled Chunks: {_tileRenderer?.PooledChunkCount ?? 0}");
            GUILayout.Label($"Active Entities: {_entityRenderer?.ActiveViewCount ?? 0}");
            GUILayout.Label($"Pooled Views: {_entityRenderer?.PooledViewCount ?? 0}");
            GUILayout.Label($"Paused: {_isPaused}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region 生命周期
        
        void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
    }
}
