using System;
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

        #region 结构操作

        /// <summary>
        /// 设置结构 (墙/门/窗)
        /// </summary>
        public void SetStructure(int x, int y, int floor, int structureDefId, int health = -1)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            var def = TempConfigProvider.GetStructureDef(structureDefId);
            if (health < 0) health = def.MaxHealth;

            cell.StructureDefId = structureDefId;
            cell.StructureHealth = health;

            // 门默认关闭
            cell.DoorState = def.StructureType == EStructureType.Door
                ? EDoorState.Closed
                : EDoorState.None;

            // 墙/窗阻挡移动
            if (def.BlocksMovement)
            {
                cell.SetFlag(ECellFlags.Walkable, false);
                cell.MoveCost = 0;
            }

            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            _mapDataModel.InvalidatePathGrid(floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
            _roomSystem.RecalculateRooms(floor);
        }

        /// <summary>
        /// 移除结构
        /// </summary>
        public void RemoveStructure(int x, int y, int floor)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null || !cell.HasStructure) return;

            cell.StructureDefId = MapConst.InvalidDefId;
            cell.DoorState = EDoorState.None;
            cell.StructureHealth = 0;

            // 恢复可通行 (如果有地形或地板)
            if (cell.HasTerrain || cell.HasFloor)
            {
                cell.SetFlag(ECellFlags.Walkable, true);
                cell.MoveCost = 1;
            }

            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            _mapDataModel.InvalidatePathGrid(floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
            _roomSystem.RecalculateRooms(floor);
        }

        /// <summary>
        /// 获取结构DefId
        /// </summary>
        public int GetStructure(int x, int y, int floor)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            return cell?.StructureDefId ?? MapConst.InvalidDefId;
        }

        /// <summary>
        /// 设置门状态
        /// </summary>
        public void SetDoorState(int x, int y, int floor, EDoorState state)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null || !cell.HasStructure) return;

            var def = TempConfigProvider.GetStructureDef(cell.StructureDefId);
            if (def.StructureType != EStructureType.Door) return;

            cell.DoorState = state;
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            _mapDataModel.InvalidatePathGrid(floor);
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
                    if (cell == null || !cell.IsWalkable || cell.MoveCost == 0
                        || cell.HasFlag(ECellFlags.Occupied))
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
            int wallCheckFloor = floor;
            return Astar.instance.find(grid, map.Height, map.Width,
                startX, startY, endX, endY, searchRadius,
                (fX, fY, tX, tY) => IsWallBlocking(fX, fY, tX, tY, wallCheckFloor));
        }

        /// <summary>
        /// 检查目标Cell是否有结构阻挡移动
        /// 格子占据式: 检查目标格是否有阻挡结构
        /// </summary>
        public bool IsWallBlocking(int fromX, int fromY, int toX, int toY, int floor = -1)
        {
            if (floor < 0) floor = _mapDataModel.CurrentFloor;
            var map = _mapDataModel.CurrentMap;
            if (map == null) return false;

            var targetCell = map.GetCell(toX, toY, floor);
            if (targetCell == null) return false;
            if (!targetCell.HasStructure) return false;

            return !targetCell.IsStructurePassable;
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
