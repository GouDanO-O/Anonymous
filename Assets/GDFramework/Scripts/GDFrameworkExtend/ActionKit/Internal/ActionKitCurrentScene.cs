/****************************************************************************
 * Copyright (c) 2016 - 2024 liangxiegame UNDER MIT License
 * 
 * https://GDFrameworkExtend.CoreKit.cn
 * https://github.com/liangxiegame/GDFrameworkExtend.CoreKit
 * https://gitee.com/liangxiegame/GDFrameworkExtend.CoreKit
 ****************************************************************************/

using UnityEngine;

namespace GDFrameworkExtend.ActionKit
{
    public class ActionKitCurrentScene : MonoBehaviour
    {
        public static ActionKitCurrentScene mSceneComponent = null;

        public static ActionKitCurrentScene SceneComponent
        {
            get
            {
                if (!mSceneComponent)
                {
                    mSceneComponent = new GameObject("ActionKitCurrentScene").AddComponent<ActionKitCurrentScene>();
                }

                return mSceneComponent;
            }
        }

        private void Awake() => hideFlags = HideFlags.HideInHierarchy;

        private void OnDestroy() => mSceneComponent = null;
    }
}