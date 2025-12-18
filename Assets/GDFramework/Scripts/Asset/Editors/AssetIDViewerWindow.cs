#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GDFramework.FrameData;

namespace Game.Asset.Editors
{
    public class AssetIDViewerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchText = "";
        private string selectedGroup = "All";
        private bool showBundleNames = false;
        private bool showAssetPaths = true;
        private int selectedSortMode = 0; // 0: ID, 1: Name, 2: Group
        
        private List<AssetIDInfo> assetInfos = new List<AssetIDInfo>();
        private List<AssetIDInfo> filteredAssetInfos = new List<AssetIDInfo>();
        private List<string> groupNames = new List<string>();
        
        // 样式缓存
        private GUIStyle headerStyle;
        private GUIStyle rowStyle;
        private GUIStyle alternateRowStyle;
        private GUIStyle searchStyle;
        
        [MenuItem("YooAsset/Asset ID Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetIDViewerWindow>("Asset ID Viewer");
            window.minSize = new Vector2(800, 400);
            window.RefreshData();
        }
        
        private void OnEnable()
        {
            RefreshData();
        }
        
        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };
            }
            
            if (rowStyle == null)
            {
                rowStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    padding = new RectOffset(5, 5, 2, 2)
                };
            }
            
            if (alternateRowStyle == null)
            {
                alternateRowStyle = new GUIStyle(rowStyle);
                alternateRowStyle.normal.background = MakeTex(1, 1, new Color(0.5f, 0.5f, 0.5f, 0.1f));
            }
            
            if (searchStyle == null)
            {
                searchStyle = new GUIStyle(EditorStyles.toolbarSearchField)
                {
                    fontSize = 12
                };
            }
        }
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        private void RefreshData()
        {
            assetInfos.Clear();
            groupNames.Clear();
            groupNames.Add("All");
            
            try
            {
                // 通过反射获取AssetIDMapping中的数据
                var idMappingType = typeof(AssetIDMapping);
                
                // 获取IDToAssetPath字典
                var idToPathField = idMappingType.GetField("IDToAssetPath", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                if (idToPathField != null)
                {
                    var idToPathDict = idToPathField.GetValue(null) as Dictionary<int, string>;
                    
                    if (idToPathDict != null)
                    {
                        foreach (var kvp in idToPathDict)
                        {
                            string groupName = GetGroupNameFromID(kvp.Key);
                            string assetName = ExtractAssetNameFromPath(kvp.Value);
                            string bundleName = GetBundleNameForAsset(assetName);
                            
                            assetInfos.Add(new AssetIDInfo
                            {
                                ID = kvp.Key,
                                AssetName = assetName,
                                AssetPath = kvp.Value,
                                GroupName = groupName,
                                BundleName = bundleName
                            });
                            
                            if (!groupNames.Contains(groupName))
                                groupNames.Add(groupName);
                        }
                    }
                }
                
                // 排序
                SortAssetInfos();
                FilterAssetInfos();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to refresh Asset ID data: {e.Message}");
            }
        }
        
        private string GetGroupNameFromID(int id)
        {
            if (id >= 1000 && id < 2000) return "Music";
            if (id >= 2000 && id < 3000) return "Prefabs";
            if (id >= 3000 && id < 4000) return "Particle";
            return "Unknown";
        }
        
        private string ExtractAssetNameFromPath(string assetPath)
        {
            if (assetPath.StartsWith("yoo:"))
                return assetPath.Substring(4);
            return assetPath;
        }
        
        private string GetBundleNameForAsset(string assetName)
        {
            // 这里可以通过反射或其他方式获取Bundle名称
            // 简单起见，返回一个占位符
            return "bundle_name_here";
        }
        
        private void SortAssetInfos()
        {
            switch (selectedSortMode)
            {
                case 0: // ID
                    assetInfos.Sort((a, b) => a.ID.CompareTo(b.ID));
                    break;
                case 1: // Name
                    assetInfos.Sort((a, b) => string.Compare(a.AssetName, b.AssetName, StringComparison.OrdinalIgnoreCase));
                    break;
                case 2: // Group
                    assetInfos.Sort((a, b) => 
                    {
                        int groupCompare = string.Compare(a.GroupName, b.GroupName, StringComparison.OrdinalIgnoreCase);
                        return groupCompare != 0 ? groupCompare : a.ID.CompareTo(b.ID);
                    });
                    break;
            }
        }
        
        private void FilterAssetInfos()
        {
            filteredAssetInfos.Clear();
            
            foreach (var asset in assetInfos)
            {
                // 群组过滤
                if (selectedGroup != "All" && asset.GroupName != selectedGroup)
                    continue;
                
                // 搜索过滤
                if (!string.IsNullOrEmpty(searchText))
                {
                    bool matchFound = false;
                    string searchLower = searchText.ToLower();
                    
                    if (asset.AssetName.ToLower().Contains(searchLower) ||
                        asset.ID.ToString().Contains(searchText) ||
                        asset.AssetPath.ToLower().Contains(searchLower) ||
                        asset.GroupName.ToLower().Contains(searchLower))
                    {
                        matchFound = true;
                    }
                    
                    if (!matchFound)
                        continue;
                }
                
                filteredAssetInfos.Add(asset);
            }
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            EditorGUILayout.BeginVertical();
            
            // 工具栏
            DrawToolbar();
            
            EditorGUILayout.Space(5);
            
            // 表头
            DrawTableHeader();
            
            // 数据列表
            DrawAssetList();
            
            // 底部信息
            DrawBottomInfo();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // 刷新按钮
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshData();
            }
            
            GUILayout.Space(10);
            
            // 搜索框
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearchText = EditorGUILayout.TextField(searchText, searchStyle, GUILayout.Width(200));
            if (newSearchText != searchText)
            {
                searchText = newSearchText;
                FilterAssetInfos();
            }
            
            GUILayout.Space(10);
            
            // 群组过滤
            EditorGUILayout.LabelField("Group:", GUILayout.Width(50));
            int selectedGroupIndex = groupNames.IndexOf(selectedGroup);
            if (selectedGroupIndex < 0) selectedGroupIndex = 0;
            
            int newGroupIndex = EditorGUILayout.Popup(selectedGroupIndex, groupNames.ToArray(), GUILayout.Width(100));
            if (newGroupIndex != selectedGroupIndex)
            {
                selectedGroup = groupNames[newGroupIndex];
                FilterAssetInfos();
            }
            
            GUILayout.Space(10);
            
            // 排序方式
            EditorGUILayout.LabelField("Sort:", GUILayout.Width(40));
            string[] sortOptions = { "ID", "Name", "Group" };
            int newSortMode = EditorGUILayout.Popup(selectedSortMode, sortOptions, GUILayout.Width(80));
            if (newSortMode != selectedSortMode)
            {
                selectedSortMode = newSortMode;
                SortAssetInfos();
                FilterAssetInfos();
            }
            
            GUILayout.FlexibleSpace();
            
            // 显示选项
            showAssetPaths = GUILayout.Toggle(showAssetPaths, "Paths", EditorStyles.toolbarButton);
            showBundleNames = GUILayout.Toggle(showBundleNames, "Bundles", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("ID", headerStyle, GUILayout.Width(80));
            EditorGUILayout.LabelField("Asset Name", headerStyle, GUILayout.Width(200));
            EditorGUILayout.LabelField("Group", headerStyle, GUILayout.Width(100));
            
            if (showAssetPaths)
                EditorGUILayout.LabelField("Asset Path", headerStyle, GUILayout.Width(200));
            
            if (showBundleNames)
                EditorGUILayout.LabelField("Bundle Name", headerStyle);
            
            EditorGUILayout.EndHorizontal();
            
            // 分隔线
            Rect rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, Color.gray);
        }
        
        private void DrawAssetList()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            for (int i = 0; i < filteredAssetInfos.Count; i++)
            {
                var asset = filteredAssetInfos[i];
                GUIStyle currentRowStyle = (i % 2 == 0) ? rowStyle : alternateRowStyle;
                
                EditorGUILayout.BeginHorizontal(currentRowStyle);
                
                // ID (可点击复制)
                if (GUILayout.Button(asset.ID.ToString(), EditorStyles.label, GUILayout.Width(80)))
                {
                    EditorGUIUtility.systemCopyBuffer = asset.ID.ToString();
                    Debug.Log($"Copied ID to clipboard: {asset.ID}");
                }
                
                // Asset Name (可点击复制)
                if (GUILayout.Button(asset.AssetName, EditorStyles.label, GUILayout.Width(200)))
                {
                    EditorGUIUtility.systemCopyBuffer = asset.AssetName;
                    Debug.Log($"Copied asset name to clipboard: {asset.AssetName}");
                }
                
                // Group
                EditorGUILayout.LabelField(asset.GroupName, currentRowStyle, GUILayout.Width(100));
                
                // Asset Path (可点击复制)
                if (showAssetPaths)
                {
                    if (GUILayout.Button(asset.AssetPath, EditorStyles.label, GUILayout.Width(200)))
                    {
                        EditorGUIUtility.systemCopyBuffer = asset.AssetPath;
                        Debug.Log($"Copied asset path to clipboard: {asset.AssetPath}");
                    }
                }
                
                // Bundle Name
                if (showBundleNames)
                {
                    EditorGUILayout.LabelField(asset.BundleName, currentRowStyle);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawBottomInfo()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField($"Showing {filteredAssetInfos.Count} of {assetInfos.Count} assets", 
                EditorStyles.miniLabel);
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField("Click on ID/Name/Path to copy to clipboard", 
                EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private class AssetIDInfo
        {
            public int ID;
            public string AssetName;
            public string AssetPath;
            public string GroupName;
            public string BundleName;
        }
    }
}
#endif