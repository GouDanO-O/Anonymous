using System.Collections.Generic;
using Core.Game.Blueprint.Data;
using Core.Game.Blueprint.Define;
using GDFrameworkCore;

namespace Core.Game.Blueprint.Model
{
    public class BlueprintDataModel : AbstractModel
    {
        public const long BlueprintIdStart = 3_000_000;

        private readonly Dictionary<long, BlueprintData> _blueprints = new();
        private long _nextBlueprintId = BlueprintIdStart;

        protected override void OnInit()
        {
        }

        public long AddBlueprint(BlueprintData data)
        {
            long id = _nextBlueprintId++;
            data.BlueprintId = id;
            _blueprints[id] = data;
            return id;
        }

        public void RemoveBlueprint(long blueprintId)
        {
            _blueprints.Remove(blueprintId);
        }

        public BlueprintData GetBlueprint(long blueprintId)
        {
            return _blueprints.GetValueOrDefault(blueprintId);
        }

        public IReadOnlyDictionary<long, BlueprintData> GetAllBlueprints()
        {
            return _blueprints;
        }

        /// <summary>
        /// 获取指定楼层的所有蓝图
        /// </summary>
        public List<BlueprintData> GetBlueprintsOnFloor(int floor)
        {
            var result = new List<BlueprintData>();
            foreach (var bp in _blueprints.Values)
            {
                if (bp.Floor == floor && bp.State != EBlueprintState.Cancelled)
                    result.Add(bp);
            }
            return result;
        }

        /// <summary>
        /// 获取未指派的蓝图 (给Pawn AI使用)
        /// </summary>
        public List<BlueprintData> GetUnassignedBlueprints()
        {
            var result = new List<BlueprintData>();
            foreach (var bp in _blueprints.Values)
            {
                if (bp.State == EBlueprintState.Planned && bp.AssignedPawnId == 0)
                    result.Add(bp);
            }
            return result;
        }

        /// <summary>
        /// 获取指定位置的蓝图
        /// </summary>
        public BlueprintData GetBlueprintAt(int x, int y, int floor)
        {
            foreach (var bp in _blueprints.Values)
            {
                if (bp.X == x && bp.Y == y && bp.Floor == floor
                    && bp.State != EBlueprintState.Cancelled
                    && bp.State != EBlueprintState.Complete)
                    return bp;
            }
            return null;
        }

        public int BlueprintCount => _blueprints.Count;
    }
}
