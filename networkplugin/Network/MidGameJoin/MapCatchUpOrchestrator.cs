using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LBoL.Core;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 客户端追赶执行器（最小骨架）：
/// - 接收主机 FullStateSnapshot（重点是 MapState）并暂存。
/// - 当本地 GameRun 可用（通常在地图界面）时，将 MapState 尽力应用到当前地图。
///
/// 备注：本类不负责“创建新 GameRun”；无本地 GameRun 时仅暂存并等待。
/// </summary>
public sealed class MapCatchUpOrchestrator
{
    private readonly ManualLogSource _logger;

    private readonly object _lock = new();

    private FullStateSnapshot _pendingSnapshot;
    private long _pendingReceivedAtUtcTicks;
    private string _pendingCheckpointId = string.Empty;
    private long _lastSeedMismatchLogAtUtcTicks;
    private string _lastSeedMismatchCheckpointId = string.Empty;
    private bool _applied;

    public MapCatchUpOrchestrator(ManualLogSource logger)
    {
        _logger = logger ?? Plugin.Logger;
    }

    /// <summary>
    /// 设置待应用的 FullSnapshot（通常来自中途加入 FullStateSyncResponse）。
    /// </summary>
    public void SetPendingFullSnapshot(FullStateSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        lock (_lock)
        {
            _pendingSnapshot = snapshot;
            _pendingReceivedAtUtcTicks = DateTime.UtcNow.Ticks;
            _pendingCheckpointId = snapshot.MapState?.LastCheckpointId ?? string.Empty;
            _applied = false;
        }

        _logger?.LogInfo($"[MapCatchUp] Pending snapshot stored: ts={snapshot.Timestamp}, checkpoint={_pendingCheckpointId}");
    }

