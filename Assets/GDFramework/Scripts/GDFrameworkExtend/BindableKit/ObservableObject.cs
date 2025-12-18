using System.Collections.Generic;
using UnityEngine;

namespace GDFrameworkExtend.EventKit
{
/// <summary>
    /// 可观察对象扩展，支持对象属性级别的监控
    /// </summary>
    [System.Serializable]
    public class ObservableObject<T> where T : class, new()
    {
        [SerializeField] private T _value;
        private readonly List<System.Action<T>> _onValueChanged = new List<System.Action<T>>();
        
        public ObservableObject()
        {
            _value = new T();
        }
        
        public ObservableObject(T initialValue)
        {
            _value = initialValue ?? new T();
        }
        
        public T Value
        {
            get { return _value; }
        }
        
        /// <summary>
        /// 设置值并触发事件
        /// </summary>
        public void SetValue(T newValue)
        {
            if (!ReferenceEquals(_value, newValue))
            {
                _value = newValue;
                TriggerValueChanged();
            }
        }
        
        /// <summary>
        /// 设置值但不触发事件
        /// </summary>
        public void SetValueWithoutEvent(T newValue)
        {
            _value = newValue;
        }
        
        /// <summary>
        /// 手动触发值变化事件（用于修改对象内部属性后）
        /// </summary>
        public void NotifyValueChanged()
        {
            TriggerValueChanged();
        }
        
        /// <summary>
        /// 注册值变化监听
        /// </summary>
        public ObservableObject<T> RegisterOnValueChanged(System.Action<T> onValueChanged)
        {
            _onValueChanged.Add(onValueChanged);
            return this;
        }
        
        /// <summary>
        /// 取消注册值变化监听
        /// </summary>
        public void UnRegisterOnValueChanged(System.Action<T> onValueChanged)
        {
            _onValueChanged.Remove(onValueChanged);
        }
        
        /// <summary>
        /// 安全地修改值的属性
        /// </summary>
        public void ModifyValue(System.Action<T> modifier)
        {
            modifier?.Invoke(_value);
            TriggerValueChanged();
        }
        
        private void TriggerValueChanged()
        {
            for (int i = 0; i < _onValueChanged.Count; i++)
            {
                _onValueChanged[i]?.Invoke(_value);
            }
        }
        
        /// <summary>
        /// 清除所有监听
        /// </summary>
        public void Clear()
        {
            _onValueChanged.Clear();
        }
        
        // 隐式转换
        public static implicit operator T(ObservableObject<T> bindableProperty)
        {
            return bindableProperty.Value;
        }
    }
    
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ObservableObjectExtensions
    {
        /// <summary>
        /// 创建一个可观察对象
        /// </summary>
        public static ObservableObject<T> ToObservable<T>(this T obj) where T : class, new()
        {
            return new ObservableObject<T>(obj);
        }
    }
}