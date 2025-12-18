using GDFramework.Utility;
using GDFrameworkExtend.LogKit;
using UnityEngine;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    internal class CreateDownloaderFsmNode : PatchFsmNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LogKit.Log("创建资源下载器");
            CreateDownloader();
        }

        private void CreateDownloader()
        {
            var package = YooAssets.GetPackage(PackageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            YooAssetManager.UpdateDownloader(downloader);
            
            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("没有找到任何下载文件");
                FsmManager.ChangeFsmNode(typeof(StartGameFsmNode));
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                FsmManager.ChangeFsmNode(typeof(DownloadPackageFilesFsmNode));
            }
        }
    }
}