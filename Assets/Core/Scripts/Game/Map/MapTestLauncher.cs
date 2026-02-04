using Core.Game.Map.Define;
using Core.Game.Map.System;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map
{
    /// <summary>
    /// 地图测试启动器
    /// 用于快速测试地图系统
    /// </summary>
    public class MapTestLauncher : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private int _mapWidth = 100;
        [SerializeField] private int _mapHeight = 100;
        [SerializeField] private int _floorCount = 2;
        [SerializeField] private int _seed = 12345;

        [Header("Camera Control")]
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _minZoom = 2f;
        [SerializeField] private float _maxZoom = 20f;

        [Header("Debug")]
        [SerializeField] private bool _autoLoadOnStart = true;

        private MapSystem _mapSystem;
        private MapOcclusionSystem _occlusionSystem;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;

            // 等待框架初始化
            if (GameMain.Interface == null)
            {
                Debug.LogError("[MapTestLauncher] GameMain not initialized!");
                return;
            }

            _mapSystem = GameMain.Interface.GetSystem<MapSystem>();
            _occlusionSystem = GameMain.Interface.GetSystem<MapOcclusionSystem>();

            if (_autoLoadOnStart)
            {
                LoadMap();
            }
        }

        private void Update()
        {
            HandleCameraMovement();
            HandleCameraZoom();
            HandleKeyboardInput();
        }

        #region Map Control

        [ContextMenu("Load Map")]
        public void LoadMap()
        {
            if (_mapSystem == null)
            {
                Debug.LogError("[MapTestLauncher] MapSystem not found!");
                return;
            }

            _mapSystem.LoadMap("TestMap", _mapWidth, _mapHeight, _floorCount, _seed);
            Debug.Log($"[MapTestLauncher] Map loaded: {_mapWidth}x{_mapHeight}, {_floorCount} floors");
        }

        [ContextMenu("Unload Map")]
        public void UnloadMap()
        {
            _mapSystem?.UnloadMap();
        }

        #endregion

        #region Camera Control

        private void HandleCameraMovement()
        {
            if (_mainCamera == null) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (h != 0 || v != 0)
            {
                Vector3 movement = new Vector3(h, v, 0) * _moveSpeed * Time.deltaTime;
                _mainCamera.transform.position += movement;
            }
        }

        private void HandleCameraZoom()
        {
            if (_mainCamera == null || !_mainCamera.orthographic) return;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                float newSize = _mainCamera.orthographicSize - scroll * _zoomSpeed;
                _mainCamera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
            }
        }

        #endregion

        #region Keyboard Input

        private void HandleKeyboardInput()
        {
            // 楼层切换
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                _mapSystem?.FloorUp();
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                _mapSystem?.FloorDown();
            }

            // 遮挡模式切换
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _occlusionSystem?.SetMode(EOcclusionMode.Global);
                Debug.Log("Occlusion Mode: Global");
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _occlusionSystem?.SetMode(EOcclusionMode.Hover);
                Debug.Log("Occlusion Mode: Hover");
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                _occlusionSystem?.SetMode(EOcclusionMode.FollowPawn);
                Debug.Log("Occlusion Mode: FollowPawn");
            }

            // 全局模式切换显隐
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _occlusionSystem?.ToggleGlobalInterior();
            }

            // 重新加载地图
            if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
            {
                _seed = Random.Range(0, int.MaxValue);
                LoadMap();
            }
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));

            GUILayout.Label("=== Map Test Controls ===");
            GUILayout.Space(10);

            GUILayout.Label("Movement: WASD / Arrow Keys");
            GUILayout.Label("Zoom: Mouse Scroll");
            GUILayout.Label("Floor Up/Down: PageUp / PageDown");
            GUILayout.Space(10);

            GUILayout.Label("Occlusion Mode:");
            GUILayout.Label("  F1 - Global Mode");
            GUILayout.Label("  F2 - Hover Mode");
            GUILayout.Label("  F3 - Follow Pawn Mode");
            GUILayout.Label("  Tab - Toggle Interior (Global Mode)");
            GUILayout.Space(10);

            GUILayout.Label("Ctrl+R - Reload with new seed");
            GUILayout.Space(20);

            if (_mapSystem != null && _mapSystem.IsMapLoaded)
            {
                GUILayout.Label($"Current Floor: {_mapSystem.CurrentFloor}");
                GUILayout.Label($"Occlusion Mode: {_occlusionSystem?.CurrentMode}");

                if (_mainCamera != null)
                {
                    var mousePos = Input.mousePosition;
                    var worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
                    var cellPos = _mapSystem.ScreenToCell(worldPos);
                    GUILayout.Label($"Mouse Cell: ({cellPos.x}, {cellPos.y})");

                    var roomId = _mapSystem.GetRoomId(cellPos.x, cellPos.y);
                    GUILayout.Label($"Room ID: {roomId}");
                }
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
