/**
 * MapEnums.cs
 * 地图系统枚举定义
 * 
 * 混合系统设计：
 * - TileCategory: 静态瓦片分类（地形、地板、墙壁等）
 * - EntityType: 动态实体类型（家具、容器、门等）
 */

using System;

namespace GDFramework.MapSystem
{
    #region Tile 相关枚举（静态内容）
    
    /// <summary>
    /// 瓦片分类（静态内容）
    /// 只用于不会移动、不需要复杂交互的地图元素
    /// </summary>
    public enum TileCategory : byte
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 地形（草地、泥土、沙子、水等自然地形）
        /// </summary>
        Terrain = 1,
        
        /// <summary>
        /// 地板（木板、瓷砖、地毯等人造地面）
        /// </summary>
        Floor = 2,
        
        /// <summary>
        /// 地面装饰（血迹、裂缝、落叶等纯视觉效果）
        /// </summary>
        FloorDecor = 3,
        
        /// <summary>
        /// 墙壁
        /// </summary>
        Wall = 4,
        
        /// <summary>
        /// 墙壁装饰（窗框、门框、墙上挂件等）
        /// 注意：门本身是 Entity，门框是 Tile
        /// </summary>
        WallDecor = 5,
        
        /// <summary>
        /// 屋顶
        /// </summary>
        Roof = 6,
        
