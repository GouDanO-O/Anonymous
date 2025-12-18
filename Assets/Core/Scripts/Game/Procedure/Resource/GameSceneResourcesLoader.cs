using Core.Game.Procedure.Models.Resource;
using Cysharp.Threading.Tasks;
using GDFramework.Resource;
using GDFramework.YooAssetKit;
using GDFrameworkCore;
using UnityEngine;
using YooAsset;

namespace Core.Game.Procedure.Resource
{
    public class GameSceneResourcesLoader : BaseResourcesLoader,ICanGetSystem
    {
        private GameSceneResourcesDataModel _gameSceneResourcesDataModel;
        
        protected override void AddLoadingResource()
        {
            _gameSceneResourcesDataModel = this.GetModel<GameSceneResourcesDataModel>();
        }
        
    }
    
    
}
