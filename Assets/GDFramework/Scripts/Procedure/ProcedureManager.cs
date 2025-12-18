using GDFrameworkCore;
using GDFrameworkExtend.FSM;

namespace GDFramework.Procedure
{
    public class ProcedureManager : FsmManager
    {
        public override void Init()
        {
            base.Init();
            // ② 监听“切流程”事件
            this.RegisterEvent<SChangeProcedureEvent>(evt => ChangeFsmNode(evt._procedureType));
        }

        public override void DeInit()
        {
            base.DeInit();
        }
    }
}