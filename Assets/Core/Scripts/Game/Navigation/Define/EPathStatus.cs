namespace Core.Game.Navigation.Define
{
    /// <summary>
    /// 寻路结果状态
    /// </summary>
    public enum EPathStatus : byte
    {
        /// <summary>
        /// 寻路成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 寻路失败（无法到达目标）
        /// </summary>
        Failed = 1,

        /// <summary>
        /// 超出节点上限（路径过长或无法找到）
        /// </summary>
        NodeLimitReached = 2,

        /// <summary>
        /// 起点无效
        /// </summary>
        InvalidStart = 3,

        /// <summary>
        /// 终点无效
        /// </summary>
        InvalidEnd = 4,

        /// <summary>
        /// 起点与终点相同
        /// </summary>
        AlreadyAtTarget = 5
    }
}
