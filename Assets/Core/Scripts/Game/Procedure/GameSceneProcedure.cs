using Core.Game.Procedure.Resource;
using Core.Game.View;
using GDFramework.Input;
using GDFramework.Procedure;
using GDFramework.Resource;
using GDFramework.Scene;
using GDFrameworkCore;
using GDFrameworkExtend.FSM;
using GDFrameworkExtend.UIKit;

namespace Core.Game.Procedure
{
    public class GameSceneProcedure : ProcedureBase
    {
        private ResourcesManager _resourcesManager;
        
        private GameSceneResourcesLoader _gameSceneResourcesLoader; 
        
        public override void OnInit(FsmManager fsmManager)
        {
            base.OnInit(fsmManager);

            _resourcesManager = this.GetSystem<ResourcesManager>();
            _gameSceneResourcesLoader=new GameSceneResourcesLoader();
        }

        public override void OnEnter()
        {
            UIKit.ClosePanel<UI_GameMenuPanel>();
            _resourcesManager.StartLoadingResources(typeof(GameSceneResourcesLoader), _gameSceneResourcesLoader,
                () =>
                {
                    DataLoadComplete();
                });
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
        
        /// <summary>
        /// 数据加载完成
        /// 准备进入场景
        /// </summary>
        private void DataLoadComplete()
        {
            ChangeToGameScene();
        }
        
        private void ChangeToGameScene()
        {
            SceneLoaderKit sceneLoaderKit = this.GetSystem<SceneLoaderKit>();
            sceneLoaderKit.onLoadScene.Invoke(ESceneName.GameScene);
            sceneLoaderKit.OnSceneLoadComplete += LoadGameSceneComplete;
        }

        private void LoadGameSceneComplete()
        {
            GameManager.Instance.LoadGameSceneComplete();
            this.GetSystem<SceneLoaderKit>().OnSceneLoadComplete -= LoadGameSceneComplete;
        }
    }
}