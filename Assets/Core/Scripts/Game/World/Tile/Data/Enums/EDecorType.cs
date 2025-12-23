namespace Core.Game.World.Tile.Data.Enums
{
    /// <summary>
    /// 承重层装饰类型
    /// </summary>
    public enum EDecorType :byte
    {
        //无
        None,
        //落叶       玻璃渣           花瓣
        Leaves, GrassClump, Flowers,
        //血迹            泥土泥泞         水渍
        BloodStain, DirtStain, WaterPuddle,
        //弹壳
        CartridgeCase,
    }
}