/*******************************************************************************
 * 文件名:    EntityDef.cs
 * 描述:      实体定义基类，所有游戏实体（建筑、物品、生物等）的定义基类
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   EntityDef 定义了游戏中所有动态实体的基础属性。
 *   与Tile不同，Entity是独立的游戏对象，可以移动、交互、被销毁。
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 实体定义基类
    /// </summary>
    [Serializable]
    public class EntityDef : DefBase
    {
        #region 基础属性

        /// <summary>
        /// 实体分类
        /// </summary>
        [SerializeField]
        internal EntityCategory _category = EntityCategory.None;

        /// <summary>
        /// 占据尺寸（格子数）
        /// </summary>
        [SerializeField]
        internal IntVec2 _size = new IntVec2(1, 1);

        /// <summary>
        /// 是否可旋转
        /// </summary>
        [SerializeField]
        internal bool _rotatable = true;

        /// <summary>
        /// 可通行性
        /// </summary>
        [SerializeField]
        internal Passability _passability = Passability.Passable;

        /// <summary>
        /// 寻路代价（额外移动消耗）
        /// </summary>
        [SerializeField]
        internal int _pathCost = 0;

        /// <summary>
        /// 填充百分比（掩体效果，0-1）
        /// </summary>
        [SerializeField]
        internal float _fillPercent = 0f;

        #endregion

        #region 放置规则

        /// <summary>
        /// 需要的地面承重等级
        /// </summary>
        [SerializeField]
        internal BearingCapacity _requiredBearing = BearingCapacity.Light;

        /// <summary>
        /// 是否必须放在地板上
        /// </summary>
        [SerializeField]
        internal bool _mustPlaceOnFloor = false;

        /// <summary>
        /// 是否可以放在墙上
        /// </summary>
        [SerializeField]
        internal bool _canPlaceOnWall = false;

        /// <summary>
        /// 是否需要在室内
        /// </summary>
        [SerializeField]
        internal bool _mustBeIndoors = false;

        /// <summary>
        /// 是否需要在室外
        /// </summary>
        [SerializeField]
        internal bool _mustBeOutdoors = false;

        /// <summary>
        /// 是否覆盖地板显示
        /// </summary>
        [SerializeField]
        internal bool _coversFloor = false;

        /// <summary>
        /// 是否阻挡风
        /// </summary>
        [SerializeField]
        internal bool _blockWind = false;

        /// <summary>
        /// 是否阻挡光线
        /// </summary>
        [SerializeField]
        internal bool _blockLight = false;

        /// <summary>
        /// 是否支撑屋顶
        /// </summary>
        [SerializeField]
        internal bool _holdsRoof = false;

        #endregion

        #region 渲染相关

        /// <summary>
        /// 渲染层级
        /// </summary>
        [SerializeField]
        internal AltitudeLayer _altitudeLayer = AltitudeLayer.Building;

        /// <summary>
        /// 图形资源路径
        /// </summary>
        [SerializeField]
        internal string _graphicPath;

        /// <summary>
        /// 图标资源路径
        /// </summary>
        [SerializeField]
        internal string _iconPath;

        /// <summary>
        /// 默认颜色
        /// </summary>
        [SerializeField]
        internal Color _defaultColor = Color.white;

        /// <summary>
        /// 绘制尺寸（世界单位）
        /// </summary>
        [SerializeField]
        internal Vector2 _drawSize = Vector2.one;

        /// <summary>
        /// 绘制偏移
        /// </summary>
        [SerializeField]
        internal Vector2 _drawOffset = Vector2.zero;

        /// <summary>
        /// 是否根据旋转切换图形
        /// </summary>
        [SerializeField]
        internal bool _graphicRotates = true;

        #endregion

        #region 耐久与生命

        /// <summary>
        /// 最大生命值（0表示无法被攻击）
        /// </summary>
        [SerializeField]
        internal int _maxHitPoints = 0;

        /// <summary>
        /// 是否可被攻击
        /// </summary>
        [SerializeField]
        internal bool _attackable = false;

        /// <summary>
        /// 易燃性（0-1）
        /// </summary>
        [SerializeField]
        internal float _flammability = 0f;

        #endregion

        #region 楼层连接器（用于楼梯/电梯等）

        /// <summary>
        /// 是否是楼层连接器
        /// </summary>
        [SerializeField]
        internal bool _isFloorConnector = false;

        /// <summary>
        /// 楼层连接器类型
        /// </summary>
        [SerializeField]
        internal FloorConnectorType _connectorType = FloorConnectorType.None;

        /// <summary>
        /// 连接的楼层数（电梯可能连接多层）
        /// </summary>
        [SerializeField]
        internal int _connectsFloors = 1;

        /// <summary>
        /// 通过连接器的移动代价
        /// </summary>
        [SerializeField]
        internal int _traverseCost = 10;

        /// <summary>
        /// 是否需要电力
        /// </summary>
        [SerializeField]
        internal bool _connectorNeedsPower = false;

        #endregion

        #region 组件配置

        /// <summary>
        /// 组件定义ID列表
        /// </summary>
        [SerializeField]
        internal List<string> _compDefIds = new List<string>();

        #endregion

        #region 属性访问

        public EntityCategory Category => _category;
        public IntVec2 Size => _size;
        public bool Rotatable => _rotatable;
        public Passability Passability => _passability;
        public int PathCost => _pathCost;
        public float FillPercent => _fillPercent;

        public BearingCapacity RequiredBearing => _requiredBearing;
        public bool MustPlaceOnFloor => _mustPlaceOnFloor;
        public bool CanPlaceOnWall => _canPlaceOnWall;
        public bool MustBeIndoors => _mustBeIndoors;
        public bool MustBeOutdoors => _mustBeOutdoors;
        public bool CoversFloor => _coversFloor;
        public bool BlockWind => _blockWind;
        public bool BlockLight => _blockLight;
        public bool HoldsRoof => _holdsRoof;

        public AltitudeLayer AltitudeLayer => _altitudeLayer;
        public string GraphicPath => _graphicPath;
        public string IconPath => _iconPath;
        public Color DefaultColor => _defaultColor;
        public Vector2 DrawSize => _drawSize;
        public Vector2 DrawOffset => _drawOffset;
        public bool GraphicRotates => _graphicRotates;

        public int MaxHitPoints => _maxHitPoints;
        public bool Attackable => _attackable;
        public float Flammability => _flammability;

        public bool IsFloorConnector => _isFloorConnector;
        public FloorConnectorType ConnectorType => _connectorType;
        public int ConnectsFloors => _connectsFloors;
        public int TraverseCost => _traverseCost;
        public bool ConnectorNeedsPower => _connectorNeedsPower;

        public IReadOnlyList<string> CompDefIds => _compDefIds;

        #endregion

        #region 派生属性

        /// <summary>
        /// 是否是单格实体
        /// </summary>
        public bool IsSingleCell => _size.x == 1 && _size.y == 1;

        /// <summary>
        /// 是否阻挡移动
        /// </summary>
        public bool BlocksMovement => _passability == Passability.Impassable;

        /// <summary>
        /// 占据的格子数
        /// </summary>
        public int CellCount => _size.Area;

        /// <summary>
        /// 是否有生命值
        /// </summary>
        public bool HasHealth => _maxHitPoints > 0;

        /// <summary>
        /// 是否可燃
        /// </summary>
        public bool IsFlammable => _flammability > 0;

        #endregion

        #region 尺寸计算

        /// <summary>
        /// 获取旋转后的尺寸
        /// </summary>
        public IntVec2 GetRotatedSize(Rotation rotation)
        {
            if (!_rotatable)
                return _size;
            return rotation.IsHorizontal ? new IntVec2(_size.y, _size.x) : _size;
        }

        /// <summary>
        /// 获取实体占据的所有格子
        /// </summary>
        public IEnumerable<CellCoord> GetOccupiedCells(CellCoord origin, Rotation rotation)
        {
            IntVec2 rotatedSize = GetRotatedSize(rotation);
            
            for (int dz = 0; dz < rotatedSize.y; dz++)
            {
                for (int dx = 0; dx < rotatedSize.x; dx++)
                {
                    yield return new CellCoord(origin.x + dx, origin.z + dz);
                }
            }
        }

        /// <summary>
        /// 获取实体的边界
        /// </summary>
        public (CellCoord min, CellCoord max) GetBounds(CellCoord origin, Rotation rotation)
        {
            IntVec2 rotatedSize = GetRotatedSize(rotation);
            return (origin, new CellCoord(origin.x + rotatedSize.x - 1, origin.z + rotatedSize.y - 1));
        }

        /// <summary>
        /// 检查指定格子是否被该实体占据
        /// </summary>
        public bool OccupiesCell(CellCoord origin, Rotation rotation, CellCoord target)
        {
            IntVec2 rotatedSize = GetRotatedSize(rotation);
            int localX = target.x - origin.x;
            int localZ = target.z - origin.z;
            return localX >= 0 && localX < rotatedSize.x && 
                   localZ >= 0 && localZ < rotatedSize.y;
        }

        #endregion

        #region 构造函数

        public EntityDef() : base() { }

        public EntityDef(string defId) : base(defId) { }

        public EntityDef(string defId, string defName) : base(defId, defName) { }

        #endregion

        #region 工厂方法

        /// <summary>
        /// 创建建筑定义
        /// </summary>
        public static EntityDef CreateBuilding(string id, string name, IntVec2 size,
            Passability passability = Passability.Impassable,
            int maxHp = 100)
        {
            return new EntityDef
            {
                _defId = id,
                _defName = name,
                _category = EntityCategory.Building,
                _size = size,
                _passability = passability,
                _maxHitPoints = maxHp,
                _attackable = true,
                _altitudeLayer = AltitudeLayer.Building
            };
        }

        /// <summary>
        /// 创建物品定义
        /// </summary>
        public static EntityDef CreateItem(string id, string name)
        {
            return new EntityDef
            {
                _defId = id,
                _defName = name,
                _category = EntityCategory.Item,
                _size = new IntVec2(1, 1),
                _passability = Passability.Passable,
                _altitudeLayer = AltitudeLayer.Item
            };
        }

        /// <summary>
        /// 创建楼梯定义
        /// </summary>
        public static EntityDef CreateStairs(string id, string name, IntVec2 size)
        {
            return new EntityDef
            {
                _defId = id,
                _defName = name,
                _category = EntityCategory.Building,
                _size = size,
                _passability = Passability.Passable,
                _isFloorConnector = true,
                _connectorType = FloorConnectorType.Stair,
                _connectsFloors = 1,
                _traverseCost = 15,
                _altitudeLayer = AltitudeLayer.Building
            };
        }

        /// <summary>
        /// 创建电梯定义
        /// </summary>
        public static EntityDef CreateElevator(string id, string name, IntVec2 size, int floors = 1)
        {
            return new EntityDef
            {
                _defId = id,
                _defName = name,
                _category = EntityCategory.Building,
                _size = size,
                _passability = Passability.Standable,
                _isFloorConnector = true,
                _connectorType = FloorConnectorType.Elevator,
                _connectsFloors = floors,
                _traverseCost = 5,
                _connectorNeedsPower = true,
                _altitudeLayer = AltitudeLayer.Building
            };
        }

        #endregion
    }

    /// <summary>
    /// 建筑定义（EntityDef的特化）
    /// </summary>
    [Serializable]
    public class BuildingDef : EntityDef
    {
        #region 建筑特有属性

        /// <summary>
        /// 工作量
        /// </summary>
        [SerializeField]
        private int _workToBuild = 100;

        /// <summary>
        /// 建造材料列表（DefId -> 数量）
        /// </summary>
        [SerializeField]
        private List<CostEntry> _costList = new List<CostEntry>();

        /// <summary>
        /// 是否可拆除
        /// </summary>
        [SerializeField]
        private bool _canDeconstruct = true;

        /// <summary>
        /// 拆除返还材料比例（0-1）
        /// </summary>
        [SerializeField]
        private float _deconstructReturnRate = 0.75f;

        /// <summary>
        /// 是否可以维修
        /// </summary>
        [SerializeField]
        private bool _canRepair = true;

        /// <summary>
        /// 是否需要电力
        /// </summary>
        [SerializeField]
        private bool _requiresPower = false;

        /// <summary>
        /// 耗电量（如果需要电力）
        /// </summary>
        [SerializeField]
        private int _powerConsumption = 0;

        /// <summary>
        /// 产电量（如果是发电机）
        /// </summary>
        [SerializeField]
        private int _powerGeneration = 0;

        /// <summary>
        /// 美观度
        /// </summary>
        [SerializeField]
        private int _beauty = 0;

        /// <summary>
        /// 舒适度
        /// </summary>
        [SerializeField]
        private float _comfort = 0f;

        /// <summary>
        /// 是否是工作台
        /// </summary>
        [SerializeField]
        private bool _isWorkTable = false;

        /// <summary>
        /// 是否是容器
        /// </summary>
        [SerializeField]
        private bool _isContainer = false;

        /// <summary>
        /// 容器容量（如果是容器）
        /// </summary>
        [SerializeField]
        private int _containerCapacity = 0;

        #endregion

        #region 属性访问

        public int WorkToBuild => _workToBuild;
        public IReadOnlyList<CostEntry> CostList => _costList;
        public bool CanDeconstruct => _canDeconstruct;
        public float DeconstructReturnRate => _deconstructReturnRate;
        public bool CanRepair => _canRepair;
        public bool RequiresPower => _requiresPower;
        public int PowerConsumption => _powerConsumption;
        public int PowerGeneration => _powerGeneration;
        public int Beauty => _beauty;
        public float Comfort => _comfort;
        public bool IsWorkTable => _isWorkTable;
        public bool IsContainer => _isContainer;
        public int ContainerCapacity => _containerCapacity;

        /// <summary>
        /// 是否是发电机
        /// </summary>
        public bool IsPowerGenerator => _powerGeneration > 0;

        /// <summary>
        /// 净功率（产电-耗电）
        /// </summary>
        public int NetPower => _powerGeneration - _powerConsumption;

        #endregion

        #region 构造函数

        public BuildingDef() : base()
        {
            _category = EntityCategory.Building;
            _altitudeLayer = AltitudeLayer.Building;
        }

        #endregion
    }

    /// <summary>
    /// 物品定义（EntityDef的特化）
    /// </summary>
    [Serializable]
    public class ItemDef : EntityDef
    {
        #region 物品特有属性

        /// <summary>
        /// 最大堆叠数量
        /// </summary>
        [SerializeField]
        private int _maxStackCount = 100;

        /// <summary>
        /// 单个物品质量（kg）
        /// </summary>
        [SerializeField]
        private float _mass = 1f;

        /// <summary>
        /// 基础价值
        /// </summary>
        [SerializeField]
        private float _baseValue = 1f;

        /// <summary>
        /// 是否会腐烂
        /// </summary>
        [SerializeField]
        private bool _canRot = false;

        /// <summary>
        /// 腐烂速率（天）
        /// </summary>
        [SerializeField]
        private float _rotDays = 3f;

        /// <summary>
        /// 是否是资源
        /// </summary>
        [SerializeField]
        private bool _isResource = false;

        /// <summary>
        /// 是否是装备
        /// </summary>
        [SerializeField]
        private bool _isEquipment = false;

        /// <summary>
        /// 是否有品质
        /// </summary>
        [SerializeField]
        private bool _hasQuality = false;

        /// <summary>
        /// 营养值（如果是食物）
        /// </summary>
        [SerializeField]
        private float _nutrition = 0f;

        #endregion

        #region 属性访问

        public int MaxStackCount => _maxStackCount;
        public float Mass => _mass;
        public float BaseValue => _baseValue;
        public bool CanRot => _canRot;
        public float RotDays => _rotDays;
        public bool IsResource => _isResource;
        public bool IsEquipment => _isEquipment;
        public bool HasQuality => _hasQuality;
        public float Nutrition => _nutrition;

        /// <summary>
        /// 是否是食物
        /// </summary>
        public bool IsFood => _nutrition > 0;

        /// <summary>
        /// 是否可堆叠
        /// </summary>
        public bool IsStackable => _maxStackCount > 1;

        #endregion

        #region 构造函数

        public ItemDef() : base()
        {
            _category = EntityCategory.Item;
            _altitudeLayer = AltitudeLayer.Item;
            _size = new IntVec2(1, 1);
            _passability = Passability.Passable;
        }

        #endregion
    }

    /// <summary>
    /// 建造成本条目
    /// </summary>
    [Serializable]
    public struct CostEntry
    {
        /// <summary>
        /// 物品定义ID
        /// </summary>
        public string ItemDefId;

        /// <summary>
        /// 需要数量
        /// </summary>
        public int Amount;

        public CostEntry(string itemDefId, int amount)
        {
            ItemDefId = itemDefId;
            Amount = amount;
        }

        public override string ToString()
        {
            return $"{ItemDefId} x{Amount}";
        }
    }
}
