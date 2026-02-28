using Core.Game.Resource.Define;

namespace Core.Game.Resource.Event
{
    public struct SMaterialStackCreatedEvent
    {
        public long StackId;
        public EMaterialType Type;
        public int Amount;
        public int X;
        public int Y;
        public int Floor;
    }

    public struct SMaterialStackRemovedEvent
    {
        public long StackId;
    }
}
