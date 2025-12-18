using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GDFramework;
using GDFrameworkCore;
using GDFrameworkExtend.FluentAPI;
using GDFrameworkExtend.LogKit;
using UnityEngine;
using UnityEngine.U2D;
using GDFrameworkExtend.ResKit;
using YooAsset;

namespace GDFramework.Utility
{
    public class ResourcesUtility : BasicToolUtility
    {
        private ResLoader resLoader;

        public ResourcesUtility()
        {
            InitUtility();
        }
        
        protected override void InitUtility()
        {
            InitLoader();
        }
        
        private void InitLoader()
        {
            if (resLoader == null)
            {
                resLoader = ResLoader.Allocate();
            }
        }

        /// <summary>
        /// 加载泛型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadObjAsync(string name, Action<object> action)
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed) 
                    action?.Invoke(res.Asset);
            });
            resLoader.LoadAsync();
        }
        
        /// <summary>
        /// 加载图集
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadSpriteAtlasAsync(string name, Action<SpriteAtlas> action)
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed)
                    action?.Invoke(res.Asset.As<SpriteAtlas>());
            });
            resLoader.LoadAsync();
        }

        /// <summary>
        /// 加载AB精灵图
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadSpritesAsync(string name, Action<Sprite> action)
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed) 
                    action?.Invoke(res.Asset.As<Sprite>());
            });
            resLoader.LoadAsync();
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadAudioAsync(string name, Action<AudioClip> action)
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed) 
                    action?.Invoke(res.Asset.As<AudioClip>());
            });
            resLoader.LoadAsync();
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadPrefabAsync(string name, Action<GameObject> action)
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed)
                    action?.Invoke(res.Asset.As<GameObject>());
            });
            resLoader.LoadAsync();
        }
        
        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="name"></param>
        public async UniTask<GameObject> LoadPrefabAsync(string name)
        {
            var completionSource = new UniTaskCompletionSource<GameObject>();
    
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed)
                    completionSource.TrySetResult(res.Asset.As<GameObject>());
                else
                    completionSource.TrySetException(new System.Exception($"加载预制体失败: {name}"));
            });
    
            resLoader.LoadAsync();
    
            return await completionSource.Task;
        }

        /// <summary>
        /// 加载ScriptableObject
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadScriptObjAsync<T>(string name, Action<T> action) where T : ScriptableObject
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed)
                {
                    var loadedObject = res.Asset as T;
                    if (loadedObject != null)
                        action?.Invoke(loadedObject);
                    else
                        LogKit.Error($"错误的加载类型:{typeof(T)}.");
                }
                else
                {
                    LogKit.Error($"加载错误:{name}");
                }
            });

            resLoader.LoadAsync();
        }

        /// <summary>
        /// 加载Json
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void LoadJsonAsync(string name, Action<TextAsset> action)
        {
            resLoader.Add2Load(name, (succeed, res) =>
            {
                if (succeed) 
                    action?.Invoke(res.Asset.As<TextAsset>());
            });
            resLoader.LoadAsync();
        }
        
        public async UniTask LoadRawTextAsync(string yooAddress, System.Action<string> onText)
        {
            var pkg = YooAssets.GetPackage("DefaultPackage");
            var info = pkg.GetAssetInfo(yooAddress.Substring("yoo:".Length));
            var handle = pkg.LoadRawFileAsync(info);
            await handle.Task;
            if (handle.Status == EOperationStatus.Succeed)
            {
                var text = System.Text.Encoding.UTF8.GetString(handle.GetRawFileData());
                onText?.Invoke(text);
            }
            else
            {
                LogKit.Error(handle.LastError);
            }
            handle.Release();
        }

    }
}