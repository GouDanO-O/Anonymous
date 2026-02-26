using System.Collections.Generic;
using Core.Game.Building.Data;
using Core.Game.Building.Define;
using GDFrameworkCore;
using UnityEngine.Events;

namespace Core.Game.Building.Model
{
    /// <summary>
    /// 建筑数据模型
    /// 管理所有蓝图和建筑实例的增删查改
    /// </summary>
    public class BuildingDataModel : AbstractModel
    {
        #region 数据

        /// <summary>
        /// 蓝图数据（BlueprintId -> BlueprintData）
        /// </summary>
        private readonly Dictionary<int, BlueprintData> _blueprintMap = new Dictionary<int, BlueprintData>();

        /// <summary>
        /// 建筑实例（BuildingId -> BuildingInstance）
        /// </summary>
        private readonly Dictionary<int, BuildingInstance> _buildingMap = new Dictionary<int, BuildingInstance>();

        /// <summary>
        /// 下一个蓝图ID
        /// </summary>
        private int _nextBlueprintId = BuildingDefine.BlueprintIdStart;

        /// <summary>
        /// 下一个建筑ID
        /// </summary>
        private int _nextBuildingId = BuildingDefine.BuildingIdStart;

        #endregion

        #region 事件

        /// <summary>
        /// 蓝图放置事件
        /// </summary>
        public UnityAction<BlueprintData> OnBlueprintPlaced;

        /// <summary>
        /// 蓝图取消事件
        /// </summary>
        public UnityAction<int> OnBlueprintCancelled;

        /// <summary>
        /// 建筑建成事件
        /// </summary>
        public UnityAction<BuildingInstance> OnBuildingCompleted;

        /// <summary>
        /// 建筑销毁事件
        /// </summary>
        public UnityAction<int> OnBuildingDestroyed;

        /// <summary>
        /// 蓝图建造进度变更
        /// </summary>
        public UnityAction<int, float> OnBlueprintProgressChanged;

        #endregion

        #region 属性

        public int BlueprintCount => _blueprintMap.Count;
        public int BuildingCount => _buildingMap.Count;

        #endregion

        #region 初始化

        protected override void OnInit()
        {
        }

        #endregion

        #region 蓝图管理

        /// <summary>
        /// 分配新的蓝图ID
        /// </summary>
        public int AllocateBlueprintId()
        {
            return _nextBlueprintId++;
        }

        /// <summary>
        /// 添加蓝图
        /// </summary>
        public void AddBlueprint(BlueprintData blueprint)
        {
            _blueprintMap[blueprint.BlueprintId] = blueprint;
            OnBlueprintPlaced?.Invoke(blueprint);
        }

        /// <summary>
        /// 获取蓝图
        /// </summary>
        public BlueprintData GetBlueprint(int blueprintId)
        {
            return _blueprintMap.TryGetValue(blueprintId, out var bp) ? bp : null;
        }

        /// <summary>
        /// 取消蓝图
        /// </summary>
        public bool CancelBlueprint(int blueprintId)
        {
            if (!_blueprintMap.TryGetValue(blueprintId, out var bp))
                return false;

            bp.State = EBlueprintState.Cancelled;
            _blueprintMap.Remove(blueprintId);
            OnBlueprintCancelled?.Invoke(blueprintId);
            return true;
        }

        /// <summary>
        /// 移除蓝图（建成后调用）
        /// </summary>
        public void RemoveBlueprint(int blueprintId)
        {
            _blueprintMap.Remove(blueprintId);
        }

        /// <summary>
        /// 获取所有待建造的蓝图
        /// </summary>
        public List<BlueprintData> GetPendingBlueprints()
        {
            var result = new List<BlueprintData>();
            foreach (var bp in _blueprintMap.Values)
            {
                if (bp.State == EBlueprintState.Placed)
                    result.Add(bp);
            }

            return result;
        }

        /// <summary>
        /// 获取指定位置的蓝图
        /// </summary>
        public BlueprintData GetBlueprintAtCell(int cellX, int cellY, int floor)
        {
            foreach (var bp in _blueprintMap.Values)
            {
                if (bp.Floor == floor && bp.ContainsCell(cellX, cellY))
                    return bp;
            }

            return null;
        }

        /// <summary>
        /// 更新蓝图建造进度
        /// </summary>
        public void UpdateBlueprintProgress(int blueprintId, float progress)
        {
            OnBlueprintProgressChanged?.Invoke(blueprintId, progress);
        }

        #endregion

        #region 建筑实例管理

        /// <summary>
        /// 添加建筑实例
        /// </summary>
        public BuildingInstance AddBuilding(BlueprintData blueprint, int maxHealth)
        {
            int buildingId = _nextBuildingId++;
            var building = BuildingInstance.FromBlueprint(blueprint, buildingId, maxHealth);

            _buildingMap[buildingId] = building;
            OnBuildingCompleted?.Invoke(building);

            return building;
        }

        /// <summary>
        /// 获取建筑实例
        /// </summary>
        public BuildingInstance GetBuilding(int buildingId)
        {
            return _buildingMap.TryGetValue(buildingId, out var b) ? b : null;
        }

        /// <summary>
        /// 销毁建筑实例
        /// </summary>
        public bool DestroyBuilding(int buildingId)
        {
            if (!_buildingMap.Remove(buildingId))
                return false;

            OnBuildingDestroyed?.Invoke(buildingId);
            return true;
        }

        /// <summary>
        /// 获取指定位置的建筑
        /// </summary>
        public BuildingInstance GetBuildingAtCell(int cellX, int cellY, int floor)
        {
            foreach (var b in _buildingMap.Values)
            {
                if (b.Floor == floor && b.ContainsCell(cellX, cellY))
                    return b;
            }

            return null;
        }

        /// <summary>
        /// 获取所有建筑
        /// </summary>
        public IEnumerable<BuildingInstance> GetAllBuildings()
        {
            return _buildingMap.Values;
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void ClearAll()
        {
            _blueprintMap.Clear();
            _buildingMap.Clear();
        }

        #endregion
    }
}
