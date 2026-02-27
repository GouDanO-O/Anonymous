using Core.Game.Selection.Define;

namespace Core.Game.Selection.Event
{
    /// <summary>
    /// 选择变更事件
    /// </summary>
    public struct SSelectionChangedEvent
    {
        public ESelectionType Type;
        public int CellX;
        public int CellY;
        public int Floor;
        public long TargetId;
    }

    /// <summary>
    /// 右键命令事件 (选中Pawn后右键下达移动指令)
    /// </summary>
    public struct SCommandMoveEvent
    {
        public long PawnId;
        public int TargetX;
        public int TargetY;
        public int Floor;
    }
}
