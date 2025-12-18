using System;
using Sirenix.OdinInspector;

namespace GDFrameworkExtend.Data
{
    /// <summary>
    /// 临时游戏数据
    /// 仅当前存档中持续存在,会被玩家的行为影响而产生影响
    /// </summary>
    [Serializable]
    public abstract class TemporaryData : ITemporaryData
    {
        
    }
}