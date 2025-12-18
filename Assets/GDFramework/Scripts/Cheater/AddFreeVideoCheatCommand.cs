using System;

namespace GDFramework.Cheater
{
    public class AddFreeVideoCheatCommand : AddCheatCommand
    {
        public bool IsActive { get; private set; }
    
        public Func<bool> FreeVideoCheatAction;
    
        public AddFreeVideoCheatCommand(string name, Func<bool> freeVideoCheatAction)
        {
            //Name = name;
            FreeVideoCheatAction = freeVideoCheatAction;
        }

        public override void ExecuteCommand()
        {
            IsActive = FreeVideoCheatAction?.Invoke() ?? false;
        }
    }
}