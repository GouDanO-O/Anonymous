/*******************************************************************************
 * 文件名:    DefBase.cs
 * 描述:      所有Def定义的基类
 * 作者:      TycoonGame
 * 创建时间:  2024
 * 
 * 使用说明:
 *   DefBase 是所有游戏定义（Definition）的基类。
 *   Def是数据驱动设计的核心，所有游戏内容（物品、建筑、地形等）都通过Def定义。
 *   
 *   Def系统与Luban配置表对接：
 *   - Luban生成数据类（纯数据）
 *   - Def类包装Luban数据，提供运行时逻辑
 *   - DefDatabase统一管理所有Def实例
 ******************************************************************************/

using System;
using UnityEngine;

namespace TycoonGame.MapSystem
{
    /// <summary>
    /// Def基类 - 所有定义的基类
    /// </summary>
    [Serializable]
    public abstract class DefBase
    {
        #region 核心字段

        /// <summary>
        /// 定义ID（唯一标识符，与Luban配置表ID对应）
        /// </summary>
        [SerializeField]
        internal string _defId;

        /// <summary>
        /// 显示名称（本地化key或直接文本）
        /// </summary>
        [SerializeField]
        internal string _defName;

        /// <summary>
        /// 描述文本（本地化key或直接文本）
        /// </summary>
        [SerializeField]
        internal string _description;

        /// <summary>
        /// 短哈希值（用于快速比较和网络同步）
        /// </summary>
        private int _shortHash;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _initialized;

        #endregion

        #region 属性

        /// <summary>
        /// 定义ID（只读）
        /// </summary>
        public string DefId => _defId;

        /// <summary>
        /// 显示名称
        /// </summary>
        public virtual string DefName => _defName;

        /// <summary>
        /// 描述文本
        /// </summary>
        public virtual string Description => _description;

        /// <summary>
        /// 短哈希值（基于DefId计算）
        /// </summary>
        public int ShortHash
        {
            get
            {
                if (_shortHash == 0 && !string.IsNullOrEmpty(_defId))
                {
                    _shortHash = ComputeShortHash(_defId);
                }
                return _shortHash;
            }
        }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Def类型名称（用于调试）
        /// </summary>
        public virtual string DefTypeName => GetType().Name;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        protected DefBase()
        {
        }

        /// <summary>
        /// 带ID的构造函数
        /// </summary>
        protected DefBase(string defId)
        {
            _defId = defId;
        }

        /// <summary>
        /// 完整构造函数
        /// </summary>
        protected DefBase(string defId, string defName, string description = null)
        {
            _defId = defId;
            _defName = defName;
            _description = description;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化Def（在所有Def加载完成后调用）
        /// 用于解析引用、验证数据等
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            OnPreInitialize();
            ResolveReferences();
            OnPostInitialize();
            Validate();

            _initialized = true;
        }

        /// <summary>
        /// 初始化前回调
        /// </summary>
        protected virtual void OnPreInitialize()
        {
        }

        /// <summary>
        /// 解析引用（将DefId字符串转换为Def引用）
        /// </summary>
        protected virtual void ResolveReferences()
        {
        }

        /// <summary>
        /// 初始化后回调
        /// </summary>
        protected virtual void OnPostInitialize()
        {
        }

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        protected virtual void Validate()
        {
            if (string.IsNullOrEmpty(_defId))
            {
                Debug.LogWarning($"[{DefTypeName}] DefId is null or empty");
            }
        }

        #endregion

        #region 从Luban数据初始化

        /// <summary>
        /// 从Luban配置数据初始化
        /// 子类应覆写此方法以处理特定字段
        /// </summary>
        /// <typeparam name="T">Luban生成的数据类型</typeparam>
        /// <param name="lubanData">Luban数据实例</param>
        public virtual void InitFromLuban<T>(T lubanData) where T : class
        {
            // 基类尝试通过反射获取通用字段
            var type = lubanData.GetType();

            // 尝试获取Id字段
            _defId = GetMemberValue(type, lubanData, "Id")?.ToString();

            // 尝试获取Name字段
            _defName = GetMemberValue(type, lubanData, "Name")?.ToString();

            // 尝试获取Description/Desc字段
            _description = GetMemberValue(type, lubanData, "Description")?.ToString() 
                        ?? GetMemberValue(type, lubanData, "Desc")?.ToString();
        }

        /// <summary>
        /// 通过反射获取成员值（支持字段和属性）
        /// </summary>
        private static object GetMemberValue(Type type, object instance, string memberName)
        {
            // 先尝试字段
            var field = type.GetField(memberName);
            if (field != null)
            {
                return field.GetValue(instance);
            }

            // 再尝试属性
            var property = type.GetProperty(memberName);
            if (property != null)
            {
                return property.GetValue(instance);
            }

            return null;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 计算短哈希值
        /// 使用稳定的哈希算法，确保不同运行时结果一致
        /// </summary>
        private static int ComputeShortHash(string str)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in str)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }

        /// <summary>
        /// 获取本地化后的名称
        /// </summary>
        public virtual string GetLocalizedName()
        {
            // TODO: 接入本地化系统
            // return LocalizationManager.GetText(_defName);
            return _defName;
        }

        /// <summary>
        /// 获取本地化后的描述
        /// </summary>
        public virtual string GetLocalizedDescription()
        {
            // TODO: 接入本地化系统
            return _description;
        }

        #endregion

        #region 比较和哈希

        public override bool Equals(object obj)
        {
            if (obj is DefBase other)
            {
                return _defId == other._defId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _defId?.GetHashCode() ?? 0;
        }

        public static bool operator ==(DefBase a, DefBase b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a._defId == b._defId;
        }

        public static bool operator !=(DefBase a, DefBase b)
        {
            return !(a == b);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"{DefTypeName}({_defId})";
        }

        /// <summary>
        /// 详细信息（调试用）
        /// </summary>
        public virtual string ToDetailedString()
        {
            return $"{DefTypeName}: Id={_defId}, Name={_defName}";
        }

        #endregion
    }

    /// <summary>
    /// 泛型Def基类 - 支持强类型的Luban数据
    /// </summary>
    /// <typeparam name="TLubanData">Luban生成的数据类型</typeparam>
    public abstract class DefBase<TLubanData> : DefBase where TLubanData : class
    {
        /// <summary>
        /// 原始Luban数据（可选保留）
        /// </summary>
        protected TLubanData _rawData;

        /// <summary>
        /// 原始Luban数据
        /// </summary>
        public TLubanData RawData => _rawData;

        /// <summary>
        /// 从Luban数据初始化（强类型版本）
        /// </summary>
        public virtual void InitFromLuban(TLubanData lubanData)
        {
            _rawData = lubanData;
            base.InitFromLuban(lubanData);
        }

        /// <summary>
        /// 泛型版本调用强类型版本
        /// </summary>
        public override void InitFromLuban<T>(T lubanData)
        {
            if (lubanData is TLubanData typedData)
            {
                InitFromLuban(typedData);
            }
            else
            {
                base.InitFromLuban(lubanData);
            }
        }
    }
}
