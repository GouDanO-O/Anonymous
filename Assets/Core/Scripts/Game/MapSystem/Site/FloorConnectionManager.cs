/*******************************************************************************
 * 文件名:    FloorConnectionManager.cs
 * 描述:      楼层连接管理器，管理楼梯、电梯等垂直连接
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   FloorConnectionManager 管理场景中的所有楼层连接器：
 *   - 楼梯、梯子、电梯、洞口等
 *   - 支持跨楼层寻路查询
 *   - 维护连接图结构
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// 楼层连接管理器
    /// </summary>
    public class FloorConnectionManager
    {
        #region 字段

        /// <summary>
        /// 所属Site
        /// </summary>
        private Site _parentSite;

        /// <summary>
        /// 所有连接器
        /// </summary>
        private List<FloorConnector> _allConnectors;

        /// <summary>
        /// 按楼层索引的连接器
        /// </summary>
        private Dictionary<int, List<FloorConnector>> _connectorsByFloor;

        /// <summary>
        /// 按位置快速查找（全局坐标 -> 连接器）
        /// </summary>
        private Dictionary<GlobalCoord, FloorConnector> _connectorsByPosition;

        /// <summary>
        /// 连接器ID计数器
        /// </summary>
        private int _nextConnectorId;

        /// <summary>
        /// 连接图是否需要重建
        /// </summary>
        private bool _graphDirty;

        #endregion

        #region 属性

        /// <summary>
        /// 所属Site
        /// </summary>
        public Site ParentSite => _parentSite;

        /// <summary>
        /// 所有连接器
        /// </summary>
        public IReadOnlyList<FloorConnector> AllConnectors => _allConnectors;

        /// <summary>
        /// 连接器数量
        /// </summary>
        public int ConnectorCount => _allConnectors.Count;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public FloorConnectionManager(Site parentSite)
        {
            _parentSite = parentSite;
            _allConnectors = new List<FloorConnector>();
            _connectorsByFloor = new Dictionary<int, List<FloorConnector>>();
            _connectorsByPosition = new Dictionary<GlobalCoord, FloorConnector>();
            _nextConnectorId = 1;
            _graphDirty = true;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 为每个楼层创建列表
            for (int floor = _parentSite.MinFloor; floor <= _parentSite.MaxFloor; floor++)
            {
                _connectorsByFloor[floor] = new List<FloorConnector>();
            }

            _graphDirty = true;
        }

        #endregion

        #region 连接器注册

        /// <summary>
        /// 注册连接器
        /// </summary>
        public void RegisterConnector(FloorConnector connector)
        {
            if (connector == null)
                return;

            // 分配ID
            if (connector.ConnectorId <= 0)
            {
                connector.ConnectorId = _nextConnectorId++;
            }

            // 添加到总列表
            _allConnectors.Add(connector);

            // 添加到楼层索引
            if (_connectorsByFloor.TryGetValue(connector.LowerFloorIndex, out var lowerList))
            {
                lowerList.Add(connector);
            }
            if (_connectorsByFloor.TryGetValue(connector.UpperFloorIndex, out var upperList))
            {
                if (connector.LowerFloorIndex != connector.UpperFloorIndex)
                {
                    upperList.Add(connector);
                }
            }

            // 添加到位置索引
            var lowerPos = new GlobalCoord(connector.LowerPosition, connector.LowerFloorIndex);
            var upperPos = new GlobalCoord(connector.UpperPosition, connector.UpperFloorIndex);
            
            _connectorsByPosition[lowerPos] = connector;
            if (lowerPos != upperPos)
            {
                _connectorsByPosition[upperPos] = connector;
            }

            _graphDirty = true;
        }

        /// <summary>
        /// 注销连接器
        /// </summary>
        public void UnregisterConnector(FloorConnector connector)
        {
            if (connector == null)
                return;

            _allConnectors.Remove(connector);

            // 从楼层索引移除
            if (_connectorsByFloor.TryGetValue(connector.LowerFloorIndex, out var lowerList))
            {
                lowerList.Remove(connector);
            }
            if (_connectorsByFloor.TryGetValue(connector.UpperFloorIndex, out var upperList))
            {
                upperList.Remove(connector);
            }

            // 从位置索引移除
            var lowerPos = new GlobalCoord(connector.LowerPosition, connector.LowerFloorIndex);
            var upperPos = new GlobalCoord(connector.UpperPosition, connector.UpperFloorIndex);
            
            _connectorsByPosition.Remove(lowerPos);
            _connectorsByPosition.Remove(upperPos);

            _graphDirty = true;
        }

        /// <summary>
        /// 注销连接器（通过ID）
        /// </summary>
        public void UnregisterConnector(int connectorId)
        {
            var connector = _allConnectors.FirstOrDefault(c => c.ConnectorId == connectorId);
            if (connector != null)
            {
                UnregisterConnector(connector);
            }
        }

        /// <summary>
        /// 清空所有连接器
        /// </summary>
        public void Clear()
        {
            _allConnectors.Clear();
            foreach (var list in _connectorsByFloor.Values)
            {
                list.Clear();
            }
            _connectorsByPosition.Clear();
            _graphDirty = true;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取指定楼层的所有连接器
        /// </summary>
        public IReadOnlyList<FloorConnector> GetConnectorsOnFloor(int floorIndex)
        {
            if (_connectorsByFloor.TryGetValue(floorIndex, out var list))
            {
                return list;
            }
            return Array.Empty<FloorConnector>();
        }

        /// <summary>
        /// 获取指定位置的连接器
        /// </summary>
        public FloorConnector GetConnectorAt(GlobalCoord position)
        {
            _connectorsByPosition.TryGetValue(position, out var connector);
            return connector;
        }

        /// <summary>
        /// 获取指定位置的连接器
        /// </summary>
        public FloorConnector GetConnectorAt(CellCoord cell, int floorIndex)
        {
            return GetConnectorAt(new GlobalCoord(cell, floorIndex));
        }

        /// <summary>
        /// 检查位置是否有连接器
        /// </summary>
        public bool HasConnectorAt(GlobalCoord position)
        {
            return _connectorsByPosition.ContainsKey(position);
        }

        /// <summary>
        /// 获取连接器（通过ID）
        /// </summary>
        public FloorConnector GetConnectorById(int connectorId)
        {
            return _allConnectors.FirstOrDefault(c => c.ConnectorId == connectorId);
        }

        /// <summary>
        /// 获取指定类型的所有连接器
        /// </summary>
        public IEnumerable<FloorConnector> GetConnectorsByType(FloorConnectorType type)
        {
            return _allConnectors.Where(c => c.ConnectorType == type);
        }

        /// <summary>
        /// 获取连接两个楼层的连接器
        /// </summary>
        public IEnumerable<FloorConnector> GetConnectorsBetweenFloors(int floor1, int floor2)
        {
            int lower = Mathf.Min(floor1, floor2);
            int upper = Mathf.Max(floor1, floor2);

            return _allConnectors.Where(c => 
                c.LowerFloorIndex == lower && c.UpperFloorIndex == upper);
        }

        /// <summary>
        /// 查找从指定位置可达的连接器
        /// </summary>
        /// <param name="position">起始位置</param>
        /// <param name="maxDistance">最大搜索距离</param>
        public IEnumerable<FloorConnector> FindReachableConnectors(GlobalCoord position, int maxDistance = 50)
        {
            var floor = _parentSite.GetFloor(position.y);
            if (floor == null)
                yield break;

            var connectors = GetConnectorsOnFloor(position.y);
            foreach (var connector in connectors)
            {
                // 计算到连接器入口的距离
                CellCoord entryPoint = position.y == connector.LowerFloorIndex 
                    ? connector.LowerPosition 
                    : connector.UpperPosition;

                int distance = position.ToCellCoord().ManhattanDistance(entryPoint);
                if (distance <= maxDistance)
                {
                    yield return connector;
                }
            }
        }

        #endregion

        #region 连通性检查

        /// <summary>
        /// 检查两个楼层是否连通
        /// </summary>
        public bool AreFloorsConnected(int floor1, int floor2)
        {
            if (floor1 == floor2)
                return true;

            // BFS搜索楼层连通性
            var visited = new HashSet<int> { floor1 };
            var queue = new Queue<int>();
            queue.Enqueue(floor1);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                // 获取当前楼层的所有连接器
                var connectors = GetConnectorsOnFloor(current);
                foreach (var connector in connectors)
                {
                    if (!connector.IsPassable)
                        continue;

                    // 获取连接的另一楼层
                    int otherFloor = connector.LowerFloorIndex == current 
                        ? connector.UpperFloorIndex 
                        : connector.LowerFloorIndex;

                    if (otherFloor == floor2)
                        return true;

                    if (!visited.Contains(otherFloor))
                    {
                        visited.Add(otherFloor);
                        queue.Enqueue(otherFloor);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取两个楼层之间的最短路径（楼层序列）
        /// </summary>
        public List<int> FindFloorPath(int fromFloor, int toFloor)
        {
            if (fromFloor == toFloor)
                return new List<int> { fromFloor };

            // BFS搜索
            var visited = new Dictionary<int, int> { { fromFloor, -1 } }; // floor -> 前驱楼层
            var queue = new Queue<int>();
            queue.Enqueue(fromFloor);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                var connectors = GetConnectorsOnFloor(current);
                foreach (var connector in connectors)
                {
                    if (!connector.IsPassable)
                        continue;

                    int otherFloor = connector.LowerFloorIndex == current 
                        ? connector.UpperFloorIndex 
                        : connector.LowerFloorIndex;

                    if (!visited.ContainsKey(otherFloor))
                    {
                        visited[otherFloor] = current;

                        if (otherFloor == toFloor)
                        {
                            // 重建路径
                            var path = new List<int>();
                            int floor = toFloor;
                            while (floor != -1)
                            {
                                path.Add(floor);
                                floor = visited[floor];
                            }
                            path.Reverse();
                            return path;
                        }

                        queue.Enqueue(otherFloor);
                    }
                }
            }

            return null; // 不连通
        }

        /// <summary>
        /// 检查从一个位置是否可以到达另一个位置（跨楼层）
        /// </summary>
        public bool CanReach(GlobalCoord from, GlobalCoord to)
        {
            // 同楼层
            if (from.y == to.y)
            {
                // TODO: 需要调用寻路系统检查
                return true;
            }

            // 不同楼层，检查连通性
            return AreFloorsConnected(from.y, to.y);
        }

        #endregion

        #region 创建连接器

        /// <summary>
        /// 创建楼梯连接器
        /// </summary>
        public FloorConnector CreateStairs(CellCoord position, int lowerFloor, 
            IntVec2 size, Rotation rotation)
        {
            var connector = new FloorConnector
            {
                ConnectorType = FloorConnectorType.Stair,
                LowerFloorIndex = lowerFloor,
                UpperFloorIndex = lowerFloor + 1,
                LowerPosition = position,
                UpperPosition = position, // 简化处理，实际可能根据旋转计算
                Size = size,
                Rotation = rotation,
                TraverseCost = 15,
                IsBidirectional = true,
                IsPassable = true
            };

            RegisterConnector(connector);
            return connector;
        }

        /// <summary>
        /// 创建梯子连接器
        /// </summary>
        public FloorConnector CreateLadder(CellCoord position, int lowerFloor)
        {
            var connector = new FloorConnector
            {
                ConnectorType = FloorConnectorType.Ladder,
                LowerFloorIndex = lowerFloor,
                UpperFloorIndex = lowerFloor + 1,
                LowerPosition = position,
                UpperPosition = position,
                Size = new IntVec2(1, 1),
                TraverseCost = 20,
                IsBidirectional = true,
                IsPassable = true
            };

            RegisterConnector(connector);
            return connector;
        }

        /// <summary>
        /// 创建电梯连接器
        /// </summary>
        public FloorConnector CreateElevator(CellCoord position, int bottomFloor, 
            int topFloor, IntVec2 size)
        {
            var connector = new FloorConnector
            {
                ConnectorType = FloorConnectorType.Elevator,
                LowerFloorIndex = bottomFloor,
                UpperFloorIndex = topFloor,
                LowerPosition = position,
                UpperPosition = position,
                Size = size,
                TraverseCost = 5,
                IsBidirectional = true,
                IsPassable = true,
                RequiresPower = true
            };

            RegisterConnector(connector);
            return connector;
        }

        /// <summary>
        /// 创建洞口连接器
        /// </summary>
        public FloorConnector CreateHole(CellCoord position, int upperFloor)
        {
            var connector = new FloorConnector
            {
                ConnectorType = FloorConnectorType.Hole,
                LowerFloorIndex = upperFloor - 1,
                UpperFloorIndex = upperFloor,
                LowerPosition = position,
                UpperPosition = position,
                Size = new IntVec2(1, 1),
                TraverseCost = 5, // 掉下去很快
                IsBidirectional = false, // 单向（只能从上往下）
                IsPassable = true
            };

            RegisterConnector(connector);
            return connector;
        }

        #endregion

        #region 连接图更新

        /// <summary>
        /// 标记连接图需要重建
        /// </summary>
        public void MarkGraphDirty()
        {
            _graphDirty = true;
        }

        /// <summary>
        /// 重建连接图（如果需要）
        /// </summary>
        public void RebuildGraphIfNeeded()
        {
            if (!_graphDirty)
                return;

            // TODO: 重建用于寻路的连接图结构

            _graphDirty = false;
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印连接器信息
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log($"=== FloorConnectionManager ===");
            Debug.Log($"  Total Connectors: {_allConnectors.Count}");

            foreach (var connector in _allConnectors)
            {
                Debug.Log($"  - {connector}");
            }

            // 打印每层的连接器数量
            foreach (var kvp in _connectorsByFloor)
            {
                if (kvp.Value.Count > 0)
                {
                    Debug.Log($"  Floor {kvp.Key}: {kvp.Value.Count} connectors");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 楼层连接器数据
    /// </summary>
    [Serializable]
    public class FloorConnector
    {
        /// <summary>
        /// 连接器ID
        /// </summary>
        public int ConnectorId;

        /// <summary>
        /// 连接器类型
        /// </summary>
        public FloorConnectorType ConnectorType;

        /// <summary>
        /// 下层楼层索引
        /// </summary>
        public int LowerFloorIndex;

        /// <summary>
        /// 上层楼层索引
        /// </summary>
        public int UpperFloorIndex;

        /// <summary>
        /// 下层入口位置
        /// </summary>
        public CellCoord LowerPosition;

        /// <summary>
        /// 上层入口位置
        /// </summary>
        public CellCoord UpperPosition;

        /// <summary>
        /// 占据尺寸
        /// </summary>
        public IntVec2 Size = new IntVec2(1, 1);

        /// <summary>
        /// 旋转
        /// </summary>
        public Rotation Rotation;

        /// <summary>
        /// 通过代价
        /// </summary>
        public int TraverseCost = 10;

        /// <summary>
        /// 是否双向
        /// </summary>
        public bool IsBidirectional = true;

        /// <summary>
        /// 是否可通行
        /// </summary>
        public bool IsPassable = true;

        /// <summary>
        /// 是否需要电力
        /// </summary>
        public bool RequiresPower;

        /// <summary>
        /// 是否有电力（运行时状态）
        /// </summary>
        public bool HasPower = true;

        /// <summary>
        /// 关联的实体ID
        /// </summary>
        public int LinkedEntityId;

        /// <summary>
        /// 连接的楼层数
        /// </summary>
        public int FloorSpan => Mathf.Abs(UpperFloorIndex - LowerFloorIndex);

        /// <summary>
        /// 是否可以使用
        /// </summary>
        public bool CanUse => IsPassable && (!RequiresPower || HasPower);

        /// <summary>
        /// 检查是否可以从指定楼层进入
        /// </summary>
        public bool CanEnterFrom(int floorIndex)
        {
            if (!CanUse)
                return false;

            if (floorIndex == LowerFloorIndex)
                return true;

            if (floorIndex == UpperFloorIndex && IsBidirectional)
                return true;

            // 电梯可能连接多层
            if (ConnectorType == FloorConnectorType.Elevator)
            {
                return floorIndex >= LowerFloorIndex && floorIndex <= UpperFloorIndex;
            }

            return false;
        }

        /// <summary>
        /// 获取从指定楼层出发可到达的楼层
        /// </summary>
        public IEnumerable<int> GetReachableFloors(int fromFloor)
        {
            if (!CanUse)
                yield break;

            if (ConnectorType == FloorConnectorType.Elevator)
            {
                // 电梯可以到达范围内的任何楼层
                for (int f = LowerFloorIndex; f <= UpperFloorIndex; f++)
                {
                    if (f != fromFloor)
                        yield return f;
                }
            }
            else
            {
                // 普通连接器
                if (fromFloor == LowerFloorIndex)
                {
                    yield return UpperFloorIndex;
                }
                else if (fromFloor == UpperFloorIndex && IsBidirectional)
                {
                    yield return LowerFloorIndex;
                }
            }
        }

        /// <summary>
        /// 获取入口位置
        /// </summary>
        public CellCoord GetEntryPosition(int fromFloor)
        {
            return fromFloor == LowerFloorIndex ? LowerPosition : UpperPosition;
        }

        /// <summary>
        /// 获取出口位置
        /// </summary>
        public CellCoord GetExitPosition(int toFloor)
        {
            return toFloor == LowerFloorIndex ? LowerPosition : UpperPosition;
        }

        public override string ToString()
        {
            string typeStr = ConnectorType.ToString();
            string biDir = IsBidirectional ? "↕" : "↓";
            return $"Connector[{ConnectorId}] {typeStr} F{LowerFloorIndex}{biDir}F{UpperFloorIndex} at {LowerPosition}";
        }
    }
}
