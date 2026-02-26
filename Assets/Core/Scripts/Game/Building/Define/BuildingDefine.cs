using UnityEngine;

namespace Core.Game.Building.Define
{
    /// <summary>
    /// 建造系统常量定义
    /// </summary>
    public static class BuildingDefine
    {
        #region 预览颜色

        /// <summary>
        /// 合法放置的预览颜色（半透明绿色）
        /// </summary>
        public static readonly Color PreviewColorValid = new Color(0.2f, 0.8f, 0.2f, 0.5f);

        /// <summary>
        /// 非法放置的预览颜色（半透明红色）
        /// </summary>
        public static readonly Color PreviewColorInvalid = new Color(0.8f, 0.2f, 0.2f, 0.5f);

        /// <summary>
        /// 蓝图已放置的颜色（半透明蓝色）
        /// </summary>
        public static readonly Color BlueprintColor = new Color(0.3f, 0.5f, 0.9f, 0.6f);

        #endregion

        #region 建造

        /// <summary>
        /// 默认建造工作量
        /// </summary>
        public const float DefaultWorkToBuild = 100f;

        /// <summary>
        /// Pawn每秒建造的工作量
        /// </summary>
        public const float DefaultBuildSpeed = 20f;

        #endregion

        #region ID分配

        /// <summary>
        /// 建筑实例ID起始值
        /// </summary>
        public const int BuildingIdStart = 1;

        /// <summary>
        /// 蓝图ID起始值
        /// </summary>
        public const int BlueprintIdStart = 10000;

        /// <summary>
        /// 无效ID
        /// </summary>
        public const int InvalidId = 0;

        #endregion
    }
}
