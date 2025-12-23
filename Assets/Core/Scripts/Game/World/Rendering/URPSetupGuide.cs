/**
 * URPSetupGuide.cs
 * URP 2D 渲染设置指南
 * 
 * 包含：
 * - Sorting Layer 设置
 * - URP 2D Renderer 配置
 * - 材质设置
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// URP 设置帮助类
    /// </summary>
    public static class URPSetupGuide
    {
        /*
         * ============================================================
         *                    URP 2D 渲染设置指南
         * ============================================================
         * 
         * 本地图系统使用 URP (Universal Render Pipeline) 2D Renderer。
         * 请按照以下步骤设置项目：
         * 
         * ============================================================
         *                    第一步：安装 URP
         * ============================================================
         * 
         * 1. Window -> Package Manager
         * 2. 搜索 "Universal RP"
         * 3. 安装 Universal RP 包
         * 4. 如果使用 2D 光照，还需要安装 "2D Sprite" 包
         * 
         * ============================================================
         *                    第二步：创建 URP Asset
         * ============================================================
         * 
         * 1. Project 窗口右键 -> Create -> Rendering -> URP Asset (with 2D Renderer)
         * 2. 这会同时创建两个文件：
         *    - URP Asset (渲染管线设置)
         *    - URP Renderer 2D (2D 渲染器设置)
         * 
         * ============================================================
         *                    第三步：配置 2D Renderer
         * ============================================================
         * 
         * 选中 URP Renderer 2D 文件，在 Inspector 中设置：
         * 
         * [Transparency Sort Mode]
         * - 设置为: Custom Axis
         * - Transparency Sort Axis: (0, 1, 0)
         *   这确保按 Y 轴排序（上面的物体在后面渲染）
         * 
         * [Default Material Type]
         * - 设置为: Lit
         *   如果不需要 2D 光照，设置为 Unlit
         * 
         * ============================================================
         *                    第四步：应用渲染管线
         * ============================================================
         * 
         * 1. Edit -> Project Settings -> Graphics
         * 2. 将创建的 URP Asset 拖到 "Scriptable Render Pipeline Settings"
         * 
         * 1. Edit -> Project Settings -> Quality
         * 2. 对每个质量级别，设置对应的 URP Asset
         * 
         * ============================================================
         *                    第五步：设置 Sorting Layers
         * ============================================================
         * 
         * 1. Edit -> Project Settings -> Tags and Layers
         * 2. 展开 "Sorting Layers" 部分
         * 3. 按以下顺序添加（从上到下，下面的渲染在上层）：
         * 
         *    | 顺序 | 名称       | 用途                    |
         *    |------|------------|-------------------------|
         *    | 0    | Default    | Unity 默认层            |
         *    | 1    | Ground     | 地形层                  |
         *    | 2    | Floor      | 地板层                  |
         *    | 3    | FloorDecor | 地面装饰                |
         *    | 4    | Wall       | 墙壁                    |
         *    | 5    | WallDecor  | 墙壁装饰/门窗框         |
         *    | 6    | Entity     | 动态实体（家具、NPC等） |
         *    | 7    | Roof       | 屋顶                    |
         *    | 8    | UI         | UI 元素                 |
         * 
         * ============================================================
         *                    第六步：设置相机
         * ============================================================
         * 
         * 选中 Main Camera，在 Inspector 中设置：
         * 
         * [Projection]
         * - 设置为: Orthographic
         * 
         * [Render Type]
         * - 设置为: Base
         * 
         * [Renderer]
         * - 选择创建的 2D Renderer
         * 
         * ============================================================
         *                    第七步：添加 2D 光照（可选）
         * ============================================================
         * 
         * 如果需要 2D 光照效果：
         * 
         * 1. 创建全局光：
         *    - Hierarchy 右键 -> Light -> 2D -> Global Light 2D
         *    - 设置 Intensity 为 1
         * 
         * 2. 创建点光源（可选）：
         *    - Hierarchy 右键 -> Light -> 2D -> Point Light 2D
         * 
         * 3. 创建聚光灯（可选）：
         *    - Hierarchy 右键 -> Light -> 2D -> Spot Light 2D
         * 
         * ============================================================
         *                    第八步：材质设置
         * ============================================================
         * 
         * 本系统会自动设置正确的 URP 材质，但如果需要手动设置：
         * 
         * 对于 SpriteRenderer / TilemapRenderer：
         * - 使用光照: Sprite-Lit-Default
         * - 不使用光照: Sprite-Unlit-Default
         * 
         * 可通过代码手动设置：
         * URPMaterialHelper.SetupSpriteRenderer(renderer, useLighting: true);
         * URPMaterialHelper.SetupTilemapRenderer(renderer, useLighting: true);
         * 
         * ============================================================
         *                    常见问题
         * ============================================================
         * 
         * Q: 物体显示为紫色/粉色
         * A: 材质丢失。确保使用了正确的 URP 材质。
         * 
         * Q: 物体完全黑色
         * A: 没有光源。添加 Global Light 2D 或设置 UseLighting = false。
         * 
         * Q: 渲染顺序不正确
         * A: 检查 Sorting Layer 和 Sorting Order 设置。
         *    确保 2D Renderer 的 Transparency Sort Axis 设置正确。
         * 
         * Q: 性能问题
         * A: 使用 TilemapRenderer.Mode = Chunk 模式（默认已设置）。
         *    减少实时光源数量。
         * 
         * ============================================================
         */
        
        /// <summary>
        /// 需要创建的 Sorting Layer 列表
        /// </summary>
        public static readonly string[] RequiredSortingLayers = new string[]
        {
            RenderingConstants.SORTING_LAYER_GROUND,
            RenderingConstants.SORTING_LAYER_FLOOR,
            RenderingConstants.SORTING_LAYER_FLOOR_DECOR,
            RenderingConstants.SORTING_LAYER_WALL,
            RenderingConstants.SORTING_LAYER_WALL_DECOR,
            RenderingConstants.SORTING_LAYER_ENTITY,
            RenderingConstants.SORTING_LAYER_ROOF,
            RenderingConstants.SORTING_LAYER_UI
        };
        
        /// <summary>
        /// 检查所有需要的 Sorting Layer 是否存在
        /// </summary>
        public static bool ValidateSortingLayers()
        {
            bool allValid = true;
            
            foreach (var layerName in RequiredSortingLayers)
            {
                int layerId = SortingLayer.NameToID(layerName);
                if (layerId == 0 && layerName != "Default")
                {
                    Debug.LogWarning($"[URPSetupGuide] 缺少 Sorting Layer: {layerName}");
                    allValid = false;
                }
            }
            
            return allValid;
        }
        
        /// <summary>
        /// 检查 URP 是否正确配置
        /// </summary>
        public static bool ValidateURPSetup()
        {
            bool isValid = true;
            
            // 检查渲染管线
            var currentPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (currentPipeline == null)
            {
                Debug.LogWarning("[URPSetupGuide] 未设置渲染管线！请在 Project Settings -> Graphics 中设置 URP Asset。");
                isValid = false;
            }
            else
            {
                Debug.Log($"[URPSetupGuide] 当前渲染管线: {currentPipeline.name}");
            }
            
            // 检查主相机
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                if (!mainCamera.orthographic)
                {
                    Debug.LogWarning("[URPSetupGuide] 主相机应该设置为正交模式 (Orthographic)！");
                    isValid = false;
                }
            }
            else
            {
                Debug.LogWarning("[URPSetupGuide] 未找到主相机！");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 输出设置说明到控制台
        /// </summary>
        public static void PrintSetupInstructions()
        {
            Debug.Log(@"
============================================================
               URP 2D 渲染设置指南
============================================================

请按照以下步骤设置：

1. 安装 URP
   Window -> Package Manager -> 搜索 'Universal RP' -> Install

2. 创建 URP Asset
   Project 窗口右键 -> Create -> Rendering -> URP Asset (with 2D Renderer)

3. 配置 2D Renderer
   - Transparency Sort Mode: Custom Axis
   - Transparency Sort Axis: (0, 1, 0)

4. 应用渲染管线
   Edit -> Project Settings -> Graphics -> 设置 URP Asset

5. 设置 Sorting Layers (Edit -> Project Settings -> Tags and Layers)
   [0] Ground      - 地形层
   [1] Floor       - 地板层
   [2] FloorDecor  - 地面装饰
   [3] Wall        - 墙壁
   [4] WallDecor   - 墙壁装饰
   [5] Entity      - 动态实体
   [6] Roof        - 屋顶
   [7] UI          - UI 元素

6. 设置相机
   - Projection: Orthographic
   - Renderer: 选择 2D Renderer

7. 添加全局光（可选）
   Hierarchy 右键 -> Light -> 2D -> Global Light 2D

============================================================
");
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor 菜单工具
    /// </summary>
    public static class URPSetupMenu
    {
        [MenuItem("Tools/Map System/Print URP Setup Guide")]
        public static void PrintSetup()
        {
            URPSetupGuide.PrintSetupInstructions();
        }
        
        [MenuItem("Tools/Map System/Validate Sorting Layers")]
        public static void ValidateLayers()
        {
            if (URPSetupGuide.ValidateSortingLayers())
            {
                Debug.Log("[URPSetupGuide] 所有 Sorting Layer 已正确设置!");
            }
            else
            {
                Debug.LogWarning("[URPSetupGuide] 部分 Sorting Layer 缺失，请查看设置指南。");
                URPSetupGuide.PrintSetupInstructions();
            }
        }
        
        [MenuItem("Tools/Map System/Validate URP Setup")]
        public static void ValidateURP()
        {
            bool sortingValid = URPSetupGuide.ValidateSortingLayers();
            bool urpValid = URPSetupGuide.ValidateURPSetup();
            
            if (sortingValid && urpValid)
            {
                Debug.Log("[URPSetupGuide] URP 设置验证通过!");
            }
            else
            {
                Debug.LogWarning("[URPSetupGuide] 设置存在问题，请查看上方警告信息。");
            }
        }
        
        [MenuItem("Tools/Map System/Create 2D Global Light")]
        public static void CreateGlobalLight()
        {
            // 检查是否已存在
            var existingLight = Object.FindObjectOfType<UnityEngine.Rendering.Universal.Light2D>();
            if (existingLight != null)
            {
                Debug.LogWarning("[URPSetupGuide] 场景中已存在 2D 光源！");
                Selection.activeGameObject = existingLight.gameObject;
                return;
            }
            
            // 创建全局光
            GameObject lightGo = new GameObject("Global Light 2D");
            var light2D = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2D.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light2D.intensity = 1f;
            light2D.color = Color.white;
            
            Selection.activeGameObject = lightGo;
            Debug.Log("[URPSetupGuide] 已创建 Global Light 2D!");
        }
    }
#endif
}