    /// <summary>
    /// 若存在待应用快照且本地 GameRun 已就绪，则尽力将 MapState 应用到当前地图。
    /// </summary>
    public bool TryApplyPendingToCurrentRun()
    {
        FullStateSnapshot snapshot;
        long receivedAt;
        string checkpointId;

        lock (_lock)
        {
            if (_pendingSnapshot == null || _applied)
            {
                return false;
            }

            snapshot = _pendingSnapshot;
            receivedAt = _pendingReceivedAtUtcTicks;
            checkpointId = _pendingCheckpointId;
        }

        try
        {
            GameRunController run = GameStateUtils.GetCurrentGameRun();
            if (run == null || run.CurrentMap == null)
            {
                return false;
            }

            if (snapshot.MapState == null)
            {
                return false;
            }

            if (!IsLikelySameMapSeed(run, snapshot.MapState))
            {
                MaybeLogSeedMismatch(checkpointId);
                return false;
            }

            ApplyMapStateToRun(run, snapshot.MapState);
            MarkApplied();

            _logger?.LogInfo($"[MapCatchUp] Applied MapState: checkpoint={checkpointId}, receivedAt={receivedAt}");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"[MapCatchUp] Apply degraded: {ex.Message}");
            return false;
        }
    }

    private void MarkApplied()
    {
        lock (_lock)
        {
            _applied = true;
        }
    }

    private void MaybeLogSeedMismatch(string checkpointId)
    {
        try
        {
            long now = DateTime.UtcNow.Ticks;
            const long logIntervalTicks = TimeSpan.TicksPerSecond * 5;

            lock (_lock)
            {
                if (string.Equals(_lastSeedMismatchCheckpointId, checkpointId ?? string.Empty, StringComparison.Ordinal) &&
                    now - _lastSeedMismatchLogAtUtcTicks < logIntervalTicks)
                {
                    return;
                }

                _lastSeedMismatchCheckpointId = checkpointId ?? string.Empty;
                _lastSeedMismatchLogAtUtcTicks = now;
            }

            _logger?.LogWarning($"[MapCatchUp] Pending MapState waiting for map seed alignment: checkpoint={checkpointId}");
        }
        catch
        {
            // ignored
        }
    }

    private static bool IsLikelySameMapSeed(GameRunController run, MapStateSnapshot mapState)
    {
        try
        {
            if (mapState == null || mapState.MapSeedUlong == null)
            {
                return true;
            }

            ulong? local = run?.CurrentStage?.MapSeed;
            if (local == null)
            {
                return true;
            }

            return local.Value == mapState.MapSeedUlong.Value;
        }
        catch
        {
            return true;
        }
    }

    private void ApplyMapStateToRun(GameRunController run, MapStateSnapshot mapState)
    {
        GameMap map = run.CurrentMap;
        if (map == null)
        {
            return;
        }

        // 1) 节点状态：按 host 的 node.Status.ToString() 反向设置。
        if (mapState.NodeStates != null && mapState.NodeStates.Count > 0)
        {
            foreach (KeyValuePair<string, string> kv in mapState.NodeStates)
            {
                string nodeKey = kv.Key;
                string state = kv.Value;

                if (!TryParseNodeKey(nodeKey, out int act, out int x, out int y, out string stationType))
                {
                    continue;
                }

                MapNode node = TryFindNode(map, act, x, y);
                if (node == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(stationType) &&
                    !string.Equals(node.StationType.ToString(), stationType, StringComparison.Ordinal))
                {
                    // 坐标是主键；StationType 不一致仅记录，不阻断。
                }

                if (!Enum.TryParse(state, out MapNodeStatus status))
                {
                    continue;
                }

                TrySetNodeStatus(node, status);
            }
        }

        // 2) 路径（仅用于 UI/追赶基准，不触发 EnterNode，避免刷 RoomStateRequest）。
        TryApplyPath(map, mapState.PathHistory);

        // 3) 当前位置（尽力设置 VisitingNode；失败则不阻断）。
        TryApplyCurrentLocation(map, mapState);
    }

    private static bool TryParseNodeKey(string nodeKey, out int act, out int x, out int y, out string stationType)
    {
        act = 0;
        x = 0;
        y = 0;
        stationType = string.Empty;

        if (string.IsNullOrWhiteSpace(nodeKey))
        {
            return false;
        }

        string[] parts = nodeKey.Split(':');
        if (parts.Length < 4)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out act) || !int.TryParse(parts[1], out x) || !int.TryParse(parts[2], out y))
        {
            return false;
        }

        stationType = parts[3] ?? string.Empty;
        return true;
    }

    private static MapNode TryFindNode(GameMap map, int act, int x, int y)
    {
        try
        {
            if (map != null && x >= 0 && x < map.Levels && y >= 0 && y < map.Width)
            {
                MapNode node = map.Nodes[x, y];
                if (node != null && (act <= 0 || node.Act == act))
                {
                    return node;
                }
            }

            if (map?.AllNodes == null)
            {
                return null;
            }

            return act > 0
                ? map.AllNodes.FirstOrDefault(n => n != null && n.Act == act && n.X == x && n.Y == y)
                : map.AllNodes.FirstOrDefault(n => n != null && n.X == x && n.Y == y);
        }
        catch
        {
            return null;
        }
    }

    private static void TrySetNodeStatus(MapNode node, MapNodeStatus status)
    {
        try
        {
            Traverse.Create(node).Property("Status").SetValue(status);
        }
        catch
        {
            // ignored
        }
    }

    private static void TryApplyPath(GameMap map, List<LocationSnapshot> pathHistory)
    {
        try
        {
            if (map == null)
            {
                return;
            }

            var list = Traverse.Create(map).Field("_path").GetValue<List<MapNode>>();
            if (list == null)
            {
                return;
            }

            list.Clear();

            if (pathHistory == null || pathHistory.Count == 0)
            {
                return;
            }

            foreach (LocationSnapshot loc in pathHistory)
            {
                if (loc == null)
                {
                    continue;
                }

                int act;
                int x;
                int y;
                if (!TryParseNodeKey(loc.NodeId, out act, out x, out y, out _))
                {
                    act = 0;
                    x = loc.X;
                    y = loc.Y;
                }

                MapNode node = TryFindNode(map, act, x, y);
                if (node != null)
                {
                    list.Add(node);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryApplyCurrentLocation(GameMap map, MapStateSnapshot mapState)
    {
        try
        {
            if (map == null || mapState?.CurrentLocation == null)
            {
                return;
            }

            int act;
            int x;
            int y;
            if (!TryParseNodeKey(mapState.CurrentLocation.NodeId, out act, out x, out y, out _))
            {
                act = 0;
                x = mapState.CurrentLocation.X;
                y = mapState.CurrentLocation.Y;
            }

            MapNode node = TryFindNode(map, act, x, y);
            if (node == null)
            {
                return;
            }

            // VisitingNode 私有 setter：用反射/Traverse 尽力设置。
            try
            {
                Traverse.Create(map).Property("VisitingNode").SetValue(node);
            }
            catch
            {
                // ignored
            }
        }
        catch
        {
            // ignored
        }
    }
}
