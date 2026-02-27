using System.Collections.Generic;
using Core.Game.Map.System;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Event;
using Core.Game.Pawn.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn移动系统: 寻路 + 逐格移动
    /// </summary>
    public class PawnMovementSystem : AbstractSystem
    {
        private PawnDataModel _pawnModel;
        private MapSystem _mapSystem;

        protected override void OnInit()
        {
            _pawnModel = this.GetModel<PawnDataModel>();
            _mapSystem = this.GetSystem<MapSystem>();
        }

        /// <summary>
        /// 请求移动到目标位置
        /// </summary>
        public bool RequestMove(long pawnId, int targetX, int targetY)
        {
            var pawn = _pawnModel.GetPawn(pawnId);
            if (pawn == null) return false;

            // 已经在目标位置
            if (pawn.X == targetX && pawn.Y == targetY) return false;

            // 目标不可行走
            if (!_mapSystem.IsCellWalkable(targetX, targetY, pawn.Floor))
                return false;

            // 寻路
            var astarPath = _mapSystem.FindPath(
                pawn.X, pawn.Y, targetX, targetY,
                pawn.Floor, PawnConst.PathSearchRadius);

            if (astarPath == null || astarPath.Count < 2)
                return false;

            // 转换AstarPosVo为Vector2Int路径 (跳过起点)
            var path = new List<Vector2Int>();
            for (int i = 1; i < astarPath.Count; i++)
            {
                path.Add(new Vector2Int(astarPath[i].x, astarPath[i].y));
            }

            pawn.CurrentPath = path;
            pawn.PathIndex = 0;
            pawn.MoveProgress = 0f;
            pawn.State = EPawnState.Moving;

            return true;
        }

        /// <summary>
        /// 每帧更新所有移动中的Pawn
        /// </summary>
        public void Tick(float deltaTime)
        {
            foreach (var pawn in _pawnModel.GetAllPawns().Values)
            {
                if (pawn.State != EPawnState.Moving) continue;
                if (pawn.CurrentPath == null || pawn.PathIndex >= pawn.CurrentPath.Count)
                {
                    FinishMovement(pawn);
                    continue;
                }

                UpdatePawnMovement(pawn, deltaTime);
            }
        }

        private void UpdatePawnMovement(PawnData pawn, float deltaTime)
        {
            var target = pawn.CurrentPath[pawn.PathIndex];
            int dx = target.x - pawn.X;
            int dy = target.y - pawn.Y;
            bool isDiagonal = dx != 0 && dy != 0;

            // 对角线移动代价更高
            float moveCost = isDiagonal ? PawnConst.DiagonalMultiplier : 1f;
            pawn.MoveProgress += pawn.MoveSpeed * deltaTime / moveCost;

            if (pawn.MoveProgress >= 1f)
            {
                // 到达下一格
                ArriveAtNextTile(pawn, target);
            }
        }

        private void ArriveAtNextTile(PawnData pawn, Vector2Int target)
        {
            // 检查墙壁阻挡
            if (_mapSystem.IsWallBlocking(pawn.X, pawn.Y, target.x, target.y, pawn.Floor))
            {
                LogKit.Log($"Pawn {pawn.Name}: 被墙壁阻挡 ({pawn.X},{pawn.Y})→({target.x},{target.y})");
                FinishMovement(pawn);
                return;
            }

            // 检查目标格可行走
            if (!_mapSystem.IsCellWalkable(target.x, target.y, pawn.Floor))
            {
                FinishMovement(pawn);
                return;
            }

            int oldX = pawn.X;
            int oldY = pawn.Y;

            // 更新地图位置注册
            _mapSystem.RemoveObject(oldX, oldY, pawn.Floor, pawn.PawnId);
            _mapSystem.PlaceObject(target.x, target.y, pawn.Floor, pawn.PawnId);

            // 更新Pawn位置
            pawn.X = target.x;
            pawn.Y = target.y;
            pawn.MoveProgress = 0f;
            pawn.PathIndex++;

            // 发送移动事件
            this.SendEvent(new SPawnMovedEvent
            {
                PawnId = pawn.PawnId,
                OldX = oldX, OldY = oldY,
                NewX = target.x, NewY = target.y,
                Floor = pawn.Floor
            });

            // 路径走完
            if (pawn.PathIndex >= pawn.CurrentPath.Count)
            {
                FinishMovement(pawn);
            }
        }

        private void FinishMovement(PawnData pawn)
        {
            var oldState = pawn.State;
            pawn.ClearPath();
            pawn.State = EPawnState.Idle;
            pawn.StateTimer = 0f;

            if (oldState != EPawnState.Idle)
            {
                this.SendEvent(new SPawnStateChangedEvent
                {
                    PawnId = pawn.PawnId,
                    OldState = oldState,
                    NewState = EPawnState.Idle
                });
            }
        }
    }
}
