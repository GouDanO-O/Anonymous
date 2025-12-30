/*******************************************************************************
 * 文件名:    EntityTypes.cs
 * 描述:      特化的实体类型实现
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   本文件包含Entity的特化实现：
 *   - Building: 建筑实体
 *   - Item: 物品实体
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 建筑实体
    /// </summary>
    public class Building : Entity
    {
        #region 字段

        /// <summary>
        /// 建筑定义（强类型）
        /// </summary>
        private BuildingDef _buildingDef;

        /// <summary>
        /// 是否已完成建造
        /// </summary>
        private bool _constructionComplete = true;

        /// <summary>
        /// 建造进度（0-1）
        /// </summary>
        private float _constructionProgress = 1f;

        /// <summary>
        /// 电力组件
        /// </summary>
        private CompPower _compPower;

        /// <summary>
        /// 存储组件
        /// </summary>
        private CompStorage _compStorage;

        /// <summary>
        /// 开关组件
        /// </summary>
        private CompFlickable _compFlickable;

        #endregion

        #region 属性

        /// <summary>
        /// 建筑定义
        /// </summary>
        public BuildingDef BuildingDef => _buildingDef;

        /// <summary>
        /// 是否已完成建造
        /// </summary>
        public bool ConstructionComplete => _constructionComplete;

        /// <summary>
        /// 建造进度
        /// </summary>
        public float ConstructionProgress => _constructionProgress;

        /// <summary>
        /// 是否需要电力
        /// </summary>
        public bool RequiresPower => _buildingDef?.RequiresPower ?? false;

        /// <summary>
        /// 是否有电
        /// </summary>
        public bool HasPower => _compPower?.HasPower ?? !RequiresPower;

        /// <summary>
        /// 是否是发电机
        /// </summary>
        public bool IsPowerGenerator => _buildingDef?.IsPowerGenerator ?? false;

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool SwitchedOn => _compFlickable?.SwitchedOn ?? true;

        /// <summary>
        /// 是否正常工作
        /// </summary>
        public bool IsWorking => _constructionComplete && SwitchedOn && HasPower;

        /// <summary>
        /// 美观度
        /// </summary>
        public int Beauty => _buildingDef?.Beauty ?? 0;

        /// <summary>
        /// 是否是容器
        /// </summary>
        public bool IsContainer => _buildingDef?.IsContainer ?? false;

        /// <summary>
        /// 是否是工作台
        /// </summary>
        public bool IsWorkTable => _buildingDef?.IsWorkTable ?? false;

        /// <summary>
        /// 电力组件
        /// </summary>
        public CompPower PowerComp => _compPower;

        /// <summary>
        /// 存储组件
        /// </summary>
        public CompStorage StorageComp => _compStorage;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Building() : base()
        {
        }

        /// <summary>
        /// 带定义的构造函数
        /// </summary>
        public Building(BuildingDef def) : base(def)
        {
            _buildingDef = def;
        }

        /// <summary>
        /// 带基类定义的构造函数
        /// </summary>
        public Building(EntityDef def) : base(def)
        {
            _buildingDef = def as BuildingDef;
        }

        #endregion

        #region 初始化

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            // 根据定义添加组件
            if (_buildingDef != null)
            {
                // 电力组件
                if (_buildingDef.RequiresPower || _buildingDef.IsPowerGenerator)
                {
                    _compPower = AddComp<CompPower>();
                    _compPower.PowerConsumption = _buildingDef.PowerConsumption - _buildingDef.PowerGeneration;
                }

                // 存储组件
                if (_buildingDef.IsContainer && _buildingDef.ContainerCapacity > 0)
                {
                    _compStorage = AddComp<CompStorage>();
                    _compStorage.Capacity = _buildingDef.ContainerCapacity;
                }

                // 开关组件（大部分建筑都需要）
                _compFlickable = AddComp<CompFlickable>();
            }
        }

        #endregion

        #region 建造

        /// <summary>
        /// 开始建造（将建筑设为未完成状态）
        /// </summary>
        public void StartConstruction()
        {
            _constructionComplete = false;
            _constructionProgress = 0f;
        }

        /// <summary>
        /// 添加建造进度
        /// </summary>
        /// <param name="work">工作量</param>
        /// <returns>是否完成</returns>
        public bool AddConstructionWork(float work)
        {
            if (_constructionComplete)
                return true;

            float totalWork = _buildingDef?.WorkToBuild ?? 100;
            _constructionProgress += work / totalWork;

            if (_constructionProgress >= 1f)
            {
                CompleteConstruction();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 完成建造
        /// </summary>
        public void CompleteConstruction()
        {
            _constructionProgress = 1f;
            _constructionComplete = true;
            OnConstructionComplete();
        }

        /// <summary>
        /// 建造完成回调
        /// </summary>
        protected virtual void OnConstructionComplete()
        {
        }

        #endregion

        #region 拆除

        /// <summary>
        /// 拆除建筑
        /// </summary>
        /// <param name="dropItems">是否掉落材料</param>
        public void Deconstruct(bool dropItems = true)
        {
            if (!(_buildingDef?.CanDeconstruct ?? true))
                return;

            if (dropItems)
            {
                DropDeconstructItems();
            }

            Destroy();
        }

        /// <summary>
        /// 掉落拆除材料
        /// </summary>
        protected virtual void DropDeconstructItems()
        {
            if (_buildingDef == null || Floor == null)
                return;

            float returnRate = _buildingDef.DeconstructReturnRate;
            
            // TODO: 根据CostList掉落材料
            // foreach (var cost in _buildingDef.CostList)
            // {
            //     int amount = Mathf.FloorToInt(cost.Amount * returnRate);
            //     if (amount > 0)
            //     {
            //         SpawnItem(cost.ItemDefId, amount);
            //     }
            // }
        }

        #endregion

        #region 开关

        /// <summary>
        /// 切换开关
        /// </summary>
        public void ToggleSwitch()
        {
            _compFlickable?.Toggle();
        }

        /// <summary>
        /// 设置开关状态
        /// </summary>
        public void SetSwitched(bool on)
        {
            if (_compFlickable != null)
            {
                _compFlickable.WantSwitchOn = on;
                _compFlickable.DoSwitch();
            }
        }

        #endregion

        #region 重写

        public override string ToDetailedString()
        {
            return base.ToDetailedString() + 
                   $", Working={IsWorking}, Power={HasPower}, Switched={SwitchedOn}";
        }

        #endregion
    }

    /// <summary>
    /// 物品实体
    /// </summary>
    public class Item : Entity
    {
        #region 字段

        /// <summary>
        /// 物品定义（强类型）
        /// </summary>
        private ItemDef _itemDef;

        /// <summary>
        /// 堆叠数量
        /// </summary>
        private int _stackCount = 1;

        /// <summary>
        /// 品质
        /// </summary>
        private QualityLevel _quality = QualityLevel.Normal;

        /// <summary>
        /// 腐烂进度（0-1，1表示完全腐烂）
        /// </summary>
        private float _rotProgress;

        /// <summary>
        /// 是否被禁止
        /// </summary>
        private bool _forbidden;

        /// <summary>
        /// 所属者（如果在容器中）
        /// </summary>
        private Entity _holder;

        #endregion

        #region 属性

        /// <summary>
        /// 物品定义
        /// </summary>
        public ItemDef ItemDef => _itemDef;

        /// <summary>
        /// 堆叠数量
        /// </summary>
        public int StackCount
        {
            get => _stackCount;
            set => _stackCount = Mathf.Clamp(value, 0, MaxStackCount);
        }

        /// <summary>
        /// 最大堆叠数量
        /// </summary>
        public int MaxStackCount => _itemDef?.MaxStackCount ?? 1;

        /// <summary>
        /// 是否可堆叠
        /// </summary>
        public bool IsStackable => MaxStackCount > 1;

        /// <summary>
        /// 是否已满堆
        /// </summary>
        public bool IsFullStack => _stackCount >= MaxStackCount;

        /// <summary>
        /// 可添加数量
        /// </summary>
        public int FreeStackSpace => MaxStackCount - _stackCount;

        /// <summary>
        /// 品质
        /// </summary>
        public QualityLevel Quality
        {
            get => _quality;
            set => _quality = value;
        }

        /// <summary>
        /// 是否有品质
        /// </summary>
        public bool HasQuality => _itemDef?.HasQuality ?? false;

        /// <summary>
        /// 单个物品质量
        /// </summary>
        public float Mass => _itemDef?.Mass ?? 1f;

        /// <summary>
        /// 总质量
        /// </summary>
        public float TotalMass => Mass * _stackCount;

        /// <summary>
        /// 单个物品价值
        /// </summary>
        public float BaseValue => _itemDef?.BaseValue ?? 1f;

        /// <summary>
        /// 总价值（考虑品质）
        /// </summary>
        public float TotalValue => BaseValue * _stackCount * _quality.ToMultiplier();

        /// <summary>
        /// 是否会腐烂
        /// </summary>
        public bool CanRot => _itemDef?.CanRot ?? false;

        /// <summary>
        /// 腐烂进度
        /// </summary>
        public float RotProgress => _rotProgress;

        /// <summary>
        /// 是否已腐烂
        /// </summary>
        public bool IsRotten => _rotProgress >= 1f;

        /// <summary>
        /// 腐烂剩余天数
        /// </summary>
        public float RotDaysRemaining
        {
            get
            {
                if (!CanRot || IsRotten)
                    return 0;
                float totalDays = _itemDef?.RotDays ?? 3f;
                return totalDays * (1f - _rotProgress);
            }
        }

        /// <summary>
        /// 是否被禁止（不被AI使用）
        /// </summary>
        public bool Forbidden
        {
            get => _forbidden;
            set => _forbidden = value;
        }

        /// <summary>
        /// 所属者
        /// </summary>
        public Entity Holder
        {
            get => _holder;
            set => _holder = value;
        }

        /// <summary>
        /// 是否被持有
        /// </summary>
        public bool IsHeld => _holder != null;

        /// <summary>
        /// 是否是食物
        /// </summary>
        public bool IsFood => _itemDef?.IsFood ?? false;

        /// <summary>
        /// 营养值
        /// </summary>
        public float Nutrition => _itemDef?.Nutrition ?? 0;

        /// <summary>
        /// 总营养值
        /// </summary>
        public float TotalNutrition => Nutrition * _stackCount;

        /// <summary>
        /// 是否是资源
        /// </summary>
        public bool IsResource => _itemDef?.IsResource ?? false;

        /// <summary>
        /// 是否是装备
        /// </summary>
        public bool IsEquipment => _itemDef?.IsEquipment ?? false;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Item() : base()
        {
        }

        /// <summary>
        /// 带定义的构造函数
        /// </summary>
        public Item(ItemDef def) : base(def)
        {
            _itemDef = def;
        }

        /// <summary>
        /// 带基类定义的构造函数
        /// </summary>
        public Item(EntityDef def) : base(def)
        {
            _itemDef = def as ItemDef;
        }

        #endregion

        #region 堆叠操作

        /// <summary>
        /// 尝试与另一个物品堆叠
        /// </summary>
        /// <returns>实际合并的数量</returns>
        public int TryStackWith(Item other)
        {
            if (other == null || other == this)
                return 0;

            // 检查是否可以堆叠
            if (!CanStackWith(other))
                return 0;

            int toStack = Mathf.Min(other._stackCount, FreeStackSpace);
            if (toStack <= 0)
                return 0;

            _stackCount += toStack;
            other._stackCount -= toStack;

            // 如果另一个物品被清空，销毁它
            if (other._stackCount <= 0)
            {
                other.Destroy();
            }

            return toStack;
        }

        /// <summary>
        /// 检查是否可以与另一个物品堆叠
        /// </summary>
        public bool CanStackWith(Item other)
        {
            if (other == null || other == this)
                return false;

            // 必须是相同DefId
            if (DefId != other.DefId)
                return false;

            // 必须可堆叠
            if (!IsStackable)
                return false;

            // 品质必须相同（如果有品质）
            if (HasQuality && _quality != other._quality)
                return false;

            return true;
        }

        /// <summary>
        /// 分割出指定数量
        /// </summary>
        public Item SplitOff(int count)
        {
            if (count <= 0 || count >= _stackCount)
                return null;

            // 创建新物品
            var newItem = new Item(_itemDef)
            {
                _stackCount = count,
                _quality = _quality,
                _rotProgress = _rotProgress
            };

            _stackCount -= count;

            return newItem;
        }

        #endregion

        #region 腐烂

        public override void TickRare()
        {
            base.TickRare();

            // 处理腐烂
            if (CanRot && !IsRotten)
            {
                ProcessRot();
            }
        }

        /// <summary>
        /// 处理腐烂进度
        /// </summary>
        private void ProcessRot()
        {
            if (_itemDef == null)
                return;

            // 检查是否在冷藏中
            // TODO: 检查温度环境

            float rotDays = _itemDef.RotDays;
            if (rotDays <= 0)
                return;

            // 每稀有Tick（250 tick）增加腐烂进度
            // 假设1天 = 60000 tick
            float ticksPerDay = 60000f;
            float progressPerRareTick = 250f / (ticksPerDay * rotDays);
            
            _rotProgress += progressPerRareTick;

            if (_rotProgress >= 1f)
            {
                OnRotten();
            }
        }

        /// <summary>
        /// 腐烂回调
        /// </summary>
        protected virtual void OnRotten()
        {
            // TODO: 可以转换为腐烂物品或销毁
            // 简单处理：销毁
            Destroy();
        }

        #endregion

        #region 使用

        /// <summary>
        /// 使用/消耗物品
        /// </summary>
        /// <param name="amount">消耗数量</param>
        /// <returns>实际消耗数量</returns>
        public int Consume(int amount = 1)
        {
            int consumed = Mathf.Min(amount, _stackCount);
            _stackCount -= consumed;

            if (_stackCount <= 0)
            {
                Destroy();
            }

            return consumed;
        }

        #endregion

        #region 重写

        public override string Label
        {
            get
            {
                string baseName = base.Label;
                if (_stackCount > 1)
                    return $"{baseName} x{_stackCount}";
                if (HasQuality && _quality != QualityLevel.Normal)
                    return $"{_quality.ToDisplayName()}{baseName}";
                return baseName;
            }
        }

        public override string ToDetailedString()
        {
            return base.ToDetailedString() + 
                   $", Stack={_stackCount}/{MaxStackCount}, Quality={_quality}";
        }

        #endregion
    }

    /// <summary>
    /// 蓝图实体（待建造）
    /// </summary>
    public class Blueprint : Entity
    {
        /// <summary>
        /// 要建造的建筑DefId
        /// </summary>
        private string _targetDefId;

        /// <summary>
        /// 要建造的建筑定义
        /// </summary>
        public string TargetDefId => _targetDefId;

        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(string defId)
        {
            _targetDefId = defId;
        }

        /// <summary>
        /// 开始建造（转换为Frame）
        /// </summary>
        public Frame StartBuilding()
        {
            // TODO: 创建Frame并替换此Blueprint
            return null;
        }
    }

    /// <summary>
    /// 建造框架（建造中）
    /// </summary>
    public class Frame : Entity
    {
        /// <summary>
        /// 要建造的建筑DefId
        /// </summary>
        private string _targetDefId;

        /// <summary>
        /// 已投入的材料
        /// </summary>
        private Dictionary<string, int> _materials = new Dictionary<string, int>();

        /// <summary>
        /// 建造进度
        /// </summary>
        private float _progress;

        /// <summary>
        /// 目标DefId
        /// </summary>
        public string TargetDefId => _targetDefId;

        /// <summary>
        /// 建造进度
        /// </summary>
        public float Progress => _progress;

        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(string defId)
        {
            _targetDefId = defId;
        }

        /// <summary>
        /// 添加材料
        /// </summary>
        public void AddMaterial(string itemDefId, int amount)
        {
            if (!_materials.ContainsKey(itemDefId))
                _materials[itemDefId] = 0;
            _materials[itemDefId] += amount;
        }

        /// <summary>
        /// 添加建造工作
        /// </summary>
        public bool AddWork(float work)
        {
            var def = DefDatabase.GetDef<BuildingDef>(_targetDefId);
            if (def == null)
                return false;

            _progress += work / def.WorkToBuild;
            
            if (_progress >= 1f)
            {
                CompleteBuilding();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 完成建造
        /// </summary>
        private void CompleteBuilding()
        {
            // TODO: 创建Building并替换此Frame
        }
    }
}
