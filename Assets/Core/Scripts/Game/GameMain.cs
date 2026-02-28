using Core.Game.Blueprint.Model;
using Core.Game.Blueprint.System;
using Core.Game.Item.Model;
using Core.Game.Item.System;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.System;
using Core.Game.Procedure.Models.Resource;
using Core.Game.Resource.Model;
using Core.Game.Selection.Model;
using Core.Game.Selection.System;
using GDFrameworkCore;

namespace Core.Game
{
    public class GameMain : Main
    {
        protected override void Register_System()
        {
            base.Register_System();

            // Map模块 (依赖顺序: 数据优先, 消费者在后)
            this.RegisterSystem(new MapGenerateSystem());
            this.RegisterSystem(new RoomSystem());
            this.RegisterSystem(new ChunkCullingSystem());
            this.RegisterSystem(new MapOcclusionSystem());
            this.RegisterSystem(new FloorLevelSystem());
            this.RegisterSystem(new MapSystem());

            // Pawn模块
            this.RegisterSystem(new PawnMovementSystem());
            this.RegisterSystem(new PawnNeedSystem());
            this.RegisterSystem(new PawnAISystem());
            this.RegisterSystem(new PawnSystem());

            // Item模块
            this.RegisterSystem(new ItemSystem());

            // Blueprint模块
            this.RegisterSystem(new BlueprintSystem());
            this.RegisterSystem(new ConstructionSystem());

            // Selection模块
            this.RegisterSystem(new SelectionSystem());
        }

        protected override void Register_Model()
        {
            base.Register_Model();
            this.RegisterModel(new LaunchResourcesDataModel());
            this.RegisterModel(new GameSceneResourcesDataModel());
            this.RegisterModel(new MapDataModel());
            this.RegisterModel(new PawnDataModel());
            this.RegisterModel(new ItemDataModel());
            this.RegisterModel(new SelectionDataModel());
            this.RegisterModel(new MaterialDataModel());
            this.RegisterModel(new BlueprintDataModel());
        }

        protected override void Register_Utility()
        {
            base.Register_Utility();
        }
    }
}