namespace Core.Game.World.Tile.Data.Enums
{
    /// <summary>
    /// 地板类型
    /// </summary>
    public enum EFloorType : byte
    {
        //无
        None,
        //木地板        
        WoodPlank,
        //石砖      大理石砖
        StoneTile, Marble,
        //陶瓷砖
        Ceramic,
        //铁   玻璃(能看到下一层)
        Metal, Glass,
    }
}