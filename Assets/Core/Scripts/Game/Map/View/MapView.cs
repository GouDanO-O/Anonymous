using System.Collections.Generic;
using Core.Game.Blueprint.Model;
using Core.Game.Item.Model;
using Core.Game.Map.Event;
using Core.Game.Map.System;
using Core.Game.Map.Utility;
using Core.Game.Pawn.View;
using Core.Game.Resource.Model;
using Core.Game.Selection.View;
using Core.Game.View;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// 地图场景视图协调器 (MonoBehaviour)
    /// 负责管理所有ChunkRenderer, 响应系统事件
    /// </summary>
    public class MapView : MonoBehaviour, IController
    {
        [SerializeField] private Camera _mainCamera;

        private MapSystem _mapSystem;
        private ChunkCullingSystem _cullingSystem;
        private FloorLevelSystem _floorLevelSystem;
        private MapOcclusionSystem _occlusionSystem;

        private ChunkMeshBuilder _meshBuilder;
        private Material _baseMaterial;

        /// <summary>
        /// 活跃的ChunkRenderer字典: key = (chunkX, chunkY, floor)
        /// </summary>
        private Dictionary<Vector3Int, ChunkRenderer> _activeRenderers =
            new Dictionary<Vector3Int, ChunkRenderer>();

        /// <summary>
        /// ChunkRenderer对象池
        /// </summary>
        private Queue<ChunkRenderer> _rendererPool = new Queue<ChunkRenderer>();

        /// <summary>
        /// 每层楼的容器GameObject
        /// </summary>
        private Dictionary<int, GameObject> _floorContainers = new Dictionary<int, GameObject>();

        private int _mapWidth;
        private int _mapHeight;

        private void Start()
        {
            _mapSystem = this.GetSystem<MapSystem>();
            _cullingSystem = this.GetSystem<ChunkCullingSystem>();
            _floorLevelSystem = this.GetSystem<FloorLevelSystem>();
            _occlusionSystem = this.GetSystem<MapOcclusionSystem>();

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            // 确保相机上有控制器
            if (_mainCamera != null && _mainCamera.GetComponent<MapCameraController>() == null)
                _mainCamera.gameObject.AddComponent<MapCameraController>();

            _cullingSystem.SetMainCamera(_mainCamera);
            _cullingSystem.OnChunksVisible += OnChunksVisible;
            _cullingSystem.OnChunksHidden += OnChunksHidden;

            _occlusionSystem.OnRoomAlphaChanged += OnRoomAlphaChanged;

            this.RegisterEvent<SMapLoadedEvent>(OnMapLoaded);
            this.RegisterEvent<SMapUnloadedEvent>(OnMapUnloaded);
            this.RegisterEvent<SFloorVisibilityChangedEvent>(OnFloorVisibilityChanged);
            this.RegisterEvent<SChunkDirtyEvent>(OnChunkDirty);

            InitializeRendering();
        }

        private void InitializeRendering()
        {
            _meshBuilder = new ChunkMeshBuilder();

            // 创建基础材质 (使用顶点色)
            _baseMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (_mapSystem != null && _mapSystem.IsMapLoaded)
            {
                _mapSystem.Tick(Time.deltaTime);
            }
        }

        #region 事件回调

        private void OnMapLoaded(SMapLoadedEvent evt)
        {
            _mapWidth = evt.MapData.Width;
            _mapHeight = evt.MapData.Height;

            // 创建楼层容器
            for (int f = 0; f < evt.MapData.FloorCount; f++)
            {
                var container = new GameObject($"Floor_{f}");
                container.transform.SetParent(transform, false);
                _floorContainers[f] = container;
            }

            // 设置相机位置到地图中心
            if (_mainCamera != null)
            {
                _mainCamera.transform.position = new Vector3(
                    _mapWidth * 0.5f, _mapHeight * 0.5f, -10f);
                _mainCamera.orthographic = true;
                _mainCamera.orthographicSize = 20f;
            }

            // 确保PawnView存在
            if (GetComponent<PawnView>() == null)
                gameObject.AddComponent<PawnView>();

            // 确保SelectionView存在
            if (GetComponent<SelectionView>() == null)
                gameObject.AddComponent<SelectionView>();

            // 确保GameHudView存在
            if (GetComponent<GameHudView>() == null)
                gameObject.AddComponent<GameHudView>();

            // 确保BuildModeView存在
            if (GetComponent<BuildModeView>() == null)
                gameObject.AddComponent<BuildModeView>();

            // 强制刷新裁剪
            _cullingSystem.ForceRefresh();

            LogKit.Log($"MapView: 地图加载完成 ({_mapWidth}x{_mapHeight})");
        }

        private void OnMapUnloaded(SMapUnloadedEvent evt)
        {
            // 回收所有活跃渲染器
            foreach (var kvp in _activeRenderers)
            {
                RecycleRenderer(kvp.Value);
            }
            _activeRenderers.Clear();

            // 销毁楼层容器
            foreach (var kvp in _floorContainers)
            {
                Destroy(kvp.Value);
            }
            _floorContainers.Clear();
        }

        private void OnChunksVisible(List<Vector3Int> chunks)
        {
            foreach (var key in chunks)
            {
                if (_activeRenderers.ContainsKey(key)) continue;

                var chunk = this.GetModel<Model.MapDataModel>()
                    .GetChunk(key.x, key.y, key.z);

                if (chunk == null) continue;

                var renderer = AllocateRenderer();
                renderer.transform.SetParent(GetFloorContainer(key.z).transform, false);
                var itemModel = this.GetModel<ItemDataModel>();
                var materialModel = this.GetModel<MaterialDataModel>();
                var blueprintModel = this.GetModel<BlueprintDataModel>();
                renderer.Initialize(chunk, _meshBuilder, _baseMaterial, _mapWidth, _mapHeight,
                    itemModel, materialModel, blueprintModel);
                renderer.SetVisible(true);

                // 应用楼层可见性
                bool roofVisible = _floorLevelSystem.ShouldShowRoof(key.z);
                renderer.SetRoofVisible(roofVisible);

                _activeRenderers[key] = renderer;
            }
        }

        private void OnChunksHidden(List<Vector3Int> chunks)
        {
            foreach (var key in chunks)
            {
                if (_activeRenderers.TryGetValue(key, out var renderer))
                {
                    RecycleRenderer(renderer);
                    _activeRenderers.Remove(key);
                }
            }
        }

        private void OnFloorVisibilityChanged(SFloorVisibilityChangedEvent evt)
        {
            int currentFloor = evt.CurrentViewFloor;

            // 更新所有活跃渲染器的屋顶可见性
            foreach (var kvp in _activeRenderers)
            {
                int floor = kvp.Key.z;
                var renderer = kvp.Value;

                if (floor > currentFloor)
                {
                    renderer.SetVisible(false);
                }
                else
                {
                    renderer.SetVisible(true);
                    renderer.SetRoofVisible(_floorLevelSystem.ShouldShowRoof(floor));
                }
            }
        }

        private void OnChunkDirty(SChunkDirtyEvent evt)
        {
            var key = new Vector3Int(evt.ChunkX, evt.ChunkY, evt.Floor);
            if (_activeRenderers.TryGetValue(key, out var renderer))
            {
                renderer.RebuildMeshes();
            }
        }

        private void OnRoomAlphaChanged(int roomId, float alpha)
        {
            // 找到该房间所在的所有chunk并更新透明度
            // 简化实现: 遍历所有活跃渲染器
            foreach (var kvp in _activeRenderers)
            {
                if (kvp.Key.z != _mapSystem.CurrentFloor) continue;

                // 检查该chunk是否包含该房间的cell
                var chunk = this.GetModel<Model.MapDataModel>()
                    .GetChunk(kvp.Key.x, kvp.Key.y, kvp.Key.z);

                if (chunk == null) continue;

                bool containsRoom = false;
                chunk.ForEachCell((cell, lx, ly) =>
                {
                    if (cell.RoomId == roomId)
                        containsRoom = true;
                });

                if (containsRoom)
                {
                    kvp.Value.SetWallAlpha(alpha);
                    kvp.Value.SetRoofAlpha(alpha);
                }
            }
        }

        #endregion

        #region 对象池

        private ChunkRenderer AllocateRenderer()
        {
            if (_rendererPool.Count > 0)
            {
                var renderer = _rendererPool.Dequeue();
                return renderer;
            }

            var go = new GameObject("ChunkRenderer");
            return go.AddComponent<ChunkRenderer>();
        }

        private void RecycleRenderer(ChunkRenderer renderer)
        {
            renderer.Reset();
            renderer.transform.SetParent(transform, false);
            _rendererPool.Enqueue(renderer);
        }

        private GameObject GetFloorContainer(int floor)
        {
            if (_floorContainers.TryGetValue(floor, out var container))
                return container;

            container = new GameObject($"Floor_{floor}");
            container.transform.SetParent(transform, false);
            _floorContainers[floor] = container;
            return container;
        }

        #endregion

        private void OnDestroy()
        {
            if (_baseMaterial != null)
                Destroy(_baseMaterial);

            if (_cullingSystem != null)
            {
                _cullingSystem.OnChunksVisible -= OnChunksVisible;
                _cullingSystem.OnChunksHidden -= OnChunksHidden;
            }

            if (_occlusionSystem != null)
                _occlusionSystem.OnRoomAlphaChanged -= OnRoomAlphaChanged;
        }

        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}
