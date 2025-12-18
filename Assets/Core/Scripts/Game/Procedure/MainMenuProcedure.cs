using GDFramework.Input;
using GDFrameworkCore;
using GDFramework.Procedure;
using GDFramework.Scene;
using GDFramework.Utility;
using GDFramework.View;
using GDFrameworkExtend.FSM;


namespace Core.Game.Procedure
{
    /// <summary>
    /// 主界面流程
    /// </summary>
    public class MainMenuProcedure : ProcedureBase
    {
        public override void OnEnter()
        {
            StartLoadMenu();
        }

        private void StartLoadMenu()
        {
            SceneLoaderKit sceneLoaderKit = this.GetSystem<SceneLoaderKit>();
            sceneLoaderKit.onLoadScene.Invoke(ESceneName.Menu);
            
            sceneLoaderKit.OnSceneLoadComplete += LoadMenuSceneComplete;
        }
        
        /// <summary>
        /// 加载菜单场景完成
        /// </summary>
        /// <param name="sceneName"></param>
        private void LoadMenuSceneComplete()
        {
            this.GetSystem<ViewManager>().EnterMenu();
            this.GetSystem<SceneLoaderKit>().OnSceneLoadComplete -= LoadMenuSceneComplete;
        }

        public override void OnUpdate()
        {
        }

        public override void OnExit()
        {
        }

        public override void OnDeInit()
        {
        }
    }
}