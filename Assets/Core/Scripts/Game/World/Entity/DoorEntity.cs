/**
 * DoorEntity.cs
 * 门实体
 * 
 * 继承自 MapEntity，增加门特有的功能：
 * - 开关状态
 * - 锁定/解锁
 * - 钥匙匹配
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 门的类型
    /// </summary>
    public enum DoorType : byte
    {
        /// <summary>
        /// 普通木门
        /// </summary>
        Wooden = 0,
        
        /// <summary>
        /// 金属门
        /// </summary>
        Metal = 1,
        
        /// <summary>
        /// 玻璃门
        /// </summary>
        Glass = 2,
        
        /// <summary>
        /// 卷帘门/车库门
        /// </summary>
        Garage = 3,
        
        /// <summary>
        /// 栅栏门
        /// </summary>
        Fence = 4
    }
    
    /// <summary>
    /// 门实体
    /// </summary>
    [Serializable]
    public class DoorEntity : MapEntity
    {
        #region 字段
        
        /// <summary>
        /// 门类型
        /// </summary>
        [SerializeField]
        private DoorType _doorType;
        
        /// <summary>
        /// 所需钥匙ID（空字符串表示不需要钥匙）
        /// </summary>
        [SerializeField]
        private string _requiredKeyId;
        
        /// <summary>
        /// 开门/关门动画时长
        /// </summary>
        [SerializeField]
        private float _animationDuration;
        
        /// <summary>
        /// 当前动画进度 (0-1)
        /// 0 = 完全关闭，1 = 完全打开
        /// </summary>
        [SerializeField]
        private float _openProgress;
        
        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        [NonSerialized]
        private bool _isAnimating;
        
        /// <summary>
        /// 动画目标状态
        /// </summary>
        [NonSerialized]
        private bool _animatingToOpen;
        
        #endregion
        
        #region 属性
        
        public DoorType DoorType => _doorType;
        public string RequiredKeyId => _requiredKeyId;
        public float AnimationDuration => _animationDuration;
        public float OpenProgress => _openProgress;
        public bool IsAnimating => _isAnimating;
        
        /// <summary>
        /// 是否已锁定
        /// </summary>
        public bool IsLocked => HasFlag(EntityFlags.IsLocked);
        
        /// <summary>
        /// 是否需要钥匙
        /// </summary>
        public bool RequiresKey => !string.IsNullOrEmpty(_requiredKeyId);
        
        /// <summary>
        /// 是否完全打开
        /// </summary>
        public bool IsFullyOpen => _openProgress >= 1f;
        
        /// <summary>
        /// 是否完全关闭
        /// </summary>
        public bool IsFullyClosed => _openProgress <= 0f;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DoorEntity() : base()
        {
            _animationDuration = 0.3f;
        }
        
        /// <summary>
        /// 创建门实体
        /// </summary>
        public DoorEntity(int entityId, int configId, string mapId, 
            TileCoord position, DoorType doorType = DoorType.Wooden) 
            : base(entityId, configId, EntityType.Door, mapId, position)
        {
            _doorType = doorType;
            _requiredKeyId = string.Empty;
            _animationDuration = 0.3f;
            _openProgress = 0f;
            
            // 门的默认属性
            AddFlag(EntityFlags.Interactive);
            AddFlag(EntityFlags.Blocking);      // 关闭时阻挡
            AddFlag(EntityFlags.Destructible);  // 可破坏
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 设置为需要钥匙的锁定门
        /// </summary>
        public DoorEntity WithLock(string keyId)
        {
            _requiredKeyId = keyId;
            AddFlag(EntityFlags.IsLocked);
            return this;
        }
        
        /// <summary>
        /// 设置动画时长
        /// </summary>
        public DoorEntity WithAnimationDuration(float duration)
        {
            _animationDuration = duration;
            return this;
        }
        
        /// <summary>
        /// 设置门类型
        /// </summary>
        public DoorEntity WithDoorType(DoorType type)
        {
            _doorType = type;
            return this;
        }
        
        /// <summary>
        /// 设置为初始打开状态
        /// </summary>
        public DoorEntity AsOpen()
        {
            _openProgress = 1f;
            AddFlag(EntityFlags.IsOpen);
            RemoveFlag(EntityFlags.Blocking);
            return this;
        }
        
        #endregion
        
        #region 开关操作
        
        /// <summary>
        /// 尝试开门
        /// </summary>
        /// <param name="keyId">使用的钥匙ID（可选）</param>
        /// <returns>是否成功</returns>
        public bool TryOpen(string keyId = null)
        {
            // 已经打开
            if (IsOpen || IsFullyOpen) return true;
            
            // 检查锁定
            if (IsLocked)
            {
                // 检查是否有匹配的钥匙
                if (!string.IsNullOrEmpty(keyId) && keyId == _requiredKeyId)
                {
                    Unlock();
                }
                else
                {
                    return false; // 锁住了且没有钥匙
                }
            }
            
            // 开始开门
            StartOpenAnimation();
            return true;
        }
        
        /// <summary>
        /// 关门
        /// </summary>
        public void Close()
        {
            if (!IsOpen && IsFullyClosed) return;
            
            StartCloseAnimation();
        }
        
        /// <summary>
        /// 切换开关状态
        /// </summary>
        public bool Toggle(string keyId = null)
        {
            if (IsOpen || _openProgress > 0.5f)
            {
                Close();
                return true;
            }
            else
            {
                return TryOpen(keyId);
            }
        }
        
        /// <summary>
        /// 强制设置为打开（无动画）
        /// </summary>
        public void ForceOpen()
        {
            _openProgress = 1f;
            _isAnimating = false;
            AddFlag(EntityFlags.IsOpen);
            RemoveFlag(EntityFlags.Blocking);
            MarkDirty();
        }
        
        /// <summary>
        /// 强制设置为关闭（无动画）
        /// </summary>
        public void ForceClose()
        {
            _openProgress = 0f;
            _isAnimating = false;
            RemoveFlag(EntityFlags.IsOpen);
            AddFlag(EntityFlags.Blocking);
            MarkDirty();
        }
        
        #endregion
        
        #region 锁定操作
        
        /// <summary>
        /// 用钥匙解锁
        /// </summary>
        public bool TryUnlock(string keyId)
        {
            if (!IsLocked) return true;
            
            if (keyId == _requiredKeyId)
            {
                Unlock();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 强制解锁
        /// </summary>
        public void Unlock()
        {
            RemoveFlag(EntityFlags.IsLocked);
            MarkDirty();
        }
        
        /// <summary>
        /// 锁定
        /// </summary>
        public void Lock()
        {
            ForceClose(); // 锁定时自动关闭
            AddFlag(EntityFlags.IsLocked);
            MarkDirty();
        }
        
        /// <summary>
        /// 更换锁（设置新钥匙）
        /// </summary>
        public void ChangeLock(string newKeyId)
        {
            _requiredKeyId = newKeyId;
            if (!string.IsNullOrEmpty(newKeyId))
            {
                AddFlag(EntityFlags.IsLocked);
            }
            MarkDirty();
        }
        
        #endregion
        
        #region 动画
        
        /// <summary>
        /// 开始开门动画
        /// </summary>
        private void StartOpenAnimation()
        {
            _isAnimating = true;
            _animatingToOpen = true;
            AddFlag(EntityFlags.IsOpen);
            MarkDirty();
        }
        
        /// <summary>
        /// 开始关门动画
        /// </summary>
        private void StartCloseAnimation()
        {
            _isAnimating = true;
            _animatingToOpen = false;
            MarkDirty();
        }
        
        /// <summary>
        /// 更新动画（每帧调用）
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void UpdateAnimation(float deltaTime)
        {
            if (!_isAnimating) return;
            
            float speed = 1f / _animationDuration;
            
            if (_animatingToOpen)
            {
                _openProgress += speed * deltaTime;
                if (_openProgress >= 1f)
                {
                    _openProgress = 1f;
                    _isAnimating = false;
                    RemoveFlag(EntityFlags.Blocking); // 完全打开后不阻挡
                }
            }
            else
            {
                _openProgress -= speed * deltaTime;
                if (_openProgress <= 0f)
                {
                    _openProgress = 0f;
                    _isAnimating = false;
                    RemoveFlag(EntityFlags.IsOpen);
                    AddFlag(EntityFlags.Blocking); // 完全关闭后阻挡
                }
            }
            
            MarkDirty();
        }
        
        #endregion
        
        #region 破坏
        
        /// <summary>
        /// 撞开门（破坏性开门）
        /// </summary>
        /// <param name="force">力度</param>
        public bool TryBashOpen(int force)
        {
            if (IsFullyOpen) return true;
            
            // 根据门类型计算所需力度
            int requiredForce = GetBashForceRequired();
            
            if (force >= requiredForce)
            {
                // 撞开成功
                ForceOpen();
                Unlock(); // 锁也被破坏
                
                // 造成伤害
                TakeDamage(20);
                
                return true;
            }
            else
            {
                // 撞击但没撞开，造成一些伤害
                TakeDamage(5);
                return false;
            }
        }
        
        /// <summary>
        /// 获取撞开所需力度
        /// </summary>
        private int GetBashForceRequired()
        {
            switch (_doorType)
            {
                case DoorType.Wooden: return 30;
                case DoorType.Glass: return 15;
                case DoorType.Metal: return 80;
                case DoorType.Garage: return 100;
                case DoorType.Fence: return 25;
                default: return 50;
            }
        }
        
        #endregion
        
        public override string ToString()
        {
            string lockStatus = IsLocked ? "Locked" : "Unlocked";
            string openStatus = IsFullyOpen ? "Open" : (IsFullyClosed ? "Closed" : $"{_openProgress:P0}");
            
            return $"Door({EntityId}, Type:{_doorType}, {openStatus}, {lockStatus}, Pos:{TilePosition})";
        }
    }
}
