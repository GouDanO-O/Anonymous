/**
 * EntityView.cs
 * 单个实体的视图组件
 * 
 * 负责：
 * - 渲染单个 Entity
 * - 同步 Entity 数据到视觉表现
 * - 处理动画和视觉效果
 */

using UnityEngine;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// 实体视图组件
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EntityView : MonoBehaviour
    {
        #region 组件引用
        
        /// <summary>
        /// 精灵渲染器
        /// </summary>
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        
        /// <summary>
        /// 碰撞体（可选）
        /// </summary>
        [SerializeField]
        private Collider2D _collider;
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 关联的实体数据
        /// </summary>
        private MapEntity _entity;
        
        /// <summary>
        /// 实体配置
        /// </summary>
        private EntityConfig _config;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// 当前精灵索引
        /// </summary>
        private int _currentSpriteIndex;
        
        /// <summary>
        /// 所属的 EntityRenderer
        /// </summary>
        private EntityRenderer _entityRenderer;
        
        /// <summary>
        /// 是否使用 2D 光照
        /// </summary>
        private bool _useLighting = true;
        
        #endregion
        
        #region 属性
        
        public MapEntity Entity => _entity;
        public int EntityId => _entity?.EntityId ?? MapConstants.INVALID_ENTITY_ID;
        public bool IsActive => _entity != null && gameObject.activeSelf;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        
        /// <summary>
        /// 是否使用 2D 光照
        /// </summary>
        public bool UseLighting
        {
            get => _useLighting;
            set
            {
                _useLighting = value;
                ApplyMaterial();
            }
        }
        
        #endregion
        
        #region 初始化
        
        void Awake()
        {
            // 获取组件引用
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }
        
        /// <summary>
        /// 初始化视图
        /// </summary>
        public void Initialize(EntityRenderer entityRenderer, bool useLighting = true)
        {
            _entityRenderer = entityRenderer;
            _useLighting = useLighting;
            _isInitialized = true;
            
            // 应用 URP 材质
            ApplyMaterial();
        }
        
        /// <summary>
        /// 应用 URP 材质
        /// </summary>
        private void ApplyMaterial()
        {
            if (_spriteRenderer != null)
            {
                URPMaterialHelper.SetupSpriteRenderer(_spriteRenderer, _useLighting);
            }
        }
        
        #endregion
        
        #region 绑定/解绑
        
        /// <summary>
        /// 绑定到实体
        /// </summary>
        public void Bind(MapEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("[EntityView] Entity cannot be null");
                return;
            }
            
            _entity = entity;
            _entity.BindGameObject(gameObject);
            
            // 获取配置
            _config = SpriteManager.Instance.GetEntityConfig(entity.ConfigId);
            
            // 设置名称
            gameObject.name = $"Entity_{entity.EntityId}_{_config?.EntityName ?? "Unknown"}";
            
            // 初始化视觉
            SetupVisuals();
            
            // 同步位置
            SyncTransform();
            
            // 设置排序
            UpdateSortingOrder();
            
            // 激活
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 解绑实体
        /// </summary>
        public void Unbind()
        {
            if (_entity != null)
            {
                _entity.UnbindGameObject();
            }
            
            _entity = null;
            _config = null;
            _currentSpriteIndex = 0;
            
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 设置视觉效果
        /// </summary>
        private void SetupVisuals()
        {
            if (_entity == null || _spriteRenderer == null) return;
            
            // 设置精灵
            UpdateSprite();
            
            // 设置碰撞体
            SetupCollider();
        }
        
        /// <summary>
        /// 设置碰撞体
        /// </summary>
        private void SetupCollider()
        {
            if (_config == null) return;
            
            // 根据配置决定是否需要碰撞体
            if (_config.ColliderType == ColliderType.None)
            {
                if (_collider != null)
                {
                    _collider.enabled = false;
                }
                return;
            }
            
            // 创建或配置碰撞体
            if (_config.ColliderType == ColliderType.Box)
            {
                BoxCollider2D boxCollider = _collider as BoxCollider2D;
                if (boxCollider == null)
                {
                    boxCollider = gameObject.AddComponent<BoxCollider2D>();
                    _collider = boxCollider;
                }
                
                boxCollider.size = _config.ColliderSize;
                boxCollider.offset = _config.ColliderOffset;
                boxCollider.enabled = true;
            }
            else if (_config.ColliderType == ColliderType.Circle)
            {
                CircleCollider2D circleCollider = _collider as CircleCollider2D;
                if (circleCollider == null)
                {
                    circleCollider = gameObject.AddComponent<CircleCollider2D>();
                    _collider = circleCollider;
                }
                
                circleCollider.radius = _config.ColliderSize.x * 0.5f;
                circleCollider.offset = _config.ColliderOffset;
                circleCollider.enabled = true;
            }
        }
        
        #endregion
        
        #region 视觉更新
        
        /// <summary>
        /// 更新精灵
        /// </summary>
        public void UpdateSprite(int spriteIndex = -1)
        {
            if (_entity == null || _spriteRenderer == null) return;
            
            if (spriteIndex >= 0)
            {
                _currentSpriteIndex = spriteIndex;
            }
            
            Sprite sprite = SpriteManager.Instance.GetEntitySprite(
                _entity.ConfigId, 
                _currentSpriteIndex
            );
            
            _spriteRenderer.sprite = sprite;
        }
        
        /// <summary>
        /// 根据朝向更新精灵
        /// </summary>
        public void UpdateSpriteByRotation()
        {
            if (_entity == null) return;
            
            // 假设精灵按 North(0), East(1), South(2), West(3) 顺序排列
            int spriteIndex = (int)_entity.Rotation;
            UpdateSprite(spriteIndex);
        }
        
        /// <summary>
        /// 同步位置和旋转
        /// </summary>
        public void SyncTransform()
        {
            if (_entity == null) return;
            
            // 同步位置
            Vector2 worldPos = _entity.WorldPosition;
            transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            
            // 同步旋转（对于需要旋转的实体）
            // 注意：有些实体用精灵变体表示方向，不需要实际旋转
            // 这里根据配置决定
            if (_config != null && _config.SpriteNames.Length <= 1)
            {
                // 只有一个精灵，使用实际旋转
                float angle = (int)_entity.Rotation * -90f;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                // 多精灵，使用精灵变体
                transform.rotation = Quaternion.identity;
                UpdateSpriteByRotation();
            }
            
            // 更新排序
            UpdateSortingOrder();
        }
        
        /// <summary>
        /// 更新排序顺序
        /// </summary>
        public void UpdateSortingOrder()
        {
            if (_entity == null || _spriteRenderer == null) return;
            
            // 基于 Y 坐标计算排序值
            // Y 越小（越靠下），排序值越大（渲染在上面）
            float y = transform.position.y;
            int order = RenderingConstants.ENTITY_BASE_SORTING_ORDER 
                        - Mathf.RoundToInt(y * RenderingConstants.SORTING_ORDER_PER_Y);
            
            _spriteRenderer.sortingOrder = order;
        }
        
        /// <summary>
        /// 设置透明度
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (_spriteRenderer == null) return;
            
            Color color = _spriteRenderer.color;
            color.a = Mathf.Clamp01(alpha);
            _spriteRenderer.color = color;
        }
        
        /// <summary>
        /// 设置颜色
        /// </summary>
        public void SetColor(Color color)
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.color = color;
        }
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.enabled = visible;
        }
        
        #endregion
        
        #region 门特殊处理
        
        /// <summary>
        /// 更新门的视觉状态
        /// </summary>
        public void UpdateDoorVisual()
        {
            if (_entity is DoorEntity door)
            {
                // 根据开门进度选择精灵
                // 假设精灵数组：0=关闭, 1=半开, 2=全开
                int spriteIndex;
                if (door.IsFullyClosed)
                {
                    spriteIndex = 0;
                }
                else if (door.IsFullyOpen)
                {
                    spriteIndex = _config != null && _config.SpriteNames.Length > 2 ? 2 : 1;
                }
                else
                {
                    spriteIndex = 1; // 过渡状态
                }
                
                UpdateSprite(spriteIndex);
                
                // 更新碰撞体
                if (_collider != null)
                {
                    _collider.enabled = !door.IsFullyOpen;
                }
            }
        }
        
        #endregion
        
        #region 容器特殊处理
        
        /// <summary>
        /// 更新容器的视觉状态
        /// </summary>
        public void UpdateContainerVisual()
        {
            if (_entity is ContainerEntity container)
            {
                // 可以根据是否打开显示不同精灵
                // 假设精灵数组：0=关闭, 1=打开
                int spriteIndex = container.IsOpen ? 1 : 0;
                
                if (_config != null && spriteIndex < _config.SpriteNames.Length)
                {
                    UpdateSprite(spriteIndex);
                }
            }
        }
        
        #endregion
        
        #region 更新
        
        /// <summary>
        /// 同步所有状态
        /// </summary>
        public void SyncAll()
        {
            if (_entity == null) return;
            
            SyncTransform();
            
            // 特殊类型处理
            if (_entity is DoorEntity)
            {
                UpdateDoorVisual();
            }
            else if (_entity is ContainerEntity)
            {
                UpdateContainerVisual();
            }
        }
        
        /// <summary>
        /// 每帧更新（如果需要）
        /// </summary>
        public void UpdateView(float deltaTime)
        {
            if (_entity == null) return;
            
            // 检查是否需要同步
            if (_entity.IsDirty)
            {
                SyncAll();
                _entity.ClearDirty();
            }
            
            // 门动画更新
            if (_entity is DoorEntity door && door.IsAnimating)
            {
                UpdateDoorVisual();
            }
        }
        
        #endregion
        
        #region 生命周期
        
        void OnDestroy()
        {
            Unbind();
        }
        
        #endregion
    }
}
