using Core.Game.Blueprint.Data;
using Core.Game.Blueprint.Define;
using Core.Game.Blueprint.Event;
using Core.Game.Blueprint.Model;
using Core.Game.Config;
using Core.Game.Item.System;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using Core.Game.Pawn.Define;
using Core.Game.Pawn.Model;
using Core.Game.Resource.Data;
using Core.Game.Resource.Define;
using Core.Game.Resource.Model;
using GDFrameworkCore;
using GDFrameworkExtend.LogKit;

namespace Core.Game.Blueprint.System
{
    /// <summary>
    /// 施工系统: 推进蓝图工作量, 完成后执行实际建造/拆除
    /// </summary>
    public class ConstructionSystem : AbstractSystem
    {
        private BlueprintDataModel _blueprintModel;
        private MapSystem _mapSystem;
        private MapDataModel _mapDataModel;
        private ItemSystem _itemSystem;
        private MaterialDataModel _materialModel;
        private PawnDataModel _pawnModel;

        protected override void OnInit()
        {
            _blueprintModel = this.GetModel<BlueprintDataModel>();
            _mapSystem = this.GetSystem<MapSystem>();
            _mapDataModel = this.GetModel<MapDataModel>();
            _itemSystem = this.GetSystem<ItemSystem>();
            _materialModel = this.GetModel<MaterialDataModel>();
            _pawnModel = this.GetModel<PawnDataModel>();
        }

        /// <summary>
        /// 完成蓝图施工
        /// </summary>
        public void CompleteConstruction(BlueprintData bp)
        {
            switch (bp.Type)
            {
                case EBlueprintType.BuildStructure:
                    _mapSystem.SetStructure(bp.X, bp.Y, bp.Floor, bp.DefId);
                    // 如果结构阻挡移动, 驱逐该格上的Pawn
                    var builtDef = ConfigManager.GetStructureDef(bp.DefId);
                    if (builtDef.BlocksMovement)
                    {
                        DisplacePawnsFromCell(bp.X, bp.Y, bp.Floor);
                    }
                    break;

                case EBlueprintType.BuildFurniture:
                    _itemSystem.PlaceItem(bp.DefId, bp.X, bp.Y, bp.Floor, bp.Rotation);
                    break;

                case EBlueprintType.Demolish:
                    if (bp.SourceItemId != 0)
                    {
                        // 拆除家具 → 返还材料
                        var itemDef = ConfigManager.GetItemDef(bp.DefId);
                        _itemSystem.RemoveItem(bp.SourceItemId);
                        SpawnMaterials(ConfigManager.ToMaterialCosts(itemDef.DemolishReturns), bp.X, bp.Y, bp.Floor);
                    }
                    else
                    {
                        // 拆除结构 → 返还材料
                        var structureDef = ConfigManager.GetStructureDef(bp.DefId);
                        _mapSystem.RemoveStructure(bp.X, bp.Y, bp.Floor);
                        SpawnMaterials(ConfigManager.ToMaterialCosts(structureDef.DemolishReturns), bp.X, bp.Y, bp.Floor);
                    }
                    break;

                case EBlueprintType.Disassemble:
                    // 拆卸: 不返还材料, 物品变为已拆卸状态
                    _itemSystem.UninstallItem(bp.SourceItemId);
                    break;

                case EBlueprintType.Reinstall:
                    // 重装: 无材料消耗, 使用已拆卸物品
                    _itemSystem.ReinstallItem(bp.SourceItemId, bp.X, bp.Y, bp.Floor, bp.Rotation);
                    break;

                case EBlueprintType.BuildFloor:
                    _mapSystem.SetCellFloor(bp.X, bp.Y, bp.Floor, bp.DefId);
                    break;

                case EBlueprintType.DemolishFloor:
                {
                    var floorDef = ConfigManager.GetFloorDef(bp.DefId);
                    _mapSystem.RemoveCellFloor(bp.X, bp.Y, bp.Floor);
                    SpawnMaterials(ConfigManager.ToMaterialCosts(floorDef.DemolishReturns), bp.X, bp.Y, bp.Floor);
                    break;
                }

                case EBlueprintType.BuildRoof:
                    _mapSystem.SetRoof(bp.X, bp.Y, bp.Floor, true);
                    break;

                case EBlueprintType.DemolishRoof:
                    _mapSystem.SetRoof(bp.X, bp.Y, bp.Floor, false);
                    SpawnMaterials(ConfigManager.ToMaterialCosts(ConfigManager.Tables.TbBuildCostConfig.RoofDemolishReturns), bp.X, bp.Y, bp.Floor);
                    break;

                case EBlueprintType.BuildFoundation:
                    _mapSystem.SetTerrain(bp.X, bp.Y, bp.Floor, 7); // Foundation
                    break;
            }

            bp.State = EBlueprintState.Complete;
            this.SendEvent(new SBlueprintCompletedEvent
            {
                BlueprintId = bp.BlueprintId,
                Type = bp.Type
            });

            // 标记chunk dirty (蓝图层需要更新)
            MarkBlueprintChunkDirty(bp.X, bp.Y, bp.Floor);

            _blueprintModel.RemoveBlueprint(bp.BlueprintId);
            LogKit.Log($"施工完成: [{bp.BlueprintId}] {bp.Type}");
        }

