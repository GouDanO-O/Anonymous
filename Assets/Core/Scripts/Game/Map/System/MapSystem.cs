using System;
using System.Collections.Generic;
using Core.Game.Config;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Event;
using Core.Game.Map.Model;
using Core.Game.Pawn.Data;
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

            var def = ConfigManager.GetStructureDef(structureDefId);
            if (health < 0) health = def.MaxHealth;

            cell.StructureDefId = structureDefId;
            cell.StructureHealth = health;

            // 门默认关闭
            cell.DoorState = (EStructureType)def.StructureType == EStructureType.Door
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

            // Autotile: 标记邻接 chunk dirty (结构变更影响邻居墙壁贴图)
            MarkNeighborChunksDirtyIfEdge(x, y, floor);
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

            // Autotile: 标记邻接 chunk dirty (结构变更影响邻居墙壁贴图)
            MarkNeighborChunksDirtyIfEdge(x, y, floor);
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

            var def = ConfigManager.GetStructureDef(cell.StructureDefId);
            if ((EStructureType)def.StructureType != EStructureType.Door) return;

            cell.DoorState = state;
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            _mapDataModel.InvalidatePathGrid(floor);
        }

        #endregion

        #region 地板操作

        /// <summary>
        /// 设置地板
        /// </summary>
        public void SetCellFloor(int x, int y, int floor, int floorDefId)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            cell.FloorDefId = floorDefId;
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
        }

        /// <summary>
        /// 移除地板
        /// </summary>
        public void RemoveCellFloor(int x, int y, int floor)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null || !cell.HasFloor) return;

            cell.FloorDefId = MapConst.InvalidDefId;
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
        }

        #endregion

        #region 地形操作

        /// <summary>
        /// 修改地形 (用于地基建造/拆除)
        /// </summary>
        public void SetTerrain(int x, int y, int floor, int terrainDefId)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            var def = ConfigManager.GetTerrainDef(terrainDefId);
            cell.TerrainDefId = terrainDefId;
            cell.MoveCost = def.MoveCost;
            cell.SetFlag(ECellFlags.Walkable, def.MoveCost > 0);
            cell.SetFlag(ECellFlags.Buildable, def.CanBuild);

            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            _mapDataModel.InvalidatePathGrid(floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));
        }

        #endregion

        #region 屋顶操作

        /// <summary>
        /// 设置/清除屋顶
        /// 屋顶影响上层可行走性: HasRoof → 上层变为可行走可建造
        /// </summary>
        public void SetRoof(int x, int y, int floor, bool hasRoof)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null) return;

            cell.SetFlag(ECellFlags.HasRoof, hasRoof);
            _mapDataModel.CurrentMap.MarkCellDirty(x, y, floor);
            this.SendEvent(new SCellChangedEvent(x, y, floor));

            // 屋顶影响上层可行走性
            var map = _mapDataModel.CurrentMap;
            int upperFloor = floor + 1;
            if (upperFloor >= map.FloorCount) return;

            var upperCell = map.GetCell(x, y, upperFloor);
            if (upperCell == null) return;

            if (hasRoof)
            {
                upperCell.SetFlag(ECellFlags.Walkable, true);
                upperCell.SetFlag(ECellFlags.Buildable, true);
                if (upperCell.MoveCost == 0) upperCell.MoveCost = 1;
            }
            else
            {
                // 清除上层可行走性(如果无其他地形/地板支持)
                if (!upperCell.HasTerrain && !upperCell.HasFloor)
                {
                    upperCell.SetFlag(ECellFlags.Walkable, false);
                    upperCell.SetFlag(ECellFlags.Buildable, false);
                    upperCell.MoveCost = 0;
                }
            }

            _mapDataModel.CurrentMap.MarkCellDirty(x, y, upperFloor);
            _mapDataModel.InvalidatePathGrid(upperFloor);
            this.SendEvent(new SCellChangedEvent(x, y, upperFloor));
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

        #region 多层寻路

        /// <summary>
        /// 收集指定楼层所有楼梯位置
        /// </summary>
        public List<Vector2Int> CollectStairs(int floor)
        {
            var map = _mapDataModel.CurrentMap;
            if (map == null) return new List<Vector2Int>();

            var stairs = new List<Vector2Int>();
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = map.GetCell(x, y, floor);
                    if (cell == null || !cell.HasStructure) continue;

                    var def = ConfigManager.GetStructureDef(cell.StructureDefId);
                    if ((EStructureType)def.StructureType == EStructureType.Stair)
                        stairs.Add(new Vector2Int(x, y));
                }
            }
            return stairs;
        }

        /// <summary>
        /// 检查指定位置是否有楼梯
        /// </summary>
        public bool HasStairAt(int x, int y, int floor)
        {
            var cell = _mapDataModel.GetCell(x, y, floor);
            if (cell == null || !cell.HasStructure) return false;

            var def = ConfigManager.GetStructureDef(cell.StructureDefId);
            return (EStructureType)def.StructureType == EStructureType.Stair;
        }

        /// <summary>
        /// 多层寻路: 返回跨楼层的路径 (包含楼梯过渡步骤)
        /// 返回List of (x, y, floor, isStairTransition)
        /// </summary>
        public List<Pawn.Data.PathStep> FindMultiFloorPath(
            int startX, int startY, int startFloor,
            int endX, int endY, int endFloor,
            int searchRadius = 50)
        {
            // 同层直接用2D A*
            if (startFloor == endFloor)
            {
                var astarPath = FindPath(startX, startY, endX, endY, startFloor, searchRadius);
                if (astarPath == null || astarPath.Count < 2) return null;

                var result = new List<Pawn.Data.PathStep>();
                for (int i = 1; i < astarPath.Count; i++)
                {
                    result.Add(new Pawn.Data.PathStep
                    {
                        X = astarPath[i].x,
                        Y = astarPath[i].y,
                        Floor = startFloor,
                        IsStairTransition = false
                    });
                }
                return result;
            }

            // 跨层: 逐层贪心寻路
            var path = new List<Pawn.Data.PathStep>();
            int curX = startX, curY = startY, curFloor = startFloor;

            while (curFloor != endFloor)
            {
                int direction = curFloor < endFloor ? 1 : -1;
                int targetFloor = curFloor + direction;

                // 在当前层找所有楼梯
                var stairs = CollectStairs(curFloor);
                if (stairs.Count == 0) return null; // 无楼梯, 无法跨层

                // 目标层也需要有楼梯在同一位置 (楼梯连接上下层)
                List<AstarPosVo> bestPath = null;
                Vector2Int bestStair = default;

                foreach (var stair in stairs)
                {
                    // 检查目标层同位置也有楼梯或可行走
                    if (!HasStairAt(stair.x, stair.y, targetFloor) &&
                        !IsCellWalkable(stair.x, stair.y, targetFloor))
                        continue;

                    var p = FindPath(curX, curY, stair.x, stair.y, curFloor, searchRadius);
                    if (p != null && p.Count >= 2)
                    {
                        if (bestPath == null || p.Count < bestPath.Count)
                        {
                            bestPath = p;
                            bestStair = stair;
                        }
                    }
                    // 已在楼梯上
                    else if (curX == stair.x && curY == stair.y)
                    {
                        bestPath = new List<AstarPosVo>
                        {
                            new AstarPosVo { x = curX, y = curY },
                            new AstarPosVo { x = curX, y = curY }
                        };
                        bestStair = stair;
                        break;
                    }
                }

                if (bestPath == null) return null; // 无法到达任何楼梯

                // 追加路径到楼梯 (跳过起点)
                for (int i = 1; i < bestPath.Count; i++)
                {
                    path.Add(new Pawn.Data.PathStep
                    {
                        X = bestPath[i].x,
                        Y = bestPath[i].y,
                        Floor = curFloor,
                        IsStairTransition = false
                    });
                }

                // 追加楼梯过渡
                path.Add(new Pawn.Data.PathStep
                {
                    X = bestStair.x,
                    Y = bestStair.y,
                    Floor = targetFloor,
                    IsStairTransition = true
                });

                curX = bestStair.x;
                curY = bestStair.y;
                curFloor = targetFloor;
            }

            // 最后一段: 从楼梯到目标位置
            if (curX != endX || curY != endY)
            {
                var finalPath = FindPath(curX, curY, endX, endY, endFloor, searchRadius);
                if (finalPath == null || finalPath.Count < 2) return null;

                for (int i = 1; i < finalPath.Count; i++)
                {
                    path.Add(new Pawn.Data.PathStep
                    {
                        X = finalPath[i].x,
                        Y = finalPath[i].y,
                        Floor = endFloor,
                        IsStairTransition = false
                    });
                }
            }

            return path.Count > 0 ? path : null;
        }

        #endregion

        #region Autotile 邻接 Chunk Dirty

        /// <summary>
        /// 结构变更时, 如果在 chunk 边缘, 需要标记邻接 chunk dirty
        /// 以便邻接墙壁的 autotile bitmask 重新计算
        /// </summary>
        private void MarkNeighborChunksDirtyIfEdge(int x, int y, int floor)
        {
            var map = _mapDataModel.CurrentMap;
            if (map == null) return;

            int lx = x % MapConst.ChunkSize;
            int ly = y % MapConst.ChunkSize;

            // 检查4个方向是否在 chunk 边缘
            if (lx == 0) MarkChunkDirtyAt(x - 1, y, floor, map);
            if (lx == MapConst.ChunkSize - 1) MarkChunkDirtyAt(x + 1, y, floor, map);
            if (ly == 0) MarkChunkDirtyAt(x, y - 1, floor, map);
            if (ly == MapConst.ChunkSize - 1) MarkChunkDirtyAt(x, y + 1, floor, map);
        }

        private void MarkChunkDirtyAt(int worldX, int worldY, int floor, MapData map)
        {
            if (worldX < 0 || worldX >= map.Width || worldY < 0 || worldY >= map.Height) return;
            int cx = worldX / MapConst.ChunkSize;
            int cy = worldY / MapConst.ChunkSize;
            var chunk = map.GetChunk(cx, cy, floor);
            if (chunk != null)
            {
                chunk.MarkDirty();
                this.SendEvent(new SChunkDirtyEvent(cx, cy, floor));
            }
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
