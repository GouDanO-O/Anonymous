using System.Collections.Generic;
using System.Linq;
using Core.Game.Resource.Data;
using Core.Game.Resource.Define;
using GDFrameworkExtend.LogKit;
using UnityEngine;

namespace Core.Game.Config
{
    /// <summary>
    /// 配置管理器: 加载Luban生成的配置表, 提供全局访问
    /// 替代 TempConfigProvider
    /// </summary>
    public static class ConfigManager
    {
        public static global::Config.CfgTables Tables { get; private set; }
        public static bool IsLoaded { get; private set; }

        /// <summary>
        /// 从 Resources/ConfigData 加载所有配置表
        /// </summary>
        public static void Load()
        {
            if (IsLoaded) return;

            Tables = new global::Config.CfgTables(fileName =>
            {
                var textAsset = Resources.Load<TextAsset>($"ConfigData/{fileName}");
                if (textAsset == null)
                {
                    LogKit.Error($"ConfigManager: 配置文件未找到 ConfigData/{fileName}");
                    return null;
                }
                return SimpleJSON.JSON.Parse(textAsset.text);
            });

            IsLoaded = true;
            LogKit.Log($"ConfigManager: 配置加载完成 — " +
                       $"Terrain:{Tables.TbTerrainDef.DataList.Count}, " +
                       $"Floor:{Tables.TbFloorDef.DataList.Count}, " +
                       $"Structure:{Tables.TbStructureDef.DataList.Count}, " +
                       $"Item:{Tables.TbItemDef.DataList.Count}, " +
                       $"Pawn:{Tables.TbPawnDef.DataList.Count}");
        }

        #region 便捷访问

        public static global::Config.TerrainDef GetTerrainDef(int id)
            => Tables.TbTerrainDef.GetOrDefault(id);

        public static global::Config.FloorDef GetFloorDef(int id)
            => Tables.TbFloorDef.GetOrDefault(id);

        public static global::Config.StructureDef GetStructureDef(int id)
            => Tables.TbStructureDef.GetOrDefault(id);

        public static global::Config.ItemDef GetItemDef(int id)
            => Tables.TbItemDef.GetOrDefault(id);

        public static global::Config.PawnDef GetPawnDef(int id)
            => Tables.TbPawnDef.GetOrDefault(id);

        #endregion

        #region 类型转换工具

        /// <summary>
        /// Luban ColorRGBA → UnityEngine.Color
        /// </summary>
        public static Color ToUnityColor(global::Config.ColorRGBA c)
        {
            if (c == null) return Color.magenta;
            return new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        /// <summary>
        /// Luban List&lt;Config.MaterialCost&gt; → 游戏内 MaterialCost[]
        /// </summary>
        public static MaterialCost[] ToMaterialCosts(List<global::Config.MaterialCost> costs)
        {
            if (costs == null || costs.Count == 0)
                return System.Array.Empty<MaterialCost>();
            return costs.Select(c =>
                new MaterialCost((EMaterialType)c.Type, c.Amount)).ToArray();
        }

        #endregion

        #region DefId 枚举 (供 TileAtlasManager 使用)

        public static IEnumerable<int> GetAllTerrainDefIds()
            => Tables.TbTerrainDef.DataMap.Keys;

        public static IEnumerable<int> GetAllFloorDefIds()
            => Tables.TbFloorDef.DataMap.Keys;

        public static IEnumerable<int> GetAllStructureDefIds()
            => Tables.TbStructureDef.DataMap.Keys;

        public static IEnumerable<int> GetAllItemDefIds()
            => Tables.TbItemDef.DataMap.Keys;

        public static IEnumerable<int> GetAllPawnDefIds()
            => Tables.TbPawnDef.DataMap.Keys;

        #endregion
    }
}
