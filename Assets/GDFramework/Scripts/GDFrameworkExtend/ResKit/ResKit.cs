/****************************************************************************
 * Copyright (c) 2016 - 2025 liangxiegame UNDER MIT License
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using System.Collections;
using GDFrameworkCore;
using UnityEngine;

namespace GDFrameworkExtend.ResKit
{

    public class ResKit
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void CheckAutoInit()
        {
            if (PlatformCheck.IsEditor && AssetBundlePathHelper.SimulationMode)
            {
                Init();
            }
        }
#endif
        
        public static void Init()
        {
            ResMgr.Init();
        }
        
        public static IEnumerator InitAsync()
        {
            yield return ResMgr.InitAsync();
        }

        private static readonly Lazy<ResKit> mInstance = new Lazy<ResKit>(() => new ResKit().InternalInit());
        internal static ResKit Get => mInstance.Value;

        internal IOCContainer Container = new IOCContainer();

        ResKit InternalInit()
        {
            Container.Register<IZipFileHelper>(new ZipFileHelper());
            Container.Register<IBinarySerializer>(new BinarySerializer());
            return this;
        }
    }
}