using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Generator;
using Core.Game.Map.Model;
using GDFrameworkCore;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 地图生成系统
    /// 负责地图的生成和加载
    /// </summary>
    public class MapGenerateSystem : AbstractSystem
    {
        private MapDataModel _mapDataModel;
        private IMapGenerator _currentGenerator;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _currentGenerator = new SimpleMapGenerator();
        }

        /// <summary>
        /// 设置地图生成器
        /// </summary>
        public void SetGenerator(IMapGenerator generator)
        {
            _currentGenerator = generator;
        }

        /// <summary>
        /// 生成新地图
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="floorCount">楼层数</param>
        /// <param name="seed">随机种子（-1 则随机生成）</param>
        public MapData GenerateMap(string mapName, int width = -1, int height = -1, int floorCount = -1, int seed = -1)
        {
            if (width <= 0)
                width = MapDefine.DefaultMapWidth;
            if (height <= 0)
                height = MapDefine.DefaultMapHeight;
            if (floorCount <= 0)
                floorCount = MapDefine.DefaultFloorCount;
            if (seed < 0)
                seed = UnityEngine.Random.Range(0, int.MaxValue);

            var mapData = _currentGenerator.Generate(mapName, width, height, floorCount, seed);
            return mapData;
        }

        /// <summary>
        /// 生成并加载地图
        /// </summary>
        public void GenerateAndLoadMap(string mapName, int width = -1, int height = -1, int floorCount = -1,
            int seed = -1)
        {
            var mapData = GenerateMap(mapName, width, height, floorCount, seed);
            _mapDataModel.LoadMap(mapData);
        }

        /// <summary>
        /// 使用默认参数生成测试地图
        /// </summary>
        public void GenerateTestMap()
        {
            GenerateAndLoadMap("TestMap",
                MapDefine.DefaultMapWidth,
                MapDefine.DefaultMapHeight,
                MapDefine.DefaultFloorCount,
                12345);
        }
    }
}
