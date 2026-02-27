using Core.Game.Item.Define;
using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Utility;
using UnityEngine;

namespace Core.Game.Map.View
{
    /// <summary>
    /// 单个Chunk的渲染器
    /// 管理5个子Mesh (地形/地板/物品/墙/屋顶)
    /// </summary>
    public class ChunkRenderer : MonoBehaviour
    {
        private ChunkData _chunkData;
        private ChunkMeshBuilder _meshBuilder;
        private ItemDataModel _itemModel;
        private int _mapWidth;
        private int _mapHeight;

        // 子渲染层
        private MeshFilter _terrainFilter;
        private MeshRenderer _terrainRenderer;

        private MeshFilter _floorFilter;
        private MeshRenderer _floorRenderer;

        private MeshFilter _objectFilter;
        private MeshRenderer _objectRenderer;

        private MeshFilter _wallFilter;
        private MeshRenderer _wallRenderer;

        private MeshFilter _roofFilter;
        private MeshRenderer _roofRenderer;

        // 材质实例 (用于alpha控制)
        private MaterialPropertyBlock _wallPropBlock;
        private MaterialPropertyBlock _roofPropBlock;

        private bool _isInitialized;
        private bool _roofVisible = true;

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        // 层级排序偏移量 (保证不同层级之间不会交叉)
        private const int SortOrderLayerOffset = 5000;
        private const int TerrainLayerBase = 0;
        private const int FloorLayerBase = SortOrderLayerOffset;
        private const int ObjectLayerBase = ItemConst.ItemSortOrderBase; // 7500
        private const int WallLayerBase = SortOrderLayerOffset * 2;
        private const int RoofLayerBase = SortOrderLayerOffset * 3;

        public void Initialize(ChunkData chunkData, ChunkMeshBuilder meshBuilder,
            Material baseMaterial, int mapWidth, int mapHeight,
            ItemDataModel itemModel = null)
        {
            _chunkData = chunkData;
            _meshBuilder = meshBuilder;
            _itemModel = itemModel;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;

            if (!_isInitialized)
            {
                CreateLayerRenderers(baseMaterial);
                _wallPropBlock = new MaterialPropertyBlock();
                _roofPropBlock = new MaterialPropertyBlock();
                _isInitialized = true;
            }

            // 设置位置
            transform.position = MapCoordUtility.ChunkToWorldCorner(
                chunkData.ChunkX, chunkData.ChunkY);

            // 使用sortingOrder偏移保证层级顺序, 不依赖自定义Sorting Layer
            int baseSortOrder = MapCoordUtility.CalculateSortingOrder(
                chunkData.WorldStartY, chunkData.Floor, mapHeight);

            _terrainRenderer.sortingOrder = TerrainLayerBase + baseSortOrder;
            _floorRenderer.sortingOrder = FloorLayerBase + baseSortOrder;
            _objectRenderer.sortingOrder = ObjectLayerBase + baseSortOrder;
            _wallRenderer.sortingOrder = WallLayerBase + baseSortOrder;
            _roofRenderer.sortingOrder = RoofLayerBase + baseSortOrder;

            RebuildMeshes();
        }

        /// <summary>
        /// 重建所有Mesh
        /// </summary>
        public void RebuildMeshes()
        {
            if (_chunkData == null || _meshBuilder == null) return;

            DestroyMesh(_terrainFilter);
            DestroyMesh(_floorFilter);
            DestroyMesh(_objectFilter);
            DestroyMesh(_wallFilter);
            DestroyMesh(_roofFilter);

            _terrainFilter.mesh = _meshBuilder.BuildTerrainMesh(_chunkData, _mapWidth, _mapHeight);
            _floorFilter.mesh = _meshBuilder.BuildFloorMesh(_chunkData, _mapWidth, _mapHeight);
            _objectFilter.mesh = _meshBuilder.BuildObjectMesh(_chunkData, _itemModel, _chunkData.Floor);
            _wallFilter.mesh = _meshBuilder.BuildWallMesh(_chunkData, _mapWidth, _mapHeight);
            _roofFilter.mesh = _meshBuilder.BuildRoofMesh(_chunkData, _mapWidth, _mapHeight);

            _chunkData.ClearDirty();
        }

        /// <summary>
        /// 设置墙壁透明度 (遮挡系统使用)
        /// </summary>
        public void SetWallAlpha(float alpha)
        {
            _wallPropBlock.SetColor(ColorProperty, new Color(1, 1, 1, alpha));
            _wallRenderer.SetPropertyBlock(_wallPropBlock);
        }

        /// <summary>
        /// 设置屋顶透明度
        /// </summary>
        public void SetRoofAlpha(float alpha)
        {
            _roofPropBlock.SetColor(ColorProperty, new Color(1, 1, 1, alpha));
            _roofRenderer.SetPropertyBlock(_roofPropBlock);
        }

        /// <summary>
        /// 设置屋顶是否显示
        /// </summary>
        public void SetRoofVisible(bool visible)
        {
            _roofVisible = visible;
            if (_roofRenderer != null)
                _roofRenderer.enabled = visible;
        }

        /// <summary>
        /// 设置整个Chunk是否可见
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// 重置状态 (回收到池前调用)
        /// </summary>
        public void Reset()
        {
            _chunkData = null;
            _itemModel = null;

            DestroyMesh(_terrainFilter);
            DestroyMesh(_floorFilter);
            DestroyMesh(_objectFilter);
            DestroyMesh(_wallFilter);
            DestroyMesh(_roofFilter);

            SetWallAlpha(1f);
            SetRoofAlpha(1f);
            SetRoofVisible(true);
            gameObject.SetActive(false);
        }

        private void CreateLayerRenderers(Material baseMaterial)
        {
            _terrainFilter = CreateLayerChild("Terrain", baseMaterial, out _terrainRenderer);
            _floorFilter = CreateLayerChild("Floor", baseMaterial, out _floorRenderer);
            _objectFilter = CreateLayerChild("Object", baseMaterial, out _objectRenderer);
            _wallFilter = CreateLayerChild("Wall", baseMaterial, out _wallRenderer);
            _roofFilter = CreateLayerChild("Roof", baseMaterial, out _roofRenderer);
        }

        private MeshFilter CreateLayerChild(string layerName, Material material,
            out MeshRenderer meshRenderer)
        {
            var go = new GameObject(layerName);
            go.transform.SetParent(transform, false);

            var filter = go.AddComponent<MeshFilter>();
            meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            return filter;
        }

        private void DestroyMesh(MeshFilter filter)
        {
            if (filter != null && filter.mesh != null)
            {
                Destroy(filter.mesh);
                filter.mesh = null;
            }
        }

        private void OnDestroy()
        {
            DestroyMesh(_terrainFilter);
            DestroyMesh(_floorFilter);
            DestroyMesh(_objectFilter);
            DestroyMesh(_wallFilter);
            DestroyMesh(_roofFilter);
        }
    }
}
