using System.Collections.Generic;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Map.Utility;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 地图遮挡系统
    /// 管理室内透视的三种控制模式
    /// </summary>
    public class MapOcclusionSystem : AbstractSystem
    {
        #region 配置

        /// <summary>
        /// 当前遮挡模式
        /// </summary>
        public EOcclusionMode CurrentMode { get; private set; } = EOcclusionMode.FollowPawn;

        /// <summary>
        /// 全局模式：是否显示室内
        /// </summary>
        public bool GlobalShowInterior { get; private set; } = false;

        /// <summary>
        /// 跟随模式：目标PawnId
        /// </summary>
        public int FollowTargetPawnId { get; private set; } = -1;

        /// <summary>
        /// 室内屋顶透明度（0=完全透明，1=完全不透明）
        /// </summary>
        public float IndoorRoofAlpha = 0.2f;

        /// <summary>
        /// 透明度过渡速度
        /// </summary>
        public float TransitionSpeed = 5f;

        #endregion

        #region 数据

        private MapDataModel _mapDataModel;
        private RoomSystem _roomSystem;

        /// <summary>
        /// 当前需要显示内部的房间ID集合
        /// </summary>
        private HashSet<int> _revealedRoomIds = new HashSet<int>();

        /// <summary>
        /// 房间透明度目标值
        /// </summary>
        private Dictionary<int, float> _roomAlphaTargets = new Dictionary<int, float>();

        /// <summary>
        /// 房间当前透明度
        /// </summary>
        private Dictionary<int, float> _roomAlphaCurrent = new Dictionary<int, float>();

        /// <summary>
        /// 鼠标悬停位置
        /// </summary>
        private Vector2Int _hoverCellPos;

        /// <summary>
        /// 跟随目标的位置
        /// </summary>
        private Vector2Int _followTargetCellPos;

        #endregion

        #region 事件

        /// <summary>
        /// 遮挡模式变化事件
        /// </summary>
        public UnityAction<EOcclusionMode> OnModeChanged;

        /// <summary>
        /// 房间透明度变化事件 (roomId, alpha)
        /// </summary>
        public UnityAction<int, float> OnRoomAlphaChanged;

        /// <summary>
        /// 显示的房间集合变化事件
        /// </summary>
        public UnityAction<HashSet<int>> OnRevealedRoomsChanged;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _roomSystem = this.GetSystem<RoomSystem>();
        }

        #endregion

        #region 公共方法 - 模式控制

        /// <summary>
        /// 设置遮挡模式
        /// </summary>
        public void SetMode(EOcclusionMode mode)
        {
            if (CurrentMode == mode)
                return;

            CurrentMode = mode;
            Debug.Log($"[MapOcclusionSystem] Mode changed to: {mode}");

            RecalculateOcclusion();
            OnModeChanged?.Invoke(mode);
        }

        /// <summary>
        /// 全局模式：切换显示/隐藏
        /// </summary>
        public void ToggleGlobalInterior()
        {
            GlobalShowInterior = !GlobalShowInterior;
            Debug.Log($"[MapOcclusionSystem] Global interior: {GlobalShowInterior}");

            if (CurrentMode == EOcclusionMode.Global)
            {
                RecalculateOcclusion();
            }
        }

        /// <summary>
        /// 全局模式：设置显示状态
        /// </summary>
        public void SetGlobalInterior(bool show)
        {
            if (GlobalShowInterior == show)
                return;

            GlobalShowInterior = show;

            if (CurrentMode == EOcclusionMode.Global)
            {
                RecalculateOcclusion();
            }
        }

        /// <summary>
        /// 悬停模式：更新鼠标位置
        /// </summary>
        public void UpdateMousePosition(Vector3 worldPos)
        {
            if (CurrentMode != EOcclusionMode.Hover)
                return;

            var cellPos = IsometricUtility.ScreenToCellInt(worldPos, _mapDataModel.CurrentFloor);

            if (cellPos != _hoverCellPos)
            {
                _hoverCellPos = cellPos;
                RecalculateOcclusion();
            }
        }

        /// <summary>
        /// 悬停模式：更新鼠标位置（屏幕坐标）
        /// </summary>
        public void UpdateMouseScreenPosition(Vector3 screenPos, Camera camera)
        {
            if (CurrentMode != EOcclusionMode.Hover || camera == null)
                return;

            Vector3 worldPos = camera.ScreenToWorldPoint(screenPos);
            UpdateMousePosition(worldPos);
        }

        /// <summary>
        /// 跟随模式：设置跟随目标
        /// </summary>
        public void SetFollowTarget(int pawnId)
        {
            FollowTargetPawnId = pawnId;

            if (CurrentMode == EOcclusionMode.FollowPawn)
            {
                RecalculateOcclusion();
            }
        }

        /// <summary>
        /// 跟随模式：更新目标位置
        /// </summary>
        public void UpdateFollowTargetPosition(int cellX, int cellY)
        {
            if (CurrentMode != EOcclusionMode.FollowPawn)
                return;

            var newPos = new Vector2Int(cellX, cellY);
            if (newPos != _followTargetCellPos)
            {
                _followTargetCellPos = newPos;
                RecalculateOcclusion();
            }
        }

        #endregion

        #region 公共方法 - 查询

        /// <summary>
        /// 检查房间是否应该显示内部
        /// </summary>
        public bool IsRoomRevealed(int roomId)
        {
            return _revealedRoomIds.Contains(roomId);
        }

        /// <summary>
        /// 获取房间当前透明度
        /// </summary>
        public float GetRoomAlpha(int roomId)
        {
            return _roomAlphaCurrent.TryGetValue(roomId, out float alpha) ? alpha : 1f;
        }

        /// <summary>
        /// 获取所有显示内部的房间ID
        /// </summary>
        public IReadOnlyCollection<int> GetRevealedRoomIds()
        {
            return _revealedRoomIds;
        }

        #endregion

        #region 公共方法 - 更新

        /// <summary>
        /// 更新透明度过渡（需要每帧调用）
        /// </summary>
        public void UpdateTransitions(float deltaTime)
        {
            var roomsToRemove = new List<int>();

            foreach (var kvp in _roomAlphaTargets)
            {
                int roomId = kvp.Key;
                float targetAlpha = kvp.Value;

                if (!_roomAlphaCurrent.TryGetValue(roomId, out float currentAlpha))
                {
                    currentAlpha = 1f;
                }

                // 插值
                float newAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, TransitionSpeed * deltaTime);
                _roomAlphaCurrent[roomId] = newAlpha;

                // 通知变化
                OnRoomAlphaChanged?.Invoke(roomId, newAlpha);

                // 如果达到目标且目标是完全不透明，可以移除追踪
                if (Mathf.Approximately(newAlpha, targetAlpha) && Mathf.Approximately(targetAlpha, 1f))
                {
                    roomsToRemove.Add(roomId);
                }
            }

            // 清理不再需要追踪的房间
            foreach (var roomId in roomsToRemove)
            {
                if (!_revealedRoomIds.Contains(roomId))
                {
                    _roomAlphaTargets.Remove(roomId);
                    _roomAlphaCurrent.Remove(roomId);
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 重新计算遮挡状态
        /// </summary>
        private void RecalculateOcclusion()
        {
            var newRevealedRooms = new HashSet<int>();

            switch (CurrentMode)
            {
                case EOcclusionMode.Global:
                    if (GlobalShowInterior)
                    {
                        // 显示所有房间内部
                        var allRooms = _roomSystem.GetRoomsInFloor(_mapDataModel.CurrentFloor);
                        foreach (var room in allRooms)
                        {
                            newRevealedRooms.Add(room.RoomId);
                        }
                    }
                    break;

                case EOcclusionMode.Hover:
                    // 获取鼠标位置的房间
                    int hoverRoomId = _roomSystem.GetRoomId(_hoverCellPos.x, _hoverCellPos.y);
                    if (hoverRoomId >= 0)
                    {
                        newRevealedRooms.Add(hoverRoomId);
                    }
                    break;

                case EOcclusionMode.FollowPawn:
                    // 获取跟随目标的房间
                    int followRoomId = _roomSystem.GetRoomId(_followTargetCellPos.x, _followTargetCellPos.y);
                    if (followRoomId >= 0)
                    {
                        newRevealedRooms.Add(followRoomId);
                    }
                    break;
            }

            // 更新状态
            UpdateRevealedRooms(newRevealedRooms);
        }

        /// <summary>
        /// 更新显示的房间集合
        /// </summary>
        private void UpdateRevealedRooms(HashSet<int> newRevealedRooms)
        {
            // 找出需要开始隐藏的房间（之前显示，现在不显示）
            foreach (int roomId in _revealedRoomIds)
            {
                if (!newRevealedRooms.Contains(roomId))
                {
                    // 开始隐藏过渡：目标透明度 = 1（完全不透明）
                    _roomAlphaTargets[roomId] = 1f;
                }
            }

            // 找出需要开始显示的房间（之前不显示，现在显示）
            foreach (int roomId in newRevealedRooms)
            {
                if (!_revealedRoomIds.Contains(roomId))
                {
                    // 开始显示过渡：目标透明度 = IndoorRoofAlpha
                    _roomAlphaTargets[roomId] = IndoorRoofAlpha;

                    // 如果还没有当前值，设置为完全不透明
                    if (!_roomAlphaCurrent.ContainsKey(roomId))
                    {
                        _roomAlphaCurrent[roomId] = 1f;
                    }
                }
            }

            // 更新集合
            bool changed = !_revealedRoomIds.SetEquals(newRevealedRooms);
            _revealedRoomIds = newRevealedRooms;

            if (changed)
            {
                OnRevealedRoomsChanged?.Invoke(_revealedRoomIds);
            }
        }

        #endregion
    }
}