        /// <summary>
        /// 指派Pawn到蓝图
        /// </summary>
        public bool AssignPawnToBlueprint(long pawnId, long blueprintId)
        {
            var bp = _blueprintModel.GetBlueprint(blueprintId);
            if (bp == null || bp.State != EBlueprintState.Planned)
                return false;

            bp.AssignedPawnId = pawnId;
            bp.State = EBlueprintState.InProgress;

            MarkBlueprintChunkDirty(bp.X, bp.Y, bp.Floor);
            return true;
        }

        /// <summary>
        /// 取消Pawn指派
        /// </summary>
        public void UnassignPawn(long blueprintId)
        {
            var bp = _blueprintModel.GetBlueprint(blueprintId);
            if (bp == null) return;

            bp.AssignedPawnId = 0;
            if (bp.State == EBlueprintState.InProgress)
                bp.State = EBlueprintState.Planned;

            MarkBlueprintChunkDirty(bp.X, bp.Y, bp.Floor);
        }

        /// <summary>
        /// 推进蓝图工作量
        /// </summary>
        public void AddWork(long blueprintId, float workAmount)
        {
            var bp = _blueprintModel.GetBlueprint(blueprintId);
            if (bp == null || bp.State != EBlueprintState.InProgress)
                return;

            bp.WorkDone += workAmount;

            this.SendEvent(new SBlueprintProgressEvent
            {
                BlueprintId = bp.BlueprintId,
                Progress = bp.Progress
            });

            if (bp.IsComplete)
            {
                CompleteConstruction(bp);
            }
        }

        /// <summary>
        /// 驱逐指定格子上的所有Pawn到最近可行走邻格
        /// </summary>
        private void DisplacePawnsFromCell(int x, int y, int floor)
        {
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            foreach (var pawn in _pawnModel.GetAllPawns().Values)
            {
                if (pawn.X != x || pawn.Y != y || pawn.Floor != floor) continue;

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i], ny = y + dy[i];
                    if (_mapSystem.IsValidPosition(nx, ny, floor) &&
                        _mapSystem.IsCellWalkable(nx, ny, floor))
                    {
                        _mapSystem.RemoveObject(pawn.X, pawn.Y, pawn.Floor, pawn.PawnId);
                        pawn.X = nx;
                        pawn.Y = ny;
                        _mapSystem.PlaceObject(nx, ny, floor, pawn.PawnId);
                        pawn.ClearPath();
                        pawn.State = EPawnState.Idle;
                        pawn.StateTimer = 0f;
                        LogKit.Log($"Pawn {pawn.Name}: 被驱逐 ({x},{y})→({nx},{ny})");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 生成材料堆
        /// </summary>
        private void SpawnMaterials(MaterialCost[] returns, int x, int y, int floor)
        {
            if (returns == null) return;

            foreach (var cost in returns)
            {
                if (cost.Amount <= 0 || cost.Type == EMaterialType.None)
                    continue;

                var stack = new MaterialStackData
                {
                    Type = cost.Type,
                    Amount = cost.Amount,
                    X = x, Y = y, Floor = floor
                };

                long stackId = _materialModel.AddStack(stack);
                _mapSystem.PlaceObject(x, y, floor, stackId);
                LogKit.Log($"材料生成: {cost.Type} x{cost.Amount} @ ({x},{y},F{floor})");
            }
        }

        private void MarkBlueprintChunkDirty(int x, int y, int floor)
        {
            var map = _mapDataModel.CurrentMap;
            if (map == null) return;

            int cx = x / MapConst.ChunkSize;
            int cy = y / MapConst.ChunkSize;
            var chunk = map.GetChunk(cx, cy, floor);
            if (chunk != null)
            {
                chunk.MarkDirty();
                this.SendEvent(new Map.Event.SChunkDirtyEvent(cx, cy, floor));
            }
        }
    }
}
