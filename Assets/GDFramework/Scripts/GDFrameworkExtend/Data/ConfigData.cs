using System;
using System.IO;
using GDFramework.Utility;
using GDFrameworkExtend.JsonKit;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GDFrameworkExtend.Data
{
    [Serializable]
    public abstract class ConfigData : PersistentData
    {
        [LabelText("配置名称")] public string configName;

        [LabelText("配置唯一ID")] public string configId;

        [LabelText("配置描述")] public string configDes;

        public virtual void LoadConfigData()
        {
        }

        public virtual void SaveConfigData()
        {
        }

        public virtual void SaveConfigData(string directory) => SaveConfigData(directory, JsonSettings.Default);

        public virtual void SaveConfigData(string directory, JsonSerializerSettings settings)
        {

        }
    }
}