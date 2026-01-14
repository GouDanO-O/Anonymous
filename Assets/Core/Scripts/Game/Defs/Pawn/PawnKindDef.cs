using System;
using UnityEngine;

namespace Core.Game.Defs.Pawn
{
    /// <summary>
    /// 角色种族类型
    /// </summary>
    public enum ERaceType
    {
        Human, // 人类
        Animal, // 动物
        Mechanoid, // 机械体
    }

    /// <summary>
    /// 角色种类定义 - 定义角色的种类和属性
    /// 例如：殖民者、劫掠者、商人、野兔、狼等
    /// </summary>
    [Serializable]
    public class PawnKindDef : BaseDef
    {
        public override EDefType DefType => EDefType.PawnKind;

        #region 基础属性

        /// <summary>
        /// 种族类型
        /// </summary>
        public ERaceType raceType = ERaceType.Human;

        /// <summary>
        /// 是否可被玩家控制
        /// </summary>
        public bool playerControlled = false;

        /// <summary>
        /// 是否是敌对的
        /// </summary>
        public bool hostile = false;

        /// <summary>
        /// 是否是野生动物
        /// </summary>
        public bool wild = false;

        /// <summary>
        /// 战斗力评估（用于难度计算）
        /// </summary>
        public float combatPower = 50f;

        #endregion

        #region 外观属性

        /// <summary>
        /// 身体贴图路径
        /// </summary>
        public string bodyTexturePath;

        /// <summary>
        /// 头部贴图路径（仅人类有效）
        /// </summary>
        public string headTexturePath;

        /// <summary>
        /// 绘制尺寸
        /// </summary>
        public float drawSize = 1f;

        /// <summary>
        /// 默认颜色
        /// </summary>
        public Color color = Color.white;

        #endregion

        #region 属性数值

        /// <summary>
        /// 基础最大生命值
        /// </summary>
        public int baseMaxHealth = 100;

        /// <summary>
        /// 基础移动速度
        /// </summary>
        public float baseMoveSpeed = 4.5f;

        /// <summary>
        /// 基础工作速度（仅人类）
        /// </summary>
        public float baseWorkSpeed = 1f;

        /// <summary>
        /// 搬运容量
        /// </summary>
        public int carryCapacity = 75;

        /// <summary>
        /// 视野范围
        /// </summary>
        public float sightRange = 20f;

        #endregion

        #region 需求系统（仅人类/高级生物）

        /// <summary>
        /// 是否有心情系统
        /// </summary>
        public bool hasMood = false;

        /// <summary>
        /// 是否需要食物
        /// </summary>
        public bool needsFood = true;

        /// <summary>
        /// 是否需要睡眠
        /// </summary>
        public bool needsRest = true;

        /// <summary>
        /// 饥饿速率（每天消耗的食物量）
        /// </summary>
        public float hungerRate = 1.6f;

        /// <summary>
        /// 疲劳速率
        /// </summary>
        public float restFallRate = 1f;

        #endregion

        #region 战斗属性

        /// <summary>
        /// 近战DPS
        /// </summary>
        public float meleeDPS = 5f;

        /// <summary>
        /// 护甲值
        /// </summary>
        public float armor = 0f;

        /// <summary>
        /// 闪避率
        /// </summary>
        public float dodgeChance = 0f;

        /// <summary>
        /// 默认武器defName（如果有）
        /// </summary>
        public string defaultWeaponDefName;

        #endregion

        #region 工作能力（仅人类）

        /// <summary>
        /// 可用的工作类型defName列表
        /// </summary>
        public string[] allowedWorkTypes;

        /// <summary>
        /// 禁止的工作类型defName列表
        /// </summary>
        public string[] disabledWorkTypes;

        /// <summary>
        /// 技能数值范围（用于生成角色时随机）
        /// </summary>
        public int skillMin = 0;

        public int skillMax = 10;

        #endregion

        #region AI属性

        /// <summary>
        /// 使用的AI思维树defName
        /// </summary>
        public string thinkTreeDefName;

        /// <summary>
        /// 逃跑血量阈值（血量低于此百分比时逃跑）
        /// </summary>
        public float fleeHealthThreshold = 0.3f;

        /// <summary>
        /// 是否会逃跑
        /// </summary>
        public bool canFlee = true;

        #endregion

        public override void PostLoad()
        {
            base.PostLoad();

            // 自动设置一些默认值
            if (raceType == ERaceType.Human && string.IsNullOrEmpty(thinkTreeDefName))
            {
                thinkTreeDefName = "ThinkTree_Humanlike";
            }
            else if (raceType == ERaceType.Animal && string.IsNullOrEmpty(thinkTreeDefName))
            {
                thinkTreeDefName = "ThinkTree_Animal";
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

            if (string.IsNullOrEmpty(bodyTexturePath))
            {
                errors.Add("bodyTexturePath未设置");
            }

            if (raceType == ERaceType.Human && string.IsNullOrEmpty(headTexturePath))
            {
                // 人类应该有头部贴图，但不强制报错
                // errors.Add("人类角色headTexturePath未设置");
            }

            if (baseMaxHealth <= 0)
            {
                errors.Add("baseMaxHealth必须大于0");
            }

            return errors.Count > 0 ? errors.ToArray() : null;
        }
    }

    /// <summary>
    /// 预定义的角色种类
    /// </summary>
    public static class PawnKindDefOf
    {
        public static PawnKindDef Colonist; // 殖民者
        public static PawnKindDef Raider; // 劫掠者
        public static PawnKindDef Trader; // 商人

        public static void BindReferences()
        {
            Colonist = DefDataBase<PawnKindDef>.GetNamed("PawnKind_Colonist");
            Raider = DefDataBase<PawnKindDef>.GetNamed("PawnKind_Raider");
            Trader = DefDataBase<PawnKindDef>.GetNamed("PawnKind_Trader");
        }
    }
}