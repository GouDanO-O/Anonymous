using System;
using Core.Game.Pawn.Define;

namespace Core.Game.Pawn.Data
{
    [Serializable]
    public struct NeedData
    {
        /// <summary>
        /// 饥饿度 (100=饱, 0=极度饥饿)
        /// </summary>
        public float Hunger;

        /// <summary>
        /// 疲劳度 (100=精力充沛, 0=极度疲劳)
        /// </summary>
        public float Fatigue;

        /// <summary>
        /// 心情 (100=很好, 0=崩溃)
        /// </summary>
        public float Mood;

        public static NeedData CreateDefault()
        {
            return new NeedData
            {
                Hunger = PawnConst.MaxNeedValue,
                Fatigue = PawnConst.MaxNeedValue,
                Mood = PawnConst.MaxNeedValue
            };
        }
    }
}
