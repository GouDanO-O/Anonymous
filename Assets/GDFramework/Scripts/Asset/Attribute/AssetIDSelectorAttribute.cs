using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GDFramework.Asset
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetIDSelectorAttribute : PropertyAttribute
    {
        public EAssetGroupType GroupType { get; }
        
        public AssetIDSelectorAttribute(EAssetGroupType groupType)
        {
            GroupType = groupType;
        }
    }
}