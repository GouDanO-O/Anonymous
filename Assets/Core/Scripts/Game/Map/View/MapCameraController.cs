using Core.Game.Map.Event;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using GDFramework.Input;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// 地图相机控制器
    /// 处理: 中键拖拽平移, 滚轮缩放, WASD移动, PageUp/PageDown楼层切换
    /// </summary>
    public class MapCameraController : MonoBehaviour, IController
    {
        [Header("平移")]
        [SerializeField] private float _keyboardMoveSpeed = 20f;
        [SerializeField] private float _dragSensitivity = 1f;

        [Header("缩放")]
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _minOrthoSize = 5f;
        [SerializeField] private float _maxOrthoSize = 60f;

        private Camera _camera;
        private MapSystem _mapSystem;

        // 地图边界
        private float _mapWidth;
        private float _mapHeight;
        private bool _hasBounds;

        // 中键拖拽状态
        private bool _isMiddleDragging;
        private Vector3 _dragOriginWorld;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Start()
        {
            _mapSystem = this.GetSystem<MapSystem>();

            this.RegisterEvent<SMapLoadedEvent>(OnMapLoaded);
            this.RegisterEvent<SInputEvent_MouseMiddleDown>(OnMiddleDown);
            this.RegisterEvent<SInputEvent_MouseMiddleUp>(OnMiddleUp);
            this.RegisterEvent<SInputEvent_MouseMiddleScroll>(OnScroll);

            // 如果地图已加载(错过了SMapLoadedEvent), 直接初始化边界
            if (_mapSystem.IsMapLoaded)
            {
                var mapModel = this.GetModel<MapDataModel>();
                var map = mapModel.CurrentMap;
                if (map != null)
                {
                    _mapWidth = map.Width;
                    _mapHeight = map.Height;
                    _hasBounds = true;
                }
            }
        }

        private void Update()
        {
            if (!_hasBounds) return;

            HandleKeyboardMove();
            HandleMiddleDrag();
            HandleFloorSwitch();
            ClampPosition();
        }

        #region 事件回调

        private void OnMapLoaded(SMapLoadedEvent evt)
        {
            _mapWidth = evt.MapData.Width;
            _mapHeight = evt.MapData.Height;
            _hasBounds = true;
        }

        private void OnMiddleDown(SInputEvent_MouseMiddleDown evt)
        {
            _isMiddleDragging = true;
            _dragOriginWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMiddleUp(SInputEvent_MouseMiddleUp evt)
        {
            _isMiddleDragging = false;
        }

        private void OnScroll(SInputEvent_MouseMiddleScroll evt)
        {
            float scrollDelta = evt.scrollValue.y;
            if (Mathf.Abs(scrollDelta) < 0.01f) return;

            // InputSystem scroll值通常为±120, 归一化后乘以缩放速度
            float normalized = Mathf.Sign(scrollDelta);
            float targetSize = _camera.orthographicSize - normalized * _zoomSpeed;
            _camera.orthographicSize = Mathf.Clamp(targetSize, _minOrthoSize, _maxOrthoSize);
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// WASD键盘移动
        /// </summary>
        private void HandleKeyboardMove()
        {
            float speed = _keyboardMoveSpeed * _camera.orthographicSize / 20f;
            Vector3 move = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                move.y += speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                move.y -= speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                move.x -= speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                move.x += speed * Time.deltaTime;

            if (move.sqrMagnitude > 0)
                transform.position += move;
        }

        /// <summary>
        /// 中键拖拽平移
        /// </summary>
        private void HandleMiddleDrag()
        {
            if (!_isMiddleDragging) return;

            Vector3 currentWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = _dragOriginWorld - currentWorld;
            transform.position += delta * _dragSensitivity;
        }

        /// <summary>
        /// 楼层切换
        /// </summary>
        private void HandleFloorSwitch()
        {
            if (_mapSystem == null || !_mapSystem.IsMapLoaded) return;

            if (Input.GetKeyDown(KeyCode.PageUp))
                _mapSystem.FloorUp();
            else if (Input.GetKeyDown(KeyCode.PageDown))
                _mapSystem.FloorDown();
        }

        /// <summary>
        /// 约束相机在地图范围内
        /// </summary>
        private void ClampPosition()
        {
            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;

            float minX = halfW;
            float maxX = _mapWidth - halfW;
            float minY = halfH;
            float maxY = _mapHeight - halfH;

            // 当缩放过大导致地图比视口小时，居中
            if (minX > maxX) minX = maxX = _mapWidth * 0.5f;
            if (minY > maxY) minY = maxY = _mapHeight * 0.5f;

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }

        #endregion

        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}
