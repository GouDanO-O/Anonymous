using System;
using System.Collections.Generic;
using Core.Game.Map.Model;
using Core.Game.Navigation.Data;
using Core.Game.Navigation.Define;
using Core.Game.Navigation.Utility;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Navigation.System
{
    /// <summary>
    /// 寻路主系统
    /// 管理寻路请求队列，每帧分片处理请求
    /// </summary>
    public class NavigationSystem : AbstractSystem
    {
        #region 子系统引用

        private MapDataModel _mapDataModel;
        private PathfinderCore _pathfinder;

        #endregion

        #region 请求队列

        private readonly Queue<PathRequest> _requestQueue = new Queue<PathRequest>();

        #endregion

        #region 统计

        /// <summary>
        /// 当前队列中的请求数
        /// </summary>
        public int PendingRequestCount => _requestQueue.Count;

        /// <summary>
        /// 本帧处理的请求数
        /// </summary>
        public int ProcessedThisFrame { get; private set; }

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _pathfinder = new PathfinderCore();
        }

        protected override void OnDeinit()
        {
            _requestQueue.Clear();
        }

        #endregion

        #region 公共方法 - 请求寻路

        /// <summary>
        /// 提交寻路请求（异步，通过回调返回结果）
        /// </summary>
        public void RequestPath(int startX, int startY, int startFloor,
            int endX, int endY, int endFloor,
            Action<PathResult> callback, int requesterId = -1)
        {
            var request = new PathRequest(startX, startY, startFloor,
                endX, endY, endFloor, callback, requesterId);
            _requestQueue.Enqueue(request);
        }

        /// <summary>
        /// 提交寻路请求
        /// </summary>
        public void RequestPath(PathRequest request)
        {
            _requestQueue.Enqueue(request);
        }

        /// <summary>
        /// 同步寻路（立即返回结果，适用于紧急情况）
        /// </summary>
        public PathResult FindPathImmediate(int startX, int startY, int startFloor,
            int endX, int endY, int endFloor, int requesterId = -1)
        {
            if (!_mapDataModel.IsMapLoaded)
                return PathResult.Fail(EPathStatus.Failed, 0, requesterId);

            var request = new PathRequest(startX, startY, startFloor,
                endX, endY, endFloor, null, requesterId);
            return _pathfinder.FindPath(_mapDataModel.CurrentMap, request);
        }

        /// <summary>
        /// 取消指定请求者的所有待处理请求
        /// </summary>
        public void CancelRequests(int requesterId)
        {
            if (_requestQueue.Count == 0)
                return;

            int count = _requestQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var request = _requestQueue.Dequeue();
                if (request.RequesterId != requesterId)
                {
                    _requestQueue.Enqueue(request);
                }
            }
        }

        /// <summary>
        /// 清除所有待处理请求
        /// </summary>
        public void ClearAllRequests()
        {
            _requestQueue.Clear();
        }

        #endregion

        #region 公共方法 - 更新

        /// <summary>
        /// 每帧调用，处理请求队列
        /// </summary>
        public void UpdateNavigation()
        {
            if (!_mapDataModel.IsMapLoaded)
                return;

            ProcessedThisFrame = 0;
            var mapData = _mapDataModel.CurrentMap;

            int maxProcess = Mathf.Min(_requestQueue.Count, NavigationDefine.MaxRequestsPerFrame);

            for (int i = 0; i < maxProcess; i++)
            {
                var request = _requestQueue.Dequeue();
                var result = _pathfinder.FindPath(mapData, request);

                ProcessedThisFrame++;

                // 通过回调返回结果
                try
                {
                    request.Callback?.Invoke(result);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NavigationSystem] Callback error for requester {request.RequesterId}: {e}");
                }
            }
        }

        #endregion

        #region 公共方法 - 查询

        /// <summary>
        /// 检查目标点是否可达（快速检查，不做完整寻路）
        /// </summary>
        public bool IsCellReachable(int x, int y, int floor)
        {
            if (!_mapDataModel.IsMapLoaded)
                return false;

            return NavigationUtility.IsCellPassable(_mapDataModel.CurrentMap, x, y, floor);
        }

        #endregion
    }
}
