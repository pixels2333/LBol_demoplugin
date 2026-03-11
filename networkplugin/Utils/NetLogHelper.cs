using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NetworkPlugin.Utils;

/// <summary>
/// 网络事件日志辅助：生成稳定的 payload 指纹，并提取少量关键字段用于判重。
/// 设计目标：
/// - 不引入敏感信息泄露（Token/Key 等字段默认脱敏）
/// - 低开销（大 payload 只做指纹+长度，不做深解析）
/// - 输出中文，便于用户排查“同一事件重复发送/重复处理”
/// </summary>
public static class NetLogHelper
{
    private const int DefaultHeadLimit = 160;
    private const int MaxParseLength = 20_000; // 大于该长度时跳过 JsonDocument.Parse

    private static readonly string[] InterestingKeys =
    {
        // 通用
        "Timestamp",
        "EventId",
        "SenderPlayerId",
        "SenderName",
        "PlayerId",
        "PlayerName",

        // 地图/位置
        "LocationX",
        "LocationY",
        "LocationName",
        "Stage",
        "Act",
        "X",
        "Y",

        // 战斗/敌人
        "SpawnId",
        "RootIndex",
        "Round",
        "EnemyName",

        // 房间/追赶
        "RoomKey",
        "RoomVersion",

        // 可能包含敏感信息（会脱敏）
        "JoinToken",
        "ReconnectToken",
    };

    /// <summary>
    /// 生成日志摘要：指纹 + 长度 + 少量关键字段 + 头部预览。
    /// </summary>
    public static string BuildSummary(string messageType, string json)
    {
        json ??= string.Empty;
        ulong fp = ComputeFnv1a64(json);

        // 基础信息
        StringBuilder sb = new StringBuilder();
        sb.Append("指纹=0x");
        sb.Append(fp.ToString("x16", CultureInfo.InvariantCulture));
        sb.Append(", 长度=");
        sb.Append(json.Length.ToString(CultureInfo.InvariantCulture));

        if (string.IsNullOrWhiteSpace(json))
        {
            return sb.ToString();
        }

        // 大 payload：不深解析，避免卡顿
        if (json.Length > MaxParseLength)
        {
            sb.Append(", 预览=");
            sb.Append(Quote(TruncateOneLine(json, DefaultHeadLimit)));
            return sb.ToString();
        }

        // 尝试解析并提取字段
        if (TryExtractKeyFields(json, out var fields) && fields.Count > 0)
        {
            sb.Append(", 字段=");
            bool first = true;
            foreach (var kv in fields)
            {
                if (!first)
                {
                    sb.Append(' ');
                }

                first = false;
                sb.Append(kv.Key);
                sb.Append('=');
                sb.Append(kv.Value);
            }

            return sb.ToString();
        }

        // 解析失败或没有目标字段：给一段头部预览
        sb.Append(", 预览=");
        sb.Append(Quote(TruncateOneLine(json, DefaultHeadLimit)));
        return sb.ToString();
    }

    public static ulong ComputeFnv1a64(string s)
    {
        if (s == null)
        {
            return 0;
        }

        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        ulong hash = offset;
        for (int i = 0; i < s.Length; i++)
        {
            hash ^= s[i];
            hash *= prime;
        }

        return hash;
    }

    private static bool TryExtractKeyFields(string json, out Dictionary<string, string> fields)
    {
        fields = new Dictionary<string, string>(StringComparer.Ordinal);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (string k in InterestingKeys)
            {
                if (!doc.RootElement.TryGetProperty(k, out var v))
                {
                    continue;
                }

                string rendered = RenderValue(k, v);
                if (!string.IsNullOrWhiteSpace(rendered))
                {
                    fields[k] = rendered;
                }
            }

            return fields.Count > 0;
        }
        catch
        {
            fields.Clear();
            return false;
        }
    }

    private static string RenderValue(string key, JsonElement v)
    {
        if (IsSensitiveKey(key))
        {
            // 只暴露“存在”而不暴露内容
            return "<已脱敏>";
        }

        try
        {
            return v.ValueKind switch
            {
                JsonValueKind.String => Quote(TruncateOneLine(v.GetString(), 80)),
                JsonValueKind.Number => v.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "null",
                // 对象/数组：只显示 kind+长度（避免刷屏）
                JsonValueKind.Object or JsonValueKind.Array => $"<{v.ValueKind.ToString().ToLowerInvariant()}>",
                _ => TruncateOneLine(v.GetRawText(), 80),
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return key.IndexOf("token", StringComparison.OrdinalIgnoreCase) >= 0 ||
               key.IndexOf("secret", StringComparison.OrdinalIgnoreCase) >= 0 ||
               key.IndexOf("key", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string TruncateOneLine(string s, int maxLen)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        string one = s.Replace("\r", " ").Replace("\n", " ");
        if (one.Length <= maxLen)
        {
            return one;
        }

        return one.Substring(0, maxLen);
    }

    private static string Quote(string s)
    {
        if (s == null)
        {
            return "\"\"";
        }

        return "\"" + s.Replace("\"", "\\\"") + "\"";
    }
}
