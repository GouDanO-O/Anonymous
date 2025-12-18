/****************************************************************************
 * Copyright (c) 2016 ~ 2022 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GDFrameworkExtend.ResKit
{

    public static class IResLoaderExtensions
    {
        private static Type ComponentType = typeof(Component);
        private static Type GameObjectType = typeof(GameObject);

        public static T LoadSync<T>(this IResLoader self, string assetName) where T : Object
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, null, GameObjectType);
                var retAsset = (self.LoadAssetSync(resSearchKeys) as GameObject)?.GetComponent<T>();
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
            else
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, null, type);
                var retAsset = self.LoadAssetSync(resSearchKeys) as T;
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
        }
        
        
        public static T LoadSync<T>(this IResLoader self, string ownerBundle, string assetName) where T : Object
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, ownerBundle, GameObjectType);
                var retAsset = (self.LoadAssetSync(resSearchKeys) as GameObject)?.GetComponent<T>();
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
            else
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, ownerBundle, type);
                var retAsset = self.LoadAssetSync(resSearchKeys) as T;
                resSearchKeys.Recycle2Cache();
                return retAsset;
            }
        }

        public static void Add2Load(this IResLoader self, string assetName, Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            
            var searchRule = ResSearchKeys.Allocate(assetName);
            self.Add2Load(searchRule, listener, lastOrder);
            searchRule.Recycle2Cache();
        }

        public static void Add2Load<T>(this IResLoader self, string assetName, Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, null, GameObjectType);
                self.Add2Load(resSearchKeys, listener, lastOrder);
                resSearchKeys.Recycle2Cache();
            }
            else
            {
                var searchRule = ResSearchKeys.Allocate(assetName, null, type);
                self.Add2Load(searchRule, listener, lastOrder);
                searchRule.Recycle2Cache();
            }
        }


        public static void Add2Load(this IResLoader self, string ownerBundle, string assetName,
            Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            var searchRule = ResSearchKeys.Allocate(assetName, ownerBundle);
            self.Add2Load(searchRule, listener, lastOrder);
            searchRule.Recycle2Cache();
        }

        public static void Add2Load<T>(this IResLoader self, string ownerBundle, string assetName,
            Action<bool, IRes> listener = null,
            bool lastOrder = true)
        {
            var type = typeof(T);
            if (ComponentType.IsAssignableFrom(type))
            {
                var resSearchKeys = ResSearchKeys.Allocate(assetName, ownerBundle, GameObjectType);
                self.Add2Load(resSearchKeys, listener, lastOrder);
                resSearchKeys.Recycle2Cache();
            }
            else
            {
                var searchRule = ResSearchKeys.Allocate(assetName, ownerBundle, type);
                self.Add2Load(searchRule, listener, lastOrder);
                searchRule.Recycle2Cache();
            }
        }
        
        
        public static void LoadSceneSync(this IResLoader self, string assetName,
            LoadSceneMode mode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            var resSearchRule = ResSearchKeys.Allocate(assetName);
            self.LoadSceneSync(resSearchRule, mode, physicsMode);
            resSearchRule.Recycle2Cache();
        }

        public static void LoadSceneSync(this IResLoader self, string ownerBundle, string assetName,
            LoadSceneMode mode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            var resSearchRule = ResSearchKeys.Allocate(assetName, ownerBundle);
            self.LoadSceneSync(resSearchRule, mode, physicsMode);
            resSearchRule.Recycle2Cache();
        }

        public static void LoadSceneSync(this IResLoader self, ResSearchKeys resSearchRule,
            LoadSceneMode mode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            if (ResFactory.AssetBundleSceneResCreator.Match(resSearchRule))
            {
                //加载的为场景
                IRes res = ResFactory.AssetBundleSceneResCreator.Create(resSearchRule);
#if UNITY_EDITOR
                if (AssetBundlePathHelper.SimulationMode)
                {
                    string path =
                        UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle((res as AssetBundleSceneRes)
                            .AssetBundleName)[0];
                    if (!string.IsNullOrEmpty(path))
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path,
                            new LoadSceneParameters(mode, physicsMode));
                    }
                }
                else
#endif
                {
                    self.LoadResSync(resSearchRule);
                    SceneManager.LoadScene(resSearchRule.OriginalAssetName, new LoadSceneParameters(mode, physicsMode));
                }
            }
            else
            {
                Debug.LogError("资源名称错误！请检查资源名称是否正确或是否被标记！AssetName:" + resSearchRule.AssetName);
            }
        }

        public static void LoadSceneAsync(this IResLoader self, string sceneName,
            LoadSceneMode loadSceneMode =
                LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            Action<AsyncOperation> onStartLoading = null)
        {

            var resSearchKey = ResSearchKeys.Allocate(sceneName);
            self.LoadSceneAsync(resSearchKey,loadSceneMode,physicsMode,onStartLoading);
            resSearchKey.Recycle2Cache();
        }
        
        public static void LoadSceneAsync(this IResLoader self, string bundleName,string sceneName,
            LoadSceneMode loadSceneMode =
                LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            Action<AsyncOperation> onStartLoading = null)
        {

            var resSearchKey = ResSearchKeys.Allocate(sceneName,bundleName);
            self.LoadSceneAsync(resSearchKey,loadSceneMode,physicsMode,onStartLoading);
            resSearchKey.Recycle2Cache();
        }
        

        public static void LoadSceneAsync(this IResLoader self,ResSearchKeys resSearchKeys,
            LoadSceneMode loadSceneMode =
                LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            Action<AsyncOperation> onStartLoading = null)
        {

            if (ResFactory.AssetBundleSceneResCreator.Match(resSearchKeys))
            {
                //加载的为场景
                var res = ResFactory.AssetBundleSceneResCreator.Create(resSearchKeys);
#if UNITY_EDITOR
                if (AssetBundlePathHelper.SimulationMode)
                {
                    var path =
                        UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle((res as AssetBundleSceneRes)
                            .AssetBundleName)[0];

                    if (!string.IsNullOrEmpty(path))
                    {
                        var sceneParameters = new LoadSceneParameters
                        {
                            loadSceneMode = loadSceneMode,
                            localPhysicsMode = physicsMode
                        };

                        var asyncOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                            path,
                            sceneParameters);
                        onStartLoading?.Invoke(asyncOperation);
                    }
                }
                else
#endif
                {
                    var sceneName = resSearchKeys.OriginalAssetName;
                    
                    self.Add2Load(resSearchKeys,(b, res1) =>
                    {
                        var asyncOperation = SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters()
                        {
                            loadSceneMode = loadSceneMode,
                            localPhysicsMode = physicsMode
                        });
                        onStartLoading?.Invoke(asyncOperation);
                    });
                    self.LoadAsync();
                }
            }
            else
            {
                Debug.LogError("场景名称错误！请检查名称是否正确或资源是否被标记！AssetName:" + resSearchKeys.AssetName);
            }
        }

        [Obsolete("请使用 LoadSync<Sprite>,use LoadSync<Sprite> instead", true)]
        public static Sprite LoadSprite(this IResLoader self, string spriteName) => self.LoadSync<Sprite>(spriteName);

        [Obsolete("请使用 LoadSync<Sprite>,use LoadSync<Sprite> instead", true)]
        public static Sprite LoadSprite(this IResLoader self, string bundleName, string spriteName) =>
            self.LoadSync<Sprite>(bundleName, spriteName);
    }
}