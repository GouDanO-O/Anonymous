using Core.Game.Config;
using Core.Game.Item.Define;
using Core.Game.Item.Model;
using Core.Game.Map.Data;
using Core.Game.Map.System;
using Core.Game.Pawn.Model;
using Core.Game.Pawn.System;
using Core.Game.Selection.Data;
using Core.Game.Selection.Define;
using Core.Game.Selection.Event;
using Core.Game.Selection.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;

namespace Core.Game.Selection.System
{
    /// <summary>
    /// 选择与交互门面系统
    /// 处理点击 → 确定选中目标 → 发送事件
    /// </summary>
    public class SelectionSystem : AbstractSystem
    {
        private SelectionDataModel _selectionModel;
        private MapSystem _mapSystem;
        private PawnDataModel _pawnModel;
        private ItemDataModel _itemModel;

        protected override void OnInit()
        {
            _selectionModel = this.GetModel<SelectionDataModel>();
            _mapSystem = this.GetSystem<MapSystem>();
            _pawnModel = this.GetModel<PawnDataModel>();
            _itemModel = this.GetModel<ItemDataModel>();
        }

        /// <summary>
        /// 左键点击: 选中格子上的目标
        /// 优先级: Pawn > Item > Cell
        /// </summary>
        public void HandleLeftClick(int cellX, int cellY, int floor)
        {
            if (!_mapSystem.IsValidPosition(cellX, cellY, floor))
            {
                ClearSelection();
                return;
            }

            var selection = new SelectionData();

            // 1. 检查是否点中了Pawn
            long pawnId = FindPawnAt(cellX, cellY, floor);
            if (pawnId != 0)
            {
                selection.SetPawn(pawnId, cellX, cellY, floor);
                ApplySelection(selection);
                var pawn = _pawnModel.GetPawn(pawnId);
                LogKit.Log($"选中Pawn: [{pawnId}] {pawn?.Name} @ ({cellX},{cellY})");
                return;
            }

            // 2. 检查是否点中了Item
            long itemId = FindItemAt(cellX, cellY, floor);
            if (itemId != 0)
            {
                selection.SetItem(itemId, cellX, cellY, floor);
                ApplySelection(selection);
                var item = _itemModel.GetItem(itemId);
                var def = ConfigManager.GetItemDef(item?.ItemDefId ?? 0);
                LogKit.Log($"选中Item: [{itemId}] {def.Name} @ ({cellX},{cellY})");
                return;
            }

            // 3. 选中空地格子
            selection.SetCell(cellX, cellY, floor);
            ApplySelection(selection);

            var cell = _mapSystem.GetCell(cellX, cellY, floor);
            string terrain = cell != null
                ? $"Terrain={cell.TerrainDefId}, Floor={cell.FloorDefId}, Walk={cell.IsWalkable}"
                : "null";
            LogKit.Log($"选中Cell: ({cellX},{cellY}) {terrain}");
        }

        /// <summary>
        /// 右键点击: 对选中的Pawn下达移动指令
        /// </summary>
        public void HandleRightClick(int cellX, int cellY, int floor)
        {
            if (_selectionModel.CurrentType != ESelectionType.Pawn) return;

            long pawnId = _selectionModel.Current.TargetId;
            var pawnSystem = this.GetSystem<PawnSystem>();
            bool success = pawnSystem.MovePawnTo(pawnId, cellX, cellY);

            if (success)
            {
                this.SendEvent(new SCommandMoveEvent
                {
                    PawnId = pawnId,
                    TargetX = cellX,
                    TargetY = cellY,
                    Floor = floor
                });

                var pawn = _pawnModel.GetPawn(pawnId);
                LogKit.Log($"移动指令: {pawn?.Name} → ({cellX},{cellY})");
            }
            else
            {
                LogKit.Log($"移动指令失败: 无法到达 ({cellX},{cellY})");
            }
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            if (!_selectionModel.HasSelection) return;
            _selectionModel.ClearSelection();
            this.SendEvent(new SSelectionChangedEvent
            {
                Type = ESelectionType.None
            });
        }

        #region 查找

        /// <summary>
        /// 在指定格子查找Pawn (遍历当前楼层所有Pawn)
        /// </summary>
        private long FindPawnAt(int x, int y, int floor)
        {
            var pawns = _pawnModel.GetPawnsOnFloor(floor);
            if (pawns == null) return 0;
            foreach (var pawn in pawns)
            {
                if (pawn.X == x && pawn.Y == y)
                    return pawn.PawnId;
            }
            return 0;
        }

        /// <summary>
        /// 在指定格子查找Item (通过CellData.ObjectIds)
        /// </summary>
        private long FindItemAt(int x, int y, int floor)
        {
            var cell = _mapSystem.GetCell(x, y, floor);
            if (cell == null || cell.ObjectIds == null) return 0;

            foreach (var objId in cell.ObjectIds)
            {
                // Item ID >= 1_000_000, Pawn ID < 1_000_000
                if (objId >= ItemConst.ItemIdStart)
                    return objId;
            }
            return 0;
        }

        #endregion

        private void ApplySelection(SelectionData selection)
        {
            _selectionModel.SetSelection(selection);
            this.SendEvent(new SSelectionChangedEvent
            {
                Type = selection.Type,
                CellX = selection.CellX,
                CellY = selection.CellY,
                Floor = selection.Floor,
                TargetId = selection.TargetId
            });
        }
    }
}
