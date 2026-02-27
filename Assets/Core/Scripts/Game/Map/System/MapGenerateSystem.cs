using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;
using Random = System.Random;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 地图生成系统
    /// </summary>
    public class MapGenerateSystem : AbstractSystem
    {
        private MapDataModel _mapDataModel;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
        }

        /// <summary>
        /// 生成并加载地图
        /// </summary>
        public void GenerateAndLoadMap(string name, int width, int height, int floors, int seed)
        {
            LogKit.Log($"开始生成地图: {name} ({width}x{height}), {floors}层, 种子: {seed}");

            var mapData = new MapData(name, width, height, floors, seed);
            var rng = new Random(seed);

            GenerateTerrain(mapData, rng);
            GenerateWater(mapData, rng);
            GenerateBuildings(mapData, rng);
            GenerateSecondFloor(mapData, rng);

            _mapDataModel.LoadMap(mapData);

            LogKit.Log($"地图生成完成: {mapData.ChunkCountX}x{mapData.ChunkCountY} chunks/层");
        }

        /// <summary>
        /// 使用Perlin噪声生成地形
        /// </summary>
        private void GenerateTerrain(MapData mapData, Random rng)
        {
            float offsetX = (float)rng.NextDouble() * 10000f;
            float offsetY = (float)rng.NextDouble() * 10000f;
            float scale = 0.05f;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    var cell = mapData.GetCell(x, y, 0);
                    if (cell == null) continue;

                    float noise = Mathf.PerlinNoise(
                        x * scale + offsetX,
                        y * scale + offsetY
                    );

                    // TerrainDefId: 1=草地, 2=泥土, 3=沙地, 4=石头
                    if (noise < 0.3f)
                        cell.TerrainDefId = 3; // 沙地
                    else if (noise < 0.6f)
                        cell.TerrainDefId = 1; // 草地
                    else if (noise < 0.8f)
                        cell.TerrainDefId = 2; // 泥土
                    else
                        cell.TerrainDefId = 4; // 石头

                    cell.MoveCost = cell.TerrainDefId == 4 ? (byte)0 : (byte)1;
                    cell.SetFlag(ECellFlags.Walkable, cell.MoveCost > 0);
                    cell.SetFlag(ECellFlags.Buildable, cell.MoveCost > 0);
                }
            }
        }

        /// <summary>
        /// 使用第二层Perlin噪声生成水体 (河流/池塘)
        /// </summary>
        private void GenerateWater(MapData mapData, Random rng)
        {
            float offsetX = (float)rng.NextDouble() * 10000f;
            float offsetY = (float)rng.NextDouble() * 10000f;
            float scale = 0.03f; // 更大范围的噪声 → 更连贯的水域
            float waterThreshold = 0.65f;

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    var cell = mapData.GetCell(x, y, 0);
                    if (cell == null) continue;

                    float noise = Mathf.PerlinNoise(
                        x * scale + offsetX,
                        y * scale + offsetY
                    );

                    if (noise > waterThreshold)
                    {
                        cell.TerrainDefId = 5; // 水
                        cell.MoveCost = 0;
                        cell.SetFlag(ECellFlags.Walkable, false);
                        cell.SetFlag(ECellFlags.Buildable, false);
                    }
                }
            }
        }

        /// <summary>
        /// 生成多个建筑
        /// </summary>
        private void GenerateBuildings(MapData mapData, Random rng)
        {
            int centerX = mapData.Width / 2;
            int centerY = mapData.Height / 2;

            // 中心主建筑 (较大, 石墙)
            GenerateBuilding(mapData, centerX - 6, centerY - 5, 12, 10, 2, 2, rng);

            // 周围4个小建筑
            int spread = 25;
            GenerateBuilding(mapData,
                centerX - spread + rng.Next(-5, 5),
                centerY + rng.Next(-5, 5),
                6 + rng.Next(0, 4), 5 + rng.Next(0, 3),
                1, 1, rng);

            GenerateBuilding(mapData,
                centerX + spread / 2 + rng.Next(-5, 5),
                centerY + spread / 2 + rng.Next(-5, 5),
                7 + rng.Next(0, 3), 6 + rng.Next(0, 3),
                1, 1, rng);

            GenerateBuilding(mapData,
                centerX + rng.Next(-5, 5),
                centerY - spread + rng.Next(-3, 3),
                5 + rng.Next(0, 4), 5 + rng.Next(0, 3),
                1, 2, rng);

            GenerateBuilding(mapData,
                centerX + spread / 2 + rng.Next(-3, 3),
                centerY - spread / 2 + rng.Next(-3, 3),
                8 + rng.Next(0, 3), 5 + rng.Next(0, 4),
                2, 3, rng);

            LogKit.Log("多建筑生成完成: 1个主建筑 + 4个小建筑");
        }

        /// <summary>
        /// 生成单个建筑
        /// </summary>
        /// <param name="startX">左下角X</param>
        /// <param name="startY">左下角Y</param>
        /// <param name="w">宽度</param>
        /// <param name="h">高度</param>
        /// <param name="wallDefId">墙壁类型ID</param>
        /// <param name="floorDefId">地板类型ID</param>
        private void GenerateBuilding(MapData mapData, int startX, int startY,
            int w, int h, int wallDefId, int floorDefId, Random rng)
        {
            // 边界检查
            if (startX < 1 || startY < 1 ||
                startX + w >= mapData.Width - 1 || startY + h >= mapData.Height - 1)
                return;

            // 铺设地板 + 四面墙
            for (int y = startY; y < startY + h; y++)
            {
                for (int x = startX; x < startX + w; x++)
                {
                    var cell = mapData.GetCell(x, y, 0);
                    if (cell == null) continue;

                    // 清除水体 (建筑压过水)
                    if (cell.TerrainDefId == 5)
                    {
                        cell.TerrainDefId = 2; // 改为泥土
                        cell.MoveCost = 1;
                        cell.SetFlag(ECellFlags.Walkable, true);
                    }

                    cell.FloorDefId = floorDefId;

                    // 北墙
                    if (y == startY + h - 1)
                        cell.WallNorth = WallData.Create(wallDefId);

                    // 南墙 = startY-1行的WallNorth
                    if (y == startY)
                    {
                        var belowCell = mapData.GetCell(x, startY - 1, 0);
                        if (belowCell != null)
                            belowCell.WallNorth = WallData.Create(wallDefId);
                    }

                    // 西墙
                    if (x == startX)
                        cell.WallWest = WallData.Create(wallDefId);

                    // 东墙 = 右侧cell的WallWest
                    if (x == startX + w - 1)
                    {
                        var eastCell = mapData.GetCell(startX + w, y, 0);
                        if (eastCell != null)
                            eastCell.WallWest = WallData.Create(wallDefId);
                    }

                    cell.SetFlag(ECellFlags.Indoor, true);
                    cell.SetFlag(ECellFlags.HasRoof, true);
                }
            }

            // 在南墙随机位置放一个门
            int doorX = startX + 1 + rng.Next(0, w - 2);
            var doorBelowCell = mapData.GetCell(doorX, startY - 1, 0);
            if (doorBelowCell != null)
            {
                doorBelowCell.WallNorth = new WallData
                {
                    WallDefId = wallDefId,
                    DoorState = EDoorState.Closed,
                    WindowState = EWindowState.None,
                    Health = 100
                };
            }

            // 清除门口前方水体，确保入口可通行 (门前3格深, 左右各1格)
            for (int dy = -1; dy >= -3; dy--)
            {
                for (int ddx = -1; ddx <= 1; ddx++)
                {
                    var entryCell = mapData.GetCell(doorX + ddx, startY + dy, 0);
                    if (entryCell != null && entryCell.TerrainDefId == 5)
                    {
                        entryCell.TerrainDefId = 2; // 泥土
                        entryCell.MoveCost = 1;
                        entryCell.SetFlag(ECellFlags.Walkable, true);
                    }
                }
            }

            // 随机在北墙或东墙放一个窗
            bool windowOnNorth = rng.Next(2) == 0;
            if (windowOnNorth)
            {
                int winX = startX + 1 + rng.Next(0, w - 2);
                var winCell = mapData.GetCell(winX, startY + h - 1, 0);
                if (winCell != null)
                {
                    winCell.WallNorth = new WallData
                    {
                        WallDefId = wallDefId,
                        DoorState = EDoorState.None,
                        WindowState = EWindowState.Closed,
                        Health = 100
                    };
                }
            }
            else
            {
                int winY = startY + 1 + rng.Next(0, h - 2);
                var winCell = mapData.GetCell(startX + w, winY, 0);
                if (winCell != null)
                {
                    winCell.WallWest = new WallData
                    {
                        WallDefId = wallDefId,
                        DoorState = EDoorState.None,
                        WindowState = EWindowState.Closed,
                        Health = 100
                    };
                }
            }
        }

        /// <summary>
        /// 在中心建筑上方生成二楼结构 (验证多楼层)
        /// </summary>
        private void GenerateSecondFloor(MapData mapData, Random rng)
        {
            if (mapData.FloorCount < 2) return;

            int centerX = mapData.Width / 2;
            int centerY = mapData.Height / 2;

            // 二楼范围比一楼稍小 (内缩1格)
            int startX = centerX - 5;
            int startY = centerY - 4;
            int w = 10;
            int h = 8;

            // 边界检查
            if (startX < 1 || startY < 1 ||
                startX + w >= mapData.Width - 1 || startY + h >= mapData.Height - 1)
                return;

            for (int y = startY; y < startY + h; y++)
            {
                for (int x = startX; x < startX + w; x++)
                {
                    var cell = mapData.GetCell(x, y, 1);
                    if (cell == null) continue;

                    cell.FloorDefId = 2; // 石地板
                    cell.TerrainDefId = MapConst.InvalidDefId;

                    // 北墙
                    if (y == startY + h - 1)
                        cell.WallNorth = WallData.Create(2); // 石墙

                    // 南墙
                    if (y == startY)
                    {
                        var belowCell = mapData.GetCell(x, startY - 1, 1);
                        if (belowCell != null)
                            belowCell.WallNorth = WallData.Create(2);
                    }

                    // 西墙
                    if (x == startX)
                        cell.WallWest = WallData.Create(2);

                    // 东墙
                    if (x == startX + w - 1)
                    {
                        var eastCell = mapData.GetCell(startX + w, y, 1);
                        if (eastCell != null)
                            eastCell.WallWest = WallData.Create(2);
                    }

                    cell.SetFlag(ECellFlags.Indoor, true);
                    cell.SetFlag(ECellFlags.HasRoof, true);
                }
            }

            // 二楼门 (南墙)
            int doorX = centerX;
            var doorCell = mapData.GetCell(doorX, startY - 1, 1);
            if (doorCell != null)
            {
                doorCell.WallNorth = new WallData
                {
                    WallDefId = 2,
                    DoorState = EDoorState.Closed,
                    WindowState = EWindowState.None,
                    Health = 100
                };
            }

            // 二楼窗 (北墙中间)
            int winX = centerX;
            var winCell = mapData.GetCell(winX, startY + h - 1, 1);
            if (winCell != null)
            {
                winCell.WallNorth = new WallData
                {
                    WallDefId = 2,
                    DoorState = EDoorState.None,
                    WindowState = EWindowState.Closed,
                    Health = 100
                };
            }

            LogKit.Log($"二楼结构生成完成: ({startX},{startY}) - ({startX + w},{startY + h})");
        }
    }
}
