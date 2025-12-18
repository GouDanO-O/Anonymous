using System.Collections;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFrameworkExtend.FSM;
using YooAsset;
using GDFrameworkExtend.LogKit;

namespace GDFramework.YooAssetKit
{
    internal class PatchFsmNode : FsmNode,ICanGetUtility,ICanGetSystem
    {
        protected YooAssetManager YooAssetManager => this.GetSystem<YooAssetManager>();

        protected PatchOperation PatchOperation =>YooAssetManager.PatchOperation;
        
        protected string PackageName => YooAssetManager.PackageName;
        
        protected string  PackageVersion => YooAssetManager.PackageVersion;
        
        protected EPlayMode PlayMode => FrameManager.Instance.YooAssetPlayMode;

        public override void OnEnter()
        {
            
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

        protected void StartCoroutine(IEnumerator coroutine)
        {
            this.GetUtility<CoroutineMonoUtility>().StartCoroutine(coroutine);
        }

        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }
    }
}