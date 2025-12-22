/**
 * SortingLayerSetup.cs
 * Unity Sorting Layer 设置指南
 * 
 * 这个文件包含设置 Sorting Layer 的说明和 Editor 辅助工具
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// Sorting Layer 设置帮助类
    /// </summary>
    public static class SortingLayerSetup
    {
        /*
         * ============================================================
         *                    Unity Sorting Layer 设置指南
         * ============================================================
         * 
         * 在使用渲染系统之前，需要在 Unity 中设置 Sorting Layers。
         * 
         * 步骤：
         * 1. 打开 Unity Editor
         * 2. Edit -> Project Settings -> Tags and Layers
         * 3. 展开 "Sorting Layers" 部分
         * 4. 按以下顺序添加 Sorting Layers（从上到下，上面的先渲染）：
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
         * 注意：
         * - Sorting Layer 的顺序决定了渲染顺序
         * - 列表靠下的层会渲染在靠上的层之上
         * - 同一 Sorting Layer 内，使用 Sorting Order 进一步排序
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
            foreach (var layerName in RequiredSortingLayers)
            {
                if (SortingLayer.NameToID(layerName) == 0 && layerName != "Default")
                {
                    Debug.LogWarning($"[SortingLayerSetup] 缺少 Sorting Layer: {layerName}");
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 输出设置说明到控制台
        /// </summary>
        public static void PrintSetupInstructions()
        {
            Debug.Log(@"
============================================================
               Unity Sorting Layer 设置指南
============================================================

请在 Unity Editor 中设置 Sorting Layers:

1. 打开 Edit -> Project Settings -> Tags and Layers
2. 展开 'Sorting Layers' 部分
3. 按以下顺序添加（从上到下）:

   [0] Ground      - 地形层
   [1] Floor       - 地板层
   [2] FloorDecor  - 地面装饰
   [3] Wall        - 墙壁
   [4] WallDecor   - 墙壁装饰/门窗框
   [5] Entity      - 动态实体
   [6] Roof        - 屋顶
   [7] UI          - UI 元素

============================================================
");
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor 菜单工具
    /// </summary>
    public static class SortingLayerSetupMenu
    {
        [MenuItem("Tools/Map System/Print Sorting Layer Setup")]
        public static void PrintSetup()
        {
            SortingLayerSetup.PrintSetupInstructions();
        }
        
        [MenuItem("Tools/Map System/Validate Sorting Layers")]
        public static void ValidateLayers()
        {
            if (SortingLayerSetup.ValidateSortingLayers())
            {
                Debug.Log("[SortingLayerSetup] 所有 Sorting Layer 已正确设置!");
            }
            else
            {
                SortingLayerSetup.PrintSetupInstructions();
            }
        }
    }
#endif
}
