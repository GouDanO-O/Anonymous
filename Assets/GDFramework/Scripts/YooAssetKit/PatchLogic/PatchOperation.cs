using GDFrameworkCore;
using GDFrameworkExtend.FSM;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    public class PatchOperation : GameAsyncOperation,ICanRegisterEvent
    {
        private enum ESteps
        {
            None,
            Update,
            Done,
        }
        
        private ESteps _steps = ESteps.None;
        
        private FsmManager _fsmManager;

        public PatchOperation()
        {
            _fsmManager = new FsmManager();
            _fsmManager.Init();
            AddFsmNode();
        }
        
        protected override void OnStart()
        {
            _steps = ESteps.Update;
            _fsmManager.SetInitialFsmNode(typeof(InitializePackageFsmNode));
        }

        protected override void OnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Update)
            {
                _fsmManager.UpdateFsmNode();
            }
        }

        protected override void OnAbort()
        {
            
        }
        
        /// <summary>
        /// 终止
        /// </summary>
        public void SetFinish()
        {
            _steps = ESteps.Done;
            Status = EOperationStatus.Succeed;
        }

        /// <summary>
        /// 添加流程
        /// </summary>
        private void AddFsmNode()
        {
            _fsmManager.RegisterFsmNode(new InitializePackageFsmNode());
            _fsmManager.RegisterFsmNode(new RequestPackageVersionFsmNode());
            _fsmManager.RegisterFsmNode(new UpdatePackageManifestFsmNode());
            _fsmManager.RegisterFsmNode(new CreateDownloaderFsmNode());
            _fsmManager.RegisterFsmNode(new DownloadPackageFilesFsmNode());
            _fsmManager.RegisterFsmNode(new DownloadPackageOverFsmNode());
            _fsmManager.RegisterFsmNode(new StartGameFsmNode());
            _fsmManager.RegisterFsmNode(new ClearCacheBundleFsmNode());
        }
        
        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }
    }
}