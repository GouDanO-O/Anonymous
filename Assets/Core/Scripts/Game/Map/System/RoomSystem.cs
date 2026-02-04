using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Model;
using Core.Game.Map.Utility;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 房间系统
    /// 管理房间检测和房间数据
    /// </summary>
    public class RoomSystem : AbstractSystem
    {
        #region 引用

        private MapDataModel _mapDataModel;

        #endregion

        #region 事件

        /// <summary>
        /// 房间检测完成事件
        /// </summary>
        public UnityAction<int> OnRoomsDetected;

        /// <summary>
        /// 房间数据变化事件
        /// </summary>
        public UnityAction<int, int> OnRoomChanged;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();

            // 订阅地图加载事件
            _mapDataModel.OnMapLoaded += OnMapLoaded;
        }

        protected override void OnDeinit()
        {
            if (_mapDataModel != null)
            {
                _mapDataModel.OnMapLoaded -= OnMapLoaded;
            }
        }

        #endregion

        #region 事件处理

        private void OnMapLoaded(MapData mapData)
        {
            // 地图加载后自动检测房间
            DetectAllRooms();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 检测所有楼层的房间
        /// </summary>
        public void DetectAllRooms()
        {
            if (!_mapDataModel.IsMapLoaded)
                return;

            RoomDetectionUtility.DetectAllRooms(_mapDataModel.CurrentMap);

            int totalRooms = 0;
            for (int floor = 0; floor < _mapDataModel.FloorCount; floor++)
            {
                var floorRoomData = _mapDataModel.CurrentMap.GetFloorRoomData(floor);
                if (floorRoomData != null)
                {
                    totalRooms += floorRoomData.Rooms.Count;
                }
            }

            Debug.Log($"[RoomSystem] Detected {totalRooms} rooms across all floors");
            OnRoomsDetected?.Invoke(totalRooms);
        }

        /// <summary>
        /// 检测指定楼层的房间
        /// </summary>
        public void DetectRoomsInFloor(int floor)
        {
            if (!_mapDataModel.IsMapLoaded)
                return;

            var floorRoomData = RoomDetectionUtility.DetectRoomsInFloor(_mapDataModel.CurrentMap, floor);
            _mapDataModel.CurrentMap.SetFloorRoomData(floor, floorRoomData);

            OnRoomsDetected?.Invoke(floorRoomData.Rooms.Count);
        }

        /// <summary>
        /// 获取格子所属的房间ID
        /// </summary>
        public int GetRoomId(int x, int y, int floor = -1)
        {
            if (floor < 0)
                floor = _mapDataModel.CurrentFloor;

            return _mapDataModel.GetRoomId(x, y, floor);
        }

        /// <summary>
        /// 获取房间数据
        /// </summary>
        public RoomData GetRoom(int roomId, int floor = -1)
        {
            if (floor < 0)
                floor = _mapDataModel.CurrentFloor;

            return _mapDataModel.GetRoom(roomId, floor);
        }

        /// <summary>
        /// 获取指定楼层的所有房间
        /// </summary>
        public List<RoomData> GetRoomsInFloor(int floor = -1)
        {
            if (floor < 0)
                floor = _mapDataModel.CurrentFloor;

            var floorRoomData = _mapDataModel.CurrentMap?.GetFloorRoomData(floor);
            return floorRoomData?.Rooms ?? new List<RoomData>();
        }

        /// <summary>
        /// 检查坐标是否在室内
        /// </summary>
        public bool IsCellIndoor(int x, int y, int floor = -1)
        {
            if (floor < 0)
                floor = _mapDataModel.CurrentFloor;

            return RoomDetectionUtility.IsCellIndoor(_mapDataModel.CurrentMap, x, y, floor);
        }

        /// <summary>
        /// 刷新指定区域的房间检测
        /// </summary>
        public void RefreshRoomsInArea(int minX, int minY, int maxX, int maxY, int floor = -1)
        {
            if (!_mapDataModel.IsMapLoaded)
                return;

            if (floor < 0)
                floor = _mapDataModel.CurrentFloor;

            // 简化实现：重新检测整层
            DetectRoomsInFloor(floor);
        }

        #endregion
    }
}
