namespace Core.Game.Navigation.Define
{
    /// <summary>
    /// 寻路系统常量定义
    /// </summary>
    public static class NavigationDefine
    {
        #region 移动代价

        /// <summary>
        /// 直线移动代价（整数化避免浮点）
        /// </summary>
        public const int CostStraight = 10;

        /// <summary>
        /// 对角移动代价（√2 × 10 ≈ 14）
        /// </summary>
        public const int CostDiagonal = 14;

        /// <summary>
        /// 楼梯移动额外代价
        /// </summary>
        public const int CostStairs = 20;

        #endregion

        #region 性能限制

        /// <summary>
        /// 单次寻路最大展开节点数
        /// </summary>
        public const int MaxNodeCount = 5000;

        /// <summary>
        /// 每帧最大处理请求数
        /// </summary>
        public const int MaxRequestsPerFrame = 3;

        /// <summary>
        /// PathNode对象池初始容量
        /// </summary>
        public const int NodePoolInitialCapacity = 256;

        /// <summary>
        /// PathNode对象池最大容量
        /// </summary>
        public const int NodePoolMaxCapacity = 6000;

        #endregion
    }
}
