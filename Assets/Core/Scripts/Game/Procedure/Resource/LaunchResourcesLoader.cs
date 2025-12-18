using System;
using System.Collections.Generic;
using Core.Game.Procedure.Models.Resource;
using Cysharp.Threading.Tasks;
using GDFramework.Input;
using GDFramework.Resource;
using GDFramework.YooAssetKit;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;
using UnityEngine;
using YooAsset;

namespace Core.Game.Procedure.Resource
{
    public class LaunchResourcesLoader : BaseResourcesLoader, ICanGetSystem
    {
        private LaunchResourcesDataModel _launchResourcesDataModel;
        
        protected async override void AddLoadingResource()
        {
            _launchResourcesDataModel = this.GetModel<LaunchResourcesDataModel>();

            this.GetSystem<NewInputManager>().InitActionAsset();
            await LoadAllChunkDefJson();
        }
        
        
        private async UniTask LoadAllChunkDefJson()
        {
            LoadingComplete();
        }
    }
}