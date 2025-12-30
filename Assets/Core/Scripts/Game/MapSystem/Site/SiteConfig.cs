/*******************************************************************************
 * 文件名:    SiteConfig.cs
 * 描述:      场景配置类，定义场景的基本参数
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   SiteConfig 包含创建和生成场景所需的所有配置参数：
 *   - 地图尺寸
 *   - 楼层范围
 *   - 随机种子
 *   - 生物群系
 *   - 预设建筑
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 场景配置
    /// </summary>
    [Serializable]
    public class SiteConfig
    {
        #region 基础配置

        /// <summary>
        /// 场景ID（唯一标识）
        /// </summary>
        [SerializeField]
        private string _siteId;

        /// <summary>
        /// 场景名称
        /// </summary>
        [SerializeField]
        private string _siteName;

        /// <summary>
        /// 随机种子（用于程序化生成）
        /// </summary>
        [SerializeField]
        private int _seed;

        #endregion

        #region 尺寸配置

        /// <summary>
        /// 地图X方向尺寸（格子数）
        /// </summary>
        [SerializeField]
        private int _sizeX = 100;

        /// <summary>
        /// 地图Z方向尺寸（格子数）
        /// </summary>
        [SerializeField]
        private int _sizeZ = 100;

        /// <summary>
        /// 最低楼层索引（负数表示地下层）
        /// </summary>
        [SerializeField]
        private int _minFloor = -1;

        /// <summary>
        /// 最高楼层索引
        /// </summary>
        [SerializeField]
        private int _maxFloor = 2;

        /// <summary>
        /// 格子尺寸（世界单位）
        /// </summary>
        [SerializeField]
        private float _cellSize = 1f;

        /// <summary>
        /// 楼层高度（世界单位）
        /// </summary>
        [SerializeField]
        private float _floorHeight = 3f;

        #endregion

        #region 生成配置

        /// <summary>
        /// 生物群系ID（影响地形、植被、天气等）
        /// </summary>
        [SerializeField]
        private string _biomeId;

        /// <summary>
        /// 地形生成器ID
        /// </summary>
        [SerializeField]
        private string _terrainGeneratorId;

        /// <summary>
        /// 预设建筑配置列表
        /// </summary>
        [SerializeField]
        private List<PresetBuildingConfig> _presetBuildings = new List<PresetBuildingConfig>();

        /// <summary>
        /// 是否生成洞穴/地下结构
        /// </summary>
        [SerializeField]
        private bool _generateCaves = true;

        /// <summary>
        /// 是否生成水域
        /// </summary>
        [SerializeField]
        private bool _generateWater = true;

        /// <summary>
        /// 是否生成矿脉
        /// </summary>
        [SerializeField]
        private bool _generateOres = true;

        /// <summary>
        /// 初始植被密度（0-1）
        /// </summary>
        [SerializeField]
        private float _plantDensity = 0.5f;

        #endregion

        #region 属性访问

        public string SiteId
        {
            get => _siteId;
            set => _siteId = value;
        }

        public string SiteName
        {
            get => _siteName;
            set => _siteName = value;
        }

        public int Seed
        {
            get => _seed;
            set => _seed = value;
        }

        public int SizeX
        {
            get => _sizeX;
            set => _sizeX = Mathf.Max(1, value);
        }

        public int SizeZ
        {
            get => _sizeZ;
            set => _sizeZ = Mathf.Max(1, value);
        }

        public int MinFloor
        {
            get => _minFloor;
            set => _minFloor = Mathf.Min(value, _maxFloor);
        }

        public int MaxFloor
        {
            get => _maxFloor;
            set => _maxFloor = Mathf.Max(value, _minFloor);
        }

        public float CellSize
        {
            get => _cellSize;
            set => _cellSize = Mathf.Max(0.1f, value);
        }

        public float FloorHeight
        {
            get => _floorHeight;
            set => _floorHeight = Mathf.Max(0.1f, value);
        }

        public string BiomeId
        {
            get => _biomeId;
            set => _biomeId = value;
        }

        public string TerrainGeneratorId
        {
            get => _terrainGeneratorId;
            set => _terrainGeneratorId = value;
        }

        public List<PresetBuildingConfig> PresetBuildings => _presetBuildings;

        public bool GenerateCaves
        {
            get => _generateCaves;
            set => _generateCaves = value;
        }

        public bool GenerateWater
        {
            get => _generateWater;
            set => _generateWater = value;
        }

        public bool GenerateOres
        {
            get => _generateOres;
            set => _generateOres = value;
        }

        public float PlantDensity
        {
            get => _plantDensity;
            set => _plantDensity = Mathf.Clamp01(value);
        }

        #endregion

        #region 派生属性

        /// <summary>
        /// 地图尺寸（IntVec2）
        /// </summary>
        public IntVec2 Size => new IntVec2(_sizeX, _sizeZ);

        /// <summary>
        /// 单层格子总数
        /// </summary>
        public int CellCount => _sizeX * _sizeZ;

        /// <summary>
        /// 楼层总数
        /// </summary>
        public int FloorCount => _maxFloor - _minFloor + 1;

        /// <summary>
        /// 全部格子总数（所有楼层）
        /// </summary>
        public int TotalCellCount => CellCount * FloorCount;

        /// <summary>
        /// 地图世界尺寸X
        /// </summary>
        public float WorldSizeX => _sizeX * _cellSize;

        /// <summary>
        /// 地图世界尺寸Z
        /// </summary>
        public float WorldSizeZ => _sizeZ * _cellSize;

        /// <summary>
        /// 地图中心（世界坐标）
        /// </summary>
        public Vector3 WorldCenter => new Vector3(
            WorldSizeX * 0.5f,
            0,
            WorldSizeZ * 0.5f
        );

        /// <summary>
        /// 是否有地下层
        /// </summary>
        public bool HasUnderground => _minFloor < 0;

        /// <summary>
        /// 是否有多层
        /// </summary>
        public bool IsMultiFloor => FloorCount > 1;

        /// <summary>
        /// 地下层数量
        /// </summary>
        public int UndergroundFloorCount => _minFloor < 0 ? -_minFloor : 0;

        /// <summary>
        /// 地上层数量（包括地面层）
        /// </summary>
        public int AbovegroundFloorCount => _maxFloor >= 0 ? _maxFloor + 1 : 0;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SiteConfig()
        {
            _siteId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// 带尺寸的构造函数
        /// </summary>
        public SiteConfig(int sizeX, int sizeZ, int minFloor = 0, int maxFloor = 0)
            : this()
        {
            _sizeX = Mathf.Max(1, sizeX);
            _sizeZ = Mathf.Max(1, sizeZ);
            _minFloor = minFloor;
            _maxFloor = Mathf.Max(minFloor, maxFloor);
        }

        /// <summary>
        /// 完整构造函数
        /// </summary>
        public SiteConfig(string siteId, string siteName, int sizeX, int sizeZ, 
            int minFloor, int maxFloor, int seed)
        {
            _siteId = siteId;
            _siteName = siteName;
            _sizeX = Mathf.Max(1, sizeX);
            _sizeZ = Mathf.Max(1, sizeZ);
            _minFloor = minFloor;
            _maxFloor = Mathf.Max(minFloor, maxFloor);
            _seed = seed;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 检查楼层索引是否有效
        /// </summary>
        public bool IsValidFloor(int floorIndex)
        {
            return floorIndex >= _minFloor && floorIndex <= _maxFloor;
        }

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool IsInBounds(CellCoord cell)
        {
            return cell.x >= 0 && cell.x < _sizeX && 
                   cell.z >= 0 && cell.z < _sizeZ;
        }

        /// <summary>
        /// 检查全局坐标是否在范围内
        /// </summary>
        public bool IsInBounds(GlobalCoord coord)
        {
            return coord.x >= 0 && coord.x < _sizeX &&
                   coord.z >= 0 && coord.z < _sizeZ &&
                   coord.y >= _minFloor && coord.y <= _maxFloor;
        }

        /// <summary>
        /// 楼层索引转数组索引
        /// </summary>
        public int FloorToArrayIndex(int floorIndex)
        {
            return floorIndex - _minFloor;
        }

        /// <summary>
        /// 数组索引转楼层索引
        /// </summary>
        public int ArrayIndexToFloor(int arrayIndex)
        {
            return arrayIndex + _minFloor;
        }

        /// <summary>
        /// 获取随机数生成器（基于种子）
        /// </summary>
        public System.Random GetSeededRandom()
        {
            return new System.Random(_seed);
        }

        /// <summary>
        /// 获取指定用途的子种子
        /// </summary>
        public int GetSubSeed(string purpose)
        {
            unchecked
            {
                int hash = _seed;
                foreach (char c in purpose)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        public SiteConfig Clone()
        {
            var clone = new SiteConfig
            {
                _siteId = _siteId,
                _siteName = _siteName,
                _seed = _seed,
                _sizeX = _sizeX,
                _sizeZ = _sizeZ,
                _minFloor = _minFloor,
                _maxFloor = _maxFloor,
                _cellSize = _cellSize,
                _floorHeight = _floorHeight,
                _biomeId = _biomeId,
                _terrainGeneratorId = _terrainGeneratorId,
                _generateCaves = _generateCaves,
                _generateWater = _generateWater,
                _generateOres = _generateOres,
                _plantDensity = _plantDensity
            };

            foreach (var preset in _presetBuildings)
            {
                clone._presetBuildings.Add(preset.Clone());
            }

            return clone;
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建小型地图配置
        /// </summary>
        public static SiteConfig CreateSmall(int seed = 0)
        {
            return new SiteConfig
            {
                _siteId = "small_" + seed,
                _siteName = "小型地图",
                _sizeX = 50,
                _sizeZ = 50,
                _minFloor = 0,
                _maxFloor = 1,
                _seed = seed != 0 ? seed : UnityEngine.Random.Range(int.MinValue, int.MaxValue)
            };
        }

        /// <summary>
        /// 创建中型地图配置
        /// </summary>
        public static SiteConfig CreateMedium(int seed = 0)
        {
            return new SiteConfig
            {
                _siteId = "medium_" + seed,
                _siteName = "中型地图",
                _sizeX = 100,
                _sizeZ = 100,
                _minFloor = -1,
                _maxFloor = 2,
                _seed = seed != 0 ? seed : UnityEngine.Random.Range(int.MinValue, int.MaxValue)
            };
        }

        /// <summary>
        /// 创建大型地图配置
        /// </summary>
        public static SiteConfig CreateLarge(int seed = 0)
        {
            return new SiteConfig
            {
                _siteId = "large_" + seed,
                _siteName = "大型地图",
                _sizeX = 200,
                _sizeZ = 200,
                _minFloor = -2,
                _maxFloor = 3,
                _seed = seed != 0 ? seed : UnityEngine.Random.Range(int.MinValue, int.MaxValue)
            };
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"SiteConfig({_siteId}): {_sizeX}x{_sizeZ}, Floors[{_minFloor}~{_maxFloor}], Seed={_seed}";
        }

        #endregion
    }

    /// <summary>
    /// 预设建筑配置
    /// </summary>
    [Serializable]
    public class PresetBuildingConfig
    {
        /// <summary>
        /// 预设建筑定义ID
        /// </summary>
        public string PresetDefId;

        /// <summary>
        /// 放置位置（如果指定）
        /// </summary>
        public CellCoord? Position;

        /// <summary>
        /// 放置楼层
        /// </summary>
        public int FloorIndex;

        /// <summary>
        /// 是否随机位置
        /// </summary>
        public bool RandomPosition;

        /// <summary>
        /// 旋转
        /// </summary>
        public Rotation Rotation;

        /// <summary>
        /// 是否必须生成
        /// </summary>
        public bool Required;

        /// <summary>
        /// 生成权重（随机选择时使用）
        /// </summary>
        public float Weight = 1f;

        public PresetBuildingConfig Clone()
        {
            return new PresetBuildingConfig
            {
                PresetDefId = PresetDefId,
                Position = Position,
                FloorIndex = FloorIndex,
                RandomPosition = RandomPosition,
                Rotation = Rotation,
                Required = Required,
                Weight = Weight
            };
        }
    }
}
