namespace Core.Game.Pawn.Define
{
    /// <summary>
    /// Pawn状态枚举
    /// </summary>
    public enum EPawnState : byte
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,

        /// <summary>
        /// 移动中
        /// </summary>
        Moving = 1,

        /// <summary>
        /// 工作中
        /// </summary>
        Working = 2,

        /// <summary>
        /// 战斗中
        /// </summary>
        Fighting = 3,

        /// <summary>
        /// 休息中
        /// </summary>
        Resting = 4,

        /// <summary>
        /// 被击倒
        /// </summary>
        Downed = 5,

        /// <summary>
        /// 死亡
        /// </summary>
        Dead = 6
    }
}
