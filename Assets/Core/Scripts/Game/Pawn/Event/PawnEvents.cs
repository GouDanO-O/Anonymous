using Core.Game.Pawn.Data;
using Core.Game.Pawn.Define;

namespace Core.Game.Pawn.Event
{
    public struct SPawnCreatedEvent
    {
        public long PawnId;
        public PawnData Data;
    }

    public struct SPawnDestroyedEvent
    {
        public long PawnId;
    }

    public struct SPawnMovedEvent
    {
        public long PawnId;
        public int OldX;
        public int OldY;
        public int NewX;
        public int NewY;
        public int Floor;
    }

    public struct SPawnStateChangedEvent
    {
        public long PawnId;
        public EPawnState OldState;
        public EPawnState NewState;
    }

    public struct SPawnNeedChangedEvent
    {
        public long PawnId;
        public NeedData Needs;
    }
}
