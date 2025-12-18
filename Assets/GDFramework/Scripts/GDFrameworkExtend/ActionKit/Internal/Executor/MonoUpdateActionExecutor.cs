/****************************************************************************
 * Copyright (c) 2015 - 2024 liangxiegame UNDER MIT License
 *
 * https://GDFrameworkExtend.CoreKit.cn
 * https://github.com/liangxiegame/GDFrameworkExtend.CoreKit
 * https://gitee.com/liangxiegame/GDFrameworkExtend.CoreKit
 ****************************************************************************/

using System;
using System.Collections.Generic;
using GDFrameworkExtend.PoolKit;
using Unity.VisualScripting;
using UnityEngine;

namespace GDFrameworkExtend.ActionKit
{
    internal class MonoUpdateActionExecutor : MonoBehaviour, IActionExecutor
    {
        public class ActionTask
        {
            public IAction Action;
            public IActionController Controller;
            public Action<IActionController> OnFinish;
        }

        private List<ActionTask> mPrepareExecutionActions =
            new List<ActionTask>();

        private Dictionary<IAction, ActionTask> mExecutingActions =
            new Dictionary<IAction, ActionTask>();

        private static SimpleObjectPool<ActionTask> mActionTaskPool = new SimpleObjectPool<ActionTask>(
            () => new ActionTask(), (task) =>
            {
                task.Action = null;
                task.Controller = null;
                task.OnFinish = null;
            }, 50);

        public void Execute(IActionController controller, Action<IActionController> onFinish = null)
        {
            if (controller.Action.Status == ActionStatus.Finished) controller.Action.Reset();
            if (this.UpdateAction(controller, 0, onFinish)) return;

            var actionTask = mActionTaskPool.Allocate();
            actionTask.Action = controller.Action;
            actionTask.Controller = controller;
            actionTask.OnFinish = onFinish;
            mPrepareExecutionActions.Add(actionTask);
        }

        private List<IActionController> mToActionRemove = new List<IActionController>();

        private void Update()
        {
            if (mPrepareExecutionActions.Count > 0)
            {
                foreach (var prepareExecutionAction in mPrepareExecutionActions)
                {
                    if (mExecutingActions.ContainsKey(prepareExecutionAction.Action))
                    {
                        mExecutingActions[prepareExecutionAction.Action] = prepareExecutionAction;
                    }
                    else
                    {
                        mExecutingActions.Add(prepareExecutionAction.Action, prepareExecutionAction);
                    }
                }

                mPrepareExecutionActions.Clear();
            }

            foreach (var actionAndFinishCallback in mExecutingActions)
            {
                if (actionAndFinishCallback.Value.Controller.UpdateMode == ActionUpdateModes.ScaledDeltaTime)
                {
                    if (this.UpdateAction(actionAndFinishCallback.Value.Controller, Time.deltaTime,
                            actionAndFinishCallback.Value.OnFinish))
                    {
                        mToActionRemove.Add(actionAndFinishCallback.Value.Controller);
                    }
                }
                else if (actionAndFinishCallback.Value.Controller.UpdateMode == ActionUpdateModes.UnscaledDeltaTime)
                {
                    if (this.UpdateAction(actionAndFinishCallback.Value.Controller, Time.unscaledDeltaTime,
                            actionAndFinishCallback.Value.OnFinish))
                    {
                        mToActionRemove.Add(actionAndFinishCallback.Value.Controller);
                    }
                }
            }

            if (mToActionRemove.Count > 0)
            {
                foreach (var controller in mToActionRemove)
                {
                    mExecutingActions.Remove(controller.Action);
                    controller.Recycle();
                }

                mToActionRemove.Clear();
            }
        }
    }

    public static class MonoUpdateActionExecutorExtension
    {
        public static IAction ExecuteByUpdate<T>(this T self, IAction action, IActionController controller,
            Action<IActionController> onFinish = null)
            where T : MonoBehaviour
        {
            if (action.Status == ActionStatus.Finished) action.Reset();
            self.gameObject.GetOrAddComponent<MonoUpdateActionExecutor>().Execute(controller, onFinish);
            return action;
        }
    }
}