using GDFramework.Cheater;
using GDFrameworkCore;
using GDFrameworkExtend.StorageKit;

namespace GDFramework.Scripts.Cheater
{
    public class StorageClearAllCheatCommand : AddCheatCommand
    {
        public override void ExecuteCommand()
        {
            this.GetSystem<StorageKit>().ClearAllData();
        }
    }
}