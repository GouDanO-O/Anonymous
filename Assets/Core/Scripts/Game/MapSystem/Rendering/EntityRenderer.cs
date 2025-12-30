/*******************************************************************************
 * 文件名:    EntityRenderer.cs
 * 描述:      实体渲染器，负责渲染Building、Item、Pawn等实体
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   EntityRenderer 负责：
 *   - 渲染所有可见实体
 *   - 实体动画播放
 *   - 实体状态可视化（建造中、损坏等）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// 实体渲染器
    /// </summary>
    public class EntityRenderer : MonoBehaviour
    {
        #region 字段

        /// <summary>
        /// 父渲染器
        /// </summary>
        private MapRenderer _mapRenderer;

        /// <summary>
        /// 当前Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 当前楼层索引
        /// </summary>
        private int _currentFloorIndex;

        /// <summary>
        /// 实体视图缓存
        /// </summary>
        private Dictionary<int, EntityView> _entityViews;

        /// <summary>
        /// 视图对象池
        /// </summary>
        private EntityViewPool _viewPool;

        /// <summary>
        /// 当前可见区域
        /// </summary>
        private CellRect _visibleRect;

        /// <summary>
        /// 类别容器
        /// </summary>
        private Dictionary<EntityCategory, Transform> _categoryContainers;

        /// <summary>
        /// 默认精灵
        /// </summary>
        private Sprite _defaultSprite;

        #endregion

        #region 属性

        /// <summary>
        /// 当前楼层
        /// </summary>
        public Floor CurrentFloor => _site?.GetFloor(_currentFloorIndex);

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(MapRenderer mapRenderer)
        {
            _mapRenderer = mapRenderer;
            _entityViews = new Dictionary<int, EntityView>();

            // 创建类别容器
            CreateCategoryContainers();

            // 创建对象池
            _viewPool = new EntityViewPool(transform);

            // 创建默认精灵
            CreateDefaultSprite();
        }

        /// <summary>
        /// 创建类别容器
        /// </summary>
        private void CreateCategoryContainers()
        {
            _categoryContainers = new Dictionary<EntityCategory, Transform>();

            foreach (EntityCategory category in Enum.GetValues(typeof(EntityCategory)))
            {
                var container = new GameObject($"Category_{category}");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                _categoryContainers[category] = container.transform;
            }
        }

        /// <summary>
        /// 创建默认精灵
        /// </summary>
        private void CreateDefaultSprite()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            _defaultSprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1
            );
        }

        /// <summary>
        /// 设置Site
        /// </summary>
        public void SetSite(Site site)
        {
            _site = site;
            Clear();
        }

        #endregion

        #region 楼层切换

        /// <summary>
        /// 楼层变更回调
        /// </summary>
        public void OnFloorChanged(int floorIndex)
        {
            _currentFloorIndex = floorIndex;
            Clear();
        }

        #endregion

        #region 可见区域

        /// <summary>
        /// 可见区域变更回调
        /// </summary>
        public void OnVisibleRectChanged(CellRect visibleRect)
        {
            _visibleRect = visibleRect;
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 刷新所有实体渲染
        /// </summary>
        public void Refresh()
        {
            if (_site == null)
                return;

            var floor = CurrentFloor;
            if (floor == null)
                return;

            var entityLister = floor.EntityLister;
            if (entityLister == null)
                return;

            // 收集当前楼层所有可见实体
            var visibleEntities = new HashSet<int>();

            foreach (var entity in entityLister.AllEntities)
            {
                if (entity == null || entity.Destroyed)
                    continue;

                // 检查是否在可见区域内
                if (IsEntityVisible(entity))
                {
                    visibleEntities.Add(entity.EntityId);

                    // 创建或更新视图
                    if (!_entityViews.ContainsKey(entity.EntityId))
                    {
                        CreateEntityView(entity);
                    }
                    else
                    {
                        UpdateEntityView(entity);
                    }
                }
            }

            // 移除不可见的实体视图
            var toRemove = new List<int>();
            foreach (var kvp in _entityViews)
            {
                if (!visibleEntities.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var entityId in toRemove)
            {
                RemoveEntityView(entityId);
            }
        }

        /// <summary>
        /// 检查实体是否可见
        /// </summary>
        private bool IsEntityVisible(Entity entity)
        {
            // 检查实体占用的任意格子是否在可见区域内
            foreach (var cell in entity.OccupiedCells())
            {
                if (_visibleRect.Contains(cell))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 清除所有渲染
        /// </summary>
        public void Clear()
        {
            foreach (var kvp in _entityViews)
            {
                _viewPool.Return(kvp.Value);
            }
            _entityViews.Clear();
        }

        #endregion

        #region 实体视图管理

        /// <summary>
        /// 创建实体视图
        /// </summary>
        private void CreateEntityView(Entity entity)
        {
            var view = _viewPool.Get();
            view.Entity = entity;

            // 设置父容器
            if (_categoryContainers.TryGetValue(entity.Category, out var container))
            {
                view.transform.SetParent(container);
            }

            // 设置位置和大小
            UpdateViewTransform(view, entity);

            // 设置渲染
            SetupEntityRenderer(view, entity);

            _entityViews[entity.EntityId] = view;
        }

        /// <summary>
        /// 更新实体视图
        /// </summary>
        private void UpdateEntityView(Entity entity)
        {
            if (!_entityViews.TryGetValue(entity.EntityId, out var view))
                return;

            UpdateViewTransform(view, entity);
            UpdateViewAppearance(view, entity);
        }

        /// <summary>
        /// 更新视图变换
        /// </summary>
        private void UpdateViewTransform(EntityView view, Entity entity)
        {
            float cellSize = _site.CellSize;
            float floorHeight = _site.FloorHeight;

            // 计算世界位置
            Vector3 worldPos = entity.GetWorldPosition();
            worldPos.y = _currentFloorIndex * floorHeight + GetEntityYOffset(entity);

            view.transform.position = worldPos;

            // 设置大小（Size返回IntVec2，x是宽度，y是深度/高度）
            var size = entity.Size;
            view.transform.localScale = new Vector3(
                size.x * cellSize,
                size.y * cellSize,
                1
            );

            // 设置旋转（使用Angle属性）- 面向上方被俯视相机看到
            float rotationY = entity.Rotation.Angle;
            view.transform.rotation = Quaternion.Euler(-90, rotationY, 0);
        }

        /// <summary>
        /// 获取实体Y轴偏移
        /// </summary>
        private float GetEntityYOffset(Entity entity)
        {
            // 根据类别设置不同的高度层级
            switch (entity.Category)
            {
                case EntityCategory.Item:
                    return 0.05f;
                case EntityCategory.Plant:
                    return 0.06f;
                case EntityCategory.Building:
                    return 0.1f;
                case EntityCategory.Pawn:
                    return 0.15f;
                case EntityCategory.Projectile:
                    return 0.2f;
                default:
                    return 0.05f;
            }
        }

        /// <summary>
        /// 设置实体渲染器
        /// </summary>
        private void SetupEntityRenderer(EntityView view, Entity entity)
        {
            var spriteRenderer = view.SpriteRenderer;
            spriteRenderer.sortingOrder = GetSortingOrder(entity);

            // 获取EntityDef
            var def = entity.Def;

            if (def != null)
            {
                // 尝试从SpriteManager获取精灵
                var sprite = SpriteManager.Instance.GenerateEntitySprite(def);
                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                    spriteRenderer.color = def.DefaultColor;
                }
                else
                {
                    // 使用默认精灵和颜色
                    spriteRenderer.sprite = _defaultSprite;
                    spriteRenderer.color = GetColorForCategory(entity.Category, def.DefId);
                }
            }
            else
            {
                spriteRenderer.sprite = _defaultSprite;
                spriteRenderer.color = GetColorForCategory(entity.Category, "Unknown");
            }

            UpdateViewAppearance(view, entity);
        }

        /// <summary>
        /// 更新视图外观
        /// </summary>
        private void UpdateViewAppearance(EntityView view, Entity entity)
        {
            var spriteRenderer = view.SpriteRenderer;
            Color baseColor = spriteRenderer.color;

            // 建造中的建筑半透明
            if (entity is Building building && !building.ConstructionComplete)
            {
                float progress = building.ConstructionProgress;
                baseColor.a = 0.3f + progress * 0.7f;
            }

            // 损坏状态变红
            if (entity.HitPointsPercent < 0.3f)
            {
                baseColor = Color.Lerp(baseColor, Color.red, 0.3f);
            }

            spriteRenderer.color = baseColor;

            // 更新状态图标
            UpdateStatusIcons(view, entity);
        }

        /// <summary>
        /// 更新状态图标
        /// </summary>
        private void UpdateStatusIcons(EntityView view, Entity entity)
        {
            // 建造中图标
            view.SetStatusIcon("construction", 
                entity is Building b && !b.ConstructionComplete);

            // 损坏图标
            view.SetStatusIcon("damaged", 
                entity.HitPointsPercent < 0.5f && entity.HitPointsPercent > 0);

            // 无电图标
            if (entity is Building building2 && building2.RequiresPower)
            {
                view.SetStatusIcon("no_power", !building2.HasPower);
            }
        }

        /// <summary>
        /// 移除实体视图
        /// </summary>
        private void RemoveEntityView(int entityId)
        {
            if (_entityViews.TryGetValue(entityId, out var view))
            {
                _viewPool.Return(view);
                _entityViews.Remove(entityId);
            }
        }

        /// <summary>
        /// 获取排序顺序
        /// </summary>
        private int GetSortingOrder(Entity entity)
        {
            // 基于类别和位置计算排序顺序
            int categoryBase = (int)entity.Category * 1000;
            int posOrder = entity.Position.z * 100 + entity.Position.x;
            return categoryBase + posOrder;
        }

        /// <summary>
        /// 根据类别获取颜色
        /// </summary>
        private Color GetColorForCategory(EntityCategory category, string defId)
        {
            switch (category)
            {
                case EntityCategory.Building:
                    if (defId.Contains("Door")) return new Color(0.5f, 0.3f, 0.1f);
                    if (defId.Contains("Wall")) return new Color(0.6f, 0.6f, 0.6f);
                    if (defId.Contains("Bed")) return new Color(0.8f, 0.6f, 0.4f);
                    if (defId.Contains("Table")) return new Color(0.6f, 0.4f, 0.2f);
                    if (defId.Contains("Chair")) return new Color(0.5f, 0.35f, 0.2f);
                    if (defId.Contains("Lamp")) return new Color(1f, 0.9f, 0.5f);
                    if (defId.Contains("Generator")) return new Color(0.4f, 0.4f, 0.5f);
                    return new Color(0.5f, 0.5f, 0.5f);

                case EntityCategory.Item:
                    if (defId.Contains("Wood")) return new Color(0.6f, 0.4f, 0.2f);
                    if (defId.Contains("Steel")) return new Color(0.6f, 0.6f, 0.7f);
                    if (defId.Contains("Stone")) return new Color(0.5f, 0.5f, 0.5f);
                    if (defId.Contains("Food")) return new Color(0.8f, 0.6f, 0.3f);
                    if (defId.Contains("Medicine")) return new Color(0.9f, 0.9f, 0.9f);
                    return new Color(0.7f, 0.7f, 0.5f);

                case EntityCategory.Pawn:
                    return new Color(0.9f, 0.7f, 0.6f);

                case EntityCategory.Plant:
                    if (defId.Contains("Tree")) return new Color(0.2f, 0.5f, 0.2f);
                    if (defId.Contains("Bush")) return new Color(0.3f, 0.6f, 0.3f);
                    if (defId.Contains("Grass")) return new Color(0.4f, 0.7f, 0.3f);
                    return new Color(0.3f, 0.6f, 0.3f);

                case EntityCategory.Projectile:
                    return new Color(1f, 0.5f, 0f);

                default:
                    return Color.magenta;
            }
        }

        #endregion

        #region 位置更新

        /// <summary>
        /// 更新实体位置（每帧调用）
        /// </summary>
        public void UpdateEntityPositions()
        {
            if (_site == null)
                return;

            foreach (var kvp in _entityViews)
            {
                var view = kvp.Value;
                var entity = view.Entity;

                if (entity == null || entity.Destroyed)
                    continue;

                // 更新移动中实体的位置
                // TODO: 插值平滑移动
                UpdateViewTransform(view, entity);
            }
        }

        #endregion

        #region 特效

        /// <summary>
        /// 播放生成特效
        /// </summary>
        public void PlaySpawnEffect(Entity entity)
        {
            if (!_entityViews.TryGetValue(entity.EntityId, out var view))
                return;

            // TODO: 实现生成特效（缩放动画等）
            StartCoroutine(SpawnEffectCoroutine(view));
        }

        private System.Collections.IEnumerator SpawnEffectCoroutine(EntityView view)
        {
            Vector3 originalScale = view.transform.localScale;
            view.transform.localScale = Vector3.zero;

            float duration = 0.3f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1 - Mathf.Pow(1 - t, 3); // Ease out cubic

                view.transform.localScale = originalScale * t;
                yield return null;
            }

            view.transform.localScale = originalScale;
        }

        /// <summary>
        /// 播放销毁特效
        /// </summary>
        public void PlayDestroyEffect(Vector3 position, EntityCategory category)
        {
            // TODO: 实现销毁特效（粒子效果等）
        }

        #endregion
    }

    /// <summary>
    /// 实体视图
    /// </summary>
    public class EntityView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Dictionary<string, GameObject> _statusIcons;

        /// <summary>
        /// 关联的实体
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// 精灵渲染器
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                    if (_spriteRenderer == null)
                    {
                        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                    }
                }
                return _spriteRenderer;
            }
        }

        private void Awake()
        {
            _statusIcons = new Dictionary<string, GameObject>();
        }

        /// <summary>
        /// 设置状态图标
        /// </summary>
        public void SetStatusIcon(string iconId, bool visible)
        {
            if (!_statusIcons.TryGetValue(iconId, out var iconObj))
            {
                if (!visible) return;

                // 创建图标对象
                iconObj = new GameObject($"Icon_{iconId}");
                iconObj.transform.SetParent(transform);
                iconObj.transform.localPosition = GetIconOffset(iconId);
                iconObj.transform.localScale = Vector3.one * 0.3f;

                var sr = iconObj.AddComponent<SpriteRenderer>();
                sr.sprite = CreateIconSprite(iconId);
                sr.sortingOrder = SpriteRenderer.sortingOrder + 1;

                _statusIcons[iconId] = iconObj;
            }

            iconObj.SetActive(visible);
        }

        /// <summary>
        /// 获取图标偏移
        /// </summary>
        private Vector3 GetIconOffset(string iconId)
        {
            switch (iconId)
            {
                case "construction": return new Vector3(0.3f, 0, 0.3f);
                case "damaged": return new Vector3(-0.3f, 0, 0.3f);
                case "no_power": return new Vector3(0, 0, 0.3f);
                default: return new Vector3(0, 0, 0.3f);
            }
        }

        /// <summary>
        /// 创建图标精灵
        /// </summary>
        private Sprite CreateIconSprite(string iconId)
        {
            var texture = new Texture2D(8, 8);
            Color color = Color.white;

            switch (iconId)
            {
                case "construction":
                    color = Color.yellow;
                    break;
                case "damaged":
                    color = Color.red;
                    break;
                case "no_power":
                    color = new Color(1f, 0.5f, 0f);
                    break;
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // 简单的圆形图标
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(3.5f, 3.5f));
                    texture.SetPixel(x, y, dist < 3 ? color : Color.clear);
                }
            }
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            Entity = null;
            SpriteRenderer.sprite = null;
            SpriteRenderer.color = Color.white;

            foreach (var icon in _statusIcons.Values)
            {
                icon.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 实体视图对象池
    /// </summary>
    public class EntityViewPool
    {
        private Transform _parent;
        private Queue<EntityView> _pool;
        private int _created;

        public EntityViewPool(Transform parent)
        {
            _parent = parent;
            _pool = new Queue<EntityView>();
        }

        public EntityView Get()
        {
            EntityView view;
            if (_pool.Count > 0)
            {
                view = _pool.Dequeue();
                view.gameObject.SetActive(true);
            }
            else
            {
                var go = new GameObject($"EntityView_{_created++}");
                go.transform.SetParent(_parent);
                view = go.AddComponent<EntityView>();
            }
            return view;
        }

        public void Return(EntityView view)
        {
            view.Reset();
            view.gameObject.SetActive(false);
            _pool.Enqueue(view);
        }
    }
}
