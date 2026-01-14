using System;

namespace Core.Game.Defs.Thing
{
    /// <summary>
    /// 物品类型
    /// </summary>
    public enum EItemType
    {
        None = 0,
        Resource,       // 资源（木材、石头、钢铁）
        RawFood,        // 生食材
        MealSimple,     // 简单餐食
        MealFine,       // 精致餐食
        Medicine,       // 药品
        Weapon,         // 武器
        Apparel,        // 服装
        Drug,           // 药物
        Artifact,       // 神器/特殊物品
    }
    
    /// <summary>
    /// 物品定义 - 定义可拾取、可堆叠物品的属性
    /// 继承自ThingDef
    /// </summary>
    [Serializable]
    public class ItemDef : ThingDef
    {
        public override EDefType DefType => EDefType.Item;

        public ItemDef()
        {
            // 物品的默认值
            category = EThingCategory.Item;
            blockPass = false;
            destroyable = true;
            stackLimit = 75; // 默认可堆叠
        }

        #region 物品特有属性

        /// <summary>
        /// 物品类型
        /// </summary>
        public EItemType itemType = EItemType.None;

        /// <summary>
        /// 是否是原材料
        /// </summary>
        public bool isRawMaterial = false;

        /// <summary>
        /// 材料类型标签（用于配方系统）
        /// </summary>
        public string[] materialTags;

        #endregion

        #region 食物属性

        /// <summary>
        /// 是否是食物
        /// </summary>
        public bool isFood = false;

        /// <summary>
        /// 营养值
        /// </summary>
        public float nutrition = 0f;

        /// <summary>
        /// 食物腐烂时间（天），0表示不会腐烂
        /// </summary>
        public float daysToRotStart = 0f;

        /// <summary>
        /// 食物口感（影响心情）
        /// </summary>
        public int foodTaste = 0;

        #endregion

        #region 药品属性

        /// <summary>
        /// 是否是药品
        /// </summary>
        public bool isMedicine = false;

        /// <summary>
        /// 医疗效果
        /// </summary>
        public float medicalPotency = 0f;

        #endregion

        #region 装备属性

        /// <summary>
        /// 是否可装备
        /// </summary>
        public bool isEquipment = false;

        /// <summary>
        /// 装备槽位
        /// </summary>
        public string equipSlot;

        /// <summary>
        /// 武器伤害（如果是武器）
        /// </summary>
        public float weaponDamage = 0f;

        /// <summary>
        /// 武器攻击间隔（秒）
        /// </summary>
        public float weaponCooldown = 1f;

        /// <summary>
        /// 武器射程（0表示近战）
        /// </summary>
        public float weaponRange = 0f;

        /// <summary>
        /// 护甲加成（如果是服装）
        /// </summary>
        public float armorBonus = 0f;

        /// <summary>
        /// 移动速度修正（如果是服装）
        /// </summary>
        public float moveSpeedModifier = 0f;

        #endregion

        #region 经济属性

        /// <summary>
        /// 是否可交易
        /// </summary>
        public bool tradeable = true;

        /// <summary>
        /// 交易标签（用于商人生成）
        /// </summary>
        public string[] tradeTags;

        #endregion

        public override void PostLoad()
        {
            base.PostLoad();

            // 根据物品类型自动设置一些属性
            switch (itemType)
            {
                case EItemType.RawFood:
                case EItemType.MealSimple:
                case EItemType.MealFine:
                    isFood = true;
                    break;
                case EItemType.Medicine:
                    isMedicine = true;
                    break;
                case EItemType.Weapon:
                case EItemType.Apparel:
                    isEquipment = true;
                    stackLimit = 1; // 装备不可堆叠
                    break;
                case EItemType.Resource:
                    isRawMaterial = true;
                    break;
            }
        }

        public override string[] ConfigErrors()
        {
            var baseErrors = base.ConfigErrors();
            var errors = new System.Collections.Generic.List<string>();

            if (baseErrors != null)
            {
                errors.AddRange(baseErrors);
            }

            if (itemType == EItemType.None)
            {
                errors.Add("itemType未设置");
            }

            if (isFood && nutrition <= 0)
            {
                errors.Add("食物的nutrition必须大于0");
            }

            if (isEquipment && string.IsNullOrEmpty(equipSlot))
            {
                errors.Add("装备的equipSlot未设置");
            }

            return errors.Count > 0 ? errors.ToArray() : null;
        }
    }

    /// <summary>
    /// 预定义的常用物品
    /// </summary>
    public static class ItemDefOf
    {
        // 资源
        public static ItemDef Wood;
        public static ItemDef Stone;
        public static ItemDef Steel;

        // 食物
        public static ItemDef RawPotato;
        public static ItemDef MealSimple;

        // 药品
        public static ItemDef HerbalMedicine;

        public static void BindReferences()
        {
            Wood = DefDataBase<ItemDef>.GetNamed("Item_Wood");
            Stone = DefDataBase<ItemDef>.GetNamed("Item_Stone");
            Steel = DefDataBase<ItemDef>.GetNamed("Item_Steel");
            RawPotato = DefDataBase<ItemDef>.GetNamed("Item_RawPotato");
            MealSimple = DefDataBase<ItemDef>.GetNamed("Item_MealSimple");
            HerbalMedicine = DefDataBase<ItemDef>.GetNamed("Item_HerbalMedicine");
        }
    }
}