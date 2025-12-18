using GDFramework.Utility;
using GDFramework.YooAssetKit;
using GDFrameworkCore;

namespace GDFramework.Procedure
{
    /// <summary>
    /// 加载资源流程
    /// 用来获取必要的初始化资源
    /// 和加载ab包
    /// </summary>
    public class LoadResProcedure : ProcedureBase
    {
        public override void OnEnter()
        {
            this.GetSystem<YooAssetManager>().InitYooAssetKit(() =>
            {
                ResourceLoadComplete();
            });
        }

        /// <summary>
        /// 资源加载完毕(校验)
        /// </summary>
        private void ResourceLoadComplete()
        {
            StartHotFix();
        }

        /// <summary>
        /// 开始热更新
        /// </summary>
        private void StartHotFix()
        {
            HotFixOver();
        }

        private void HotFixOver()
        {
            this.SendEvent(new SChangeProcedureEvent(typeof(InitialFrameProcedure)));
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