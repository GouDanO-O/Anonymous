using System.Collections.Generic;
using UnityEngine.Events;

namespace Core.Game.Defs
{
    /// <summary>
    /// DefDatabase管理器 - DefDataBase
    /// </summary>
    public static class DefDataBaseManager
    {
        private static List<UnityAction> _clearActions = new List<UnityAction>();
        private static List<UnityAction> _resolveActions = new List<UnityAction>();
        private static List<UnityAction> _validateActions = new List<UnityAction>();

        /// <summary>
        /// 注册一个DefDatabase类型
        /// </summary>
        public static void Register<T>() where T : BaseDef
        {
            _clearActions.Add(() => DefDataBase<T>.Clear());
            _resolveActions.Add(() => DefDataBase<T>.ResolveAllReferences());
            _validateActions.Add(() => DefDataBase<T>.ValidateAll());
        }

        /// <summary>
        /// 清空所有DefDatabase
        /// </summary>
        public static void ClearAll()
        {
            foreach (var action in _clearActions)
            {
                action();
            }
        }

        /// <summary>
        /// 解析所有DefDatabase的引用
        /// </summary>
        public static void ResolveAllReferences()
        {
            foreach (var action in _resolveActions)
            {
                action();
            }
        }

        /// <summary>
        /// 验证所有DefDatabase
        /// </summary>
        public static void ValidateAll()
        {
            foreach (var action in _validateActions)
            {
                action();
            }
        }
    }
}