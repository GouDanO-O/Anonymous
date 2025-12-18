using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using GDFramework.Procedure;
using GDFrameworkExtend.FSM;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFrameworkExtend.AudioKit;
using GDFrameworkExtend.ResKit;
using GDFrameworkExtend.UIKit;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    /// <summary>
    /// 使用yooasset来进行分包管理和加载
    /// </summary>
    public class YooAssetManager : AbstractSystem
    {
        public string PackageName = "DefaultPackage";
        
        public string PackageVersion ="1.0.0";
        
        public ResourceDownloaderOperation ResourceDownloaderOperation{get;private set;}

        private IRemoteServices remoteServices;
        
        public PatchOperation PatchOperation;
        
        private List<AssetHandle> _assetHandleList = new List<AssetHandle>();
        private List<ResourcePackage> _resourcePackageList = new List<ResourcePackage>();
        
        protected override void OnInit()
        {
            
        }

        public void InitYooAssetKit(Action callback)
        {
            YooAssets.Initialize();
            ResKit.Init();
            AudioKit.Config.AudioLoaderPool = new YooAssetsAudioLoaderPool();
            UIKit.Config.PanelLoaderPool = new YooAssetsPanelLoaderPool();
            ResFactory.AddResCreator<YooAssetResCreator>();
            this.GetUtility<CoroutineMonoUtility>().StartCoroutine(InitYooAsset(callback));
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private IEnumerator InitYooAsset(Action callBack)
        {
            // 开始补丁更新流程
            PatchOperation = new PatchOperation();
            YooAssets.StartOperation(PatchOperation);
            yield return PatchOperation;
            
            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(PackageName);
            YooAssets.SetDefaultPackage(gamePackage);
            callBack.Invoke();
        }

        public ResourcePackage GetPackage()
        {
            return YooAssets.GetPackage(PackageName);
        }
        

        /// <summary>
        /// 更新版本
        /// </summary>
        /// <param name="packageVersion"></param>
        public void UpdatePackageVersion(string packageVersion)
        {
            PackageVersion= packageVersion;
        }

        /// <summary>
        /// 更新下载器
        /// </summary>
        /// <param name="resourceDownloaderOperation"></param>
        public void UpdateDownloader(ResourceDownloaderOperation resourceDownloaderOperation)
        {
            ResourceDownloaderOperation = resourceDownloaderOperation;
        }

        /// <summary>
        /// 下载错误
        /// </summary>
        /// <param name="downloadError"></param>
        public void DownloadError(DownloadErrorData  downloadError)
        {
            
        }

        /// <summary>
        /// 更新下载进度
        /// </summary>
        /// <param name="downloadUpdate"></param>
        public void DownloadUpdate(DownloadUpdateData downloadUpdate)
        {
            
        }

        public void AddAssetHandle(AssetHandle assetHandle,ResourcePackage resourcePackage)
        {
            _assetHandleList.Add(assetHandle);
            _resourcePackageList.Add(resourcePackage);
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release() 
        {
            foreach (AssetHandle mHandles in _assetHandleList)
            {
                mHandles.Release();
            }
            _assetHandleList.Clear();

            foreach (ResourcePackage mPackage in _resourcePackageList)
            {
                mPackage.UnloadUnusedAssetsAsync();
            }
            _resourcePackageList.Clear();
        }
    }
}

