using Game;
using GDFramework.Utility;
using GDFrameworkExtend.LogKit;

namespace GDFramework.YooAssetKit
{
    internal class StartGameFsmNode : PatchFsmNode
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LogKit.Log("进入游戏");
            PatchOperation.SetFinish();
        }
    }
}