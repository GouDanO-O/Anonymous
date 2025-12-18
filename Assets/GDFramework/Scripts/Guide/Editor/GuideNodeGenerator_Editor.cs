using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GDFramework.Guide.Editor
{
    public class GuideNodeGenerator_Editor : EditorWindow
    {
        private GameObject targetPrefab;
        private string namespaceName = "GuideNodeAsset";
        private string outputPath = "Assets/Res/GuideNodeConfigs";
        private string aggregatedClassName = "UIPrefabHierarchy";

        [MenuItem("Tools/UI/Generate Aggregated Prefab Hierarchy")]
        public static void ShowWindow()
        {
            GetWindow<GuideNodeGenerator_Editor>("Aggregated UI Hierarchy Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Aggregated UI Hierarchy Generator", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            targetPrefab =
                EditorGUILayout.ObjectField("Target Prefab", targetPrefab, typeof(GameObject), false) as GameObject;

            EditorGUILayout.Space();

            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            aggregatedClassName = EditorGUILayout.TextField("Aggregated Class Name", aggregatedClassName);

            EditorGUILayout.Space();

            GUI.enabled = targetPrefab != null;
            if (GUILayout.Button("Add/Update Prefab to Aggregated Class")) GenerateAggregatedScript();
            GUI.enabled = true;
        }

        private void GenerateAggregatedScript()
        {
            if (targetPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a prefab first!", "OK");
                return;
            }

            var filePath = Path.Combine(outputPath, "GuideNodeAsset.cs");
            Directory.CreateDirectory(outputPath);

            // Get prefab class content
            var className = targetPrefab.name;
            var prefabClassContent = new StringBuilder();

            prefabClassContent.AppendLine($"    public class {className} {{");

            // Process the children - only first level
            for (var i = 0; i < targetPrefab.transform.childCount; i++)
            {
                var child = targetPrefab.transform.GetChild(i);
                var constName = $"{child.name}";

                // Add const definition
                prefabClassContent.AppendLine(
                    $"        public const string {constName} = \"{className}_{constName}\";");
            }

            prefabClassContent.AppendLine("    }");

            // Read existing file content if it exists
            var existingContent = "";
            if (File.Exists(filePath))
                existingContent = File.ReadAllText(filePath);
            else
                // Create new file with namespace structure
                existingContent = $"namespace {namespaceName} {{\r\n}}\r\n";

            // Check if the class already exists in the file
            var classPattern = $@"public class {className} {{[^}}]*}}";
            string newContent;

            if (Regex.IsMatch(existingContent, classPattern))
            {
                // Replace existing class definition
                newContent = Regex.Replace(existingContent, classPattern, prefabClassContent.ToString().TrimEnd());
            }
            else
            {
                // Add new class definition before the last closing brace of namespace
                var lastBraceIndex = existingContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                    newContent = existingContent.Insert(lastBraceIndex, prefabClassContent.ToString());
                else
                    // If for some reason lastBraceIndex is not found, just append
                    newContent = $"namespace {namespaceName} {{\r\n{prefabClassContent}}}\r\n";
            }

            // Write updated content back to file
            File.WriteAllText(filePath, newContent);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Updated {className} in {filePath}", "OK");

            // Open the file
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, 1);
        }
    }
}