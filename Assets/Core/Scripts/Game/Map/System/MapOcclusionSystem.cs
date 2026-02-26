using System;
using System.Collections.Generic;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 地图遮挡系统
    /// 当Pawn在建筑内时，该房间的墙壁和屋顶渐变透明
    /// </summary>
    public class MapOcclusionSystem : AbstractSystem
    {
        private MapDataModel _mapDataModel;

        private Dictionary<int, float> _roomTargetAlpha = new Dictionary<int, float>();
        private Dictionary<int, float> _roomCurrentAlpha = new Dictionary<int, float>();

        /// <summary>
        /// 房间透明度变化事件 (roomId, alpha)
        /// </summary>
        public event Action<int, float> OnRoomAlphaChanged;

        private const float FadeSpeed = 4f;
        private const float OccludedAlpha = 0.2f;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
        }

        /// <summary>
        /// 更新Pawn所在位置，确定哪些房间需要透明
        /// </summary>
        public void UpdatePawnPositions(List<Vector3Int> pawnPositions)
        {
            var occupiedRooms = new HashSet<int>();
            int floor = _mapDataModel.CurrentFloor;

            if (pawnPositions != null)
            {
                foreach (var pos in pawnPositions)
                {
                    if (pos.z != floor) continue;
                    var cell = _mapDataModel.GetCell(pos.x, pos.y, floor);
                    if (cell != null && cell.RoomId >= 0)
                        occupiedRooms.Add(cell.RoomId);
                }
            }

            // 更新目标透明度
            // 已跟踪的房间: 有Pawn→透明, 无Pawn→恢复
            var keysToUpdate = new List<int>(_roomTargetAlpha.Keys);
            foreach (var roomId in keysToUpdate)
            {
                _roomTargetAlpha[roomId] = occupiedRooms.Contains(roomId) ? OccludedAlpha : 1f;
            }

            // 新增有Pawn的房间
            foreach (var roomId in occupiedRooms)
            {
                if (!_roomTargetAlpha.ContainsKey(roomId))
                {
                    _roomTargetAlpha[roomId] = OccludedAlpha;
                    _roomCurrentAlpha[roomId] = 1f;
                }
            }
        }

        /// <summary>
        /// 每帧更新透明度过渡
        /// </summary>
        public void UpdateTransitions(float deltaTime)
        {
            var toRemove = new List<int>();

            foreach (var roomId in new List<int>(_roomTargetAlpha.Keys))
            {
                float current = _roomCurrentAlpha.GetValueOrDefault(roomId, 1f);
                float target = _roomTargetAlpha[roomId];
                float newAlpha = Mathf.MoveTowards(current, target, FadeSpeed * deltaTime);
                _roomCurrentAlpha[roomId] = newAlpha;

                OnRoomAlphaChanged?.Invoke(roomId, newAlpha);

                // 完全恢复的房间移出跟踪列表
                if (Mathf.Approximately(newAlpha, 1f) && Mathf.Approximately(target, 1f))
                    toRemove.Add(roomId);
            }

            foreach (var id in toRemove)
            {
                _roomTargetAlpha.Remove(id);
                _roomCurrentAlpha.Remove(id);
            }
        }

        /// <summary>
        /// 获取房间当前透明度
        /// </summary>
        public float GetRoomAlpha(int roomId)
        {
            return _roomCurrentAlpha.GetValueOrDefault(roomId, 1f);
        }

        /// <summary>
        /// 清除所有遮挡状态
        /// </summary>
        public void ClearAll()
        {
            _roomTargetAlpha.Clear();
            _roomCurrentAlpha.Clear();
        }
    }
}
