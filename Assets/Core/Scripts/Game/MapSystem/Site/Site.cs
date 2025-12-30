/*******************************************************************************
 * 文件名:    Site.cs
 * 描述:      场景容器类，管理多个楼层和跨楼层系统
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Site 是整个地图场景的顶层容器，包含：
 *   - 多个Floor（楼层）
 *   - FloorConnectionManager（楼层连接管理）
 *   - 跨楼层系统（寻路、实体查询等）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 场景容器
    /// </summary>
    public class Site
    {
        #region 字段

        /// <summary>
        /// 场景配置
        /// </summary>
        private SiteConfig _config;

        /// <summary>
        /// 楼层数组（按数组索引访问，不是楼层索引）
        /// </summary>
        private Floor[] _floors;

        /// <summary>
        /// 楼层连接管理器
        /// </summary>
        private FloorConnectionManager _connectionManager;

        /// <summary>
        /// 实体管理器
        /// </summary>
        private EntityManager _entityManager;

        /// <summary>
        /// 寻路器
        /// </summary>
        private Pathfinder _pathfinder;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// 当前游戏时间（Tick数）
        /// </summary>
        private long _gameTick;

        #endregion

        #region 属性

        /// <summary>
        /// 场景配置
        /// </summary>
        public SiteConfig Config => _config;

        /// <summary>
        /// 场景ID
        /// </summary>
        public string SiteId => _config.SiteId;

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SiteName => _config.SiteName;

        /// <summary>
        /// X方向尺寸
        /// </summary>
        public int SizeX => _config.SizeX;

        /// <summary>
        /// Z方向尺寸
        /// </summary>
        public int SizeZ => _config.SizeZ;

        /// <summary>
        /// 尺寸
        /// </summary>
        public IntVec2 Size => _config.Size;

        /// <summary>
        /// 最低楼层
        /// </summary>
        public int MinFloor => _config.MinFloor;

        /// <summary>
        /// 最高楼层
        /// </summary>
        public int MaxFloor => _config.MaxFloor;

        /// <summary>
        /// 楼层数量
        /// </summary>
        public int FloorCount => _config.FloorCount;

        /// <summary>
        /// 格子尺寸
        /// </summary>
        public float CellSize => _config.CellSize;

        /// <summary>
        /// 楼层高度
        /// </summary>
        public float FloorHeight => _config.FloorHeight;

        /// <summary>
        /// 楼层连接管理器
        /// </summary>
        public FloorConnectionManager ConnectionManager => _connectionManager;

        /// <summary>
        /// 实体管理器
        /// </summary>
        public EntityManager EntityManager => _entityManager;

        /// <summary>
        /// 寻路器
        /// </summary>
        public Pathfinder Pathfinder => _pathfinder;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// 当前游戏Tick
        /// </summary>
        public long GameTick => _gameTick;

        /// <summary>
        /// 随机数生成器（基于种子）
        /// </summary>
        public System.Random SeededRandom { get; private set; }

        #endregion

        #region 楼层访问

        /// <summary>
        /// 获取楼层（通过楼层索引）
        /// </summary>
        public Floor GetFloor(int floorIndex)
        {
            if (!_config.IsValidFloor(floorIndex))
                return null;

            int arrayIndex = _config.FloorToArrayIndex(floorIndex);
            return _floors[arrayIndex];
        }

        /// <summary>
        /// 获取地面层
        /// </summary>
        public Floor GroundFloor => GetFloor(0);

        /// <summary>
        /// 所有楼层
        /// </summary>
        public IEnumerable<Floor> AllFloors
        {
            get
            {
                for (int i = 0; i < _floors.Length; i++)
                {
                    yield return _floors[i];
                }
            }
        }

        /// <summary>
        /// 按楼层索引遍历（从低到高）
        /// </summary>
        public IEnumerable<Floor> FloorsAscending
        {
            get
            {
                for (int floorIndex = _config.MinFloor; floorIndex <= _config.MaxFloor; floorIndex++)
                {
                    yield return GetFloor(floorIndex);
                }
            }
        }

        /// <summary>
        /// 按楼层索引遍历（从高到低）
        /// </summary>
        public IEnumerable<Floor> FloorsDescending
        {
            get
            {
                for (int floorIndex = _config.MaxFloor; floorIndex >= _config.MinFloor; floorIndex--)
                {
                    yield return GetFloor(floorIndex);
                }
            }
        }

        /// <summary>
        /// 索引器访问楼层
        /// </summary>
        public Floor this[int floorIndex] => GetFloor(floorIndex);

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">场景配置</param>
        public Site(SiteConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            // 创建随机数生成器
            SeededRandom = _config.GetSeededRandom();

            // 创建楼层数组
            _floors = new Floor[_config.FloorCount];
            for (int i = 0; i < _floors.Length; i++)
            {
                int floorIndex = _config.ArrayIndexToFloor(i);
                _floors[i] = new Floor(this, floorIndex, _config.SizeX, _config.SizeZ);
            }

            // 创建楼层连接管理器
            _connectionManager = new FloorConnectionManager(this);

            // 创建实体管理器
            _entityManager = new EntityManager(this);

            // 创建寻路器
            _pathfinder = new Pathfinder(this);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化场景
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            Debug.Log($"[Site] Initializing site: {_config}");

            // 初始化所有楼层
            foreach (var floor in _floors)
            {
                floor.Initialize();
            }

            // 初始化连接管理器
            _connectionManager.Initialize();

            // 初始化实体管理器
            _entityManager.Initialize();

            _initialized = true;

            Debug.Log($"[Site] Initialization complete");
        }

        /// <summary>
        /// 使用默认数据填充场景
        /// </summary>
        public void FillWithDefaults()
        {
            foreach (var floor in _floors)
            {
                floor.FillWithDefaults();
            }
        }

        #endregion

        #region 坐标验证

        /// <summary>
        /// 检查楼层索引是否有效
        /// </summary>
        public bool IsValidFloor(int floorIndex)
        {
            return _config.IsValidFloor(floorIndex);
        }

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool InBounds(CellCoord cell)
        {
            return _config.IsInBounds(cell);
        }

        /// <summary>
        /// 检查全局坐标是否在范围内
        /// </summary>
        public bool InBounds(GlobalCoord coord)
        {
            return _config.IsInBounds(coord);
        }

        #endregion

        #region 全局坐标访问

        /// <summary>
        /// 获取地形（通过全局坐标）
        /// </summary>
        public TerrainDef GetTerrain(GlobalCoord coord)
        {
            var floor = GetFloor(coord.y);
            return floor?.GetTerrain(coord.ToCellCoord());
        }

        /// <summary>
        /// 设置地形（通过全局坐标）
        /// </summary>
        public void SetTerrain(GlobalCoord coord, string terrainDefId)
        {
            var floor = GetFloor(coord.y);
            floor?.SetTerrain(coord.ToCellCoord(), terrainDefId);
        }

        /// <summary>
        /// 获取墙壁（通过全局坐标）
        /// </summary>
        public WallDef GetWall(GlobalCoord coord)
        {
            var floor = GetFloor(coord.y);
            return floor?.GetWall(coord.ToCellCoord());
        }

        /// <summary>
        /// 设置墙壁（通过全局坐标）
        /// </summary>
        public void SetWall(GlobalCoord coord, string wallDefId)
        {
            var floor = GetFloor(coord.y);
            floor?.SetWall(coord.ToCellCoord(), wallDefId);
        }

        /// <summary>
        /// 获取承重等级（通过全局坐标）
        /// </summary>
        public BearingCapacity GetBearingCapacity(GlobalCoord coord)
        {
            var floor = GetFloor(coord.y);
            return floor?.GetBearingCapacity(coord.ToCellCoord()) ?? BearingCapacity.None;
        }

        /// <summary>
        /// 获取通行性（通过全局坐标）
        /// </summary>
        public Passability GetPassability(GlobalCoord coord)
        {
            var floor = GetFloor(coord.y);
            return floor?.GetPassability(coord.ToCellCoord()) ?? Passability.Impassable;
        }

        /// <summary>
        /// 检查是否可通行（通过全局坐标）
        /// </summary>
        public bool IsPassable(GlobalCoord coord)
        {
            return GetPassability(coord) != Passability.Impassable;
        }

        /// <summary>
        /// 获取寻路代价（通过全局坐标）
        /// </summary>
        public int GetPathCost(GlobalCoord coord)
        {
            var floor = GetFloor(coord.y);
            return floor?.GetPathCost(coord.ToCellCoord()) ?? int.MaxValue;
        }

        #endregion

        #region Tick更新

        /// <summary>
        /// 每Tick更新
        /// </summary>
        public void Tick()
        {
            _gameTick++;

            // 更新实体
            _entityManager?.Tick();
        }

        /// <summary>
        /// 稀有Tick更新（每250 Tick）
        /// </summary>
        public void TickRare()
        {
            _entityManager?.TickRare();
        }

        /// <summary>
        /// 长周期Tick更新（每2000 Tick）
        /// </summary>
        public void TickLong()
        {
            _entityManager?.TickLong();
        }

        #endregion

        #region 世界坐标转换

        /// <summary>
        /// 世界坐标转全局坐标
        /// </summary>
        public GlobalCoord WorldToGlobal(Vector3 worldPos)
        {
            return GlobalCoord.FromWorldPosition(worldPos, _config.CellSize, _config.FloorHeight);
        }

        /// <summary>
        /// 全局坐标转世界坐标
        /// </summary>
        public Vector3 GlobalToWorld(GlobalCoord coord)
        {
            return coord.ToWorldPosition(_config.CellSize, _config.FloorHeight);
        }

        /// <summary>
        /// 获取楼层的世界Y坐标
        /// </summary>
        public float GetFloorWorldY(int floorIndex)
        {
            return floorIndex * _config.FloorHeight;
        }

        #endregion

        #region 随机位置

        /// <summary>
        /// 获取随机格子坐标
        /// </summary>
        public CellCoord GetRandomCell()
        {
            return new CellCoord(
                SeededRandom.Next(0, _config.SizeX),
                SeededRandom.Next(0, _config.SizeZ)
            );
        }

        /// <summary>
        /// 获取随机全局坐标
        /// </summary>
        public GlobalCoord GetRandomGlobalCoord()
        {
            return new GlobalCoord(
                SeededRandom.Next(0, _config.SizeX),
                SeededRandom.Next(_config.MinFloor, _config.MaxFloor + 1),
                SeededRandom.Next(0, _config.SizeZ)
            );
        }

        /// <summary>
        /// 获取随机可通行位置
        /// </summary>
        /// <param name="floorIndex">楼层索引</param>
        /// <param name="maxAttempts">最大尝试次数</param>
        public CellCoord? GetRandomPassableCell(int floorIndex, int maxAttempts = 100)
        {
            var floor = GetFloor(floorIndex);
            if (floor == null)
                return null;

            for (int i = 0; i < maxAttempts; i++)
            {
                var cell = GetRandomCell();
                if (floor.IsPassable(cell))
                    return cell;
            }

            return null;
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印场景信息
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log($"=== Site: {_config.SiteName} ===");
            Debug.Log($"  Size: {_config.SizeX} x {_config.SizeZ}");
            Debug.Log($"  Floors: {_config.MinFloor} to {_config.MaxFloor} ({_config.FloorCount} total)");
            Debug.Log($"  Seed: {_config.Seed}");
            Debug.Log($"  Initialized: {_initialized}");
            Debug.Log($"  Game Tick: {_gameTick}");

            foreach (var floor in _floors)
            {
                Debug.Log($"  - {floor}");
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"Site({_config.SiteId}): {_config.SizeX}x{_config.SizeZ}, {_config.FloorCount} floors";
        }

        #endregion
    }
}
