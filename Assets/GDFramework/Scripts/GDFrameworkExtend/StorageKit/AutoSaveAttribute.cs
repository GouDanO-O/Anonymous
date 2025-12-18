using System;

namespace GDFrameworkExtend.StorageKit
{
    /// <summary>
    /// 自动存储特性，标记需要自动存储的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutoSaveAttribute : Attribute
    {
        public string SaveKey { get; }
        
        public AutoSaveAttribute(string saveKey = null)
        {
            SaveKey = saveKey;
        }
    }
}