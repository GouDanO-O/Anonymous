using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Map.Data
{
    /// <summary>
    /// 临时地形定义 (后续由Luban生成替代)
    /// </summary>
    public class TerrainDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public byte MoveCost;
        public float Fertility;
    }

    /// <summary>
    /// 临时地板定义
    /// </summary>
    public class FloorDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public byte MoveCost;
        public int Beauty;
    }

    /// <summary>
    /// 临时墙壁定义
    /// </summary>
    public class WallDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public Color DoorColor;
        public Color WindowColor;
        public int MaxHealth;
        public bool IsTransparent;
    }

    /// <summary>
    /// 临时配置数据提供器
    /// 硬编码配置数据，模拟Luban生成的结构
    /// </summary>
    public static class TempConfigProvider
    {
        private static Dictionary<int, TerrainDef> _terrainDefs;
        private static Dictionary<int, FloorDef> _floorDefs;
        private static Dictionary<int, WallDef> _wallDefs;

        private static TerrainDef _defaultTerrain;
        private static FloorDef _defaultFloor;
        private static WallDef _defaultWall;

        static TempConfigProvider()
        {
            InitTerrainDefs();
            InitFloorDefs();
            InitWallDefs();
        }

        private static void InitTerrainDefs()
        {
            _defaultTerrain = new TerrainDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta, MoveCost = 0, Fertility = 0f
            };

            _terrainDefs = new Dictionary<int, TerrainDef>
            {
                {
                    1, new TerrainDef
                    {
                        Id = 1, Name = "Grass",
                        Color = new Color(0.42f, 0.60f, 0.30f),
                        MoveCost = 1, Fertility = 1f
                    }
                },
                {
                    2, new TerrainDef
                    {
                        Id = 2, Name = "Dirt",
                        Color = new Color(0.55f, 0.40f, 0.25f),
                        MoveCost = 1, Fertility = 0.5f
                    }
                },
                {
                    3, new TerrainDef
                    {
                        Id = 3, Name = "Sand",
                        Color = new Color(0.85f, 0.80f, 0.60f),
                        MoveCost = 1, Fertility = 0.1f
                    }
                },
                {
                    4, new TerrainDef
                    {
                        Id = 4, Name = "Rock",
                        Color = new Color(0.50f, 0.50f, 0.50f),
                        MoveCost = 0, Fertility = 0f
                    }
                },
                {
                    5, new TerrainDef
                    {
                        Id = 5, Name = "Water",
                        Color = new Color(0.25f, 0.45f, 0.70f),
                        MoveCost = 0, Fertility = 0f
                    }
                },
            };
        }

        private static void InitFloorDefs()
        {
            _defaultFloor = new FloorDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta, MoveCost = 1, Beauty = 0
            };

            _floorDefs = new Dictionary<int, FloorDef>
            {
                {
                    1, new FloorDef
                    {
                        Id = 1, Name = "WoodFloor",
                        Color = new Color(0.65f, 0.50f, 0.30f),
                        MoveCost = 1, Beauty = 1
                    }
                },
                {
                    2, new FloorDef
                    {
                        Id = 2, Name = "StoneFloor",
                        Color = new Color(0.70f, 0.70f, 0.70f),
                        MoveCost = 1, Beauty = 2
                    }
                },
                {
                    3, new FloorDef
                    {
                        Id = 3, Name = "TileFloor",
                        Color = new Color(0.80f, 0.75f, 0.65f),
                        MoveCost = 1, Beauty = 3
                    }
                },
            };
        }

        private static void InitWallDefs()
        {
            _defaultWall = new WallDef
            {
                Id = 0, Name = "Unknown",
                Color = Color.magenta,
                DoorColor = Color.magenta,
                WindowColor = Color.magenta,
                MaxHealth = 100, IsTransparent = false
            };

            _wallDefs = new Dictionary<int, WallDef>
            {
                {
                    1, new WallDef
                    {
                        Id = 1, Name = "WoodWall",
                        Color = new Color(0.75f, 0.75f, 0.70f),
                        DoorColor = new Color(0.50f, 0.35f, 0.20f),
                        WindowColor = new Color(0.60f, 0.80f, 0.90f),
                        MaxHealth = 100, IsTransparent = false
                    }
                },
                {
                    2, new WallDef
                    {
                        Id = 2, Name = "StoneWall",
                        Color = new Color(0.60f, 0.58f, 0.55f),
                        DoorColor = new Color(0.45f, 0.30f, 0.15f),
                        WindowColor = new Color(0.55f, 0.75f, 0.85f),
                        MaxHealth = 200, IsTransparent = false
                    }
                },
            };
        }

        public static TerrainDef GetTerrainDef(int id)
        {
            return _terrainDefs.GetValueOrDefault(id, _defaultTerrain);
        }

        public static FloorDef GetFloorDef(int id)
        {
            return _floorDefs.GetValueOrDefault(id, _defaultFloor);
        }

        public static WallDef GetWallDef(int id)
        {
            return _wallDefs.GetValueOrDefault(id, _defaultWall);
        }
    }
}
