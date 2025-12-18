
using System.Collections.Generic;
using GDFramework.Asset;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Game.Asset.Editors
{
    public class AssetIDSelectorAttributeDrawer : OdinAttributeDrawer<AssetIDSelectorAttribute, string>
    {
        private List<AssetIDInfo> availableAssets;
        private string[] displayOptions;
        private int[] idValues;
        private int currentSelectedIndex = 0;
        
        protected override void Initialize()
        {
            RefreshAssetOptions();
        }
        
        private void RefreshAssetOptions()
        {
            // 获取指定组的资源
            availableAssets = AssetIDHelper.GetAssetInfosByGroup(Attribute.GroupType);
            
            // 创建显示选项数组
            displayOptions = new string[availableAssets.Count + 1];
            idValues = new int[availableAssets.Count + 1];
            
            displayOptions[0] = "None (Empty)";
            idValues[0] = -1;
            
            for (int i = 0; i < availableAssets.Count; i++)
            {
                var asset = availableAssets[i];
                displayOptions[i + 1] = $"[{asset.id}] {asset.name}";
                idValues[i + 1] = asset.id;
            }
            
            // 更新当前选中的索引
            UpdateSelectedIndex();
        }
        
        private void UpdateSelectedIndex()
        {
            if (int.TryParse(ValueEntry.SmartValue, out int currentID))
            {
                currentSelectedIndex = 0; // 默认选择 "None"
                
                for (int i = 0; i < idValues.Length; i++)
                {
                    if (idValues[i] == currentID)
                    {
                        currentSelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                currentSelectedIndex = 0;
            }
        }
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            
            EditorGUI.BeginChangeCheck();
            
            // 绘制下拉框
            int newSelectedIndex = EditorGUI.Popup(rect, label.text, currentSelectedIndex, displayOptions);
            
            if (EditorGUI.EndChangeCheck() || newSelectedIndex != currentSelectedIndex)
            {
                currentSelectedIndex = newSelectedIndex;
                
                // 更新值
                if (currentSelectedIndex == 0)
                {
                    ValueEntry.SmartValue = ""; // 或者 "-1"，根据你的需求
                }
                else
                {
                    ValueEntry.SmartValue = idValues[currentSelectedIndex].ToString();
                }
            }
            
            // 添加刷新按钮
            var buttonRect = new Rect(rect.xMax - 60, rect.y, 60, rect.height);
            rect.width -= 65;
            
            if (GUI.Button(buttonRect, "Refresh", EditorStyles.miniButton))
            {
                AssetIDHelper.RefreshAssetInfos();
                RefreshAssetOptions();
            }
            
            // 显示当前选中的资源信息（可选）
            if (currentSelectedIndex > 0 && currentSelectedIndex <= availableAssets.Count)
            {
                var selectedAsset = availableAssets[currentSelectedIndex - 1];
                var infoRect = EditorGUILayout.GetControlRect(false, 16);
                EditorGUI.LabelField(infoRect, $"Path: {selectedAsset.path}", EditorStyles.miniLabel);
            }
        }
    }
    
    // 传统Unity Inspector 自定义属性绘制器（备用方案）
    [CustomPropertyDrawer(typeof(AssetIDSelectorAttribute))]
    public class AssetIDSelectorPropertyDrawer : PropertyDrawer
    {
        private List<AssetIDInfo> availableAssets;
        private string[] displayOptions;
        private int[] idValues;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "AssetIDSelector only works with string fields");
                return;
            }
            
            var selectorAttribute = attribute as AssetIDSelectorAttribute;
            if (selectorAttribute == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            
            // 初始化选项
            if (availableAssets == null)
            {
                RefreshAssetOptions(selectorAttribute.GroupType);
            }
            
            // 找到当前选中的索引
            int currentSelectedIndex = 0;
            if (int.TryParse(property.stringValue, out int currentID))
            {
                for (int i = 0; i < idValues.Length; i++)
                {
                    if (idValues[i] == currentID)
                    {
                        currentSelectedIndex = i;
                        break;
                    }
                }
            }
            
            // 绘制下拉框
            var dropdownRect = new Rect(position.x, position.y, position.width - 65, position.height);
            var buttonRect = new Rect(position.xMax - 60, position.y, 60, position.height);
            
            EditorGUI.BeginChangeCheck();
            int newSelectedIndex = EditorGUI.Popup(dropdownRect, label.text, currentSelectedIndex, displayOptions);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (newSelectedIndex == 0)
                {
                    property.stringValue = "";
                }
                else
                {
                    property.stringValue = idValues[newSelectedIndex].ToString();
                }
            }
            
            // 刷新按钮
            if (GUI.Button(buttonRect, "Refresh", EditorStyles.miniButton))
            {
                AssetIDHelper.RefreshAssetInfos();
                RefreshAssetOptions(selectorAttribute.GroupType);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        private void RefreshAssetOptions(EAssetGroupType groupType)
        {
            availableAssets = AssetIDHelper.GetAssetInfosByGroup(groupType);
            
            displayOptions = new string[availableAssets.Count + 1];
            idValues = new int[availableAssets.Count + 1];
            
            displayOptions[0] = "None (Empty)";
            idValues[0] = -1;
            
            for (int i = 0; i < availableAssets.Count; i++)
            {
                var asset = availableAssets[i];
                displayOptions[i + 1] = $"[{asset.id}] {asset.name}";
                idValues[i + 1] = asset.id;
            }
        }
    }
}
#endif