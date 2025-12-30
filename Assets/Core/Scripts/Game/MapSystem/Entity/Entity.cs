/*******************************************************************************
 * 文件名:    Entity.cs
 * 描述:      实体基类，所有游戏实体的基类
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Entity 是游戏中所有动态对象的基类，包括：
 *   - 建筑（Building）
 *   - 物品（Item）
 *   - 生物（Pawn）
 *   - 植物（Plant）
 *   - 投射物（Projectile）
 *   - 特效（Mote）
 *   
 *   使用组件模式扩展功能。
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public class Entity
    {
        #region 字段

        /// <summary>
        /// 实体唯一ID
        /// </summary>
        private int _entityId;

        /// <summary>
        /// 实体定义
        /// </summary>
        private EntityDef _def;

        /// <summary>
        /// 所在楼层
        /// </summary>
        private Floor _floor;

        /// <summary>
        /// 位置（左下角锚点）
        /// </summary>
        private CellCoord _position;

        /// <summary>
        /// 旋转
        /// </summary>
        private Rotation _rotation;

        /// <summary>
        /// 生命值
        /// </summary>
        private int _hitPoints;

        /// <summary>
        /// 组件列表
        /// </summary>
        private List<EntityComp> _comps;

        /// <summary>
        /// 组件字典（类型 -> 组件）
        /// </summary>
        private Dictionary<Type, EntityComp> _compsByType;

        /// <summary>
        /// 是否已生成（在地图上）
        /// </summary>
        private bool _spawned;

        /// <summary>
        /// 是否已销毁
        /// </summary>
        private bool _destroyed;

        /// <summary>
        /// 创建时的游戏Tick
        /// </summary>
        private long _createdTick;

        /// <summary>
        /// 自定义数据
        /// </summary>
        private Dictionary<string, object> _customData;

        #endregion

        #region 属性

        /// <summary>
        /// 实体ID
        /// </summary>
        public int EntityId => _entityId;

        /// <summary>
        /// 实体定义
        /// </summary>
        public EntityDef Def => _def;

        /// <summary>
        /// 定义ID
        /// </summary>
        public string DefId => _def?.DefId;

        /// <summary>
        /// 显示名称
        /// </summary>
        public virtual string Label => _def?.DefName ?? "Unknown";

        /// <summary>
        /// 所在楼层
        /// </summary>
        public Floor Floor => _floor;

        /// <summary>
        /// 所在Site
        /// </summary>
        public Site Site => _floor?.ParentSite;

        /// <summary>
        /// 楼层索引
        /// </summary>
        public int FloorIndex => _floor?.FloorIndex ?? 0;

        /// <summary>
        /// 位置（左下角锚点）
        /// </summary>
        public CellCoord Position
        {
            get => _position;
            set => SetPosition(value);
        }

        /// <summary>
        /// 全局坐标
        /// </summary>
        public GlobalCoord GlobalPosition => new GlobalCoord(_position, FloorIndex);

        /// <summary>
        /// 旋转
        /// </summary>
        public Rotation Rotation
        {
            get => _rotation;
            set => SetRotation(value);
        }

        /// <summary>
        /// 尺寸（考虑旋转）
        /// </summary>
        public IntVec2 Size => _def?.GetRotatedSize(_rotation) ?? new IntVec2(1, 1);

        /// <summary>
        /// 是否是单格实体
        /// </summary>
        public bool IsSingleCell => Size.x == 1 && Size.y == 1;

        /// <summary>
        /// 实体分类
        /// </summary>
        public EntityCategory Category => _def?.Category ?? EntityCategory.None;

        /// <summary>
        /// 生命值
        /// </summary>
        public int HitPoints
        {
            get => _hitPoints;
            set => _hitPoints = Mathf.Clamp(value, 0, MaxHitPoints);
        }

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHitPoints => _def?.MaxHitPoints ?? 0;

        /// <summary>
        /// 生命值百分比
        /// </summary>
        public float HitPointsPercent => MaxHitPoints > 0 ? (float)_hitPoints / MaxHitPoints : 1f;

        /// <summary>
        /// 是否已损坏
        /// </summary>
        public bool IsDamaged => _hitPoints < MaxHitPoints;

        /// <summary>
        /// 是否已生成
        /// </summary>
        public bool Spawned => _spawned;

        /// <summary>
        /// 是否已销毁
        /// </summary>
        public bool Destroyed => _destroyed;

        /// <summary>
        /// 创建Tick
        /// </summary>
        public long CreatedTick => _createdTick;

        /// <summary>
        /// 存在时间（Tick数）
        /// </summary>
        public long AgeTicks => Site != null ? Site.GameTick - _createdTick : 0;

        /// <summary>
        /// 组件列表
        /// </summary>
        public IReadOnlyList<EntityComp> Comps => _comps;

        #endregion

        #region 派生属性

        /// <summary>
        /// 通行性
        /// </summary>
        public virtual Passability Passability => _def?.Passability ?? Passability.Passable;

        /// <summary>
        /// 是否阻挡移动
        /// </summary>
        public bool BlocksMovement => Passability == Passability.Impassable;

        /// <summary>
        /// 寻路代价
        /// </summary>
        public virtual int PathCost => _def?.PathCost ?? 0;

        /// <summary>
        /// 填充百分比（掩体）
        /// </summary>
        public virtual float FillPercent => _def?.FillPercent ?? 0f;

        /// <summary>
        /// 是否可攻击
        /// </summary>
        public bool Attackable => _def?.Attackable ?? false;

        /// <summary>
        /// 是否可燃
        /// </summary>
        public bool IsFlammable => (_def?.Flammability ?? 0) > 0;

        /// <summary>
        /// 是否是楼层连接器
        /// </summary>
        public bool IsFloorConnector => _def?.IsFloorConnector ?? false;

        /// <summary>
        /// 渲染层级
        /// </summary>
        public AltitudeLayer AltitudeLayer => _def?.AltitudeLayer ?? AltitudeLayer.Building;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Entity()
        {
            _comps = new List<EntityComp>();
            _compsByType = new Dictionary<Type, EntityComp>();
        }

        /// <summary>
        /// 带定义的构造函数
        /// </summary>
        public Entity(EntityDef def) : this()
        {
            _def = def;
            _hitPoints = def?.MaxHitPoints ?? 0;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 设置实体ID（由EntityManager调用）
        /// </summary>
        internal void SetEntityId(int id)
        {
            _entityId = id;
        }

        /// <summary>
        /// 设置定义
        /// </summary>
        public void SetDef(EntityDef def)
        {
            _def = def;
            _hitPoints = def?.MaxHitPoints ?? 0;
        }

        /// <summary>
        /// 初始化组件（根据Def配置）
        /// </summary>
        protected virtual void InitializeComponents()
        {
            if (_def == null)
                return;

            // TODO: 根据Def中的CompDefIds创建组件
            // foreach (var compDefId in _def.CompDefIds)
            // {
            //     var comp = CreateCompFromDef(compDefId);
            //     if (comp != null)
            //         AddComp(comp);
            // }
        }

        #endregion

        #region 生成/销毁

        /// <summary>
        /// 生成到地图
        /// </summary>
        public virtual void SpawnSetup(Floor floor, CellCoord position, Rotation rotation)
        {
            if (_spawned)
            {
                Debug.LogWarning($"[Entity] {this} is already spawned");
                return;
            }

            _floor = floor;
            _position = position;
            _rotation = rotation;
            _createdTick = Site?.GameTick ?? 0;
            _spawned = true;

            // 初始化组件
            InitializeComponents();

            // 通知所有组件
            foreach (var comp in _comps)
            {
                comp.PostSpawnSetup();
            }

            // 注册到楼层
            // TODO: floor.EntityGrid.Register(this);

            OnSpawned();
        }

        /// <summary>
        /// 生成后回调
        /// </summary>
        protected virtual void OnSpawned()
        {
        }

        /// <summary>
        /// 从地图移除
        /// </summary>
        public virtual void DeSpawn()
        {
            if (!_spawned)
                return;

            // 通知组件
            foreach (var comp in _comps)
            {
                comp.PostDeSpawn();
            }

            // 从楼层注销
            // TODO: _floor?.EntityGrid.Unregister(this);

            OnDeSpawned();

            _floor = null;
            _spawned = false;
        }

        /// <summary>
        /// 移除后回调
        /// </summary>
        protected virtual void OnDeSpawned()
        {
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        public virtual void Destroy()
        {
            if (_destroyed)
                return;

            if (_spawned)
            {
                DeSpawn();
            }

            // 销毁组件
            foreach (var comp in _comps)
            {
                comp.PostDestroy();
            }
            _comps.Clear();
            _compsByType.Clear();

            _destroyed = true;

            OnDestroyed();
        }

        /// <summary>
        /// 销毁后回调
        /// </summary>
        protected virtual void OnDestroyed()
        {
        }

        #endregion

        #region 位置/旋转

        /// <summary>
        /// 设置位置
        /// </summary>
        public virtual void SetPosition(CellCoord newPosition)
        {
            if (_position == newPosition)
                return;

            var oldPosition = _position;
            _position = newPosition;

            if (_spawned)
            {
                OnPositionChanged(oldPosition, newPosition);
            }
        }

        /// <summary>
        /// 位置变更回调
        /// </summary>
        protected virtual void OnPositionChanged(CellCoord oldPos, CellCoord newPos)
        {
            // 更新空间索引
            // TODO: _floor?.EntityGrid.UpdatePosition(this, oldPos, newPos);

            // 通知组件
            foreach (var comp in _comps)
            {
                comp.OnPositionChanged(oldPos, newPos);
            }
        }

        /// <summary>
        /// 设置旋转
        /// </summary>
        public virtual void SetRotation(Rotation newRotation)
        {
            if (_rotation == newRotation)
                return;

            var oldRotation = _rotation;
            _rotation = newRotation;

            if (_spawned)
            {
                OnRotationChanged(oldRotation, newRotation);
            }
        }

        /// <summary>
        /// 旋转变更回调
        /// </summary>
        protected virtual void OnRotationChanged(Rotation oldRot, Rotation newRot)
        {
            // 通知组件
            foreach (var comp in _comps)
            {
                comp.OnRotationChanged(oldRot, newRot);
            }
        }

        /// <summary>
        /// 移动到新楼层
        /// </summary>
        public virtual void MoveToFloor(Floor newFloor, CellCoord newPosition)
        {
            if (_floor == newFloor && _position == newPosition)
                return;

            var oldFloor = _floor;
            var oldPosition = _position;

            // 从旧楼层注销
            // TODO: oldFloor?.EntityGrid.Unregister(this);

            _floor = newFloor;
            _position = newPosition;

            // 注册到新楼层
            // TODO: newFloor?.EntityGrid.Register(this);

            OnFloorChanged(oldFloor, newFloor);
        }

        /// <summary>
        /// 楼层变更回调
        /// </summary>
        protected virtual void OnFloorChanged(Floor oldFloor, Floor newFloor)
        {
            foreach (var comp in _comps)
            {
                comp.OnFloorChanged(oldFloor, newFloor);
            }
        }

        #endregion

        #region 占据格子

        /// <summary>
        /// 获取占据的所有格子
        /// </summary>
        public IEnumerable<CellCoord> OccupiedCells()
        {
            if (_def == null)
            {
                yield return _position;
                yield break;
            }

            foreach (var cell in _def.GetOccupiedCells(_position, _rotation))
            {
                yield return cell;
            }
        }

        /// <summary>
        /// 检查是否占据指定格子
        /// </summary>
        public bool OccupiesCell(CellCoord cell)
        {
            if (_def == null)
                return _position == cell;

            return _def.OccupiesCell(_position, _rotation, cell);
        }

        /// <summary>
        /// 获取边界
        /// </summary>
        public (CellCoord min, CellCoord max) GetBounds()
        {
            if (_def == null)
                return (_position, _position);

            return _def.GetBounds(_position, _rotation);
        }

        /// <summary>
        /// 获取中心位置
        /// </summary>
        public CellCoord GetCenter()
        {
            var size = Size;
            return new CellCoord(
                _position.x + size.x / 2,
                _position.z + size.y / 2
            );
        }

        /// <summary>
        /// 获取世界位置（中心）
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            float cellSize = Site?.CellSize ?? 1f;
            float floorHeight = Site?.FloorHeight ?? 3f;
            var size = Size;

            return new Vector3(
                (_position.x + size.x * 0.5f) * cellSize,
                FloorIndex * floorHeight,
                (_position.z + size.y * 0.5f) * cellSize
            );
        }

        #endregion

        #region 组件系统

        /// <summary>
        /// 添加组件
        /// </summary>
        public T AddComp<T>() where T : EntityComp, new()
        {
            var comp = new T();
            AddComp(comp);
            return comp;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public void AddComp(EntityComp comp)
        {
            if (comp == null)
                return;

            var type = comp.GetType();
            if (_compsByType.ContainsKey(type))
            {
                Debug.LogWarning($"[Entity] Component of type {type.Name} already exists");
                return;
            }

            comp.SetParent(this);
            _comps.Add(comp);
            _compsByType[type] = comp;

            if (_spawned)
            {
                comp.PostSpawnSetup();
            }
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        public T GetComp<T>() where T : EntityComp
        {
            if (_compsByType.TryGetValue(typeof(T), out var comp))
            {
                return comp as T;
            }
            return null;
        }

        /// <summary>
        /// 尝试获取组件
        /// </summary>
        public bool TryGetComp<T>(out T comp) where T : EntityComp
        {
            comp = GetComp<T>();
            return comp != null;
        }

        /// <summary>
        /// 检查是否有组件
        /// </summary>
        public bool HasComp<T>() where T : EntityComp
        {
            return _compsByType.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        public bool RemoveComp<T>() where T : EntityComp
        {
            if (_compsByType.TryGetValue(typeof(T), out var comp))
            {
                comp.PostDestroy();
                _comps.Remove(comp);
                _compsByType.Remove(typeof(T));
                return true;
            }
            return false;
        }

        #endregion

        #region Tick更新

        /// <summary>
        /// 每Tick更新
        /// </summary>
        public virtual void Tick()
        {
            foreach (var comp in _comps)
            {
                comp.CompTick();
            }
        }

        /// <summary>
        /// 稀有Tick（每250 Tick）
        /// </summary>
        public virtual void TickRare()
        {
            foreach (var comp in _comps)
            {
                comp.CompTickRare();
            }
        }

        /// <summary>
        /// 长周期Tick（每2000 Tick）
        /// </summary>
        public virtual void TickLong()
        {
            foreach (var comp in _comps)
            {
                comp.CompTickLong();
            }
        }

        #endregion

        #region 伤害系统

        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(int damage, Entity instigator = null)
        {
            if (!Attackable || damage <= 0)
                return;

            _hitPoints -= damage;

            OnDamaged(damage, instigator);

            if (_hitPoints <= 0)
            {
                OnKilled(instigator);
            }
        }

        /// <summary>
        /// 受伤回调
        /// </summary>
        protected virtual void OnDamaged(int damage, Entity instigator)
        {
            foreach (var comp in _comps)
            {
                comp.OnDamaged(damage, instigator);
            }
        }

        /// <summary>
        /// 被击杀回调
        /// </summary>
        protected virtual void OnKilled(Entity killer)
        {
            // 默认销毁
            Destroy();
        }

        /// <summary>
        /// 治疗/修复
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (amount <= 0)
                return;

            _hitPoints = Mathf.Min(_hitPoints + amount, MaxHitPoints);
        }

        #endregion

        #region 自定义数据

        /// <summary>
        /// 设置自定义数据
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            _customData ??= new Dictionary<string, object>();
            _customData[key] = value;
        }

        /// <summary>
        /// 获取自定义数据
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (_customData != null && _customData.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 移除自定义数据
        /// </summary>
        public bool RemoveCustomData(string key)
        {
            return _customData?.Remove(key) ?? false;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{Label}({_entityId}) at {_position}";
        }

        /// <summary>
        /// 详细信息
        /// </summary>
        public virtual string ToDetailedString()
        {
            return $"{Label}[{_entityId}]: Def={DefId}, Pos={_position}, Floor={FloorIndex}, " +
                   $"HP={_hitPoints}/{MaxHitPoints}, Spawned={_spawned}";
        }

        #endregion
    }
}
