using GDFramework;
using GDFrameworkCore;


namespace GDFramework.Utility
{
    public abstract class BasicToolUtility : IUtility
    {
        protected abstract void InitUtility();

        public virtual void DeInitUtility()
        {
            
        }
    }
}