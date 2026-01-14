using System;
using UnityEngine;

namespace Core.Game.Defs.Map
{
    /// <summary>
    /// 地面定义 - 定义地面类型的属性
    /// 例如：草地、泥土、石头地面、木地板、石砖地板等
    /// </summary>
    [Serializable]
    public class TerrainDef : BaseDef
    {
        public override EDefType DefType => EDefType.Terrain;

        #region 基础属性

        /// <summary>
        /// 是否可通行
        /// </summary>
        public bool passable = true;

        /// <summary>
        /// 行进速率
        /// 为1代表标准移动速度
        /// 为0.5代表当前移动速度*0.5
        /// 为0代表无法进行移动
        /// </summary>
        public float pathSpeed = 1f;

        /// <summary>
        /// 是否是自然地形（非人造）
        /// </summary>
        public bool natural = true;

        /// <summary>
        /// 地形层级（用于渲染排序）
        /// </summary>
        public int layerOrder = 0;

        #endregion

        #region 视觉属性

        /// <summary>
        /// 贴图路径（用于加载Sprite）
        /// </summary>
        public string texturePath;

        /// <summary>
        /// 贴图在图集中的索引（如果使用Tilemap）
        /// </summary>
        public int tileIndex = -1;

        /// <summary>
        /// 颜色色调
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// 是否使用自动拼接（类似RPG Maker的自动图块）
        /// </summary>
        public bool autoTile = false;

        /// <summary>
        /// 自动拼接规则ID
        /// </summary>
        public int autoTileRuleId = -1;

        #endregion

        #region 功能属性

        /// <summary>
        /// 肥沃度（影响植物生长）
        /// </summary>
        public float fertility = 0f;

        /// <summary>
        /// 是否可以种植
        /// </summary>
        public bool supportPlant = false;

        /// <summary>
        /// 是否是水面
        /// </summary>
        public bool isWater = false;

        /// <summary>
        /// 水深（如果是水面）
        /// </summary>
        public float waterDepth = 0f;

        /// <summary>
        /// 美观度（影响角色心情）
        /// </summary>
        public int beauty = 0;

        /// <summary>
        /// 清洁度影响
        /// </summary>
        public float cleanlinessModifier = 0f;

        #endregion

        #region 建造属性

        /// <summary>
        /// 是否可以在上面建造
        /// </summary>
        public bool affordances_Light = true;     // 轻型建筑
        public bool affordances_Medium = true;    // 中型建筑
        public bool affordances_Heavy = true;     // 重型建筑

        /// <summary>
        /// 建造此地面需要的资源（defName -> 数量）
        /// 仅对人造地面有效
        /// </summary>
        public string[] costDefNames;
        public int[] costAmounts;

        /// <summary>
        /// 建造工作量
        /// </summary>
        public float workToBuild = 0f;

        #endregion

        public override void PostLoad()
        {
            base.PostLoad();

            // 验证数据
            if (pathSpeed <= 0)
            {
                pathSpeed = 0f;
            }
        }

        public override string[] ConfigErrors()
        {
            var baseErrors = base.ConfigErrors();
            var errors = new System.Collections.Generic.List<string>();
            
            if (baseErrors != null)
            {
                errors.AddRange(baseErrors);
            }

            if (string.IsNullOrEmpty(texturePath) && tileIndex < 0)
            {
                errors.Add("texturePath和tileIndex至少需要设置一个");
            }

            if (costDefNames != null && costAmounts != null && costDefNames.Length != costAmounts.Length)
            {
                errors.Add("costDefNames和costAmounts长度不匹配");
            }

            return errors.Count > 0 ? errors.ToArray() : null;
        }
    }
    
    /// <summary>
    /// 预定义的常用地形
    /// 方便代码中引用，实际数据从Luban加载
    /// </summary>
    public static class TerrainDefOf
    {
        public static TerrainDef Soil;          // 泥土
        public static TerrainDef Grass;         // 草地
        public static TerrainDef Sand;          // 沙地
        public static TerrainDef Stone;         // 石头地面
        public static TerrainDef Water;         // 水
        public static TerrainDef WoodFloor;     // 木地板
        public static TerrainDef StoneFloor;    // 石砖地板

        /// <summary>
        /// 从DefDatabase绑定引用
        /// 应在所有TerrainDef加载完成后调用
        /// </summary>
        public static void BindReferences()
        {
            Soil = DefDataBase<TerrainDef>.GetNamed("Terrain_Soil");
            Grass = DefDataBase<TerrainDef>.GetNamed("Terrain_Grass");
            Sand = DefDataBase<TerrainDef>.GetNamed("Terrain_Sand");
            Stone = DefDataBase<TerrainDef>.GetNamed("Terrain_Stone");
            Water = DefDataBase<TerrainDef>.GetNamed("Terrain_Water");
            WoodFloor = DefDataBase<TerrainDef>.GetNamed("Terrain_WoodFloor");
            StoneFloor = DefDataBase<TerrainDef>.GetNamed("Terrain_StoneFloor");
        }
    }
}