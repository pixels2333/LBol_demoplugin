using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.RoomSync;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// GapOptions 同步补丁。
/// </summary>
public static class GapOptionsSyncPatch
{
	private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

	private static readonly object SyncLock = new();
	private static bool _subscribed;
	private static INetworkClient _subscribedClient;

	private static readonly object DedupLock = new();
	private static readonly HashSet<string> ProcessedActionIds = [];
	private static readonly Queue<string> ProcessedActionOrder = [];
	private const int MaxProcessedActionCount = 512;

	private static readonly object RoomEventsLock = new();
	private static readonly Dictionary<string, Queue<GapOptionsEventSnapshot>> RoomEventsByRoomKey = new(StringComparer.Ordinal);
	private const int MaxRoomEventsPerRoom = 8;

	public static void EnsureSubscribed(INetworkClient client)
	{
		if (client == null)
		{
			return;
		}

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
				_subscribedClient.OnGameEventReceived -= OnGameEventReceived;
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

	public static void BroadcastGapOptionsEvent(string eventType, string cardId = null, string cardName = null)
	{
		if (!IsGapOptionsEvent(eventType))
		{
			return;
		}

		INetworkClient client = ServiceProvider?.GetService<INetworkClient>();
		if (client?.IsConnected != true)
		{
			return;
		}

		string playerId = NetworkIdentityTracker.GetSelfPlayerId();
		if (string.IsNullOrWhiteSpace(playerId))
		{
			playerId = GameStateUtils.GetCurrentPlayerId();
		}

		string actionId = Guid.NewGuid().ToString("N");
		string roomKey = RoomSyncManager.GetLastEnteredRoomKey() ?? string.Empty;
		long timestampUtcTicks = DateTime.UtcNow.Ticks;

		GapOptionsEventSnapshot snapshot = new()
		{
			ActionId = actionId,
			EventType = eventType,
			PlayerId = playerId ?? string.Empty,
			CardId = cardId ?? string.Empty,
			CardName = cardName ?? string.Empty,
			RoomKey = roomKey,
			TimestampUtcTicks = timestampUtcTicks,
		};

		TryMarkFirstSeen(actionId);
		AppendRoomEvent(snapshot);

		client.SendGameEventData(eventType, new
		{
			ActionId = actionId,
			PlayerId = playerId ?? string.Empty,
			CardId = cardId ?? string.Empty,
			CardName = cardName ?? string.Empty,
			RoomKey = roomKey,
			Timestamp = timestampUtcTicks,
		});
	}

	private static void OnGameEventReceived(string eventType, object payload)
	{
		if (!IsGapOptionsEvent(eventType) || !TryGetJsonElement(payload, out JsonElement root))
		{
			return;
		}

		string actionId = GetString(root, "ActionId");
		if (string.IsNullOrWhiteSpace(actionId))
		{
			actionId = BuildFallbackActionId(eventType, root);
		}

		if (string.IsNullOrWhiteSpace(actionId) || !TryMarkFirstSeen(actionId))
		{
			return;
		}

		string playerId = GetString(root, "PlayerId") ?? "unknown";
		string cardName = GetString(root, "CardName") ?? string.Empty;
		string cardId = GetString(root, "CardId") ?? string.Empty;
		string roomKey = GetString(root, "RoomKey") ?? string.Empty;
		long timestampUtcTicks = TryGetLong(root, "Timestamp") ?? DateTime.UtcNow.Ticks;

		AppendRoomEvent(new GapOptionsEventSnapshot
		{
			ActionId = actionId,
			EventType = eventType,
			PlayerId = playerId,
			CardId = cardId,
			CardName = cardName,
			RoomKey = roomKey,
			TimestampUtcTicks = timestampUtcTicks,
		});

		Plugin.Logger?.LogInfo($"[GapOptionsSync] recv {eventType}: player={playerId}, card={cardName}/{cardId}, actionId={actionId}");
	}

	/// <summary>
	/// 获取指定房间最近 GapOptions 事件（默认最多 8 条）。
	/// </summary>
	public static List<GapOptionsEventSnapshot> GetRecentGapOptionsEvents(string roomKey, int maxCount = MaxRoomEventsPerRoom)
	{
		if (string.IsNullOrWhiteSpace(roomKey))
		{
			return [];
		}

		if (maxCount <= 0)
		{
			maxCount = MaxRoomEventsPerRoom;
		}

		lock (RoomEventsLock)
		{
			if (!RoomEventsByRoomKey.TryGetValue(roomKey, out Queue<GapOptionsEventSnapshot> queue) || queue.Count == 0)
			{
				return [];
			}

			List<GapOptionsEventSnapshot> events = [];
			foreach (GapOptionsEventSnapshot e in queue)
			{
				events.Add(CloneEvent(e));
			}

			if (events.Count > maxCount)
			{
				events.RemoveRange(0, events.Count - maxCount);
			}

			return events;
		}
	}

	/// <summary>
	/// 合并中途加入追赶下发的 GapOptions 事件：仅缓存并按 ActionId 去重标记，不执行游戏动作。
	/// </summary>
	public static void MergeCatchupGapOptionsEvents(string roomKey, IReadOnlyList<GapOptionsEventSnapshot> events)
	{
		if (string.IsNullOrWhiteSpace(roomKey) || events == null || events.Count == 0)
		{
			return;
		}

		for (int i = 0; i < events.Count; i++)
		{
			GapOptionsEventSnapshot e = events[i];
			if (e == null || string.IsNullOrWhiteSpace(e.EventType) || !IsGapOptionsEvent(e.EventType))
			{
				continue;
			}

			string actionId = e.ActionId;
			if (string.IsNullOrWhiteSpace(actionId))
			{
				actionId = $"{e.EventType}:{e.PlayerId}:{e.CardId}:{e.TimestampUtcTicks}";
			}

			if (!TryMarkFirstSeen(actionId))
			{
				continue;
			}

			AppendRoomEvent(new GapOptionsEventSnapshot
			{
				ActionId = actionId,
				EventType = e.EventType,
				PlayerId = e.PlayerId ?? string.Empty,
				CardId = e.CardId ?? string.Empty,
				CardName = e.CardName ?? string.Empty,
				RoomKey = roomKey,
				TimestampUtcTicks = e.TimestampUtcTicks,
			});
		}
	}

	private static bool IsGapOptionsEvent(string eventType)
	{
		return string.Equals(eventType, NetworkMessageTypes.GapOptionsUpgradeSelected, StringComparison.Ordinal) ||
			   string.Equals(eventType, NetworkMessageTypes.GapOptionsRemoveCard, StringComparison.Ordinal) ||
			   string.Equals(eventType, NetworkMessageTypes.GapStationEntered, StringComparison.Ordinal) ||
			   string.Equals(eventType, NetworkMessageTypes.DrinkTeaStarted, StringComparison.Ordinal) ||
			   string.Equals(eventType, NetworkMessageTypes.DrinkTeaCompleted, StringComparison.Ordinal);
	}

	private static string BuildFallbackActionId(string eventType, JsonElement root)
	{
		string playerId = GetString(root, "PlayerId") ?? string.Empty;
		string cardId = GetString(root, "CardId") ?? string.Empty;
		string roomKey = GetString(root, "RoomKey") ?? string.Empty;
		string timestamp = root.TryGetProperty("Timestamp", out JsonElement ts) ? ts.GetRawText() : string.Empty;
		return $"{eventType}:{playerId}:{cardId}:{roomKey}:{timestamp}";
	}

	private static bool TryGetProperty(JsonElement elem, string property, out JsonElement result)
	{
		result = default;
		return elem.ValueKind == JsonValueKind.Object && elem.TryGetProperty(property, out result);
	}

	private static bool TryGetJsonElement(object payload, out JsonElement root)
		=> NetworkEventHelper.TryGetJsonElement(payload, out root);

	private static string GetString(JsonElement root, string name)
		=> NetworkEventHelper.GetString(root, name);

	private static long? TryGetLong(JsonElement elem, string property)
	{
		try
		{
			if (!TryGetProperty(elem, property, out JsonElement p))
			{
				return null;
			}

			if (p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out long n))
			{
				return n;
			}

			if (p.ValueKind == JsonValueKind.String && long.TryParse(p.GetString(), out n))
			{
				return n;
			}

			return null;
		}
		catch
		{
			return null;
		}
	}

