using System.Collections.Generic;
using Core.Game.Pawn.Define;
using UnityEngine;

namespace Core.Game.Pawn.Data
{
    /// <summary>
    /// 单个Pawn的完整数据
    /// </summary>
    public class PawnData
    {
        // 标识
        public long PawnId;
        public int PawnDefId;
        public string Name;

        // 位置
        public int X;
        public int Y;
        public int Floor;

        // 状态
        public EPawnState State;
        public NeedData Needs;

        // 移动
        public float MoveSpeed;
        public List<Vector2Int> CurrentPath;
        public int PathIndex;
        public float MoveProgress; // 0~1, 当前格到下一格的进度

        // AI
        public float StateTimer;

        /// <summary>
        /// 是否正在移动中
        /// </summary>
        public bool IsMoving => State == EPawnState.Moving &&
                                CurrentPath != null &&
                                PathIndex < CurrentPath.Count;

        /// <summary>
        /// 当前路径的下一个目标格
        /// </summary>
        public Vector2Int? NextPathTarget
        {
            get
            {
                if (CurrentPath == null || PathIndex >= CurrentPath.Count)
                    return null;
                return CurrentPath[PathIndex];
            }
        }

        public void ClearPath()
        {
            CurrentPath = null;
            PathIndex = 0;
            MoveProgress = 0f;
        }
    }
}
