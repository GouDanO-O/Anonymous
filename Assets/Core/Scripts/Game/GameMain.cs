using Core.Game.Map.Model;
using Core.Game.Map.System;
using Core.Game.Procedure.Models.Resource;
using GDFrameworkCore;

namespace Core.Game
{
    public class GameMain : Main
    {
        protected override void Register_System()
        {
            base.Register_System();
            
            this.RegisterSystem(new MapGenerateSystem());
            this.RegisterSystem(new RoomSystem());        
            this.RegisterSystem(new MapOcclusionSystem());
            this.RegisterSystem(new MapSystem());
        }
        
        protected override void Register_Model()
        {
            base.Register_Model();
            this.RegisterModel(new LaunchResourcesDataModel());
            this.RegisterModel(new GameSceneResourcesDataModel());
            
            this.RegisterModel(new MapDataModel());
        }

        protected override void Register_Utility()
        {
            base.Register_Utility();
        }
    }
}