using System;
using Core.Game.World.Entity.Data.Enums;
using Core.Game.World.Tile;
using Core.Game.World.Tile.Data.Enums;

namespace Core.Game.World.Entity.Data
{
    /// <summary>
    /// 家具实体数据
    /// </summary>
    [Serializable]
    public class FurnitureEntityData : EntityData
    {
        /// <summary>
        /// 舒适度（床、椅子使用）
        /// </summary>
        public int comfort;
        
        public FurnitureEntityData() : base()
        {
            category = EEntityCategory.Furniture;
            comfort = 0;
            requiredBearingType = EBearingType.Light;
            
            AddProperty(EEntityProperty.Destructible);
            AddProperty(EEntityProperty.Placeable);
            AddProperty(EEntityProperty.Interactable);
        }
        
        public FurnitureEntityData(int configId, TileCoord position) 
            : base(configId, EEntityCategory.Furniture, position)
        {
            comfort = 0;
            requiredBearingType = EBearingType.Light;
            
            AddProperty(EEntityProperty.Destructible);
            AddProperty(EEntityProperty.Placeable);
            AddProperty(EEntityProperty.Interactable);
        }
    }
}