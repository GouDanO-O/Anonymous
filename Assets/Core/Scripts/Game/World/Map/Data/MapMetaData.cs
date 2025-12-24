using System;

namespace Core.Game.World.Map.Data
{
    /// <summary>
    /// 地图元数据
    /// </summary>
    [Serializable]
    public class MapMetaData
    {
        public string mapId;
        
        public string mapName;
        
        public int widthInChunks;
        
        public int heightInChunks;
        
        public int defaultMinFloor = 0;
        
        public int defaultMaxFloor = 1;
        
        public int WidthInTiles => widthInChunks * MapConstants.CHUNK_SIZE;
        
        public int HeightInTiles => heightInChunks * MapConstants.CHUNK_SIZE;
    }
}