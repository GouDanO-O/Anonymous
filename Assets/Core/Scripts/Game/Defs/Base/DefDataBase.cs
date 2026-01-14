using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Defs
{
    /// <summary>
    /// 定义数据库 - 存储和检索所有游戏定义
    /// 使用泛型静态类实现按类型分类存储
    /// </summary>
    /// <typeparam name="T">Def类型</typeparam>
    public static class DefDataBase<T> where T : BaseDef
    {
        private static List<T> _allDefs = new List<T>();
        
        private static Dictionary<string, T> _defsByName = new Dictionary<string, T>();
        
        private static bool _initialized = false;

        /// <summary>
        /// 获取所有该类型的定义
        /// </summary>
        public static IReadOnlyList<T> AllDefs => _allDefs;

        /// <summary>
        /// 定义数量
        /// </summary>
        public static int Count => _allDefs.Count;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool Initialized => _initialized;

        /// <summary>
        /// 添加定义
        /// </summary>
        public static void Add(T def)
        {
            if (def == null)
            {
                Debug.LogError($"[DefDatabase<{typeof(T).Name}>] 尝试添加null定义");
                return;
            }

            if (string.IsNullOrEmpty(def.defName))
            {
                Debug.LogError($"[DefDatabase<{typeof(T).Name}>] 定义的defName为空");
                return;
            }

            if (_defsByName.ContainsKey(def.defName))
            {
                Debug.LogWarning($"[DefDatabase<{typeof(T).Name}>] 定义 '{def.defName}' 已存在，将被覆盖");
                // 移除旧的
                var oldDef = _defsByName[def.defName];
                _allDefs.Remove(oldDef);
            }

            _allDefs.Add(def);
            _defsByName[def.defName] = def;
        }

        /// <summary>
        /// 批量添加定义
        /// </summary>
        public static void AddRange(IEnumerable<T> defs)
        {
            foreach (var def in defs)
            {
                Add(def);
            }
        }

        /// <summary>
        /// 根据defName获取定义
        /// </summary>
        /// <param name="defName">定义名称</param>
        /// <param name="errorOnFail">找不到时是否报错</param>
        /// <returns>找到的定义，或null</returns>
        public static T GetNamed(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                Debug.LogError($"[DefDatabase<{typeof(T).Name}>] 查询的defName为空");
                return null;
            }

            if (_defsByName.TryGetValue(defName, out T def))
            {
                return def;
            }

            Debug.LogError($"[DefDatabase<{typeof(T).Name}>] 找不到定义: {defName}");
            return null;
        }

        /// <summary>
        /// 尝试获取定义
        /// </summary>
        public static bool TryGetNamed(string defName, out T def)
        {
            def = GetNamed(defName);
            return def != null;
        }

        /// <summary>
        /// 检查是否存在指定defName的定义
        /// </summary>
        public static bool Contains(string defName)
        {
            return !string.IsNullOrEmpty(defName) && _defsByName.ContainsKey(defName);
        }

        /// <summary>
        /// 获取随机定义
        /// </summary>
        public static T GetRandom()
        {
            if (_allDefs.Count == 0)
            {
                return null;
            }
            return _allDefs[UnityEngine.Random.Range(0, _allDefs.Count)];
        }

        /// <summary>
        /// 根据条件查找定义
        /// </summary>
        public static T Find(Predicate<T> predicate)
        {
            return _allDefs.Find(predicate);
        }

        /// <summary>
        /// 根据条件查找所有匹配的定义
        /// </summary>
        public static List<T> FindAll(Predicate<T> predicate)
        {
            return _allDefs.FindAll(predicate);
        }

        /// <summary>
        /// 标记为已初始化
        /// </summary>
        public static void SetInitialized()
        {
            _initialized = true;
            
            // 调用所有Def的PostLoad
            foreach (var def in _allDefs)
            {
                try
                {
                    def.PostLoad();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DefDatabase<{typeof(T).Name}>] {def.defName}.PostLoad() 错误: {e}");
                }
            }
        }

        /// <summary>
        /// 解析所有Def的引用关系
        /// 应在所有DefDatabase都加载完成后调用
        /// </summary>
        public static void ResolveAllReferences()
        {
            foreach (var def in _allDefs)
            {
                try
                {
                    def.ResolveReferences();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DefDatabase<{typeof(T).Name}>] {def.defName}.ResolveReferences() 错误: {e}");
                }
            }
        }

        /// <summary>
        /// 验证所有定义的配置
        /// </summary>
        public static void ValidateAll()
        {
            foreach (var def in _allDefs)
            {
                var errors = def.ConfigErrors();
                if (errors != null && errors.Length > 0)
                {
                    foreach (var error in errors)
                    {
                        Debug.LogWarning($"[DefDatabase<{typeof(T).Name}>] 配置错误 - {def.defName}: {error}");
                    }
                }
            }
        }

        /// <summary>
        /// 清空数据库
        /// </summary>
        public static void Clear()
        {
            _allDefs.Clear();
            _defsByName.Clear();
            _initialized = false;
        }
    }
}