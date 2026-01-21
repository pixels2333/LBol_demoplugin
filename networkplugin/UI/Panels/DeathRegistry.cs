using System;
using System.Collections.Generic;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// UI 层使用的“死亡玩家登记册”：从网络事件汇总死者信息，供 Gap 的复活面板展示。
/// </summary>
public static class DeathRegistry
{
    private static readonly object SyncLock = new();
    private static readonly Dictionary<string, DeadPlayerEntry> DeadPlayersById = new(StringComparer.Ordinal);

    public static void UpsertDeadPlayer(DeadPlayerEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.PlayerId))
        {
            return;
        }

        lock (SyncLock)
        {
            DeadPlayersById[entry.PlayerId] = entry;
        }
    }

    public static void MarkAlive(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (SyncLock)
        {
            DeadPlayersById.Remove(playerId);
        }
    }

    public static List<DeadPlayerEntry> GetDeadPlayersSnapshot()
    {
        lock (SyncLock)
        {
            return new List<DeadPlayerEntry>(DeadPlayersById.Values);
        }
    }

    public static bool TryGetDeadPlayer(string playerId, out DeadPlayerEntry entry)
    {
        lock (SyncLock)
        {
            return DeadPlayersById.TryGetValue(playerId, out entry);
        }
    }

    public static void Clear()
    {
        lock (SyncLock)
        {
            DeadPlayersById.Clear();
        }
    }
}
