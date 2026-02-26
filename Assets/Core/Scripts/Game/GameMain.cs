using Core.Game.Building.Model;
using Core.Game.Building.System;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using Core.Game.Navigation.System;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.System;
using Core.Game.Procedure.Models.Resource;
using GDFrameworkCore;

namespace Core.Game
{
    public class GameMain : Main
    {
        protected override void Register_System()
        {
            base.Register_System();

            // Map模块（注意注册顺序：被依赖的先注册）
            this.RegisterSystem(new MapGenerateSystem());
            this.RegisterSystem(new RoomSystem());
            this.RegisterSystem(new MapOcclusionSystem());
            this.RegisterSystem(new ChunkCullingSystem());
            this.RegisterSystem(new MapSystem());

            // Navigation模块
            this.RegisterSystem(new NavigationSystem());

            // Pawn模块（依赖Navigation）
            this.RegisterSystem(new PawnMovementSystem());
            this.RegisterSystem(new PawnSystem());

            // Building模块（依赖Map）
            this.RegisterSystem(new BlueprintPlaceSystem());
            this.RegisterSystem(new BuildingSystem());
        }

        protected override void Register_Model()
        {
            base.Register_Model();
            this.RegisterModel(new LaunchResourcesDataModel());
            this.RegisterModel(new GameSceneResourcesDataModel());

            // Map模块
            this.RegisterModel(new MapDataModel());

            // Pawn模块
            this.RegisterModel(new PawnDataModel());

            // Building模块
            this.RegisterModel(new BuildingDataModel());
        }

        protected override void Register_Utility()
        {
            base.Register_Utility();
        }
    }
}