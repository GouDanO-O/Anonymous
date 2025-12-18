using Core.Game;
using GDFrameworkCore;

namespace GDFramework.Cheater
{
    /// <summary>
    /// 添加作弊模块
    /// </summary>
    public abstract class AddCheatCommand : ICanGetSystem
    {
        public abstract void ExecuteCommand();
        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}