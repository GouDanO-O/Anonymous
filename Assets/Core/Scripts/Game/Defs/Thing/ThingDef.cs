using System;
using UnityEngine;

namespace Core.Game.Defs.Thing
{
    /// <summary>
    /// 物件类别
    /// </summary>
    public enum EThingCategory
    {
        None = 0,
        Item,           // 物品（可拾取、可堆叠）
        Building,       // 建筑（固定在地面）
        Plant,          // 植物
        Pawn,           // 角色（人、动物）
        Projectile,     // 投射物
        Filth,          // 污物
        Effect,         // 特效
    }
    
    /// <summary>
    /// 物件定义 - 所有可放置在地图上的物件的基类
    /// 包括：建筑、物品、植物、角色等
    /// </summary>
    [Serializable]
    public class ThingDef : BaseDef
    {
        public override EDefType DefType => EDefType.Thing;

        #region 基础属性

        /// <summary>
        /// 物件类别
        /// </summary>
        public EThingCategory category = EThingCategory.None;

        /// <summary>
        /// 物件占用的尺寸（格子数）
        /// </summary>
        public Vector2Int size = Vector2Int.one;

        /// <summary>
        /// 选择优先级（点击时的选中优先级）
        /// </summary>
        public int selectionPriority = 0;

        /// <summary>
        /// 是否可选择
        /// </summary>
        public bool selectable = true;

        #endregion

        #region 视觉属性

        /// <summary>
        /// 贴图路径
        /// </summary>
        public string texturePath;

        /// <summary>
        /// 图标路径（用于UI显示）
        /// </summary>
        public string iconPath;

        /// <summary>
        /// 默认颜色
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// 绘制尺寸倍率
        /// </summary>
        public float drawSize = 1f;

        /// <summary>
        /// 渲染层级
        /// </summary>
        public int renderLayer = 0;

        /// <summary>
        /// 是否随旋转改变贴图
        /// </summary>
        public bool rotatable = false;

        #endregion

        #region 物理属性

        /// <summary>
        /// 是否阻挡通行
        /// </summary>
        public bool blockPass = false;

        /// <summary>
        /// 通行消耗（如果不完全阻挡）
        /// </summary>
        public float pathCost = 0f;

        /// <summary>
        /// 是否阻挡视线
        /// </summary>
        public bool blockLight = false;

        /// <summary>
        /// 覆盖率（0-1，用于判断是否完全占据格子）
        /// </summary>
        public float fillPercent = 0f;

        #endregion

        #region 交互属性

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int maxHitPoints = 100;

        /// <summary>
        /// 是否可被攻击/破坏
        /// </summary>
        public bool destroyable = true;

        /// <summary>
        /// 是否可燃
        /// </summary>
        public bool flammable = false;

        /// <summary>
        /// 燃烧性（越高越容易着火）
        /// </summary>
        public float flammability = 0f;

        #endregion

        #region 物品属性（仅Item类别有效）

        /// <summary>
        /// 堆叠上限
        /// </summary>
        public int stackLimit = 1;

        /// <summary>
        /// 单个物品质量
        /// </summary>
        public float mass = 1f;

        /// <summary>
        /// 基础市场价值
        /// </summary>
        public float baseMarketValue = 0f;

        #endregion

        public override void PostLoad()
        {
            base.PostLoad();

            // 自动设置一些默认值
            if (string.IsNullOrEmpty(iconPath))
            {
                iconPath = texturePath;
            }

            if (size.x <= 0) size.x = 1;
            if (size.y <= 0) size.y = 1;
        }

        public override string[] ConfigErrors()
        {
            var baseErrors = base.ConfigErrors();
            var errors = new System.Collections.Generic.List<string>();

            if (baseErrors != null)
            {
                errors.AddRange(baseErrors);
            }

            if (category == EThingCategory.None)
            {
                errors.Add("category未设置");
            }

            if (string.IsNullOrEmpty(texturePath))
            {
                errors.Add("texturePath未设置");
            }

            if (maxHitPoints <= 0 && destroyable)
            {
                errors.Add("destroyable为true但maxHitPoints <= 0");
            }

            return errors.Count > 0 ? errors.ToArray() : null;
        }

        /// <summary>
        /// 是否是建筑
        /// </summary>
        public bool IsBuilding => category == EThingCategory.Building;

        /// <summary>
        /// 是否是物品
        /// </summary>
        public bool IsItem => category == EThingCategory.Item;

        /// <summary>
        /// 是否是角色
        /// </summary>
        public bool IsPawn => category == EThingCategory.Pawn;

        /// <summary>
        /// 是否可堆叠
        /// </summary>
        public bool Stackable => stackLimit > 1;
    }
}