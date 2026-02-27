using Core.Game.Pawn.Define;
using Core.Game.Pawn.Event;
using Core.Game.Pawn.Model;
using GDFrameworkCore;
using UnityEngine;

namespace Core.Game.Pawn.System
{
    /// <summary>
    /// Pawn需求系统: 饥饿/疲劳/心情 衰减
    /// </summary>
    public class PawnNeedSystem : AbstractSystem
    {
        private PawnDataModel _pawnModel;

        protected override void OnInit()
        {
            _pawnModel = this.GetModel<PawnDataModel>();
        }

        public void Tick(float deltaTime)
        {
            foreach (var pawn in _pawnModel.GetAllPawns().Values)
            {
                var needs = pawn.Needs;

                // 基础衰减
                needs.Hunger -= PawnConst.HungerDecayRate * deltaTime;
                needs.Fatigue -= PawnConst.FatigueDecayRate * deltaTime;
                needs.Mood -= PawnConst.MoodBaseDecayRate * deltaTime;

                // 饥饿和疲劳低时额外影响心情
                if (needs.Hunger < PawnConst.HungerCriticalThreshold)
                    needs.Mood -= 0.2f * deltaTime;
                if (needs.Fatigue < PawnConst.FatigueCriticalThreshold)
                    needs.Mood -= 0.15f * deltaTime;

                // Clamp
                needs.Hunger = Mathf.Clamp(needs.Hunger, PawnConst.MinNeedValue, PawnConst.MaxNeedValue);
                needs.Fatigue = Mathf.Clamp(needs.Fatigue, PawnConst.MinNeedValue, PawnConst.MaxNeedValue);
                needs.Mood = Mathf.Clamp(needs.Mood, PawnConst.MinNeedValue, PawnConst.MaxNeedValue);

                pawn.Needs = needs;
            }
        }
    }
}
