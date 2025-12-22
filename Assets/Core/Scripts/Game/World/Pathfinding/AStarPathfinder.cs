/**
 * AStarPathfinder.cs
 * A* 寻路算法实现
 * 
 * 特性：
 * - 支持 8 方向移动
 * - 综合检查 Tile 和 Entity 阻挡
 * - 支持不同地形的移动消耗
 * - 可配置对角移动
 * - 路径平滑
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDFramework.MapSystem.Pathfinding
{
    /// <summary>
    /// 寻路结果
    /// </summary>
    public class PathResult
    {
        /// <summary>
        /// 是否找到路径
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 路径点列表（从起点到终点）
        /// </summary>
        public List<TileCoord> Path { get; set; }
        
        /// <summary>
        /// 路径总长度
        /// </summary>
        public float TotalCost { get; set; }
        
        /// <summary>
        /// 搜索的节点数量
        /// </summary>
        public int NodesSearched { get; set; }
        
        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailureReason { get; set; }
        
        public PathResult()
        {
            Path = new List<TileCoord>();
        }
        
        public static PathResult Failed(string reason)
        {
            return new PathResult
            {
                Success = false,
                FailureReason = reason
            };
        }
    }
    
    /// <summary>
    /// 寻路节点
    /// </summary>
    internal class PathNode : IComparable<PathNode>
    {
        public TileCoord Coord;
        public PathNode Parent;
        public float G; // 从起点到当前节点的实际代价
        public float H; // 从当前节点到终点的估计代价（启发式）
        public float F => G + H; // 总代价
        
        public PathNode(TileCoord coord)
        {
            Coord = coord;
        }
        
        public int CompareTo(PathNode other)
        {
            int compare = F.CompareTo(other.F);
            if (compare == 0)
            {
                compare = H.CompareTo(other.H);
            }
            return compare;
        }
    }
    
    /// <summary>
    /// 寻路配置
    /// </summary>
    [Serializable]
    public class PathfindingConfig
    {
        /// <summary>
        /// 是否允许对角移动
        /// </summary>
        public bool AllowDiagonal = true;
        
        /// <summary>
        /// 对角移动时是否需要两侧都可通行
        /// </summary>
        public bool DiagonalRequiresBothSides = true;
        
        /// <summary>
        /// 最大搜索节点数（防止死循环）
        /// </summary>
        public int MaxSearchNodes = 10000;
        
        /// <summary>
        /// 最大路径长度
        /// </summary>
        public int MaxPathLength = 500;
        
        /// <summary>
        /// 是否忽略 Entity 阻挡
        /// </summary>
        public bool IgnoreEntities = false;
        
        /// <summary>
        /// 启发式权重（>1 会更快但可能不是最优路径）
        /// </summary>
        public float HeuristicWeight = 1.0f;
    }
    
    /// <summary>
    /// A* 寻路器
    /// </summary>
    public class AStarPathfinder
    {
        #region 方向定义
        
        /// <summary>
        /// 4 方向偏移（上下左右）
        /// </summary>
        private static readonly TileCoord[] FourDirections = new TileCoord[]
        {
            new TileCoord(0, 1),   // 上
            new TileCoord(0, -1),  // 下
            new TileCoord(-1, 0),  // 左
            new TileCoord(1, 0)    // 右
        };
        
        /// <summary>
        /// 8 方向偏移（包含对角）
        /// </summary>
        private static readonly TileCoord[] EightDirections = new TileCoord[]
        {
            new TileCoord(0, 1),    // 上
            new TileCoord(0, -1),   // 下
            new TileCoord(-1, 0),   // 左
            new TileCoord(1, 0),    // 右
            new TileCoord(-1, 1),   // 左上
            new TileCoord(1, 1),    // 右上
            new TileCoord(-1, -1),  // 左下
            new TileCoord(1, -1)    // 右下
        };
        
        /// <summary>
        /// 对角方向索引
        /// </summary>
        private static readonly int[] DiagonalIndices = new int[] { 4, 5, 6, 7 };
        
        /// <summary>
        /// 对角方向对应的两个正交方向索引
        /// </summary>
        private static readonly int[][] DiagonalToOrthogonal = new int[][]
        {
            new int[] { 2, 0 }, // 左上 = 左 + 上
            new int[] { 3, 0 }, // 右上 = 右 + 上
            new int[] { 2, 1 }, // 左下 = 左 + 下
            new int[] { 3, 1 }  // 右下 = 右 + 下
        };
        
        /// <summary>
        /// 直线移动代价
        /// </summary>
        private const float STRAIGHT_COST = 1.0f;
        
        /// <summary>
        /// 对角移动代价 (√2)
        /// </summary>
        private const float DIAGONAL_COST = 1.414f;
        
        #endregion
        
        #region 字段
        
        /// <summary>
        /// 当前地图
        /// </summary>
        private Map _map;
        
        /// <summary>
        /// 寻路配置
        /// </summary>
        private PathfindingConfig _config;
        
        /// <summary>
        /// 开放列表（待检查的节点）
        /// </summary>
        private SortedSet<PathNode> _openSet;
        
        /// <summary>
        /// 开放列表快速查找
        /// </summary>
        private Dictionary<TileCoord, PathNode> _openDict;
        
        /// <summary>
        /// 关闭列表（已检查的节点）
        /// </summary>
        private HashSet<TileCoord> _closedSet;
        
        /// <summary>
        /// 节点缓存
        /// </summary>
        private Dictionary<TileCoord, PathNode> _nodeCache;
        
        #endregion
        
        #region 构造函数
        
        public AStarPathfinder(Map map, PathfindingConfig config = null)
        {
            _map = map;
            _config = config ?? new PathfindingConfig();
            
            _openSet = new SortedSet<PathNode>(new PathNodeComparer());
            _openDict = new Dictionary<TileCoord, PathNode>();
            _closedSet = new HashSet<TileCoord>();
            _nodeCache = new Dictionary<TileCoord, PathNode>();
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 寻找路径
        /// </summary>
        public PathResult FindPath(TileCoord start, TileCoord end)
        {
            // 清理上次搜索的数据
            ClearSearchData();
            
            // 检查起点和终点
            if (!_map.IsTileCoordValid(start))
            {
                return PathResult.Failed("起点坐标无效");
            }
            
            if (!_map.IsTileCoordValid(end))
            {
                return PathResult.Failed("终点坐标无效");
            }
            
            if (start.Equals(end))
            {
                return new PathResult
                {
                    Success = true,
                    Path = new List<TileCoord> { start },
                    TotalCost = 0
                };
            }
            
            if (!IsWalkable(end))
            {
                return PathResult.Failed("终点不可行走");
            }
            
            // 初始化起点
            PathNode startNode = GetOrCreateNode(start);
            startNode.G = 0;
            startNode.H = CalculateHeuristic(start, end);
            
            _openSet.Add(startNode);
            _openDict[start] = startNode;
            
            int nodesSearched = 0;
            
            // A* 主循环
            while (_openSet.Count > 0 && nodesSearched < _config.MaxSearchNodes)
            {
                // 获取 F 值最小的节点
                PathNode current = _openSet.Min;
                _openSet.Remove(current);
                _openDict.Remove(current.Coord);
                
                nodesSearched++;
                
                // 找到终点
                if (current.Coord.Equals(end))
                {
                    return BuildResult(current, nodesSearched);
                }
                
                // 添加到关闭列表
                _closedSet.Add(current.Coord);
                
                // 检查相邻节点
                var directions = _config.AllowDiagonal ? EightDirections : FourDirections;
                
                for (int i = 0; i < directions.Length; i++)
                {
                    TileCoord neighborCoord = new TileCoord(
                        current.Coord.x + directions[i].x,
                        current.Coord.y + directions[i].y
                    );
                    
                    // 跳过无效或已关闭的节点
                    if (!_map.IsTileCoordValid(neighborCoord)) continue;
                    if (_closedSet.Contains(neighborCoord)) continue;
                    if (!IsWalkable(neighborCoord)) continue;
                    
                    // 对角移动检查
                    if (_config.AllowDiagonal && _config.DiagonalRequiresBothSides && i >= 4)
                    {
                        int diagIndex = i - 4;
                        var orthogonal = DiagonalToOrthogonal[diagIndex];
                        
                        TileCoord side1 = new TileCoord(
                            current.Coord.x + FourDirections[orthogonal[0]].x,
                            current.Coord.y + FourDirections[orthogonal[0]].y
                        );
                        TileCoord side2 = new TileCoord(
                            current.Coord.x + FourDirections[orthogonal[1]].x,
                            current.Coord.y + FourDirections[orthogonal[1]].y
                        );
                        
                        // 如果两侧有任一不可通行，跳过对角移动
                        if (!IsWalkable(side1) || !IsWalkable(side2))
                        {
                            continue;
                        }
                    }
                    
                    // 计算移动代价
                    float moveCost = i < 4 ? STRAIGHT_COST : DIAGONAL_COST;
                    moveCost *= GetMoveCost(neighborCoord);
                    
                    float tentativeG = current.G + moveCost;
                    
                    // 获取或创建邻居节点
                    PathNode neighbor;
                    bool inOpenSet = _openDict.TryGetValue(neighborCoord, out neighbor);
                    
                    if (!inOpenSet)
                    {
                        neighbor = GetOrCreateNode(neighborCoord);
                        neighbor.H = CalculateHeuristic(neighborCoord, end);
                    }
                    
                    // 如果找到更好的路径
                    if (!inOpenSet || tentativeG < neighbor.G)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeG;
                        
                        if (inOpenSet)
                        {
                            // 更新优先级（需要先移除再添加）
                            _openSet.Remove(neighbor);
                        }
                        
                        _openSet.Add(neighbor);
                        _openDict[neighborCoord] = neighbor;
                    }
                }
            }
            
            // 没找到路径
            return PathResult.Failed(nodesSearched >= _config.MaxSearchNodes 
                ? "搜索节点数超过上限" 
                : "无法到达终点");
        }
        
        /// <summary>
        /// 检查两点之间是否有直线路径（用于优化）
        /// </summary>
        public bool HasLineOfSight(TileCoord start, TileCoord end)
        {
            // Bresenham 直线算法
            int x0 = start.x, y0 = start.y;
            int x1 = end.x, y1 = end.y;
            
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                if (x0 == x1 && y0 == y1) break;
                
                if (!IsWalkable(new TileCoord(x0, y0)))
                {
                    return false;
                }
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 平滑路径（移除不必要的中间点）
        /// </summary>
        public List<TileCoord> SmoothPath(List<TileCoord> path)
        {
            if (path == null || path.Count <= 2)
            {
                return path;
            }
            
            var smoothed = new List<TileCoord> { path[0] };
            int current = 0;
            
            while (current < path.Count - 1)
            {
                int farthest = current + 1;
                
                // 找到最远的可直接到达的点
                for (int i = path.Count - 1; i > current + 1; i--)
                {
                    if (HasLineOfSight(path[current], path[i]))
                    {
                        farthest = i;
                        break;
                    }
                }
                
                smoothed.Add(path[farthest]);
                current = farthest;
            }
            
            return smoothed;
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 清理搜索数据
        /// </summary>
        private void ClearSearchData()
        {
            _openSet.Clear();
            _openDict.Clear();
            _closedSet.Clear();
            
            // 重置节点缓存中的数据
            foreach (var node in _nodeCache.Values)
            {
                node.G = float.MaxValue;
                node.H = 0;
                node.Parent = null;
            }
        }
        
        /// <summary>
        /// 获取或创建节点
        /// </summary>
        private PathNode GetOrCreateNode(TileCoord coord)
        {
            if (!_nodeCache.TryGetValue(coord, out var node))
            {
                node = new PathNode(coord);
                _nodeCache[coord] = node;
            }
            
            node.G = float.MaxValue;
            node.H = 0;
            node.Parent = null;
            
            return node;
        }
        
        /// <summary>
        /// 检查位置是否可行走
        /// </summary>
        private bool IsWalkable(TileCoord coord)
        {
            if (!_map.IsTileCoordValid(coord))
            {
                return false;
            }
            
            // 检查 Tile 阻挡
            TileData tile = _map.GetTile(coord);
            if (tile.IsBlocking)
            {
                return false;
            }
            
            // 检查 Entity 阻挡
            if (!_config.IgnoreEntities)
            {
                if (_map.Entities.HasBlockingEntityAt(coord))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取移动消耗
        /// </summary>
        private float GetMoveCost(TileCoord coord)
        {
            // TODO: 从 TileConfig 获取移动消耗
            // 目前返回固定值 1.0
            return 1.0f;
        }
        
        /// <summary>
        /// 计算启发式值（曼哈顿距离或欧几里得距离）
        /// </summary>
        private float CalculateHeuristic(TileCoord from, TileCoord to)
        {
            if (_config.AllowDiagonal)
            {
                // 对角距离（Chebyshev）
                int dx = Math.Abs(to.x - from.x);
                int dy = Math.Abs(to.y - from.y);
                return (dx + dy) + (DIAGONAL_COST - 2 * STRAIGHT_COST) * Math.Min(dx, dy);
            }
            else
            {
                // 曼哈顿距离
                return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
            }
        }
        
        /// <summary>
        /// 构建寻路结果
        /// </summary>
        private PathResult BuildResult(PathNode endNode, int nodesSearched)
        {
            var result = new PathResult
            {
                Success = true,
                TotalCost = endNode.G,
                NodesSearched = nodesSearched
            };
            
            // 回溯构建路径
            PathNode current = endNode;
            while (current != null)
            {
                result.Path.Add(current.Coord);
                current = current.Parent;
            }
            
            // 反转路径（从起点到终点）
            result.Path.Reverse();
            
            // 检查路径长度
            if (result.Path.Count > _config.MaxPathLength)
            {
                return PathResult.Failed("路径长度超过上限");
            }
            
            return result;
        }
        
        #endregion
    }
    
    /// <summary>
    /// PathNode 比较器
    /// </summary>
    internal class PathNodeComparer : IComparer<PathNode>
    {
        public int Compare(PathNode x, PathNode y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            int compare = x.F.CompareTo(y.F);
            if (compare == 0)
            {
                compare = x.H.CompareTo(y.H);
            }
            if (compare == 0)
            {
                // 使用坐标作为唯一标识
                compare = x.Coord.x.CompareTo(y.Coord.x);
                if (compare == 0)
                {
                    compare = x.Coord.y.CompareTo(y.Coord.y);
                }
            }
            return compare;
        }
    }
}
