namespace GDFrameworkExtend.FSM
{
    public interface IFsmNode
    {
        void OnInit(FsmManager fsmManager);

        bool OnEnterCondition();

        void OnEnter();

        void OnUpdate();

        bool OnExitCondition();

        void OnExit();

        void OnDeInit();
    }
}