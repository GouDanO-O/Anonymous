using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// 地图视图
    /// 管理所有Chunk的渲染
    /// </summary>
    public class MapView : MonoBehaviour
    {
        #region 配置

        [Header("Camera")]
        [SerializeField] private Camera _mainCamera;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;

        #endregion

        #region 数据

        private MapData _mapData;
        private ChunkMeshBuilder _meshBuilder;

        /// <summary>
        /// Chunk渲染器字典 [floor, chunkY, chunkX] -> ChunkRenderer
        /// </summary>
        private Dictionary<Vector3Int, ChunkRenderer> _chunkRenderers;

        /// <summary>
        /// 楼层容器对象
        /// </summary>
        private Dictionary<int, GameObject> _floorContainers;

        // 系统引用
        private MapSystem _mapSystem;
        private MapDataModel _mapDataModel;
        private ChunkCullingSystem _cullingSystem;
        private MapOcclusionSystem _occlusionSystem;
        private RoomSystem _roomSystem;

        #endregion

        #region 属性

        public bool IsInitialized => _mapData != null;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            _chunkRenderers = new Dictionary<Vector3Int, ChunkRenderer>();
            _floorContainers = new Dictionary<int, GameObject>();
            _meshBuilder = new ChunkMeshBuilder();
        }

        private void Start()
        {
            // 获取系统引用
            _mapSystem = GameMain.Interface.GetSystem<MapSystem>();
            _mapDataModel = GameMain.Interface.GetModel<MapDataModel>();
            _cullingSystem = GameMain.Interface.GetSystem<ChunkCullingSystem>();
            _occlusionSystem = GameMain.Interface.GetSystem<MapOcclusionSystem>();
            _roomSystem = GameMain.Interface.GetSystem<RoomSystem>();

            // 注册视图
            _mapSystem.InitializeMapView(this);

            // 设置摄像机
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _mapSystem.SetMainCamera(_mainCamera);

            // 订阅事件
            _cullingSystem.OnChunksVisible += OnChunksVisible;
            _cullingSystem.OnChunksHidden += OnChunksHidden;
            _occlusionSystem.OnRoomAlphaChanged += OnRoomAlphaChanged;
        }

        private void Update()
        {
            if (!IsInitialized)
                return;

            // 更新地图系统
            _mapSystem.UpdateMap(Time.deltaTime);

            // 处理输入（悬停模式）
            if (_occlusionSystem.CurrentMode == EOcclusionMode.Hover)
            {
                _occlusionSystem.UpdateMouseScreenPosition(Input.mousePosition, _mainCamera);
            }
        }

        private void OnDestroy()
        {
            if (_cullingSystem != null)
            {
                _cullingSystem.OnChunksVisible -= OnChunksVisible;
                _cullingSystem.OnChunksHidden -= OnChunksHidden;
            }

            if (_occlusionSystem != null)
            {
                _occlusionSystem.OnRoomAlphaChanged -= OnRoomAlphaChanged;
            }

            Clear();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化地图视图
        /// </summary>
        public void Initialize(MapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogError("[MapView] Initialize failed: mapData is null");
                return;
            }

            Clear();

            _mapData = mapData;

            // 创建楼层容器
            CreateFloorContainers();

            // 创建所有Chunk渲染器
            CreateAllChunkRenderers();

            Debug.Log($"[MapView] Initialized with {_chunkRenderers.Count} chunk renderers");
        }

        /// <summary>
        /// 清除视图
        /// </summary>
        public void Clear()
        {
            // 销毁所有Chunk渲染器
            foreach (var renderer in _chunkRenderers.Values)
            {
                if (renderer != null)
                {
                    Destroy(renderer.gameObject);
                }
            }
            _chunkRenderers.Clear();

            // 销毁楼层容器
            foreach (var container in _floorContainers.Values)
            {
                if (container != null)
                {
                    Destroy(container);
                }
            }
            _floorContainers.Clear();

            _mapData = null;
        }

        /// <summary>
        /// 楼层变化处理
        /// </summary>
        public void OnFloorChanged(int oldFloor, int newFloor)
        {
            // 隐藏高于当前楼层的所有Chunk
            foreach (var kvp in _chunkRenderers)
            {
                var pos = kvp.Key;
                var renderer = kvp.Value;

                if (pos.z > newFloor)
                {
                    renderer.SetVisible(false);
                }
                else
                {
                    renderer.SetVisible(true);
                }
            }

            // 更新楼层容器可见性
            foreach (var kvp in _floorContainers)
            {
                kvp.Value.SetActive(kvp.Key <= newFloor);
            }
        }

        /// <summary>
        /// 刷新指定Chunk
        /// </summary>
        public void RefreshChunk(int chunkX, int chunkY, int floor)
        {
            var key = new Vector3Int(chunkX, chunkY, floor);
            if (_chunkRenderers.TryGetValue(key, out var renderer))
            {
                renderer.BuildAllMeshes();
            }
        }

        /// <summary>
        /// 刷新所有脏Chunk
        /// </summary>
        public void RefreshDirtyChunks()
        {
            foreach (var renderer in _chunkRenderers.Values)
            {
                renderer.RefreshIfDirty();
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建楼层容器
        /// </summary>
        private void CreateFloorContainers()
        {
            for (int floor = 0; floor < _mapData.FloorCount; floor++)
            {
                var container = new GameObject($"Floor_{floor}");
                container.transform.SetParent(transform, false);
                _floorContainers[floor] = container;
            }
        }

        /// <summary>
        /// 创建所有Chunk渲染器
        /// </summary>
        private void CreateAllChunkRenderers()
        {
            for (int floor = 0; floor < _mapData.FloorCount; floor++)
            {
                for (int cy = 0; cy < _mapData.ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < _mapData.ChunkCountX; cx++)
                    {
                        CreateChunkRenderer(cx, cy, floor);
                    }
                }
            }
        }

        /// <summary>
        /// 创建单个Chunk渲染器
        /// </summary>
        private ChunkRenderer CreateChunkRenderer(int chunkX, int chunkY, int floor)
        {
            var chunkData = _mapData.GetChunk(chunkX, chunkY, floor);
            if (chunkData == null)
                return null;

            var key = new Vector3Int(chunkX, chunkY, floor);
            if (_chunkRenderers.ContainsKey(key))
                return _chunkRenderers[key];

            // 创建GameObject
            var go = new GameObject($"Chunk_{chunkX}_{chunkY}_{floor}");

            // 设置父对象
            if (_floorContainers.TryGetValue(floor, out var container))
            {
                go.transform.SetParent(container.transform, false);
            }
            else
            {
                go.transform.SetParent(transform, false);
            }

            // 添加ChunkRenderer组件
            var renderer = go.AddComponent<ChunkRenderer>();
            renderer.Initialize(chunkData, _meshBuilder);

            // 默认隐藏（等待视野裁剪系统激活）
            renderer.SetVisible(false);

            _chunkRenderers[key] = renderer;
            return renderer;
        }

        /// <summary>
        /// 获取Chunk渲染器
        /// </summary>
        private ChunkRenderer GetChunkRenderer(int chunkX, int chunkY, int floor)
        {
            var key = new Vector3Int(chunkX, chunkY, floor);
            return _chunkRenderers.TryGetValue(key, out var renderer) ? renderer : null;
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// Chunk变为可见
        /// </summary>
        private void OnChunksVisible(List<Vector3Int> chunks)
        {
            foreach (var chunk in chunks)
            {
                var renderer = GetChunkRenderer(chunk.x, chunk.y, chunk.z);
                if (renderer != null)
                {
                    renderer.SetVisible(true);
                    renderer.RefreshIfDirty();
                }
            }
        }

        /// <summary>
        /// Chunk变为不可见
        /// </summary>
        private void OnChunksHidden(List<Vector3Int> chunks)
        {
            foreach (var chunk in chunks)
            {
                var renderer = GetChunkRenderer(chunk.x, chunk.y, chunk.z);
                if (renderer != null)
                {
                    renderer.SetVisible(false);
                }
            }
        }

        /// <summary>
        /// 房间透明度变化
        /// </summary>
        private void OnRoomAlphaChanged(int roomId, float alpha)
        {
            // 找到包含该房间的所有Chunk，更新屋顶透明度
            // 这是一个简化实现，实际需要更精确的房间-Chunk映射
            var room = _roomSystem.GetRoom(roomId);
            if (room == null)
                return;

            // 遍历房间内的格子，找到涉及的Chunk
            var affectedChunks = new HashSet<Vector3Int>();
            foreach (var cellPos in room.Cells)
            {
                int chunkX = cellPos.x / MapDefine.ChunkSize;
                int chunkY = cellPos.y / MapDefine.ChunkSize;
                affectedChunks.Add(new Vector3Int(chunkX, chunkY, room.Floor));
            }

            // 更新这些Chunk的屋顶透明度
            // 注意：这是简化实现，会影响整个Chunk而不是精确到房间
            // 后续可以优化为使用房间遮罩纹理
            foreach (var chunkPos in affectedChunks)
            {
                var renderer = GetChunkRenderer(chunkPos.x, chunkPos.y, chunkPos.z);
                if (renderer != null)
                {
                    renderer.SetRoofAlpha(alpha);
                }
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!_showDebugInfo || !IsInitialized)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Map: {_mapData.MapName}");
            GUILayout.Label($"Size: {_mapData.Width}x{_mapData.Height}");
            GUILayout.Label($"Chunks: {_mapData.ChunkCountX}x{_mapData.ChunkCountY}");
            GUILayout.Label($"Floor: {_mapDataModel.CurrentFloor}/{_mapData.FloorCount}");
            GUILayout.Label($"Visible Chunks: {_cullingSystem.VisibleChunks.Count}");
            GUILayout.Label($"Occlusion Mode: {_occlusionSystem.CurrentMode}");
            GUILayout.EndArea();
        }

        #endregion
    }
}
