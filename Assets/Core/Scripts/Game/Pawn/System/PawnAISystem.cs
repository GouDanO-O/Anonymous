using System;
using Core.Game.Blueprint.Define;
using Core.Game.Blueprint.Model;
using Core.Game.Blueprint.System;
using Core.Game.Map.System;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Event;
using Core.Game.Pawn.Model;
using GDFrameworkCore;
using UnityEngine;
using Random = System.Random;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn AI系统: 状态机 (Idle→检查蓝图→Wander→Moving→Working→Idle循环)
    /// </summary>
    public class PawnAISystem : AbstractSystem
    {
        private PawnDataModel _pawnModel;
        private PawnMovementSystem _movementSystem;
        private MapSystem _mapSystem;
        private BlueprintDataModel _blueprintModel;
        private ConstructionSystem _constructionSystem;
        private Random _rng;

        protected override void OnInit()
        {
            _pawnModel = this.GetModel<PawnDataModel>();
            _movementSystem = this.GetSystem<PawnMovementSystem>();
            _mapSystem = this.GetSystem<MapSystem>();
            _blueprintModel = this.GetModel<BlueprintDataModel>();
            _constructionSystem = this.GetSystem<ConstructionSystem>();
            _rng = new Random();
        }

        public void Tick(float deltaTime)
        {
            foreach (var pawn in _pawnModel.GetAllPawns().Values)
            {
                switch (pawn.State)
                {
                    case EPawnState.Idle:
                        HandleIdle(pawn, deltaTime);
                        break;
                    case EPawnState.Wandering:
                        HandleWandering(pawn);
                        break;
                    case EPawnState.Working:
                        HandleWorking(pawn, deltaTime);
                        break;
                    // Moving状态由MovementSystem驱动, 完成后自动回到Idle
                }
            }
        }

        private void HandleIdle(Data.PawnData pawn, float deltaTime)
        {
            pawn.StateTimer += deltaTime;

            // 优先检查蓝图任务 (每帧检查, 不等待)
            if (TryAssignBlueprint(pawn))
                return;

            // 随机等待后开始漫游
            float waitTime = PawnConst.WanderIntervalMin +
                (float)_rng.NextDouble() * (PawnConst.WanderIntervalMax - PawnConst.WanderIntervalMin);

            if (pawn.StateTimer >= waitTime)
            {
                var oldState = pawn.State;
                pawn.State = EPawnState.Wandering;
                pawn.StateTimer = 0f;

                this.SendEvent(new SPawnStateChangedEvent
                {
                    PawnId = pawn.PawnId,
                    OldState = oldState,
                    NewState = EPawnState.Wandering
                });
            }
        }

        private void HandleWandering(Data.PawnData pawn)
        {
            // 在半径内随机选一个可行走的目标
            int radius = PawnConst.WanderRadius;
            int attempts = 10;

            for (int i = 0; i < attempts; i++)
            {
                int targetX = pawn.X + _rng.Next(-radius, radius + 1);
                int targetY = pawn.Y + _rng.Next(-radius, radius + 1);

                if (targetX == pawn.X && targetY == pawn.Y) continue;

                if (!_mapSystem.IsValidPosition(targetX, targetY, pawn.Floor)) continue;
                if (!_mapSystem.IsCellWalkable(targetX, targetY, pawn.Floor)) continue;

                if (_movementSystem.RequestMove(pawn.PawnId, targetX, targetY))
                    return; // RequestMove内部已将State设为Moving
            }

            // 找不到有效目标, 回到Idle重新等待
            pawn.State = EPawnState.Idle;
            pawn.StateTimer = 0f;
        }

        private void HandleWorking(Data.PawnData pawn, float deltaTime)
        {
            if (pawn.CurrentBlueprintId == 0)
            {
                // 没有蓝图任务, 回到Idle
                pawn.State = EPawnState.Idle;
                pawn.StateTimer = 0f;
                return;
            }

            var bp = _blueprintModel.GetBlueprint(pawn.CurrentBlueprintId);
            if (bp == null || bp.State == EBlueprintState.Cancelled || bp.State == EBlueprintState.Complete)
            {
                // 蓝图被取消或已完成
                pawn.CurrentBlueprintId = 0;
                pawn.State = EPawnState.Idle;
                pawn.StateTimer = 0f;
                return;
            }

            // 检查是否在蓝图旁边 (曼哈顿距离 <= 1)
            int dist = Math.Abs(pawn.X - bp.X) + Math.Abs(pawn.Y - bp.Y);
            if (dist > 1)
            {
                // 还没到, 重新移动过去
                if (!MoveToBlueprint(pawn, bp))
                {
                    // 无法到达, 放弃
                    _constructionSystem.UnassignPawn(pawn.CurrentBlueprintId);
                    pawn.CurrentBlueprintId = 0;
                    pawn.State = EPawnState.Idle;
                    pawn.StateTimer = 0f;
                }
                return;
            }

            // 在蓝图旁, 推进工作量
            bool isDemolish = bp.Type == EBlueprintType.Demolish ||
                              bp.Type == EBlueprintType.Disassemble;
            float speed = isDemolish ? PawnConst.DemolishSpeed : PawnConst.ConstructionSpeed;

            _constructionSystem.AddWork(pawn.CurrentBlueprintId, speed * deltaTime);

            // 检查是否完成 (AddWork内部会处理完成逻辑并移除蓝图)
            var bpAfter = _blueprintModel.GetBlueprint(pawn.CurrentBlueprintId);
            if (bpAfter == null || bpAfter.State == EBlueprintState.Complete)
            {
                pawn.CurrentBlueprintId = 0;
                pawn.State = EPawnState.Idle;
                pawn.StateTimer = 0f;
            }
        }

        /// <summary>
        /// 尝试给Pawn分配一个蓝图任务
        /// </summary>
        private bool TryAssignBlueprint(Data.PawnData pawn)
        {
            var unassigned = _blueprintModel.GetUnassignedBlueprints();
            if (unassigned.Count == 0) return false;

            // 选择最近的蓝图
            Blueprint.Data.BlueprintData closest = null;
            int closestDist = int.MaxValue;

            foreach (var bp in unassigned)
            {
                if (bp.Floor != pawn.Floor) continue;

                int dist = Math.Abs(pawn.X - bp.X) + Math.Abs(pawn.Y - bp.Y);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = bp;
                }
            }

            if (closest == null) return false;

            // 指派
            if (!_constructionSystem.AssignPawnToBlueprint(pawn.PawnId, closest.BlueprintId))
                return false;

            pawn.CurrentBlueprintId = closest.BlueprintId;

            // 移动到蓝图旁
            if (MoveToBlueprint(pawn, closest))
                return true;

            // 如果已经在旁边, 直接开始施工
            int curDist = Math.Abs(pawn.X - closest.X) + Math.Abs(pawn.Y - closest.Y);
            if (curDist <= 1)
            {
                pawn.State = EPawnState.Working;
                pawn.StateTimer = 0f;
                return true;
            }

            // 无法到达, 取消指派
            _constructionSystem.UnassignPawn(closest.BlueprintId);
            pawn.CurrentBlueprintId = 0;
            return false;
        }

        /// <summary>
        /// 移动Pawn到蓝图旁边可行走的格子
        /// </summary>
        private bool MoveToBlueprint(Data.PawnData pawn, Blueprint.Data.BlueprintData bp)
        {
            // 尝试蓝图相邻4个格子
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            int bestDist = int.MaxValue;
            int bestX = bp.X, bestY = bp.Y;
            bool found = false;

            for (int i = 0; i < 4; i++)
            {
                int tx = bp.X + dx[i];
                int ty = bp.Y + dy[i];

                if (!_mapSystem.IsValidPosition(tx, ty, pawn.Floor)) continue;
                if (!_mapSystem.IsCellWalkable(tx, ty, pawn.Floor)) continue;

                int dist = Math.Abs(pawn.X - tx) + Math.Abs(pawn.Y - ty);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestX = tx;
                    bestY = ty;
                    found = true;
                }
            }

            if (!found) return false;

            // 如果已经在目标格, 直接进入Working状态
            if (pawn.X == bestX && pawn.Y == bestY)
            {
                pawn.State = EPawnState.Working;
                pawn.StateTimer = 0f;
                return true;
            }

            return _movementSystem.RequestMove(pawn.PawnId, bestX, bestY);
        }
    }
}
