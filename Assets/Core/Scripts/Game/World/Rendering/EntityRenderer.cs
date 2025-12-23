/**
 * EntityRenderer.cs
 * Entity 层渲染器
 * 
 * 负责：
 * - 管理所有 EntityView
 * - 视野剔除，只渲染可见实体
 * - EntityView 对象池
 * - 实体的创建和销毁视图
 */

using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// Entity 渲染器
    /// </summary>
    public class EntityRenderer : MonoBehaviour
    {
        #region 字段
        
        /// <summary>
        /// 当前地图
        /// </summary>
        private Map _map;
        
        /// <summary>
        /// 实体管理器引用
        /// </summary>
        private EntityManager _entityManager;
        
        /// <summary>
        /// 活跃的 EntityView（按实体ID索引）
        /// </summary>
        private Dictionary<int, EntityView> _activeViews;
        
        /// <summary>
        /// EntityView 对象池
        /// </summary>
        private Queue<EntityView> _viewPool;
        
        /// <summary>
        /// EntityView 容器
        /// </summary>
        private Transform _entityContainer;
        
        /// <summary>
        /// 当前可见的实体ID集合
        /// </summary>
        private HashSet<int> _visibleEntityIds;
        
        /// <summary>
        /// 跟踪的相机
        /// </summary>
        private Camera _mainCamera;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 视野扩展（世界单位）
        /// </summary>
        private float _viewportExtend = 2f;
        
        /// <summary>
        /// 是否使用 2D 光照
        /// </summary>
        private bool _useLighting = true;
        
        #endregion
        
        #region 属性
        
        public Map Map => _map;
        public int ActiveViewCount => _activeViews?.Count ?? 0;
        public int PooledViewCount => _viewPool?.Count ?? 0;
        
        /// <summary>
        /// 是否使用 2D 光照
        /// </summary>
        public bool UseLighting
        {
            get => _useLighting;
            set
            {
                _useLighting = value;
                // 应用到所有活跃的视图
                foreach (var kvp in _activeViews)
                {
                    kvp.Value.UseLighting = value;
                }
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 初始化渲染器
        /// </summary>
        public void Initialize(Map map, Camera mainCamera = null)
        {
            if (_isInitialized)
            {
                Cleanup();
            }
            
            _map = map;
            _entityManager = map.Entities;
            _mainCamera = mainCamera ?? Camera.main;
            
            // 初始化集合
            _activeViews = new Dictionary<int, EntityView>();
            _viewPool = new Queue<EntityView>();
            _visibleEntityIds = new HashSet<int>();
            
            // 创建容器
            CreateEntityContainer();
            
            // 预热对象池
            PrewarmPool(RenderingConstants.ENTITY_VIEW_POOL_SIZE);
            
            _isInitialized = true;
            
            Debug.Log($"[EntityRenderer] 初始化完成: Map={map.MapId}");
        }
        
        /// <summary>
        /// 创建实体容器
        /// </summary>
        private void CreateEntityContainer()
        {
            if (_entityContainer != null)
            {
                Destroy(_entityContainer.gameObject);
            }
            
            GameObject container = new GameObject("EntityContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            _entityContainer = container.transform;
        }
        
        /// <summary>
        /// 预热对象池
        /// </summary>
        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                EntityView view = CreateEntityView();
                view.gameObject.SetActive(false);
                _viewPool.Enqueue(view);
            }
        }
        
        /// <summary>
        /// 创建新的 EntityView
        /// </summary>
        private EntityView CreateEntityView()
        {
            GameObject go = new GameObject("EntityView");
            go.transform.SetParent(_entityContainer);
            
            // 添加 SpriteRenderer
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = RenderingConstants.SORTING_LAYER_ENTITY;
            
            // 添加 EntityView 组件
            EntityView view = go.AddComponent<EntityView>();
            view.Initialize(this, _useLighting);
            
            return view;
        }
        
        #endregion
        
        #region 对象池管理
        
        /// <summary>
        /// 从对象池获取 EntityView
        /// </summary>
        private EntityView GetEntityView()
        {
            if (_viewPool.Count > 0)
            {
                return _viewPool.Dequeue();
            }
            
            // 池为空，创建新的
            return CreateEntityView();
        }
        
        /// <summary>
        /// 归还 EntityView 到对象池
        /// </summary>
        private void ReturnEntityView(EntityView view)
        {
            view.Unbind();
            _viewPool.Enqueue(view);
        }
        
        #endregion
        
        #region 更新逻辑
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public void UpdateRendering(float deltaTime)
        {
            if (!_isInitialized || _map == null || _mainCamera == null) return;
            
            // 计算可见实体
            UpdateVisibleEntities();
            
            // 激活新可见的实体
            ActivateVisibleEntities();
            
            // 停用不再可见的实体
            DeactivateInvisibleEntities();
            
            // 更新所有活跃视图
            UpdateActiveViews(deltaTime);
        }
        
        /// <summary>
        /// 计算可见实体
        /// </summary>
        private void UpdateVisibleEntities()
        {
            _visibleEntityIds.Clear();
            
            // 获取相机视野范围
            float height = _mainCamera.orthographicSize * 2f;
            float width = height * _mainCamera.aspect;
            Vector2 cameraPos = _mainCamera.transform.position;
            
            // 扩展范围
            float halfWidth = width * 0.5f + _viewportExtend;
            float halfHeight = height * 0.5f + _viewportExtend;
            
            // 计算视野 Tile 范围
            TileCoord minTile = MapCoordUtility.WorldToTile(new Vector2(
                cameraPos.x - halfWidth,
                cameraPos.y - halfHeight
            ));
            TileCoord maxTile = MapCoordUtility.WorldToTile(new Vector2(
                cameraPos.x + halfWidth,
                cameraPos.y + halfHeight
            ));
            
            // 获取范围内的实体
            var entitiesInRange = _entityManager.GetEntitiesInRect(minTile, maxTile);
            
            foreach (var entity in entitiesInRange)
            {
                if (entity.IsAlive)
                {
                    _visibleEntityIds.Add(entity.EntityId);
                }
            }
        }
        
        /// <summary>
        /// 激活可见实体
        /// </summary>
        private void ActivateVisibleEntities()
        {
            foreach (var entityId in _visibleEntityIds)
            {
                if (!_activeViews.ContainsKey(entityId))
                {
                    MapEntity entity = _entityManager.GetEntity(entityId);
                    if (entity != null)
                    {
                        ActivateEntity(entity);
                    }
                }
            }
        }
        
        /// <summary>
        /// 激活单个实体
        /// </summary>
        private void ActivateEntity(MapEntity entity)
        {
            EntityView view = GetEntityView();
            view.Bind(entity);
            _activeViews[entity.EntityId] = view;
        }
        
        /// <summary>
        /// 停用不可见实体
        /// </summary>
        private void DeactivateInvisibleEntities()
        {
            List<int> toRemove = new List<int>();
            
            foreach (var kvp in _activeViews)
            {
                if (!_visibleEntityIds.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var entityId in toRemove)
            {
                if (_activeViews.TryGetValue(entityId, out var view))
                {
                    ReturnEntityView(view);
                    _activeViews.Remove(entityId);
                }
            }
        }
        
        /// <summary>
        /// 更新所有活跃视图
        /// </summary>
        private void UpdateActiveViews(float deltaTime)
        {
            foreach (var kvp in _activeViews)
            {
                kvp.Value.UpdateView(deltaTime);
            }
        }
        
        #endregion
        
        #region 手动控制
        
        /// <summary>
        /// 强制刷新指定实体
        /// </summary>
        public void RefreshEntity(int entityId)
        {
            if (_activeViews.TryGetValue(entityId, out var view))
            {
                view.SyncAll();
            }
        }
        
        /// <summary>
        /// 获取实体视图
        /// </summary>
        public EntityView GetView(int entityId)
        {
            _activeViews.TryGetValue(entityId, out var view);
            return view;
        }
        
        /// <summary>
        /// 立即创建实体视图（不等待视野检测）
        /// </summary>
        public EntityView CreateViewImmediate(MapEntity entity)
        {
            if (entity == null) return null;
            
            // 如果已存在，返回现有视图
            if (_activeViews.TryGetValue(entity.EntityId, out var existingView))
            {
                return existingView;
            }
            
            EntityView view = GetEntityView();
            view.Bind(entity);
            _activeViews[entity.EntityId] = view;
            
            return view;
        }
        
        /// <summary>
        /// 立即销毁实体视图
        /// </summary>
        public void DestroyViewImmediate(int entityId)
        {
            if (_activeViews.TryGetValue(entityId, out var view))
            {
                ReturnEntityView(view);
                _activeViews.Remove(entityId);
            }
        }
        
        #endregion
        
        #region 批量操作
        
        /// <summary>
        /// 刷新所有活跃视图
        /// </summary>
        public void RefreshAllViews()
        {
            foreach (var kvp in _activeViews)
            {
                kvp.Value.SyncAll();
            }
        }
        
        /// <summary>
        /// 更新所有排序顺序
        /// </summary>
        public void UpdateAllSortingOrders()
        {
            foreach (var kvp in _activeViews)
            {
                kvp.Value.UpdateSortingOrder();
            }
        }
        
        #endregion
        
        #region 清理
        
        /// <summary>
        /// 清理所有视图
        /// </summary>
        public void Cleanup()
        {
            // 停用所有活跃视图
            if (_activeViews != null)
            {
                foreach (var kvp in _activeViews)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value.gameObject);
                    }
                }
                _activeViews.Clear();
            }
            
            // 清理对象池
            if (_viewPool != null)
            {
                while (_viewPool.Count > 0)
                {
                    var view = _viewPool.Dequeue();
                    if (view != null)
                    {
                        Destroy(view.gameObject);
                    }
                }
            }
            
            // 销毁容器
            if (_entityContainer != null)
            {
                Destroy(_entityContainer.gameObject);
                _entityContainer = null;
            }
            
            _map = null;
            _entityManager = null;
            _isInitialized = false;
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
