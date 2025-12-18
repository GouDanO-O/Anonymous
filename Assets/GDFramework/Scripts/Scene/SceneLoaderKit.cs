using System.Collections;
using Cysharp.Threading.Tasks;
using GDFramework.Utility;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace GDFramework.Scene
{
    /// <summary>
    /// 场景名称
    /// </summary>
    public enum ESceneName : byte
    {
        Menu = 1,
        GameScene = 2
    }

    public class SceneLoaderKit : AbstractSystem
    {
        public UnityAction<ESceneName> onLoadScene;

        public UnityAction OnSceneLoadStart;

        public UnityAction<float> OnSceneLoading;

        public UnityAction OnSceneLoadComplete;
        
        protected override void OnInit()
        {
            onLoadScene += LoadSceneAsync;
        }

        protected override void OnDeinit()
        {
            base.OnDeinit();
            onLoadScene -= LoadSceneAsync;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        private void LoadSceneAsync(ESceneName sceneName)
        {
            LoadingScene(sceneName);
        }

        private async UniTask LoadingScene(ESceneName sceneName)
        {
            await LoadSceneAsyncCoroutine(sceneName);
        }

        /// <summary>
        /// 协程加载
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneCoroutine(ESceneName sceneName)
        {
            OnSceneLoadStart?.Invoke();
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)sceneName);

            while (asyncOperation.progress < 0.9f)
            {
                OnSceneLoading?.Invoke(asyncOperation.progress);
                yield return null;
            }

            while (!asyncOperation.isDone) 
                yield return null;

            OnSceneLoadComplete?.Invoke();
        }
        
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="sceneName"></param>
        private async UniTask LoadSceneAsyncCoroutine(ESceneName sceneName)
        {
            OnSceneLoadStart?.Invoke();
            
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)sceneName);
            while (!asyncOperation.isDone)
            {
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); 
 
                if (progress >= 1.0f)
                {
                    asyncOperation.allowSceneActivation = true;
                }
                OnSceneLoading?.Invoke(progress);
                await UniTask.Yield();
            }
 
            await UniTask.SwitchToMainThread();
            
            OnSceneLoadComplete?.Invoke();
        }
    }
}