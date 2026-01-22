using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Host 侧转发补丁：把客户端上报的 Battle*Report 转发成 Battle*Broadcast。
/// </summary>
/// <remarks>
/// 约定：
/// - 客户端（非 Host）只发送 *Report，上报给 Host/Server。
/// - Host 负责把 Report 重新广播为 Broadcast，让其他客户端都能收到一致事件。
///
/// 注意：这里只做“事件转发”，不做任何战斗状态落地；真正落地仍由各自客户端的接收逻辑决定。
/// </remarks>
[HarmonyPatch]
public static class BattleReportForwardPatch
{
	private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

	private static readonly object SyncLock = new();
	private static bool _subscribed;
	private static INetworkClient _subscribedClient;

	// 轻量去重：同一 PlayerId+TargetId+EventType 在同一 Timestamp 下只转发一次。
	private static readonly Dictionary<string, long> _lastForwardedTicksByKey = new(StringComparer.Ordinal);
	private const int MaxForwardedKeys = 256;

	private static INetworkClient TryGetNetworkClient()
	{
		try
		{
			return ServiceProvider?.GetService<INetworkClient>();
		}
		catch
		{
			return null;
		}
	}

	[HarmonyTargetMethod]
	private static MethodBase TargetMethod()
	{
		// 某些构建环境下 GameDirector 类型可能不在可直接引用的程序集里，
		// 这里用字符串反射定位，避免编译期依赖。
		return AccessTools.Method("LBoL.Presentation.GameDirector:Update")
			   ?? AccessTools.Method("LBoL.Presentation.UI.GameDirector:Update");
	}

	[HarmonyPostfix]
	private static void Postfix()
	{
		INetworkClient client = TryGetNetworkClient();
		if (client == null)
		{
			return;
		}

		EnsureSubscribed(client);
	}

	private static void EnsureSubscribed(INetworkClient client)
	{
		lock (SyncLock)
		{
			if (_subscribed && ReferenceEquals(_subscribedClient, client))
			{
				return;
			}
		}

		try
		{
			if (_subscribedClient != null)
			{
				_subscribedClient.OnGameEventReceived -= OnGameEventReceived;
			}
		}
		catch
		{
			// ignored
		}

		try
		{
			client.OnGameEventReceived += OnGameEventReceived;

			lock (SyncLock)
			{
				_subscribedClient = client;
				_subscribed = true;
			}
		}
		catch
		{
			lock (SyncLock)
			{
				_subscribedClient = null;
				_subscribed = false;
			}
		}
	}

	private static void OnGameEventReceived(string eventType, object payload)
	{
		if (string.IsNullOrWhiteSpace(eventType))
		{
			return;
		}

		// 只处理 Report，避免转发 Broadcast 造成循环。
		if (!eventType.EndsWith("Report", StringComparison.Ordinal))
		{
			return;
		}

		// 仅转发 BattlePlayer*Report（避免误转发其他模块的 Report）。
		if (!eventType.StartsWith("BattlePlayer", StringComparison.Ordinal))
		{
			return;
		}

		INetworkClient client;
		lock (SyncLock)
		{
			client = _subscribedClient;
		}

		if (client?.IsConnected != true)
		{
			return;
		}

		// 只有 Host 才做转发。
		if (!NetworkIdentityTracker.GetSelfIsHost())
		{
			return;
		}

		string broadcastType = MapToBroadcastType(eventType);
		if (string.IsNullOrWhiteSpace(broadcastType))
		{
			return;
		}

		if (!TryGetJsonElement(payload, out JsonElement root))
		{
			return;
		}

		long ts = TryGetLong(root, "Timestamp") ?? 0;
		string playerId = TryGetString(root, "PlayerId") ?? string.Empty;
		string targetId = TryGetString(root, "TargetId") ?? string.Empty;
		string key = $"{eventType}|{playerId}|{targetId}";

		lock (SyncLock)
		{
			if (_lastForwardedTicksByKey.TryGetValue(key, out long lastTs) && lastTs == ts)
			{
				return;
			}

			_lastForwardedTicksByKey[key] = ts;

			if (_lastForwardedTicksByKey.Count > MaxForwardedKeys)
			{
				_lastForwardedTicksByKey.Clear();
			}
		}

		// 直接复用原 payload（Host 不修改内容，只负责转发）。
		client.SendGameEventData(broadcastType, payload);
	}

	private static string MapToBroadcastType(string reportType)
	{
		// 明确映射：避免把未知 Report 误转发。
		return reportType switch
		{
			NetworkMessageTypes.BattlePlayerDamageReport => NetworkMessageTypes.BattlePlayerDamageBroadcast,
			NetworkMessageTypes.BattlePlayerHealReport => NetworkMessageTypes.BattlePlayerHealBroadcast,
			NetworkMessageTypes.BattlePlayerStatusEffectsDeltaReport => NetworkMessageTypes.BattlePlayerStatusEffectsDeltaBroadcast,
			NetworkMessageTypes.BattlePlayerStatusEffectsFullReport => NetworkMessageTypes.BattlePlayerStatusEffectsFullBroadcast,
			_ => null,
		};
	}

	private static bool TryGetJsonElement(object payload, out JsonElement root)
	{
		try
		{
			if (payload is JsonElement je)
			{
				root = je;
				return true;
			}

			if (payload is string s)
			{
				root = JsonDocument.Parse(s).RootElement;
				return true;
			}
		}
		catch
		{
			// ignored
		}

		root = default;
		return false;
	}

	private static string TryGetString(JsonElement root, string prop)
	{
		if (root.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		if (!root.TryGetProperty(prop, out JsonElement el))
		{
			return null;
		}

		return el.ValueKind == JsonValueKind.String ? el.GetString() : null;
	}

	private static long? TryGetLong(JsonElement root, string prop)
	{
		if (root.ValueKind != JsonValueKind.Object)
		{
			return null;
		}

		if (!root.TryGetProperty(prop, out JsonElement el))
		{
			return null;
		}

		return el.ValueKind switch
		{
			JsonValueKind.Number => el.TryGetInt64(out long v) ? v : null,
			JsonValueKind.String => long.TryParse(el.GetString(), out long v) ? v : null,
			_ => null,
		};
	}
}