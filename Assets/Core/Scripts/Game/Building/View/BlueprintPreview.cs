using Core.Game.Building.Define;
using Core.Game.Building.System;
using Core.Game.Map.Define;
using Core.Game.Map.Utility;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Building.View
{
    /// <summary>
    /// 蓝图预览视图
    /// 使用临时Mesh渲染半透明预览，跟随鼠标位置
    /// </summary>
    public class BlueprintPreview : MonoBehaviour
    {
        #region 组件

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _previewMesh;
        private Material _previewMaterial;

        #endregion

        #region 系统引用

        private BlueprintPlaceSystem _placeSystem;

        #endregion

        #region 状态

        private int _lastPreviewX = int.MinValue;
        private int _lastPreviewY = int.MinValue;
        private bool _lastValid;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 创建渲染组件
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // 创建半透明材质
            _previewMaterial = new Material(Shader.Find("Sprites/Default"));
            _previewMaterial.renderQueue = 3100; // 在其他物体之上
            _meshRenderer.material = _previewMaterial;

            _previewMesh = new Mesh();
            _previewMesh.name = "BlueprintPreview";
            _meshFilter.mesh = _previewMesh;

            gameObject.SetActive(false);
        }

        private void Start()
        {
            _placeSystem = GameMain.Interface.GetSystem<BlueprintPlaceSystem>();

            // 订阅事件
            _placeSystem.OnEnterPlaceMode += OnEnterPlaceMode;
            _placeSystem.OnExitPlaceMode += OnExitPlaceMode;
            _placeSystem.OnPreviewUpdated += OnPreviewUpdated;
        }

        private void OnDestroy()
        {
            if (_placeSystem != null)
            {
                _placeSystem.OnEnterPlaceMode -= OnEnterPlaceMode;
                _placeSystem.OnExitPlaceMode -= OnExitPlaceMode;
                _placeSystem.OnPreviewUpdated -= OnPreviewUpdated;
            }

            if (_previewMesh != null)
                Destroy(_previewMesh);
            if (_previewMaterial != null)
                Destroy(_previewMaterial);
        }

        #endregion

        #region 事件处理

        private void OnEnterPlaceMode(string defName, EBuildingCategory category)
        {
            gameObject.SetActive(true);
            _lastPreviewX = int.MinValue;
            _lastPreviewY = int.MinValue;
        }

        private void OnExitPlaceMode()
        {
            gameObject.SetActive(false);
            _previewMesh.Clear();
        }

        private void OnPreviewUpdated(int cellX, int cellY, int floor, bool isValid)
        {
            // 避免重复构建同一位置的Mesh
            if (cellX == _lastPreviewX && cellY == _lastPreviewY && isValid == _lastValid)
                return;

            _lastPreviewX = cellX;
            _lastPreviewY = cellY;
            _lastValid = isValid;

            BuildPreviewMesh(cellX, cellY, floor, isValid);
        }

        #endregion

        #region Mesh构建

        /// <summary>
        /// 构建预览Mesh
        /// </summary>
        private void BuildPreviewMesh(int cellX, int cellY, int floor, bool isValid)
        {
            _previewMesh.Clear();

            Color previewColor = isValid
                ? BuildingDefine.PreviewColorValid
                : BuildingDefine.PreviewColorInvalid;

            int width = _placeSystem.CurrentWidth;
            int height = _placeSystem.CurrentHeight;
            var category = _placeSystem.CurrentCategory;

            if (category == EBuildingCategory.Wall || category == EBuildingCategory.Door)
            {
                BuildWallPreviewMesh(cellX, cellY, floor, previewColor);
            }
            else
            {
                BuildTilePreviewMesh(cellX, cellY, floor, width, height, previewColor);
            }
        }

        /// <summary>
        /// 构建地面/物体预览（等轴测菱形）
        /// </summary>
        private void BuildTilePreviewMesh(int cellX, int cellY, int floor,
            int width, int height, Color color)
        {
            int quadCount = width * height;
            var vertices = new Vector3[quadCount * 4];
            var triangles = new int[quadCount * 6];
            var colors = new Color[quadCount * 4];

            int vi = 0;
            int ti = 0;

            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    int x = cellX + dx;
                    int y = cellY + dy;

                    // 等轴测菱形的4个顶点
                    Vector3 left = IsometricUtility.CellToScreen(x, y, floor);
                    Vector3 top = IsometricUtility.CellToScreen(x, y + 1, floor);
                    Vector3 right = IsometricUtility.CellToScreen(x + 1, y + 1, floor);
                    Vector3 bottom = IsometricUtility.CellToScreen(x + 1, y, floor);

                    float depth = IsometricUtility.CalculateDepth(x, y, floor, MapDefine.DepthOffsetObject - 0.05f);

                    vertices[vi] = new Vector3(left.x, left.y, depth);
                    vertices[vi + 1] = new Vector3(top.x, top.y, depth);
                    vertices[vi + 2] = new Vector3(right.x, right.y, depth);
                    vertices[vi + 3] = new Vector3(bottom.x, bottom.y, depth);

                    colors[vi] = color;
                    colors[vi + 1] = color;
                    colors[vi + 2] = color;
                    colors[vi + 3] = color;

                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + 1;
                    triangles[ti + 2] = vi + 2;
                    triangles[ti + 3] = vi;
                    triangles[ti + 4] = vi + 2;
                    triangles[ti + 5] = vi + 3;

                    vi += 4;
                    ti += 6;
                }
            }

            _previewMesh.vertices = vertices;
            _previewMesh.triangles = triangles;
            _previewMesh.colors = colors;
        }

        /// <summary>
        /// 构建墙体预览
        /// </summary>
        private void BuildWallPreviewMesh(int cellX, int cellY, int floor, Color color)
        {
            var vertices = new Vector3[4];
            var triangles = new int[6];
            var colors = new Color[4];

            var direction = _placeSystem.CurrentWallDirection;
            float depth = IsometricUtility.CalculateDepth(cellX, cellY, floor, MapDefine.DepthOffsetWall - 0.05f);

            Vector3 baseLeft, baseRight;

            if (direction == EWallDirection.North)
            {
                // 北墙：连接left和top
                baseLeft = IsometricUtility.CellToScreen(cellX, cellY + 1, floor);
                baseRight = IsometricUtility.CellToScreen(cellX + 1, cellY + 1, floor);
            }
            else
            {
                // 西墙：连接bottom和left
                baseLeft = IsometricUtility.CellToScreen(cellX, cellY, floor);
                baseRight = IsometricUtility.CellToScreen(cellX, cellY + 1, floor);
            }

            float wallHeight = MapDefine.WallWorldHeight;

            vertices[0] = new Vector3(baseLeft.x, baseLeft.y, depth);
            vertices[1] = new Vector3(baseLeft.x, baseLeft.y + wallHeight, depth);
            vertices[2] = new Vector3(baseRight.x, baseRight.y + wallHeight, depth);
            vertices[3] = new Vector3(baseRight.x, baseRight.y, depth);

            colors[0] = color;
            colors[1] = color;
            colors[2] = color;
            colors[3] = color;

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;

            _previewMesh.vertices = vertices;
            _previewMesh.triangles = triangles;
            _previewMesh.colors = colors;
        }

        #endregion
    }
}
