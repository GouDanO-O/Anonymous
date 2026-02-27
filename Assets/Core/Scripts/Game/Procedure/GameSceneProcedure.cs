using Core.Game.Item.System;
using Core.Game.Map.System;
using Core.Game.Pawn.System;
using Core.Game.Procedure.Resource;
using Core.Game.View;
using GDFramework.Input;
using GDFramework.Procedure;
using GDFramework.Resource;
using GDFramework.Scene;
using GDFrameworkCore;
using GDFrameworkExtend.FSM;
using GDFrameworkExtend.LogKit;
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

            // 初始化地图
            var mapSystem = this.GetSystem<MapSystem>();
            mapSystem.CreateMap("TestWorld", 250, 250, 3, 42);
            LogKit.Log("GameSceneProcedure: 地图初始化完成");

            // 在中心建筑内放置测试家具
            // 中心建筑: startX=119, startY=120, 12x10, 内部约(120,121)~(129,128)
            var itemSystem = this.GetSystem<ItemSystem>();
            itemSystem.PlaceItem(1, 120, 127, 0); // Bed (1x2) 左上
            itemSystem.PlaceItem(1, 122, 127, 0); // Bed (1x2)
            itemSystem.PlaceItem(2, 124, 124, 0); // Table (2x2) 中间
            itemSystem.PlaceItem(3, 123, 124, 0); // Chair 桌旁
            itemSystem.PlaceItem(3, 126, 125, 0); // Chair 桌旁
            itemSystem.PlaceItem(4, 128, 121, 0); // StorageCrate 右下
            itemSystem.PlaceItem(5, 124, 121, 0); // Rug (2x2) 中下
            itemSystem.PlaceItem(6, 120, 121, 0); // Lamp 左下
            LogKit.Log("GameSceneProcedure: 测试家具放置完成");

            // 生成测试Pawn (地图中心附近)
            int cx = 125;
            int cy = 125;
            var pawnSystem = this.GetSystem<PawnSystem>();
            pawnSystem.CreatePawn(1, cx, cy + 3, 0, "Colonist A");
            pawnSystem.CreatePawn(2, cx + 3, cy - 2, 0, "Worker B");
            pawnSystem.CreatePawn(3, cx - 4, cy + 1, 0, "Scout C");
            LogKit.Log("GameSceneProcedure: 测试Pawn生成完成");
        }
    }
}