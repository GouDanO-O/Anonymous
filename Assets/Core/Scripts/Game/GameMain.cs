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

            // Map模块（注意注册顺序：被依赖的先注册）
            this.RegisterSystem(new MapGenerateSystem());
            this.RegisterSystem(new RoomSystem());
            this.RegisterSystem(new MapOcclusionSystem());
            this.RegisterSystem(new ChunkCullingSystem());
            this.RegisterSystem(new MapSystem());
        }

        protected override void Register_Model()
        {
            base.Register_Model();
            this.RegisterModel(new LaunchResourcesDataModel());
            this.RegisterModel(new GameSceneResourcesDataModel());

            // Map模块
            this.RegisterModel(new MapDataModel());
        }

        protected override void Register_Utility()
        {
            base.Register_Utility();
        }
    }
}