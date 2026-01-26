using System;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetworkPlugin.Utils;

/// <summary>
/// Unity/Mono 兼容的 JSON 序列化/反序列化封装。
/// 目的：避免 System.Text.Json 在部分运行时触发 Utf8JsonWriter 相关崩溃，同时保留项目现有的 JsonPropertyName 映射。
/// </summary>
public static class JsonCompat
{
    private sealed class IPEndPointNewtonsoftConverter : Newtonsoft.Json.JsonConverter<IPEndPoint>
    {
        public override void WriteJson(JsonWriter writer, IPEndPoint value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // IPv6 常见格式为 [::1]:1234，这里输出同样的兼容格式。
            string ip = value.Address?.ToString() ?? string.Empty;
            string s = ip.Contains(":", StringComparison.Ordinal) ? $"[{ip}]:{value.Port}" : $"{ip}:{value.Port}";
            writer.WriteValue(s);
        }

        public override IPEndPoint ReadJson(JsonReader reader, Type objectType, IPEndPoint existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.String)
            {
                return null;
            }

            string s = reader.Value as string;
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            // IPv6: [::1]:1234
            if (s.StartsWith("[", StringComparison.Ordinal))
            {
                int idx = s.IndexOf("]:", StringComparison.Ordinal);
                if (idx <= 0)
                {
                    return null;
                }

                string ipPart = s.Substring(1, idx - 1);
                string portPart = s.Substring(idx + 2);
                if (!IPAddress.TryParse(ipPart, out var ip) || !int.TryParse(portPart, out int port))
                {
                    return null;
                }

                return new IPEndPoint(ip, port);
            }

            int lastColon = s.LastIndexOf(':');
            if (lastColon <= 0)
            {
                return null;
            }

            string ipStr = s.Substring(0, lastColon);
            string portStr = s.Substring(lastColon + 1);
            if (!IPAddress.TryParse(ipStr, out var ip4) || !int.TryParse(portStr, out int port4))
            {
                return null;
            }

            return new IPEndPoint(ip4, port4);
        }
    }

    private sealed class SystemTextJsonAttributeContractResolver : DefaultContractResolver
    {
        private static readonly ConcurrentDictionary<MemberInfo, string> _nameCache = new();

        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            Newtonsoft.Json.Serialization.JsonProperty prop = base.CreateProperty(member, memberSerialization);

            // 保持与 System.Text.Json 的 [JsonPropertyName] 一致，避免协议字段名改变。
            try
            {
                string mapped = _nameCache.GetOrAdd(member, static m =>
                {
                    var attr = m.GetCustomAttribute<JsonPropertyNameAttribute>(inherit: true);
                    return attr?.Name;
                });

                if (!string.IsNullOrWhiteSpace(mapped))
                {
                    prop.PropertyName = mapped;
                }
            }
            catch
            {
                // ignored
            }

            // 兼容 [JsonIgnore]（System.Text.Json 的特性）。
            try
            {
                var ignore = member.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>(inherit: true);
                if (ignore != null)
                {
                    prop.Ignored = true;
                }
            }
            catch
            {
                // ignored
            }

            return prop;
        }
    }

    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new SystemTextJsonAttributeContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        // 维持可读的 JSON（便于排查联机协议）。
        Formatting = Formatting.None,
        DateParseHandling = DateParseHandling.DateTime,
        Converters = { new IPEndPointNewtonsoftConverter() },
    };

    /// <summary>
    /// 将对象序列化为 JSON 字符串。
    /// </summary>
    public static string Serialize(object value)
    {
        // 重要：不要“直接返回 string”，因为调用方期望得到 JSON（string 需要被加引号）。
        return JsonConvert.SerializeObject(value, Settings);
    }

    /// <summary>
    /// 将 JSON 字符串反序列化为指定类型。
    /// </summary>
    public static T Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// 将任意 payload 归一化为 JsonElement（只用于读取字段）。
    /// </summary>
    public static JsonElement ToJsonElement(object payload)
    {
        try
        {
            if (payload is JsonElement je)
            {
                return je;
            }

            if (payload is string s)
            {
                return JsonDocument.Parse(s).RootElement;
            }

            // 先用 Newtonsoft 生成 JSON，再用 System.Text.Json 解析为 JsonElement（避免 Utf8JsonWriter）。
            return JsonDocument.Parse(Serialize(payload)).RootElement;
        }
        catch
        {
            return default;
        }
    }
}
