/**
 * Portal.cs
 * 传送门数据结构
 * 
 * Portal 表示连接两个地图的传送点
 * 玩家进入传送门后会被传送到目标地图的指定位置
 */

using System;
using UnityEngine;

namespace GDFramework.MapSystem
{
    /// <summary>
    /// 传送门数据
    /// </summary>
    [Serializable]
    public class Portal
    {
        #region 字段
        
        /// <summary>
        /// 传送门唯一标识
        /// </summary>
        [SerializeField]
        private string _portalId;
        
        /// <summary>
        /// 传送门显示名称
        /// </summary>
        [SerializeField]
        private string _portalName;
        
        /// <summary>
        /// 来源地图ID（当前地图）
        /// </summary>
        [SerializeField]
        private string _sourceMapId;
        
        /// <summary>
        /// 来源位置（Tile 坐标）
        /// </summary>
        [SerializeField]
        private TileCoord _sourcePosition;
        
        /// <summary>
        /// 触发区域大小（以 Tile 为单位）
        /// </summary>
        [SerializeField]
        private Vector2Int _triggerSize;
        
        /// <summary>
        /// 目标地图ID
        /// </summary>
        [SerializeField]
        private string _targetMapId;
        
        /// <summary>
        /// 目标位置（Tile 坐标）
        /// </summary>
        [SerializeField]
        private TileCoord _targetPosition;
        
        /// <summary>
        /// 传送后玩家面朝方向
        /// </summary>
        [SerializeField]
        private Direction _targetFacing;
        
        /// <summary>
        /// 触发类型
        /// </summary>
        [SerializeField]
        private PortalTriggerType _triggerType;
        
        /// <summary>
        /// 过渡效果类型
        /// </summary>
        [SerializeField]
        private TransitionType _transitionType;
        
        /// <summary>
        /// 是否被锁定
        /// </summary>
        [SerializeField]
        private bool _isLocked;
        
        /// <summary>
        /// 解锁所需的钥匙ID（如果锁定）
        /// </summary>
        [SerializeField]
        private string _requiredKeyId;
        
        /// <summary>
        /// 是否双向传送（自动在目标地图创建返回传送门）
        /// </summary>
        [SerializeField]
        private bool _isBidirectional;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        [SerializeField]
        private bool _isEnabled;
        
        #endregion
        
        #region 属性
        
        public string PortalId => _portalId;
        public string PortalName => _portalName;
        public string SourceMapId => _sourceMapId;
        public TileCoord SourcePosition => _sourcePosition;
        public Vector2Int TriggerSize => _triggerSize;
        public string TargetMapId => _targetMapId;
        public TileCoord TargetPosition => _targetPosition;
        public Direction TargetFacing => _targetFacing;
        public PortalTriggerType TriggerType => _triggerType;
        public TransitionType TransitionType => _transitionType;
        public bool IsLocked => _isLocked;
        public string RequiredKeyId => _requiredKeyId;
        public bool IsBidirectional => _isBidirectional;
        public bool IsEnabled => _isEnabled;
        
