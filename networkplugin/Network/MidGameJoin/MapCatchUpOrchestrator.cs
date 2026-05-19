using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.RoomSync;
using NetworkPlugin.Network.Services;
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

    private CatchUpSession _session;

    private bool _startGamePrompted;

    // Prevent spamming room-state requests for the same checkpoint/location.
    private string _lastRoomStateRequestedCheckpointId = string.Empty;

    private sealed class CatchUpSession
    {
        public FullStateSnapshot Snapshot;
        public string CheckpointId;
        public long ReceivedAtUtcTicks;
        public long CreatedAtUtcTicks;

        public bool PathInitialized;
        public int PathIndex;

        public List<KeyValuePair<string, string>> NodeStatePairs;
        public int NodeStateIndex;

        public bool CurrentLocationApplied;
        public bool RoomStateRequested;
        public bool Completed;

        public HashSet<string> ClearedNodeKeys;
        public HashSet<string> SettledNodeKeys;

        // When not empty, catch-up pauses and drives reward/settlement UI for this node.
        public string PendingSettlementNodeKey;
        public Station PendingSettlementStation;
        public bool PendingBossExhibitShown;
        public bool PendingRewardShown;
    }

    public MapCatchUpOrchestrator(ManualLogSource logger)
    {
        _logger = logger ?? Plugin.Logger;
    }

    /// <summary>
    /// Main-thread pump for catch-up:
    /// - Apply pending snapshot when a local run is ready.
    /// - If joiner has no local run yet, best-effort prompt StartGamePanel.
    ///
    /// This is called from <see cref="Plugin.Update"/> periodically.
    /// </summary>
    public void PumpMainThread()
    {
        try
        {
            // First try to apply map state if possible.
            // PumpMainThread is throttled (Plugin.Update), so use a larger budget to keep progress reasonable.
            TryApplyPendingToCurrentRun(pathStepsBudget: 4, nodeStatesBudget: 120);

            // If still pending and joiner has no run, try to prompt StartGamePanel.
            if (TryGetPendingFullSnapshot(out FullStateSnapshot snapshot))
            {
                TryPromptStartGameForJoiner_NoThrow(snapshot);
            }
        }
        catch
        {
            // ignored
        }
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
            _startGamePrompted = false;
            _session = null;

            // Allow requesting room state again for the new pending snapshot.
            _lastRoomStateRequestedCheckpointId = string.Empty;
        }

        _logger?.LogInfo($"[MapCatchUp] Pending snapshot stored: ts={snapshot.Timestamp}, checkpoint={_pendingCheckpointId}");

        // UI-related prompts must run on the main thread; the periodic PumpMainThread will handle it.
    }

    /// <summary>
    /// Joiner start-game lock needs to read host config from the pending snapshot.
    /// Returns false when there is no snapshot or it has already been applied.
    /// </summary>
    public bool TryGetPendingFullSnapshot(out FullStateSnapshot snapshot)
    {
        snapshot = null;

        lock (_lock)
        {
            if (_pendingSnapshot == null || _applied)
            {
                return false;
            }

            snapshot = _pendingSnapshot;
            return snapshot != null;
        }
    }

    private void TryPromptStartGameForJoiner_NoThrow(FullStateSnapshot snapshot)
    {
        try
        {
            if (_startGamePrompted)
            {
                return;
            }

            if (snapshot?.GameState == null)
            {
                return;
            }

            // Only prompt when there is no active run.
            if (GameStateUtils.GetCurrentGameRun() != null)
            {
                return;
            }

            // Only prompt when UI is ready.
            if (!UiManager.IsInitialized)
            {
                return;
            }

            // Require host start config present; otherwise we cannot guarantee deterministic alignment.
            if (snapshot.GameState.RootSeed == null ||
                snapshot.GameState.Difficulty == null ||
                snapshot.GameState.StageTypeNames == null ||
                snapshot.GameState.StageTypeNames.Count == 0)
            {
                return;
            }

            _startGamePrompted = true;

            // Build a StartGameData that matches the host stage list.
            StartGameData data = new StartGameData
            {
                StagesCreateFunc = () =>
                {
                    List<Stage> stages = new();
                    foreach (string name in snapshot.GameState.StageTypeNames)
                    {
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        Stage s = Library.CreateStage(name);
                        if (s != null)
                        {
                            stages.Add(s);
                        }
                    }

                    // Fallback to the game's default mode if stage creation failed.
                    if (stages.Count == 0)
                    {
                        return new Stage[]
                        {
                            Library.CreateStage<BambooForest>(),
                            Library.CreateStage<XuanwuRavine>(),
                            Library.CreateStage<WindGodLake>().AsNormalFinal(),
                            Library.CreateStage<FinalStage>().AsTrueEndFinal(),
                        };
                    }

                    // Apply final-stage flags to keep behavior consistent with the default mode.
                    if (stages.Count >= 4)
                    {
                        stages[2]?.AsNormalFinal();
                        stages[3]?.AsTrueEndFinal();
                    }

                    return stages.ToArray();
                },
                DebutAdventure = typeof(Debut),
            };

            // Friendly prompt: character is selectable, but difficulty/seed will be locked to the host.
            try
            {
                string diff = snapshot.GameState.Difficulty?.ToString() ?? "<unknown>";
                UiManager.GetDialog<MessageDialog>().Show(new MessageContent
                {
                    Text = "检测到正在进行的联机对局。\n\n请先选择角色开始新局以加入追赶。\n\n提示：难度/地图种子/关卡列表将自动锁定为房主设置。\n（你仍可以选择角色）",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.Confirm,
                    OnConfirm = () =>
                    {
                        try
                        {
                            UiManager.GetPanel<StartGamePanel>()?.Show(data);
                        }
                        catch
                        {
                            // ignored
                        }
                    },
                });
            }
            catch
            {
                // If dialog fails, still try to open the panel.
                UiManager.GetPanel<StartGamePanel>()?.Show(data);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 若存在待应用快照且本地 GameRun 已就绪，则尽力将 MapState 应用到当前地图。
    /// </summary>
    public bool TryApplyPendingToCurrentRun(int pathStepsBudget = 1, int nodeStatesBudget = 25)
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

            // Ensure a session exists (and is tied to the current pending snapshot).
            CatchUpSession session = GetOrCreateSession_NoThrow(snapshot, checkpointId, receivedAt);
            if (session == null)
            {
                return false;
            }

            // 0) Drive reward/settlement UI first (it may pause catch-up until the panel is closed).
            if (StepDriveSettlementUi_NoThrow(run, session))
            {
                return false;
            }

            // Sanitize budgets (negative budgets are treated as zero).
            if (pathStepsBudget < 0)
            {
                pathStepsBudget = 0;
            }

            if (nodeStatesBudget < 0)
            {
                nodeStatesBudget = 0;
            }

            // 0) Path: rebuild the path by calling GameMap.EnterNode(forced=true) incrementally.
            // RoomStateSyncPatch ignores forced EnterNode, so this won't spam RoomStateRequest.
            StepApplyPathByEnterNode(run.CurrentMap, snapshot.MapState.PathHistory, session, pathStepsBudget);

            // After path advanced, a cleared battle node may require reward/settlement UI.
            if (StepDriveSettlementUi_NoThrow(run, session))
            {
                return false;
            }

            // 1) Node statuses: apply host's node.Status strings in small batches.
            if (IsPathDone(session))
            {
                StepApplyNodeStates(run.CurrentMap, session, nodeStatesBudget);
            }

            // 2) Current location: set VisitingNode once path + statuses are done.
            if (IsPathDone(session) && IsNodeStatesDone(session) && !session.CurrentLocationApplied)
            {
                TryApplyCurrentLocation(run.CurrentMap, snapshot.MapState);
                session.CurrentLocationApplied = true;
            }

            // 3) Room state request: once per checkpoint after location is aligned.
            if (IsPathDone(session) && IsNodeStatesDone(session) && session.CurrentLocationApplied && !session.RoomStateRequested)
            {
                // Catch-up avoids EnterNode by design, so RoomStateSyncPatch won't fire automatically.
                TryRequestRoomStateAfterMapApplied_NoThrow(snapshot);
                session.RoomStateRequested = true;
            }

            // 4) Completion: only mark applied after the session has fully finished all steps.
            if (IsPathDone(session) && IsNodeStatesDone(session) && session.CurrentLocationApplied && session.RoomStateRequested)
            {
                session.Completed = true;
                MarkApplied();
                ClearPendingAfterApplied_NoThrow();
                _logger?.LogInfo($"[MapCatchUp] Applied MapState (incremental): checkpoint={checkpointId}, receivedAt={receivedAt}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"[MapCatchUp] Apply degraded: {ex.Message}");
            return false;
        }
    }

    private CatchUpSession GetOrCreateSession_NoThrow(FullStateSnapshot snapshot, string checkpointId, long receivedAtUtcTicks)
    {
        try
        {
            lock (_lock)
            {
                if (_pendingSnapshot == null || _applied)
                {
                    return null;
                }

                // If a new snapshot arrived between the outer capture and this call, abort this tick.
                // Next tick will capture and apply the latest pending snapshot.
                if (!ReferenceEquals(_pendingSnapshot, snapshot))
                {
                    return null;
                }

                if (_session != null && ReferenceEquals(_session.Snapshot, snapshot) &&
                    string.Equals(_session.CheckpointId, checkpointId ?? string.Empty, StringComparison.Ordinal))
                {
                    return _session;
                }

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
                try
                {
                    if (snapshot?.MapState?.NodeStates != null && snapshot.MapState.NodeStates.Count > 0)
                    {
                        pairs = snapshot.MapState.NodeStates.ToList();
                    }
                }
                catch
                {
                    pairs = new List<KeyValuePair<string, string>>();
                }

                _session = new CatchUpSession
                {
                    Snapshot = snapshot,
                    CheckpointId = checkpointId ?? string.Empty,
                    ReceivedAtUtcTicks = receivedAtUtcTicks,
                    CreatedAtUtcTicks = DateTime.UtcNow.Ticks,
                    PathInitialized = false,
                    PathIndex = 0,
                    NodeStatePairs = pairs,
                    NodeStateIndex = 0,
                    CurrentLocationApplied = false,
                    RoomStateRequested = false,
                    Completed = false,

                    ClearedNodeKeys = new HashSet<string>(StringComparer.Ordinal),
                    SettledNodeKeys = new HashSet<string>(StringComparer.Ordinal),
                    PendingSettlementNodeKey = string.Empty,
                    PendingSettlementStation = null,
                    PendingBossExhibitShown = false,
                    PendingRewardShown = false,
                };

                try
                {
                    if (snapshot?.MapState?.ClearedNodes != null)
                    {
                        foreach (string k in snapshot.MapState.ClearedNodes)
                        {
                            if (!string.IsNullOrWhiteSpace(k))
                            {
                                _session.ClearedNodeKeys.Add(k);
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return _session;
            }
        }
        catch
        {
            return null;
        }
    }

    private static bool IsPathDone(CatchUpSession session)
    {
        try
        {
            if (session == null)
            {
                return true;
            }

            int total = session.Snapshot?.MapState?.PathHistory?.Count ?? 0;
            return session.PathIndex >= total;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsNodeStatesDone(CatchUpSession session)
    {
        try
        {
            if (session == null)
            {
                return true;
            }

            int total = session.NodeStatePairs?.Count ?? 0;
            return session.NodeStateIndex >= total;
        }
        catch
        {
            return true;
        }
    }

    private static void StepApplyPathByEnterNode(GameMap map, List<LocationSnapshot> pathHistory, CatchUpSession session, int pathStepsBudget)
    {
        try
        {
            if (map == null || session == null)
            {
                return;
            }

            if (!session.PathInitialized)
            {
                // Clear existing path to avoid appending duplicates.
                try
                {
                    var list = Traverse.Create(map).Field("_path").GetValue<List<MapNode>>();
                    list?.Clear();
                }
                catch
                {
                    // ignored
                }

                // Reset visiting node so the first EnterNode won't mark an unrelated node as visited.
                try
                {
                    Traverse.Create(map).Property("VisitingNode").SetValue(null);
                }
                catch
                {
                    // ignored
                }

                session.PathInitialized = true;
                session.PathIndex = 0;
            }

            int total = pathHistory?.Count ?? 0;
            if (total <= 0)
            {
                session.PathIndex = 0;
                return;
            }

            if (pathStepsBudget <= 0)
            {
                return;
            }

            for (int i = 0; i < pathStepsBudget && session.PathIndex < total; i++)
            {
                LocationSnapshot loc = null;
                try
                {
                    loc = pathHistory[session.PathIndex];
                }
                catch
                {
                    loc = null;
                }

                session.PathIndex++;

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
                if (node == null)
                {
                    continue;
                }

                // EnterNode requires Active/CrossActive when forced=false; we always force.
                // MapNode.Status setter is internal in the game assembly, so use reflection.
                TrySetNodeStatus(node, MapNodeStatus.Active);

                try
                {
                    map.EnterNode(node, freeMove: true, forced: true);
                }
                catch
                {
                    // ignored
                }

                // If this is a cleared battle node, pause catch-up and let the user settle rewards.
                TryBeginSettlementForNode_NoThrow(map, node, session);
                if (!string.IsNullOrWhiteSpace(session.PendingSettlementNodeKey))
                {
                    break;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryBeginSettlementForNode_NoThrow(GameMap map, MapNode node, CatchUpSession session)
    {
        try
        {
            if (map == null || node == null || session == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(session.PendingSettlementNodeKey))
            {
                // Already settling something.
                return;
            }

            string nodeKey = BuildNodeKey(node);
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                return;
            }

            if (session.SettledNodeKeys.Contains(nodeKey))
            {
                return;
            }

            if (session.ClearedNodeKeys == null || !session.ClearedNodeKeys.Contains(nodeKey))
            {
                return;
            }

            // Only battle nodes have “RewardPanel” settlement. (Other stations have their own UIs.)
            if (node.StationType != StationType.Enemy && node.StationType != StationType.EliteEnemy && node.StationType != StationType.Boss)
            {
                session.SettledNodeKeys.Add(nodeKey);
                return;
            }

            // Create a Station instance and pre-generate its rewards.
            // We rely on the current run's stage to create a station (pathHistory is expected to be within the current stage).
            // Stage.CreateStation wires GameRun/Stage/Act/Level/BossId and is the minimal safe entrypoint.
            GameRunController run = null;
            try
            {
                run = GameStateUtils.GetCurrentGameRun();
            }
            catch
            {
                run = null;
            }

            if (run?.CurrentStage == null)
            {
                return;
            }

            Station station = null;
            try
            {
                station = run.CurrentStage.CreateStation(node);
            }
            catch
            {
                station = null;
            }

            if (station == null)
            {
                return;
            }

            try
            {
                // Rewards are only defined on battle stations.
                (station as BattleStation)?.GenerateRewards();
            }
            catch
            {
                // ignored
            }

            session.PendingSettlementNodeKey = nodeKey;
            session.PendingSettlementStation = station;
            session.PendingBossExhibitShown = false;
            session.PendingRewardShown = false;
        }
        catch
        {
            // ignored
        }
    }

    private bool StepDriveSettlementUi_NoThrow(GameRunController run, CatchUpSession session)
    {
        try
        {
            if (run == null || session == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(session.PendingSettlementNodeKey) || session.PendingSettlementStation == null)
            {
                return false;
            }

            // Only joiner should do reward settlement.
            if (!IsJoinerConnected_NoThrow())
            {
                session.SettledNodeKeys.Add(session.PendingSettlementNodeKey);
                ClearPendingSettlement_NoThrow(session);
                return false;
            }

            // UI must be initialized.
            if (!UiManager.IsInitialized)
            {
                return true;
            }

            // If any settlement-related panel is currently open, wait until it closes.
            try
            {
                var rp = UiManager.GetPanel<RewardPanel>();
                if (rp != null && rp.gameObject != null && rp.gameObject.activeSelf)
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                var bp = UiManager.GetPanel<BossExhibitPanel>();
                if (bp != null && bp.gameObject != null && bp.gameObject.activeSelf)
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            // Boss nodes: show boss exhibit reward first (mirrors GameMaster.EndStationFlow ordering).
            if (!session.PendingBossExhibitShown)
            {
                if (session.PendingSettlementStation is BossStation bossStation)
                {
                    try
                    {
                        bossStation.GenerateBossRewards();
                        Exhibit[] bossRewards = bossStation.BossRewards;
                        if (bossRewards != null && bossRewards.Length > 0)
                        {
                            UiManager.GetPanel<BossExhibitPanel>()?.Show(bossRewards);
                            session.PendingBossExhibitShown = true;
                            return true;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                // Not a boss node, or no boss reward to show.
                session.PendingBossExhibitShown = true;
            }

            // Show the main RewardPanel for station rewards.
            if (!session.PendingRewardShown)
            {
                try
                {
                    UiManager.GetPanel<RewardPanel>()?.Show(new ShowRewardContent
                    {
                        RewardType = RewardType.Station,
                        Station = session.PendingSettlementStation,
                        ShowNextButton = true,
                    });

                    session.PendingRewardShown = true;
                    return true;
                }
                catch
                {
                    // If showing fails, don't block catch-up forever.
                    session.SettledNodeKeys.Add(session.PendingSettlementNodeKey);
                    ClearPendingSettlement_NoThrow(session);
                    return false;
                }
            }

            // If we reach here, panels are closed and we already showed them.
            session.SettledNodeKeys.Add(session.PendingSettlementNodeKey);
            ClearPendingSettlement_NoThrow(session);
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void ClearPendingSettlement_NoThrow(CatchUpSession session)
    {
        try
        {
            if (session == null)
            {
                return;
            }

            session.PendingSettlementNodeKey = string.Empty;
            session.PendingSettlementStation = null;
            session.PendingBossExhibitShown = false;
            session.PendingRewardShown = false;
        }
        catch
        {
            // ignored
        }
    }

    private static string BuildNodeKey(MapNode node)
    {
        try
        {
            if (node == null)
            {
                return string.Empty;
            }

            return $"{node.Act}:{node.X}:{node.Y}:{node.StationType}";
        }
        catch
        {
            return string.Empty;
        }
    }

    private bool IsJoinerConnected_NoThrow()
    {
        try
        {
            IServiceProvider sp = ModService.ServiceProvider;
            INetworkClient client = sp?.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return false;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            return !NetworkIdentityTracker.GetSelfIsHost();
        }
        catch
        {
            return false;
        }
    }

    private static void StepApplyNodeStates(GameMap map, CatchUpSession session, int nodeStatesBudget)
    {
        try
        {
            if (map == null || session == null)
            {
                return;
            }

            if (nodeStatesBudget <= 0)
            {
                return;
            }

            var list = session.NodeStatePairs;
            int total = list?.Count ?? 0;
            if (total <= 0)
            {
                session.NodeStateIndex = 0;
                return;
            }

            for (int i = 0; i < nodeStatesBudget && session.NodeStateIndex < total; i++)
            {
                KeyValuePair<string, string> kv;
                try
                {
                    kv = list[session.NodeStateIndex];
                }
                catch
                {
                    session.NodeStateIndex++;
                    continue;
                }

                session.NodeStateIndex++;

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
        catch
        {
            // ignored
        }
    }

    private void ClearPendingAfterApplied_NoThrow()
    {
        try
        {
            lock (_lock)
            {
                _pendingSnapshot = null;
                _pendingReceivedAtUtcTicks = 0;
                _pendingCheckpointId = string.Empty;
                _session = null;
            }
        }
        catch
        {
            // ignored
        }
    }

    private void MarkApplied()
    {
        lock (_lock)
        {
            _applied = true;
        }
    }

    private void TryRequestRoomStateAfterMapApplied_NoThrow(FullStateSnapshot snapshot)
    {
        try
        {
            if (snapshot?.MapState?.CurrentLocation == null)
            {
                return;
            }

            string checkpointId = snapshot.MapState.LastCheckpointId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(checkpointId))
            {
                // Still allow requests without a checkpoint, but avoid spamming.
                checkpointId = "<no-checkpoint>";
            }

            lock (_lock)
            {
                if (string.Equals(_lastRoomStateRequestedCheckpointId, checkpointId, StringComparison.Ordinal))
                {
                    return;
                }

                _lastRoomStateRequestedCheckpointId = checkpointId;
            }

            LocationSnapshot loc = snapshot.MapState.CurrentLocation;
            int act;
            int x;
            int y;
            string stationType;

            if (!TryParseNodeKey(loc.NodeId, out act, out x, out y, out stationType))
            {
                act = 0;
                x = loc.X;
                y = loc.Y;
                stationType = loc.NodeType ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(stationType))
            {
                stationType = "Unknown";
            }

            // Keep RoomSyncManager's local helpers aligned so battle patches can reuse the last-entered room.
            RoomSyncManager.SetLastEnteredNode(act, x, y, stationType);

            string roomKey = RoomSyncManager.BuildRoomKey(act, x, y, stationType);
            long knownVersion = RoomSyncManager.TryGetClientRoomState(roomKey)?.RoomVersion ?? 0;
            RoomSyncManager.RequestRoomState(roomKey, knownVersion);
        }
        catch
        {
            // ignored
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

        // 0) Path history: rebuild the path by calling GameMap.EnterNode(forced=true)
        // so internal side-effects (Visiting/Visited/Passed and _path bookkeeping) are consistent.
        // RoomStateSyncPatch ignores forced EnterNode, so this won't spam RoomStateRequest.
        TryApplyPathByEnterNode(map, mapState.PathHistory);

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

        // 2) 路径已在上面通过 EnterNode 重建；这里不再直接写 _path。

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

    private static void TryApplyPathByEnterNode(GameMap map, List<LocationSnapshot> pathHistory)
    {
        try
        {
            if (map == null)
            {
                return;
            }

            // Clear existing path to avoid appending duplicates.
            try
            {
                var list = Traverse.Create(map).Field("_path").GetValue<List<MapNode>>();
                list?.Clear();
            }
            catch
            {
                // ignored
            }

            // Reset visiting node so the first EnterNode won't mark an unrelated node as visited.
            try
            {
                Traverse.Create(map).Property("VisitingNode").SetValue(null);
            }
            catch
            {
                // ignored
            }

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
                if (node == null)
                {
                    continue;
                }

                // EnterNode requires Active/CrossActive when forced=false; we always force.
                // MapNode.Status setter is internal in the game assembly, so use reflection.
                try
                {
                    Traverse.Create(node).Property("Status").SetValue(MapNodeStatus.Active);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    map.EnterNode(node, freeMove: true, forced: true);
                }
                catch
                {
                    // ignored
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
