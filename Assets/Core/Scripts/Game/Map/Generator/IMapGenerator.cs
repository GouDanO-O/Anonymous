using Core.Game.Map.Data;

namespace Core.Game.Map.Generator
{
    /// <summary>
    /// 地图生成器接口
    /// </summary>
    public interface IMapGenerator
    {
        /// <summary>
        /// 生成地图
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="width">宽度（格子数）</param>
        /// <param name="height">高度（格子数）</param>
        /// <param name="floorCount">楼层数</param>
        /// <param name="seed">随机种子</param>
        /// <returns>生成的地图数据</returns>
        MapData Generate(string mapName, int width, int height, int floorCount, int seed);

        /// <summary>
        /// 生成单个 Chunk
        /// </summary>
        void GenerateChunk(ChunkData chunk, int seed);
    }
}
