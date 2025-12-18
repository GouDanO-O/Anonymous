using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFramework.YooAssetKit;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace GDFramework.Resource
{
    public class ResourcesManager : AbstractSystem
    {
        private ResourcesUtility _loader;

        private Dictionary<Type, BaseResourcesLoader> _resourcesLoaderDict = new();

        private Type _currentLoaderType;

        private bool _isInitialized = false;
        
        protected override void OnInit()
        {
            _loader = this.GetUtility<ResourcesUtility>();
        }
        
        /// <summary>
        /// 开始加载模块的资源
        /// </summary>
        /// <param name="loaderType"></param>
        /// <param name="callback"></param>
        public void StartLoadingResources(Type loaderType, BaseResourcesLoader resourcesLoader,
            UnityAction callback)
        {
            if (!_resourcesLoaderDict.ContainsKey(loaderType)) 
                _resourcesLoaderDict.Add(loaderType, resourcesLoader);

            _resourcesLoaderDict[loaderType].InitLoader(_loader, callback);
            _currentLoaderType=loaderType;
        }
    }
}