using System.Collections.Generic;
using Core.Game.Map.Data;
using Core.Game.Map.Utility;
using Core.Game.Navigation.Data;
using Core.Game.Navigation.Define;
using Core.Game.Navigation.Utility;
using GDFrameworkExtend.PoolKit;
using UnityEngine;

namespace Core.Game.Navigation.System
{
    /// <summary>
    /// A*寻路算法核心
    /// 纯逻辑实现，无框架依赖，支持8方向移动和跨楼层寻路
    /// </summary>
    public class PathfinderCore
    {
        #region 对象池

        private readonly SafeObjectPool<PathNode> _nodePool;

        #endregion

        #region 工作数据

        /// <summary>
        /// 开放列表（待探索节点）使用最小堆实现
        /// </summary>
        private readonly List<PathNode> _openList;

        /// <summary>
        /// 所有已创建节点的字典（key -> node，用于快速查找）
        /// </summary>
        private readonly Dictionary<long, PathNode> _allNodes;

        /// <summary>
        /// 本次寻路使用的节点列表（用于回收）
        /// </summary>
        private readonly List<PathNode> _usedNodes;

        #endregion

        #region 构造函数

        public PathfinderCore()
        {
            _nodePool = SafeObjectPool<PathNode>.Instance;
            _nodePool.Init(NavigationDefine.NodePoolMaxCapacity, NavigationDefine.NodePoolInitialCapacity);

            _openList = new List<PathNode>(256);
            _allNodes = new Dictionary<long, PathNode>(256);
            _usedNodes = new List<PathNode>(256);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 执行A*寻路
        /// </summary>
        public PathResult FindPath(MapData mapData, PathRequest request)
        {
            int startX = request.StartX;
            int startY = request.StartY;
            int startFloor = request.StartFloor;
            int endX = request.EndX;
            int endY = request.EndY;
            int endFloor = request.EndFloor;

            // 验证起点
            if (!mapData.IsValidCellPos(startX, startY, startFloor) ||
                !NavigationUtility.IsCellPassable(mapData, startX, startY, startFloor))
            {
                return PathResult.Fail(EPathStatus.InvalidStart, 0, request.RequesterId);
            }

            // 验证终点
            if (!mapData.IsValidCellPos(endX, endY, endFloor) ||
                !NavigationUtility.IsCellPassable(mapData, endX, endY, endFloor))
            {
                return PathResult.Fail(EPathStatus.InvalidEnd, 0, request.RequesterId);
            }

            // 起点终点相同
            if (startX == endX && startY == endY && startFloor == endFloor)
            {
                return PathResult.Fail(EPathStatus.AlreadyAtTarget, 0, request.RequesterId);
            }

            // 重置工作数据
            Reset();

            // 创建起始节点
            var startNode = AllocateNode(startX, startY, startFloor);
            startNode.G = 0;
            startNode.H = NavigationUtility.CalculateHeuristic(startX, startY, startFloor, endX, endY, endFloor);
            AddToOpenList(startNode);

            int nodesExpanded = 0;

            // A*主循环
            while (_openList.Count > 0)
            {
                // 取出F值最小的节点
                var current = PopMinFromOpenList();
                current.IsInOpenList = false;
                current.IsInClosedList = true;

                nodesExpanded++;

                // 到达终点
                if (current.X == endX && current.Y == endY && current.Floor == endFloor)
                {
                    var path = ReconstructPath(current);
                    var result = PathResult.Success(path, current.G, nodesExpanded, request.RequesterId);
                    Cleanup();
                    return result;
                }

                // 节点上限
                if (nodesExpanded >= NavigationDefine.MaxNodeCount)
                {
                    Cleanup();
                    return PathResult.Fail(EPathStatus.NodeLimitReached, nodesExpanded, request.RequesterId);
                }

                // 展开同层8方向邻居
                ExpandNeighbors(mapData, current, endX, endY, endFloor);

                // 展开跨楼层邻居（楼梯）
                ExpandStairNeighbors(mapData, current, endX, endY, endFloor);
            }

            // 开放列表空了但没找到路径
            Cleanup();
            return PathResult.Fail(EPathStatus.Failed, nodesExpanded, request.RequesterId);
        }

        #endregion

        #region 邻居展开

        /// <summary>
        /// 展开同层8方向邻居
        /// </summary>
        private void ExpandNeighbors(MapData mapData, PathNode current, int endX, int endY, int endFloor)
        {
            for (int i = 0; i < IsometricUtility.EightDirections.Length; i++)
            {
                var dir = IsometricUtility.EightDirections[i];
                int nx = current.X + dir.x;
                int ny = current.Y + dir.y;

                // 边界检查
                if (!mapData.IsValidCellPos(nx, ny, current.Floor))
                    continue;

                // 是否对角移动
                bool isDiagonal = dir.x != 0 && dir.y != 0;

                // 通行检查（包含对角墙角检测）
                if (!NavigationUtility.CanMoveTo(mapData, current.X, current.Y, current.Floor, nx, ny))
                    continue;

                // 计算代价
                int moveCost = NavigationUtility.GetMoveCost(mapData, nx, ny, current.Floor, isDiagonal);
                if (moveCost == int.MaxValue)
                    continue;

                int newG = current.G + moveCost;

                ProcessNeighbor(current, nx, ny, current.Floor, newG, endX, endY, endFloor);
            }
        }

        /// <summary>
        /// 展开楼梯邻居（跨楼层）
        /// </summary>
        private void ExpandStairNeighbors(MapData mapData, PathNode current, int endX, int endY, int endFloor)
        {
            // 检查当前格子是否有上楼梯
            if (NavigationUtility.IsStairUp(mapData, current.X, current.Y, current.Floor))
            {
                int upperFloor = current.Floor + 1;
                if (mapData.IsValidCellPos(current.X, current.Y, upperFloor) &&
                    NavigationUtility.IsCellPassable(mapData, current.X, current.Y, upperFloor))
                {
                    int newG = current.G + NavigationDefine.CostStairs;
                    ProcessNeighbor(current, current.X, current.Y, upperFloor, newG, endX, endY, endFloor);
                }
            }

            // 检查当前格子是否有下楼梯
            if (NavigationUtility.IsStairDown(mapData, current.X, current.Y, current.Floor))
            {
                int lowerFloor = current.Floor - 1;
                if (lowerFloor >= 0 &&
                    mapData.IsValidCellPos(current.X, current.Y, lowerFloor) &&
                    NavigationUtility.IsCellPassable(mapData, current.X, current.Y, lowerFloor))
                {
                    int newG = current.G + NavigationDefine.CostStairs;
                    ProcessNeighbor(current, current.X, current.Y, lowerFloor, newG, endX, endY, endFloor);
                }
            }
        }

        /// <summary>
        /// 处理单个邻居节点
        /// </summary>
        private void ProcessNeighbor(PathNode current, int nx, int ny, int nFloor,
            int newG, int endX, int endY, int endFloor)
        {
            long key = PathNode.GetKey(nx, ny, nFloor);

            if (_allNodes.TryGetValue(key, out var existingNode))
            {
                // 节点已在关闭列表中，跳过
                if (existingNode.IsInClosedList)
                    return;

                // 在开放列表中，检查是否有更优路径
                if (existingNode.IsInOpenList && newG < existingNode.G)
                {
                    existingNode.G = newG;
                    existingNode.Parent = current;
                    // 需要重新排序开放列表
                    SiftUpOpenList(existingNode);
                }
            }
            else
            {
                // 新节点
                var neighborNode = AllocateNode(nx, ny, nFloor);
                neighborNode.G = newG;
                neighborNode.H = NavigationUtility.CalculateHeuristic(nx, ny, nFloor, endX, endY, endFloor);
                neighborNode.Parent = current;
                AddToOpenList(neighborNode);
            }
        }

        #endregion

        #region 路径回溯

        /// <summary>
        /// 从终点回溯构建完整路径
        /// </summary>
        private List<Vector3Int> ReconstructPath(PathNode endNode)
        {
            var path = new List<Vector3Int>();
            var node = endNode;

            while (node != null)
            {
                path.Add(new Vector3Int(node.X, node.Y, node.Floor));
                node = node.Parent;
            }

            path.Reverse();
            return path;
        }

        #endregion

        #region 最小堆（开放列表）

        private void AddToOpenList(PathNode node)
        {
            node.IsInOpenList = true;
            _openList.Add(node);
            SiftUp(_openList.Count - 1);
        }

        private PathNode PopMinFromOpenList()
        {
            var min = _openList[0];
            int lastIndex = _openList.Count - 1;
            _openList[0] = _openList[lastIndex];
            _openList.RemoveAt(lastIndex);

            if (_openList.Count > 0)
                SiftDown(0);

            return min;
        }

        private void SiftUpOpenList(PathNode node)
        {
            int index = _openList.IndexOf(node);
            if (index >= 0)
                SiftUp(index);
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_openList[index].F < _openList[parentIndex].F)
                {
                    Swap(index, parentIndex);
                    index = parentIndex;
                }
                else
                {
                    break;
                }
            }
        }

