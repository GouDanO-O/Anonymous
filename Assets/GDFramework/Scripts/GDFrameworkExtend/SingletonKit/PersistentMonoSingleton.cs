/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using UnityEngine;

namespace GDFrameworkExtend.SingletonKit
{
    /// <summary>
    /// 当场景里包含两个 PersistentMonoSingleton，保留先创建的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PersistentMonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T mInstance;
        protected bool mEnabled;

        public static T Instance
        {
            get
            {
                if (mInstance != null) return mInstance;
                mInstance = FindObjectOfType<T>();
                if (mInstance != null) return mInstance;
                var obj = new GameObject();
                mInstance = obj.AddComponent<T>();
                return mInstance;
            }
        }

        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (mInstance == null)
            {
                mInstance = this as T;
                DontDestroyOnLoad(transform.gameObject);
                mEnabled = true;
            }
            else
            {
                if (this != mInstance)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}