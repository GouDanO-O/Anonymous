/*******************************************************************************
 * 文件名:    EntityComp.cs
 * 描述:      实体组件基类，使用组合模式扩展实体功能
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   EntityComp 是组件模式的基类，用于为Entity添加各种功能。
 *   常见组件类型：
 *   - CompPower: 电力组件
 *   - CompStorage: 存储组件
 *   - CompRefuelable: 燃料组件
 *   - CompFlickable: 开关组件
 *   - CompBreakdown: 故障组件
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 实体组件基类
    /// </summary>
    public abstract class EntityComp
    {
        #region 字段

        /// <summary>
        /// 父实体
        /// </summary>
        protected Entity _parent;

        /// <summary>
        /// 是否激活
        /// </summary>
        protected bool _active = true;

        #endregion

        #region 属性

        /// <summary>
        /// 父实体
        /// </summary>
        public Entity Parent => _parent;

        /// <summary>
        /// 所在楼层
        /// </summary>
        public Floor Floor => _parent?.Floor;

        /// <summary>
        /// 所在Site
        /// </summary>
        public Site Site => _parent?.Site;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active
        {
            get => _active;
            set
            {
                if (_active != value)
                {
                    _active = value;
                    OnActiveChanged(value);
                }
            }
        }

        /// <summary>
        /// 组件名称
        /// </summary>
        public virtual string CompName => GetType().Name;

        #endregion

        #region 生命周期

        /// <summary>
        /// 设置父实体（内部方法）
        /// </summary>
        internal void SetParent(Entity parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// 生成后初始化
        /// </summary>
        public virtual void PostSpawnSetup()
        {
        }

        /// <summary>
        /// 从地图移除后
        /// </summary>
        public virtual void PostDeSpawn()
        {
        }

        /// <summary>
        /// 销毁后
        /// </summary>
        public virtual void PostDestroy()
        {
        }

        #endregion

        #region Tick更新

        /// <summary>
        /// 每Tick更新
        /// </summary>
        public virtual void CompTick()
        {
        }

        /// <summary>
        /// 稀有Tick（每250 Tick）
        /// </summary>
        public virtual void CompTickRare()
        {
        }

        /// <summary>
        /// 长周期Tick（每2000 Tick）
        /// </summary>
        public virtual void CompTickLong()
        {
        }

        #endregion

        #region 事件回调

        /// <summary>
        /// 激活状态变更
        /// </summary>
        protected virtual void OnActiveChanged(bool active)
        {
        }

        /// <summary>
        /// 位置变更
        /// </summary>
        public virtual void OnPositionChanged(CellCoord oldPos, CellCoord newPos)
        {
        }

        /// <summary>
        /// 旋转变更
        /// </summary>
        public virtual void OnRotationChanged(Rotation oldRot, Rotation newRot)
        {
        }

        /// <summary>
        /// 楼层变更
        /// </summary>
        public virtual void OnFloorChanged(Floor oldFloor, Floor newFloor)
        {
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void OnDamaged(int damage, Entity instigator)
        {
        }

        #endregion

        #region 交互

        /// <summary>
        /// 获取交互命令
        /// </summary>
        public virtual void GetInteractionCommands(List<InteractionCommand> commands)
        {
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{CompName} on {_parent}";
        }

        #endregion
    }

    /// <summary>
    /// 交互命令
    /// </summary>
    public class InteractionCommand
    {
        /// <summary>
        /// 命令ID
        /// </summary>
        public string CommandId;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Label;

        /// <summary>
        /// 图标路径
        /// </summary>
        public string IconPath;

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// 优先级（数值越小越靠前）
        /// </summary>
        public int Priority;

        /// <summary>
        /// 执行动作
        /// </summary>
        public Action Action;

        /// <summary>
        /// 禁用原因
        /// </summary>
        public string DisabledReason;
    }

    #region 常用组件实现

    /// <summary>
    /// 电力组件
    /// </summary>
    public class CompPower : EntityComp
    {
        /// <summary>
        /// 电力消耗（正数消耗，负数产生）
        /// </summary>
        private int _powerConsumption;

        /// <summary>
        /// 是否有电
        /// </summary>
        private bool _hasPower;

        /// <summary>
        /// 电力消耗量
        /// </summary>
        public int PowerConsumption
        {
            get => _powerConsumption;
            set => _powerConsumption = value;
        }

        /// <summary>
        /// 是否是发电机
        /// </summary>
        public bool IsGenerator => _powerConsumption < 0;

        /// <summary>
        /// 产电量（如果是发电机）
        /// </summary>
        public int PowerGeneration => IsGenerator ? -_powerConsumption : 0;

        /// <summary>
        /// 是否有电
        /// </summary>
        public bool HasPower
        {
            get => _hasPower;
            set
            {
                if (_hasPower != value)
                {
                    _hasPower = value;
                    OnPowerChanged(value);
                }
            }
        }

        /// <summary>
        /// 电力状态变更
        /// </summary>
        protected virtual void OnPowerChanged(bool hasPower)
        {
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            
            // 从Def获取电力消耗
            if (_parent.Def is BuildingDef buildingDef)
            {
                _powerConsumption = buildingDef.PowerConsumption - buildingDef.PowerGeneration;
            }
        }
    }

    /// <summary>
    /// 存储组件
    /// </summary>
    public class CompStorage : EntityComp
    {
        /// <summary>
        /// 容量
        /// </summary>
        private int _capacity;

        /// <summary>
        /// 存储的物品
        /// </summary>
        private List<Entity> _storedItems;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity
        {
            get => _capacity;
            set => _capacity = Mathf.Max(0, value);
        }

        /// <summary>
        /// 当前存储数量
        /// </summary>
        public int Count => _storedItems?.Count ?? 0;

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => Count >= _capacity;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// 剩余空间
        /// </summary>
        public int FreeSpace => _capacity - Count;

        /// <summary>
        /// 存储的物品（只读）
        /// </summary>
        public IReadOnlyList<Entity> StoredItems => _storedItems;

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            _storedItems = new List<Entity>();
            
            if (_parent.Def is BuildingDef buildingDef)
            {
                _capacity = buildingDef.ContainerCapacity;
            }
        }

        /// <summary>
        /// 尝试存入物品
        /// </summary>
        public bool TryStore(Entity item)
        {
            if (IsFull || item == null)
                return false;

            _storedItems.Add(item);
            item.DeSpawn();
            OnItemStored(item);
            return true;
        }

        /// <summary>
        /// 取出物品
        /// </summary>
        public Entity Retrieve()
        {
            if (IsEmpty)
                return null;

            var item = _storedItems[_storedItems.Count - 1];
            _storedItems.RemoveAt(_storedItems.Count - 1);
            OnItemRetrieved(item);
            return item;
        }

        /// <summary>
        /// 物品存入回调
        /// </summary>
        protected virtual void OnItemStored(Entity item)
        {
        }

        /// <summary>
        /// 物品取出回调
        /// </summary>
        protected virtual void OnItemRetrieved(Entity item)
        {
        }
    }

    /// <summary>
    /// 开关组件
    /// </summary>
    public class CompFlickable : EntityComp
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        private bool _switchedOn = true;

        /// <summary>
        /// 目标状态（用于延迟切换）
        /// </summary>
        private bool _wantSwitchOn = true;

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool SwitchedOn
        {
            get => _switchedOn;
            set
            {
                if (_switchedOn != value)
                {
                    _switchedOn = value;
                    OnSwitchedChanged(value);
                }
            }
        }

        /// <summary>
        /// 目标状态
        /// </summary>
        public bool WantSwitchOn
        {
            get => _wantSwitchOn;
            set => _wantSwitchOn = value;
        }

        /// <summary>
        /// 是否需要切换
        /// </summary>
        public bool NeedsSwitch => _switchedOn != _wantSwitchOn;

        /// <summary>
        /// 切换开关
        /// </summary>
        public void Toggle()
        {
            _wantSwitchOn = !_wantSwitchOn;
        }

        /// <summary>
        /// 执行切换
        /// </summary>
        public void DoSwitch()
        {
            SwitchedOn = _wantSwitchOn;
        }

        /// <summary>
        /// 开关状态变更
        /// </summary>
        protected virtual void OnSwitchedChanged(bool on)
        {
        }

        public override void GetInteractionCommands(List<InteractionCommand> commands)
        {
            commands.Add(new InteractionCommand
            {
                CommandId = "toggle",
                Label = _switchedOn ? "关闭" : "开启",
                Priority = 10,
                Action = Toggle
            });
        }
    }

    /// <summary>
    /// 燃料组件
    /// </summary>
    public class CompRefuelable : EntityComp
    {
        /// <summary>
        /// 当前燃料量
        /// </summary>
        private float _fuel;

        /// <summary>
        /// 最大燃料量
        /// </summary>
        private float _maxFuel = 100f;

        /// <summary>
        /// 每Tick消耗
        /// </summary>
        private float _consumptionPerTick = 0.001f;

        /// <summary>
        /// 燃料不足阈值
        /// </summary>
        private float _autoRefuelThreshold = 0.3f;

        /// <summary>
        /// 当前燃料
        /// </summary>
        public float Fuel
        {
            get => _fuel;
            set => _fuel = Mathf.Clamp(value, 0, _maxFuel);
        }

        /// <summary>
        /// 最大燃料
        /// </summary>
        public float MaxFuel
        {
            get => _maxFuel;
            set => _maxFuel = Mathf.Max(0, value);
        }

        /// <summary>
        /// 燃料百分比
        /// </summary>
        public float FuelPercent => _maxFuel > 0 ? _fuel / _maxFuel : 0;

        /// <summary>
        /// 是否有燃料
        /// </summary>
        public bool HasFuel => _fuel > 0;

        /// <summary>
        /// 是否需要加燃料
        /// </summary>
        public bool NeedsRefuel => FuelPercent < _autoRefuelThreshold;

        public override void CompTick()
        {
            if (_consumptionPerTick > 0 && _fuel > 0)
            {
                _fuel -= _consumptionPerTick;
                if (_fuel <= 0)
                {
                    _fuel = 0;
                    OnFuelEmpty();
                }
            }
        }

        /// <summary>
        /// 加燃料
        /// </summary>
        public float Refuel(float amount)
        {
            float oldFuel = _fuel;
            _fuel = Mathf.Min(_fuel + amount, _maxFuel);
            return _fuel - oldFuel; // 返回实际加入量
        }

        /// <summary>
        /// 燃料耗尽回调
        /// </summary>
        protected virtual void OnFuelEmpty()
        {
        }
    }

    /// <summary>
    /// 故障组件
    /// </summary>
    public class CompBreakdown : EntityComp
    {
        /// <summary>
        /// 是否故障中
        /// </summary>
        private bool _brokenDown;

        /// <summary>
        /// 故障概率（每稀有Tick）
        /// </summary>
        private float _breakdownChance = 0.001f;

        /// <summary>
        /// 是否故障
        /// </summary>
        public bool BrokenDown
        {
            get => _brokenDown;
            set
            {
                if (_brokenDown != value)
                {
                    _brokenDown = value;
                    OnBreakdownChanged(value);
                }
            }
        }

        public override void CompTickRare()
        {
            if (!_brokenDown && UnityEngine.Random.value < _breakdownChance)
            {
                BrokenDown = true;
            }
        }

        /// <summary>
        /// 修复
        /// </summary>
        public void Repair()
        {
            BrokenDown = false;
        }

        /// <summary>
        /// 故障状态变更
        /// </summary>
        protected virtual void OnBreakdownChanged(bool broken)
        {
        }

        public override void GetInteractionCommands(List<InteractionCommand> commands)
        {
            if (_brokenDown)
            {
                commands.Add(new InteractionCommand
                {
                    CommandId = "repair",
                    Label = "修复",
                    Priority = 5,
                    Action = Repair
                });
            }
        }
    }

    /// <summary>
    /// 可连接组件（连接相同类型建筑，如墙壁）
    /// </summary>
    public class CompLinkable : EntityComp
    {
        /// <summary>
        /// 连接组ID
        /// </summary>
        private string _linkGroupId;

        /// <summary>
        /// 连接遮罩（4方向）
        /// </summary>
        private int _linkMask;

        /// <summary>
        /// 连接组ID
        /// </summary>
        public string LinkGroupId
        {
            get => _linkGroupId;
            set => _linkGroupId = value;
        }

        /// <summary>
        /// 连接遮罩
        /// </summary>
        public int LinkMask => _linkMask;

        /// <summary>
        /// 更新连接状态
        /// </summary>
        public void UpdateLinks()
        {
            _linkMask = 0;
            
            if (Floor == null || string.IsNullOrEmpty(_linkGroupId))
                return;

            var pos = _parent.Position;

            // 检查四方向
            // 北
            if (HasLinkableNeighbor(pos + CellCoord.North))
                _linkMask |= 1;
            // 东
            if (HasLinkableNeighbor(pos + CellCoord.East))
                _linkMask |= 2;
            // 南
            if (HasLinkableNeighbor(pos + CellCoord.South))
                _linkMask |= 4;
            // 西
            if (HasLinkableNeighbor(pos + CellCoord.West))
                _linkMask |= 8;
        }

        private bool HasLinkableNeighbor(CellCoord neighborPos)
        {
            // TODO: 从EntityGrid查询邻居实体
            // var neighbor = Floor.EntityGrid.GetEntityAt(neighborPos);
            // if (neighbor != null && neighbor.TryGetComp<CompLinkable>(out var comp))
            // {
            //     return comp.LinkGroupId == _linkGroupId;
            // }
            return false;
        }

        public override void OnPositionChanged(CellCoord oldPos, CellCoord newPos)
        {
            UpdateLinks();
        }
    }

    #endregion
}
