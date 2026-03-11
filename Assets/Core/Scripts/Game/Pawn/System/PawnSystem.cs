using Core.Game.Config;
using Core.Game.Map.Data;
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
    /// Pawn门面系统: 对外统一API
    /// </summary>
    public class PawnSystem : AbstractSystem
    {
        private PawnDataModel _pawnModel;
        private PawnMovementSystem _movementSystem;
        private PawnNeedSystem _needSystem;
        private PawnAISystem _aiSystem;
        private MapSystem _mapSystem;
        private MapOcclusionSystem _occlusionSystem;

        protected override void OnInit()
        {
            _pawnModel = this.GetModel<PawnDataModel>();
            _movementSystem = this.GetSystem<PawnMovementSystem>();
            _needSystem = this.GetSystem<PawnNeedSystem>();
            _aiSystem = this.GetSystem<PawnAISystem>();
            _mapSystem = this.GetSystem<MapSystem>();
            _occlusionSystem = this.GetSystem<MapOcclusionSystem>();
        }

        #region 创建/销毁

        /// <summary>
        /// 创建Pawn
        /// </summary>
        public long CreatePawn(int defId, int x, int y, int floor, string name)
        {
            var def = ConfigManager.GetPawnDef(defId);

            var data = new PawnData
            {
                PawnDefId = defId,
                Name = name,
                X = x,
                Y = y,
                Floor = floor,
                State = EPawnState.Idle,
                Needs = NeedData.CreateDefault(),
                MoveSpeed = def.MoveSpeed,
                StateTimer = 0f
            };

            long id = _pawnModel.AddPawn(data);

            // 注册到地图
            _mapSystem.PlaceObject(x, y, floor, id);

            this.SendEvent(new SPawnCreatedEvent
            {
                PawnId = id,
                Data = data
            });

            LogKit.Log($"Pawn创建: [{id}] {name} (Def={defId}) @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 销毁Pawn
        /// </summary>
        public void DestroyPawn(long pawnId)
        {
            var pawn = _pawnModel.GetPawn(pawnId);
            if (pawn == null) return;

            _mapSystem.RemoveObject(pawn.X, pawn.Y, pawn.Floor, pawnId);
            _pawnModel.RemovePawn(pawnId);

            this.SendEvent(new SPawnDestroyedEvent { PawnId = pawnId });
            LogKit.Log($"Pawn销毁: [{pawnId}] {pawn.Name}");
        }

        #endregion

        #region 指令

        /// <summary>
        /// 命令Pawn移动到目标位置
        /// </summary>
        public bool MovePawnTo(long pawnId, int targetX, int targetY)
        {
            return _movementSystem.RequestMove(pawnId, targetX, targetY);
        }

        #endregion

        #region 查询

        public PawnData GetPawn(long pawnId) => _pawnModel.GetPawn(pawnId);
        public int PawnCount => _pawnModel.PawnCount;

        #endregion

        #region 更新

        /// <summary>
        /// 每帧由PawnView调用
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_pawnModel.PawnCount == 0) return;

            _aiSystem.Tick(deltaTime);
            _movementSystem.Tick(deltaTime);
            _needSystem.Tick(deltaTime);

            // 向遮挡系统报告所有Pawn位置
            _occlusionSystem.UpdatePawnPositions(_pawnModel.GetPawnPositions());
        }

        #endregion
    }
}
