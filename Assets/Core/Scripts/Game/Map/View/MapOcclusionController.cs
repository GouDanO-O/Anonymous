using Core.Game.Map.Data;
using Core.Game.Map.System;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// 地图遮挡控制器
    /// 负责根据遮挡系统的状态更新渲染器
    /// </summary>
    public class MapOcclusionController : MonoBehaviour
    {
        #region 配置

        [Header("遮挡配置")]
        [Tooltip("检测位置来源")]
        [SerializeField] private EOcclusionDetectionSource _detectionSource = EOcclusionDetectionSource.Camera;

        [Tooltip("目标摄像机（当检测来源为摄像机时使用）")]
        [SerializeField] private Camera _targetCamera;

        [Tooltip("目标变换（当检测来源为变换时使用）")]
        [SerializeField] private Transform _targetTransform;

        [Header("调试")]
        [SerializeField] private bool _showDebugInfo = true;

        #endregion

        #region 引用

        private MapOcclusionSystem _occlusionSystem;
        private RoomSystem _roomSystem;
        private MapView _mapView;

        #endregion

        #region 状态

        private bool _isInitialized;
        private int _lastRoomId = -1;

        #endregion

        /// <summary>
        /// 检测位置来源
        /// </summary>
        public enum EOcclusionDetectionSource
        {
            /// <summary>
            /// 使用摄像机位置
            /// </summary>
            Camera,

            /// <summary>
            /// 使用指定的变换位置
            /// </summary>
            Transform,

            /// <summary>
            /// 手动更新（不自动检测）
            /// </summary>
            Manual
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            _occlusionSystem = Main.Interface.GetSystem<MapOcclusionSystem>();
            _roomSystem = Main.Interface.GetSystem<RoomSystem>();
            _mapView = GetComponent<MapView>();

            if (_mapView == null)
            {
                _mapView = FindObjectOfType<MapView>();
            }

            // 订阅事件
            if (_occlusionSystem != null)
            {
                _occlusionSystem.OnOcclusionAlphaChanged += OnOcclusionAlphaChanged;
                _occlusionSystem.OnEnterRoom += OnEnterRoom;
                _occlusionSystem.OnExitRoom += OnExitRoom;
            }

            // 如果没有指定摄像机，使用主摄像机
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_occlusionSystem != null)
            {
                _occlusionSystem.OnOcclusionAlphaChanged -= OnOcclusionAlphaChanged;
                _occlusionSystem.OnEnterRoom -= OnEnterRoom;
                _occlusionSystem.OnExitRoom -= OnExitRoom;
            }
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            UpdateDetection();
        }

        /// <summary>
        /// 更新检测
        /// </summary>
        private void UpdateDetection()
        {
            if (_occlusionSystem == null)
                return;

            Vector3 detectPos = GetDetectionPosition();
            _occlusionSystem.UpdatePosition(detectPos);
        }

        /// <summary>
        /// 获取检测位置
        /// </summary>
        private Vector3 GetDetectionPosition()
        {
            switch (_detectionSource)
            {
                case EOcclusionDetectionSource.Camera:
                    if (_targetCamera != null)
                    {
                        return _targetCamera.transform.position;
                    }
                    break;

                case EOcclusionDetectionSource.Transform:
                    if (_targetTransform != null)
                    {
                        return _targetTransform.position;
                    }
                    break;

                case EOcclusionDetectionSource.Manual:
                    // 手动模式，返回上一次的位置
                    return Utility.IsometricUtility.CellCenterToScreen(
                        _occlusionSystem.CurrentCellPos.x,
                        _occlusionSystem.CurrentCellPos.y, 0);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 手动设置检测位置
        /// </summary>
        public void SetDetectionPosition(Vector3 worldPos)
        {
            if (_occlusionSystem != null)
            {
                _occlusionSystem.UpdatePosition(worldPos);
            }
        }

        /// <summary>
        /// 设置检测来源
        /// </summary>
        public void SetDetectionSource(EOcclusionDetectionSource source)
        {
            _detectionSource = source;
        }

        /// <summary>
        /// 设置目标摄像机
        /// </summary>
        public void SetTargetCamera(Camera camera)
        {
            _targetCamera = camera;
        }

        /// <summary>
        /// 设置目标变换
        /// </summary>
        public void SetTargetTransform(Transform target)
        {
            _targetTransform = target;
        }

        #region 事件处理

        /// <summary>
        /// 遮挡透明度变化
        /// </summary>
        private void OnOcclusionAlphaChanged(float alpha)
        {
            if (_mapView == null)
                return;

            // 更新所有 ChunkRenderer 的遮挡状态
            UpdateAllChunksOcclusion(alpha);
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        private void OnEnterRoom(RoomData room)
        {
            _lastRoomId = room.RoomId;
            Debug.Log($"[MapOcclusionController] Entered room {room.RoomId} (Area: {room.Area})");
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        private void OnExitRoom(RoomData room)
        {
            _lastRoomId = -1;
            Debug.Log($"[MapOcclusionController] Exited room {room.RoomId}");
        }

        /// <summary>
        /// 更新所有 Chunk 的遮挡状态
        /// </summary>
        private void UpdateAllChunksOcclusion(float alpha)
        {
            if (_mapView == null)
                return;

            bool isIndoor = _occlusionSystem.IsIndoor;

            // 遍历当前楼层的所有 Chunk
            // 这里简化处理：当在室内时，隐藏所有有屋顶的 Chunk 的屋顶
            // 更精确的实现可以只隐藏当前所在房间的屋顶

            var mapDataModel = Main.Interface.GetModel<Model.MapDataModel>();
            if (mapDataModel?.CurrentMap == null)
                return;

            int currentFloor = mapDataModel.CurrentFloor;

            for (int cy = 0; cy < mapDataModel.CurrentMap.ChunkCountY; cy++)
            {
                for (int cx = 0; cx < mapDataModel.CurrentMap.ChunkCountX; cx++)
                {
                    var chunkRenderer = _mapView.GetChunkRenderer(cx, cy, currentFloor);
                    if (chunkRenderer != null)
                    {
                        chunkRenderer.SetOcclusionState(isIndoor, alpha);
                    }
                }
            }
        }

        #endregion

        #region 调试

        private void OnGUI()
        {
            if (!_showDebugInfo || !_isInitialized || _occlusionSystem == null)
                return;

            GUILayout.BeginArea(new Rect(10, 220, 300, 150));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== 遮挡系统 ===");
            GUILayout.Label($"状态: {_occlusionSystem.CurrentState}");
            GUILayout.Label($"房间ID: {_occlusionSystem.CurrentRoomId}");
            GUILayout.Label($"格子位置: {_occlusionSystem.CurrentCellPos}");
            GUILayout.Label($"透明度: {_occlusionSystem.CurrentOcclusionAlpha:F2}");
            GUILayout.Label($"检测来源: {_detectionSource}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
