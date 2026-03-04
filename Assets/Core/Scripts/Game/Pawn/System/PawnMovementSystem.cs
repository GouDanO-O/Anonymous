using System.Collections.Generic;
using Core.Game.Map.System;
using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Event;
using Core.Game.Pawn.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn移动系统: 寻路 + 逐格移动 + 跨层楼梯过渡
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
        /// 请求移动到目标位置 (支持跨层)
        /// targetFloor = -1 表示同层移动
        /// </summary>
        public bool RequestMove(long pawnId, int targetX, int targetY, int targetFloor = -1)
        {
            var pawn = _pawnModel.GetPawn(pawnId);
            if (pawn == null) return false;

            if (targetFloor < 0) targetFloor = pawn.Floor;

            // 已经在目标位置
            if (pawn.X == targetX && pawn.Y == targetY && pawn.Floor == targetFloor)
                return false;

            List<PathStep> path;

            if (pawn.Floor == targetFloor)
            {
                // 同层寻路
                if (!_mapSystem.IsCellWalkable(targetX, targetY, pawn.Floor))
                    return false;

                var astarPath = _mapSystem.FindPath(
                    pawn.X, pawn.Y, targetX, targetY,
                    pawn.Floor, PawnConst.PathSearchRadius);

                if (astarPath == null || astarPath.Count < 2)
                    return false;

                path = new List<PathStep>();
                for (int i = 1; i < astarPath.Count; i++)
                {
                    path.Add(new PathStep(astarPath[i].x, astarPath[i].y, pawn.Floor));
                }
            }
            else
            {
                // 跨层寻路
                path = _mapSystem.FindMultiFloorPath(
                    pawn.X, pawn.Y, pawn.Floor,
                    targetX, targetY, targetFloor,
                    PawnConst.PathSearchRadius);

                if (path == null || path.Count == 0)
                    return false;
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
            var step = pawn.CurrentPath[pawn.PathIndex];

            // 楼梯过渡: 立即切换楼层
            if (step.IsStairTransition)
            {
                HandleStairTransition(pawn, step);
                return;
            }

            int dx = step.X - pawn.X;
            int dy = step.Y - pawn.Y;
            bool isDiagonal = dx != 0 && dy != 0;

            float moveCost = isDiagonal ? PawnConst.DiagonalMultiplier : 1f;
            pawn.MoveProgress += pawn.MoveSpeed * deltaTime / moveCost;

            if (pawn.MoveProgress >= 1f)
            {
                ArriveAtNextTile(pawn, step);
            }
        }

        private void HandleStairTransition(PawnData pawn, PathStep step)
        {
            // 更新地图注册
            _mapSystem.RemoveObject(pawn.X, pawn.Y, pawn.Floor, pawn.PawnId);
            pawn.Floor = step.Floor;
            pawn.X = step.X;
            pawn.Y = step.Y;
            _mapSystem.PlaceObject(pawn.X, pawn.Y, pawn.Floor, pawn.PawnId);

            pawn.MoveProgress = 0f;
            pawn.PathIndex++;

            this.SendEvent(new SPawnMovedEvent
            {
                PawnId = pawn.PawnId,
                OldX = step.X, OldY = step.Y,
                NewX = step.X, NewY = step.Y,
                Floor = pawn.Floor
            });

            if (pawn.PathIndex >= pawn.CurrentPath.Count)
            {
                FinishMovement(pawn);
            }
        }

        private void ArriveAtNextTile(PawnData pawn, PathStep step)
        {
            // 检查墙壁阻挡
            if (_mapSystem.IsWallBlocking(pawn.X, pawn.Y, step.X, step.Y, pawn.Floor))
            {
                LogKit.Log($"Pawn {pawn.Name}: 被墙壁阻挡 ({pawn.X},{pawn.Y})→({step.X},{step.Y})");
                FinishMovement(pawn);
                return;
            }

            // 检查目标格可行走
            if (!_mapSystem.IsCellWalkable(step.X, step.Y, pawn.Floor))
            {
                FinishMovement(pawn);
                return;
            }

            int oldX = pawn.X;
            int oldY = pawn.Y;

            // 更新地图位置注册
            _mapSystem.RemoveObject(oldX, oldY, pawn.Floor, pawn.PawnId);
            _mapSystem.PlaceObject(step.X, step.Y, pawn.Floor, pawn.PawnId);

            pawn.X = step.X;
            pawn.Y = step.Y;
            pawn.MoveProgress = 0f;
            pawn.PathIndex++;

            this.SendEvent(new SPawnMovedEvent
            {
                PawnId = pawn.PawnId,
                OldX = oldX, OldY = oldY,
                NewX = step.X, NewY = step.Y,
                Floor = pawn.Floor
            });

            if (pawn.PathIndex >= pawn.CurrentPath.Count)
            {
                FinishMovement(pawn);
            }
        }

        private void FinishMovement(PawnData pawn)
        {
            var oldState = pawn.State;
            pawn.ClearPath();

            EPawnState newState = pawn.CurrentBlueprintId != 0
                ? EPawnState.Working
                : EPawnState.Idle;

            pawn.State = newState;
            pawn.StateTimer = 0f;

            if (oldState != newState)
            {
                this.SendEvent(new SPawnStateChangedEvent
                {
                    PawnId = pawn.PawnId,
                    OldState = oldState,
                    NewState = newState
                });
            }
        }
    }
}