        /// <summary>
        /// 触发区域（世界坐标矩形）
        /// </summary>
        public Rect TriggerBounds
        {
            get
            {
                Vector2 worldPos = _sourcePosition.ToWorldPosition();
                return new Rect(
                    worldPos.x,
                    worldPos.y,
                    _triggerSize.x * MapConstants.TILE_SIZE,
                    _triggerSize.y * MapConstants.TILE_SIZE
                );
            }
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数（序列化需要）
        /// </summary>
        public Portal()
        {
            _triggerSize = Vector2Int.one;
            _targetFacing = Direction.None;
            _triggerType = PortalTriggerType.OnEnter;
            _transitionType = TransitionType.Fade;
            _isEnabled = true;
        }
        
        /// <summary>
        /// 创建传送门
        /// </summary>
        public Portal(string portalId, string portalName,
            string sourceMapId, TileCoord sourcePosition,
            string targetMapId, TileCoord targetPosition)
        {
            _portalId = portalId;
            _portalName = portalName;
            _sourceMapId = sourceMapId;
            _sourcePosition = sourcePosition;
            _targetMapId = targetMapId;
            _targetPosition = targetPosition;
            _triggerSize = Vector2Int.one;
            _targetFacing = Direction.None;
            _triggerType = PortalTriggerType.OnEnter;
            _transitionType = TransitionType.Fade;
            _isEnabled = true;
        }
        
        #endregion
        
        #region Builder 模式
        
        /// <summary>
        /// 设置触发区域大小
        /// </summary>
        public Portal WithTriggerSize(int width, int height)
        {
            _triggerSize = new Vector2Int(width, height);
            return this;
        }
        
        /// <summary>
        /// 设置目标朝向
        /// </summary>
        public Portal WithTargetFacing(Direction facing)
        {
            _targetFacing = facing;
            return this;
        }
        
        /// <summary>
        /// 设置触发类型
        /// </summary>
        public Portal WithTriggerType(PortalTriggerType triggerType)
        {
            _triggerType = triggerType;
            return this;
        }
        
        /// <summary>
        /// 设置过渡效果
        /// </summary>
        public Portal WithTransition(TransitionType transitionType)
        {
            _transitionType = transitionType;
            return this;
        }
        
        /// <summary>
        /// 设置为锁定状态
        /// </summary>
        public Portal WithLock(string requiredKeyId)
        {
            _isLocked = true;
            _requiredKeyId = requiredKeyId;
            return this;
        }
        
        /// <summary>
        /// 设置为双向传送
        /// </summary>
        public Portal AsBidirectional()
        {
            _isBidirectional = true;
            return this;
        }
        
        /// <summary>
        /// 设置为需要交互触发
        /// </summary>
        public Portal RequiresInteraction()
        {
            _triggerType = PortalTriggerType.OnInteract;
            return this;
        }
        
        #endregion
        
        #region 状态操作
        
        /// <summary>
        /// 启用传送门
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
        }
        
        /// <summary>
        /// 禁用传送门
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
        }
        
        /// <summary>
        /// 解锁传送门
        /// </summary>
        public void Unlock()
        {
            _isLocked = false;
        }
        
        /// <summary>
        /// 锁定传送门
        /// </summary>
        public void Lock(string keyId = null)
        {
            _isLocked = true;
            if (!string.IsNullOrEmpty(keyId))
            {
                _requiredKeyId = keyId;
            }
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 检查指定位置是否在触发区域内
        /// </summary>
        public bool ContainsPosition(TileCoord coord)
        {
            return coord.x >= _sourcePosition.x 
                && coord.x < _sourcePosition.x + _triggerSize.x
                && coord.y >= _sourcePosition.y 
                && coord.y < _sourcePosition.y + _triggerSize.y;
        }
        
        /// <summary>
        /// 检查世界坐标是否在触发区域内
        /// </summary>
        public bool ContainsWorldPosition(Vector2 worldPos)
        {
            return TriggerBounds.Contains(worldPos);
        }
        
        /// <summary>
        /// 检查是否可以使用（启用且未锁定）
        /// </summary>
        public bool CanUse()
        {
            return _isEnabled && !_isLocked;
        }
        
        /// <summary>
        /// 检查是否可以用指定的钥匙解锁
        /// </summary>
        public bool CanUnlockWith(string keyId)
        {
            return _isLocked && _requiredKeyId == keyId;
        }
        
        #endregion
        
        /// <summary>
        /// 创建返回方向的传送门（用于双向传送）
        /// </summary>
        public Portal CreateReturnPortal()
        {
            string returnId = $"{_portalId}_return";
            string returnName = $"{_portalName} (返回)";
            
            var returnPortal = new Portal(
                returnId, returnName,
                _targetMapId, _targetPosition,
                _sourceMapId, _sourcePosition
            );
            
            returnPortal._triggerSize = _triggerSize;
            returnPortal._transitionType = _transitionType;
            returnPortal._triggerType = _triggerType;
            
            // 反转朝向
            returnPortal._targetFacing = GetOppositeDirection(_targetFacing);
            
            return returnPortal;
        }
        
        /// <summary>
        /// 获取相反方向
        /// </summary>
        private Direction GetOppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
                case Direction.NorthEast: return Direction.SouthWest;
                case Direction.NorthWest: return Direction.SouthEast;
                case Direction.SouthEast: return Direction.NorthWest;
                case Direction.SouthWest: return Direction.NorthEast;
                default: return Direction.None;
            }
        }
        
        public override string ToString()
        {
            return $"Portal({_portalId}: {_sourceMapId}[{_sourcePosition}] -> " +
                   $"{_targetMapId}[{_targetPosition}], Enabled:{_isEnabled}, Locked:{_isLocked})";
        }
    }
}
