/**
 * MultiLevelPathfinder.cs
 * 多层寻路系统
 * 
 * 支持：
 * - 跨楼层寻路
 * - 楼梯/梯子/电梯作为连接点
 * - 分层寻路优化
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using GDFramework.MapSystem.Pathfinding;

namespace GDFramework.MapSystem.MultiLevel
{
    /// <summary>
    /// 多层寻路结果
    /// </summary>
    public class MultiLevelPathResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success;
        
        /// <summary>
        /// 路径段列表（每段在同一层）
        /// </summary>
        public List<PathSegment> Segments;
        
        /// <summary>
        /// 总路径长度
        /// </summary>
        public float TotalCost;
        
        /// <summary>
        /// 需要经过的楼层转换次数
        /// </summary>
        public int TransitionCount;
        
        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailureReason;
        
        public MultiLevelPathResult()
        {
            Segments = new List<PathSegment>();
        }
        
        public static MultiLevelPathResult Failed(string reason)
        {
            return new MultiLevelPathResult
            {
                Success = false,
                FailureReason = reason
            };
        }
        
        /// <summary>
        /// 获取完整路径（展平所有段）
        /// </summary>
        public List<LevelCoord> GetFullPath()
        {
            var fullPath = new List<LevelCoord>();
            
            foreach (var segment in Segments)
            {
                foreach (var coord in segment.Path)
                {
                    fullPath.Add(new LevelCoord(coord, segment.Level));
                }
            }
            
            return fullPath;
        }
    }
    
    /// <summary>
    /// 路径段（单层内的路径）
    /// </summary>
    public class PathSegment
    {
        /// <summary>
        /// 所在层级
        /// </summary>
        public int Level;
        
        /// <summary>
        /// 该层内的路径
        /// </summary>
        public List<TileCoord> Path;
        
        /// <summary>
        /// 段的移动消耗
        /// </summary>
        public float Cost;
        
        /// <summary>
        /// 段末尾的转换点（如果有）
        /// </summary>
        public LevelTransition EndTransition;
        
        public PathSegment(int level)
        {
            Level = level;
            Path = new List<TileCoord>();
        }
    }
    
    /// <summary>
    /// 多层寻路器
    /// </summary>
    public class MultiLevelPathfinder
    {
        #region 字段
        
        private MultiLevelMap _map;
        private Dictionary<int, AStarPathfinder> _pathfinders;
        private PathfindingConfig _config;
        
        /// <summary>
        /// 楼层转换的额外消耗
        /// </summary>
        private float _transitionCost = 5f;
        
        /// <summary>
        /// 最大搜索层数
        /// </summary>
        private int _maxLevelSearch = 10;
        
        #endregion
        
        #region 构造函数
        
        public MultiLevelPathfinder(MultiLevelMap map, PathfindingConfig config = null)
        {
            _map = map;
            _config = config ?? new PathfindingConfig();
            _pathfinders = new Dictionary<int, AStarPathfinder>();
            
            // 为每层创建寻路器
            foreach (var level in map.GetAllLevels())
            {
                var adapter = new MapLevelAdapter(level, map.MapId);
                _pathfinders[level.LevelIndex] = new AStarPathfinder(adapter, _config);
            }
        }
        
        #endregion
        
        #region 寻路
        
        /// <summary>
        /// 同层寻路
        /// </summary>
        public PathResult FindPath(LevelCoord start, LevelCoord end)
        {
            if (start.z != end.z)
            {
                // 跨层需要使用 FindMultiLevelPath
                var result = FindMultiLevelPath(start, end);
                if (result.Success && result.Segments.Count == 1)
                {
                    // 转换为普通 PathResult
                    return new PathResult
                    {
                        Success = true,
                        Path = result.Segments[0].Path,
                        TotalCost = result.TotalCost
                    };
                }
                return PathResult.Failed("需要跨层寻路");
            }
            
            if (!_pathfinders.TryGetValue(start.z, out var pathfinder))
            {
                return PathResult.Failed($"层级 {start.z} 不存在");
            }
            
            return pathfinder.FindPath(start.ToTileCoord(), end.ToTileCoord());
        }
        
        /// <summary>
        /// 跨层寻路
        /// </summary>
        public MultiLevelPathResult FindMultiLevelPath(LevelCoord start, LevelCoord end)
        {
            // 如果在同一层，直接寻路
            if (start.z == end.z)
            {
                return FindSameLevelPath(start, end);
            }
            
            // 使用分层 A* 算法
            return FindCrossLevelPath(start, end);
        }
        
        /// <summary>
        /// 同层寻路
        /// </summary>
        private MultiLevelPathResult FindSameLevelPath(LevelCoord start, LevelCoord end)
        {
            if (!_pathfinders.TryGetValue(start.z, out var pathfinder))
            {
                return MultiLevelPathResult.Failed($"层级 {start.z} 不存在");
            }
            
            var result = pathfinder.FindPath(start.ToTileCoord(), end.ToTileCoord());
            
            if (!result.Success)
            {
                return MultiLevelPathResult.Failed(result.FailureReason);
            }
            
            var multiResult = new MultiLevelPathResult
            {
                Success = true,
                TotalCost = result.TotalCost,
                TransitionCount = 0
            };
            
            multiResult.Segments.Add(new PathSegment(start.z)
            {
                Path = result.Path,
                Cost = result.TotalCost
            });
            
            return multiResult;
        }
        
        /// <summary>
        /// 跨层寻路（使用层级图搜索）
        /// </summary>
        private MultiLevelPathResult FindCrossLevelPath(LevelCoord start, LevelCoord end)
        {
            // 构建层级图
            var levelGraph = BuildLevelGraph(start, end);
            
            if (levelGraph.Count == 0)
            {
                return MultiLevelPathResult.Failed("无法找到连接两层的转换点");
            }
            
            // 在层级图上搜索最优路径
            var levelPath = SearchLevelGraph(levelGraph, start, end);
            
            if (levelPath == null || levelPath.Count == 0)
            {
                return MultiLevelPathResult.Failed("无法找到跨层路径");
            }
            
            // 构建详细路径
            return BuildDetailedPath(levelPath, start, end);
        }
        
        /// <summary>
        /// 构建层级图
        /// </summary>
        private List<TransitionNode> BuildLevelGraph(LevelCoord start, LevelCoord end)
        {
            var nodes = new List<TransitionNode>();
            
            // 添加起点作为虚拟节点
            nodes.Add(new TransitionNode
            {
                Coord = start,
                IsStart = true
            });
            
            // 添加所有相关的转换点
            foreach (var transition in _map.Transitions)
            {
                // 只考虑在起点和终点层级范围内的转换点
                int minLevel = Mathf.Min(start.z, end.z);
                int maxLevel = Mathf.Max(start.z, end.z);
                
                if (transition.From.z >= minLevel - 1 && transition.From.z <= maxLevel + 1)
                {
                    nodes.Add(new TransitionNode
                    {
                        Coord = transition.From,
                        Transition = transition
                    });
                }
            }
            
            // 添加终点作为虚拟节点
            nodes.Add(new TransitionNode
            {
                Coord = end,
                IsEnd = true
            });
            
            return nodes;
        }
        
        /// <summary>
        /// 在层级图上搜索
        /// </summary>
        private List<TransitionNode> SearchLevelGraph(List<TransitionNode> nodes, 
            LevelCoord start, LevelCoord end)
        {
            // 使用 Dijkstra 或 A* 在层级图上搜索
            var openSet = new SortedSet<LevelSearchNode>(new LevelSearchNodeComparer());
            var closedSet = new HashSet<LevelCoord>();
            var cameFrom = new Dictionary<LevelCoord, TransitionNode>();
            var gScore = new Dictionary<LevelCoord, float>();
            
            var startNode = new LevelSearchNode
            {
                Node = nodes[0],
                G = 0,
                H = EstimateLevelDistance(start, end)
            };
            
            openSet.Add(startNode);
            gScore[start] = 0;
            
            while (openSet.Count > 0)
            {
                var current = openSet.Min;
                openSet.Remove(current);
                
                if (current.Node.IsEnd)
                {
                    // 找到终点，重建路径
                    return ReconstructLevelPath(cameFrom, current.Node, start);
                }
                
                closedSet.Add(current.Node.Coord);
                
                // 检查可达的下一个节点
                foreach (var node in nodes)
                {
                    if (closedSet.Contains(node.Coord)) continue;
                    
                    // 检查是否可达
                    float? pathCost = GetPathCost(current.Node.Coord, node.Coord);
                    if (!pathCost.HasValue) continue;
                    
                    float tentativeG = current.G + pathCost.Value;
                    
                    if (!gScore.ContainsKey(node.Coord) || tentativeG < gScore[node.Coord])
                    {
                        cameFrom[node.Coord] = current.Node;
                        gScore[node.Coord] = tentativeG;
                        
                        var searchNode = new LevelSearchNode
                        {
                            Node = node,
                            G = tentativeG,
                            H = EstimateLevelDistance(node.Coord, end)
                        };
                        
                        openSet.Add(searchNode);
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 估计层级距离
        /// </summary>
        private float EstimateLevelDistance(LevelCoord from, LevelCoord to)
        {
            // 水平距离 + 垂直层级距离
            float horizontal = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
            float vertical = Mathf.Abs(from.z - to.z) * _transitionCost;
            return horizontal + vertical;
        }
        
        /// <summary>
        /// 获取两点之间的路径消耗
        /// </summary>
        private float? GetPathCost(LevelCoord from, LevelCoord to)
        {
            // 同层：直接寻路
            if (from.z == to.z)
            {
                if (!_pathfinders.TryGetValue(from.z, out var pathfinder))
                {
                    return null;
                }
                
                var result = pathfinder.FindPath(from.ToTileCoord(), to.ToTileCoord());
                if (result.Success)
                {
                    return result.TotalCost;
                }
                return null;
            }
            
            // 不同层：检查是否通过转换点连接
            foreach (var transition in _map.Transitions)
            {
                if (transition.From == from && transition.To == to)
                {
                    return _transitionCost;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 重建层级路径
        /// </summary>
        private List<TransitionNode> ReconstructLevelPath(
            Dictionary<LevelCoord, TransitionNode> cameFrom,
            TransitionNode endNode,
            LevelCoord start)
        {
            var path = new List<TransitionNode> { endNode };
            var current = endNode.Coord;
            
            while (cameFrom.ContainsKey(current))
            {
                var node = cameFrom[current];
                path.Add(node);
                current = node.Coord;
                
                if (current == start) break;
            }
            
            path.Reverse();
            return path;
        }
        
        /// <summary>
        /// 构建详细路径
        /// </summary>
        private MultiLevelPathResult BuildDetailedPath(List<TransitionNode> levelPath,
            LevelCoord start, LevelCoord end)
        {
            var result = new MultiLevelPathResult
            {
                Success = true
            };
            
            LevelCoord currentPos = start;
            
            for (int i = 0; i < levelPath.Count; i++)
            {
                var node = levelPath[i];
                
                // 在当前层寻路到节点位置
                if (currentPos.z == node.Coord.z && currentPos != node.Coord)
                {
                    if (_pathfinders.TryGetValue(currentPos.z, out var pathfinder))
                    {
                        var pathResult = pathfinder.FindPath(
                            currentPos.ToTileCoord(), 
                            node.Coord.ToTileCoord()
                        );
                        
                        if (pathResult.Success)
                        {
                            var segment = new PathSegment(currentPos.z)
                            {
                                Path = pathResult.Path,
                                Cost = pathResult.TotalCost,
                                EndTransition = node.Transition
                            };
                            
                            result.Segments.Add(segment);
                            result.TotalCost += pathResult.TotalCost;
                        }
                    }
                }
                
                // 如果有转换，记录转换消耗
                if (node.Transition != null)
                {
                    result.TotalCost += _transitionCost;
                    result.TransitionCount++;
                    currentPos = node.Transition.To;
                }
                else
                {
                    currentPos = node.Coord;
                }
            }
            
            return result;
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 检查位置是否可行走
        /// </summary>
        public bool IsWalkable(LevelCoord coord)
        {
            var level = _map.GetLevel(coord.z);
            return level?.IsWalkable(coord.ToTileCoord()) ?? false;
        }
        
        /// <summary>
        /// 获取指定位置的转换点
        /// </summary>
        public LevelTransition GetTransitionAt(LevelCoord coord)
        {
            return _map.GetTransitionAt(coord);
        }
        
        /// <summary>
        /// 设置转换消耗
        /// </summary>
        public void SetTransitionCost(float cost)
        {
            _transitionCost = Mathf.Max(0, cost);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 转换节点（用于层级图搜索）
    /// </summary>
    internal class TransitionNode
    {
        public LevelCoord Coord;
        public LevelTransition Transition;
        public bool IsStart;
        public bool IsEnd;
    }
    
    /// <summary>
    /// 层级搜索节点
    /// </summary>
    internal class LevelSearchNode
    {
        public TransitionNode Node;
        public float G;
        public float H;
        public float F => G + H;
    }
    
    /// <summary>
    /// 层级搜索节点比较器
    /// </summary>
    internal class LevelSearchNodeComparer : IComparer<LevelSearchNode>
    {
        public int Compare(LevelSearchNode x, LevelSearchNode y)
        {
            int compare = x.F.CompareTo(y.F);
            if (compare == 0)
            {
                compare = x.H.CompareTo(y.H);
            }
            if (compare == 0)
            {
                // 使用坐标作为唯一标识
                compare = x.Node.Coord.x.CompareTo(y.Node.Coord.x);
                if (compare == 0)
                {
                    compare = x.Node.Coord.y.CompareTo(y.Node.Coord.y);
                    if (compare == 0)
                    {
                        compare = x.Node.Coord.z.CompareTo(y.Node.Coord.z);
                    }
                }
            }
            return compare;
        }
    }
}
