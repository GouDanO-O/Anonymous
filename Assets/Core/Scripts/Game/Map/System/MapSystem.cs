using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Event;
using Core.Game.Map.Model;
using GDFrameworkCore;
using GDFrameworkExtend.AStar;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 地图门面系统 (对外统一API)
    /// 其他游戏系统(Pawn, Building等)只通过此系统与地图交互
    /// </summary>
    public class MapSystem : AbstractSystem
    {
        private MapDataModel _mapDataModel;
        private MapGenerateSystem _generateSystem;
        private RoomSystem _roomSystem;
        private ChunkCullingSystem _cullingSystem;
        private MapOcclusionSystem _occlusionSystem;
        private FloorLevelSystem _floorLevelSystem;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _generateSystem = this.GetSystem<MapGenerateSystem>();
            _roomSystem = this.GetSystem<RoomSystem>();
            _cullingSystem = this.GetSystem<ChunkCullingSystem>();
            _occlusionSystem = this.GetSystem<MapOcclusionSystem>();
            _floorLevelSystem = this.GetSystem<FloorLevelSystem>();
        }

        #region 地图生命周期

        /// <summary>
        /// 创建新地图
        /// </summary>
        public void CreateMap(string name, int width, int height, int floors, int seed)
        {
            _generateSystem.GenerateAndLoadMap(name, width, height, floors, seed);

            // 生成完成后计算房间
            for (int f = 0; f < floors; f++)
            {
                _roomSystem.RecalculateRooms(f);
            }

            LogKit.Log("地图创建完成，房间计算完毕");
        }

        /// <summary>
        /// 卸载当前地图
        /// </summary>
        public void UnloadMap()
        {
            _occlusionSystem.ClearAll();
            _cullingSystem.ForceRefresh();
            _mapDataModel.UnloadMap();
        }

        #endregion

        #region 楼层控制

        public int CurrentFloor => _mapDataModel.CurrentFloor;
        public bool IsMapLoaded => _mapDataModel.IsMapLoaded;

        public void SetFloor(int floor)
        {
            _mapDataModel.SetCurrentFloor(floor);
            _cullingSystem.ForceRefresh();
        }

        public void FloorUp()
        {
            SetFloor(_mapDataModel.CurrentFloor + 1);
        }

        public void FloorDown()
        {
            SetFloor(_mapDataModel.CurrentFloor - 1);
        }

        #endregion

        #region Cell访问

        public CellData GetCell(int x, int y, int floor = -1)
        {
            return _mapDataModel.GetCell(x, y, floor);
        }

        public bool IsCellWalkable(int x, int y, int floor = -1)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            return cell != null && cell.IsWalkable;
        }

        public bool IsValidPosition(int x, int y, int floor = -1)
        {
            if (floor < 0) floor = _mapDataModel.CurrentFloor;
            return _mapDataModel.CurrentMap?.IsValidCellPos(x, y, floor) ?? false;
        }

        #endregion

        #region 墙壁操作

        /// <summary>
        /// 设置墙壁
        /// </summary>
        public void SetWall(int x, int y, int floor, EWallSegment segment, WallData wall)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            if (segment == EWallSegment.North)
                cell.WallNorth = wall;
            else
                cell.WallWest = wall;

            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            _mapDataModel.InvalidatePathGrid(floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));

            // 墙壁变更需要重新计算房间
            _roomSystem.RecalculateRooms(floor);
        }

        /// <summary>
        /// 移除墙壁
        /// </summary>
        public void RemoveWall(int x, int y, int floor, EWallSegment segment)
        {
            SetWall(x, y, floor, segment, WallData.Empty);
        }

        /// <summary>
        /// 获取墙壁数据
        /// </summary>
        public WallData GetWall(int x, int y, int floor, EWallSegment segment)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return WallData.Empty;
            return segment == EWallSegment.North ? cell.WallNorth : cell.WallWest;
        }

        #endregion

        #region 物体放置

        public void PlaceObject(int x, int y, int floor, long objectId)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            cell.AddObject(objectId);
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
        }

        public void RemoveObject(int x, int y, int floor, long objectId)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            cell.RemoveObject(objectId);
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
        }

        #endregion

        #region 寻路集成

        /// <summary>
        /// 构建指定楼层的寻路网格
        /// byte[y][x], bit 0 = 阻挡
        /// </summary>
        public byte[][] BuildPathGrid(int floor)
        {
            // 检查缓存
            if (!_mapDataModel.IsPathGridDirty(floor))
            {
                var cached = _mapDataModel.GetPathGridCache(floor);
                if (cached != null) return cached;
            }

            var map = _mapDataModel.CurrentMap;
            if (map == null) return null;

            var grid = new byte[map.Height][];
            for (int y = 0; y < map.Height; y++)
            {
                grid[y] = new byte[map.Width];
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, floor);
                    if (cell == null || !cell.IsWalkable || cell.MoveCost == 0)
                    {
                        grid[y][x] = 1; // 阻挡
                    }
                    else
                    {
                        grid[y][x] = 0; // 可通行
                    }
                }
            }

            _mapDataModel.SetPathGridCache(floor, grid);
            return grid;
        }

        /// <summary>
        /// 在指定楼层查找路径
        /// </summary>
        public List<AstarPosVo> FindPath(int startX, int startY, int endX, int endY,
            int floor = -1, int searchRadius = 50)
        {
            if (floor < 0) floor = _mapDataModel.CurrentFloor;
            var grid = BuildPathGrid(floor);
            if (grid == null) return null;

            var map = _mapDataModel.CurrentMap;
            return Astar.instance.find(grid, map.Height, map.Width,
                startX, startY, endX, endY, searchRadius);
        }

        /// <summary>
        /// 检查两个相邻Cell之间是否有墙阻隔
        /// </summary>
        public bool IsWallBlocking(int fromX, int fromY, int toX, int toY, int floor = -1)
        {
            if (floor < 0) floor = _mapDataModel.CurrentFloor;
            var map = _mapDataModel.CurrentMap;
            if (map == null) return false;

            int dx = toX - fromX;
            int dy = toY - fromY;

            if (dy == 1 && dx == 0) // 向北
            {
                var cell = map.GetCell(fromX, fromY, floor);
                return cell != null && cell.WallNorth.HasWall && !cell.WallNorth.IsPassable;
            }
            if (dy == -1 && dx == 0) // 向南
            {
                var cell = map.GetCell(toX, toY, floor);
                return cell != null && cell.WallNorth.HasWall && !cell.WallNorth.IsPassable;
            }
            if (dx == -1 && dy == 0) // 向西
            {
                var cell = map.GetCell(fromX, fromY, floor);
                return cell != null && cell.WallWest.HasWall && !cell.WallWest.IsPassable;
            }
            if (dx == 1 && dy == 0) // 向东
            {
                var cell = map.GetCell(toX, toY, floor);
                return cell != null && cell.WallWest.HasWall && !cell.WallWest.IsPassable;
            }

            return false;
        }

        #endregion

        #region 每帧更新

        /// <summary>
        /// 由MapView每帧调用
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!_mapDataModel.IsMapLoaded) return;

            _cullingSystem.UpdateVisibility();
            _occlusionSystem.UpdateTransitions(deltaTime);
        }

        #endregion
    }
}
