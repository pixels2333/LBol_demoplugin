using System;
using System.Collections.Generic;
using NetworkPlugin.UI.Models;

namespace NetworkPlugin.UI.State;

/// <summary>
/// UI 层使用的“死亡玩家登记册”：从网络事件汇总死者信息，供 Gap 的复活面板展示。
/// </summary>
public static class DeathRegistry
{
    private static readonly object SyncLock = new();
    private static readonly Dictionary<string, DeadPlayerEntry> DeadPlayersById = new(StringComparer.Ordinal);

    /// <summary>
    /// 添加或更新死亡玩家记录
    /// </summary>
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

    /// <summary>
    /// 将指定玩家标记为存活（从死亡记录中移除）
    /// </summary>
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

    /// <summary>
    /// 获取当前死亡玩家列表的快照
    /// </summary>
    public static List<DeadPlayerEntry> GetDeadPlayersSnapshot()
    {
        lock (SyncLock)
        {
            return new List<DeadPlayerEntry>(DeadPlayersById.Values);
        }
    }

    /// <summary>
    /// 尝试获取指定玩家的死亡记录
    /// </summary>
    public static bool TryGetDeadPlayer(string playerId, out DeadPlayerEntry entry)
    {
        lock (SyncLock)
        {
            return DeadPlayersById.TryGetValue(playerId, out entry);
        }
    }

    /// <summary>
    /// 清空所有死亡玩家记录
    /// </summary>
    public static void Clear()
    {
        lock (SyncLock)
        {
            DeadPlayersById.Clear();
        }
    }
}
