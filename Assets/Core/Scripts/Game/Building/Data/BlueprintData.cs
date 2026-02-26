using System;
using Core.Game.Building.Define;
using Core.Game.Map.Define;

namespace Core.Game.Building.Data
{
    /// <summary>
    /// 蓝图数据（待建造的建筑信息）
    /// </summary>
    [Serializable]
    public class BlueprintData
    {
        #region 基础属性

        /// <summary>
        /// 蓝图ID
        /// </summary>
        public int BlueprintId;

        /// <summary>
        /// 建筑配置名（关联Luban BuildingDef）
        /// </summary>
        public string DefName;

        /// <summary>
        /// 建筑类别
        /// </summary>
        public EBuildingCategory Category;

        #endregion

        #region 位置

        /// <summary>
        /// 放置格子X
        /// </summary>
        public int CellX;

        /// <summary>
        /// 放置格子Y
        /// </summary>
        public int CellY;

        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor;

        /// <summary>
        /// 墙体方向（仅墙/门类建筑有效）
        /// </summary>
        public EWallDirection WallDirection;

        #endregion

        #region 尺寸

        /// <summary>
        /// 宽度（格子数）
        /// </summary>
        public int Width;

        /// <summary>
        /// 高度（格子数）
        /// </summary>
        public int Height;

        #endregion

        #region 状态

        /// <summary>
        /// 蓝图状态
        /// </summary>
        public EBlueprintState State;

        /// <summary>
        /// 建造进度（0~1）
        /// </summary>
        public float BuildProgress;

        /// <summary>
        /// 需要的总工作量
        /// </summary>
        public float WorkToBuild;

        /// <summary>
        /// 已完成的工作量
        /// </summary>
        public float WorkDone;

        /// <summary>
        /// 当前建造者PawnId（-1表示无人建造）
        /// </summary>
        public int BuilderPawnId;

        #endregion

        #region 属性访问器

        /// <summary>
        /// 是否已放置
        /// </summary>
        public bool IsPlaced => State == EBlueprintState.Placed;

        /// <summary>
        /// 是否正在建造
        /// </summary>
        public bool IsBuilding => State == EBlueprintState.Building;

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted => State == EBlueprintState.Completed;

        /// <summary>
        /// 是否是墙体类建筑
        /// </summary>
        public bool IsWallType => Category == EBuildingCategory.Wall || Category == EBuildingCategory.Door;

        #endregion

        #region 构造函数

        public BlueprintData()
        {
            State = EBlueprintState.Preview;
            BuilderPawnId = -1;
            Width = 1;
            Height = 1;
            WorkToBuild = BuildingDefine.DefaultWorkToBuild;
        }

        public BlueprintData(int blueprintId, string defName, EBuildingCategory category,
            int cellX, int cellY, int floor) : this()
        {
            BlueprintId = blueprintId;
            DefName = defName;
            Category = category;
            CellX = cellX;
            CellY = cellY;
            Floor = floor;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加建造进度
        /// </summary>
        public bool AddWork(float amount)
        {
            WorkDone += amount;
            BuildProgress = WorkToBuild > 0 ? WorkDone / WorkToBuild : 1f;

            if (WorkDone >= WorkToBuild)
            {
                WorkDone = WorkToBuild;
                BuildProgress = 1f;
                return true; // 建造完成
            }

            return false;
        }

        /// <summary>
        /// 检查位置是否在蓝图范围内
        /// </summary>
        public bool ContainsCell(int x, int y)
        {
            return x >= CellX && x < CellX + Width &&
                   y >= CellY && y < CellY + Height;
        }

        public override string ToString()
        {
            return $"Blueprint({BlueprintId}, {DefName}, ({CellX},{CellY},F{Floor}), {State})";
        }

        #endregion
    }
}
