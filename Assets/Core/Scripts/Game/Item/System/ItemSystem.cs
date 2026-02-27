using Core.Game.Item.Data;
using Core.Game.Item.Event;
using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;

namespace Core.Game.Item.System
{
    /// <summary>
    /// 物品/家具门面系统
    /// </summary>
    public class ItemSystem : AbstractSystem
    {
        private ItemDataModel _itemModel;
        private MapDataModel _mapDataModel;
        private MapSystem _mapSystem;

        protected override void OnInit()
        {
            _itemModel = this.GetModel<ItemDataModel>();
            _mapDataModel = this.GetModel<MapDataModel>();
            _mapSystem = this.GetSystem<MapSystem>();
        }

        /// <summary>
        /// 检查能否放置物品
        /// </summary>
        public bool CanPlaceItem(int defId, int x, int y, int floor, int rotation = 0)
        {
            var def = TempConfigProvider.GetItemDef(defId);
            GetRotatedSize(def.Width, def.Height, rotation, out int w, out int h);

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    int cx = x + dx;
                    int cy = y + dy;

                    if (!_mapSystem.IsValidPosition(cx, cy, floor))
                        return false;

                    var cell = _mapSystem.GetCell(cx, cy, floor);
                    if (cell == null) return false;

                    // 需要有地板或地形
                    if (!cell.HasFloor && !cell.HasTerrain) return false;

                    // 如果要放阻挡物品, 目标格不能已经有阻挡物品
                    if (def.BlocksMovement && cell.HasFlag(ECellFlags.Occupied))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 放置物品
        /// </summary>
        public long PlaceItem(int defId, int x, int y, int floor, int rotation = 0)
        {
            if (!CanPlaceItem(defId, x, y, floor, rotation))
                return Define.ItemConst.InvalidItemId;

            var def = TempConfigProvider.GetItemDef(defId);
            GetRotatedSize(def.Width, def.Height, rotation, out int w, out int h);

            var data = new ItemData
            {
                ItemDefId = defId,
                AnchorX = x,
                AnchorY = y,
                Floor = floor,
                Width = w,
                Height = h,
                Rotation = rotation,
                Health = def.MaxHealth
            };

            long id = _itemModel.AddItem(data);

            // 注册到所有占用格子
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    int cx = x + dx;
                    int cy = y + dy;
                    _mapSystem.PlaceObject(cx, cy, floor, id);

                    if (def.BlocksMovement)
                    {
                        var cell = _mapSystem.GetCell(cx, cy, floor);
                        cell?.SetFlag(ECellFlags.Occupied, true);
                    }
                }
            }

            // 刷新寻路网格
            if (def.BlocksMovement)
                _mapDataModel.InvalidatePathGrid(floor);

            // 标记相关chunk dirty
            MarkChunksDirty(x, y, w, h, floor);

            this.SendEvent(new SItemPlacedEvent { ItemId = id, Data = data });
            LogKit.Log($"物品放置: [{id}] {def.Name} @ ({x},{y},F{floor}) {w}x{h}");
            return id;
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        public void RemoveItem(long itemId)
        {
            var item = _itemModel.GetItem(itemId);
            if (item == null) return;

            var def = TempConfigProvider.GetItemDef(item.ItemDefId);

            for (int dy = 0; dy < item.Height; dy++)
            {
                for (int dx = 0; dx < item.Width; dx++)
                {
                    int cx = item.AnchorX + dx;
                    int cy = item.AnchorY + dy;
                    _mapSystem.RemoveObject(cx, cy, item.Floor, itemId);

                    if (def.BlocksMovement)
                    {
                        var cell = _mapSystem.GetCell(cx, cy, item.Floor);
                        cell?.SetFlag(ECellFlags.Occupied, false);
                    }
                }
            }

            if (def.BlocksMovement)
                _mapDataModel.InvalidatePathGrid(item.Floor);

            MarkChunksDirty(item.AnchorX, item.AnchorY, item.Width, item.Height, item.Floor);

            _itemModel.RemoveItem(itemId);
            this.SendEvent(new SItemRemovedEvent { ItemId = itemId });
            LogKit.Log($"物品移除: [{itemId}] {def.Name}");
        }

        public ItemData GetItem(long itemId) => _itemModel.GetItem(itemId);

        #region 工具方法

        private void GetRotatedSize(int defW, int defH, int rotation, out int w, out int h)
        {
            if (rotation == 90 || rotation == 270)
            {
                w = defH;
                h = defW;
            }
            else
            {
                w = defW;
                h = defH;
            }
        }

        private void MarkChunksDirty(int x, int y, int w, int h, int floor)
        {
            var map = _mapDataModel.CurrentMap;
            if (map == null) return;

            int chunkSize = MapConst.ChunkSize;

            int startCX = x / chunkSize;
            int startCY = y / chunkSize;
            int endCX = (x + w - 1) / chunkSize;
            int endCY = (y + h - 1) / chunkSize;

            for (int cy = startCY; cy <= endCY; cy++)
            {
                for (int cx = startCX; cx <= endCX; cx++)
                {
                    var chunk = map.GetChunk(cx, cy, floor);
                    if (chunk != null)
                    {
                        chunk.MarkDirty();
                        this.SendEvent(new Map.Event.SChunkDirtyEvent(cx,cy,floor));
                    }
                }
            }
        }

        #endregion
    }
}
