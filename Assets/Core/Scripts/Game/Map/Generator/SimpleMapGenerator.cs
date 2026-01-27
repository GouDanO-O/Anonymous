using Core.Game.Map.Data;
using Core.Game.Map.Define;
using UnityEngine;
using Random = System.Random;

namespace Core.Game.Map.Generator
{
    /// <summary>
    /// 简单地图生成器（Demo 用）
    /// 生成草地 + 泥土的随机地图，带有简单建筑
    /// </summary>
    public class SimpleMapGenerator : IMapGenerator
    {
        private Random _random;

        #region 生成参数

        /// <summary>
        /// 草地比例（0-1）
        /// </summary>
        public float GrassRatio = 0.7f;

        /// <summary>
        /// 生成建筑的概率
        /// </summary>
        public float BuildingChance = 0.1f;

        /// <summary>
        /// 最小建筑尺寸
        /// </summary>
        public int MinBuildingSize = 3;

        /// <summary>
        /// 最大建筑尺寸
        /// </summary>
        public int MaxBuildingSize = 6;

        #endregion

        public MapData Generate(string mapName, int width, int height, int floorCount, int seed)
        {
            _random = new Random(seed);

            var mapData = new MapData(mapName, width, height, floorCount, seed);

            // 生成所有 Chunk
            for (int floor = 0; floor < floorCount; floor++)
            {
                for (int cy = 0; cy < mapData.ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < mapData.ChunkCountX; cx++)
                    {
                        var chunk = mapData.CreateChunk(cx, cy, floor);
                        GenerateChunk(chunk, seed + cx * 1000 + cy * 100 + floor);
                    }
                }
            }

            // 生成一些简单建筑（仅在一楼）
            GenerateBuildings(mapData);

            return mapData;
        }

        public void GenerateChunk(ChunkData chunk, int seed)
        {
            var random = new Random(seed);

            for (int ly = 0; ly < MapDefine.ChunkHeight; ly++)
            {
                for (int lx = 0; lx < MapDefine.ChunkWidth; lx++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null)
                        continue;

                    // 只在一楼生成地面
                    if (chunk.Floor == 0)
                    {
                        // 随机地面类型
                        float value = (float)random.NextDouble();
                        cell.GroundType = value < GrassRatio ? EGroundType.Grass : EGroundType.Dirt;
                        cell.SetFlag(ECellFlags.Walkable, true);
                    }
                    else
                    {
                        // 高层默认无地面（需要有地板才能站立）
                        cell.GroundType = EGroundType.None;
                        cell.SetFlag(ECellFlags.Walkable, false);
                    }
                }
            }
        }

        /// <summary>
        /// 生成建筑
        /// </summary>
        private void GenerateBuildings(MapData mapData)
        {
            // 在地图上随机放置几个建筑
            int buildingCount = (mapData.Width * mapData.Height) / 100; // 每100格一个建筑
            buildingCount = Mathf.Clamp(buildingCount, 1, 10);

            for (int i = 0; i < buildingCount; i++)
            {
                int width = _random.Next(MinBuildingSize, MaxBuildingSize + 1);
                int height = _random.Next(MinBuildingSize, MaxBuildingSize + 1);

                // 随机位置（留出边缘）
                int x = _random.Next(2, mapData.Width - width - 2);
                int y = _random.Next(2, mapData.Height - height - 2);

                // 生成建筑（1-2层）
                int floors = _random.Next(1, 3);
                GenerateSimpleBuilding(mapData, x, y, width, height, floors);
            }
        }

        /// <summary>
        /// 生成简单矩形建筑
        /// </summary>
        private void GenerateSimpleBuilding(MapData mapData, int startX, int startY, int width, int height,
            int floorCount)
        {
            floorCount = Mathf.Min(floorCount, mapData.FloorCount);

            for (int floor = 0; floor < floorCount; floor++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    for (int x = startX; x < startX + width; x++)
                    {
                        var cell = mapData.GetCell(x, y, floor);
                        if (cell == null)
                            continue;

                        // 设置地板
                        cell.FloorType = EFloorType.Wood;
                        cell.SetFlag(ECellFlags.Walkable, true);
                        cell.SetFlag(ECellFlags.Indoor, true);

                        // 设置墙壁（边缘格子）
                        bool isNorthEdge = (y == startY + height - 1);
                        bool isSouthEdge = (y == startY);
                        bool isWestEdge = (x == startX);
                        bool isEastEdge = (x == startX + width - 1);

                        // 北墙（顶边的格子需要北墙）
                        if (isNorthEdge)
                        {
                            // 中间留门
                            bool isDoor = (x == startX + width / 2);
                            if (isDoor)
                                cell.WallNorth = WallData.CreateWithDoor(EWallType.Wood, EDoorState.Closed);
                            else
                                cell.WallNorth = WallData.Create(EWallType.Wood);
                        }

                        // 西墙（左边的格子需要西墙）
                        if (isWestEdge)
                        {
                            cell.WallWest = WallData.Create(EWallType.Wood);
                        }

                        // 东边缘的格子，需要在东边邻居的西墙设置
                        if (isEastEdge)
                        {
                            var eastCell = mapData.GetCell(x + 1, y, floor);
                            if (eastCell != null)
                            {
                                eastCell.WallWest = WallData.Create(EWallType.Wood);
                            }
                        }

                        // 南边缘的格子，需要在南边邻居的北墙设置
                        if (isSouthEdge)
                        {
                            // 由于我们用 N/W 墙系统，南墙实际上是当前格子的北墙
                            // 但这里 isSouthEdge 的格子的南边界其实应该设置
                            // 在 PZ 系统中，南边界是由 (x, y-1) 格子的北墙来表示
                            // 但我们也可以直接在当前格子标记
                            
                            // 实际处理：南边缘需要在下方格子添加北墙
                            var southCell = mapData.GetCell(x, y - 1, floor);
                            if (southCell != null)
                            {
                                // 中间留门
                                bool isDoor = (x == startX + width / 2);
                                if (isDoor)
                                    southCell.WallNorth = WallData.CreateWithDoor(EWallType.Wood, EDoorState.Closed);
                                else
                                    southCell.WallNorth = WallData.Create(EWallType.Wood);
                            }
                        }
                    }
                }

                // 二楼需要设置屋顶标记
                if (floor == floorCount - 1)
                {
                    for (int y = startY; y < startY + height; y++)
                    {
                        for (int x = startX; x < startX + width; x++)
                        {
                            var cell = mapData.GetCell(x, y, floor);
                            if (cell != null)
                            {
                                cell.SetFlag(ECellFlags.HasRoof, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
