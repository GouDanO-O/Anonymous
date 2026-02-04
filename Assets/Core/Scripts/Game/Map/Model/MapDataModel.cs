using System;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Map.Model
{
    /// <summary>
    /// 地图数据模型
    /// 管理地图数据和状态
    /// </summary>
    public class MapDataModel : AbstractModel
    {
        #region 数据

        /// <summary>
        /// 当前地图数据
        /// </summary>
        public MapData CurrentMap { get; private set; }

        /// <summary>
        /// 当前楼层
        /// </summary>
        public int CurrentFloor { get; private set; }

        #endregion

        #region 属性

        /// <summary>
        /// 是否已加载地图
        /// </summary>
        public bool IsMapLoaded => CurrentMap != null;

        /// <summary>
        /// 地图宽度
        /// </summary>
        public int MapWidth => CurrentMap?.Width ?? 0;

        /// <summary>
        /// 地图高度
        /// </summary>
        public int MapHeight => CurrentMap?.Height ?? 0;

        /// <summary>
        /// 楼层数
        /// </summary>
        public int FloorCount => CurrentMap?.FloorCount ?? 0;

        /// <summary>
        /// Chunk列数
        /// </summary>
        public int ChunkCountX => CurrentMap?.ChunkCountX ?? 0;

        /// <summary>
        /// Chunk行数
        /// </summary>
        public int ChunkCountY => CurrentMap?.ChunkCountY ?? 0;

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
        /// 楼层切换事件 (oldFloor, newFloor)
        /// </summary>
        public UnityAction<int, int> OnFloorChanged;

        /// <summary>
        /// Chunk数据变化事件 (chunkX, chunkY, floor)
        /// </summary>
        public UnityAction<int, int, int> OnChunkDirty;

        /// <summary>
        /// 格子数据变化事件 (x, y, floor)
        /// </summary>
        public UnityAction<int, int, int> OnCellChanged;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            CurrentFloor = 0;
        }

        #endregion

        #region 地图加载/卸载

        /// <summary>
        /// 加载地图
        /// </summary>
        public void LoadMap(MapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogError("[MapDataModel] LoadMap failed: mapData is null");
                return;
            }

            // 如果已有地图，先卸载
            if (CurrentMap != null)
            {
                UnloadMap();
            }

            CurrentMap = mapData;
            CurrentFloor = 0;

            Debug.Log($"[MapDataModel] Map loaded: {mapData}");
            OnMapLoaded?.Invoke(mapData);
        }

        /// <summary>
        /// 卸载地图
        /// </summary>
        public void UnloadMap()
        {
            if (CurrentMap == null)
                return;

            CurrentMap = null;
            CurrentFloor = 0;

            Debug.Log("[MapDataModel] Map unloaded");
            OnMapUnloaded?.Invoke();
        }

        #endregion

        #region 楼层控制

        /// <summary>
        /// 设置当前楼层
        /// </summary>
        public void SetCurrentFloor(int floor)
        {
            if (CurrentMap == null)
                return;

            floor = Mathf.Clamp(floor, 0, FloorCount - 1);

            if (floor == CurrentFloor)
                return;

            int oldFloor = CurrentFloor;
            CurrentFloor = floor;

            Debug.Log($"[MapDataModel] Floor changed: {oldFloor} -> {CurrentFloor}");
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

        #region 数据访问

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
        /// 获取Chunk
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
        /// 根据世界坐标获取Chunk
        /// </summary>
        public ChunkData GetChunkByWorld(int worldX, int worldY, int floor = -1)
        {
            if (CurrentMap == null)
                return null;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.GetChunkByWorld(worldX, worldY, floor);
        }

        /// <summary>
        /// 获取房间ID
        /// </summary>
        public int GetRoomId(int x, int y, int floor = -1)
        {
            if (CurrentMap == null)
                return -1;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.GetRoomId(x, y, floor);
        }

        /// <summary>
        /// 获取房间数据
        /// </summary>
        public RoomData GetRoom(int roomId, int floor = -1)
        {
            if (CurrentMap == null)
                return null;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.GetRoom(roomId, floor);
        }

        /// <summary>
        /// 检查坐标是否有效
        /// </summary>
        public bool IsValidCellPos(int x, int y, int floor = -1)
        {
            if (CurrentMap == null)
                return false;

            if (floor < 0)
                floor = CurrentFloor;

            return CurrentMap.IsValidCellPos(x, y, floor);
        }

        #endregion

        #region 数据修改

        /// <summary>
        /// 标记格子所在Chunk为脏
        /// </summary>
        public void MarkCellDirty(int x, int y, int floor = -1)
        {
            if (CurrentMap == null)
                return;

            if (floor < 0)
                floor = CurrentFloor;

            CurrentMap.MarkCellDirty(x, y, floor);

            var chunkPos = CurrentMap.WorldToChunkPos(x, y);
            OnChunkDirty?.Invoke(chunkPos.x, chunkPos.y, floor);
        }

        /// <summary>
        /// 通知格子数据变化
        /// </summary>
        public void NotifyCellChanged(int x, int y, int floor = -1)
        {
            if (floor < 0)
                floor = CurrentFloor;

            MarkCellDirty(x, y, floor);
            OnCellChanged?.Invoke(x, y, floor);
        }

        /// <summary>
        /// 设置格子地面类型
        /// </summary>
        public void SetGroundType(int x, int y, int floor, EGroundType groundType)
        {
            var cell = GetCell(x, y, floor);
            if (cell != null)
            {
                cell.GroundType = groundType;
                NotifyCellChanged(x, y, floor);
            }
        }

        /// <summary>
        /// 设置格子地板类型
        /// </summary>
        public void SetFloorType(int x, int y, int floor, EFloorType floorType)
        {
            var cell = GetCell(x, y, floor);
            if (cell != null)
            {
                cell.FloorType = floorType;
                NotifyCellChanged(x, y, floor);
            }
        }

        /// <summary>
        /// 设置墙体
        /// </summary>
        public void SetWall(int x, int y, int floor, EWallDirection direction, WallData wallData)
        {
            var cell = GetCell(x, y, floor);
            if (cell != null)
            {
                cell.SetWall(direction, wallData);
                NotifyCellChanged(x, y, floor);
            }
        }

        /// <summary>
        /// 设置格子标记
        /// </summary>
        public void SetCellFlag(int x, int y, int floor, ECellFlags flag, bool value)
        {
            var cell = GetCell(x, y, floor);
            if (cell != null)
            {
                cell.SetFlag(flag, value);
                NotifyCellChanged(x, y, floor);
            }
        }

        #endregion
    }
}
