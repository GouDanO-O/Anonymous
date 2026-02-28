using System.Collections.Generic;
using Core.Game.Resource.Data;
using Core.Game.Resource.Define;
using GDFrameworkCore;

namespace Core.Game.Resource.Model
{
    public class MaterialDataModel : AbstractModel
    {
        public const long MaterialStackIdStart = 2_000_000;

        private readonly Dictionary<long, MaterialStackData> _stacks = new();
        private long _nextStackId = MaterialStackIdStart;

        protected override void OnInit()
        {
        }

        public long AddStack(MaterialStackData data)
        {
            long id = _nextStackId++;
            data.StackId = id;
            _stacks[id] = data;
            return id;
        }

        public void RemoveStack(long stackId)
        {
            _stacks.Remove(stackId);
        }

        public MaterialStackData GetStack(long stackId)
        {
            return _stacks.GetValueOrDefault(stackId);
        }

        /// <summary>
        /// 获取指定位置的所有材料堆
        /// </summary>
        public List<MaterialStackData> GetStacksAt(int x, int y, int floor)
        {
            var result = new List<MaterialStackData>();
            foreach (var stack in _stacks.Values)
            {
                if (stack.X == x && stack.Y == y && stack.Floor == floor)
                    result.Add(stack);
            }
            return result;
        }

        /// <summary>
        /// 获取指定位置指定类型的材料总量
        /// </summary>
        public int GetMaterialAmountAt(int x, int y, int floor, EMaterialType type)
        {
            int total = 0;
            foreach (var stack in _stacks.Values)
            {
                if (stack.X == x && stack.Y == y && stack.Floor == floor && stack.Type == type)
                    total += stack.Amount;
            }
            return total;
        }

        public IReadOnlyDictionary<long, MaterialStackData> GetAllStacks()
        {
            return _stacks;
        }

        public int StackCount => _stacks.Count;
    }
}
