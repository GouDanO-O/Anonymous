using System;

namespace Core.Game.Defs
{
    public enum EDefType
    {
        None = 0,
        Terrain,        // 地面（草地、泥土、地板）
        Tile,           // Tile（墙、门）
        Thing,          // 通用物品
        Building,       // 建筑
        Item,           // 物品
        PawnKind,       // 角色种类
        Job,            // 工作
        WorkType,       // 工作类型
        Recipe,         // 配方
        Research,       // 研究
    }
    
    /// <summary>
    /// 所有游戏定义(Definition)的基类
    /// Def是"模板/类型"，描述某类事物的固有属性，运行时只读
    /// 与Data（实例数据）区分：Def不存档，Data需要存档
    /// </summary>
    [Serializable]
    public abstract class BaseDef
    {
        /// <summary>
        /// 唯一标识符,用于索引和引用
        /// 命名规范：类型_名称，如 Terrain_Grass, Building_Wall
        /// </summary>
        public string defName;
        
        /// <summary>
        /// 显示名称
        /// </summary>
        public string label;
        
        /// <summary>
        /// 详细描述
        /// </summary>
        public string description;
        
        /// <summary>
        /// 定义类型，用于分类管理
        /// </summary>
        public virtual EDefType DefType => EDefType.None;

        /// <summary>
        /// 配置加载完成后的初始化回调
        /// 用于处理引用解析、数据验证等
        /// </summary>
        public virtual void PostLoad()
        {
        }

        /// <summary>
        /// 所有Def加载完成后的回调
        /// 用于处理跨Def引用
        /// </summary>
        public virtual void ResolveReferences()
        {
            
        }

        /// <summary>
        /// 验证配置数据的有效性
        /// </summary>
        /// <returns>错误信息列表，无错误返回null</returns>
        public virtual string[] ConfigErrors()
        {
            if (string.IsNullOrEmpty(defName))
            {
                return new[] { $"{GetType().Name}: defName不能为空" };
            }
            return null;
        }
        
        public override string ToString()
        {
            return $"{GetType().Name}({defName})";
        }

        public override int GetHashCode()
        {
            return defName?.GetHashCode() ?? 0;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is BaseDef other)
            {
                return defName == other.defName;
            }
            return false;
        }
    }
}