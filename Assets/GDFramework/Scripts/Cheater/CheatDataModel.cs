using System.Collections.Generic;
using GDFramework.Cheater;
using GDFrameworkCore;


namespace GDFramework.Models
{
    public class CheatDataModel : AbstractModel
    {
        public Dictionary<string, AddCheatCommand> CheatModuleDict = new();

        protected override void OnInit()
        {
        }

        public void AddCheatModule(string name, AddCheatCommand command)
        {
            if (!CheatModuleDict.ContainsKey(name)) 
                CheatModuleDict.Add(name, command);
        }

        public void RemoveCheatModule(string name)
        {
            if (CheatModuleDict.ContainsKey(name)) 
                CheatModuleDict.Remove(name);
        }

        public Dictionary<string, AddCheatCommand> GetCheaterDatas()
        {
            return CheatModuleDict;
        }
    }
}