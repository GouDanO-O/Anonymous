using Core.Game.Map.Data;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// Chunk渲染器
    /// 管理单个Chunk的Mesh渲染
    /// </summary>
    public class ChunkRenderer : MonoBehaviour
    {
        #region 组件引用

        private MeshFilter _groundMeshFilter;
        private MeshFilter _floorMeshFilter;
        private MeshFilter _wallMeshFilter;
        private MeshFilter _roofMeshFilter;

        private MeshRenderer _groundRenderer;
        private MeshRenderer _floorRenderer;
        private MeshRenderer _wallRenderer;
        private MeshRenderer _roofRenderer;

        #endregion

        #region 数据

        private ChunkData _chunkData;
        private ChunkMeshBuilder _meshBuilder;
        private MaterialPropertyBlock _roofPropBlock;

        /// <summary>
        /// 当前屋顶透明度
        /// </summary>
        public float RoofAlpha { get; private set; } = 1f;

        #endregion

        #region 材质

        private static Material _sharedGroundMaterial;
        private static Material _sharedFloorMaterial;
        private static Material _sharedWallMaterial;
        private static Material _sharedRoofMaterial;

        #endregion

        #region 属性

        public ChunkData ChunkData => _chunkData;
        public int ChunkX => _chunkData?.ChunkX ?? 0;
        public int ChunkY => _chunkData?.ChunkY ?? 0;
        public int Floor => _chunkData?.Floor ?? 0;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化Chunk渲染器
        /// </summary>
        public void Initialize(ChunkData chunkData, ChunkMeshBuilder meshBuilder)
        {
            _chunkData = chunkData;
            _meshBuilder = meshBuilder;
            _roofPropBlock = new MaterialPropertyBlock();

            CreateChildObjects();
            InitializeMaterials();
            BuildAllMeshes();
        }

        /// <summary>
        /// 创建子对象
        /// </summary>
        private void CreateChildObjects()
        {
            // Ground
            var groundObj = new GameObject("Ground");
            groundObj.transform.SetParent(transform, false);
            _groundMeshFilter = groundObj.AddComponent<MeshFilter>();
            _groundRenderer = groundObj.AddComponent<MeshRenderer>();

            // Floor
            var floorObj = new GameObject("Floor");
            floorObj.transform.SetParent(transform, false);
            _floorMeshFilter = floorObj.AddComponent<MeshFilter>();
            _floorRenderer = floorObj.AddComponent<MeshRenderer>();

            // Wall
            var wallObj = new GameObject("Wall");
            wallObj.transform.SetParent(transform, false);
            _wallMeshFilter = wallObj.AddComponent<MeshFilter>();
            _wallRenderer = wallObj.AddComponent<MeshRenderer>();

            // Roof
            var roofObj = new GameObject("Roof");
            roofObj.transform.SetParent(transform, false);
            _roofMeshFilter = roofObj.AddComponent<MeshFilter>();
            _roofRenderer = roofObj.AddComponent<MeshRenderer>();
        }

        /// <summary>
        /// 初始化材质
        /// </summary>
        private void InitializeMaterials()
        {
            // 使用顶点颜色的Unlit着色器
            if (_sharedGroundMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    shader = Shader.Find("Unlit/Color");

                _sharedGroundMaterial = new Material(shader);
                _sharedFloorMaterial = new Material(shader);
                _sharedWallMaterial = new Material(shader);
                _sharedRoofMaterial = new Material(shader);

                // 屋顶材质需要支持透明
                _sharedRoofMaterial.SetFloat("_Mode", 3); // Transparent
                _sharedRoofMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _sharedRoofMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _sharedRoofMaterial.SetInt("_ZWrite", 0);
                _sharedRoofMaterial.DisableKeyword("_ALPHATEST_ON");
                _sharedRoofMaterial.EnableKeyword("_ALPHABLEND_ON");
                _sharedRoofMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                _sharedRoofMaterial.renderQueue = 3000;
            }

            _groundRenderer.sharedMaterial = _sharedGroundMaterial;
            _floorRenderer.sharedMaterial = _sharedFloorMaterial;
            _wallRenderer.sharedMaterial = _sharedWallMaterial;
            _roofRenderer.sharedMaterial = _sharedRoofMaterial;
        }

        #endregion

        #region Mesh构建

        /// <summary>
        /// 构建所有Mesh
        /// </summary>
        public void BuildAllMeshes()
        {
            if (_chunkData == null || _meshBuilder == null)
                return;

            BuildGroundMesh();
            BuildFloorMesh();
            BuildWallMesh();
            BuildRoofMesh();

            _chunkData.ClearDirty();
        }

        /// <summary>
        /// 构建地面Mesh
        /// </summary>
        public void BuildGroundMesh()
        {
            if (_groundMeshFilter.sharedMesh != null)
                DestroyImmediate(_groundMeshFilter.sharedMesh);

            _groundMeshFilter.sharedMesh = _meshBuilder.BuildGroundMesh(_chunkData);
        }

        /// <summary>
        /// 构建地板Mesh
        /// </summary>
        public void BuildFloorMesh()
        {
            if (_floorMeshFilter.sharedMesh != null)
                DestroyImmediate(_floorMeshFilter.sharedMesh);

            _floorMeshFilter.sharedMesh = _meshBuilder.BuildFloorMesh(_chunkData);
        }

        /// <summary>
        /// 构建墙体Mesh
        /// </summary>
        public void BuildWallMesh()
        {
            if (_wallMeshFilter.sharedMesh != null)
                DestroyImmediate(_wallMeshFilter.sharedMesh);

            _wallMeshFilter.sharedMesh = _meshBuilder.BuildWallMesh(_chunkData);
        }

        /// <summary>
        /// 构建屋顶Mesh
        /// </summary>
        public void BuildRoofMesh()
        {
            if (_roofMeshFilter.sharedMesh != null)
                DestroyImmediate(_roofMeshFilter.sharedMesh);

            _roofMeshFilter.sharedMesh = _meshBuilder.BuildRoofMesh(_chunkData);
        }

        #endregion

        #region 显示控制

        /// <summary>
        /// 设置屋顶透明度
        /// </summary>
        public void SetRoofAlpha(float alpha)
        {
            RoofAlpha = Mathf.Clamp01(alpha);

            if (_roofRenderer != null)
            {
                _roofRenderer.GetPropertyBlock(_roofPropBlock);
                _roofPropBlock.SetColor("_Color", new Color(1, 1, 1, RoofAlpha));
                _roofRenderer.SetPropertyBlock(_roofPropBlock);

                // 完全透明时隐藏
                _roofRenderer.enabled = RoofAlpha > 0.01f;
            }
        }

        /// <summary>
        /// 设置整个Chunk的可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// 设置各层的可见性
        /// </summary>
        public void SetLayerVisible(EChunkLayer layer, bool visible)
        {
            switch (layer)
            {
                case EChunkLayer.Ground:
                    _groundRenderer.enabled = visible;
                    break;
                case EChunkLayer.Floor:
                    _floorRenderer.enabled = visible;
                    break;
                case EChunkLayer.Wall:
                    _wallRenderer.enabled = visible;
                    break;
                case EChunkLayer.Roof:
                    _roofRenderer.enabled = visible;
                    break;
            }
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 如果数据脏了，刷新Mesh
        /// </summary>
        public void RefreshIfDirty()
        {
            if (_chunkData != null && _chunkData.IsDirty)
            {
                BuildAllMeshes();
            }
        }

        /// <summary>
        /// 刷新指定层
        /// </summary>
        public void RefreshLayer(EChunkLayer layer)
        {
            switch (layer)
            {
                case EChunkLayer.Ground:
                    BuildGroundMesh();
                    break;
                case EChunkLayer.Floor:
                    BuildFloorMesh();
                    break;
                case EChunkLayer.Wall:
                    BuildWallMesh();
                    break;
                case EChunkLayer.Roof:
                    BuildRoofMesh();
                    break;
            }
        }

        #endregion

        #region 清理

        private void OnDestroy()
        {
            // 清理Mesh
            if (_groundMeshFilter != null && _groundMeshFilter.sharedMesh != null)
                DestroyImmediate(_groundMeshFilter.sharedMesh);
            if (_floorMeshFilter != null && _floorMeshFilter.sharedMesh != null)
                DestroyImmediate(_floorMeshFilter.sharedMesh);
            if (_wallMeshFilter != null && _wallMeshFilter.sharedMesh != null)
                DestroyImmediate(_wallMeshFilter.sharedMesh);
            if (_roofMeshFilter != null && _roofMeshFilter.sharedMesh != null)
                DestroyImmediate(_roofMeshFilter.sharedMesh);
        }

        #endregion
    }

    /// <summary>
    /// Chunk层级枚举
    /// </summary>
    public enum EChunkLayer
    {
        Ground,
        Floor,
        Wall,
        Roof
    }
}
