using System;
using GDFramework.Asset;
using Sirenix.OdinInspector;

namespace Core.Game.Action
{
    [Serializable]
    public class PlaySoundAction : BaseAction
    {
        [LabelText("音频剪辑ID"), AssetIDSelector(EAssetGroupType.Music)]
        public string audioClipId;
        
        public override void Execute()
        {
            
        }
    }
}