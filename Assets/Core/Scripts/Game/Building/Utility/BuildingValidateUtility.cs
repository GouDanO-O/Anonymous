using Core.Game.Building.Data;
using Core.Game.Building.Define;
using Core.Game.Building.Model;
using Core.Game.Map.Data;
using Core.Game.Map.Define;

namespace Core.Game.Building.Utility
{
    /// <summary>
    /// 建筑放置验证工具
    /// </summary>
    public static class BuildingValidateUtility
    {
        /// <summary>
        /// 验证结果
        /// </summary>
        public struct ValidateResult
        {
            public bool IsValid;
            public string Reason;

            public static ValidateResult Success() => new ValidateResult { IsValid = true };

            public static ValidateResult Fail(string reason) =>
                new ValidateResult { IsValid = false, Reason = reason };
        }

        #region 通用验证

        /// <summary>
        /// 验证建筑是否可以放置
        /// </summary>
        public static ValidateResult CanPlace(MapData mapData, BuildingDataModel buildingModel,
            string defName, EBuildingCategory category,
            int cellX, int cellY, int floor,
            int width, int height,
            EWallDirection wallDirection = EWallDirection.North)
        {
            if (mapData == null)
                return ValidateResult.Fail("地图未加载");

            // 墙体类建筑用专门的验证
            if (category == EBuildingCategory.Wall || category == EBuildingCategory.Door)
            {
                return CanPlaceWall(mapData, buildingModel, cellX, cellY, floor, wallDirection);
            }

            // 地板类建筑
            if (category == EBuildingCategory.Floor)
            {
                return CanPlaceFloor(mapData, cellX, cellY, floor);
            }

            // 物体类建筑（家具/工作台/装饰等）
            return CanPlaceObject(mapData, buildingModel, cellX, cellY, floor, width, height);
        }

        #endregion

        #region 墙体验证

        /// <summary>
        /// 验证墙体是否可以放置
        /// </summary>
        public static ValidateResult CanPlaceWall(MapData mapData, BuildingDataModel buildingModel,
            int cellX, int cellY, int floor, EWallDirection direction)
        {
            // 边界检查
            if (!mapData.IsValidCellPos(cellX, cellY, floor))
                return ValidateResult.Fail("超出地图边界");

            var cell = mapData.GetCell(cellX, cellY, floor);
            if (cell == null)
                return ValidateResult.Fail("无效的格子");

            // 检查目标方向是否已有墙
            var existingWall = cell.GetWall(direction);
            if (existingWall.HasWall)
                return ValidateResult.Fail("该位置已有墙体");

            // 检查是否在水面上
            if (cell.GroundType == EGroundType.Water)
                return ValidateResult.Fail("无法在水面上建墙");

            return ValidateResult.Success();
        }

        #endregion

        #region 地板验证

        /// <summary>
        /// 验证地板是否可以放置
        /// </summary>
        public static ValidateResult CanPlaceFloor(MapData mapData, int cellX, int cellY, int floor)
        {
            if (!mapData.IsValidCellPos(cellX, cellY, floor))
                return ValidateResult.Fail("超出地图边界");

            var cell = mapData.GetCell(cellX, cellY, floor);
            if (cell == null)
                return ValidateResult.Fail("无效的格子");

            // 检查是否已有地板
            if (cell.HasFloor)
                return ValidateResult.Fail("该位置已有地板");

            // 检查是否在水面上
            if (cell.GroundType == EGroundType.Water)
                return ValidateResult.Fail("无法在水面上铺设地板");

            return ValidateResult.Success();
        }

        #endregion

        #region 物体验证

        /// <summary>
        /// 验证物体是否可以放置（家具/工作台等）
        /// </summary>
        public static ValidateResult CanPlaceObject(MapData mapData, BuildingDataModel buildingModel,
            int cellX, int cellY, int floor, int width, int height)
        {
            // 检查所有占用格子
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    int x = cellX + dx;
                    int y = cellY + dy;

                    // 边界检查
                    if (!mapData.IsValidCellPos(x, y, floor))
                        return ValidateResult.Fail("超出地图边界");

                    var cell = mapData.GetCell(x, y, floor);
                    if (cell == null)
                        return ValidateResult.Fail("无效的格子");

                    // 可建造检查
                    if (!cell.IsBuildable)
                        return ValidateResult.Fail($"格子({x},{y})不可建造");

                    // 可行走检查（物体需要放在可行走的地面上）
                    if (!cell.IsWalkable)
                        return ValidateResult.Fail($"格子({x},{y})不可通行");

                    // 占用检查
                    if ((cell.Flags & ECellFlags.Occupied) != 0)
                        return ValidateResult.Fail($"格子({x},{y})已被占用");

                    // 水面检查
                    if (cell.GroundType == EGroundType.Water)
                        return ValidateResult.Fail("无法在水面上建造");

                    // 已有蓝图检查
                    if (buildingModel != null)
                    {
                        var existingBp = buildingModel.GetBlueprintAtCell(x, y, floor);
                        if (existingBp != null)
                            return ValidateResult.Fail($"格子({x},{y})已有蓝图");

                        var existingBuilding = buildingModel.GetBuildingAtCell(x, y, floor);
                        if (existingBuilding != null)
                            return ValidateResult.Fail($"格子({x},{y})已有建筑");
                    }
                }
            }

            return ValidateResult.Success();
        }

        #endregion
    }
}
