/*******************************************************************************
 * 文件名:    DefDatabase.cs
 * 描述:      Def数据库管理器，统一管理所有Def实例
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   DefDatabase 是所有Def的中央管理器，提供：
 *   - Def的注册和查询
 *   - 按类型获取Def列表
 *   - 初始化和引用解析
 *   - 与Luban配置表的对接
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// Def数据库 - 管理所有Def实例
    /// </summary>
    public static class DefDatabase
    {
        #region 存储

        /// <summary>
        /// 所有Def的字典（DefId -> Def）
        /// </summary>
        private static readonly Dictionary<string, DefBase> _allDefs = new Dictionary<string, DefBase>();

        /// <summary>
        /// 按类型分组的Def字典
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, DefBase>> _defsByType 
            = new Dictionary<Type, Dictionary<string, DefBase>>();

        /// <summary>
        /// 短哈希到Def的映射（用于网络同步）
        /// </summary>
        private static readonly Dictionary<int, DefBase> _defsByHash = new Dictionary<int, DefBase>();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool _initialized = false;

        /// <summary>
        /// 是否已解析引用
        /// </summary>
        private static bool _referencesResolved = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 所有Def数量
        /// </summary>
        public static int Count => _allDefs.Count;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _initialized;

        /// <summary>
        /// 所有已注册的Def ID
        /// </summary>
        public static IEnumerable<string> AllDefIds => _allDefs.Keys;

        /// <summary>
        /// 所有已注册的Def
        /// </summary>
        public static IEnumerable<DefBase> AllDefs => _allDefs.Values;

        #endregion

        #region 注册

        /// <summary>
        /// 注册单个Def
        /// </summary>
        public static void Register(DefBase def)
        {
            if (def == null)
            {
                Debug.LogWarning("[DefDatabase] Attempted to register null def");
                return;
            }

            if (string.IsNullOrEmpty(def.DefId))
            {
                Debug.LogWarning($"[DefDatabase] Def has null or empty DefId: {def.GetType().Name}");
                return;
            }

            // 检查重复
            if (_allDefs.ContainsKey(def.DefId))
            {
                Debug.LogWarning($"[DefDatabase] Duplicate DefId: {def.DefId}, replacing existing");
                Unregister(def.DefId);
            }

            // 注册到总表
            _allDefs[def.DefId] = def;

            // 注册到类型表
            Type defType = def.GetType();
            if (!_defsByType.TryGetValue(defType, out var typeDict))
            {
                typeDict = new Dictionary<string, DefBase>();
                _defsByType[defType] = typeDict;
            }
            typeDict[def.DefId] = def;

            // 注册到哈希表
            int hash = def.ShortHash;
            if (_defsByHash.ContainsKey(hash))
            {
                Debug.LogWarning($"[DefDatabase] Hash collision: {def.DefId} and {_defsByHash[hash].DefId} have same hash {hash}");
            }
            else
            {
                _defsByHash[hash] = def;
            }
        }

        /// <summary>
        /// 批量注册Def
        /// </summary>
        public static void RegisterAll(IEnumerable<DefBase> defs)
        {
            foreach (var def in defs)
            {
                Register(def);
            }
        }

        /// <summary>
        /// 注销Def
        /// </summary>
        public static bool Unregister(string defId)
        {
            if (!_allDefs.TryGetValue(defId, out var def))
                return false;

            // 从总表移除
            _allDefs.Remove(defId);

            // 从类型表移除
            Type defType = def.GetType();
            if (_defsByType.TryGetValue(defType, out var typeDict))
            {
                typeDict.Remove(defId);
            }

            // 从哈希表移除
            _defsByHash.Remove(def.ShortHash);

            return true;
        }

        /// <summary>
        /// 清空所有Def
        /// </summary>
        public static void Clear()
        {
            _allDefs.Clear();
            _defsByType.Clear();
            _defsByHash.Clear();
            _initialized = false;
            _referencesResolved = false;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取Def（泛型版本）
        /// </summary>
        public static T GetDef<T>(string defId) where T : DefBase
        {
            if (string.IsNullOrEmpty(defId))
                return null;

            if (_allDefs.TryGetValue(defId, out var def))
            {
                return def as T;
            }

            return null;
        }

        /// <summary>
        /// 获取Def（非泛型版本）
        /// </summary>
        public static DefBase GetDef(string defId)
        {
            if (string.IsNullOrEmpty(defId))
                return null;

            _allDefs.TryGetValue(defId, out var def);
            return def;
        }

        /// <summary>
        /// 通过短哈希获取Def
        /// </summary>
        public static DefBase GetDefByHash(int hash)
        {
            _defsByHash.TryGetValue(hash, out var def);
            return def;
        }

        /// <summary>
        /// 通过短哈希获取Def（泛型版本）
        /// </summary>
        public static T GetDefByHash<T>(int hash) where T : DefBase
        {
            if (_defsByHash.TryGetValue(hash, out var def))
            {
                return def as T;
            }
            return null;
        }

        /// <summary>
        /// 检查Def是否存在
        /// </summary>
        public static bool HasDef(string defId)
        {
            return !string.IsNullOrEmpty(defId) && _allDefs.ContainsKey(defId);
        }

        /// <summary>
        /// 尝试获取Def
        /// </summary>
        public static bool TryGetDef<T>(string defId, out T def) where T : DefBase
        {
            def = GetDef<T>(defId);
            return def != null;
        }

        /// <summary>
        /// 获取指定类型的所有Def
        /// </summary>
        public static IEnumerable<T> GetAllDefs<T>() where T : DefBase
        {
            Type type = typeof(T);
            
            // 如果有精确类型匹配
            if (_defsByType.TryGetValue(type, out var typeDict))
            {
                foreach (var def in typeDict.Values)
                {
                    yield return (T)def;
                }
            }
            else
            {
                // 否则遍历所有Def检查类型
                foreach (var def in _allDefs.Values)
                {
                    if (def is T typedDef)
                    {
                        yield return typedDef;
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定类型的Def列表
        /// </summary>
        public static List<T> GetDefList<T>() where T : DefBase
        {
            return GetAllDefs<T>().ToList();
        }

        /// <summary>
        /// 获取指定类型的Def数量
        /// </summary>
        public static int GetDefCount<T>() where T : DefBase
        {
            Type type = typeof(T);
            if (_defsByType.TryGetValue(type, out var typeDict))
            {
                return typeDict.Count;
            }
            return _allDefs.Values.Count(d => d is T);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化所有已注册的Def
        /// 在所有Def注册完成后调用
        /// </summary>
        public static void InitializeAll()
        {
            if (_initialized)
            {
                Debug.LogWarning("[DefDatabase] Already initialized");
                return;
            }

            Debug.Log($"[DefDatabase] Initializing {_allDefs.Count} defs...");

            // 第一遍：初始化所有Def
            foreach (var def in _allDefs.Values)
            {
                try
                {
                    def.Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DefDatabase] Failed to initialize def {def.DefId}: {e.Message}\n{e.StackTrace}");
                }
            }

            _initialized = true;
            _referencesResolved = true;

            Debug.Log($"[DefDatabase] Initialization complete");
        }

        /// <summary>
        /// 重新初始化（清空并重新加载）
        /// </summary>
        public static void Reinitialize()
        {
            _initialized = false;
            _referencesResolved = false;
            InitializeAll();
        }

        #endregion

        #region Luban集成

        /// <summary>
        /// 从Luban数据创建并注册Def
        /// </summary>
        /// <typeparam name="TDef">Def类型</typeparam>
        /// <typeparam name="TLuban">Luban数据类型</typeparam>
        /// <param name="lubanData">Luban数据实例</param>
        public static TDef CreateFromLuban<TDef, TLuban>(TLuban lubanData) 
            where TDef : DefBase, new()
            where TLuban : class
        {
            var def = new TDef();
            def.InitFromLuban(lubanData);
            Register(def);
            return def;
        }

        /// <summary>
        /// 从Luban数据列表批量创建并注册Def
        /// </summary>
        public static List<TDef> CreateAllFromLuban<TDef, TLuban>(IEnumerable<TLuban> lubanDataList)
            where TDef : DefBase, new()
            where TLuban : class
        {
            var result = new List<TDef>();
            foreach (var data in lubanDataList)
            {
                var def = CreateFromLuban<TDef, TLuban>(data);
                result.Add(def);
            }
            return result;
        }

        /// <summary>
        /// 使用自定义工厂从Luban数据创建Def
        /// </summary>
        public static void LoadFromLuban<TLuban>(IEnumerable<TLuban> lubanDataList, 
            Func<TLuban, DefBase> factory) where TLuban : class
        {
            foreach (var data in lubanDataList)
            {
                var def = factory(data);
                if (def != null)
                {
                    Register(def);
                }
            }
        }

        #endregion

        #region 特定类型的快捷访问

        /// <summary>
        /// 获取地形定义
        /// </summary>
        public static TerrainDef GetTerrainDef(string defId)
        {
            return GetDef<TerrainDef>(defId);
        }

        /// <summary>
        /// 获取所有地形定义
        /// </summary>
        public static IEnumerable<TerrainDef> AllTerrainDefs => GetAllDefs<TerrainDef>();

        /// <summary>
        /// 获取地基定义
        /// </summary>
        public static FoundationDef GetFoundationDef(string defId)
        {
            return GetDef<FoundationDef>(defId);
        }

        /// <summary>
        /// 获取所有地基定义
        /// </summary>
        public static IEnumerable<FoundationDef> AllFoundationDefs => GetAllDefs<FoundationDef>();

        /// <summary>
        /// 获取地板定义
        /// </summary>
        public static FloorDef GetFloorDef(string defId)
        {
            return GetDef<FloorDef>(defId);
        }

        /// <summary>
        /// 获取所有地板定义
        /// </summary>
        public static IEnumerable<FloorDef> AllFloorDefs => GetAllDefs<FloorDef>();

        /// <summary>
        /// 获取墙壁定义
        /// </summary>
        public static WallDef GetWallDef(string defId)
        {
            return GetDef<WallDef>(defId);
        }

        /// <summary>
        /// 获取所有墙壁定义
        /// </summary>
        public static IEnumerable<WallDef> AllWallDefs => GetAllDefs<WallDef>();

        /// <summary>
        /// 获取屋顶定义
        /// </summary>
        public static RoofDef GetRoofDef(string defId)
        {
            return GetDef<RoofDef>(defId);
        }

        /// <summary>
        /// 获取所有屋顶定义
        /// </summary>
        public static IEnumerable<RoofDef> AllRoofDefs => GetAllDefs<RoofDef>();

        /// <summary>
        /// 获取实体定义
        /// </summary>
        public static EntityDef GetEntityDef(string defId)
        {
            return GetDef<EntityDef>(defId);
        }

        /// <summary>
        /// 获取所有实体定义
        /// </summary>
        public static IEnumerable<EntityDef> AllEntityDefs => GetAllDefs<EntityDef>();

        /// <summary>
        /// 获取建筑定义
        /// </summary>
        public static BuildingDef GetBuildingDef(string defId)
        {
            return GetDef<BuildingDef>(defId);
        }

        /// <summary>
        /// 获取所有建筑定义
        /// </summary>
        public static IEnumerable<BuildingDef> AllBuildingDefs => GetAllDefs<BuildingDef>();

        /// <summary>
        /// 获取物品定义
        /// </summary>
        public static ItemDef GetItemDef(string defId)
        {
            return GetDef<ItemDef>(defId);
        }

        /// <summary>
        /// 获取所有物品定义
        /// </summary>
        public static IEnumerable<ItemDef> AllItemDefs => GetAllDefs<ItemDef>();

        #endregion

        #region 调试

        /// <summary>
        /// 打印数据库状态
        /// </summary>
        public static void DebugPrint()
        {
            Debug.Log($"[DefDatabase] Status:");
            Debug.Log($"  Initialized: {_initialized}");
            Debug.Log($"  Total Defs: {_allDefs.Count}");
            Debug.Log($"  Types registered: {_defsByType.Count}");
            
            foreach (var kvp in _defsByType)
            {
                Debug.Log($"    {kvp.Key.Name}: {kvp.Value.Count}");
            }
        }

        /// <summary>
        /// 获取数据库统计信息
        /// </summary>
        public static Dictionary<string, int> GetStats()
        {
            var stats = new Dictionary<string, int>
            {
                ["Total"] = _allDefs.Count
            };

            foreach (var kvp in _defsByType)
            {
                stats[kvp.Key.Name] = kvp.Value.Count;
            }

            return stats;
        }

        #endregion
    }

    /// <summary>
    /// 泛型Def数据库（用于类型安全访问）
    /// </summary>
    /// <typeparam name="T">Def类型</typeparam>
    public static class DefDatabase<T> where T : DefBase
    {
        /// <summary>
        /// 缓存的Def列表
        /// </summary>
        private static List<T> _cachedList;

        /// <summary>
        /// 缓存是否有效
        /// </summary>
        private static bool _cacheValid = false;

        /// <summary>
        /// 所有该类型的Def
        /// </summary>
        public static IEnumerable<T> AllDefs
        {
            get
            {
                if (!_cacheValid)
                {
                    RefreshCache();
                }
                return _cachedList;
            }
        }

        /// <summary>
        /// Def数量
        /// </summary>
        public static int Count
        {
            get
            {
                if (!_cacheValid)
                {
                    RefreshCache();
                }
                return _cachedList.Count;
            }
        }

        /// <summary>
        /// 获取Def
        /// </summary>
        public static T GetDef(string defId)
        {
            return DefDatabase.GetDef<T>(defId);
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public static void RefreshCache()
        {
            _cachedList = DefDatabase.GetDefList<T>();
            _cacheValid = true;
        }

        /// <summary>
        /// 使缓存失效
        /// </summary>
        public static void InvalidateCache()
        {
            _cacheValid = false;
            _cachedList = null;
        }

        /// <summary>
        /// 随机获取一个Def
        /// </summary>
        public static T GetRandom()
        {
            if (!_cacheValid)
            {
                RefreshCache();
            }

            if (_cachedList.Count == 0)
                return null;

            return _cachedList[UnityEngine.Random.Range(0, _cachedList.Count)];
        }

        /// <summary>
        /// 根据条件筛选
        /// </summary>
        public static IEnumerable<T> Where(Func<T, bool> predicate)
        {
            return AllDefs.Where(predicate);
        }

        /// <summary>
        /// 查找第一个满足条件的Def
        /// </summary>
        public static T FirstOrDefault(Func<T, bool> predicate)
        {
            return AllDefs.FirstOrDefault(predicate);
        }
    }
}
