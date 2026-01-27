namespace Core.Game.Map.Define
{
    /// <summary>
    /// 墙体类型
    /// </summary>
    public enum EWallType : byte
    {
        /// <summary>
        /// 无墙
        /// </summary>
        None = 0,

        /// <summary>
        /// 木墙
        /// </summary>
        Wood = 1,

        /// <summary>
        /// 砖墙
        /// </summary>
        Brick = 2,

        /// <summary>
        /// 石墙
        /// </summary>
        Stone = 3,

        /// <summary>
        /// 金属墙
        /// </summary>
        Metal = 4,

        /// <summary>
        /// 玻璃墙
        /// </summary>
        Glass = 5,
    }

    /// <summary>
    /// 门状态
    /// </summary>
    public enum EDoorState : byte
    {
        /// <summary>
        /// 无门
        /// </summary>
        None = 0,

        /// <summary>
        /// 关闭
        /// </summary>
        Closed = 1,

        /// <summary>
        /// 打开
        /// </summary>
        Open = 2,

        /// <summary>
        /// 锁定
        /// </summary>
        Locked = 3,
    }

    /// <summary>
    /// 窗户状态
    /// </summary>
    public enum EWindowState : byte
    {
        /// <summary>
        /// 无窗
        /// </summary>
        None = 0,

        /// <summary>
        /// 关闭
        /// </summary>
        Closed = 1,

        /// <summary>
        /// 打开
        /// </summary>
        Open = 2,

        /// <summary>
        /// 破损
        /// </summary>
        Broken = 3,
    }

    /// <summary>
    /// 墙体方向
    /// </summary>
    public enum EWallDirection : byte
    {
        /// <summary>
        /// 北墙（格子上边）
        /// </summary>
        North = 0,

        /// <summary>
        /// 西墙（格子左边）
        /// </summary>
        West = 1,
    }
}
