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
        }

        protected override void Register_Model()
        {
            base.Register_Model();
            this.RegisterModel(new LaunchResourcesDataModel());
            this.RegisterModel(new GameSceneResourcesDataModel());
        }

        protected override void Register_Utility()
        {
            base.Register_Utility();
        }
    }
}