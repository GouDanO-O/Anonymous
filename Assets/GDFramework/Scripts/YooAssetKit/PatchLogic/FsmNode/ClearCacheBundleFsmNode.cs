using GDFramework.Utility;
using GDFrameworkExtend.LogKit;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    internal class ClearCacheBundleFsmNode : PatchFsmNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LogKit.Log("清理未使用的缓存文件");
            var package = YooAssets.GetPackage(PackageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += Operation_Completed;
        }
        
        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            FsmManager.ChangeFsmNode(typeof(StartGameFsmNode));
        }
    }
}