        private void SiftDown(int index)
        {
            int count = _openList.Count;
            while (true)
            {
                int smallest = index;
                int left = 2 * index + 1;
                int right = 2 * index + 2;

                if (left < count && _openList[left].F < _openList[smallest].F)
                    smallest = left;
                if (right < count && _openList[right].F < _openList[smallest].F)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            (_openList[a], _openList[b]) = (_openList[b], _openList[a]);
        }

        #endregion

        #region 节点管理

        private PathNode AllocateNode(int x, int y, int floor)
        {
            var node = _nodePool.Allocate();
            node.SetPosition(x, y, floor);
            node.G = 0;
            node.H = 0;
            node.Parent = null;
            node.IsInOpenList = false;
            node.IsInClosedList = false;

            long key = node.GetKey();
            _allNodes[key] = node;
            _usedNodes.Add(node);

            return node;
        }

        private void Reset()
        {
            _openList.Clear();
            _allNodes.Clear();
            // 不清理_usedNodes，在Cleanup中回收
        }

        private void Cleanup()
        {
            // 回收所有节点到对象池
            for (int i = 0; i < _usedNodes.Count; i++)
            {
                _nodePool.Recycle(_usedNodes[i]);
            }

            _usedNodes.Clear();
            _openList.Clear();
            _allNodes.Clear();
        }

        #endregion
    }
}
