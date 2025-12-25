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