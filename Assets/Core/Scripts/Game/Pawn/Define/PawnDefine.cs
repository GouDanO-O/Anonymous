namespace Core.Game.Pawn.Define
{
    /// <summary>
    /// Pawn系统常量定义
    /// </summary>
    public static class PawnDefine
    {
        #region 移动

        /// <summary>
        /// 默认移动速度（格/秒）
        /// </summary>
        public const float DefaultMoveSpeed = 4.5f;

        /// <summary>
        /// 视觉位置到达逻辑位置的阈值
        /// 当视觉位置与目标格子中心距离小于此值时，认为已到达
        /// </summary>
        public const float ArrivalThreshold = 0.02f;

        /// <summary>
        /// 逻辑位置切换的进度阈值
        /// 当移动进度超过此值时，逻辑位置切换到下一格
        /// </summary>
        public const float LogicSwitchProgress = 0.5f;

        #endregion

        #region ID分配

        /// <summary>
        /// Pawn ID起始值
        /// </summary>
        public const int PawnIdStart = 1;

        /// <summary>
        /// 无效的Pawn ID
        /// </summary>
        public const int InvalidPawnId = 0;

        #endregion

        #region 渲染

        /// <summary>
        /// 默认Pawn占位尺寸（世界单位）
        /// </summary>
        public const float DefaultPawnSize = 0.2f;

        /// <summary>
        /// 固定Pawn批量Mesh重建的阈值
        /// 超过此数量的固定Pawn将合入Chunk Mesh
        /// </summary>
        public const int StaticPawnMeshThreshold = 10;

        #endregion
    }
}
