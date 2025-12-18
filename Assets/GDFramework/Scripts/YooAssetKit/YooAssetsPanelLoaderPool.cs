using System;
using GDFrameworkExtend.FluentAPI;
using GDFrameworkExtend.ResKit;
using GDFrameworkExtend.UIKit;
using UnityEngine;

namespace GDFramework.YooAssetKit
{
    public class YooAssetsPanelLoaderPool : AbstractPanelLoaderPool
    {
        public class YooAssetsPanelLoader : IPanelLoader
        {
             private ResLoader _resLoader;

            public GameObject LoadPanelPrefab(PanelSearchKeys panelSearchKeys)
            {
                if (_resLoader == null)
                {
                    _resLoader = ResLoader.Allocate();
                }

                if (panelSearchKeys.PanelType.IsNotNull() && panelSearchKeys.GameObjName.IsNullOrEmpty())
                {
                    return _resLoader.LoadSync<GameObject>($"yoo:{panelSearchKeys.PanelType.Name}");
                }
                
                return _resLoader.LoadSync<GameObject>($"yoo:{panelSearchKeys.GameObjName}");
            }

            public void LoadPanelPrefabAsync(PanelSearchKeys panelSearchKeys, Action<GameObject> onLoad)
            {
                if (_resLoader == null)
                {
                    _resLoader = ResLoader.Allocate();
                }

                if (panelSearchKeys.PanelType.IsNotNull() && panelSearchKeys.GameObjName.IsNullOrEmpty())
                {
                    _resLoader.Add2Load<GameObject>($"yoo:{panelSearchKeys.PanelType.Name}", (success, res) =>
                    {
                        if (success)
                        {
                            onLoad(res.Asset as GameObject);
                        }
                    });
                    _resLoader.LoadAsync();
                    return;
                }

                _resLoader.Add2Load<GameObject>($"yoo:{panelSearchKeys.GameObjName}", (success, res) =>
                {
                    if (success)
                    {
                        onLoad(res.Asset as GameObject);
                    }
                });
                _resLoader.LoadAsync();
            }

            public void Unload()
            {
                _resLoader?.Recycle2Cache();
                _resLoader = null;
            }
        }
        
        protected override IPanelLoader CreatePanelLoader()
        {
            return new YooAssetsPanelLoader();
        }
    }
}