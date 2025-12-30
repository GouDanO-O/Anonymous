/*******************************************************************************
 * 文件名:    Pathfinder.cs
 * 描述:      A*寻路算法实现，支持单层和跨楼层寻路
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   Pathfinder 提供：
 *   - 单层A*寻路
 *   - 跨楼层寻路（通过楼层连接器）
 *   - 区域级别寻路（快速可达性判断）
 ******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 寻路器
    /// </summary>
    public class Pathfinder
    {
        #region 常量

        /// <summary>
        /// 最大搜索节点数
        /// </summary>
        private const int MaxSearchNodes = 100000;

        /// <summary>
        /// 直线移动代价
        /// </summary>
        private const int StraightCost = 10;

        /// <summary>
        /// 对角线移动代价
        /// </summary>
        private const int DiagonalCost = 14;

        #endregion

        #region 字段

        /// <summary>
        /// 所属Site
        /// </summary>
        private Site _site;

        /// <summary>
        /// 开放列表
        /// </summary>
        private PriorityQueue<PathNode> _openList;

        /// <summary>
        /// 节点缓存
        /// </summary>
        private Dictionary<GlobalCoord, PathNode> _nodeCache;

        /// <summary>
        /// 关闭列表
        /// </summary>
        private HashSet<GlobalCoord> _closedSet;

        #endregion

        #region 属性

        /// <summary>
        /// 所属Site
        /// </summary>
        public Site Site => _site;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public Pathfinder(Site site)
        {
            _site = site;
            _openList = new PriorityQueue<PathNode>();
            _nodeCache = new Dictionary<GlobalCoord, PathNode>();
            _closedSet = new HashSet<GlobalCoord>();
        }

        #endregion

        #region 单层寻路

        /// <summary>
        /// 单层A*寻路
        /// </summary>
        public PathResult FindPath(Floor floor, CellCoord start, CellCoord goal, 
            PathfindingOptions options = null)
        {
            if (floor == null)
                return PathResult.Failed("Invalid floor");

            options ??= PathfindingOptions.Default;

            // 检查起点和终点
            if (!floor.InBounds(start))
                return PathResult.Failed("Start out of bounds");
            if (!floor.InBounds(goal))
                return PathResult.Failed("Goal out of bounds");

            if (start == goal)
                return PathResult.Success(new List<CellCoord> { start });

            // 检查终点可达性
            if (!options.IgnoreGoalPassability && !IsPassable(floor, goal, options))
                return PathResult.Failed("Goal is impassable");

            // 清除上次搜索的缓存
            ClearSearchData();

            // 创建起始节点
            var startNode = GetOrCreateNode(new GlobalCoord(start, floor.FloorIndex));
            startNode.G = 0;
            startNode.H = Heuristic(start, goal);
            _openList.Enqueue(startNode, startNode.F);

            int searchedNodes = 0;

            while (_openList.Count > 0 && searchedNodes < MaxSearchNodes)
            {
                var current = _openList.Dequeue();
                searchedNodes++;

                if (current.Position.ToCellCoord() == goal)
                {
                    // 找到路径
                    return PathResult.Success(ReconstructPath(current));
                }

                _closedSet.Add(current.Position);

                // 检查邻居
                foreach (var neighbor in GetNeighbors(current.Position, floor, options))
                {
                    if (_closedSet.Contains(neighbor))
                        continue;

                    var neighborCell = neighbor.ToCellCoord();
                    int moveCost = GetMoveCost(floor, current.Position.ToCellCoord(), neighborCell, options);
                    int newG = current.G + moveCost;

                    var neighborNode = GetOrCreateNode(neighbor);

                    if (newG < neighborNode.G)
                    {
                        neighborNode.Parent = current;
                        neighborNode.G = newG;
                        neighborNode.H = Heuristic(neighborCell, goal);

                        if (!_openList.Contains(neighborNode))
                        {
                            _openList.Enqueue(neighborNode, neighborNode.F);
                        }
                        else
                        {
                            _openList.UpdatePriority(neighborNode, neighborNode.F);
                        }
                    }
                }
            }

            return PathResult.Failed("No path found");
        }

        #endregion

        #region 跨楼层寻路

        /// <summary>
        /// 跨楼层A*寻路
        /// </summary>
        public PathResult FindPath(GlobalCoord start, GlobalCoord goal, 
            PathfindingOptions options = null)
        {
            if (_site == null)
                return PathResult.Failed("Invalid site");

            options ??= PathfindingOptions.Default;

            // 检查起点和终点楼层
            var startFloor = _site.GetFloor(start.y);
            var goalFloor = _site.GetFloor(goal.y);

            if (startFloor == null || goalFloor == null)
                return PathResult.Failed("Invalid floor");

            // 如果在同一层，使用单层寻路
            if (start.y == goal.y)
            {
                var result = FindPath(startFloor, start.ToCellCoord(), goal.ToCellCoord(), options);
                if (result.success)
                {
                    // 转换为全局坐标路径
                    var globalPath = new List<GlobalCoord>();
                    foreach (var cell in result.Path)
                    {
                        globalPath.Add(new GlobalCoord(cell, start.y));
                    }
                    return PathResult.Success(globalPath);
                }
                return result;
            }

            // 跨楼层寻路
            ClearSearchData();

            var startNode = GetOrCreateNode(start);
            startNode.G = 0;
            startNode.H = GlobalHeuristic(start, goal);
            _openList.Enqueue(startNode, startNode.F);

            int searchedNodes = 0;

            while (_openList.Count > 0 && searchedNodes < MaxSearchNodes)
            {
                var current = _openList.Dequeue();
                searchedNodes++;

                if (current.Position == goal)
                {
                    return PathResult.Success(ReconstructGlobalPath(current));
                }

                _closedSet.Add(current.Position);

                var currentFloor = _site.GetFloor(current.Position.y);
                if (currentFloor == null)
                    continue;

                // 检查同层邻居
                foreach (var neighbor in GetNeighbors(current.Position, currentFloor, options))
                {
                    ProcessNeighbor(current, neighbor, goal, options);
                }

                // 检查楼层连接器
                var connector = _site.ConnectionManager.GetConnectorAt(current.Position);
                if (connector != null && connector.CanUse)
                {
                    foreach (int targetFloor in connector.GetReachableFloors(current.Position.y))
                    {
                        var exitPos = connector.GetExitPosition(targetFloor);
                        var neighbor = new GlobalCoord(exitPos, targetFloor);
                        
                        int connectorCost = connector.TraverseCost;
                        ProcessNeighbor(current, neighbor, goal, options, connectorCost);
                    }
                }
            }

            return PathResult.Failed("No path found");
        }

        /// <summary>
        /// 处理邻居节点
        /// </summary>
        private void ProcessNeighbor(PathNode current, GlobalCoord neighbor, 
            GlobalCoord goal, PathfindingOptions options, int extraCost = 0)
        {
            if (_closedSet.Contains(neighbor))
                return;

            var floor = _site.GetFloor(neighbor.y);
            if (floor == null)
                return;

            int moveCost = extraCost;
            if (current.Position.y == neighbor.y)
            {
                moveCost += GetMoveCost(floor, current.Position.ToCellCoord(), 
                    neighbor.ToCellCoord(), options);
            }
            else
            {
                moveCost += StraightCost; // 楼层转换基础代价
            }

            int newG = current.G + moveCost;

            var neighborNode = GetOrCreateNode(neighbor);

            if (newG < neighborNode.G)
            {
                neighborNode.Parent = current;
                neighborNode.G = newG;
                neighborNode.H = GlobalHeuristic(neighbor, goal);

                if (!_openList.Contains(neighborNode))
                {
                    _openList.Enqueue(neighborNode, neighborNode.F);
                }
                else
                {
                    _openList.UpdatePriority(neighborNode, neighborNode.F);
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 清除搜索数据
        /// </summary>
        private void ClearSearchData()
        {
            _openList.Clear();
            _nodeCache.Clear();
            _closedSet.Clear();
        }

        /// <summary>
        /// 获取或创建节点
        /// </summary>
        private PathNode GetOrCreateNode(GlobalCoord position)
        {
            if (!_nodeCache.TryGetValue(position, out var node))
            {
                node = new PathNode(position);
                _nodeCache[position] = node;
            }
            return node;
        }

        /// <summary>
        /// 获取邻居坐标
        /// </summary>
        private IEnumerable<GlobalCoord> GetNeighbors(GlobalCoord pos, Floor floor, 
            PathfindingOptions options)
        {
            var cell = pos.ToCellCoord();
            int floorIndex = pos.y;

            // 四方向邻居
            foreach (var dir in DirectionExtensions.CardinalDirections)
            {
                var offset = dir.ToOffset();
                var neighbor = new CellCoord(cell.x + offset.x, cell.z + offset.z);

                if (floor.InBounds(neighbor) && IsPassable(floor, neighbor, options))
                {
                    yield return new GlobalCoord(neighbor, floorIndex);
                }
            }

            // 对角线邻居（如果允许）
            if (options.AllowDiagonal)
            {
                foreach (var dir in DirectionExtensions.DiagonalDirections)
                {
                    var offset = dir.ToOffset();
                    var neighbor = new CellCoord(cell.x + offset.x, cell.z + offset.z);

                    if (floor.InBounds(neighbor) && IsPassable(floor, neighbor, options))
                    {
                        // 检查对角线是否被阻挡
                        var adj1 = new CellCoord(cell.x + offset.x, cell.z);
                        var adj2 = new CellCoord(cell.x, cell.z + offset.z);

                        if (IsPassable(floor, adj1, options) && IsPassable(floor, adj2, options))
                        {
                            yield return new GlobalCoord(neighbor, floorIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查格子是否可通行
        /// </summary>
        private bool IsPassable(Floor floor, CellCoord cell, PathfindingOptions options)
        {
            // Tile层通行性
            var passability = floor.GetPassability(cell);
            if (passability == Passability.Impassable)
                return false;

            // 实体层通行性
            var entityGrid = floor.EntityGrid;
            if (entityGrid != null)
            {
                var entityPass = entityGrid.GetEntityPassability(cell);
                if (entityPass == Passability.Impassable)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取移动代价
        /// </summary>
        private int GetMoveCost(Floor floor, CellCoord from, CellCoord to, 
            PathfindingOptions options)
        {
            // 基础代价
            int baseCost = IsDiagonal(from, to) ? DiagonalCost : StraightCost;

            // 地形代价
            int terrainCost = floor.GetPathCost(to);

            // 实体代价
            var entityGrid = floor.EntityGrid;
            int entityCost = 0;
            if (entityGrid != null)
            {
                entityCost = entityGrid.GetCombinedPathCost(to) - floor.GetPathCost(to);
            }

            return baseCost + terrainCost + entityCost;
        }

        /// <summary>
        /// 检查是否是对角移动
        /// </summary>
        private bool IsDiagonal(CellCoord from, CellCoord to)
        {
            return from.x != to.x && from.z != to.z;
        }

        /// <summary>
        /// 曼哈顿距离启发式
        /// </summary>
        private int Heuristic(CellCoord from, CellCoord to)
        {
            return (Mathf.Abs(to.x - from.x) + Mathf.Abs(to.z - from.z)) * StraightCost;
        }

        /// <summary>
        /// 全局启发式（包含楼层差异）
        /// </summary>
        private int GlobalHeuristic(GlobalCoord from, GlobalCoord to)
        {
            int horizontal = (Mathf.Abs(to.x - from.x) + Mathf.Abs(to.z - from.z)) * StraightCost;
            int vertical = Mathf.Abs(to.y - from.y) * 50; // 楼层转换权重
            return horizontal + vertical;
        }

        /// <summary>
        /// 重建路径（单层）
        /// </summary>
        private List<CellCoord> ReconstructPath(PathNode endNode)
        {
            var path = new List<CellCoord>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.Position.ToCellCoord());
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// 重建路径（跨楼层）
        /// </summary>
        private List<GlobalCoord> ReconstructGlobalPath(PathNode endNode)
        {
            var path = new List<GlobalCoord>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        #endregion

        #region 快速检查

        /// <summary>
        /// 快速检查是否可达（使用区域系统）
        /// </summary>
        public bool CanReach(Floor floor, CellCoord from, CellCoord to)
        {
            // TODO: 使用RegionGrid进行快速判断
            // var regionGrid = floor.RegionGrid;
            // return regionGrid?.CanReach(from, to) ?? false;

            // 简单实现：尝试寻路
            var result = FindPath(floor, from, to);
            return result.success;
        }

        /// <summary>
        /// 快速检查是否可达（跨楼层）
        /// </summary>
        public bool CanReach(GlobalCoord from, GlobalCoord to)
        {
            if (from.y == to.y)
            {
                var floor = _site.GetFloor(from.y);
                return floor != null && CanReach(floor, from.ToCellCoord(), to.ToCellCoord());
            }

            // 跨楼层：检查楼层连通性
            return _site.ConnectionManager.AreFloorsConnected(from.y, to.y);
        }

        #endregion
    }

    /// <summary>
    /// 寻路节点
    /// </summary>
    public class PathNode
    {
        /// <summary>
        /// 位置
        /// </summary>
        public GlobalCoord Position;

        /// <summary>
        /// 父节点
        /// </summary>
        public PathNode Parent;

        /// <summary>
        /// 从起点到该点的代价
        /// </summary>
        public int G = int.MaxValue;

        /// <summary>
        /// 到终点的估计代价
        /// </summary>
        public int H;

        /// <summary>
        /// 总代价
        /// </summary>
        public int F => G + H;

        public PathNode(GlobalCoord position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// 寻路结果
    /// </summary>
    public class PathResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; private set; }

        /// <summary>
        /// 路径（单层）
        /// </summary>
        public List<CellCoord> Path { get; private set; }

        /// <summary>
        /// 路径（跨楼层）
        /// </summary>
        public List<GlobalCoord> GlobalPath { get; private set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailReason { get; private set; }

        /// <summary>
        /// 路径长度
        /// </summary>
        public int Length => Path?.Count ?? GlobalPath?.Count ?? 0;

        private PathResult() { }

        public static PathResult Success(List<CellCoord> path)
        {
            return new PathResult { success = true, Path = path };
        }

        public static PathResult Success(List<GlobalCoord> globalPath)
        {
            return new PathResult { success = true, GlobalPath = globalPath };
        }

        public static PathResult Failed(string reason)
        {
            return new PathResult { success = false, FailReason = reason };
        }
    }

    /// <summary>
    /// 寻路选项
    /// </summary>
    public class PathfindingOptions
    {
        /// <summary>
        /// 是否允许对角移动
        /// </summary>
        public bool AllowDiagonal = true;

        /// <summary>
        /// 是否忽略终点的通行性检查
        /// </summary>
        public bool IgnoreGoalPassability = false;

        /// <summary>
        /// 最大搜索距离
        /// </summary>
        public int MaxSearchDistance = int.MaxValue;

        /// <summary>
        /// 是否可以穿越门
        /// </summary>
        public bool CanOpenDoors = true;

        /// <summary>
        /// 是否可以使用电梯
        /// </summary>
        public bool CanUseElevators = true;

        /// <summary>
        /// 默认选项
        /// </summary>
        public static PathfindingOptions Default => new PathfindingOptions();
    }

    /// <summary>
    /// 简单的优先队列实现
    /// </summary>
    public class PriorityQueue<T>
    {
        private List<(T item, int priority)> _heap = new List<(T, int)>();
        private Dictionary<T, int> _indices = new Dictionary<T, int>();

        public int Count => _heap.Count;

        public void Clear()
        {
            _heap.Clear();
            _indices.Clear();
        }

        public void Enqueue(T item, int priority)
        {
            _heap.Add((item, priority));
            _indices[item] = _heap.Count - 1;
            BubbleUp(_heap.Count - 1);
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var result = _heap[0].item;
            _indices.Remove(result);

            var last = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);

            if (_heap.Count > 0)
            {
                _heap[0] = last;
                _indices[last.item] = 0;
                BubbleDown(0);
            }

            return result;
        }

        public bool Contains(T item)
        {
            return _indices.ContainsKey(item);
        }

        public void UpdatePriority(T item, int newPriority)
        {
            if (!_indices.TryGetValue(item, out int index))
                return;

            int oldPriority = _heap[index].priority;
            _heap[index] = (item, newPriority);

            if (newPriority < oldPriority)
                BubbleUp(index);
            else
                BubbleDown(index);
        }

        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_heap[index].priority >= _heap[parent].priority)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void BubbleDown(int index)
        {
            while (true)
            {
                int smallest = index;
                int left = index * 2 + 1;
                int right = index * 2 + 2;

                if (left < _heap.Count && _heap[left].priority < _heap[smallest].priority)
                    smallest = left;
                if (right < _heap.Count && _heap[right].priority < _heap[smallest].priority)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;

            _indices[_heap[i].item] = i;
            _indices[_heap[j].item] = j;
        }
    }
}
