using Core.Game.Condition.Interface;

namespace Core.Game.Condition
{
    public abstract class BaseCondition : ICondition
    {
        public abstract bool CheckCondition();
    }
}