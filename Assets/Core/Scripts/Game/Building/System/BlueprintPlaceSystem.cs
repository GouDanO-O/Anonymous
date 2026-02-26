using Core.Game.Building.Data;
using Core.Game.Building.Define;
using Core.Game.Building.Model;
using Core.Game.Building.Utility;
using Core.Game.Map.Data;
using Core.Game.Map.Define;
using Core.Game.Map.Model;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Game.Building.System
{
    /// <summary>
    /// 蓝图放置系统
    /// 处理建筑预览、验证和放置逻辑
    /// </summary>
    public class BlueprintPlaceSystem : AbstractSystem
    {
        #region 子系统引用

        private MapDataModel _mapDataModel;
        private BuildingDataModel _buildingDataModel;

        #endregion

        #region 放置状态

        /// <summary>
        /// 是否处于放置模式
        /// </summary>
        public bool IsPlacing { get; private set; }

        /// <summary>
        /// 当前选中的建筑DefName
        /// </summary>
        public string CurrentDefName { get; private set; }

        /// <summary>
        /// 当前建筑类别
        /// </summary>
        public EBuildingCategory CurrentCategory { get; private set; }

        /// <summary>
        /// 当前建筑尺寸
        /// </summary>
        public int CurrentWidth { get; private set; }
        public int CurrentHeight { get; private set; }

        /// <summary>
        /// 当前墙体方向
        /// </summary>
        public EWallDirection CurrentWallDirection { get; private set; }

        /// <summary>
        /// 当前预览位置
        /// </summary>
        public int PreviewCellX { get; private set; }
        public int PreviewCellY { get; private set; }
        public int PreviewFloor { get; private set; }

        /// <summary>
        /// 当前预览是否合法
        /// </summary>
        public bool IsPreviewValid { get; private set; }

        /// <summary>
        /// 验证失败原因
        /// </summary>
        public string ValidateReason { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 进入放置模式事件
        /// </summary>
        public UnityAction<string, EBuildingCategory> OnEnterPlaceMode;

        /// <summary>
        /// 退出放置模式事件
        /// </summary>
        public UnityAction OnExitPlaceMode;

        /// <summary>
        /// 预览更新事件（cellX, cellY, floor, isValid）
        /// </summary>
        public UnityAction<int, int, int, bool> OnPreviewUpdated;

        /// <summary>
        /// 放置成功事件
        /// </summary>
        public UnityAction<BlueprintData> OnPlaceSuccess;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
            _mapDataModel = this.GetModel<MapDataModel>();
            _buildingDataModel = this.GetModel<BuildingDataModel>();
        }

        protected override void OnDeinit()
        {
        }

        #endregion

        #region 公共方法 - 放置模式

        /// <summary>
        /// 进入放置模式
        /// </summary>
        public void EnterPlaceMode(string defName, EBuildingCategory category,
            int width = 1, int height = 1)
        {
            CurrentDefName = defName;
            CurrentCategory = category;
            CurrentWidth = width;
            CurrentHeight = height;
            CurrentWallDirection = EWallDirection.North;
            IsPlacing = true;
            IsPreviewValid = false;

            OnEnterPlaceMode?.Invoke(defName, category);
        }

        /// <summary>
        /// 退出放置模式
        /// </summary>
        public void ExitPlaceMode()
        {
            IsPlacing = false;
            CurrentDefName = null;

            OnExitPlaceMode?.Invoke();
        }

        /// <summary>
        /// 切换墙体方向（North <-> West）
        /// </summary>
        public void ToggleWallDirection()
        {
            CurrentWallDirection = CurrentWallDirection == EWallDirection.North
                ? EWallDirection.West
                : EWallDirection.North;

            // 重新验证
            UpdatePreview(PreviewCellX, PreviewCellY, PreviewFloor);
        }

        #endregion

        #region 公共方法 - 预览与放置

        /// <summary>
        /// 更新预览位置（鼠标移动时调用）
        /// </summary>
        public void UpdatePreview(int cellX, int cellY, int floor)
        {
            if (!IsPlacing) return;

            PreviewCellX = cellX;
            PreviewCellY = cellY;
            PreviewFloor = floor;

            // 执行验证
            var result = BuildingValidateUtility.CanPlace(
                _mapDataModel.CurrentMap, _buildingDataModel,
                CurrentDefName, CurrentCategory,
                cellX, cellY, floor,
                CurrentWidth, CurrentHeight,
                CurrentWallDirection);

            IsPreviewValid = result.IsValid;
            ValidateReason = result.Reason;

            OnPreviewUpdated?.Invoke(cellX, cellY, floor, IsPreviewValid);
        }

        /// <summary>
        /// 确认放置（鼠标点击时调用）
        /// </summary>
        public bool ConfirmPlace()
        {
            if (!IsPlacing || !IsPreviewValid)
                return false;

            // 创建蓝图
            int blueprintId = _buildingDataModel.AllocateBlueprintId();
            var blueprint = new BlueprintData(
                blueprintId, CurrentDefName, CurrentCategory,
                PreviewCellX, PreviewCellY, PreviewFloor)
            {
                Width = CurrentWidth,
                Height = CurrentHeight,
                WallDirection = CurrentWallDirection,
                State = EBlueprintState.Placed
            };

            // 应用到地图数据
            ApplyBlueprintToMap(blueprint);

            // 添加到数据模型
            _buildingDataModel.AddBlueprint(blueprint);

            OnPlaceSuccess?.Invoke(blueprint);

            return true;
        }

        /// <summary>
        /// 取消放置中的蓝图
        /// </summary>
        public bool CancelBlueprint(int blueprintId)
        {
            var blueprint = _buildingDataModel.GetBlueprint(blueprintId);
            if (blueprint == null) return false;

            // 从地图数据移除
            RemoveBlueprintFromMap(blueprint);

            return _buildingDataModel.CancelBlueprint(blueprintId);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 将蓝图信息写入地图数据
        /// </summary>
        private void ApplyBlueprintToMap(BlueprintData blueprint)
        {
            var mapData = _mapDataModel.CurrentMap;
            if (mapData == null) return;

            if (blueprint.IsWallType)
            {
                // 墙体/门：通过CellData的ObjectIds标记蓝图存在
                var cell = mapData.GetCell(blueprint.CellX, blueprint.CellY, blueprint.Floor);
                if (cell != null)
                {
                    cell.AddObject(blueprint.BlueprintId);
                    mapData.MarkCellDirty(blueprint.CellX, blueprint.CellY, blueprint.Floor);
                    _mapDataModel.NotifyCellChanged(blueprint.CellX, blueprint.CellY, blueprint.Floor);
                }
            }
            else
            {
                // 物体类：标记占用区域
                for (int dy = 0; dy < blueprint.Height; dy++)
                {
                    for (int dx = 0; dx < blueprint.Width; dx++)
                    {
                        int x = blueprint.CellX + dx;
                        int y = blueprint.CellY + dy;

                        var cell = mapData.GetCell(x, y, blueprint.Floor);
                        if (cell != null)
                        {
                            cell.AddObject(blueprint.BlueprintId);
                            cell.AddFlag(ECellFlags.Occupied);
                            mapData.MarkCellDirty(x, y, blueprint.Floor);
                            _mapDataModel.NotifyCellChanged(x, y, blueprint.Floor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从地图数据移除蓝图
        /// </summary>
        private void RemoveBlueprintFromMap(BlueprintData blueprint)
        {
            var mapData = _mapDataModel.CurrentMap;
            if (mapData == null) return;

            if (blueprint.IsWallType)
            {
                var cell = mapData.GetCell(blueprint.CellX, blueprint.CellY, blueprint.Floor);
                if (cell != null)
                {
                    cell.RemoveObject(blueprint.BlueprintId);
                    mapData.MarkCellDirty(blueprint.CellX, blueprint.CellY, blueprint.Floor);
                }
            }
            else
            {
                for (int dy = 0; dy < blueprint.Height; dy++)
                {
                    for (int dx = 0; dx < blueprint.Width; dx++)
                    {
                        int x = blueprint.CellX + dx;
                        int y = blueprint.CellY + dy;

                        var cell = mapData.GetCell(x, y, blueprint.Floor);
                        if (cell != null)
                        {
                            cell.RemoveObject(blueprint.BlueprintId);
                            cell.RemoveFlag(ECellFlags.Occupied);
                            mapData.MarkCellDirty(x, y, blueprint.Floor);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
