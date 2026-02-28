using Core.Game.Resource.Define;
using UnityEngine;

namespace Core.Game.Resource.Data
{
    /// <summary>
    /// 建造材料消耗/返还
    /// </summary>
    public struct MaterialCost
    {
        public EMaterialType Type;
        public int Amount;

        public MaterialCost(EMaterialType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    /// <summary>
    /// 材料定义 (硬编码临时数据)
    /// </summary>
    public class MaterialDef
    {
        public int Id;
        public string Name;
        public Color Color;
        public EMaterialType Type;
    }

    /// <summary>
    /// 世界中的一个材料堆
    /// </summary>
    public class MaterialStackData
    {
        public long StackId;
        public EMaterialType Type;
        public int Amount;
        public int X;
        public int Y;
        public int Floor;
    }
}
