/*******************************************************************************
 * 文件名:    TileDef.cs
 * 描述:      Tile定义基类，所有Tile层定义的基类
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   TileDef 是六层Tile系统中所有Tile类型定义的基类。
 *   包含通用的Tile属性如渲染信息、承重、通行性等。
 ******************************************************************************/

using System;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// Tile定义基类
    /// </summary>
    [Serializable]
    public abstract class TileDef : DefBase
    {
        #region 渲染相关

        /// <summary>
        /// 贴图/Sprite路径（或资源ID）
        /// </summary>
        [SerializeField]
        internal string _texturePath;

        /// <summary>
        /// 图集名称（如果使用图集）
        /// </summary>
        [SerializeField]
        protected string _atlasName;

        /// <summary>
        /// Sprite名称（图集中的Sprite）
        /// </summary>
        [SerializeField]
        protected string _spriteName;

        /// <summary>
        /// 颜色叠加
        /// </summary>
        [SerializeField]
        protected Color _color = Color.white;

        /// <summary>
        /// 渲染排序偏移
        /// </summary>
        [SerializeField]
        protected int _renderOrder;

        /// <summary>
        /// 是否支持边缘混合
        /// </summary>
        [SerializeField]
        protected bool _supportsEdgeBlending;

        /// <summary>
        /// 边缘混合组（相同组的Tile之间进行边缘混合）
        /// </summary>
        [SerializeField]
        protected string _edgeBlendGroup;

        #endregion

        #region 属性访问

        /// <summary>
        /// 贴图路径
        /// </summary>
        public string TexturePath => _texturePath;

        /// <summary>
        /// 图集名称
        /// </summary>
        public string AtlasName => _atlasName;

        /// <summary>
        /// Sprite名称
        /// </summary>
        public string SpriteName => _spriteName;

        /// <summary>
        /// 颜色
        /// </summary>
        public Color TileColor => _color;

        /// <summary>
        /// 渲染排序偏移
        /// </summary>
        public int RenderOrder => _renderOrder;

        /// <summary>
        /// 是否支持边缘混合
        /// </summary>
        public bool SupportsEdgeBlending => _supportsEdgeBlending;

        /// <summary>
        /// 边缘混合组
        /// </summary>
        public string EdgeBlendGroup => _edgeBlendGroup;

        /// <summary>
        /// 所属Tile层级（子类实现）
        /// </summary>
        public abstract TileLayer Layer { get; }

        #endregion

        #region 缓存的资源引用

        /// <summary>
        /// 缓存的Sprite引用
        /// </summary>
        [NonSerialized]
        protected Sprite _cachedSprite;

        /// <summary>
        /// 获取Sprite（带缓存）
        /// </summary>
        public virtual Sprite GetSprite()
        {
            if (_cachedSprite == null && !string.IsNullOrEmpty(_texturePath))
            {
                // TODO: 通过资源管理器加载
                // _cachedSprite = ResourceManager.LoadSprite(_texturePath);
            }
            return _cachedSprite;
        }

        /// <summary>
        /// 设置Sprite（运行时注入）
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            _cachedSprite = sprite;
        }

        #endregion

        #region 构造函数

        protected TileDef() : base() { }

        protected TileDef(string defId) : base(defId) { }

        protected TileDef(string defId, string defName) : base(defId, defName) { }

        #endregion
    }

    /// <summary>
    /// 地形定义（第0层）
    /// 自然地形：土、石、水、岩浆等
    /// </summary>
    [Serializable]
    public class TerrainDef : TileDef
    {
        #region 地形特有属性

        /// <summary>
        /// 地形类型
        /// </summary>
        [SerializeField]
        private TerrainType _terrainType = TerrainType.Normal;

        /// <summary>
        /// 承重等级
        /// </summary>
        [SerializeField]
        private BearingCapacity _bearingCapacity = BearingCapacity.Medium;

        /// <summary>
        /// 可通行性
        /// </summary>
        [SerializeField]
        private Passability _passability = Passability.Passable;

        /// <summary>
        /// 移动代价（寻路权重，越高移动越慢）
        /// </summary>
        [SerializeField]
        private int _pathCost = 1;

        /// <summary>
        /// 肥沃度（影响植物生长，0-2，1为标准）
        /// </summary>
        [SerializeField]
        private float _fertility = 1f;

        /// <summary>
        /// 是否可挖掘
        /// </summary>
        [SerializeField]
        private bool _canMine;

        /// <summary>
        /// 挖掘后变成的地形DefId
        /// </summary>
        [SerializeField]
        private string _minedTerrainId;

        /// <summary>
        /// 挖掘产出物品DefId
        /// </summary>
        [SerializeField]
        private string _mineYieldId;

        /// <summary>
        /// 挖掘产出数量范围
        /// </summary>
        [SerializeField]
        private Vector2Int _mineYieldRange = new Vector2Int(1, 3);

        /// <summary>
        /// 是否积雪
        /// </summary>
        [SerializeField]
        private bool _holdSnow = true;

        /// <summary>
        /// 是否留脚印
        /// </summary>
        [SerializeField]
        private bool _takeFootprints = true;

        /// <summary>
        /// 是否是水域
        /// </summary>
        [SerializeField]
        private bool _isWater;

        /// <summary>
        /// 水深（如果是水域）
        /// </summary>
        [SerializeField]
        private float _waterDepth;

        /// <summary>
        /// 是否造成伤害（如岩浆）
        /// </summary>
        [SerializeField]
        private bool _causesDamage;

        /// <summary>
        /// 伤害值（每秒）
        /// </summary>
        [SerializeField]
        private float _damagePerSecond;

        #endregion

        #region 属性访问

        public override TileLayer Layer => TileLayer.Terrain;

        public TerrainType TerrainType => _terrainType;
        public BearingCapacity BearingCapacity => _bearingCapacity;
        public Passability Passability => _passability;
        public int PathCost => _pathCost;
        public float Fertility => _fertility;
        public bool CanMine => _canMine;
        public string MinedTerrainId => _minedTerrainId;
        public string MineYieldId => _mineYieldId;
        public Vector2Int MineYieldRange => _mineYieldRange;
        public bool HoldSnow => _holdSnow;
        public bool TakeFootprints => _takeFootprints;
        public bool IsWater => _isWater;
        public float WaterDepth => _waterDepth;
        public bool CausesDamage => _causesDamage;
        public float DamagePerSecond => _damagePerSecond;

        /// <summary>
        /// 是否可以建造
        /// </summary>
        public bool CanBuild => _bearingCapacity != BearingCapacity.None && 
                                _passability != Passability.Impassable;

        /// <summary>
        /// 是否可以种植
        /// </summary>
        public bool CanPlant => _fertility > 0 && 
                                _bearingCapacity != BearingCapacity.None &&
                                !_isWater;

        #endregion

        #region 引用解析

        /// <summary>
        /// 挖掘后的地形Def引用（运行时解析）
        /// </summary>
        [NonSerialized]
        private TerrainDef _minedTerrain;

        public TerrainDef MinedTerrain => _minedTerrain;

        protected override void ResolveReferences()
        {
            base.ResolveReferences();
            
            if (!string.IsNullOrEmpty(_minedTerrainId))
            {
                _minedTerrain = DefDatabase.GetDef<TerrainDef>(_minedTerrainId);
            }
        }

        #endregion

        #region 构造函数

        public TerrainDef() : base() { }

        public TerrainDef(string defId, string defName) : base(defId, defName) { }

        /// <summary>
        /// 快速创建（用于测试）
        /// </summary>
        public static TerrainDef Create(string id, string name, 
            BearingCapacity bearing = BearingCapacity.Medium,
            Passability passability = Passability.Passable,
            int pathCost = 1)
        {
            return new TerrainDef
            {
                _defId = id,
                _defName = name,
                _bearingCapacity = bearing,
                _passability = passability,
                _pathCost = pathCost
            };
        }

        #endregion
    }

    /// <summary>
    /// 地基定义（第1层）
    /// 影响建筑承重和稳定性
    /// </summary>
    [Serializable]
    public class FoundationDef : TileDef
    {
        #region 地基特有属性

        /// <summary>
        /// 提供的承重等级
        /// </summary>
        [SerializeField]
        private BearingCapacity _providedCapacity = BearingCapacity.Medium;

        /// <summary>
        /// 需要的地形承重等级
        /// </summary>
        [SerializeField]
        private BearingCapacity _requiredTerrainCapacity = BearingCapacity.Light;

        /// <summary>
        /// 建造工作量
        /// </summary>
        [SerializeField]
        private int _workToBuild = 100;

        /// <summary>
        /// 建造所需材料DefId
        /// </summary>
        [SerializeField]
        private string _costItemId;

        /// <summary>
        /// 建造所需材料数量
        /// </summary>
        [SerializeField]
        private int _costAmount = 1;

        /// <summary>
        /// 是否可拆除
        /// </summary>
        [SerializeField]
        private bool _canDeconstruct = true;

        #endregion

        #region 属性访问

        public override TileLayer Layer => TileLayer.Foundation;

        public BearingCapacity ProvidedCapacity => _providedCapacity;
        public BearingCapacity RequiredTerrainCapacity => _requiredTerrainCapacity;
        public int WorkToBuild => _workToBuild;
        public string CostItemId => _costItemId;
        public int CostAmount => _costAmount;
        public bool CanDeconstruct => _canDeconstruct;

        #endregion

        #region 构造函数

        public FoundationDef() : base() { }

        public static FoundationDef Create(string id, string name, 
            BearingCapacity provided = BearingCapacity.Medium)
        {
            return new FoundationDef
            {
                _defId = id,
                _defName = name,
                _providedCapacity = provided
            };
        }

        #endregion
    }

    /// <summary>
    /// 地板定义（第2层）
    /// 人造地板：木地板、石砖、金属板等
    /// </summary>
    [Serializable]
    public class FloorDef : TileDef
    {
        #region 地板特有属性

        /// <summary>
        /// 提供的承重等级
        /// </summary>
        [SerializeField]
        private BearingCapacity _providedCapacity = BearingCapacity.Medium;

        /// <summary>
        /// 需要的地基/地形承重
        /// </summary>
        [SerializeField]
        private BearingCapacity _requiredCapacity = BearingCapacity.Light;

        /// <summary>
        /// 移动代价修正（叠加到地形上）
        /// </summary>
        [SerializeField]
        private int _pathCostModifier = 0;

        /// <summary>
        /// 美观度修正
        /// </summary>
        [SerializeField]
        private int _beauty = 0;

        /// <summary>
        /// 洁净度修正
        /// </summary>
        [SerializeField]
        private int _cleanliness = 0;

        /// <summary>
        /// 舒适度
        /// </summary>
        [SerializeField]
        private float _comfort = 0f;

        /// <summary>
        /// 工作量
        /// </summary>
        [SerializeField]
        private int _workToBuild = 50;

        /// <summary>
        /// 建造材料DefId
        /// </summary>
        [SerializeField]
        private string _costItemId;

        /// <summary>
        /// 材料数量
        /// </summary>
        [SerializeField]
        private int _costAmount = 1;

        /// <summary>
        /// 是否可拆除
        /// </summary>
        [SerializeField]
        private bool _canDeconstruct = true;

        /// <summary>
        /// 是否隐藏电线
        /// </summary>
        [SerializeField]
        private bool _hideConduits = true;

        /// <summary>
        /// 是否阻止植物生长
        /// </summary>
        [SerializeField]
        private bool _preventPlantGrowth = true;

        #endregion

        #region 属性访问

        public override TileLayer Layer => TileLayer.Floor;

        public BearingCapacity ProvidedCapacity => _providedCapacity;
        public BearingCapacity RequiredCapacity => _requiredCapacity;
        public int PathCostModifier => _pathCostModifier;
        public int Beauty => _beauty;
        public int Cleanliness => _cleanliness;
        public float Comfort => _comfort;
        public int WorkToBuild => _workToBuild;
        public string CostItemId => _costItemId;
        public int CostAmount => _costAmount;
        public bool CanDeconstruct => _canDeconstruct;
        public bool HideConduits => _hideConduits;
        public bool PreventPlantGrowth => _preventPlantGrowth;

        #endregion

        #region 构造函数

        public FloorDef() : base() { }

        public static FloorDef Create(string id, string name,
            BearingCapacity provided = BearingCapacity.Medium,
            int beauty = 0)
        {
            return new FloorDef
            {
                _defId = id,
                _defName = name,
                _providedCapacity = provided,
                _beauty = beauty
            };
        }

        #endregion
    }

    /// <summary>
    /// 覆盖物定义（第3层）
    /// 血迹、污渍、积雪、脚印等临时覆盖物
    /// </summary>
    [Serializable]
    public class CoverDef : TileDef
    {
        #region 覆盖物特有属性

        /// <summary>
        /// 是否是污物
        /// </summary>
        [SerializeField]
        private bool _isFilth = true;

        /// <summary>
        /// 美观度修正
        /// </summary>
        [SerializeField]
        private int _beauty = -1;

        /// <summary>
        /// 洁净度修正
        /// </summary>
        [SerializeField]
        private int _cleanliness = -1;

        /// <summary>
        /// 自然消散时间（秒，0表示不消散）
        /// </summary>
        [SerializeField]
        private float _decayTime = 0f;

        /// <summary>
        /// 是否可清扫
        /// </summary>
        [SerializeField]
        private bool _canClean = true;

        /// <summary>
        /// 清扫工作量
        /// </summary>
        [SerializeField]
        private int _workToClean = 10;

        /// <summary>
        /// 最大堆叠层数
        /// </summary>
        [SerializeField]
        private int _maxStackCount = 5;

        /// <summary>
        /// 移动代价修正
        /// </summary>
        [SerializeField]
        private int _pathCostModifier = 0;

        /// <summary>
        /// 是否造成减速
        /// </summary>
        [SerializeField]
        private float _moveSpeedModifier = 1f;

        #endregion

        #region 属性访问

        public override TileLayer Layer => TileLayer.Cover;

        public bool IsFilth => _isFilth;
        public int Beauty => _beauty;
        public int Cleanliness => _cleanliness;
        public float DecayTime => _decayTime;
        public bool CanClean => _canClean;
        public int WorkToClean => _workToClean;
        public int MaxStackCount => _maxStackCount;
        public int PathCostModifier => _pathCostModifier;
        public float MoveSpeedModifier => _moveSpeedModifier;

        /// <summary>
        /// 是否会自然消散
        /// </summary>
        public bool WillDecay => _decayTime > 0;

        #endregion

        #region 构造函数

        public CoverDef() : base() { }

        public static CoverDef Create(string id, string name, 
            bool isFilth = true, int beauty = -1)
        {
            return new CoverDef
            {
                _defId = id,
                _defName = name,
                _isFilth = isFilth,
                _beauty = beauty
            };
        }

        #endregion
    }

    /// <summary>
    /// 墙壁定义（第4层）
    /// 墙、门、窗等垂直结构
    /// </summary>
    [Serializable]
    public class WallDef : TileDef
    {
        #region 墙壁特有属性

        /// <summary>
        /// 墙壁类型
        /// </summary>
        [SerializeField]
        private WallType _wallType = WallType.Solid;

        /// <summary>
        /// 可通行性
        /// </summary>
        [SerializeField]
        private Passability _passability = Passability.Impassable;

        /// <summary>
        /// 最大生命值
        /// </summary>
        [SerializeField]
        private int _maxHitPoints = 100;

        /// <summary>
        /// 是否阻挡视线
        /// </summary>
        [SerializeField]
        private bool _blockSight = true;

        /// <summary>
        /// 是否阻挡光线
        /// </summary>
        [SerializeField]
        private bool _blockLight = true;

        /// <summary>
        /// 是否支撑屋顶
        /// </summary>
        [SerializeField]
        private bool _holdsRoof = true;

        /// <summary>
        /// 覆盖百分比（用于掩体计算，0-1）
        /// </summary>
        [SerializeField]
        private float _coverPercent = 0.75f;

        /// <summary>
        /// 美观度
        /// </summary>
        [SerializeField]
        private int _beauty = 0;

        /// <summary>
        /// 工作量
        /// </summary>
        [SerializeField]
        private int _workToBuild = 150;

        /// <summary>
        /// 建造材料DefId
        /// </summary>
        [SerializeField]
        private string _costItemId;

        /// <summary>
        /// 材料数量
        /// </summary>
        [SerializeField]
        private int _costAmount = 5;

        /// <summary>
        /// 是否可拆除
        /// </summary>
        [SerializeField]
        private bool _canDeconstruct = true;

        /// <summary>
        /// 是否是天然墙（岩石墙）
        /// </summary>
        [SerializeField]
        private bool _isNatural;

        /// <summary>
        /// 连接同类型墙壁的图形
        /// </summary>
        [SerializeField]
        private bool _linkWithSameType = true;

        #endregion

        #region 属性访问

        public override TileLayer Layer => TileLayer.Wall;

        public WallType WallType => _wallType;
        public Passability Passability => _passability;
        public int MaxHitPoints => _maxHitPoints;
        public bool BlockSight => _blockSight;
        public bool BlockLight => _blockLight;
        public bool HoldsRoof => _holdsRoof;
        public float CoverPercent => _coverPercent;
        public int Beauty => _beauty;
        public int WorkToBuild => _workToBuild;
        public string CostItemId => _costItemId;
        public int CostAmount => _costAmount;
        public bool CanDeconstruct => _canDeconstruct;
        public bool IsNatural => _isNatural;
        public bool LinkWithSameType => _linkWithSameType;

        /// <summary>
        /// 是否是门类型
        /// </summary>
        public bool IsDoor => _wallType == WallType.Door;

        /// <summary>
        /// 是否完全阻挡
        /// </summary>
        public bool IsFullBlock => _passability == Passability.Impassable && _blockSight;

        #endregion

        #region 构造函数

        public WallDef() : base() { }

        public static WallDef Create(string id, string name,
            WallType type = WallType.Solid,
            int maxHp = 100)
        {
            return new WallDef
            {
                _defId = id,
                _defName = name,
                _wallType = type,
                _maxHitPoints = maxHp
            };
        }

        #endregion
    }

    /// <summary>
    /// 屋顶定义（第5层）
    /// </summary>
    [Serializable]
    public class RoofDef : TileDef
    {
        #region 屋顶特有属性

        /// <summary>
        /// 屋顶类型
        /// </summary>
        [SerializeField]
        private RoofType _roofType = RoofType.Constructed;

        /// <summary>
        /// 是否透光
        /// </summary>
        [SerializeField]
        private bool _transparent;

        /// <summary>
        /// 是否防雨雪
        /// </summary>
        [SerializeField]
        private bool _blockWeather = true;

        /// <summary>
        /// 是否可拆除
        /// </summary>
        [SerializeField]
        private bool _canRemove = true;

        /// <summary>
        /// 需要支撑（墙/柱子）
        /// </summary>
        [SerializeField]
        private bool _needsSupport = true;

        /// <summary>
        /// 最大无支撑跨度
        /// </summary>
        [SerializeField]
        private int _maxUnsupportedSpan = 6;

        /// <summary>
        /// 隔热系数（影响室内温度）
        /// </summary>
        [SerializeField]
        private float _thermalInsulation = 1f;

        /// <summary>
        /// 工作量
        /// </summary>
        [SerializeField]
        private int _workToBuild = 50;

        /// <summary>
        /// 建造材料
        /// </summary>
        [SerializeField]
        private string _costItemId;

        /// <summary>
        /// 材料数量
        /// </summary>
        [SerializeField]
        private int _costAmount = 1;

        #endregion

        #region 属性访问

        public override TileLayer Layer => TileLayer.Roof;

        public RoofType RoofType => _roofType;
        public bool Transparent => _transparent;
        public bool BlockWeather => _blockWeather;
        public bool CanRemove => _canRemove;
        public bool NeedsSupport => _needsSupport;
        public int MaxUnsupportedSpan => _maxUnsupportedSpan;
        public float ThermalInsulation => _thermalInsulation;
        public int WorkToBuild => _workToBuild;
        public string CostItemId => _costItemId;
        public int CostAmount => _costAmount;

        /// <summary>
        /// 是否是天然屋顶（岩石顶）
        /// </summary>
        public bool IsNatural => _roofType == RoofType.RockThin || _roofType == RoofType.RockThick;

        /// <summary>
        /// 是否是厚岩顶（不可拆除）
        /// </summary>
        public bool IsThickRock => _roofType == RoofType.RockThick;

        #endregion

        #region 构造函数

        public RoofDef() : base() { }

        public static RoofDef Create(string id, string name,
            RoofType type = RoofType.Constructed)
        {
            return new RoofDef
            {
                _defId = id,
                _defName = name,
                _roofType = type
            };
        }

        #endregion
    }
}
