using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Action
{
    [Serializable]
    public class ShakeScreenAction : BaseAction
    {
        [LabelText("震动强度"), Range(1, 10)]
        public int strength = 5;
        
        public override void Execute()
        {
            
        }
    }
}