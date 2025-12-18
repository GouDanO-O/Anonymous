using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GDFrameworkExtend.JsonKit
{
    /// <summary>
    /// Vector3Int 的 JSON 转换器
    /// </summary>
    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, 
            bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            return new Vector3Int(
                obj["x"]?.Value<int>() ?? 0,
                obj["y"]?.Value<int>() ?? 0,
                obj["z"]?.Value<int>() ?? 0
            );
        }
    }
}