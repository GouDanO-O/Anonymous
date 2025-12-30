using System;

namespace GDFramework.Scripts.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UnSavedAttribute : Attribute
    {
        public bool allowLoading;
        
        public UnSavedAttribute(bool allowLoading = false)
            => this.allowLoading = allowLoading;
    }
}