using Core.Game.Blueprint.Data;
using Core.Game.Blueprint.Define;
using Core.Game.Blueprint.Event;
using Core.Game.Blueprint.Model;
using Core.Game.Config;
using Core.Game.Item.System;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.System;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;

namespace Core.Game.Blueprint.System
{
    /// <summary>
    /// 蓝图系统: 放置/取消/验证蓝图
    /// </summary>
    public class BlueprintSystem : AbstractSystem
    {
        private BlueprintDataModel _blueprintModel;
        private MapSystem _mapSystem;
        private ItemSystem _itemSystem;

        protected override void OnInit()
        {
            _blueprintModel = this.GetModel<BlueprintDataModel>();
            _mapSystem = this.GetSystem<MapSystem>();
            _itemSystem = this.GetSystem<ItemSystem>();
        }

        /// <summary>
        /// 放置建造结构蓝图 (墙/门/窗)
        /// </summary>
        public long PlaceBuildStructureBlueprint(int structureDefId, int x, int y, int floor)
        {
            // 验证: 位置有效
            if (!_mapSystem.IsValidPosition(x, y, floor))
                return 0;

            // 验证: 该格没有结构
            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || cell.HasStructure)
                return 0;

            // 验证: 地形可建造
            var terrainDef = ConfigManager.GetTerrainDef(cell.TerrainDefId);
            if (!terrainDef.CanBuild && !cell.HasFloor)
                return 0;

