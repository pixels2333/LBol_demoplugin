using System;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetworkPlugin.Utils;

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

            }

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

            }

            return prop;
        }
    }

    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new SystemTextJsonAttributeContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,

        Formatting = Formatting.None,
        DateParseHandling = DateParseHandling.DateTime,
        Converters = { new IPEndPointNewtonsoftConverter() },
    };

        public static string Serialize(object value)
    {

        return JsonConvert.SerializeObject(value, Settings);
    }

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
                using JsonDocument doc = JsonDocument.Parse(s);
                return doc.RootElement.Clone();
            }

            using JsonDocument doc2 = JsonDocument.Parse(Serialize(payload));
            return doc2.RootElement.Clone();
        }
        catch
        {
            return default;
        }
    }
}
