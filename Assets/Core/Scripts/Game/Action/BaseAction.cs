using System;
using Core.Game.Action.Interface;

namespace Core.Game.Action
{
    [Serializable]
    public abstract class BaseAction : IAction
    {
        public abstract void Execute();
    }
}