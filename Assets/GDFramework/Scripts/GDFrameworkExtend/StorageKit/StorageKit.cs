using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GDFramework.Models;
using GDFramework.Scripts.Cheater;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFrameworkExtend.Data;
using GDFrameworkExtend.SingletonKit;
using UnityEngine;

namespace GDFrameworkExtend.StorageKit
{
    public class StorageKit : AbstractSystem
    {
        private Dictionary<object, Dictionary<string, MemberInfo>> _registeredObjects =
            new Dictionary<object, Dictionary<string, MemberInfo>>();

        /// <summary>
        /// 默认存档槽
        /// </summary>
        private string _currentSaveSlot = "default";

        protected override void OnInit()
        {
            this.RegisterEvents();
        }

        private void RegisterEvents()
        {
            this.GetModel<CheatDataModel>().AddCheatModule("存储所有数据", new StorageSaveAllCheatCommand(){});
            this.GetModel<CheatDataModel>().AddCheatModule("清除所有数据", new StorageClearAllCheatCommand());
        }

        public bool IsNewGame()
        {
            bool isNewGame = false;
            if (!ES3.FileExists())
                return true;
            var keys = ES3.GetKeys();
            if (keys==null || keys.Length == 0)
                return true;
            
            var saveSlotKeys = keys.Where(key => key.Contains($"_{_currentSaveSlot}_")).ToList();
            
            isNewGame = saveSlotKeys.Count == 0;
            
            LogKit.LogKit.Log("是否是新游戏:"+isNewGame);
            return isNewGame;
        }

        /// <summary>
        /// 注册可存储的对象
        /// </summary>
        public void RegisterSaveableObject(object obj)
        {
            if (_registeredObjects.ContainsKey(obj))
                return;
            Debug.Log($"注册对象: {obj.GetType().Name}");
            var memberInfos = GetAutoSaveMembers(obj.GetType());
            _registeredObjects[obj] = memberInfos;
            // 立即加载数据
            LoadObjectData(obj, memberInfos);
            // 注册BindableProperty变化监听
            RegisterPropertyChangeListeners(obj, memberInfos);
        }

        /// <summary>
        /// 注销可存储的对象
        /// </summary>
        public void UnregisterSaveableObject(object obj)
        {
            if (_registeredObjects.ContainsKey(obj))
            {
                _registeredObjects.Remove(obj);
            }
        }

        /// <summary>
        /// 设置当前存档槽
        /// </summary>
        public void SetCurrentSaveSlot(string slotName)
        {
            _currentSaveSlot = slotName;
        }

        /// <summary>
        /// 保存单个字段/属性
        /// </summary>
        public void SaveSingleField(object obj, string memberName)
        {
            Debug.Log($"尝试保存字段: {obj.GetType().Name}.{memberName}");
            if (!_registeredObjects.ContainsKey(obj))
            {
                Debug.LogWarning($"对象 {obj.GetType().Name} 未注册");
                return;
            }


            var memberInfos = _registeredObjects[obj];
            if (!memberInfos.ContainsKey(memberName))
                return;

            var memberInfo = memberInfos[memberName];
            var saveKey = GetSaveKey(memberInfo, obj.GetType(), obj);

            object value = null;
            Type memberType = null;

            if (memberInfo is FieldInfo fieldInfo)
            {
                value = fieldInfo.GetValue(obj);
                memberType = fieldInfo.FieldType;
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                value = propertyInfo.GetValue(obj);
                memberType = propertyInfo.PropertyType;
            }

            if (IsBindableProperty(memberType))
            {
                var bindableValue = GetBindablePropertyValue(value);
                SaveValue(saveKey, bindableValue, GetBindablePropertyInnerType(memberType));
            }
            else
            {
                SaveValue(saveKey, value, memberType);
            }
        }

        /// <summary>
        /// 保存对象的所有数据
        /// </summary>
        public void SaveObjectData(object obj)
        {
            if (!_registeredObjects.ContainsKey(obj))
                return;

            var memberInfos = _registeredObjects[obj];
            foreach (var kvp in memberInfos)
            {
                SaveSingleField(obj, kvp.Key);
            }
        }

