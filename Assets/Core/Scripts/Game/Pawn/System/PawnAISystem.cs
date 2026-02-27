using Core.Game.Map.System;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Event;
using Core.Game.Pawn.Model;
using GDFrameworkCore;
using Random = System.Random;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn AI系统: 简单状态机 (Idle→Wander→Moving→Idle循环)
    /// </summary>
    public class PawnAISystem : AbstractSystem
    {
        private PawnDataModel _pawnModel;
        private PawnMovementSystem _movementSystem;
        private MapSystem _mapSystem;
        private Random _rng;

        protected override void OnInit()
        {
            _pawnModel = this.GetModel<PawnDataModel>();
            _movementSystem = this.GetSystem<PawnMovementSystem>();
            _mapSystem = this.GetSystem<MapSystem>();
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
                    // Moving状态由MovementSystem驱动, 完成后自动回到Idle
                }
            }
        }

        private void HandleIdle(Data.PawnData pawn, float deltaTime)
        {
            pawn.StateTimer += deltaTime;

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
    }
}
