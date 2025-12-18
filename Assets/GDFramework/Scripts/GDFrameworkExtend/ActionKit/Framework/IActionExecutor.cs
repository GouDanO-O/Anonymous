/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://GDFrameworkExtend.CoreKit.cn
 * https://github.com/liangxiegame/GDFrameworkExtend.CoreKit
 * https://gitee.com/liangxiegame/GDFrameworkExtend.CoreKit
 ****************************************************************************/

using System;
using UnityEngine;

namespace GDFrameworkExtend.ActionKit
{
    public interface IActionExecutor
    {
        void Execute(IActionController controller,Action<IActionController> onFinish = null);
    }
    

    public static class IActionExecutorExtensions
    {
        public static bool UpdateAction(this IActionExecutor self,IActionController controller,float dt,Action<IActionController> onFinish = null)
        {
            if (!controller.Action.Deinited && controller.Action.Execute(dt))
            {
                onFinish?.Invoke(controller);
                controller.Deinit();
                return true;
            }

            return controller.Action.Deinited;
        }
    }
}