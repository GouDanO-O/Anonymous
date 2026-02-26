namespace Core.Game.Map.Define
{
    /// <summary>
    /// 墙段方向
    /// 每个Cell存储其北边(North)和西边(West)的墙
    /// 南墙 = 下方Cell的北墙, 东墙 = 右方Cell的西墙
    /// </summary>
    public enum EWallSegment : byte
    {
        North = 0,
        West = 1,
    }

    /// <summary>
    /// 门状态
    /// </summary>
    public enum EDoorState : byte
    {
        None = 0,
        Closed = 1,
        Open = 2,
        Locked = 3,
    }

    /// <summary>
    /// 窗户状态
    /// </summary>
    public enum EWindowState : byte
    {
        None = 0,
        Closed = 1,
        Open = 2,
        Broken = 3,
    }
}
