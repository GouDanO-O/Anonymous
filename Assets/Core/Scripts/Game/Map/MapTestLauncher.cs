using Core.Game.Map.Model;
using Core.Game.Map.System;
using Core.Game.Map.View;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map
{
    /// <summary>
    /// 地图测试启动器
    /// 用于快速测试地图系统
    /// 在场景中挂载此脚本即可自动生成测试地图
    /// </summary>
    public class MapTestLauncher : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private int _mapWidth = 32;
        [SerializeField] private int _mapHeight = 32;
        [SerializeField] private int _floorCount = 2;
        [SerializeField] private int _seed = 12345;
        [SerializeField] private bool _autoGenerate = true;

        [Header("摄像机")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _cameraSpeed = 10f;
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _minZoom = 2f;
        [SerializeField] private float _maxZoom = 20f;

        [Header("遮挡系统")]
        [SerializeField] private bool _enableOcclusion = true;

        private MapSystem _mapSystem;
        private MapDataModel _mapDataModel;
        private MapOcclusionSystem _occlusionSystem;
        private RoomSystem _roomSystem;
        private MapView _mapView;
        private MapOcclusionController _occlusionController;

        private void Awake()
        {
            // 确保架构已初始化
            var _ = Main.Interface;
        }

        private void Start()
        {
            // 获取系统引用
            _mapSystem = Main.Interface.GetSystem<MapSystem>();
            _mapDataModel = Main.Interface.GetModel<MapDataModel>();
            _occlusionSystem = Main.Interface.GetSystem<MapOcclusionSystem>();
            _roomSystem = Main.Interface.GetSystem<RoomSystem>();

            // 创建 MapView
            CreateMapView();

            // 设置摄像机
            SetupCamera();

            // 创建遮挡控制器
            if (_enableOcclusion)
            {
                CreateOcclusionController();
            }

            // 自动生成测试地图
            if (_autoGenerate)
            {
                GenerateTestMap();
            }
        }

        private void Update()
        {
            HandleCameraInput();
            HandleFloorInput();
            HandleOcclusionInput();
        }

        /// <summary>
        /// 创建 MapView
        /// </summary>
        private void CreateMapView()
        {
            var mapViewObj = new GameObject("MapView");
            _mapView = mapViewObj.AddComponent<MapView>();
            _mapSystem.InitializeMapView(_mapView);
        }

        /// <summary>
        /// 创建遮挡控制器
        /// </summary>
        private void CreateOcclusionController()
        {
            if (_mapView != null)
            {
                _occlusionController = _mapView.gameObject.AddComponent<MapOcclusionController>();
                _occlusionController.SetTargetCamera(_mainCamera);
            }
        }

        /// <summary>
        /// 设置摄像机
        /// </summary>
        private void SetupCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera == null)
            {
                var cameraObj = new GameObject("Main Camera");
                _mainCamera = cameraObj.AddComponent<Camera>();
                _mainCamera.orthographic = true;
                _mainCamera.orthographicSize = 10f;
                _mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
                _mainCamera.clearFlags = CameraClearFlags.SolidColor;
                cameraObj.tag = "MainCamera";
            }

            // 设置为正交摄像机
            _mainCamera.orthographic = true;

            // 初始位置移动到地图中心
            CenterCameraOnMap();
        }

        /// <summary>
        /// 生成测试地图
        /// </summary>
        [ContextMenu("Generate Test Map")]
        public void GenerateTestMap()
        {
            _mapSystem.LoadMap("TestMap", _mapWidth, _mapHeight, _floorCount, _seed);
            CenterCameraOnMap();
        }

        /// <summary>
        /// 将摄像机移动到地图中心
        /// </summary>
        private void CenterCameraOnMap()
        {
            if (_mainCamera == null || _mapDataModel?.CurrentMap == null)
                return;

            // 计算地图中心的屏幕坐标
            int centerX = _mapDataModel.CurrentMap.Width / 2;
            int centerY = _mapDataModel.CurrentMap.Height / 2;

            var centerPos = Utility.IsometricUtility.CellCenterToScreen(centerX, centerY, 0);
            _mainCamera.transform.position = new Vector3(centerPos.x, centerPos.y, -10f);
        }

        /// <summary>
        /// 将摄像机移动到某个建筑内
        /// </summary>
        private void MoveCameraToBuilding()
        {
            if (_mainCamera == null || _roomSystem == null)
                return;

            // 获取第一个房间
            var rooms = _roomSystem.GetRoomsInFloor(0);
            if (rooms.Count > 0)
            {
                var room = rooms[0];
                var centerPos = Utility.IsometricUtility.CellCenterToScreen(
                    (int)room.Center.x, (int)room.Center.y, 0);
                _mainCamera.transform.position = new Vector3(centerPos.x, centerPos.y, -10f);
            }
        }

        /// <summary>
        /// 处理摄像机输入
        /// </summary>
        private void HandleCameraInput()
        {
            if (_mainCamera == null)
                return;

            // WASD 移动
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                Vector3 move = new Vector3(horizontal, vertical, 0) * _cameraSpeed * Time.deltaTime;
                _mainCamera.transform.position += move;
            }

            // 滚轮缩放
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                float newSize = _mainCamera.orthographicSize - scroll * _zoomSpeed;
                _mainCamera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
            }
        }

        /// <summary>
        /// 处理楼层输入
        /// </summary>
        private void HandleFloorInput()
        {
            if (_mapSystem == null)
                return;

            // Page Up/Down 切换楼层
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                _mapSystem.FloorUp();
            }
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                _mapSystem.FloorDown();
            }

            // 数字键 1-8 直接切换楼层
            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _mapSystem.SetFloor(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 处理遮挡相关输入
        /// </summary>
        private void HandleOcclusionInput()
        {
            // O 键切换遮挡系统开关
            if (Input.GetKeyDown(KeyCode.O))
            {
                _enableOcclusion = !_enableOcclusion;
                if (_occlusionController != null)
                {
                    _occlusionController.enabled = _enableOcclusion;
                }

                // 如果关闭遮挡，恢复所有屋顶显示
                if (!_enableOcclusion && _occlusionSystem != null)
                {
                    _occlusionSystem.ForceSetState(EOcclusionState.Outdoor);
                }
            }

            // B 键移动到建筑内
            if (Input.GetKeyDown(KeyCode.B))
            {
                MoveCameraToBuilding();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 280));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== 地图测试控制 ===");
            GUILayout.Label("WASD: 移动摄像机");
            GUILayout.Label("滚轮: 缩放");
            GUILayout.Label("PageUp/Down: 切换楼层");
            GUILayout.Label("1-8: 直接跳转楼层");
            GUILayout.Label("O: 切换遮挡系统");
            GUILayout.Label("B: 移动到建筑内");

            GUILayout.Space(5);

            GUILayout.Label($"遮挡系统: {(_enableOcclusion ? "开启" : "关闭")}");
            if (_roomSystem != null)
            {
                var rooms = _roomSystem.GetRoomsInFloor(_mapDataModel?.CurrentFloor ?? 0);
                GUILayout.Label($"当前楼层房间数: {rooms?.Count ?? 0}");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("重新生成地图"))
            {
                _seed = Random.Range(0, int.MaxValue);
                GenerateTestMap();
            }

            if (GUILayout.Button("居中摄像机"))
            {
                CenterCameraOnMap();
            }

            if (GUILayout.Button("移动到建筑"))
            {
                MoveCameraToBuilding();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