        /// <summary>
        /// 加载对象数据
        /// </summary>
        private void LoadObjectData(object obj, Dictionary<string, MemberInfo> memberInfos)
        {
            foreach (var kvp in memberInfos)
            {
                var memberInfo = kvp.Value;
                var saveKey = GetSaveKey(memberInfo, obj.GetType(), obj);

                if (memberInfo is FieldInfo fieldInfo)
                {
                    if (IsBindableProperty(fieldInfo.FieldType))
                    {
                        var bindableProperty = fieldInfo.GetValue(obj);
                        if (bindableProperty == null)
                        {
                            bindableProperty = CreateBindableProperty(fieldInfo.FieldType);
                            fieldInfo.SetValue(obj, bindableProperty);
                        }

                        var loadedValue = LoadValue(saveKey, GetBindablePropertyInnerType(fieldInfo.FieldType));
                        if (loadedValue != null)
                        {
                            SetBindablePropertyValue(bindableProperty, loadedValue);
                        }
                    }
                    else
                    {
                        var loadedValue = LoadValue(saveKey, fieldInfo.FieldType);
                        if (loadedValue != null)
                        {
                            fieldInfo.SetValue(obj, loadedValue);
                        }
                    }
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    if (IsBindableProperty(propertyInfo.PropertyType))
                    {
                        var bindableProperty = propertyInfo.GetValue(obj);
                        if (bindableProperty == null)
                        {
                            bindableProperty = CreateBindableProperty(propertyInfo.PropertyType);
                            propertyInfo.SetValue(obj, bindableProperty);
                        }

                        var loadedValue = LoadValue(saveKey, GetBindablePropertyInnerType(propertyInfo.PropertyType));
                        if (loadedValue != null)
                        {
                            SetBindablePropertyValue(bindableProperty, loadedValue);
                        }
                    }
                    else
                    {
                        var loadedValue = LoadValue(saveKey, propertyInfo.PropertyType);
                        if (loadedValue != null)
                        {
                            propertyInfo.SetValue(obj, loadedValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册BindableProperty变化监听
        /// </summary>
        private void RegisterPropertyChangeListeners(object obj, Dictionary<string, MemberInfo> memberInfos)
        {
            foreach (var kvp in memberInfos)
            {
                var memberInfo = kvp.Value;
                var memberName = kvp.Key;

                object bindableProperty = null;
                Type memberType = null;

                if (memberInfo is FieldInfo fieldInfo && IsBindableProperty(fieldInfo.FieldType))
                {
                    bindableProperty = fieldInfo.GetValue(obj);
                    memberType = fieldInfo.FieldType;

                    // 如果BindableProperty为null，先创建实例
                    if (bindableProperty == null)
                    {
                        bindableProperty = CreateBindableProperty(fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, bindableProperty);
                    }
                }
                else if (memberInfo is PropertyInfo propertyInfo && IsBindableProperty(propertyInfo.PropertyType))
                {
                    bindableProperty = propertyInfo.GetValue(obj);
                    memberType = propertyInfo.PropertyType;

                    // 如果BindableProperty为null，先创建实例
                    if (bindableProperty == null)
                    {
                        bindableProperty = CreateBindableProperty(propertyInfo.PropertyType);
                        propertyInfo.SetValue(obj, bindableProperty);
                    }
                }

                if (bindableProperty != null)
                {
                    // 创建特定的回调，捕获当前的obj和memberName
                    var specificCallback =
                        CreateSpecificCallback(obj, memberName, GetBindablePropertyInnerType(memberType));

                    // 使用反射注册监听
                    var registerMethod = memberType.GetMethod("Register",
                        new[] { typeof(Action<>).MakeGenericType(GetBindablePropertyInnerType(memberType)) });
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(bindableProperty, new[] { specificCallback });
                    }
                }
            }
        }

        /// <summary>
        /// 创建特定的回调，避免闭包问题
        /// </summary>
        private object CreateSpecificCallback(object targetObj, string memberName, Type valueType)
        {
            var actionType = typeof(Action<>).MakeGenericType(valueType);
            var method = typeof(StorageKit).GetMethod(nameof(SpecificCallbackWrapper),
                BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(valueType);

            // 创建一个包装方法，传递必要的参数
            var wrapper = new Action<object>(value => SaveSingleField(targetObj, memberName));

            return Delegate.CreateDelegate(actionType, this, genericMethod);
        }

        private void SpecificCallbackWrapper<T>(T value)
        {
            // 这里需要通过其他方式获取对象和成员信息
            // 由于闭包限制，暂时保持全量保存
            SaveAllRegisteredObjects();
        }

        /// <summary>
        /// 保存所有注册的对象
        /// </summary>
        public void SaveAllRegisteredObjects()
        {
            foreach (var obj in _registeredObjects.Keys.ToList())
            {
                SaveObjectData(obj);
            }
        }

        /// <summary>
        /// 获取存储键
        /// </summary>
        private string GetSaveKey(MemberInfo member, Type objectType, object instance)
        {
            var autoSaveAttr = member.GetCustomAttribute<AutoSaveAttribute>();
            var baseKey = autoSaveAttr?.SaveKey ?? $"{objectType.Name}_{member.Name}";

            if (typeof(PersistentData).IsAssignableFrom(objectType))
            {
                //生成唯一标识
                var instanceId = instance.GetHashCode();
                return $"Persistent_{_currentSaveSlot}_{objectType.Name}_{instanceId}_{baseKey}";
            }
            else if (typeof(TemporaryData).IsAssignableFrom(objectType))
            {
                var instanceId = instance.GetHashCode();
                return $"Temporal_{_currentSaveSlot}_{objectType.Name}_{instanceId}_{baseKey}";
            }
            else if (typeof(AbstractModel).IsAssignableFrom(objectType))
            {
                return $"Model_{_currentSaveSlot}_{baseKey}";
            }
            else
            {
                return $"Data_{_currentSaveSlot}_{baseKey}";
            }
        }

        /// <summary>
        /// 获取标记了AutoSave特性的成员
        /// </summary>
        private Dictionary<string, MemberInfo> GetAutoSaveMembers(Type type)
        {
            var result = new Dictionary<string, MemberInfo>();

            // 获取字段
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<AutoSaveAttribute>() != null);

            foreach (var field in fields)
            {
                result[field.Name] = field;
            }

            // 获取属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<AutoSaveAttribute>() != null);

            foreach (var property in properties)
            {
                result[property.Name] = property;
            }

            return result;
        }

        /// <summary>
        /// 保存值到Easy Save
        /// </summary>
        private void SaveValue(string key, object value, Type valueType)
        {
            if (value == null) 
                return;
            
            try
            {
                if (valueType == typeof(int))
                {
                    ES3.Save(key, (int)value);
                }
                else if (valueType == typeof(float))
                {
                    ES3.Save(key, (float)value);
                }
                else if (valueType == typeof(string))
                {
                    ES3.Save(key, (string)value);
                }
                else if (valueType == typeof(bool))
                {
                    ES3.Save(key, (bool)value);
                }
                else if (valueType == typeof(Vector2))
                {
                    ES3.Save(key, (Vector2)value);
                }
                else if (valueType == typeof(Vector3))
                {
                    ES3.Save(key, (Vector3)value);
                }
                else if (valueType.IsEnum)
                {
                    ES3.Save(key, value.ToString());
                }
                else
                {
                    ES3.Save(key, value);
                }
                LogKit.LogKit.Log("保存字段:"+key+"=>"+value.ToString()+"=>"+valueType.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError($"保存数据失败: {key}, 错误: {e.Message}");
            }
        }

        /// <summary>
        /// 从Easy Save加载值
        /// </summary>
        private object LoadValue(string key, Type valueType)
        {
            if (!ES3.KeyExists(key)) 
                return null;

            try
            {
                if (valueType == typeof(int))
                {
                    return ES3.Load<int>(key);
                }
                else if (valueType == typeof(float))
                {
                    return ES3.Load<float>(key);
                }
                else if (valueType == typeof(string))
                {
                    return ES3.Load<string>(key);
                }
                else if (valueType == typeof(bool))
                {
                    return ES3.Load<bool>(key);
                }
                else if (valueType == typeof(Vector2))
                {
                    return ES3.Load<Vector2>(key);
                }
                else if (valueType == typeof(Vector3))
                {
                    return ES3.Load<Vector3>(key);
                }
                else if (valueType.IsEnum)
                {
                    var enumString = ES3.Load<string>(key);
                    return Enum.Parse(valueType, enumString);
                }
                else
                {
                    return ES3.Load(key, valueType);
                }
                LogKit.LogKit.Log("加载值:"+key+"=>"+valueType.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError($"加载数据失败: {key}, 错误: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 判断是否为BindableProperty类型
        /// </summary>
        private bool IsBindableProperty(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BindableProperty<>);
        }

        /// <summary>
        /// 获取BindableProperty的内部类型
        /// </summary>
        private Type GetBindablePropertyInnerType(Type bindablePropertyType)
        {
            return bindablePropertyType.GetGenericArguments()[0];
        }

        /// <summary>
        /// 创建BindableProperty实例
        /// </summary>
        private object CreateBindableProperty(Type bindablePropertyType)
        {
            return Activator.CreateInstance(bindablePropertyType);
        }

        /// <summary>
        /// 获取BindableProperty的值
        /// </summary>
        private object GetBindablePropertyValue(object bindableProperty)
        {
            if (bindableProperty == null) return null;
            var valueProperty = bindableProperty.GetType().GetProperty("Value");
            return valueProperty?.GetValue(bindableProperty);
        }

        /// <summary>
        /// 设置BindableProperty的值
        /// </summary>
        private void SetBindablePropertyValue(object bindableProperty, object value)
        {
            if (bindableProperty == null) return;
            var valueProperty = bindableProperty.GetType().GetProperty("Value");
            valueProperty?.SetValue(bindableProperty, value);
            LogKit.LogKit.Log("设置存储值:"+bindableProperty.GetType().ToString()+"=>"+value.ToString());
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        public void DeleteSaveSlot(string slotName)
        {
            var keys = ES3.GetKeys();
            var keysToDelete = keys.Where(key => key.Contains($"_{slotName}_")).ToList();

            foreach (var key in keysToDelete)
            {
                ES3.DeleteKey(key);
            }
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void ClearAllData()
        {
            LogKit.LogKit.Log("清除所有数据");
            ES3.DeleteFile();
        }
    }
}