/*******************************************************************************
 * 文件名:    DebugRenderer.cs
 * 描述:      调试渲染器，显示Region、Room、寻路等调试信息
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   DebugRenderer 提供：
 *   - Region边界可视化
 *   - Room区域着色
 *   - 寻路路径显示
 *   - 网格线显示
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem.Rendering
{
    /// <summary>
    /// 调试渲染器
    /// </summary>
    public class DebugRenderer : MonoBehaviour
    {
        #region 字段

        /// <summary>
        /// 父渲染器
        /// </summary>
        private MapRenderer _mapRenderer;

        /// <summary>
        /// Region叠加层容器
        /// </summary>
        private Transform _regionOverlayContainer;

        /// <summary>
        /// Room叠加层容器
        /// </summary>
        private Transform _roomOverlayContainer;

        /// <summary>
        /// 路径显示容器
        /// </summary>
        private Transform _pathContainer;

        /// <summary>
        /// 网格线渲染器
        /// </summary>
        private LineRenderer _gridLineRenderer;

        /// <summary>
        /// Region颜色映射
        /// </summary>
        private Dictionary<int, Color> _regionColors;

        /// <summary>
        /// Room颜色映射
        /// </summary>
        private Dictionary<int, Color> _roomColors;

        /// <summary>
        /// 叠加层对象池
        /// </summary>
        private Queue<GameObject> _overlayPool;

        /// <summary>
        /// 活动的叠加层对象
        /// </summary>
        private List<GameObject> _activeOverlays;

        /// <summary>
        /// 路径线渲染器
        /// </summary>
        private LineRenderer _pathLineRenderer;

        #endregion

        #region 属性

        /// <summary>
        /// 当前Site
        /// </summary>
        private Site Site => _mapRenderer?.Site;

        /// <summary>
        /// 当前楼层
        /// </summary>
        private Floor CurrentFloor => _mapRenderer?.CurrentFloor;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(MapRenderer mapRenderer)
        {
            _mapRenderer = mapRenderer;
            _regionColors = new Dictionary<int, Color>();
            _roomColors = new Dictionary<int, Color>();
            _overlayPool = new Queue<GameObject>();
            _activeOverlays = new List<GameObject>();

            // 创建容器
            CreateContainers();
        }

        /// <summary>
        /// 创建容器
        /// </summary>
        private void CreateContainers()
        {
            // Region叠加层容器
            var regionGO = new GameObject("RegionOverlay");
            regionGO.transform.SetParent(transform);
            regionGO.transform.localPosition = new Vector3(0, 0.2f, 0);
            _regionOverlayContainer = regionGO.transform;

            // Room叠加层容器
            var roomGO = new GameObject("RoomOverlay");
            roomGO.transform.SetParent(transform);
            roomGO.transform.localPosition = new Vector3(0, 0.15f, 0);
            _roomOverlayContainer = roomGO.transform;

            // 路径容器
            var pathGO = new GameObject("PathDisplay");
            pathGO.transform.SetParent(transform);
            pathGO.transform.localPosition = new Vector3(0, 0.25f, 0);
            _pathContainer = pathGO.transform;

            // 创建路径线渲染器
            _pathLineRenderer = _pathContainer.gameObject.AddComponent<LineRenderer>();
            _pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _pathLineRenderer.startWidth = 0.1f;
            _pathLineRenderer.endWidth = 0.1f;
            _pathLineRenderer.startColor = Color.green;
            _pathLineRenderer.endColor = Color.yellow;
            _pathLineRenderer.positionCount = 0;
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 刷新调试显示
        /// </summary>
        public void Refresh(bool showRegions, bool showRooms)
        {
            Clear();

            if (Site == null || CurrentFloor == null)
                return;

            if (showRegions)
            {
                DrawRegionOverlay();
            }

            if (showRooms)
            {
                DrawRoomOverlay();
            }
        }

        /// <summary>
        /// 清除所有调试显示
        /// </summary>
        public void Clear()
        {
            // 回收所有叠加层对象
            foreach (var overlay in _activeOverlays)
            {
                overlay.SetActive(false);
                _overlayPool.Enqueue(overlay);
            }
            _activeOverlays.Clear();

            // 清除路径
            _pathLineRenderer.positionCount = 0;
        }

        #endregion

        #region Region显示

        /// <summary>
        /// 绘制Region叠加层
        /// </summary>
        private void DrawRegionOverlay()
        {
            var regionGrid = CurrentFloor?.RegionGrid;
            if (regionGrid == null)
                return;

            var visibleRect = _mapRenderer.VisibleRect;
            float cellSize = Site.CellSize;

            foreach (var cell in visibleRect.GetCells())
            {
                var region = regionGrid.GetRegionAt(cell);
                if (region == null)
                    continue;

                // 获取Region颜色
                var color = GetRegionColor(region);

                // 创建叠加层
                var overlay = GetOverlay();
                overlay.transform.SetParent(_regionOverlayContainer);
                overlay.transform.position = new Vector3(
                    (cell.x + 0.5f) * cellSize,
                    0.2f,
                    (cell.z + 0.5f) * cellSize
                );
                overlay.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1);
                overlay.transform.rotation = Quaternion.Euler(-90, 0, 0);

                var sr = overlay.GetComponent<SpriteRenderer>();
                sr.color = new Color(color.r, color.g, color.b, 0.3f);
            }
        }

        /// <summary>
        /// 获取Region颜色
        /// </summary>
        private Color GetRegionColor(Region region)
        {
            if (_regionColors.TryGetValue(region.RegionId, out var color))
                return color;

            // 根据区域类型生成颜色
            switch (region.Type)
            {
                case RegionType.Normal:
                    color = GetRandomColor(region.RegionId);
                    break;
                case RegionType.Portal:
                    color = Color.yellow;
                    break;
                case RegionType.Impassable:
                    color = Color.red;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            _regionColors[region.RegionId] = color;
            return color;
        }

        #endregion

        #region Room显示

        /// <summary>
        /// 绘制Room叠加层
        /// </summary>
        private void DrawRoomOverlay()
        {
            var roomManager = CurrentFloor?.RoomManager;
            if (roomManager == null)
                return;

            var visibleRect = _mapRenderer.VisibleRect;
            float cellSize = Site.CellSize;

            foreach (var cell in visibleRect.GetCells())
            {
                var room = roomManager.GetRoomAt(cell);
                if (room == null)
                    continue;

                // 获取Room颜色
                var color = GetRoomColor(room);

                // 创建叠加层
                var overlay = GetOverlay();
                overlay.transform.SetParent(_roomOverlayContainer);
                overlay.transform.position = new Vector3(
                    (cell.x + 0.5f) * cellSize,
                    0.15f,
                    (cell.z + 0.5f) * cellSize
                );
                overlay.transform.localScale = new Vector3(cellSize * 0.85f, cellSize * 0.85f, 1);
                overlay.transform.rotation = Quaternion.Euler(-90, 0, 0);

                var sr = overlay.GetComponent<SpriteRenderer>();
                sr.color = new Color(color.r, color.g, color.b, 0.4f);
            }
        }

        /// <summary>
        /// 获取Room颜色
        /// </summary>
        private Color GetRoomColor(Room room)
        {
            if (_roomColors.TryGetValue(room.RoomId, out var color))
                return color;

            // 室外使用特殊颜色
            if (room.IsOutdoors)
            {
                color = new Color(0.5f, 0.8f, 1f);
            }
            else
            {
                // 根据角色分配颜色
                switch (room.Role)
                {
                    case RoomRole.Bedroom:
                        color = new Color(0.8f, 0.6f, 0.8f);
                        break;
                    case RoomRole.Kitchen:
                        color = new Color(1f, 0.8f, 0.5f);
                        break;
                    case RoomRole.DiningRoom:
                        color = new Color(0.9f, 0.7f, 0.5f);
                        break;
                    case RoomRole.Storage:
                        color = new Color(0.6f, 0.5f, 0.4f);
                        break;
                    case RoomRole.Workshop:
                        color = new Color(0.7f, 0.7f, 0.5f);
                        break;
                    case RoomRole.Hospital:
                        color = new Color(0.9f, 0.9f, 0.9f);
                        break;
                    case RoomRole.Prison:
                        color = new Color(0.5f, 0.5f, 0.5f);
                        break;
                    case RoomRole.Research:
                        color = new Color(0.6f, 0.8f, 1f);
                        break;
                    case RoomRole.Hallway:
                        color = new Color(0.7f, 0.7f, 0.7f);
                        break;
                    default:
                        color = GetRandomColor(room.RoomId + 1000);
                        break;
                }
            }

            _roomColors[room.RoomId] = color;
            return color;
        }

        #endregion

        #region 路径显示

        /// <summary>
        /// 显示路径
        /// </summary>
        public void ShowPath(List<CellCoord> path)
        {
            if (path == null || path.Count == 0)
            {
                _pathLineRenderer.positionCount = 0;
                return;
            }

            float cellSize = Site?.CellSize ?? 1f;
            float height = 0.25f;

            _pathLineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var cell = path[i];
                _pathLineRenderer.SetPosition(i, new Vector3(
                    (cell.x + 0.5f) * cellSize,
                    height,
                    (cell.z + 0.5f) * cellSize
                ));
            }
        }

        /// <summary>
        /// 显示全局路径
        /// </summary>
        public void ShowPath(List<GlobalCoord> path)
        {
            if (path == null || path.Count == 0)
            {
                _pathLineRenderer.positionCount = 0;
                return;
            }

            float cellSize = Site?.CellSize ?? 1f;
            float floorHeight = Site?.FloorHeight ?? 3f;

            _pathLineRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var coord = path[i];
                _pathLineRenderer.SetPosition(i, new Vector3(
                    (coord.x + 0.5f) * cellSize,
                    coord.y * floorHeight + 0.25f,
                    (coord.z + 0.5f) * cellSize
                ));
            }
        }

        /// <summary>
        /// 清除路径显示
        /// </summary>
        public void ClearPath()
        {
            _pathLineRenderer.positionCount = 0;
        }

        #endregion

        #region 网格线

        /// <summary>
        /// 显示/隐藏网格线
        /// </summary>
        public void SetGridVisible(bool visible)
        {
            if (_gridLineRenderer != null)
            {
                _gridLineRenderer.enabled = visible;
            }
        }

        /// <summary>
        /// 绘制网格线
        /// </summary>
        public void DrawGrid()
        {
            if (Site == null)
                return;

            // 创建网格线渲染器（如果需要）
            if (_gridLineRenderer == null)
            {
                var gridGO = new GameObject("Grid");
                gridGO.transform.SetParent(transform);
                gridGO.transform.localPosition = new Vector3(0, 0.01f, 0);
                _gridLineRenderer = gridGO.AddComponent<LineRenderer>();
                _gridLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                _gridLineRenderer.startWidth = 0.02f;
                _gridLineRenderer.endWidth = 0.02f;
                _gridLineRenderer.startColor = new Color(1, 1, 1, 0.2f);
                _gridLineRenderer.endColor = new Color(1, 1, 1, 0.2f);
            }

            // 简化：使用GL或Gizmos在编辑器中绘制
            // 实际项目中可能需要更复杂的网格线实现
        }

        #endregion

        #region 标记显示

        /// <summary>
        /// 在指定位置显示标记
        /// </summary>
        public void ShowMarker(CellCoord cell, Color color, float duration = 1f)
        {
            StartCoroutine(ShowMarkerCoroutine(cell, color, duration));
        }

        private System.Collections.IEnumerator ShowMarkerCoroutine(CellCoord cell, Color color, float duration)
        {
            var marker = GetOverlay();
            float cellSize = Site?.CellSize ?? 1f;

            marker.transform.SetParent(transform);
            marker.transform.position = new Vector3(
                (cell.x + 0.5f) * cellSize,
                0.3f,
                (cell.z + 0.5f) * cellSize
            );
            marker.transform.localScale = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 1);
            marker.transform.rotation = Quaternion.Euler(-90, 0, 0);

            var sr = marker.GetComponent<SpriteRenderer>();
            sr.color = color;
            sr.sortingOrder = 1000;

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1 - (elapsed / duration);
                sr.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            marker.SetActive(false);
            _overlayPool.Enqueue(marker);
            _activeOverlays.Remove(marker);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取叠加层对象
        /// </summary>
        private GameObject GetOverlay()
        {
            GameObject overlay;
            if (_overlayPool.Count > 0)
            {
                overlay = _overlayPool.Dequeue();
                overlay.SetActive(true);
            }
            else
            {
                overlay = CreateOverlay();
            }

            _activeOverlays.Add(overlay);
            return overlay;
        }

        /// <summary>
        /// 创建叠加层对象
        /// </summary>
        private GameObject CreateOverlay()
        {
            var go = new GameObject("Overlay");
            var sr = go.AddComponent<SpriteRenderer>();

            // 创建简单的方形精灵
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

            return go;
        }

        /// <summary>
        /// 生成随机颜色（基于ID）
        /// </summary>
        private Color GetRandomColor(int id)
        {
            // 使用ID生成确定性的随机颜色
            UnityEngine.Random.InitState(id * 12345);
            return new Color(
                UnityEngine.Random.Range(0.3f, 1f),
                UnityEngine.Random.Range(0.3f, 1f),
                UnityEngine.Random.Range(0.3f, 1f)
            );
        }

        #endregion

        #region Gizmos（编辑器调试）

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || Site == null)
                return;

            // 可以在这里添加Gizmos绘制
        }

        #endregion
    }
}
