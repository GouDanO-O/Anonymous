using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using GDFrameworkExtend.PoolKit;
using GDFrameworkExtend.ResKit;
using UnityEngine;
using YooAsset;

namespace GDFramework.YooAssetKit
{
    public class YooAssetRes : Res, IController
    {
        private string _location;
        private YooAssetManager _yooAssetManager => this.GetSystem<YooAssetManager>();

        public override bool LoadSync()
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var handle = package.LoadAssetSync<Object>(_location); // FIX: 用 package.*
            if (handle.Status != EOperationStatus.Succeed)
            {
                State = ResState.Waiting;
                LogKit.Error(handle.LastError);
                _yooAssetManager.AddAssetHandle(handle, package); // 仍登记，便于统一释放
                return false;
            }

            mAsset = handle.AssetObject;
            State = ResState.Ready;
            _yooAssetManager.AddAssetHandle(handle, package);
            return true;
        }

        public override async void LoadAsync()
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var handle = package.LoadAssetAsync<Object>(_location); // FIX: 真异步
            await handle.Task;

            if (handle.Status == EOperationStatus.Succeed)
            {
                mAsset = handle.AssetObject;
                State = ResState.Ready;
            }
            else
            {
                State = ResState.Waiting;
                LogKit.Error(handle.LastError);
            }

            _yooAssetManager.AddAssetHandle(handle, package);
        }

        protected override void OnReleaseRes()
        {
            mAsset = null;
            State = ResState.Waiting;
            _yooAssetManager.Release(); // 注意：确保这里不会把“别的资源”的句柄也全清了
        }

        public static YooAssetRes Allocate(string name, string originalAssetName)
        {
            var res = SafeObjectPool<YooAssetRes>.Instance.Allocate();
            if (res != null)
            {
                if (originalAssetName.StartsWith("yoo://"))
                    originalAssetName = originalAssetName.Substring("yoo://".Length);
                if (originalAssetName.StartsWith("yoo:"))
                    originalAssetName = originalAssetName.Substring("yoo:".Length);

                res.AssetName = name;
                res._location = originalAssetName;
            }
            return res;
        }

        public IArchitecture GetArchitecture() => Main.Interface;
    }

    public class YooAssetResCreator : IResCreator
    {
        public IRes Create(ResSearchKeys keys)
        {
            var res = YooAssetRes.Allocate(keys.AssetName, keys.OriginalAssetName);
            res.AssetType = keys.AssetType;
            return res;
        }

        public bool Match(ResSearchKeys keys)
        {
            // FIX: 第二个条件原来重复了
            return keys.OriginalAssetName.StartsWith("yoo://") || keys.OriginalAssetName.StartsWith("yoo:");
        }
    }
}
