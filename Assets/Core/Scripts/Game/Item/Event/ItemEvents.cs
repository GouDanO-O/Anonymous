using Core.Game.Item.Data;

namespace Core.Game.Item.Event
{
    public struct SItemPlacedEvent
    {
        public long ItemId;
        public ItemData Data;
    }

    public struct SItemRemovedEvent
    {
        public long ItemId;
    }
}
