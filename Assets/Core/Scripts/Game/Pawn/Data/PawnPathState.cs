using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Pawn.Data
{
    /// <summary>
    /// Pawn移动路径状态
    /// </summary>
    [Serializable]
    public class PawnPathState
    {
        /// <summary>
        /// 当前路径点列表（x=格子X, y=格子Y, z=楼层）
        /// </summary>
        public List<Vector3Int> Path;

        /// <summary>
        /// 当前路径索引（走到第几步）
        /// </summary>
        public int CurrentIndex;

        /// <summary>
        /// 当前步的移动进度（0~1）
        /// </summary>
        public float MoveProgress;

        /// <summary>
        /// 是否正在等待寻路结果
        /// </summary>
        public bool IsWaitingForPath;

        /// <summary>
        /// 最终目标位置
        /// </summary>
        public Vector3Int TargetPosition;

        /// <summary>
        /// 是否有有效路径
        /// </summary>
        public bool HasPath => Path != null && Path.Count > 0 && CurrentIndex < Path.Count;

        /// <summary>
        /// 是否已到达路径终点
        /// </summary>
        public bool IsPathComplete => Path != null && Path.Count > 0 && CurrentIndex >= Path.Count - 1;

        /// <summary>
        /// 当前路径点
        /// </summary>
        public Vector3Int CurrentWaypoint => HasPath ? Path[CurrentIndex] : default;

        /// <summary>
        /// 下一个路径点
        /// </summary>
        public Vector3Int NextWaypoint
        {
            get
            {
                if (Path == null || CurrentIndex + 1 >= Path.Count)
                    return default;
                return Path[CurrentIndex + 1];
            }
        }

        /// <summary>
        /// 路径剩余步数
        /// </summary>
        public int RemainingSteps => HasPath ? Path.Count - 1 - CurrentIndex : 0;

        public PawnPathState()
        {
            Path = null;
            CurrentIndex = 0;
            MoveProgress = 0f;
            IsWaitingForPath = false;
        }

        /// <summary>
        /// 设置新路径
        /// </summary>
        public void SetPath(List<Vector3Int> path, Vector3Int target)
        {
            Path = path;
            CurrentIndex = 0;
            MoveProgress = 0f;
            IsWaitingForPath = false;
            TargetPosition = target;
        }

        /// <summary>
        /// 推进到下一个路径点
        /// </summary>
        public void AdvanceToNext()
        {
            CurrentIndex++;
            MoveProgress = 0f;
        }

        /// <summary>
        /// 清空路径
        /// </summary>
        public void ClearPath()
        {
            Path = null;
            CurrentIndex = 0;
            MoveProgress = 0f;
            IsWaitingForPath = false;
        }
    }
}
