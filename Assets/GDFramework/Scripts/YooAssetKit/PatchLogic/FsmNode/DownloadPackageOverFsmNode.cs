using GDFramework.Utility;
using GDFrameworkExtend.LogKit;

namespace GDFramework.YooAssetKit
{
    internal class DownloadPackageOverFsmNode : PatchFsmNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LogKit.Log("资源文件下载完毕");
            
        }
    }
}