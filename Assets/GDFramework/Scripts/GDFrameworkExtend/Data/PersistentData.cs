using System;
using GDFrameworkExtend.StorageKit;

namespace GDFrameworkExtend.Data
{
    /// <summary>
    /// 游戏固有数据
    /// 会在所有存档中持续存在,不会被玩家的行为影响
    /// </summary>
    [Serializable]
    public abstract class PersistentData :  IPersistentData
    {
        
    }
}