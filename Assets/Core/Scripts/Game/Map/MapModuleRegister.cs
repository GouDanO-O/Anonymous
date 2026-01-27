using Core.Game.Map.Model;
using Core.Game.Map.System;
using GDFrameworkCore;

namespace Core.Game.Map
{
    /// <summary>
    /// 地图模块注册扩展
    /// 用于在架构初始化时注册地图相关的 Model 和 System
    /// 
    /// 使用方式：
    /// 在 Main.cs 的 Register_Model() 中添加：
    ///     this.RegisterModel(new MapDataModel());
    /// 
    /// 在 Main.cs 的 Register_System() 中添加：
    ///     this.RegisterSystem(new MapGenerateSystem());
    ///     this.RegisterSystem(new RoomSystem());
    ///     this.RegisterSystem(new MapOcclusionSystem());
    ///     this.RegisterSystem(new MapSystem());
    /// </summary>
    public static class MapModuleRegister
    {
        /// <summary>
        /// 注册地图模块的所有 Model
        /// </summary>
        public static void RegisterMapModels(this Architecture<GameMain> architecture)
        {
            architecture.RegisterModel(new MapDataModel());
        }

        /// <summary>
        /// 注册地图模块的所有 System
        /// </summary>
        public static void RegisterMapSystems(this Architecture<GameMain> architecture)
        {
            // 注意顺序：依赖关系决定注册顺序
            // 1. MapGenerateSystem - 地图生成（基础）
            // 2. RoomSystem - 房间检测（依赖地图数据）
            // 3. MapOcclusionSystem - 遮挡系统（依赖房间系统）
            // 4. MapSystem - 核心系统（协调所有子系统）
            architecture.RegisterSystem(new MapGenerateSystem());
            architecture.RegisterSystem(new RoomSystem());
            architecture.RegisterSystem(new MapOcclusionSystem());
            architecture.RegisterSystem(new MapSystem());
        }

        /// <summary>
        /// 一次性注册地图模块的所有内容
        /// </summary>
        public static void RegisterMapModule(this Architecture<GameMain> architecture)
        {
            RegisterMapModels(architecture);
            RegisterMapSystems(architecture);
        }
    }
}
