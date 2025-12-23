namespace Core.Game.World.Tile.Data.Enums
{
    public enum EGroundType : byte
    {
        //空地形
        None,
        // 可承重
        //草地  土地  石地  沥青路   混凝土地面
        Grass, Dirt,Stone,Asphalt,Concrete,
        // 不可承重-需添加承重板
        //沙地 泥地 浅水
        Sand,Muddy,ShallowWater,
        //既不可承重也不可添加承重板
        //沼泽 深水
        Marsh,DeepWater,
    }
}