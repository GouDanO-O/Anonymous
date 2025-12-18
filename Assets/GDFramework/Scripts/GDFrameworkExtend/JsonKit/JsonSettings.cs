using Newtonsoft.Json;

namespace GDFrameworkExtend.JsonKit
{
    /// <summary>
    /// JSON 序列化配置管理器
    /// </summary>
    public static class JsonSettings
    {
        private static JsonSerializerSettings _settings;

        /// <summary>
        /// 获取默认的 JSON 序列化设置
        /// </summary>
        public static JsonSerializerSettings Default
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new JsonSerializerSettings
                    {
                        // 格式化输出
                        Formatting = Formatting.Indented,
                        
                        // 忽略循环引用
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        
                        // 忽略 null 值
                        NullValueHandling = NullValueHandling.Ignore,
                        
                        // 添加 Unity 类型转换器
                        Converters = new JsonConverter[]
                        {
                            new Vector2Converter(),
                            new Vector3Converter(),
                            new Vector2IntConverter(),
                            new Vector3IntConverter(),
                            new QuaternionConverter(),
                            new ColorConverter()
                        }
                    };
                }
                return _settings;
            }
        }

        /// <summary>
        /// 获取紧凑的 JSON 序列化设置(不格式化)
        /// </summary>
        public static JsonSerializerSettings Compact
        {
            get
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = Default.Converters
                };
                return settings;
            }
        }
    }
}