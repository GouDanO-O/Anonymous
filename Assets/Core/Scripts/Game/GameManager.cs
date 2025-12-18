using Core.Game.Procedure;
using Core.Game.View;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using GDFrameworkExtend.UIKit;
using UnityEngine;
using YooAsset;

namespace Core.Game
{
    /// <summary>
    /// 框架和游戏流程分离
    /// </summary>
    public class GameManager : FrameManager
    {
        protected override void InitProcedure()
        {
            base.InitProcedure();
            _procedureManager.RegisterFsmNode(new LaunchProcedure());
            _procedureManager.RegisterFsmNode(new MainMenuProcedure());
            _procedureManager.RegisterFsmNode(new GameSceneProcedure());
        }

        /// <summary>
        /// 世界初始化完成
        /// </summary>
        public void LoadGameSceneComplete()
        {
            LogKit.Log("世界初始化完成");
            //UIKit.OpenPanel<UI_UniversePanel>();
        }

        public string GetCurGamingTime()
        {
            return "";
        }
    }
}