using System;
using GDFrameworkExtend.AudioKit;
using GDFrameworkExtend.LogKit;
using GDFrameworkExtend.ResKit;
using UnityEngine;

namespace GDFramework.YooAssetKit
{
    public class YooAssetsAudioLoaderPool : AbstractAudioLoaderPool
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected override IAudioLoader CreateLoader()
        {
            return new AssetsAudioLoader();
        }
        
        class AssetsAudioLoader : IAudioLoader
        {
            private AudioClip _clip;
        
            public AudioClip Clip => _clip;

            private ResLoader _resLoader;
        
            public AudioClip LoadClip(AudioSearchKeys audioSearchKeys)
            {
                LogKit.Log("当前加载LoadClip");
                if (_resLoader == null)
                {
                    _resLoader = ResLoader.Allocate();
                }
                return _resLoader.LoadSync<AudioClip>($"yoo:{audioSearchKeys.AssetName}");
            }

            public void LoadClipAsync(AudioSearchKeys audioSearchKeys, Action<bool, AudioClip> onLoad)
            {
                LogKit.Log("当前加载LoadClipAsync");
                if (_resLoader == null)
                {
                    _resLoader = ResLoader.Allocate();
                }

                _resLoader.Add2Load<AudioClip>($"yoo:{audioSearchKeys.AssetName}", (b, res) =>
                {
                    _clip = res.Asset as AudioClip;
                    onLoad(b, res.Asset as AudioClip);
                });

                _resLoader.LoadAsync();
            }

            public void Unload()
            {
                _clip = null;
                _resLoader?.Recycle2Cache();
                _resLoader = null;
            }
        }
    }
}