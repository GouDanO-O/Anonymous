using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// 地图视图
    /// 管理所有 ChunkRenderer 的创建和更新
    /// </summary>
    public class MapView : MonoBehaviour
    {
        #region 配置

        [Header("渲染设置")]
        [SerializeField] private Material _tileMaterial;

        [Header("调试")]
        [SerializeField] private bool _showDebugInfo = true;

        #endregion

        #region 运行时数据

        private MapData _mapData;
        private ChunkMeshBuilder _meshBuilder;
        private Dictionary<(int, int, int), ChunkRenderer> _chunkRenderers;
        private int _currentViewFloor;

        #endregion

        #region 属性

        public bool IsInitialized { get; private set; }

        #endregion

        private void Awake()
        {
            _meshBuilder = new ChunkMeshBuilder();
            _chunkRenderers = new Dictionary<(int, int, int), ChunkRenderer>();

            // 如果没有指定材质，创建默认材质
            if (_tileMaterial == null)
            {
                CreateDefaultMaterial();
            }
        }

        /// <summary>
        /// 初始化地图视图
        /// </summary>
        public void Initialize(MapData mapData)
        {
            if (mapData == null)
                return;

            // 清理旧数据
            Clear();

            _mapData = mapData;
            _currentViewFloor = 0;

            // 创建所有 ChunkRenderer
            CreateAllChunkRenderers();

            IsInitialized = true;

            Debug.Log($"[MapView] Initialized with {_chunkRenderers.Count} chunks");
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            foreach (var kvp in _chunkRenderers)
            {
                if (kvp.Value != null)
                {
                    if (Application.isPlaying)
                        Destroy(kvp.Value.gameObject);
                    else
                        DestroyImmediate(kvp.Value.gameObject);
                }
            }

            _chunkRenderers.Clear();
            _mapData = null;
            IsInitialized = false;
        }

        /// <summary>
        /// 创建所有 ChunkRenderer
        /// </summary>
        private void CreateAllChunkRenderers()
        {
            if (_mapData == null)
                return;

            for (int floor = 0; floor < _mapData.FloorCount; floor++)
            {
                for (int cy = 0; cy < _mapData.ChunkCountY; cy++)
                {
                    for (int cx = 0; cx < _mapData.ChunkCountX; cx++)
                    {
                        var chunk = _mapData.GetChunk(cx, cy, floor);
                        if (chunk != null)
                        {
                            CreateChunkRenderer(chunk);
                        }
                    }
                }
            }

            // 初始化楼层可见性
            UpdateFloorVisibility();
        }

        /// <summary>
        /// 创建单个 ChunkRenderer
        /// </summary>
        private ChunkRenderer CreateChunkRenderer(ChunkData chunk)
        {
            var key = (chunk.ChunkX, chunk.ChunkY, chunk.Floor);

            if (_chunkRenderers.ContainsKey(key))
            {
                Debug.LogWarning($"[MapView] ChunkRenderer already exists: {key}");
                return _chunkRenderers[key];
            }

            var renderer = ChunkRenderer.Create(chunk, _tileMaterial, _meshBuilder, transform);
            _chunkRenderers[key] = renderer;

            return renderer;
        }

        /// <summary>
        /// 刷新指定 Chunk
        /// </summary>
        public void RefreshChunk(int chunkX, int chunkY, int floor)
        {
            var key = (chunkX, chunkY, floor);

            if (_chunkRenderers.TryGetValue(key, out var renderer))
            {
                renderer.RebuildMesh();
            }
        }

        /// <summary>
        /// 楼层变更
        /// </summary>
        public void OnFloorChanged(int oldFloor, int newFloor)
        {
            _currentViewFloor = newFloor;
            UpdateFloorVisibility();
        }

        /// <summary>
        /// 更新楼层可见性
        /// </summary>
        private void UpdateFloorVisibility()
        {
            foreach (var kvp in _chunkRenderers)
            {
                kvp.Value.SetFloorVisibility(_currentViewFloor);
            }
        }

        /// <summary>
        /// 创建默认材质
        /// </summary>
        private void CreateDefaultMaterial()
        {
            // 使用顶点颜色的简单 Shader
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            _tileMaterial = new Material(shader);
            _tileMaterial.name = "DefaultTileMaterial";

            Debug.Log("[MapView] Created default material");
        }

        /// <summary>
        /// 获取 ChunkRenderer
        /// </summary>
        public ChunkRenderer GetChunkRenderer(int chunkX, int chunkY, int floor)
        {
            var key = (chunkX, chunkY, floor);
            return _chunkRenderers.TryGetValue(key, out var renderer) ? renderer : null;
        }

        #region 调试

        private void OnGUI()
        {
            if (!_showDebugInfo || !IsInitialized)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"Map: {_mapData?.MapName ?? "None"}");
            GUILayout.Label($"Size: {_mapData?.Width ?? 0} x {_mapData?.Height ?? 0}");
            GUILayout.Label($"Floors: {_mapData?.FloorCount ?? 0}");
            GUILayout.Label($"Current Floor: {_currentViewFloor}");
            GUILayout.Label($"Chunks: {_chunkRenderers.Count}");

            GUILayout.Space(10);

            if (GUILayout.Button("Floor Up"))
            {
                // 需要通过 MapSystem 调用
            }

            if (GUILayout.Button("Floor Down"))
            {
                // 需要通过 MapSystem 调用
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        private void OnDestroy()
        {
            Clear();

            if (_tileMaterial != null && Application.isPlaying)
            {
                Destroy(_tileMaterial);
            }
        }
    }
}