	private static void AppendRoomEvent(GapOptionsEventSnapshot snapshot)
	{
		if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.RoomKey) || !IsGapOptionsEvent(snapshot.EventType))
		{
			return;
		}

		lock (RoomEventsLock)
		{
			if (!RoomEventsByRoomKey.TryGetValue(snapshot.RoomKey, out Queue<GapOptionsEventSnapshot> queue))
			{
				queue = new Queue<GapOptionsEventSnapshot>();
				RoomEventsByRoomKey[snapshot.RoomKey] = queue;
			}

			queue.Enqueue(CloneEvent(snapshot));
			while (queue.Count > MaxRoomEventsPerRoom)
			{
				queue.Dequeue();
			}
		}
	}

	private static GapOptionsEventSnapshot CloneEvent(GapOptionsEventSnapshot e)
	{
		return new GapOptionsEventSnapshot
		{
			ActionId = e.ActionId ?? string.Empty,
			EventType = e.EventType ?? string.Empty,
			PlayerId = e.PlayerId ?? string.Empty,
			CardId = e.CardId ?? string.Empty,
			CardName = e.CardName ?? string.Empty,
			RoomKey = e.RoomKey ?? string.Empty,
			TimestampUtcTicks = e.TimestampUtcTicks,
		};
	}

	private static bool TryMarkFirstSeen(string actionId)
	{
		if (string.IsNullOrWhiteSpace(actionId))
		{
			return false;
		}

		lock (DedupLock)
		{
			if (!ProcessedActionIds.Add(actionId))
			{
				return false;
			}

			ProcessedActionOrder.Enqueue(actionId);
			TrimProcessedActions_NoLock();
			return true;
		}
	}

	private static void TrimProcessedActions_NoLock()
	{
		while (ProcessedActionOrder.Count > MaxProcessedActionCount)
		{
			string oldActionId = ProcessedActionOrder.Dequeue();
			ProcessedActionIds.Remove(oldActionId);
		}
	}
}
