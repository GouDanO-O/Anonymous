using Core.Game.Building.Data;
using Core.Game.Building.Define;
using Core.Game.Building.Model;
using Core.Game.Building.View;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using Core.Game.Map.System;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Building.System
{
    /// <summary>
    /// 建造主系统
    /// 管理建筑生命周期：蓝图放置 → 建造 → 建成 → 销毁
    /// </summary>
    public class BuildingSystem : AbstractSystem
    {
        #region 子系统引用

        private BuildingDataModel _buildingDataModel;
        private MapDataModel _mapDataModel;
        private BlueprintPlaceSystem _placeSystem;
        private RoomSystem _roomSystem;

        #endregion

        #region 视图引用

        private BlueprintPreview _blueprintPreview;

        #endregion

        #region 事件

        /// <summary>
        /// 建筑建成事件
        /// </summary>
        public UnityAction<BuildingInstance> OnBuildingBuilt;

        /// <summary>
        /// 建筑销毁事件
        /// </summary>
        public UnityAction<int> OnBuildingRemoved;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _buildingDataModel = this.GetModel<BuildingDataModel>();
            _mapDataModel = this.GetModel<MapDataModel>();
            _placeSystem = this.GetSystem<BlueprintPlaceSystem>();
            _roomSystem = this.GetSystem<RoomSystem>();
        }

        protected override void OnDeinit()
        {
        }

        #endregion

        #region 公共方法 - 视图

        /// <summary>
        /// 设置蓝图预览视图
        /// </summary>
        public void SetBlueprintPreview(BlueprintPreview preview)
        {
            _blueprintPreview = preview;
        }

        #endregion

        #region 公共方法 - 建造流程

        /// <summary>
        /// 对蓝图执行建造（Pawn施工时调用）
        /// </summary>
        public bool DoWork(int blueprintId, float workAmount)
        {
            var blueprint = _buildingDataModel.GetBlueprint(blueprintId);
            if (blueprint == null || blueprint.State == EBlueprintState.Cancelled)
                return false;

            // 标记为建造中
            if (blueprint.State == EBlueprintState.Placed)
            {
                blueprint.State = EBlueprintState.Building;
            }

            // 增加进度
            bool completed = blueprint.AddWork(workAmount);
            _buildingDataModel.UpdateBlueprintProgress(blueprintId, blueprint.BuildProgress);

            if (completed)
            {
                CompleteBuilding(blueprint);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 立即完成建筑（调试/作弊用）
        /// </summary>
        public void CompleteImmediately(int blueprintId)
        {
            var blueprint = _buildingDataModel.GetBlueprint(blueprintId);
            if (blueprint == null) return;

            blueprint.WorkDone = blueprint.WorkToBuild;
            blueprint.BuildProgress = 1f;
            CompleteBuilding(blueprint);
        }

        /// <summary>
        /// 销毁已建成建筑
        /// </summary>
        public void DestroyBuilding(int buildingId)
        {
            var building = _buildingDataModel.GetBuilding(buildingId);
            if (building == null) return;

            // 从地图数据移除
            RemoveBuildingFromMap(building);

            _buildingDataModel.DestroyBuilding(buildingId);
            OnBuildingRemoved?.Invoke(buildingId);
        }

        #endregion

        #region 公共方法 - 放置快捷方法

        /// <summary>
        /// 开始放置墙体
        /// </summary>
        public void StartPlaceWall(string defName)
        {
            _placeSystem.EnterPlaceMode(defName, EBuildingCategory.Wall);
        }

        /// <summary>
        /// 开始放置门
        /// </summary>
        public void StartPlaceDoor(string defName)
        {
            _placeSystem.EnterPlaceMode(defName, EBuildingCategory.Door);
        }

        /// <summary>
        /// 开始放置地板
        /// </summary>
        public void StartPlaceFloor(string defName)
        {
            _placeSystem.EnterPlaceMode(defName, EBuildingCategory.Floor);
        }

        /// <summary>
        /// 开始放置家具
        /// </summary>
        public void StartPlaceFurniture(string defName, int width = 1, int height = 1)
        {
            _placeSystem.EnterPlaceMode(defName, EBuildingCategory.Furniture, width, height);
        }

        /// <summary>
        /// 取消放置模式
        /// </summary>
        public void CancelPlaceMode()
        {
            _placeSystem.ExitPlaceMode();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 完成建筑建造
        /// </summary>
        private void CompleteBuilding(BlueprintData blueprint)
        {
            blueprint.State = EBlueprintState.Completed;

            // 应用建筑到地图
            ApplyBuildingToMap(blueprint);

            // 创建建筑实例
            int maxHealth = 100; // 后续从Luban配置读取
            var building = _buildingDataModel.AddBuilding(blueprint, maxHealth);

            // 移除蓝图
            _buildingDataModel.RemoveBlueprint(blueprint.BlueprintId);

            // 重新检测房间（墙体变化会影响房间划分）
            if (blueprint.IsWallType)
            {
                _roomSystem?.DetectRoomsInFloor(blueprint.Floor);
            }

            OnBuildingBuilt?.Invoke(building);

            Debug.Log($"[BuildingSystem] Building completed: {building}");
        }

        /// <summary>
        /// 将建成建筑应用到地图数据
        /// </summary>
        private void ApplyBuildingToMap(BlueprintData blueprint)
        {
            var mapData = _mapDataModel.CurrentMap;
            if (mapData == null) return;

            switch (blueprint.Category)
            {
                case EBuildingCategory.Wall:
                    ApplyWall(mapData, blueprint, false);
                    break;
                case EBuildingCategory.Door:
                    ApplyWall(mapData, blueprint, true);
                    break;
                case EBuildingCategory.Floor:
                    ApplyFloor(mapData, blueprint);
                    break;
                default:
                    ApplyObject(mapData, blueprint);
                    break;
            }
        }

        /// <summary>
        /// 应用墙体
        /// </summary>
        private void ApplyWall(MapData mapData, BlueprintData blueprint, bool isDoor)
        {
            var cell = mapData.GetCell(blueprint.CellX, blueprint.CellY, blueprint.Floor);
            if (cell == null) return;

            // 移除蓝图标记
            cell.RemoveObject(blueprint.BlueprintId);

            WallData wall;
            if (isDoor)
            {
                wall = WallData.CreateWithDoor(EWallType.Wood, EDoorState.Closed);
            }
            else
            {
                wall = WallData.CreateSolid(EWallType.Wood);
            }

            cell.SetWall(blueprint.WallDirection, wall);
            mapData.MarkCellDirty(blueprint.CellX, blueprint.CellY, blueprint.Floor);

            // 通知数据变更触发Mesh重建
            _mapDataModel.NotifyCellChanged(blueprint.CellX, blueprint.CellY, blueprint.Floor);
        }

        /// <summary>
        /// 应用地板
        /// </summary>
        private void ApplyFloor(MapData mapData, BlueprintData blueprint)
        {
            var cell = mapData.GetCell(blueprint.CellX, blueprint.CellY, blueprint.Floor);
            if (cell == null) return;

            cell.RemoveObject(blueprint.BlueprintId);
            _mapDataModel.SetFloorType(blueprint.CellX, blueprint.CellY, blueprint.Floor, EFloorType.Wood);
        }

        /// <summary>
        /// 应用物体
        /// </summary>
        private void ApplyObject(MapData mapData, BlueprintData blueprint)
        {
            for (int dy = 0; dy < blueprint.Height; dy++)
            {
                for (int dx = 0; dx < blueprint.Width; dx++)
                {
                    int x = blueprint.CellX + dx;
                    int y = blueprint.CellY + dy;

                    var cell = mapData.GetCell(x, y, blueprint.Floor);
                    if (cell == null) continue;

                    // 蓝图ID替换为建筑ID（由外部处理）
                    cell.RemoveObject(blueprint.BlueprintId);
                    // Occupied标记保留（建筑本身占用空间）

                    mapData.MarkCellDirty(x, y, blueprint.Floor);
                }
            }
        }

        /// <summary>
        /// 从地图数据移除建筑
        /// </summary>
        private void RemoveBuildingFromMap(BuildingInstance building)
        {
            var mapData = _mapDataModel.CurrentMap;
            if (mapData == null) return;

            if (building.Category == EBuildingCategory.Wall || building.Category == EBuildingCategory.Door)
            {
                // 移除墙体
                var cell = mapData.GetCell(building.CellX, building.CellY, building.Floor);
                if (cell != null)
                {
                    // 尝试移除两个方向的墙（简化处理）
                    if (cell.HasWallNorth) cell.RemoveWall(EWallDirection.North);
                    if (cell.HasWallWest) cell.RemoveWall(EWallDirection.West);

                    mapData.MarkCellDirty(building.CellX, building.CellY, building.Floor);
                    _mapDataModel.NotifyCellChanged(building.CellX, building.CellY, building.Floor);
                }

                // 重新检测房间
                _roomSystem?.DetectRoomsInFloor(building.Floor);
            }
            else
            {
                // 移除物体，清除Occupied标记
                for (int dy = 0; dy < building.Height; dy++)
                {
                    for (int dx = 0; dx < building.Width; dx++)
                    {
                        int x = building.CellX + dx;
                        int y = building.CellY + dy;

                        var cell = mapData.GetCell(x, y, building.Floor);
                        if (cell != null)
                        {
                            cell.RemoveFlag(ECellFlags.Occupied);
                            mapData.MarkCellDirty(x, y, building.Floor);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
