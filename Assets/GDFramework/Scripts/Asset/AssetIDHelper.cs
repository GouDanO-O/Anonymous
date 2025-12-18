using System;
using System.Collections.Generic;
using System.Linq;
using GDFramework.FrameData;
using UnityEngine;

namespace GDFramework.Asset
{
    public static class AssetIDHelper
    {
        private static List<AssetIDInfo> _cachedAssetInfos;
        private static bool _isInitialized = false;
        
        public static List<AssetIDInfo> GetAllAssetInfos()
        {
            if (!_isInitialized || _cachedAssetInfos == null)
            {
                RefreshAssetInfos();
            }
            return _cachedAssetInfos ?? new List<AssetIDInfo>();
        }
        
        public static List<AssetIDInfo> GetAssetInfosByGroup(EAssetGroupType groupType)
        {
            var allInfos = GetAllAssetInfos();
            if (groupType == EAssetGroupType.All)
                return allInfos;
            
            return allInfos.Where(info => info.groupType == groupType).ToList();
        }
        
        public static AssetIDInfo GetAssetInfoByID(int id)
        {
            return GetAllAssetInfos().FirstOrDefault(info => info.id == id);
        }
        
        public static void RefreshAssetInfos()
        {
            _cachedAssetInfos = new List<AssetIDInfo>();
            
            try
            {
                // 通过反射获取AssetIDMapping中的数据
                var idMappingType = typeof(AssetIDMapping);
                var idToPathField = idMappingType.GetField("IDToAssetPath", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                if (idToPathField != null)
                {
                    var idToPathDict = idToPathField.GetValue(null) as Dictionary<int, string>;
                    
                    if (idToPathDict != null)
                    {
                        foreach (var kvp in idToPathDict)
                        {
                            var groupType = GetGroupTypeFromID(kvp.Key);
                            var assetName = ExtractAssetNameFromPath(kvp.Value);
                            
                            _cachedAssetInfos.Add(new AssetIDInfo
                            {
                                id = kvp.Key,
                                name = assetName,
                                path = kvp.Value,
                                groupType = groupType
                            });
                        }
                    }
                }
                
                // 按ID排序
                _cachedAssetInfos.Sort((a, b) => a.id.CompareTo(b.id));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to refresh Asset ID data: {e.Message}");
            }
            
            _isInitialized = true;
        }
        
        private static EAssetGroupType GetGroupTypeFromID(int id)
        {
            if (id >= 1000 && id < 2000) return EAssetGroupType.Music;
            if (id >= 2000 && id < 3000) return EAssetGroupType.Prefabs;
            if (id >= 3000 && id < 4000) return EAssetGroupType.Particle;
            return EAssetGroupType.All;
        }
        
        private static string ExtractAssetNameFromPath(string assetPath)
        {
            if (assetPath.StartsWith("yoo:"))
                return assetPath.Substring(4);
            return assetPath;
        }
    }
}