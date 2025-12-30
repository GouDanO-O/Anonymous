/*******************************************************************************
 * 文件名:    MapRenderer.cs
 * 描述:      地图渲染管理器，协调所有渲染组件
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   MapRenderer 是渲染系统的入口，负责：
 *   - 管理TileRenderer和EntityRenderer
 *   - 处理楼层切换
 *   - 视口裁剪优化
 *   - 渲染更新调度
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// 地图渲染管理器
    /// </summary>
    public class MapRenderer : MonoBehaviour
    {
        #region 序列化字段

        [Header("引用")]
        [SerializeField]
        private Camera _mainCamera;

        [Header("渲染设置")]
        [SerializeField]
        private int _viewPadding = 5;

        [SerializeField]
        private bool _renderAllFloors = false;

        [SerializeField]
        private float _floorFadeAlpha = 0.3f;

        [Header("调试")]
        [SerializeField]
        private bool _showDebugInfo = false;

        [SerializeField]
        private bool _showRegionOverlay = false;

        [SerializeField]
        private bool _showRoomOverlay = false;

        #endregion

        #region 字段

        /// <summary>
        /// 当前Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 当前显示的楼层
        /// </summary>
        private int _currentFloorIndex;

        /// <summary>
        /// Tile渲染器
        /// </summary>
        private TileRenderer _tileRenderer;

        /// <summary>
        /// 实体渲染器
        /// </summary>
        private EntityRenderer _entityRenderer;

        /// <summary>
        /// 调试渲染器
        /// </summary>
        private DebugRenderer _debugRenderer;

        /// <summary>
        /// 可见区域
        /// </summary>
        private CellRect _visibleRect;

        /// <summary>
        /// 是否需要刷新
        /// </summary>
        private bool _needsRefresh = true;

        #endregion

        #region 属性

        /// <summary>
        /// 当前Site
        /// </summary>
        public Site Site => _site;

        /// <summary>
        /// 当前楼层索引
        /// </summary>
        public int CurrentFloorIndex
        {
            get => _currentFloorIndex;
            set => SetCurrentFloor(value);
        }

        /// <summary>
        /// 当前楼层
        /// </summary>
        public Floor CurrentFloor => _site?.GetFloor(_currentFloorIndex);

        /// <summary>
        /// Tile渲染器
        /// </summary>
        public TileRenderer TileRenderer => _tileRenderer;

        /// <summary>
        /// 实体渲染器
        /// </summary>
        public EntityRenderer EntityRenderer => _entityRenderer;

        /// <summary>
        /// 可见区域
        /// </summary>
        public CellRect VisibleRect => _visibleRect;

        /// <summary>
        /// 显示区域叠加
        /// </summary>
        public bool ShowRegionOverlay
        {
            get => _showRegionOverlay;
            set
            {
                _showRegionOverlay = value;
                _needsRefresh = true;
            }
        }

        /// <summary>
        /// 显示房间叠加
        /// </summary>
        public bool ShowRoomOverlay
        {
            get => _showRoomOverlay;
            set
            {
                _showRoomOverlay = value;
                _needsRefresh = true;
            }
        }

        #endregion

        #region 单例

        private static MapRenderer _instance;
        public static MapRenderer Instance => _instance;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            _instance = this;

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            // 创建子渲染器
            CreateRenderers();
        }

        private void Start()
        {
            // 初始化可见区域
            UpdateVisibleRect();
        }

        private void Update()
        {
            if (_site == null)
                return;

            // 更新可见区域
            UpdateVisibleRect();

            // 更新渲染
            if (_needsRefresh)
            {
                RefreshAll();
                _needsRefresh = false;
            }
        }

        private void LateUpdate()
        {
            // 更新实体渲染位置
            _entityRenderer?.UpdateEntityPositions();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 创建子渲染器
        /// </summary>
        private void CreateRenderers()
        {
            // Tile渲染器
            var tileRendererGO = new GameObject("TileRenderer");
            tileRendererGO.transform.SetParent(transform);
            _tileRenderer = tileRendererGO.AddComponent<TileRenderer>();
            _tileRenderer.Initialize(this);

            // 实体渲染器
            var entityRendererGO = new GameObject("EntityRenderer");
            entityRendererGO.transform.SetParent(transform);
            _entityRenderer = entityRendererGO.AddComponent<EntityRenderer>();
            _entityRenderer.Initialize(this);

            // 调试渲染器
            var debugRendererGO = new GameObject("DebugRenderer");
            debugRendererGO.transform.SetParent(transform);
            _debugRenderer = debugRendererGO.AddComponent<DebugRenderer>();
            _debugRenderer.Initialize(this);
        }

        /// <summary>
        /// 设置要渲染的Site
        /// </summary>
        public void SetSite(Site site)
        {
            _site = site;
            _currentFloorIndex = 0;

            // 初始化子渲染器
            _tileRenderer?.SetSite(site);
            _entityRenderer?.SetSite(site);

            // 强制触发楼层变更
            _tileRenderer?.OnFloorChanged(_currentFloorIndex);
            _entityRenderer?.OnFloorChanged(_currentFloorIndex);

            // 强制更新可见区域
            ForceUpdateVisibleRect();

            _needsRefresh = true;
            
            Debug.Log($"[MapRenderer] SetSite: {site?.Config?.SiteName}, Floor={_currentFloorIndex}");
        }

        /// <summary>
        /// 强制更新可见区域（不检查变化）
        /// </summary>
        private void ForceUpdateVisibleRect()
        {
            if (_mainCamera == null || _site == null)
                return;

            float cellSize = _site.CellSize;

            // 使用正交相机的正确计算方式
            float halfHeight = _mainCamera.orthographicSize;
            float halfWidth = halfHeight * _mainCamera.aspect;

            Vector3 camPos = _mainCamera.transform.position;

            int minX = Mathf.FloorToInt((camPos.x - halfWidth) / cellSize) - _viewPadding;
            int maxX = Mathf.CeilToInt((camPos.x + halfWidth) / cellSize) + _viewPadding;
            int minZ = Mathf.FloorToInt((camPos.z - halfHeight) / cellSize) - _viewPadding;
            int maxZ = Mathf.CeilToInt((camPos.z + halfHeight) / cellSize) + _viewPadding;

            // 限制在地图范围内
            minX = Mathf.Max(0, minX);
            minZ = Mathf.Max(0, minZ);
            maxX = Mathf.Min(_site.SizeX - 1, maxX);
            maxZ = Mathf.Min(_site.SizeZ - 1, maxZ);

            _visibleRect = new CellRect(minX, minZ, maxX, maxZ);
            
            Debug.Log($"[MapRenderer] VisibleRect: ({minX},{minZ}) to ({maxX},{maxZ})");

            // 通知子渲染器
            _tileRenderer?.OnVisibleRectChanged(_visibleRect);
            _entityRenderer?.OnVisibleRectChanged(_visibleRect);
        }

        #endregion

        #region 楼层切换

        /// <summary>
        /// 设置当前楼层
        /// </summary>
        public void SetCurrentFloor(int floorIndex)
        {
            if (_site == null)
                return;

            floorIndex = Mathf.Clamp(floorIndex, _site.MinFloor, _site.MaxFloor);

            if (_currentFloorIndex != floorIndex)
            {
                _currentFloorIndex = floorIndex;
                OnFloorChanged();
            }
        }

        /// <summary>
        /// 上一层
        /// </summary>
        public void GoUpFloor()
        {
            SetCurrentFloor(_currentFloorIndex + 1);
        }

        /// <summary>
        /// 下一层
        /// </summary>
        public void GoDownFloor()
        {
            SetCurrentFloor(_currentFloorIndex - 1);
        }

        /// <summary>
        /// 楼层变更回调
        /// </summary>
        private void OnFloorChanged()
        {
            _tileRenderer?.OnFloorChanged(_currentFloorIndex);
            _entityRenderer?.OnFloorChanged(_currentFloorIndex);
            _needsRefresh = true;
        }

        #endregion

        #region 可见区域

        /// <summary>
        /// 更新可见区域
        /// </summary>
        private void UpdateVisibleRect()
        {
            if (_mainCamera == null || _site == null)
                return;

            float cellSize = _site.CellSize;

            // 使用正交相机的正确计算方式
            float halfHeight = _mainCamera.orthographicSize;
            float halfWidth = halfHeight * _mainCamera.aspect;

            Vector3 camPos = _mainCamera.transform.position;

            int minX = Mathf.FloorToInt((camPos.x - halfWidth) / cellSize) - _viewPadding;
            int maxX = Mathf.CeilToInt((camPos.x + halfWidth) / cellSize) + _viewPadding;
            int minZ = Mathf.FloorToInt((camPos.z - halfHeight) / cellSize) - _viewPadding;
            int maxZ = Mathf.CeilToInt((camPos.z + halfHeight) / cellSize) + _viewPadding;

            // 限制在地图范围内
            minX = Mathf.Max(0, minX);
            minZ = Mathf.Max(0, minZ);
            maxX = Mathf.Min(_site.SizeX - 1, maxX);
            maxZ = Mathf.Min(_site.SizeZ - 1, maxZ);

            var newRect = new CellRect(minX, minZ, maxX, maxZ);

            if (!_visibleRect.Equals(newRect))
            {
                _visibleRect = newRect;
                OnVisibleRectChanged();
            }
        }

        /// <summary>
        /// 可见区域变更回调
        /// </summary>
        private void OnVisibleRectChanged()
        {
            _tileRenderer?.OnVisibleRectChanged(_visibleRect);
            _entityRenderer?.OnVisibleRectChanged(_visibleRect);
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 标记需要刷新
        /// </summary>
        public void MarkNeedsRefresh()
        {
            _needsRefresh = true;
        }

        /// <summary>
        /// 刷新所有渲染
        /// </summary>
        public void RefreshAll()
        {
            _tileRenderer?.Refresh();
            _entityRenderer?.Refresh();

            if (_showDebugInfo || _showRegionOverlay || _showRoomOverlay)
            {
                _debugRenderer?.Refresh(_showRegionOverlay, _showRoomOverlay);
            }
            else
            {
                _debugRenderer?.Clear();
            }
        }

        /// <summary>
        /// 刷新指定格子
        /// </summary>
        public void RefreshCell(CellCoord cell)
        {
            _tileRenderer?.RefreshCell(cell);
        }

        /// <summary>
        /// 刷新指定区域
        /// </summary>
        public void RefreshRect(CellRect rect)
        {
            _tileRenderer?.RefreshRect(rect);
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 屏幕坐标转格子坐标
        /// </summary>
        public CellCoord ScreenToCell(Vector2 screenPos)
        {
            if (_mainCamera == null || _site == null)
                return CellCoord.Invalid;

            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _mainCamera.transform.position.y));
            return WorldToCell(worldPos);
        }

        /// <summary>
        /// 世界坐标转格子坐标
        /// </summary>
        public CellCoord WorldToCell(Vector3 worldPos)
        {
            if (_site == null)
                return CellCoord.Invalid;

            float cellSize = _site.CellSize;
            int x = Mathf.FloorToInt(worldPos.x / cellSize);
            int z = Mathf.FloorToInt(worldPos.z / cellSize);

            if (x < 0 || x >= _site.SizeX || z < 0 || z >= _site.SizeZ)
                return CellCoord.Invalid;

            return new CellCoord(x, z);
        }

        /// <summary>
        /// 格子坐标转世界坐标（中心点）
        /// </summary>
        public Vector3 CellToWorld(CellCoord cell)
        {
            if (_site == null)
                return Vector3.zero;

            float cellSize = _site.CellSize;
            float floorHeight = _site.FloorHeight;

            return new Vector3(
                (cell.x + 0.5f) * cellSize,
                _currentFloorIndex * floorHeight,
                (cell.z + 0.5f) * cellSize
            );
        }

        #endregion

        #region 调试UI

        private void OnGUI()
        {
            if (!_showDebugInfo || _site == null)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"Site: {_site.SiteName}");
            GUILayout.Label($"Size: {_site.SizeX} x {_site.SizeZ}");
            GUILayout.Label($"Floor: {_currentFloorIndex} ({_site.MinFloor} ~ {_site.MaxFloor})");
            GUILayout.Label($"Visible: {_visibleRect}");
            GUILayout.Label($"Entities: {_site.EntityManager?.TotalEntityCount ?? 0}");

            var floor = CurrentFloor;
            if (floor != null)
            {
                GUILayout.Label($"Regions: {floor.RegionGrid?.RegionCount ?? 0}");
                GUILayout.Label($"Rooms: {floor.RoomManager?.RoomCount ?? 0}");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// 格子矩形区域
    /// </summary>
    [Serializable]
    public struct CellRect : IEquatable<CellRect>
    {
        public int minX;
        public int minZ;
        public int maxX;
        public int maxZ;

        public CellRect(int minX, int minZ, int maxX, int maxZ)
        {
            this.minX = minX;
            this.minZ = minZ;
            this.maxX = maxX;
            this.maxZ = maxZ;
        }

        public CellRect(CellCoord min, CellCoord max)
        {
            this.minX = min.x;
            this.minZ = min.z;
            this.maxX = max.x;
            this.maxZ = max.z;
        }

        public int Width => maxX - minX + 1;
        public int Height => maxZ - minZ + 1;
        public int CellCount => Width * Height;

        public CellCoord Min => new CellCoord(minX, minZ);
        public CellCoord Max => new CellCoord(maxX, maxZ);
        public CellCoord Center => new CellCoord((minX + maxX) / 2, (minZ + maxZ) / 2);

        public bool Contains(CellCoord cell)
        {
            return cell.x >= minX && cell.x <= maxX && 
                   cell.z >= minZ && cell.z <= maxZ;
        }

        public bool Equals(CellRect other)
        {
            return minX == other.minX && minZ == other.minZ && 
                   maxX == other.maxX && maxZ == other.maxZ;
        }

        public override bool Equals(object obj)
        {
            return obj is CellRect other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(minX, minZ, maxX, maxZ);
        }

        public override string ToString()
        {
            return $"[({minX},{minZ})-({maxX},{maxZ})]";
        }

        public IEnumerable<CellCoord> GetCells()
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    yield return new CellCoord(x, z);
                }
            }
        }
    }
}
