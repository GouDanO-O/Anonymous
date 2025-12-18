/****************************************************************************
 * Copyright (c) 2016 - 2023 liangxiegame UNDER MIT License
 * 
 * https://GDFrameworkExtend.CoreKit.cn
 * https://github.com/liangxiegame/GDFrameworkExtend.CoreKit
 * https://gitee.com/liangxiegame/GDFrameworkExtend.CoreKit
 ****************************************************************************/

using UnityEngine;

namespace GDFrameworkExtend.ActionKit
{
    public static class AudioSourceShortCutExtensions
    {
        public static IAction PlayAction(this AudioSource self)
        {
            return ActionKit.Custom<AudioSource>(api =>
            {
                api.OnStart(() =>
                {
                    api.Data = self;
                    self.Play();
                }).OnExecute(_ =>
                {
                    if (api.Data && !api.Data.isPlaying)
                    {
                        api.Finish();
                    }
                });
            });
        }
    }
}