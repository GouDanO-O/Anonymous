/*******************************************************************************
 * 文件名:    CameraController.cs
 * 描述:      俯视角相机控制器，支持平移、缩放、边缘滚动
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   CameraController 提供：
 *   - WASD/方向键平移
 *   - 鼠标滚轮缩放
 *   - 鼠标中键拖拽
 *   - 屏幕边缘滚动
 *   - 相机边界限制
 ******************************************************************************/

using System;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// 俯视角相机控制器
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        #region 序列化字段

        [Header("移动设置")]
        [SerializeField]
        private float _moveSpeed = 20f;

        [SerializeField]
        private float _fastMoveMultiplier = 2f;

        [SerializeField]
        private bool _enableEdgeScrolling = true;

        [SerializeField]
        private float _edgeScrollThreshold = 20f;

        [SerializeField]
        private float _edgeScrollSpeed = 15f;

        [Header("缩放设置")]
        [SerializeField]
        private float _zoomSpeed = 5f;

        [SerializeField]
        private float _minZoom = 5f;

        [SerializeField]
        private float _maxZoom = 50f;

        [SerializeField]
        private float _zoomSmoothTime = 0.1f;

        [Header("拖拽设置")]
        [SerializeField]
        private bool _enableDragPan = true;

        [SerializeField]
        private float _dragSensitivity = 1f;

        [Header("边界限制")]
        [SerializeField]
        private bool _enableBounds = true;

        [SerializeField]
        private float _boundsPadding = 5f;

        [Header("平滑")]
        [SerializeField]
        private float _moveSmoothTime = 0.1f;

        #endregion

        #region 字段

        /// <summary>
        /// 相机组件
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// 当前Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 目标位置
        /// </summary>
        private Vector3 _targetPosition;

        /// <summary>
        /// 目标缩放
        /// </summary>
        private float _targetZoom;

        /// <summary>
        /// 当前缩放速度
        /// </summary>
        private float _zoomVelocity;

        /// <summary>
        /// 当前移动速度
        /// </summary>
        private Vector3 _moveVelocity;

        /// <summary>
        /// 是否正在拖拽
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// 拖拽起始位置
        /// </summary>
        private Vector3 _dragStartPosition;

        /// <summary>
        /// 拖拽起始相机位置
        /// </summary>
        private Vector3 _dragStartCameraPosition;

        /// <summary>
        /// 边界
        /// </summary>
        private Bounds _mapBounds;

        #endregion

        #region 属性

        /// <summary>
        /// 当前缩放级别
        /// </summary>
        public float Zoom => _camera?.orthographicSize ?? _targetZoom;

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving => _moveVelocity.sqrMagnitude > 0.01f;

        /// <summary>
        /// 是否正在缩放
        /// </summary>
        public bool IsZooming => Mathf.Abs(_zoomVelocity) > 0.01f;

        #endregion

        #region 单例

        private static CameraController _instance;
        public static CameraController Instance => _instance;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            _instance = this;
            _camera = GetComponent<Camera>();

            // 设置为正交相机（俯视图）
            _camera.orthographic = true;
            _targetZoom = _camera.orthographicSize;
            _targetPosition = transform.position;
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            HandleEdgeScrolling();
            
            ApplyMovement();
            ApplyZoom();
            ClampToBounds();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 设置Site
        /// </summary>
        public void SetSite(Site site)
        {
            _site = site;
            UpdateMapBounds();

            // 初始位置设为地图中心
            if (site != null)
            {
                float centerX = site.SizeX * site.CellSize / 2f;
                float centerZ = site.SizeZ * site.CellSize / 2f;
                SetPosition(new Vector3(centerX, transform.position.y, centerZ));
            }
        }

        /// <summary>
        /// 更新地图边界
        /// </summary>
        private void UpdateMapBounds()
        {
            if (_site == null)
            {
                _mapBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
                return;
            }

            float sizeX = _site.SizeX * _site.CellSize;
            float sizeZ = _site.SizeZ * _site.CellSize;

            _mapBounds = new Bounds(
                new Vector3(sizeX / 2f, 0, sizeZ / 2f),
                new Vector3(sizeX + _boundsPadding * 2, 100, sizeZ + _boundsPadding * 2)
            );
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            Vector3 moveDirection = Vector3.zero;

            // WASD / 方向键
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveDirection.z += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveDirection.z -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveDirection.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveDirection.x += 1;

            // 加速
            float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? _fastMoveMultiplier : 1f;

            // 根据缩放调整移动速度
            float zoomFactor = _camera.orthographicSize / 10f;

            // 应用移动
            if (moveDirection.sqrMagnitude > 0)
            {
                moveDirection.Normalize();
                _targetPosition += moveDirection * _moveSpeed * speedMultiplier * zoomFactor * Time.deltaTime;
            }
        }

        /// <summary>
        /// 处理鼠标输入
        /// </summary>
        private void HandleMouseInput()
        {
            // 滚轮缩放
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                // 获取鼠标位置作为缩放中心
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                
                // 计算新的缩放级别
                float newZoom = _targetZoom - scrollDelta * _zoomSpeed;
                newZoom = Mathf.Clamp(newZoom, _minZoom, _maxZoom);

                // 缩放时保持鼠标下的点不动
                float zoomRatio = newZoom / _targetZoom;
                Vector3 offset = _targetPosition - mouseWorldPos;
                _targetPosition = mouseWorldPos + offset * zoomRatio;

                _targetZoom = newZoom;
            }

            // 中键拖拽
            if (_enableDragPan)
            {
                if (Input.GetMouseButtonDown(2))
                {
                    _isDragging = true;
                    _dragStartPosition = GetMouseWorldPosition();
                    _dragStartCameraPosition = _targetPosition;
                }
                else if (Input.GetMouseButtonUp(2))
                {
                    _isDragging = false;
                }

                if (_isDragging)
                {
                    Vector3 currentMousePos = GetMouseWorldPosition();
                    Vector3 delta = _dragStartPosition - currentMousePos;
                    _targetPosition = _dragStartCameraPosition + delta * _dragSensitivity;
                }
            }
        }

        /// <summary>
        /// 处理屏幕边缘滚动
        /// </summary>
        private void HandleEdgeScrolling()
        {
            if (!_enableEdgeScrolling || _isDragging)
                return;

            Vector3 mousePos = Input.mousePosition;
            Vector3 moveDirection = Vector3.zero;

            // 左边缘
            if (mousePos.x < _edgeScrollThreshold)
                moveDirection.x -= 1;
            // 右边缘
            else if (mousePos.x > Screen.width - _edgeScrollThreshold)
                moveDirection.x += 1;

            // 下边缘
            if (mousePos.y < _edgeScrollThreshold)
                moveDirection.z -= 1;
            // 上边缘
            else if (mousePos.y > Screen.height - _edgeScrollThreshold)
                moveDirection.z += 1;

            if (moveDirection.sqrMagnitude > 0)
            {
                float zoomFactor = _camera.orthographicSize / 10f;
                _targetPosition += moveDirection.normalized * _edgeScrollSpeed * zoomFactor * Time.deltaTime;
            }
        }

        #endregion

        #region 应用变换

        /// <summary>
        /// 应用移动
        /// </summary>
        private void ApplyMovement()
        {
            Vector3 newPos = Vector3.SmoothDamp(
                transform.position, 
                _targetPosition, 
                ref _moveVelocity, 
                _moveSmoothTime
            );

            transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        }

        /// <summary>
        /// 应用缩放
        /// </summary>
        private void ApplyZoom()
        {
            float newZoom = Mathf.SmoothDamp(
                _camera.orthographicSize,
                _targetZoom,
                ref _zoomVelocity,
                _zoomSmoothTime
            );

            _camera.orthographicSize = newZoom;
        }

        /// <summary>
        /// 限制在边界内
        /// </summary>
        private void ClampToBounds()
        {
            if (!_enableBounds || _site == null)
                return;

            // 计算可视范围
            float verticalSize = _camera.orthographicSize;
            float horizontalSize = verticalSize * _camera.aspect;

            // 计算允许的位置范围
            float minX = _mapBounds.min.x + horizontalSize;
            float maxX = _mapBounds.max.x - horizontalSize;
            float minZ = _mapBounds.min.z + verticalSize;
            float maxZ = _mapBounds.max.z - verticalSize;

            // 如果地图比视野小，居中显示
            if (minX > maxX)
            {
                minX = maxX = _mapBounds.center.x;
            }
            if (minZ > maxZ)
            {
                minZ = maxZ = _mapBounds.center.z;
            }

            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minX, maxX);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, minZ, maxZ);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置相机位置
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            _targetPosition = position;
            transform.position = new Vector3(position.x, transform.position.y, position.z);
        }

        /// <summary>
        /// 移动到指定格子
        /// </summary>
        public void MoveTo(CellCoord cell, bool instant = false)
        {
            if (_site == null)
                return;

            float cellSize = _site.CellSize;
            Vector3 worldPos = new Vector3(
                (cell.x + 0.5f) * cellSize,
                transform.position.y,
                (cell.z + 0.5f) * cellSize
            );

            _targetPosition = worldPos;

            if (instant)
            {
                transform.position = worldPos;
            }
        }

        /// <summary>
        /// 设置缩放级别
        /// </summary>
        public void SetZoom(float zoom, bool instant = false)
        {
            _targetZoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);

            if (instant)
            {
                _camera.orthographicSize = _targetZoom;
            }
        }

        /// <summary>
        /// 聚焦到指定区域
        /// </summary>
        public void FocusOnRect(CellRect rect, float padding = 2f)
        {
            if (_site == null)
                return;

            float cellSize = _site.CellSize;

            // 计算中心点
            float centerX = (rect.minX + rect.maxX + 1) / 2f * cellSize;
            float centerZ = (rect.minZ + rect.maxZ + 1) / 2f * cellSize;
            _targetPosition = new Vector3(centerX, transform.position.y, centerZ);

            // 计算需要的缩放级别
            float worldWidth = (rect.Width + padding * 2) * cellSize;
            float worldHeight = (rect.Height + padding * 2) * cellSize;

            float zoomForWidth = worldWidth / (2f * _camera.aspect);
            float zoomForHeight = worldHeight / 2f;

            _targetZoom = Mathf.Clamp(Mathf.Max(zoomForWidth, zoomForHeight), _minZoom, _maxZoom);
        }

        /// <summary>
        /// 获取鼠标在世界空间的位置
        /// </summary>
        public Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = transform.position.y;
            return _camera.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// 获取鼠标所在的格子
        /// </summary>
        public CellCoord GetMouseCell()
        {
            if (_site == null)
                return CellCoord.Invalid;

            Vector3 worldPos = GetMouseWorldPosition();
            float cellSize = _site.CellSize;

            int x = Mathf.FloorToInt(worldPos.x / cellSize);
            int z = Mathf.FloorToInt(worldPos.z / cellSize);

            if (x < 0 || x >= _site.SizeX || z < 0 || z >= _site.SizeZ)
                return CellCoord.Invalid;

            return new CellCoord(x, z);
        }

        /// <summary>
        /// 屏幕抖动
        /// </summary>
        public void Shake(float intensity = 0.5f, float duration = 0.3f)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            Vector3 originalPos = _targetPosition;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentIntensity = intensity * (1 - progress);

                Vector3 offset = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f) * currentIntensity,
                    0,
                    UnityEngine.Random.Range(-1f, 1f) * currentIntensity
                );

                transform.position = originalPos + offset;
                yield return null;
            }

            transform.position = originalPos;
        }

        #endregion
    }
}
