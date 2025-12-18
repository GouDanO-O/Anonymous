using System;

namespace GDFrameworkExtend.Data
{
    /// <summary>
    /// 标记某字段/属性不进入模板（固定数据）导出
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NonPersistentAttribute : Attribute
    {
        
    }
}