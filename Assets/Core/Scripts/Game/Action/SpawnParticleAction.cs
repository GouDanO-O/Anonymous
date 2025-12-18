using System;
using GDFramework.Asset;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Action
{
    [Serializable]
    public class SpawnParticleAction : BaseAction
    {
        [LabelText("粒子对象ID"), AssetIDSelector(EAssetGroupType.Particle)]
        public string particleObjectId;

        [LabelText("粒子位置偏移")]
        public Vector2 offset = Vector2.zero;
        
        public override void Execute()
        {
            
        }
    }
}