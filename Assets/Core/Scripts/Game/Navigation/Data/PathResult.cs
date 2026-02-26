using System.Collections.Generic;
using Core.Game.Navigation.Define;
using UnityEngine;

namespace Core.Game.Navigation.Data
{
    /// <summary>
    /// 寻路结果
    /// </summary>
    public class PathResult
    {
        /// <summary>
        /// 寻路状态
        /// </summary>
        public EPathStatus Status;

        /// <summary>
        /// 路径点列表（从起点到终点，包含楼层信息）
        /// x=格子X, y=格子Y, z=楼层
        /// </summary>
        public List<Vector3Int> Path;

        /// <summary>
        /// 路径总代价
        /// </summary>
        public int TotalCost;

        /// <summary>
        /// 寻路展开的节点数
        /// </summary>
        public int NodesExpanded;

        /// <summary>
        /// 请求者ID
        /// </summary>
        public int RequesterId;

        /// <summary>
        /// 是否寻路成功
        /// </summary>
        public bool IsSuccess => Status == EPathStatus.Success;

        /// <summary>
        /// 路径长度（步数）
        /// </summary>
        public int PathLength => Path?.Count ?? 0;

        public PathResult()
        {
            Path = new List<Vector3Int>();
        }

        public PathResult(EPathStatus status) : this()
        {
            Status = status;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static PathResult Success(List<Vector3Int> path, int totalCost, int nodesExpanded, int requesterId)
        {
            return new PathResult
            {
                Status = EPathStatus.Success,
                Path = path,
                TotalCost = totalCost,
                NodesExpanded = nodesExpanded,
                RequesterId = requesterId
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static PathResult Fail(EPathStatus status, int nodesExpanded = 0, int requesterId = -1)
        {
            return new PathResult
            {
                Status = status,
                NodesExpanded = nodesExpanded,
                RequesterId = requesterId
            };
        }
    }
}
