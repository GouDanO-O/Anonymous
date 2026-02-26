using System;
using System.Collections.Generic;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Map.System
{
    /// <summary>
    /// Chunk相机裁剪系统
    /// 根据相机视口确定哪些Chunk可见
    /// </summary>
    public class ChunkCullingSystem : AbstractSystem
    {
        private Camera _mainCamera;
        private MapDataModel _mapDataModel;

        private HashSet<Vector3Int> _visibleChunks = new HashSet<Vector3Int>();
        private HashSet<Vector3Int> _prevVisibleChunks = new HashSet<Vector3Int>();

        /// <summary>
        /// Chunk变为可见时触发
        /// </summary>
        public event Action<List<Vector3Int>> OnChunksVisible;

        /// <summary>
        /// Chunk变为不可见时触发
        /// </summary>
        public event Action<List<Vector3Int>> OnChunksHidden;

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
        }

        public void SetMainCamera(Camera camera)
        {
            _mainCamera = camera;
        }

        /// <summary>
        /// 每帧更新可见性
        /// </summary>
        public void UpdateVisibility()
        {
            if (_mainCamera == null || !_mapDataModel.IsMapLoaded) return;

            var map = _mapDataModel.CurrentMap;
            int currentFloor = _mapDataModel.CurrentFloor;

            // 正交相机: 计算视口的tile范围
            var camPos = _mainCamera.transform.position;
            float halfH = _mainCamera.orthographicSize;
            float halfW = halfH * _mainCamera.aspect;

            // 额外扩展1个chunk的边距，确保滚动平滑
            int margin = MapConst.ChunkSize;
            int minTileX = Mathf.FloorToInt(camPos.x - halfW) - margin;
            int maxTileX = Mathf.CeilToInt(camPos.x + halfW) + margin;
            int minTileY = Mathf.FloorToInt(camPos.y - halfH) - margin;
            int maxTileY = Mathf.CeilToInt(camPos.y + halfH) + margin;

            int minCX = Mathf.Max(0, minTileX / MapConst.ChunkSize);
            int maxCX = Mathf.Min(map.ChunkCountX - 1, maxTileX / MapConst.ChunkSize);
            int minCY = Mathf.Max(0, minTileY / MapConst.ChunkSize);
            int maxCY = Mathf.Min(map.ChunkCountY - 1, maxTileY / MapConst.ChunkSize);

            _visibleChunks.Clear();

            // 当前楼层及以下的所有楼层
            for (int f = 0; f <= currentFloor; f++)
            {
                for (int cy = minCY; cy <= maxCY; cy++)
                {
                    for (int cx = minCX; cx <= maxCX; cx++)
                    {
                        _visibleChunks.Add(new Vector3Int(cx, cy, f));
                    }
                }
            }

            // 计算差异
            var newlyVisible = new List<Vector3Int>();
            var newlyHidden = new List<Vector3Int>();

            foreach (var c in _visibleChunks)
            {
                if (!_prevVisibleChunks.Contains(c))
                    newlyVisible.Add(c);
            }

            foreach (var c in _prevVisibleChunks)
            {
                if (!_visibleChunks.Contains(c))
                    newlyHidden.Add(c);
            }

            if (newlyVisible.Count > 0)
                OnChunksVisible?.Invoke(newlyVisible);

            if (newlyHidden.Count > 0)
                OnChunksHidden?.Invoke(newlyHidden);

            // 交换引用以避免每帧分配
            (_prevVisibleChunks, _visibleChunks) = (_visibleChunks, _prevVisibleChunks);
        }

        /// <summary>
        /// 强制刷新所有可见性 (楼层切换时)
        /// </summary>
        public void ForceRefresh()
        {
            _prevVisibleChunks.Clear();
        }

        /// <summary>
        /// 当前是否有可见Chunk
        /// </summary>
        public bool HasVisibleChunks => _prevVisibleChunks.Count > 0;
    }
}
