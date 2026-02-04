using System.Collections.Generic;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Map.Utility;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Map.System
{
    /// <summary>
    /// Chunk视野裁剪系统
    /// 只渲染摄像机可见范围内的Chunk
    /// </summary>
    public class ChunkCullingSystem : AbstractSystem
    {
        #region 配置

        /// <summary>
        /// 可见区域外扩边距（Chunk数）
        /// </summary>
        public int CullingMargin = 1;

        /// <summary>
        /// 更新间隔（秒）
        /// </summary>
        public float UpdateInterval = 0.1f;

        #endregion

        #region 数据

        private MapDataModel _mapDataModel;
        private Camera _mainCamera;

        private HashSet<Vector3Int> _visibleChunks = new HashSet<Vector3Int>();
        private HashSet<Vector3Int> _prevVisibleChunks = new HashSet<Vector3Int>();
        private float _lastUpdateTime;

        // 缓存列表，避免每帧分配
        private List<Vector3Int> _chunksToShow = new List<Vector3Int>();
        private List<Vector3Int> _chunksToHide = new List<Vector3Int>();

        #endregion

        #region 事件

        /// <summary>
        /// Chunk变为可见
        /// </summary>
        public UnityAction<List<Vector3Int>> OnChunksVisible;

        /// <summary>
        /// Chunk变为不可见
        /// </summary>
        public UnityAction<List<Vector3Int>> OnChunksHidden;

        #endregion

        #region 属性

        /// <summary>
        /// 当前可见的Chunk集合
        /// </summary>
        public IReadOnlyCollection<Vector3Int> VisibleChunks => _visibleChunks;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置主摄像机
        /// </summary>
        public void SetMainCamera(Camera camera)
        {
            _mainCamera = camera;
        }

        /// <summary>
        /// 更新可见性（需要每帧或定时调用）
        /// </summary>
        public void UpdateVisibility()
        {
            if (_mainCamera == null || !_mapDataModel.IsMapLoaded)
                return;

            // 节流
            if (Time.time - _lastUpdateTime < UpdateInterval)
                return;

            _lastUpdateTime = Time.time;

            // 计算摄像机可见区域
            var visibleRange = CalculateVisibleChunkRange();

            // 交换集合
            (_prevVisibleChunks, _visibleChunks) = (_visibleChunks, _prevVisibleChunks);
            _visibleChunks.Clear();

            // 填充新的可见Chunk
            int currentFloor = _mapDataModel.CurrentFloor;

            for (int cy = visibleRange.minY; cy <= visibleRange.maxY; cy++)
            {
                for (int cx = visibleRange.minX; cx <= visibleRange.maxX; cx++)
                {
                    // 当前楼层及以下都需要渲染（用于遮挡效果）
                    for (int f = 0; f <= currentFloor; f++)
                    {
                        if (_mapDataModel.CurrentMap.IsValidChunkPos(cx, cy, f))
                        {
                            _visibleChunks.Add(new Vector3Int(cx, cy, f));
                        }
                    }
                }
            }

            // 计算变化
            NotifyVisibilityChanges();
        }

        /// <summary>
        /// 强制刷新（立即更新，忽略节流）
        /// </summary>
        public void ForceRefresh()
        {
            _lastUpdateTime = 0;
            UpdateVisibility();
        }

        /// <summary>
        /// 检查Chunk是否可见
        /// </summary>
        public bool IsChunkVisible(int chunkX, int chunkY, int floor)
        {
            return _visibleChunks.Contains(new Vector3Int(chunkX, chunkY, floor));
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 计算摄像机可见的Chunk范围
        /// </summary>
        private (int minX, int minY, int maxX, int maxY) CalculateVisibleChunkRange()
        {
            if (_mainCamera == null)
                return (0, 0, 0, 0);

            // 获取摄像机可见区域的世界坐标边界
            float camHeight = _mainCamera.orthographicSize * 2f;
            float camWidth = camHeight * _mainCamera.aspect;

            Vector3 camPos = _mainCamera.transform.position;

            // 计算摄像机四个角的世界坐标
            float left = camPos.x - camWidth / 2f;
            float right = camPos.x + camWidth / 2f;
            float bottom = camPos.y - camHeight / 2f;
            float top = camPos.y + camHeight / 2f;

            // 将四个角转换为格子坐标
            int currentFloor = _mapDataModel.CurrentFloor;

            var cellBL = IsometricUtility.ScreenToCellInt(new Vector3(left, bottom, 0), currentFloor);
            var cellBR = IsometricUtility.ScreenToCellInt(new Vector3(right, bottom, 0), currentFloor);
            var cellTL = IsometricUtility.ScreenToCellInt(new Vector3(left, top, 0), currentFloor);
            var cellTR = IsometricUtility.ScreenToCellInt(new Vector3(right, top, 0), currentFloor);

            // 计算格子坐标范围
            int cellMinX = Mathf.Min(cellBL.x, cellBR.x, cellTL.x, cellTR.x);
            int cellMaxX = Mathf.Max(cellBL.x, cellBR.x, cellTL.x, cellTR.x);
            int cellMinY = Mathf.Min(cellBL.y, cellBR.y, cellTL.y, cellTR.y);
            int cellMaxY = Mathf.Max(cellBL.y, cellBR.y, cellTL.y, cellTR.y);

            // 转换为Chunk坐标
            int chunkMinX = cellMinX / MapDefine.ChunkSize - CullingMargin;
            int chunkMaxX = cellMaxX / MapDefine.ChunkSize + CullingMargin;
            int chunkMinY = cellMinY / MapDefine.ChunkSize - CullingMargin;
            int chunkMaxY = cellMaxY / MapDefine.ChunkSize + CullingMargin;

            // 限制在有效范围内
            chunkMinX = Mathf.Max(0, chunkMinX);
            chunkMinY = Mathf.Max(0, chunkMinY);
            chunkMaxX = Mathf.Min(_mapDataModel.ChunkCountX - 1, chunkMaxX);
            chunkMaxY = Mathf.Min(_mapDataModel.ChunkCountY - 1, chunkMaxY);

            return (chunkMinX, chunkMinY, chunkMaxX, chunkMaxY);
        }

        /// <summary>
        /// 通知可见性变化
        /// </summary>
        private void NotifyVisibilityChanges()
        {
            _chunksToShow.Clear();
            _chunksToHide.Clear();

            // 找出新可见的Chunk（在新集合中但不在旧集合中）
            foreach (var chunk in _visibleChunks)
            {
                if (!_prevVisibleChunks.Contains(chunk))
                {
                    _chunksToShow.Add(chunk);
                }
            }

            // 找出隐藏的Chunk（在旧集合中但不在新集合中）
            foreach (var chunk in _prevVisibleChunks)
            {
                if (!_visibleChunks.Contains(chunk))
                {
                    _chunksToHide.Add(chunk);
                }
            }

            // 触发事件
            if (_chunksToShow.Count > 0)
            {
                OnChunksVisible?.Invoke(_chunksToShow);
            }

            if (_chunksToHide.Count > 0)
            {
                OnChunksHidden?.Invoke(_chunksToHide);
            }
        }

        #endregion
    }
}
