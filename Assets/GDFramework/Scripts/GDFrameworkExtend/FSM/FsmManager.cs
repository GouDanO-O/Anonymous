using System;
using System.Collections.Generic;
using GDFramework.Utility;
using GDFrameworkCore;
using UnityEngine;

namespace GDFrameworkExtend.FSM
{
    public class FsmManager : ICanRegisterEvent
    {
        private Dictionary<Type, IFsmNode> _fsmNodeDict = new Dictionary<Type, IFsmNode>();
        
        protected IFsmNode CurrentFsmNode;
        
        protected IFsmNode LastFsmNode;
        
        public IArchitecture GetArchitecture()
        {
            return Main.Interface;
        }

        public virtual void Init()
        {
            
        }

        public virtual void DeInit()
        {
            _fsmNodeDict.Clear();
        }
        
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IFsmNode GetFsmNode(Type type)
        {
            if (_fsmNodeDict.TryGetValue(type, out var p)) 
                return p;
            LogKit.LogKit.Error($"{type.Name} 未注册Fsm节点");
            return null;
        }

        ///<summary>
        /// 注册节点
        /// </summary>
        public void RegisterFsmNode(IFsmNode fsmNode)
        {
            if (fsmNode == null)
            {
                LogKit.LogKit.Error("节点为空");
                return;
            }

            var type = fsmNode.GetType();
            if (_fsmNodeDict.ContainsKey(type))
            {
                LogKit.LogKit.Error($"节点{type.Name} 已注册");
                return;
            }

            fsmNode.OnInit(this);
            _fsmNodeDict.Add(type, fsmNode);
        }

        /// <summary>
        /// 设置初始流程
        /// </summary>
        /// <param name="fsmNode"></param>
        public void SetInitialFsmNode(Type fsmNode)
        {
            if (fsmNode == null)
            {
                LogKit.LogKit.Error("节点为空");
                return;
            }
            
            if (!_fsmNodeDict.ContainsKey(fsmNode))
            {
                LogKit.LogKit.Error($"节点{fsmNode.Name} 未注册");
                return;
            }
            
            ChangeFsmNode(fsmNode);
        }
        
        /// <summary>
        /// 切换流程
        /// </summary>
        /// <param name="type"></param>
        public void ChangeFsmNode(Type type)
        {
            if (!_fsmNodeDict.TryGetValue(type, out var next))
            {
                LogKit.LogKit.Error($"无法切换 {type.Name} 未注册");
                return;
            }

            if (CurrentFsmNode != null)
            {
                if (!CurrentFsmNode.OnExitCondition()) 
                    return;
                CurrentFsmNode.OnExit();
                LastFsmNode = CurrentFsmNode;
            }

            if (!next.OnEnterCondition())
                return;

            Debug.Log($"切换节点: {type.Name}");
            CurrentFsmNode = next;
            CurrentFsmNode.OnEnter();

        }

        public void ReturnToLastFsmNode()
        {
            
        }

        public void UpdateFsmNode() => CurrentFsmNode?.OnUpdate();
    }
}