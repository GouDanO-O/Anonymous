using System;

namespace GDFramework.Asset
{
    [Serializable]
    public class AssetIDInfo
    {
        public int id;
        public string name;
        public string path;
        public EAssetGroupType groupType;
        
        public override string ToString()
        {
            return $"[{id}] {name}";
        }
    }
}