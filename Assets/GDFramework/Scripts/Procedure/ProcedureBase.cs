using Core.Game;
using GDFrameworkCore;
using GDFrameworkExtend.FSM;

namespace GDFramework.Procedure
{
    public abstract class ProcedureBase : FsmNode,ICanSendEvent,ICanGetSystem
    {
        public IArchitecture GetArchitecture()
        {
            return GameMain.Interface;
        }
    }
}