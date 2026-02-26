using System;
using Core.Game.Building.Define;

namespace Core.Game.Building.Data
{
    /// <summary>
    /// 已建成建筑的实例数据
    /// </summary>
    [Serializable]
    public class BuildingInstance
    {
        #region 基础属性

        /// <summary>
        /// 建筑实例ID
        /// </summary>
        public int BuildingId;

        /// <summary>
        /// 配置定义名（关联Luban BuildingDef）
        /// </summary>
        public string DefName;

        /// <summary>
        /// 建筑类别
        /// </summary>
        public EBuildingCategory Category;

        #endregion

        #region 位置

        /// <summary>
        /// 格子X
        /// </summary>
        public int CellX;

        /// <summary>
        /// 格子Y
        /// </summary>
        public int CellY;

        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor;

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width;

        /// <summary>
        /// 高度
        /// </summary>
        public int Height;

        #endregion

        #region 状态

        /// <summary>
        /// 当前耐久
        /// </summary>
        public int CurrentHealth;

        /// <summary>
        /// 最大耐久
        /// </summary>
        public int MaxHealth;

        #endregion

        #region 属性访问器

        /// <summary>
        /// 耐久比例（0~1）
        /// </summary>
        public float HealthRatio => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

        /// <summary>
        /// 是否已损坏
        /// </summary>
        public bool IsDestroyed => CurrentHealth <= 0;

        #endregion

        #region 构造函数

        public BuildingInstance()
        {
            Width = 1;
            Height = 1;
        }

        /// <summary>
        /// 从蓝图数据创建建筑实例
        /// </summary>
        public static BuildingInstance FromBlueprint(BlueprintData blueprint, int buildingId, int maxHealth)
        {
            return new BuildingInstance
            {
                BuildingId = buildingId,
                DefName = blueprint.DefName,
                Category = blueprint.Category,
                CellX = blueprint.CellX,
                CellY = blueprint.CellY,
                Floor = blueprint.Floor,
                Width = blueprint.Width,
                Height = blueprint.Height,
                MaxHealth = maxHealth,
                CurrentHealth = maxHealth
            };
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 造成伤害
        /// </summary>
        public bool TakeDamage(int damage)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            return IsDestroyed;
        }

        /// <summary>
        /// 修复
        /// </summary>
        public void Repair(int amount)
        {
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }

        /// <summary>
        /// 检查位置是否在建筑范围内
        /// </summary>
        public bool ContainsCell(int x, int y)
        {
            return x >= CellX && x < CellX + Width &&
                   y >= CellY && y < CellY + Height;
        }

        public override string ToString()
        {
            return $"Building({BuildingId}, {DefName}, ({CellX},{CellY},F{Floor}), HP={CurrentHealth}/{MaxHealth})";
        }

        #endregion
    }
}