        /// <summary>
        /// 楼梯（静态建筑结构）
        /// </summary>
        Stairs = 7
    }
    
    /// <summary>
    /// 瓦片标志位
    /// 使用 Flags 特性支持位运算组合
    /// </summary>
    [Flags]
    public enum TileFlags : byte
    {
        /// <summary>
        /// 无任何标志
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 阻挡移动（墙壁等）
        /// </summary>
        Blocking = 1 << 0,      // 0b00000001
        
        /// <summary>
        /// 阻挡视线（不透明墙壁）
        /// </summary>
        BlockSight = 1 << 1,    // 0b00000010
        
        /// <summary>
        /// 已损坏状态
        /// </summary>
        Damaged = 1 << 2,       // 0b00000100
        
        /// <summary>
        /// 可燃烧
        /// </summary>
        Flammable = 1 << 3,     // 0b00001000
        
        /// <summary>
        /// 是水/液体（影响移动速度、声音等）
        /// </summary>
        IsLiquid = 1 << 4,      // 0b00010000
        
        /// <summary>
        /// 预留位
        /// </summary>
        Reserved1 = 1 << 5,     // 0b00100000
        Reserved2 = 1 << 6,     // 0b01000000
        Reserved3 = 1 << 7      // 0b10000000
    }
    
    #endregion
    
    #region Entity 相关枚举（动态内容）
    
    /// <summary>
    /// 实体类型（动态对象）
    /// 用于需要状态、可交互、可能移动的游戏对象
    /// </summary>
    public enum EntityType : byte
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None = 0,
        
        // ========== 家具类 ==========
        
        /// <summary>
        /// 静态家具（椅子、桌子、床等）
        /// 特点：可能被推动、可被破坏
        /// </summary>
        Furniture = 10,
        
        /// <summary>
        /// 容器（冰箱、柜子、箱子等）
        /// 特点：有库存系统、可交互打开
        /// </summary>
        Container = 11,
        
        /// <summary>
        /// 门
        /// 特点：有开关状态、可上锁
        /// </summary>
        Door = 12,
        
        /// <summary>
        /// 窗户
        /// 特点：可打开/关闭、可打破
        /// </summary>
        Window = 13,
        
        // ========== 设备类 ==========
        
        /// <summary>
        /// 电器设备（冰箱、微波炉、电视等）
        /// 特点：可能需要电力、有开关状态
        /// </summary>
        Appliance = 20,
        
        /// <summary>
        /// 灯光源
        /// 特点：影响光照、可开关
        /// </summary>
        LightSource = 21,
        
        /// <summary>
        /// 发电机
        /// 特点：消耗燃料、提供电力
        /// </summary>
        Generator = 22,
        
        // ========== 物品类 ==========
        
        /// <summary>
        /// 掉落物/地面物品
        /// 特点：可拾取
        /// </summary>
        DroppedItem = 30,
        
        /// <summary>
        /// 可采集资源（树木、矿石等）
        /// 特点：可采集、会重生
        /// </summary>
        Resource = 31,
        
        // ========== 机关类 ==========
        
        /// <summary>
        /// 陷阱
        /// </summary>
        Trap = 40,
        
        /// <summary>
        /// 触发器（压力板、绊线等）
        /// </summary>
        Trigger = 41,
        
        /// <summary>
        /// 楼层转换点（楼梯、梯子、电梯等）
        /// </summary>
        Transition = 42,
        
        // ========== 生物类 ==========
        
        /// <summary>
        /// 玩家
        /// </summary>
        Player = 100,
        
        /// <summary>
        /// NPC
        /// </summary>
        NPC = 101,
        
        /// <summary>
        /// 僵尸/敌人
        /// </summary>
        Zombie = 102,
        
        /// <summary>
        /// 动物
        /// </summary>
        Animal = 103,
        
        // ========== 载具类 ==========
        
        /// <summary>
        /// 车辆
        /// </summary>
        Vehicle = 110
    }
    
    /// <summary>
    /// 实体标志位
    /// </summary>
    [Flags]
    public enum EntityFlags : ushort
    {
        None = 0,
        
        /// <summary>
        /// 阻挡移动
        /// </summary>
        Blocking = 1 << 0,
        
        /// <summary>
        /// 可交互
        /// </summary>
        Interactive = 1 << 1,
        
        /// <summary>
        /// 可拾取
        /// </summary>
        Pickupable = 1 << 2,
        
        /// <summary>
        /// 可推动
        /// </summary>
        Pushable = 1 << 3,
        
        /// <summary>
        /// 可破坏
        /// </summary>
        Destructible = 1 << 4,
        
        /// <summary>
        /// 需要电力
        /// </summary>
        RequiresPower = 1 << 5,
        
        /// <summary>
        /// 当前开启状态
        /// </summary>
        IsOpen = 1 << 6,
        
        /// <summary>
        /// 当前锁定状态
        /// </summary>
        IsLocked = 1 << 7,
        
        /// <summary>
        /// 当前通电状态
        /// </summary>
        IsPowered = 1 << 8,
        
        /// <summary>
        /// 已损坏
        /// </summary>
        IsDamaged = 1 << 9,
        
        /// <summary>
        /// 已被摧毁（等待清理）
        /// </summary>
        IsDestroyed = 1 << 10
    }
    
    #endregion
    
    #region 通用枚举
    
    /// <summary>
    /// 旋转角度
    /// </summary>
    public enum Rotation : byte
    {
        /// <summary>
        /// 不旋转（默认朝向，朝北/上）
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 顺时针旋转 90 度（朝东/右）
        /// </summary>
        Rotate90 = 1,
        
        /// <summary>
        /// 旋转 180 度（朝南/下）
        /// </summary>
        Rotate180 = 2,
        
        /// <summary>
        /// 顺时针旋转 270 度（朝西/左）
        /// </summary>
        Rotate270 = 3
    }
    
    /// <summary>
    /// 方向（8方向）
    /// </summary>
    public enum Direction : byte
    {
        None = 0,
        North = 1,      // 上
        NorthEast = 2,  // 右上
        East = 3,       // 右
        SouthEast = 4,  // 右下
        South = 5,      // 下
        SouthWest = 6,  // 左下
        West = 7,       // 左
        NorthWest = 8   // 左上
    }
    
    /// <summary>
    /// 碰撞体类型
    /// </summary>
    public enum ColliderType : byte
    {
        /// <summary>
        /// 无碰撞体
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 矩形碰撞体（最常用）
        /// </summary>
        Box = 1,
        
        /// <summary>
        /// 圆形碰撞体
        /// </summary>
        Circle = 2,
        
        /// <summary>
        /// 自定义多边形碰撞体
        /// </summary>
        Polygon = 3
    }
    
    /// <summary>
    /// 地图类型
    /// </summary>
    public enum MapType : byte
    {
        /// <summary>
        /// 城镇地图
        /// </summary>
        Town = 0,
        
        /// <summary>
        /// 野外地图
        /// </summary>
        Outdoor = 1,
        
        /// <summary>
        /// 室内地图（建筑内部的独立地图）
        /// </summary>
        Indoor = 2,
        
        /// <summary>
        /// 地下城/地下室
        /// </summary>
        Underground = 3,
        
        /// <summary>
        /// 特殊地图（任务场景等）
        /// </summary>
        Special = 4
    }
    
    /// <summary>
    /// 传送门触发类型
    /// </summary>
    public enum PortalTriggerType : byte
    {
        /// <summary>
        /// 进入区域自动触发
        /// </summary>
        OnEnter = 0,
        
        /// <summary>
        /// 需要玩家手动交互触发
        /// </summary>
        OnInteract = 1
    }
    
    /// <summary>
    /// 地图切换过渡效果
    /// </summary>
    public enum TransitionType : byte
    {
        /// <summary>
        /// 淡入淡出
        /// </summary>
        Fade = 0,
        
        /// <summary>
        /// 瞬间切换
        /// </summary>
        Instant = 1,
        
        /// <summary>
        /// 黑屏过渡
        /// </summary>
        BlackScreen = 2,
        
        /// <summary>
        /// 自定义过渡
        /// </summary>
        Custom = 3
    }
    
    #endregion
}
