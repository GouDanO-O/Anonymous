namespace Core.Game.Map.Define
{
    /// <summary>
    /// 室内透视控制模式
    /// </summary>
    public enum EOcclusionMode : byte
    {
        /// <summary>
        /// 全局模式：手动切换显示/隐藏所有建筑内部
        /// </summary>
        Global = 0,

        /// <summary>
        /// 悬停模式：鼠标悬停在建筑上方时显示内部
        /// </summary>
        Hover = 1,

        /// <summary>
        /// 跟随模式：摄像机跟随的Pawn进入建筑时自动显示内部
        /// </summary>
        FollowPawn = 2
    }
}
