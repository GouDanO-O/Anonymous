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

                    var terrainDef = TempConfigProvider.GetTerrainDef(cell.TerrainDefId);
                    cell.MoveCost = terrainDef.MoveCost;
                    cell.SetFlag(ECellFlags.Walkable, cell.MoveCost > 0);
                    cell.SetFlag(ECellFlags.Buildable, terrainDef.CanBuild);
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

                    if (noise > waterThreshold && noise <= 0.78f)
                    {
                        // 浅水: 可涉水通过但速度慢, 不可直接建造(需先建地基)
                        cell.TerrainDefId = 5; // ShallowWater
                        cell.MoveCost = 3;
                        cell.SetFlag(ECellFlags.Walkable, true);
                        cell.SetFlag(ECellFlags.Buildable, false);
                    }
                    else if (noise > 0.78f)
                    {
                        // 深水: 不可通行, 不可建造
                        cell.TerrainDefId = 6; // DeepWater
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
            // wallStructureId: 1=WoodWall, 2=StoneWall
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
        /// 生成单个建筑 (格子占据式)
        /// 外圈格子放墙结构, 内部格子铺地板
        /// </summary>
        /// <param name="startX">左下角X</param>
        /// <param name="startY">左下角Y</param>
        /// <param name="w">总宽度 (含墙, 内部宽度=w-2)</param>
        /// <param name="h">总高度 (含墙, 内部高度=h-2)</param>
        /// <param name="wallStructureId">墙结构ID (1=WoodWall, 2=StoneWall)</param>
        /// <param name="floorDefId">地板类型ID</param>
        private void GenerateBuilding(MapData mapData, int startX, int startY,
            int w, int h, int wallStructureId, int floorDefId, Random rng)
        {
            // 边界检查, 至少3x3才能有内部空间
            if (w < 3 || h < 3) return;
            if (startX < 0 || startY < 0 ||
                startX + w > mapData.Width || startY + h > mapData.Height)
                return;

            // 门和窗的结构ID (根据材质对应)
            // WoodWall(1) → WoodDoor(3), WoodWindow(5)
            // StoneWall(2) → StoneDoor(4), StoneWindow(6)
            int doorStructureId = wallStructureId + 2;
            int windowStructureId = wallStructureId + 4;

            var wallDef = TempConfigProvider.GetStructureDef(wallStructureId);

            for (int y = startY; y < startY + h; y++)
            {
                for (int x = startX; x < startX + w; x++)
                {
                    var cell = mapData.GetCell(x, y, 0);
                    if (cell == null) continue;

                    // 清除水体 (建筑压过水)
                    if (cell.TerrainDefId == 5 || cell.TerrainDefId == 6)
                    {
                        cell.TerrainDefId = 2; // 改为泥土
                    }

                    bool isBorder = x == startX || x == startX + w - 1
                                 || y == startY || y == startY + h - 1;

                    if (isBorder)
                    {
                        // 外圈: 放置墙结构
                        cell.StructureDefId = wallStructureId;
                        cell.StructureHealth = wallDef.MaxHealth;
                        cell.DoorState = EDoorState.None;
                        cell.FloorDefId = floorDefId;
                        cell.MoveCost = 0;
                        cell.SetFlag(ECellFlags.Walkable, false);
                        cell.SetFlag(ECellFlags.Buildable, false);
                    }
                    else
                    {
                        // 内部: 铺地板, 无结构
                        cell.FloorDefId = floorDefId;
                        cell.MoveCost = 1;
                        cell.SetFlag(ECellFlags.Walkable, true);
                        cell.SetFlag(ECellFlags.Buildable, true);
                    }

                    cell.SetFlag(ECellFlags.HasRoof, true);
                }
            }

            // 在南墙随机位置放一个门 (不在角落)
            int doorX = startX + 1 + rng.Next(0, w - 2);
            var doorCell = mapData.GetCell(doorX, startY, 0);
            if (doorCell != null)
            {
                var doorDef = TempConfigProvider.GetStructureDef(doorStructureId);
                doorCell.StructureDefId = doorStructureId;
                doorCell.StructureHealth = doorDef.MaxHealth;
                doorCell.DoorState = EDoorState.Closed;
                doorCell.MoveCost = 1;
                doorCell.SetFlag(ECellFlags.Walkable, true);
            }

            // 清除门口前方水体，确保入口可通行 (门前3格深, 左右各1格)
            for (int dy = -1; dy >= -3; dy--)
            {
                for (int ddx = -1; ddx <= 1; ddx++)
                {
                    var entryCell = mapData.GetCell(doorX + ddx, startY + dy, 0);
                    if (entryCell != null && (entryCell.TerrainDefId == 5 || entryCell.TerrainDefId == 6))
                    {
                        entryCell.TerrainDefId = 2; // 泥土
                        entryCell.MoveCost = 1;
                        entryCell.SetFlag(ECellFlags.Walkable, true);
                        entryCell.SetFlag(ECellFlags.Buildable, true);
                    }
                }
            }

            // 随机在北墙或东墙放一个窗 (不在角落)
            bool windowOnNorth = rng.Next(2) == 0;
            if (windowOnNorth && w > 2)
            {
                int winX = startX + 1 + rng.Next(0, w - 2);
                var winCell = mapData.GetCell(winX, startY + h - 1, 0);
                if (winCell != null)
                {
                    var winDef = TempConfigProvider.GetStructureDef(windowStructureId);
                    winCell.StructureDefId = windowStructureId;
                    winCell.StructureHealth = winDef.MaxHealth;
                    winCell.DoorState = EDoorState.None;
                    // 窗户阻挡移动但允许视线
                    winCell.MoveCost = 0;
                    winCell.SetFlag(ECellFlags.Walkable, false);
                }
            }
            else if (h > 2)
            {
                int winY = startY + 1 + rng.Next(0, h - 2);
                var winCell = mapData.GetCell(startX + w - 1, winY, 0);
                if (winCell != null)
                {
                    var winDef = TempConfigProvider.GetStructureDef(windowStructureId);
                    winCell.StructureDefId = windowStructureId;
                    winCell.StructureHealth = winDef.MaxHealth;
                    winCell.DoorState = EDoorState.None;
                    winCell.MoveCost = 0;
                    winCell.SetFlag(ECellFlags.Walkable, false);
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
            if (startX < 0 || startY < 0 ||
                startX + w > mapData.Width || startY + h > mapData.Height)
                return;

            int wallStructureId = 2; // 石墙
            int doorStructureId = 4; // 石门
            int windowStructureId = 6; // 石窗
            var wallDef = TempConfigProvider.GetStructureDef(wallStructureId);

            for (int y = startY; y < startY + h; y++)
            {
                for (int x = startX; x < startX + w; x++)
                {
                    var cell = mapData.GetCell(x, y, 1);
                    if (cell == null) continue;

                    cell.TerrainDefId = MapConst.InvalidDefId;

                    bool isBorder = x == startX || x == startX + w - 1
                                 || y == startY || y == startY + h - 1;

                    if (isBorder)
                    {
                        cell.StructureDefId = wallStructureId;
                        cell.StructureHealth = wallDef.MaxHealth;
                        cell.DoorState = EDoorState.None;
                        cell.FloorDefId = 2; // 石地板
                        cell.MoveCost = 0;
                        cell.SetFlag(ECellFlags.Walkable, false);
                    }
                    else
                    {
                        cell.FloorDefId = 2; // 石地板
                        cell.MoveCost = 1;
                        cell.SetFlag(ECellFlags.Walkable, true);
                    }

                    cell.SetFlag(ECellFlags.HasRoof, true);
                }
            }

            // 二楼门 (南墙中间)
            var doorCell = mapData.GetCell(centerX, startY, 1);
            if (doorCell != null)
            {
                var doorDef = TempConfigProvider.GetStructureDef(doorStructureId);
                doorCell.StructureDefId = doorStructureId;
                doorCell.StructureHealth = doorDef.MaxHealth;
                doorCell.DoorState = EDoorState.Closed;
                doorCell.MoveCost = 1;
                doorCell.SetFlag(ECellFlags.Walkable, true);
            }

            // 二楼窗 (北墙中间)
            var winCell = mapData.GetCell(centerX, startY + h - 1, 1);
            if (winCell != null)
            {
                var winDef = TempConfigProvider.GetStructureDef(windowStructureId);
                winCell.StructureDefId = windowStructureId;
                winCell.StructureHealth = winDef.MaxHealth;
                winCell.DoorState = EDoorState.None;
                winCell.MoveCost = 0;
                winCell.SetFlag(ECellFlags.Walkable, false);
            }

            LogKit.Log($"二楼结构生成完成: ({startX},{startY}) - ({startX + w},{startY + h})");
        }
    }
}
