/*******************************************************************************
 * 文件名:    Floor.cs
 * 描述:      单层楼层类，包含该层的所有Tile和Entity数据
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Floor 是单个楼层的容器，包含：
 *   - 六层TileGrid数据
 *   - EntityGrid实体空间索引
 *   - 寻路数据
 *   - 环境数据（温度、光照等）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 单层楼层
    /// </summary>
    public class Floor
    {
        #region 字段

        /// <summary>
        /// 所属Site
        /// </summary>
        private Site _parentSite;

        /// <summary>
        /// 楼层索引
        /// </summary>
        private int _floorIndex;

        /// <summary>
        /// 楼层类型
        /// </summary>
        private FloorType _floorType;

        /// <summary>
        /// X方向尺寸
        /// </summary>
        private int _sizeX;

        /// <summary>
        /// Z方向尺寸
        /// </summary>
        private int _sizeZ;

        /// <summary>
        /// 六层TileGrid
        /// </summary>
        private TileGrid[] _tileGrids;

        /// <summary>
        /// 区域网格
        /// </summary>
        private RegionGrid _regionGrid;

        /// <summary>
        /// 房间管理器
        /// </summary>
        private RoomManager _roomManager;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _initialized;

        #endregion

        #region 属性

        /// <summary>
        /// 所属Site
        /// </summary>
        public Site ParentSite => _parentSite;

        /// <summary>
        /// 楼层索引
        /// </summary>
        public int FloorIndex => _floorIndex;

        /// <summary>
        /// 楼层类型
        /// </summary>
        public FloorType FloorType => _floorType;

        /// <summary>
        /// X方向尺寸
        /// </summary>
        public int SizeX => _sizeX;

        /// <summary>
        /// Z方向尺寸
        /// </summary>
        public int SizeZ => _sizeZ;

        /// <summary>
        /// 尺寸
        /// </summary>
        public IntVec2 Size => new IntVec2(_sizeX, _sizeZ);

        /// <summary>
        /// 格子总数
        /// </summary>
        public int CellCount => _sizeX * _sizeZ;

        /// <summary>
        /// 是否是地面层
        /// </summary>
        public bool IsGroundFloor => _floorIndex == 0;

        /// <summary>
        /// 是否是地下层
        /// </summary>
        public bool IsUnderground => _floorIndex < 0;

        /// <summary>
        /// 是否是地上层
        /// </summary>
        public bool IsAboveground => _floorIndex > 0;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// 实体空间索引（通过Site.EntityManager获取）
        /// </summary>
        public EntityGrid EntityGrid => _parentSite?.EntityManager?.GetEntityGrid(_floorIndex);

        /// <summary>
        /// 实体分类索引（通过Site.EntityManager获取）
        /// </summary>
        public EntityLister EntityLister => _parentSite?.EntityManager?.GetEntityLister(_floorIndex);

        /// <summary>
        /// 区域网格
        /// </summary>
        public RegionGrid RegionGrid => _regionGrid;

        /// <summary>
        /// 房间管理器
        /// </summary>
        public RoomManager RoomManager => _roomManager;

        #endregion

        #region TileGrid访问

        /// <summary>
        /// 地形层
        /// </summary>
        public TileGrid TerrainGrid => _tileGrids[(int)TileLayer.Terrain];

        /// <summary>
        /// 地基层
        /// </summary>
        public TileGrid FoundationGrid => _tileGrids[(int)TileLayer.Foundation];

        /// <summary>
        /// 地板层
        /// </summary>
        public TileGrid FloorGrid => _tileGrids[(int)TileLayer.Floor];

        /// <summary>
        /// 覆盖层
        /// </summary>
        public TileGrid CoverGrid => _tileGrids[(int)TileLayer.Cover];

        /// <summary>
        /// 墙壁层
        /// </summary>
        public TileGrid WallGrid => _tileGrids[(int)TileLayer.Wall];

        /// <summary>
        /// 屋顶层
        /// </summary>
        public TileGrid RoofGrid => _tileGrids[(int)TileLayer.Roof];

        /// <summary>
        /// 获取指定层的TileGrid
        /// </summary>
        public TileGrid GetTileGrid(TileLayer layer)
        {
            return _tileGrids[(int)layer];
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parentSite">所属Site</param>
        /// <param name="floorIndex">楼层索引</param>
        /// <param name="sizeX">X方向尺寸</param>
        /// <param name="sizeZ">Z方向尺寸</param>
        public Floor(Site parentSite, int floorIndex, int sizeX, int sizeZ)
        {
            _parentSite = parentSite;
            _floorIndex = floorIndex;
            _sizeX = sizeX;
            _sizeZ = sizeZ;

            // 确定楼层类型
            if (floorIndex < 0)
                _floorType = FloorType.Underground;
            else if (floorIndex == 0)
                _floorType = FloorType.Ground;
            else
                _floorType = FloorType.Aboveground;

            // 创建六层TileGrid
            _tileGrids = new TileGrid[TileLayerExtensions.LayerCount];
            for (int i = 0; i < TileLayerExtensions.LayerCount; i++)
            {
                _tileGrids[i] = new TileGrid(this, (TileLayer)i, sizeX, sizeZ);
            }

            // 创建区域网格和房间管理器
            _regionGrid = new RegionGrid(this);
            _roomManager = new RoomManager(this, _regionGrid);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化楼层
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            // 初始化所有TileGrid
            foreach (var grid in _tileGrids)
            {
                grid.Initialize();
            }

            // 初始化房间管理器（区域会在需要时构建）
            _roomManager.Initialize();

            _initialized = true;
        }

        /// <summary>
        /// 重建区域和房间
        /// </summary>
        public void RebuildRegionsAndRooms()
        {
            _regionGrid.RebuildAll();
            _roomManager.RebuildIfNeeded();
        }

        /// <summary>
        /// 使用默认Tile填充
        /// </summary>
        public void FillWithDefaults()
        {
            // 填充默认地形
            string defaultTerrainId = _floorIndex < 0 
                ? DefaultDefs.TerrainRock 
                : DefaultDefs.TerrainDirt;
            TerrainGrid.Fill(defaultTerrainId);

            // 地下层填充岩石墙和岩石顶
            if (_floorIndex < 0)
            {
                WallGrid.Fill(DefaultDefs.WallStone);
                RoofGrid.Fill(DefaultDefs.RoofRockThick);
            }
            else
            {
                // 地上层默认无墙无顶
                WallGrid.Fill(DefaultDefs.WallNone);
                RoofGrid.Fill(DefaultDefs.RoofNone);
            }

            // 其他层默认为空
            FoundationGrid.Fill(DefaultDefs.FoundationNone);
            FloorGrid.Fill(DefaultDefs.FloorNone);
            // CoverGrid 默认为 null（无覆盖物）
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool InBounds(CellCoord cell)
        {
            return cell.x >= 0 && cell.x < _sizeX && 
                   cell.z >= 0 && cell.z < _sizeZ;
        }

        /// <summary>
        /// 检查坐标是否在范围内
        /// </summary>
        public bool InBounds(int x, int z)
        {
            return x >= 0 && x < _sizeX && z >= 0 && z < _sizeZ;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        public int CellToIndex(CellCoord cell)
        {
            return cell.z * _sizeX + cell.x;
        }

        /// <summary>
        /// 坐标转索引
        /// </summary>
        public int CellToIndex(int x, int z)
        {
            return z * _sizeX + x;
        }

        /// <summary>
        /// 索引转坐标
        /// </summary>
        public CellCoord IndexToCell(int index)
        {
            return new CellCoord(index % _sizeX, index / _sizeX);
        }

        /// <summary>
        /// 转换为全局坐标
        /// </summary>
        public GlobalCoord ToGlobalCoord(CellCoord cell)
        {
            return new GlobalCoord(cell.x, _floorIndex, cell.z);
        }

        /// <summary>
        /// 从全局坐标提取本层坐标
        /// </summary>
        public CellCoord FromGlobalCoord(GlobalCoord coord)
        {
            return new CellCoord(coord.x, coord.z);
        }

        #endregion

        #region Tile访问快捷方法

        /// <summary>
        /// 获取地形
        /// </summary>
        public TerrainDef GetTerrain(CellCoord cell)
        {
            return TerrainGrid.GetDef<TerrainDef>(cell);
        }

        /// <summary>
        /// 获取地形
        /// </summary>
        public TerrainDef GetTerrain(int x, int z)
        {
            return TerrainGrid.GetDef<TerrainDef>(x, z);
        }

        /// <summary>
        /// 设置地形
        /// </summary>
        public void SetTerrain(CellCoord cell, string terrainDefId)
        {
            TerrainGrid.SetTile(cell, terrainDefId);
        }

        /// <summary>
        /// 设置地形
        /// </summary>
        public void SetTerrain(CellCoord cell, TerrainDef terrainDef)
        {
            TerrainGrid.SetTile(cell, terrainDef?.DefId);
        }

        /// <summary>
        /// 获取地板
        /// </summary>
        public FloorDef GetFloor(CellCoord cell)
        {
            return FloorGrid.GetDef<FloorDef>(cell);
        }

        /// <summary>
        /// 设置地板
        /// </summary>
        public void SetFloor(CellCoord cell, string floorDefId)
        {
            FloorGrid.SetTile(cell, floorDefId);
        }

        /// <summary>
        /// 获取墙壁
        /// </summary>
        public WallDef GetWall(CellCoord cell)
        {
            return WallGrid.GetDef<WallDef>(cell);
        }

        /// <summary>
        /// 设置墙壁
        /// </summary>
        public void SetWall(CellCoord cell, string wallDefId)
        {
            WallGrid.SetTile(cell, wallDefId);
        }

        /// <summary>
        /// 获取屋顶
        /// </summary>
        public RoofDef GetRoof(CellCoord cell)
        {
            return RoofGrid.GetDef<RoofDef>(cell);
        }

        /// <summary>
        /// 设置屋顶
        /// </summary>
        public void SetRoof(CellCoord cell, string roofDefId)
        {
            RoofGrid.SetTile(cell, roofDefId);
        }

        #endregion

        #region 综合查询

        /// <summary>
        /// 获取格子的承重等级（综合地形、地基、地板）
        /// </summary>
        public BearingCapacity GetBearingCapacity(CellCoord cell)
        {
            // 从下到上取最高承重
            BearingCapacity result = BearingCapacity.None;

            // 地形基础承重
            var terrain = GetTerrain(cell);
            if (terrain != null)
            {
                result = terrain.BearingCapacity;
            }

            // 地基可能提升承重
            var foundation = FoundationGrid.GetDef<FoundationDef>(cell);
            if (foundation != null && foundation.ProvidedCapacity > result)
            {
                // 检查地形是否满足地基要求
                if (terrain != null && terrain.BearingCapacity >= foundation.RequiredTerrainCapacity)
                {
                    result = foundation.ProvidedCapacity;
                }
            }

            // 地板可能提升承重
            var floor = GetFloor(cell);
            if (floor != null && floor.ProvidedCapacity > result)
            {
                if (result >= floor.RequiredCapacity)
                {
                    result = floor.ProvidedCapacity;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取格子的可通行性（综合地形和墙壁）
        /// </summary>
        public Passability GetPassability(CellCoord cell)
        {
            // 墙壁优先级最高
            var wall = GetWall(cell);
            if (wall != null && wall.WallType != WallType.None)
            {
                return wall.Passability;
            }

            // 检查地形
            var terrain = GetTerrain(cell);
            if (terrain != null)
            {
                return terrain.Passability;
            }

            return Passability.Passable;
        }

        /// <summary>
        /// 检查格子是否可通行
        /// </summary>
        public bool IsPassable(CellCoord cell)
        {
            return GetPassability(cell) != Passability.Impassable;
        }

        /// <summary>
        /// 获取格子的寻路代价
        /// </summary>
        public int GetPathCost(CellCoord cell)
        {
            int cost = 0;

            // 地形基础代价
            var terrain = GetTerrain(cell);
            if (terrain != null)
            {
                cost += terrain.PathCost;
            }

            // 地板修正
            var floor = GetFloor(cell);
            if (floor != null)
            {
                cost += floor.PathCostModifier;
            }

            // 覆盖物修正
            var cover = CoverGrid.GetDef<CoverDef>(cell);
            if (cover != null)
            {
                cost += cover.PathCostModifier;
            }

            return Mathf.Max(1, cost);
        }

        /// <summary>
        /// 检查格子是否有屋顶
        /// </summary>
        public bool HasRoof(CellCoord cell)
        {
            var roof = GetRoof(cell);
            return roof != null && roof.RoofType != RoofType.None;
        }

        /// <summary>
        /// 检查格子是否在室内
        /// </summary>
        public bool IsIndoors(CellCoord cell)
        {
            // 简单实现：有屋顶就算室内
            // TODO: 完整实现需要检查封闭房间
            return HasRoof(cell);
        }

        /// <summary>
        /// 检查是否可以在该位置建造
        /// </summary>
        public bool CanBuildAt(CellCoord cell, BearingCapacity requiredBearing)
        {
            if (!InBounds(cell))
                return false;

            // 检查承重
            if (GetBearingCapacity(cell) < requiredBearing)
                return false;

            // 检查是否已有墙壁
            var wall = GetWall(cell);
            if (wall != null && wall.WallType != WallType.None)
                return false;

            return true;
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 遍历所有格子
        /// </summary>
        public IEnumerable<CellCoord> AllCells()
        {
            for (int z = 0; z < _sizeZ; z++)
            {
                for (int x = 0; x < _sizeX; x++)
                {
                    yield return new CellCoord(x, z);
                }
            }
        }

        /// <summary>
        /// 遍历矩形区域内的格子
        /// </summary>
        public IEnumerable<CellCoord> CellsInRect(CellCoord min, CellCoord max)
        {
            int minX = Mathf.Max(0, min.x);
            int minZ = Mathf.Max(0, min.z);
            int maxX = Mathf.Min(_sizeX - 1, max.x);
            int maxZ = Mathf.Min(_sizeZ - 1, max.z);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    yield return new CellCoord(x, z);
                }
            }
        }

        /// <summary>
        /// 遍历边界格子
        /// </summary>
        public IEnumerable<CellCoord> EdgeCells()
        {
            // 底边
            for (int x = 0; x < _sizeX; x++)
                yield return new CellCoord(x, 0);

            // 顶边
            for (int x = 0; x < _sizeX; x++)
                yield return new CellCoord(x, _sizeZ - 1);

            // 左边（不含角）
            for (int z = 1; z < _sizeZ - 1; z++)
                yield return new CellCoord(0, z);

            // 右边（不含角）
            for (int z = 1; z < _sizeZ - 1; z++)
                yield return new CellCoord(_sizeX - 1, z);
        }

        #endregion

        #region 脏标记

        /// <summary>
        /// 标记格子需要更新渲染
        /// </summary>
        public void MarkDirty(CellCoord cell)
        {
            // TODO: 实现渲染脏标记系统
        }

        /// <summary>
        /// 标记区域需要更新渲染
        /// </summary>
        public void MarkDirtyRect(CellCoord min, CellCoord max)
        {
            // TODO: 实现渲染脏标记系统
        }

        /// <summary>
        /// 标记整层需要更新渲染
        /// </summary>
        public void MarkAllDirty()
        {
            // TODO: 实现渲染脏标记系统
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            string floorName = _floorIndex switch
            {
                < 0 => $"B{-_floorIndex}",
                0 => "G",
                > 0 => $"F{_floorIndex}"
            };
            return $"Floor[{floorName}] ({_sizeX}x{_sizeZ})";
        }

        #endregion
    }
}
