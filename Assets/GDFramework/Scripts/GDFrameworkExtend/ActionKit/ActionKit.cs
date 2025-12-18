/****************************************************************************
 * Copyright (c) 2016 - 2023 liangxiegame UNDER MIT License
 * 
 * https://GDFrameworkExtend.CoreKit.cn
 * https://github.com/liangxiegame/GDFrameworkExtend.CoreKit
 * https://gitee.com/liangxiegame/GDFrameworkExtend.CoreKit
 ****************************************************************************/

using System;
using System.Collections;
using System.Threading.Tasks;
using GDFrameworkCore;
using GDFrameworkExtend.ActionKit;

namespace GDFrameworkExtend.ActionKit
{
    /// <summary>
    /// Action 时序动作序列（组合模式 + 命令模式 + 建造者模式）
    /// </summary>
    public partial class ActionKit : Architecture<ActionKit>
    {
        public static ulong ID_GENERATOR = 0;
        
        /// <summary>
        /// 延时回调
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IAction Delay(float seconds, Action callback)
        {
            return GDFrameworkExtend.ActionKit.Delay.Allocate(seconds, callback);
        }
        
        /// <summary>
        /// 动作序列
        /// </summary>
        /// <returns></returns>
        public static ISequence Sequence()
        {
            return GDFrameworkExtend.ActionKit.Sequence.Allocate();
        }
        
        /// <summary>
        /// 延时帧
        /// </summary>
        /// <param name="frameCount"></param>
        /// <param name="onDelayFinish"></param>
        /// <returns></returns>
        public static IAction DelayFrame(int frameCount, Action onDelayFinish)
        {
            return GDFrameworkExtend.ActionKit.DelayFrame.Allocate(frameCount, onDelayFinish);
        }

        public static IAction NextFrame(Action onNextFrame)
        {
            return GDFrameworkExtend.ActionKit.DelayFrame.Allocate(1, onNextFrame);
        }


        public static IAction Lerp(float a,float b,float duration,Action<float> onLerp,Action onLerpFinish = null)
        {
            return GDFrameworkExtend.ActionKit.Lerp.Allocate(a, b, duration, onLerp, onLerpFinish);
        }

        public static IAction Callback(Action callback)
        {
            return GDFrameworkExtend.ActionKit.Callback.Allocate(callback);
        }
        
        /// <summary>
        /// 条件
        /// </summary>
        void ConditionAPI()
        {
        }
        
        /// <summary>
        /// 重复动作
        /// </summary>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public static IRepeat Repeat(int repeatCount = -1)
        {
            return GDFrameworkExtend.ActionKit.Repeat.Allocate(repeatCount);
        }
        
        /// <summary>
        /// 并行动作
        /// </summary>
        /// <returns></returns>
        public static IParallel Parallel()
        {
            return GDFrameworkExtend.ActionKit.Parallel.Allocate();
        }
        
        public void ComplexAPI()
        {
        }

        
        /// <summary>
        /// 自定义动作
        /// </summary>
        /// <param name="customSetting"></param>
        /// <returns></returns>
        public static IAction Custom(Action<ICustomAPI<object>> customSetting)
        {
            var action = GDFrameworkExtend.ActionKit.Custom.Allocate();
            customSetting(action);
            return action;
        }

        public static IAction Custom<TData>(Action<ICustomAPI<TData>> customSetting)
        {
            var action = GDFrameworkExtend.ActionKit.Custom<TData>.Allocate();
            customSetting(action);
            return action;
        }
        
        /// <summary>
        /// 协程支持
        /// </summary>
        /// <param name="coroutineGetter"></param>
        /// <returns></returns>
        public static IAction Coroutine(Func<IEnumerator> coroutineGetter)
        {
            return CoroutineAction.Allocate(coroutineGetter);
        }
        
        /// <summary>
        /// Task 支持
        /// </summary>
        /// <param name="taskGetter"></param>
        /// <returns></returns>
        public static IAction Task(Func<Task> taskGetter)
        {
            return TaskAction.Allocate(taskGetter);
        }


        #region Events
        
        /// <summary>
        /// Update 生命周期支持
        /// </summary>
        public static EasyEvent OnUpdate => ActionKitMonoBehaviourEvents.Instance.OnUpdate;
        
        /// <summary>
        /// FixedUpdate 生命周期支持
        /// </summary>
        public static EasyEvent OnFixedUpdate => ActionKitMonoBehaviourEvents.Instance.OnFixedUpdate;
        
        /// <summary>
        /// LateUpdate 生命周期支持
        /// </summary>
        public static EasyEvent OnLateUpdate => ActionKitMonoBehaviourEvents.Instance.OnLateUpdate;
        
        /// <summary>
        /// OnGUI 生命周期支持
        /// </summary>
        public static EasyEvent OnGUI => ActionKitMonoBehaviourEvents.Instance.OnGUIEvent;
        
        /// <summary>
        /// OnApplicationQuit 生命周期支持
        /// </summary>
        public static EasyEvent OnApplicationQuit => ActionKitMonoBehaviourEvents.Instance.OnApplicationQuitEvent;
        
        /// <summary>
        /// OnApplicationPause 生命周期支持
        /// </summary>
        public static EasyEvent<bool> OnApplicationPause =>
            ActionKitMonoBehaviourEvents.Instance.OnApplicationPauseEvent;
        
        /// <summary>
        /// OnApplicationFocus 生命周期支持
        /// </summary>
        public static EasyEvent<bool> OnApplicationFocus =>
            ActionKitMonoBehaviourEvents.Instance.OnApplicationFocusEvent;

        protected override void Init()
        {
        }

        #endregion
    }
}