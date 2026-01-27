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
    /// 房间管理系统
    /// 负责房间检测、管理和查询
    /// </summary>
    public class RoomSystem : AbstractSystem
    {
        #region 数据

        private MapDataModel _mapDataModel;

        /// <summary>
        /// 所有楼层的房间数据
        /// Key: 楼层, Value: 该楼层的房间数据
        /// </summary>
        private Dictionary<int, FloorRoomData> _floorRoomDataDict;

        #endregion

        #region 事件

        /// <summary>
        /// 房间检测完成事件
        /// </summary>
        public UnityAction<int> OnRoomDetectionComplete;

        #endregion

        #region 属性

        /// <summary>
        /// 是否已检测房间
        /// </summary>
        public bool IsRoomDetected => _floorRoomDataDict != null && _floorRoomDataDict.Count > 0;

        #endregion

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _floorRoomDataDict = new Dictionary<int, FloorRoomData>();

            // 订阅地图加载事件
            _mapDataModel.OnMapLoaded += OnMapLoaded;
            _mapDataModel.OnMapUnloaded += OnMapUnloaded;
        }

        protected override void OnDeinit()
        {
            if (_mapDataModel != null)
            {
                _mapDataModel.OnMapLoaded -= OnMapLoaded;
                _mapDataModel.OnMapUnloaded -= OnMapUnloaded;
            }
        }

        #region 事件处理

        private void OnMapLoaded(MapData mapData)
        {
            // 地图加载后自动检测房间
            DetectAllRooms();
        }

        private void OnMapUnloaded()
        {
            ClearRoomData();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 检测所有楼层的房间
        /// </summary>
        public void DetectAllRooms()
        {
            if (_mapDataModel.CurrentMap == null)
                return;

            ClearRoomData();

            _floorRoomDataDict = RoomDetectionUtility.DetectAllRooms(_mapDataModel.CurrentMap);

            int totalRooms = 0;
            foreach (var kvp in _floorRoomDataDict)
            {
                totalRooms += kvp.Value.Rooms.Count;
            }

            Debug.Log($"[RoomSystem] Detected {totalRooms} rooms across {_floorRoomDataDict.Count} floors");

            OnRoomDetectionComplete?.Invoke(totalRooms);
        }

        /// <summary>
        /// 检测单层楼的房间
        /// </summary>
        public void DetectRoomsInFloor(int floor)
        {
            if (_mapDataModel.CurrentMap == null)
                return;

            var floorRoomData = RoomDetectionUtility.DetectRoomsInFloor(_mapDataModel.CurrentMap, floor);
            _floorRoomDataDict[floor] = floorRoomData;
        }

        /// <summary>
        /// 清空房间数据
        /// </summary>
        public void ClearRoomData()
        {
            _floorRoomDataDict?.Clear();
        }

        /// <summary>
        /// 获取格子所属的房间ID
        /// </summary>
        /// <returns>房间ID，-1 表示不在任何房间内（室外）</returns>
        public int GetRoomId(int x, int y, int floor)
        {
            if (!_floorRoomDataDict.TryGetValue(floor, out var floorRoomData))
                return -1;

            return floorRoomData.GetRoomId(x, y);
        }

        /// <summary>
        /// 获取格子所属的房间
        /// </summary>
        public RoomData GetRoom(int x, int y, int floor)
        {
            if (!_floorRoomDataDict.TryGetValue(floor, out var floorRoomData))
                return null;

            return floorRoomData.GetRoom(x, y);
        }

        /// <summary>
        /// 获取指定楼层的所有房间
        /// </summary>
        public List<RoomData> GetRoomsInFloor(int floor)
        {
            if (!_floorRoomDataDict.TryGetValue(floor, out var floorRoomData))
                return new List<RoomData>();

            return floorRoomData.Rooms;
        }

        /// <summary>
        /// 获取指定ID的房间
        /// </summary>
        public RoomData GetRoomById(int roomId, int floor)
        {
            if (!_floorRoomDataDict.TryGetValue(floor, out var floorRoomData))
                return null;

            return floorRoomData.Rooms.Find(r => r.RoomId == roomId);
        }

        /// <summary>
        /// 检查格子是否在室内
        /// </summary>
        public bool IsCellIndoor(int x, int y, int floor)
        {
            return GetRoomId(x, y, floor) >= 0;
        }

        /// <summary>
        /// 检查两个格子是否在同一个房间
        /// </summary>
        public bool IsInSameRoom(int x1, int y1, int x2, int y2, int floor)
        {
            int roomId1 = GetRoomId(x1, y1, floor);
            int roomId2 = GetRoomId(x2, y2, floor);

            return roomId1 >= 0 && roomId1 == roomId2;
        }

        /// <summary>
        /// 获取楼层房间数据
        /// </summary>
        public FloorRoomData GetFloorRoomData(int floor)
        {
            return _floorRoomDataDict.TryGetValue(floor, out var data) ? data : null;
        }

        #endregion
    }
}
