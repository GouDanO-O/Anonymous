namespace Core.Game.Map.Define
{
    /// <summary>
    /// 建筑结构类型
    /// </summary>
    public enum EStructureType : byte
    {
        Wall = 0,
        Door = 1,
        Window = 2,
        Pillar = 3,
        Stair = 4,
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
}
