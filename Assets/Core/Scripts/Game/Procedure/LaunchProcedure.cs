using Core.Game.Procedure.Resource;
using GDFrameworkCore;
using GDFramework.Procedure;
using GDFramework.Resource;
using GDFrameworkExtend.FSM;


namespace Core.Game.Procedure
{
    /// <summary>
    /// 登录流程
    /// 开始初始化,加载数据
    /// 主要加载游戏相关的配置
    /// </summary>
    public class LaunchProcedure : ProcedureBase, ICanGetSystem
    {
        private ResourcesManager _resourcesManager;
        
        private LaunchResourcesLoader _launchResourcesLoader;

        public override void OnInit(FsmManager fsmManager)
        {
            base.OnInit(fsmManager);
            _resourcesManager = GetArchitecture().GetSystem<ResourcesManager>();
            _launchResourcesLoader = new LaunchResourcesLoader();
        }

        public override void OnEnter()
        {
            _resourcesManager.StartLoadingResources(typeof(LaunchResourcesLoader), _launchResourcesLoader,
                () =>
                {
                    DataLoadComplete();
                });
        }

        /// <summary>
        /// 初始数据加载完成
        /// </summary>
        private void DataLoadComplete()
        {
            this.SendEvent<SChangeProcedureEvent>(new SChangeProcedureEvent(typeof(MainMenuProcedure)));
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

        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }
    }
}