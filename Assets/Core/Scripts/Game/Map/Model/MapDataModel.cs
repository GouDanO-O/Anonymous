using System;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using GDFrameworkCore;
using UnityEngine.Events;

namespace Core.Game.Map.Model
{
    /// <summary>
    /// 地图数据模型
    /// 管理当前地图的所有数据
    /// </summary>
    public class MapDataModel : AbstractModel
    {
        #region 数据

        /// <summary>
        /// 当前地图数据
        /// </summary>
        public MapData CurrentMap { get; private set; }

        /// <summary>
        /// 当前显示的楼层
        /// </summary>
        public int CurrentFloor { get; private set; }

        /// <summary>
        /// 是否已加载地图
        /// </summary>
        public bool IsMapLoaded => CurrentMap != null;

        #endregion

        #region 事件

        /// <summary>
        /// 地图加载完成事件
        /// </summary>
        public UnityAction<MapData> OnMapLoaded;

        /// <summary>
        /// 地图卸载事件
        /// </summary>
        public UnityAction OnMapUnloaded;

        /// <summary>
        /// 楼层切换事件
        /// </summary>
        public UnityAction<int, int> OnFloorChanged;

        /// <summary>
        /// 格子数据变更事件
        /// </summary>
        public UnityAction<int, int, int> OnCellChanged;

        /// <summary>
        /// Chunk 数据变更事件
        /// </summary>
        public UnityAction<int, int, int> OnChunkChanged;

        #endregion

        protected override void OnInit()
        {
            CurrentFloor = 0;
        }

        #region 地图操作

        /// <summary>
        /// 加载地图数据
        /// </summary>
        public void LoadMap(MapData mapData)
        {
            if (mapData == null)
                return;

            CurrentMap = mapData;
            CurrentFloor = 0;
            OnMapLoaded?.Invoke(mapData);
        }

        /// <summary>
        /// 卸载当前地图
        /// </summary>
        public void UnloadMap()
        {
            if (CurrentMap == null)
                return;

            CurrentMap = null;
            CurrentFloor = 0;
            OnMapUnloaded?.Invoke();
        }

        /// <summary>
        /// 创建新地图
        /// </summary>
        public MapData CreateNewMap(string mapName, int width, int height, int floorCount, int seed = 0)
        {
            var mapData = new MapData(mapName, width, height, floorCount, seed);
            return mapData;
        }

        #endregion

        #region 楼层操作

        /// <summary>
        /// 切换楼层
        /// </summary>
        public void SetCurrentFloor(int floor)
        {
            if (CurrentMap == null)
                return;

            floor = Math.Clamp(floor, 0, CurrentMap.FloorCount - 1);

            if (floor == CurrentFloor)
                return;

            int oldFloor = CurrentFloor;
            CurrentFloor = floor;
            OnFloorChanged?.Invoke(oldFloor, CurrentFloor);
        }

        /// <summary>
        /// 上一层
        /// </summary>
        public void FloorUp()
        {
            SetCurrentFloor(CurrentFloor + 1);
        }

        /// <summary>
        /// 下一层
        /// </summary>
        public void FloorDown()
        {
            SetCurrentFloor(CurrentFloor - 1);
        }

        #endregion

        #region 格子操作

        /// <summary>
        /// 获取格子
        /// </summary>
        public CellData GetCell(int x, int y, int floor = -1)
        {
            if (CurrentMap == null)
                return null;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.GetCell(x, y, floor);
        }

        /// <summary>
        /// 设置格子地面类型
        /// </summary>
        public void SetCellGround(int x, int y, int floor, EGroundType groundType)
        {
            var cell = GetCell(x, y, floor);
            if (cell == null)
                return;

            cell.GroundType = groundType;
            NotifyCellChanged(x, y, floor);
        }

        /// <summary>
        /// 设置格子地板类型
        /// </summary>
        public void SetCellFloor(int x, int y, int floor, EFloorType floorType)
        {
            var cell = GetCell(x, y, floor);
            if (cell == null)
                return;

            cell.FloorType = floorType;
            NotifyCellChanged(x, y, floor);
        }

        /// <summary>
        /// 设置墙体
        /// </summary>
        public void SetWall(int x, int y, int floor, EWallDirection direction, WallData wallData)
        {
            var cell = GetCell(x, y, floor);
            if (cell == null)
                return;

            cell.SetWall(direction, wallData);
            NotifyCellChanged(x, y, floor);
        }

        /// <summary>
        /// 通知格子变更
        /// </summary>
        private void NotifyCellChanged(int x, int y, int floor)
        {
            OnCellChanged?.Invoke(x, y, floor);

            // 同时通知对应的 Chunk
            var chunkIndex = CurrentMap.WorldToChunkIndex(x, y);
            var chunk = CurrentMap.GetChunk(chunkIndex.chunkX, chunkIndex.chunkY, floor);
            chunk?.MarkDirty();
            OnChunkChanged?.Invoke(chunkIndex.chunkX, chunkIndex.chunkY, floor);
        }

        #endregion

        #region Chunk 操作

        /// <summary>
        /// 获取 Chunk
        /// </summary>
        public ChunkData GetChunk(int chunkX, int chunkY, int floor = -1)
        {
            if (CurrentMap == null)
                return null;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.GetChunk(chunkX, chunkY, floor);
        }

        /// <summary>
        /// 标记 Chunk 需要更新
        /// </summary>
        public void MarkChunkDirty(int chunkX, int chunkY, int floor)
        {
            var chunk = GetChunk(chunkX, chunkY, floor);
            chunk?.MarkDirty();
            OnChunkChanged?.Invoke(chunkX, chunkY, floor);
        }

        #endregion

        #region 查询

        /// <summary>
        /// 检查坐标是否有效
        /// </summary>
        public bool IsValidPosition(int x, int y, int floor = -1)
        {
            if (CurrentMap == null)
                return false;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.IsValidCellPos(x, y, floor);
        }

        /// <summary>
        /// 检查格子是否可通行
        /// </summary>
        public bool IsCellWalkable(int x, int y, int floor = -1)
        {
            var cell = GetCell(x, y, floor);
            return cell?.IsWalkable ?? false;
        }

        #endregion
    }
}
