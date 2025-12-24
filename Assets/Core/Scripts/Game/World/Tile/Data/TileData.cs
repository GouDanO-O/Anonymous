namespace Core.Game.World.Tile.Data
{
    public struct TileData
    {
        /// <summary>
        /// 地形层 - 仅 z=1 有效
        /// </summary>
        public TileLayerData groundLayerData;   
        
        /// <summary>
        /// 地板层 - 需承重，也是下层天花板
        /// </summary>
        public TileLayerData floorLayerData;
        
        /// <summary>
        /// 地面装饰层
        /// </summary>
        public DecorLayerData decorLayerData;

        public static TileData Empty => default;
        
        /// <summary>
        /// 能否承载实体
        /// </summary>
        /// <returns></returns>
        public bool CanBearingEntity()
        {
            if (groundLayerData.IsEmpty() || floorLayerData.IsEmpty())
                return false;
            if (groundLayerData.CanBearWeight || floorLayerData.CanBearWeight)
                return true;
            return false;
        }
        
        /// <summary>
        /// 是否有地板层
        /// </summary>
        public bool HasFloor => floorLayerData.IsEmpty();

        /// <summary>
        /// 设置地形层
        /// </summary>
        public void SetGround(ushort tileId)
        {
            
        }

        /// <summary>
        /// 设置地板层
        /// </summary>
        public void SetFloor(ushort tileId)
        {
            
        }

        /// <summary>
        /// 设置地面装饰层
        /// </summary>
        public void SetDecor()
        {
            
        }

        public void ClearGround()
        {
            
        }

        public void ClearFloor()
        {
            ClearDecor();
        }

        public void ClearDecor()
        {
            
        }
    }
}