            // 检查是否已有蓝图在此位置
            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.BuildStructure,
                State = EBlueprintState.Planned,
                DefId = structureDefId,
                X = x, Y = y, Floor = floor,
                WorkRequired = 5f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 建结构 [{id}] StructureDef={structureDefId} @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置建造家具蓝图
        /// </summary>
        public long PlaceBuildFurnitureBlueprint(int itemDefId, int x, int y, int floor, int rotation = 0)
        {
            if (!_itemSystem.CanPlaceItem(itemDefId, x, y, floor, rotation))
                return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.BuildFurniture,
                State = EBlueprintState.Planned,
                DefId = itemDefId,
                X = x, Y = y, Floor = floor,
                Rotation = rotation,
                WorkRequired = 8f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 建家具 [{id}] ItemDef={itemDefId} @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置拆除蓝图 (结构)
        /// </summary>
        public long PlaceDemolishStructureBlueprint(int x, int y, int floor)
        {
            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || !cell.HasStructure) return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.Demolish,
                State = EBlueprintState.Planned,
                DefId = cell.StructureDefId,
                X = x, Y = y, Floor = floor,
                WorkRequired = 4f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 拆结构 [{id}] @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置拆除蓝图 (家具)
        /// </summary>
        public long PlaceDemolishItemBlueprint(long itemId)
        {
            var item = _itemSystem.GetItem(itemId);
            if (item == null) return 0;

            if (_blueprintModel.GetBlueprintAt(item.AnchorX, item.AnchorY, item.Floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.Demolish,
                State = EBlueprintState.Planned,
                DefId = item.ItemDefId,
                X = item.AnchorX, Y = item.AnchorY, Floor = item.Floor,
                SourceItemId = itemId,
                WorkRequired = 4f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 拆家具 [{id}] ItemId={itemId}");
            return id;
        }

        /// <summary>
        /// 放置拆卸蓝图 (家具 — 不返还材料,可重装)
        /// </summary>
        public long PlaceDisassembleBlueprint(long itemId)
        {
            var item = _itemSystem.GetItem(itemId);
            if (item == null) return 0;

            if (_blueprintModel.GetBlueprintAt(item.AnchorX, item.AnchorY, item.Floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.Disassemble,
                State = EBlueprintState.Planned,
                DefId = item.ItemDefId,
                X = item.AnchorX, Y = item.AnchorY, Floor = item.Floor,
                SourceItemId = itemId,
                WorkRequired = 3f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 拆卸 [{id}] ItemId={itemId}");
            return id;
        }

        /// <summary>
        /// 放置重装蓝图 (使用已拆卸的家具,无材料消耗)
        /// </summary>
        public long PlaceReinstallBlueprint(long sourceItemId, int x, int y, int floor, int rotation = 0)
        {
            var item = _itemSystem.GetItem(sourceItemId);
            if (item == null) return 0;

            // 检查物品是否已拆卸
            if (item.IsInstalled) return 0;

            if (!_itemSystem.CanPlaceItem(item.ItemDefId, x, y, floor, rotation))
                return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.Reinstall,
                State = EBlueprintState.Planned,
                DefId = item.ItemDefId,
                X = x, Y = y, Floor = floor,
                Rotation = rotation,
                SourceItemId = sourceItemId,
                WorkRequired = 5f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 重装 [{id}] ItemId={sourceItemId} → ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置建造地板蓝图
        /// </summary>
        public long PlaceBuildFloorBlueprint(int floorDefId, int x, int y, int floor)
        {
            if (!_mapSystem.IsValidPosition(x, y, floor))
                return 0;

            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || cell.HasFloor)
                return 0;

            // 需要可建造地形或上层有屋顶支撑
            if (!cell.IsBuildable)
                return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.BuildFloor,
                State = EBlueprintState.Planned,
                DefId = floorDefId,
                X = x, Y = y, Floor = floor,
                WorkRequired = 4f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 建地板 [{id}] FloorDef={floorDefId} @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置拆除地板蓝图
        /// </summary>
        public long PlaceDemolishFloorBlueprint(int x, int y, int floor)
        {
            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || !cell.HasFloor)
                return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.DemolishFloor,
                State = EBlueprintState.Planned,
                DefId = cell.FloorDefId,
                X = x, Y = y, Floor = floor,
                WorkRequired = 3f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 拆地板 [{id}] @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置建造屋顶蓝图
        /// 规则: F==0时需要Indoor或有结构, F>0时需要下层有墙/柱
        /// </summary>
        public long PlaceBuildRoofBlueprint(int x, int y, int floor)
        {
            if (!_mapSystem.IsValidPosition(x, y, floor))
                return 0;

            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || cell.HasRoof)
                return 0;

            // 结构支撑检查
            if (floor == 0)
            {
                // F0: 需要Indoor(被墙围合) 或 本格有结构(墙/柱)
                if (!cell.IsIndoor && !cell.HasStructure)
                    return 0;
            }
            else
            {
                // F>0: 下层同位置需要墙或柱
                var lowerCell = _mapSystem.GetCell(x, y, floor - 1);
                if (lowerCell == null || !lowerCell.HasStructure)
                    return 0;

                var lowerDef = ConfigManager.GetStructureDef(lowerCell.StructureDefId);
                if ((EStructureType)lowerDef.StructureType != EStructureType.Wall &&
                    (EStructureType)lowerDef.StructureType != EStructureType.Pillar)
                    return 0;
            }

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.BuildRoof,
                State = EBlueprintState.Planned,
                DefId = 0,
                X = x, Y = y, Floor = floor,
                WorkRequired = 4f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 建屋顶 [{id}] @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置拆除屋顶蓝图
        /// </summary>
        public long PlaceDemolishRoofBlueprint(int x, int y, int floor)
        {
            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || !cell.HasRoof)
                return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.DemolishRoof,
                State = EBlueprintState.Planned,
                DefId = 0,
                X = x, Y = y, Floor = floor,
                WorkRequired = 3f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 拆屋顶 [{id}] @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 放置建造地基蓝图 (水体→Foundation)
        /// </summary>
        public long PlaceBuildFoundationBlueprint(int x, int y, int floor)
        {
            if (!_mapSystem.IsValidPosition(x, y, floor))
                return 0;

            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null) return 0;

            // 不允许在已是地基的地形上重复建造
            if (cell.TerrainDefId == 7)
                return 0;

            if (_blueprintModel.GetBlueprintAt(x, y, floor) != null)
                return 0;

            var bp = new BlueprintData
            {
                Type = EBlueprintType.BuildFoundation,
                State = EBlueprintState.Planned,
                DefId = 7, // Foundation terrain ID
                X = x, Y = y, Floor = floor,
                WorkRequired = 6f,
                WorkDone = 0f
            };

            long id = _blueprintModel.AddBlueprint(bp);
            SendBlueprintPlacedEvent(bp);
            LogKit.Log($"蓝图放置: 建地基 [{id}] @ ({x},{y},F{floor})");
            return id;
        }

        /// <summary>
        /// 取消蓝图
        /// </summary>
        public void CancelBlueprint(long blueprintId)
        {
            var bp = _blueprintModel.GetBlueprint(blueprintId);
            if (bp == null) return;

            bp.State = EBlueprintState.Cancelled;
            bp.AssignedPawnId = 0;

            this.SendEvent(new SBlueprintCancelledEvent { BlueprintId = blueprintId });
            _blueprintModel.RemoveBlueprint(blueprintId);
            LogKit.Log($"蓝图取消: [{blueprintId}]");
        }

        private void SendBlueprintPlacedEvent(BlueprintData bp)
        {
            this.SendEvent(new SBlueprintPlacedEvent
            {
                BlueprintId = bp.BlueprintId,
                Type = bp.Type,
                X = bp.X, Y = bp.Y, Floor = bp.Floor
            });
        }
    }
}
