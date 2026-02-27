using System.Collections.Generic;
using Core.Game.Item.Data;
using Core.Game.Item.Define;
using GDFrameworkCore;

namespace Core.Game.Item.Model
{
    public class ItemDataModel : AbstractModel
    {
        private readonly Dictionary<long, ItemData> _items = new();
        private long _nextItemId = ItemConst.ItemIdStart;

        protected override void OnInit()
        {
        }

        public long AddItem(ItemData data)
        {
            long id = _nextItemId++;
            data.ItemId = id;
            _items[id] = data;
            return id;
        }

        public void RemoveItem(long itemId)
        {
            _items.Remove(itemId);
        }

        public ItemData GetItem(long itemId)
        {
            return _items.GetValueOrDefault(itemId);
        }

        public IReadOnlyDictionary<long, ItemData> GetAllItems()
        {
            return _items;
        }

        public int ItemCount => _items.Count;
    }
}
