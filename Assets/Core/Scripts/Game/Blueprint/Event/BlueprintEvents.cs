using Core.Game.Blueprint.Define;

namespace Core.Game.Blueprint.Event
{
    public struct SBlueprintPlacedEvent
    {
        public long BlueprintId;
        public EBlueprintType Type;
        public int X;
        public int Y;
        public int Floor;
    }

    public struct SBlueprintCancelledEvent
    {
        public long BlueprintId;
    }

    public struct SBlueprintCompletedEvent
    {
        public long BlueprintId;
        public EBlueprintType Type;
    }

    public struct SBlueprintProgressEvent
    {
        public long BlueprintId;
        public float Progress;
    }
}
