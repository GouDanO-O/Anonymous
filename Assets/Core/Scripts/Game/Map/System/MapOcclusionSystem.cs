using Core.Game.Map.Data;
using Core.Game.Map.Model;
using Core.Game.Map.Utility;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Map.System
{
    /// <summary>
    /// 遮挡状态
    /// </summary>
    public enum EOcclusionState
    {
        /// <summary>
        /// 室外（显示所有内容）
        /// </summary>
        Outdoor,

        /// <summary>
        /// 室内（隐藏屋顶和上层）
        /// </summary>
        Indoor,

        /// <summary>
        /// 过渡中
        /// </summary>
        Transitioning
    }

    /// <summary>
    /// 遮挡控制系统
    /// 根据摄像机/玩家位置控制建筑遮挡
    /// </summary>
    public class MapOcclusionSystem : AbstractSystem
    {
        #region 配置

        /// <summary>
        /// 遮挡过渡时间（秒）
        /// </summary>
        public float TransitionDuration = 0.3f;

        /// <summary>
        /// 室内时上层的透明度
        /// </summary>
        public float IndoorUpperFloorAlpha = 0f;

        /// <summary>
        /// 室内时屋顶的透明度
        /// </summary>
        public float IndoorRoofAlpha = 0f;

        #endregion

        #region 数据

        private MapDataModel _mapDataModel;
        private RoomSystem _roomSystem;

        /// <summary>
        /// 当前遮挡状态
        /// </summary>
        private EOcclusionState _currentState = EOcclusionState.Outdoor;

        /// <summary>
        /// 当前所在房间ID
        /// </summary>
        private int _currentRoomId = -1;

        /// <summary>
        /// 当前检测位置（格子坐标）
        /// </summary>
        private Vector2Int _currentCellPos;

        /// <summary>
        /// 当前遮挡透明度（0=完全遮挡，1=完全显示）
        /// </summary>
        private float _currentOcclusionAlpha = 1f;

        /// <summary>
        /// 目标遮挡透明度
        /// </summary>
        private float _targetOcclusionAlpha = 1f;

        #endregion

        #region 事件

        /// <summary>
        /// 遮挡状态变化事件
        /// 参数：新状态、当前房间ID
        /// </summary>
        public UnityAction<EOcclusionState, int> OnOcclusionStateChanged;

        /// <summary>
        /// 遮挡透明度变化事件
        /// 参数：当前透明度（0-1）
        /// </summary>
        public UnityAction<float> OnOcclusionAlphaChanged;

        /// <summary>
        /// 进入房间事件
        /// </summary>
        public UnityAction<RoomData> OnEnterRoom;

        /// <summary>
        /// 离开房间事件
        /// </summary>
        public UnityAction<RoomData> OnExitRoom;

        #endregion

        #region 属性

        public EOcclusionState CurrentState => _currentState;
        public int CurrentRoomId => _currentRoomId;
        public float CurrentOcclusionAlpha => _currentOcclusionAlpha;
        public bool IsIndoor => _currentState == EOcclusionState.Indoor;
        public Vector2Int CurrentCellPos => _currentCellPos;

        #endregion

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _roomSystem = this.GetSystem<RoomSystem>();
        }

        #region 公共方法

        /// <summary>
        /// 更新检测位置（每帧调用）
        /// </summary>
        /// <param name="worldPosition">世界坐标（Unity 坐标）</param>
        public void UpdatePosition(Vector3 worldPosition)
        {
            if (_mapDataModel.CurrentMap == null)
                return;

            int currentFloor = _mapDataModel.CurrentFloor;

            // 转换为格子坐标
            Vector2Int cellPos = IsometricUtility.ScreenToCellInt(worldPosition, currentFloor);

            // 如果位置没有变化，只更新透明度过渡
            if (cellPos == _currentCellPos)
            {
                UpdateTransition();
                return;
            }

            _currentCellPos = cellPos;

            // 检查是否在室内
            int newRoomId = _roomSystem.GetRoomId(cellPos.x, cellPos.y, currentFloor);
            bool wasIndoor = _currentRoomId >= 0;
            bool isIndoor = newRoomId >= 0;

            // 状态变化
            if (wasIndoor != isIndoor || _currentRoomId != newRoomId)
            {
                HandleStateChange(wasIndoor, isIndoor, newRoomId, currentFloor);
            }

            UpdateTransition();
        }

        /// <summary>
        /// 更新检测位置（通过格子坐标）
        /// </summary>
        public void UpdateCellPosition(int cellX, int cellY, int floor)
        {
            Vector3 worldPos = IsometricUtility.CellCenterToScreen(cellX, cellY, floor);
            UpdatePosition(worldPos);
        }

        /// <summary>
        /// 强制设置遮挡状态
        /// </summary>
        public void ForceSetState(EOcclusionState state)
        {
            _currentState = state;
            _targetOcclusionAlpha = state == EOcclusionState.Outdoor ? 1f : 0f;
            _currentOcclusionAlpha = _targetOcclusionAlpha;

            OnOcclusionStateChanged?.Invoke(_currentState, _currentRoomId);
            OnOcclusionAlphaChanged?.Invoke(_currentOcclusionAlpha);
        }

        /// <summary>
        /// 重置遮挡状态
        /// </summary>
        public void Reset()
        {
            _currentState = EOcclusionState.Outdoor;
            _currentRoomId = -1;
            _currentOcclusionAlpha = 1f;
            _targetOcclusionAlpha = 1f;
            _currentCellPos = Vector2Int.zero;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 处理状态变化
        /// </summary>
        private void HandleStateChange(bool wasIndoor, bool isIndoor, int newRoomId, int floor)
        {
            RoomData oldRoom = null;
            RoomData newRoom = null;

            // 离开旧房间
            if (wasIndoor && _currentRoomId >= 0)
            {
                oldRoom = _roomSystem.GetRoomById(_currentRoomId, floor);
            }

            // 进入新房间
            if (isIndoor && newRoomId >= 0)
            {
                newRoom = _roomSystem.GetRoomById(newRoomId, floor);
            }

            // 更新房间ID
            int previousRoomId = _currentRoomId;
            _currentRoomId = newRoomId;

            // 状态切换
            EOcclusionState newState;
            if (isIndoor)
            {
                newState = EOcclusionState.Indoor;
                _targetOcclusionAlpha = IndoorRoofAlpha;
            }
            else
            {
                newState = EOcclusionState.Outdoor;
                _targetOcclusionAlpha = 1f;
            }

            // 触发事件
            if (oldRoom != null && (newRoom == null || oldRoom.RoomId != newRoom.RoomId))
            {
                OnExitRoom?.Invoke(oldRoom);
            }

            if (newRoom != null && (oldRoom == null || oldRoom.RoomId != newRoom.RoomId))
            {
                OnEnterRoom?.Invoke(newRoom);
            }

            if (_currentState != newState)
            {
                _currentState = newState;
                OnOcclusionStateChanged?.Invoke(_currentState, _currentRoomId);

                Debug.Log($"[MapOcclusionSystem] State changed to {_currentState}, RoomId: {_currentRoomId}");
            }
        }

        /// <summary>
        /// 更新透明度过渡
        /// </summary>
        private void UpdateTransition()
        {
            if (Mathf.Approximately(_currentOcclusionAlpha, _targetOcclusionAlpha))
                return;

            // 平滑过渡
            float speed = 1f / TransitionDuration;
            _currentOcclusionAlpha = Mathf.MoveTowards(_currentOcclusionAlpha, _targetOcclusionAlpha,
                speed * Time.deltaTime);

            OnOcclusionAlphaChanged?.Invoke(_currentOcclusionAlpha);
        }

        #endregion
    }
}
