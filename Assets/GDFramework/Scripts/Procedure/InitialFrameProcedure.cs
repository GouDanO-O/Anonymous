using Core.Game.Procedure;
using GDFramework.Input;
using GDFramework.Resource;
using GDFrameworkCore;
using GDFrameworkExtend.FSM;

namespace GDFramework.Procedure
{
    /// <summary>
    /// 框架初始化流程
    /// 主要初始化一些框架内容配置
    /// </summary>
    public class InitialFrameProcedure : ProcedureBase
    {
        private ResourcesManager _resourcesManager;
        
        private InitialFrameDataLoader _initialFrameDataLoader;
        
        public override void OnInit(FsmManager  fsmManager)
        {
            base.OnInit(fsmManager);
            _resourcesManager = GetArchitecture().GetSystem<ResourcesManager>();
            _initialFrameDataLoader = new InitialFrameDataLoader();
        }
        
        public override void OnEnter()
        {
            _resourcesManager.StartLoadingResources(typeof(InitialFrameDataLoader), _initialFrameDataLoader,
                () =>
                {
                    DataLoadComplete();
                });
            
            this.GetSystem<LubanKit.LubanKit>().InitData();
        }
        
        /// <summary>
        /// 数据加载完成
        /// </summary>
        private void DataLoadComplete()
        {
            this.SendEvent(new SChangeProcedureEvent(typeof(LaunchProcedure)));
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