using System.Collections;
using GDFramework.Utility;
using GDFrameworkExtend.LogKit;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    internal class DownloadPackageFilesFsmNode : PatchFsmNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LogKit.Log("开始下载资源文件");
            StartCoroutine(BeginDownload());
        }
        
        private IEnumerator BeginDownload()
        {
            var downloader = YooAssetManager.ResourceDownloaderOperation;
            downloader.DownloadErrorCallback = YooAssetManager.DownloadError;
            downloader.DownloadUpdateCallback = YooAssetManager.DownloadUpdate;
            downloader.BeginDownload();
            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                yield break;

            FsmManager.ChangeFsmNode(typeof(ClearCacheBundleFsmNode));
        }
    }
}