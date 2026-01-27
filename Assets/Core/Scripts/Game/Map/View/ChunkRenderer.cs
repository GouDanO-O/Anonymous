using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// Chunk 渲染器
    /// 管理单个 Chunk 的所有渲染层 Mesh
    /// </summary>
    public class ChunkRenderer : MonoBehaviour
    {
        #region 渲染层

        /// <summary>
        /// 地面层
        /// </summary>
        public MeshFilter GroundMeshFilter { get; private set; }
        public MeshRenderer GroundRenderer { get; private set; }

        /// <summary>
        /// 地板层
        /// </summary>
        public MeshFilter FloorMeshFilter { get; private set; }
        public MeshRenderer FloorRenderer { get; private set; }

        /// <summary>
        /// 墙体层
        /// </summary>
        public MeshFilter WallMeshFilter { get; private set; }
        public MeshRenderer WallRenderer { get; private set; }

        /// <summary>
        /// 屋顶层
        /// </summary>
        public MeshFilter RoofMeshFilter { get; private set; }
        public MeshRenderer RoofRenderer { get; private set; }

        #endregion

        #region 数据

        private ChunkData _chunkData;
        private ChunkMeshBuilder _meshBuilder;
        private Material _material;
        private Material _roofMaterialInstance;

        /// <summary>
        /// 所有渲染器列表（用于批量操作）
        /// </summary>
        private List<MeshRenderer> _allRenderers;

        /// <summary>
        /// 当前透明度
        /// </summary>
        private float _currentAlpha = 1f;

        #endregion

        #region 属性

        public int ChunkX => _chunkData?.ChunkX ?? 0;
        public int ChunkY => _chunkData?.ChunkY ?? 0;
        public int Floor => _chunkData?.Floor ?? 0;
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否有屋顶（该 Chunk 内是否有任何格子有屋顶）
        /// </summary>
        public bool HasRoof { get; private set; }

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(ChunkData chunkData, Material material, ChunkMeshBuilder meshBuilder)
        {
            _chunkData = chunkData;
            _material = material;
            _meshBuilder = meshBuilder;
            _allRenderers = new List<MeshRenderer>();

            CreateLayers();
            RebuildMesh();
            CheckHasRoof();

            IsInitialized = true;
        }

        /// <summary>
        /// 创建渲染层
        /// </summary>
        private void CreateLayers()
        {
            // 地面层
            var groundObj = CreateLayerObject("Ground", MapDefine.SortingOrderGround);
            GroundMeshFilter = groundObj.GetComponent<MeshFilter>();
            GroundRenderer = groundObj.GetComponent<MeshRenderer>();
            _allRenderers.Add(GroundRenderer);

            // 地板层
            var floorObj = CreateLayerObject("Floor", MapDefine.SortingOrderFloor);
            FloorMeshFilter = floorObj.GetComponent<MeshFilter>();
            FloorRenderer = floorObj.GetComponent<MeshRenderer>();
            _allRenderers.Add(FloorRenderer);

            // 墙体层
            var wallObj = CreateLayerObject("Wall", MapDefine.SortingOrderWall);
            WallMeshFilter = wallObj.GetComponent<MeshFilter>();
            WallRenderer = wallObj.GetComponent<MeshRenderer>();
            _allRenderers.Add(WallRenderer);

            // 屋顶层（使用独立材质实例以支持透明度）
            var roofObj = CreateLayerObject("Roof", MapDefine.SortingOrderPawn + 100);
            RoofMeshFilter = roofObj.GetComponent<MeshFilter>();
            RoofRenderer = roofObj.GetComponent<MeshRenderer>();

            // 为屋顶创建独立材质实例
            _roofMaterialInstance = new Material(_material);
            RoofRenderer.material = _roofMaterialInstance;
            _allRenderers.Add(RoofRenderer);
        }

        /// <summary>
        /// 创建单个渲染层对象
        /// </summary>
        private GameObject CreateLayerObject(string layerName, int sortingOrder)
        {
            var obj = new GameObject(layerName);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var meshFilter = obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();

            meshRenderer.material = _material;
            meshRenderer.sortingOrder = sortingOrder + Floor * 1000;

            return obj;
        }

        /// <summary>
        /// 重建所有 Mesh
        /// </summary>
        public void RebuildMesh()
        {
            if (_chunkData == null || _meshBuilder == null)
                return;

            // 清理旧 Mesh
            ClearMesh(GroundMeshFilter);
            ClearMesh(FloorMeshFilter);
            ClearMesh(WallMeshFilter);
            ClearMesh(RoofMeshFilter);

            // 构建新 Mesh
            GroundMeshFilter.mesh = _meshBuilder.BuildGroundMesh(_chunkData);
            FloorMeshFilter.mesh = _meshBuilder.BuildFloorMesh(_chunkData);
            WallMeshFilter.mesh = _meshBuilder.BuildWallMesh(_chunkData);
            RoofMeshFilter.mesh = _meshBuilder.BuildRoofMesh(_chunkData);

            _chunkData.IsDirty = false;

            // 更新屋顶状态
            CheckHasRoof();
        }

        /// <summary>
        /// 检查是否有屋顶
        /// </summary>
        private void CheckHasRoof()
        {
            HasRoof = RoofMeshFilter.mesh != null && RoofMeshFilter.mesh.vertexCount > 0;
        }

        /// <summary>
        /// 检查并更新（如果数据标记为脏）
        /// </summary>
        public void CheckAndUpdate()
        {
            if (_chunkData != null && _chunkData.IsDirty)
            {
                RebuildMesh();
            }
        }

        /// <summary>
        /// 设置可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// 设置楼层可见性（用于切换楼层时）
        /// </summary>
        public void SetFloorVisibility(int currentViewFloor)
        {
            // 当前楼层及以下可见
            bool visible = Floor <= currentViewFloor;
            SetVisible(visible);

            // 可以添加透明度渐变效果
            if (visible && Floor < currentViewFloor)
            {
                // 下层楼可以半透明
                SetAlpha(0.5f);
            }
            else
            {
                SetAlpha(1f);
            }
        }

        /// <summary>
        /// 设置屋顶可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        public void SetRoofVisible(bool visible)
        {
            if (RoofRenderer != null)
            {
                RoofRenderer.enabled = visible;
            }
        }

        /// <summary>
        /// 设置屋顶透明度
        /// </summary>
        /// <param name="alpha">透明度 0-1</param>
        public void SetRoofAlpha(float alpha)
        {
            if (_roofMaterialInstance != null)
            {
                Color color = _roofMaterialInstance.color;
                color.a = alpha;
                _roofMaterialInstance.color = color;

                // 根据透明度决定是否启用渲染
                if (RoofRenderer != null)
                {
                    RoofRenderer.enabled = alpha > 0.01f;
                }
            }
        }

        /// <summary>
        /// 设置整体透明度
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (Mathf.Approximately(_currentAlpha, alpha))
                return;

            _currentAlpha = alpha;

            // 设置所有渲染器的透明度
            foreach (var renderer in _allRenderers)
            {
                if (renderer == null)
                    continue;

                // 获取材质
                var mat = renderer.material;
                if (mat != null)
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;
                }
            }
        }

        /// <summary>
        /// 设置遮挡状态
        /// </summary>
        /// <param name="isIndoor">是否在室内</param>
        /// <param name="occlusionAlpha">遮挡透明度（0=完全遮挡，1=完全显示）</param>
        public void SetOcclusionState(bool isIndoor, float occlusionAlpha)
        {
            if (!HasRoof)
                return;

            // 在室内时，屋顶根据透明度显示
            SetRoofAlpha(occlusionAlpha);
        }

        /// <summary>
        /// 清理 Mesh
        /// </summary>
        private void ClearMesh(MeshFilter meshFilter)
        {
            if (meshFilter != null && meshFilter.mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(meshFilter.mesh);
                else
                    DestroyImmediate(meshFilter.mesh);

                meshFilter.mesh = null;
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        private void OnDestroy()
        {
            ClearMesh(GroundMeshFilter);
            ClearMesh(FloorMeshFilter);
            ClearMesh(WallMeshFilter);
            ClearMesh(RoofMeshFilter);

            // 销毁材质实例
            if (_roofMaterialInstance != null)
            {
                if (Application.isPlaying)
                    Destroy(_roofMaterialInstance);
                else
                    DestroyImmediate(_roofMaterialInstance);
            }
        }

        #region 静态工厂

        /// <summary>
        /// 创建 ChunkRenderer
        /// </summary>
        public static ChunkRenderer Create(ChunkData chunkData, Material material, ChunkMeshBuilder meshBuilder,
            Transform parent)
        {
            var obj = new GameObject($"Chunk_{chunkData.ChunkX}_{chunkData.ChunkY}_F{chunkData.Floor}");
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var renderer = obj.AddComponent<ChunkRenderer>();
            renderer.Initialize(chunkData, material, meshBuilder);

            return renderer;
        }

        #endregion
    }
}
