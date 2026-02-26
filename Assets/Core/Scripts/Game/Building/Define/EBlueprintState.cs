namespace Core.Game.Building.Define
{
    /// <summary>
    /// 蓝图状态枚举
    /// </summary>
    public enum EBlueprintState : byte
    {
        /// <summary>
        /// 预览中（鼠标跟随，未放置）
        /// </summary>
        Preview = 0,

        /// <summary>
        /// 已放置（等待Pawn来建造）
        /// </summary>
        Placed = 1,

        /// <summary>
        /// 建造中（Pawn正在施工）
        /// </summary>
        Building = 2,

        /// <summary>
        /// 已完成（转为正式建筑）
        /// </summary>
        Completed = 3,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 4
    }
}
