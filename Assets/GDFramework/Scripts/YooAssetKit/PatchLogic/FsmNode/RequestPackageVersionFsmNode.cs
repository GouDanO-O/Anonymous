using System.Collections;
using GDFramework.Utility;
using GDFrameworkExtend.LogKit;
using UnityEngine;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    internal class RequestPackageVersionFsmNode : PatchFsmNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LogKit.Log("请求资源版本");
            StartCoroutine(UpdatePackageVersion());
        }
        
        private IEnumerator UpdatePackageVersion()
        {
            var package = YooAssets.GetPackage(PackageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                LogKit.Error(operation.Error);
            }
            else
            {
                LogKit.Log($"请求资源版本: {operation.PackageVersion}");
                YooAssetManager.UpdatePackageVersion(operation.PackageVersion);
                FsmManager.ChangeFsmNode(typeof(UpdatePackageManifestFsmNode));
            }
        }
    }
}