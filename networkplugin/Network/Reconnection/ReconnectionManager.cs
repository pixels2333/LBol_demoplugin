using LBoL.Core;
using LBoL.Core.Stations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.Reconnection;

/// <summary>
/// 断线重连管理器：负责心跳超时判定、断线玩家状态快照保存、以及重连后的状态恢复/追赶。
/// </summary>
/// <remarks>
/// - 本模块以“主机权威”为核心：快照与追赶数据由主机生成并发送给目标玩家。
/// - 断线判定依赖外部周期调用 <see cref="UpdateHeartbeat"/> 上报玩家心跳。
/// </remarks>
public sealed class ReconnectionManager : IDisposable
{
    #region 私有字段

    /// <summary>
    /// 重连行为配置。
    /// </summary>
    private readonly ReconnectionConfig _config;

    /// <summary>
    /// 结构化日志（可能为空）；为空时使用 <see cref="_fallbackLogger"/> 输出。
    /// </summary>
    private readonly ILogger<ReconnectionManager>? _logger;

    /// <summary>
    /// 备选日志输出（用于不依赖 DI 的场景）。
    /// </summary>
    private readonly ManualLogSource? _fallbackLogger;

    /// <summary>
    /// 服务提供器：用于按需解析网络组件，降低强依赖/初始化顺序要求。
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 同步锁：保护快照/心跳/事件历史等共享状态（定时器与网络回调可能并发）。
    /// </summary>
    private readonly object _syncLock = new();

    /// <summary>
    /// 网络客户端（初始化后缓存；某些场景会按需从 DI 再获取一次以防对象变更）。
    /// </summary>
    private INetworkClient? _client;

    /// <summary>
    /// 初始化标记（0=未初始化，1=已初始化），用于保证 <see cref="Initialize"/> 幂等。
    /// </summary>
    private int _initialized;

    /// <summary>
    /// 玩家状态快照字典 - 用于断线后恢复
    /// Key: PlayerId（服务端分配）
    /// </summary>
    private readonly Dictionary<string, PlayerStateSnapshot> _playerSnapshots = new(StringComparer.Ordinal);

    /// <summary>
    /// 最近游戏事件历史（用于断线追赶）
    /// Key: 时间戳(ticks)
    /// </summary>
    private readonly SortedList<long, GameEvent> _eventHistory = new();

    /// <summary>
    /// 完整状态快照历史（限制条数，避免内存增长）
    /// </summary>
    private readonly LinkedList<FullStateSnapshot> _fullSnapshots = new();

    private readonly Dictionary<string, DateTime> _lastHeartbeatUtc = new(StringComparer.Ordinal);
    private readonly HashSet<string> _timedOutPlayers = new(StringComparer.Ordinal);

    /// <summary>
    /// 心跳检测定时器：扫描超时玩家并触发断线回调。
    /// </summary>
    private readonly Timer _heartbeatTimer;

    /// <summary>
    /// 周期快照定时器：定期构建完整状态快照（主要用于主机侧）。
    /// </summary>
    private readonly Timer _snapshotTimer;

    /// <summary>
    /// 地图关键提交点序号（仅主机递增）。
    /// </summary>
    private long _mapCheckpointSequence;

    /// <summary>
    /// 最近一次地图关键提交点 ID（仅主机更新）。
    /// </summary>
    private string _lastMapCheckpointId = string.Empty;

    /// <summary>
    /// 最近一次地图关键提交点时间（UTC ticks，仅主机更新）。
    /// </summary>
    private long _lastMapCheckpointAtUtcTicks;

    #endregion

    #region 公共属性

    /// <summary>
    /// 当前连接状态（跟随 <see cref="INetworkClient"/> 的连接状态变化）。
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// 当前是否处于处理重连的阶段（可用于 UI/状态机提示）。
    /// </summary>
    public bool IsReconnecting { get; private set; }

    #endregion

    #region 构造与初始化

    /// <summary>
    /// 创建一个断线重连管理器。
    /// </summary>
    /// <param name="config">重连配置；为 null 时使用默认配置。</param>
    /// <param name="logger">可选日志（DI 注入）。</param>
    /// <param name="serviceProvider">服务提供器，用于按需解析网络组件。</param>
    /// <param name="fallbackLogger">备选日志源；默认使用 <see cref="Plugin.Logger"/>。</param>
    public ReconnectionManager(
        ReconnectionConfig config,
        ILogger<ReconnectionManager>? logger,
        IServiceProvider serviceProvider,
        ManualLogSource? fallbackLogger = null)
    {
        _config = config ?? new ReconnectionConfig();
        _logger = logger;
        _serviceProvider = serviceProvider;
        _fallbackLogger = fallbackLogger ?? Plugin.Logger;

        // 这里使用 InfiniteTimeSpan，确保在 Initialize 后才开始周期任务（避免依赖未就绪）。
        _heartbeatTimer = new Timer(CheckHeartbeats, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _snapshotTimer = new Timer(SavePeriodicSnapshot, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// 初始化重连管理器（幂等）。
    /// </summary>
    /// <remarks>
    /// - 多次调用不会重复订阅事件或重复启动定时器。
    /// - 依赖 <see cref="INetworkClient"/>，若无法解析则会跳过初始化。
    /// </remarks>
    public void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return;
        }

        try
        {
            _client = _serviceProvider.GetService<INetworkClient>();
            if (_client == null)
            {
                LogWarning("[ReconnectionManager] Initialize skipped: INetworkClient not available.");
                return;
            }

            // 订阅网络身份跟踪（主机判定/玩家 ID 维护等）。
            NetworkIdentityTracker.EnsureSubscribed(_client);

            _client.OnConnectionStateChanged += OnConnectionStateChanged;
            _client.OnGameEventReceived += OnGameEventReceived;

            IsConnected = _client.IsConnected;

            // 心跳与快照以固定频率执行；具体阈值/间隔由配置控制。
            _heartbeatTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            _snapshotTimer.Change(
                TimeSpan.FromSeconds(_config.SnapshotIntervalSeconds),
                TimeSpan.FromSeconds(_config.SnapshotIntervalSeconds));

            LogInformation("[ReconnectionManager] Initialized");
        }
        catch (Exception ex)
        {
            LogError($"[ReconnectionManager] Initialize failed: {ex.Message}");
        }
    }

    #endregion

    #region 周期性快照（主机侧）

    /// <summary>
    /// 保存完整状态快照（定期执行）
    /// </summary>
    /// <param name="state">定时器状态对象（未使用）。</param>
    private void SavePeriodicSnapshot(object? state)
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            // 定时器回调中尽量不假设 _client 一定已就绪：必要时从 DI 再解析一次。
            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            FullStateSnapshot snapshot = CreateFullSnapshot();
            SaveSnapshot(snapshot);
        }
        catch (Exception ex)
        {
            LogError($"[ReconnectionManager] Error saving periodic snapshot: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建完整游戏状态快照（尽量从现有可用数据源提取）
    /// </summary>
    /// <returns>完整状态快照（失败时会返回降级但可用的快照）。</returns>
    public FullStateSnapshot CreateFullSnapshot()
    {
        var snapshot = new FullStateSnapshot
        {
            Timestamp = DateTime.UtcNow.Ticks,
            GameState = new GameStateSnapshot(),
            PlayerStates = new List<PlayerStateSnapshot>(),
            BattleState = new BattleStateSnapshot(),
            MapState = new MapStateSnapshot(),
            EventIndex = 0
        };

        try
        {
            lock (_syncLock)
            {
                if (_eventHistory.Count > 0)
                {
                    snapshot.EventIndex = _eventHistory.Keys[_eventHistory.Count - 1];
                }

                // 关键提交点只读：避免周期快照影响 checkpoint 语义。
                snapshot.MapState.LastCheckpointId = _lastMapCheckpointId;
                snapshot.MapState.LastCheckpointAtUtcTicks = _lastMapCheckpointAtUtcTicks;
            }

            INetworkManager? manager = _serviceProvider.GetService<INetworkManager>();
            if (manager != null)
            {
                foreach (INetworkPlayer p in manager.GetAllPlayers() ?? Enumerable.Empty<INetworkPlayer>())
                {
                    if (p == null)
                    {
                        continue;
                    }

                    snapshot.PlayerStates.Add(CreateSnapshotFromNetworkPlayer(p));
                }
            }

            // 轻量补充：标记游戏是否已开始（可用于客户端判断是否需要 UI/流程恢复）
            GameRunController? run = GameStateUtils.GetCurrentGameRun();
            snapshot.GameState.GameStarted = run != null;

            if (run != null)
            {
                try
                {
                    snapshot.GameState.RootSeed = run.RootSeed;
                    snapshot.GameState.UISeed = run.UISeed;
                    snapshot.GameState.StageIndex = run.CurrentStage?.Index;
                    snapshot.MapState.MapSeedUlong = run.CurrentStage?.MapSeed;
                }
                catch
                {
                    // ignored
                }

                TryFillMapStateFromRun(run, snapshot.MapState);
            }

            // 地图位置：尽量从自身玩家快照中提取
            PlayerStateSnapshot? firstPlayer = snapshot.PlayerStates.FirstOrDefault();
            if (firstPlayer?.GameLocation != null)
            {
                snapshot.MapState.CurrentLocation = new LocationSnapshot
                {
                    X = firstPlayer.GameLocation.X,
                    Y = firstPlayer.GameLocation.Y
                };
            }
        }
        catch (Exception ex)
        {
            LogWarning($"[ReconnectionManager] Full snapshot build degraded: {ex.Message}");
        }

        LogDebug($"[ReconnectionManager] Full snapshot created at {snapshot.Timestamp}");
        return snapshot;
    }

    /// <summary>
    /// 标记地图关键提交点（仅主机更新）。
    /// </summary>
    /// <param name="reason">提交点原因（用于日志诊断）。</param>
    /// <param name="nodeKey">可选：相关节点 key（Act:X:Y:StationType）。</param>
    public void MarkMapCheckpoint(string reason, string? nodeKey = null)
    {
        try
        {
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            long now = DateTime.UtcNow.Ticks;
            long seq = Interlocked.Increment(ref _mapCheckpointSequence);
            string safeReason = string.IsNullOrWhiteSpace(reason) ? "checkpoint" : reason;
            string id = $"cp{seq:00000000}:{safeReason}";

            lock (_syncLock)
            {
                _lastMapCheckpointId = id;
                _lastMapCheckpointAtUtcTicks = now;
            }

            LogDebug($"[ReconnectionManager] Map checkpoint: id={id}, at={now}, reason={safeReason}, nodeKey={nodeKey ?? "<null>"}");
        }
        catch (Exception ex)
        {
            LogWarning($"[ReconnectionManager] MarkMapCheckpoint degraded: {ex.Message}");
        }
    }

    private static string BuildNodeKey(MapNode node)
    {
        if (node == null)
        {
            return string.Empty;
        }

        string stationType = node.StationType.ToString();
        return $"{node.Act}:{node.X}:{node.Y}:{stationType}";
    }

    private static LocationSnapshot BuildLocationSnapshot(MapNode node)
    {
        if (node == null)
        {
            return new LocationSnapshot();
        }

        return new LocationSnapshot
        {
            X = node.X,
            Y = node.Y,
            NodeId = BuildNodeKey(node),
            NodeType = node.StationType.ToString(),
            IsBranch = node.FollowerList != null && node.FollowerList.Count > 1,
            VisitTime = 0,
        };
    }

    private void TryFillMapStateFromRun(GameRunController run, MapStateSnapshot mapState)
    {
        try
        {
            if (run == null || mapState == null)
            {
                return;
            }

            GameMap map = run.CurrentMap;
            if (map == null)
            {
                return;
            }

            // Current location: prefer the game map visiting node (more reliable than player X/Y).
            if (map.VisitingNode != null)
            {
                mapState.CurrentLocation = BuildLocationSnapshot(map.VisitingNode);
            }

            // Path history: preserves execution order for catch-up.
            try
            {
                IReadOnlyList<MapNode> path = map.Path;
                mapState.PathHistory = path
                    .Where(n => n != null)
                    .Select(BuildLocationSnapshot)
                    .ToList();
            }
            catch
            {
                // ignored
            }

            // Node states: host extracts the whole map node status table.
            try
            {
                Dictionary<string, string> nodeStates = new(StringComparer.Ordinal);
                foreach (MapNode node in map.AllNodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    string key = BuildNodeKey(node);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    nodeStates[key] = node.Status.ToString();
                }

                mapState.NodeStates = nodeStates;
            }
            catch
            {
                // ignored
            }

            // Back-compat: keep VisitedNodes aligned with path.
            try
            {
                mapState.VisitedNodes = mapState.PathHistory
                    .Select(p => p?.NodeId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
            catch
            {
                // ignored
            }

            // Cleared nodes: all nodes in the path except the current visiting node unless current station is finished.
            try
            {
                HashSet<string> cleared = new(StringComparer.Ordinal);

                IReadOnlyList<MapNode> path = map.Path;
                int cut = Math.Max(0, path.Count - 1);
                for (int i = 0; i < cut; i++)
                {
                    string key = BuildNodeKey(path[i]);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        cleared.Add(key);
                    }
                }

                if (run.CurrentStation != null && run.CurrentStation.Status == StationStatus.Finished && map.VisitingNode != null)
                {
                    string cur = BuildNodeKey(map.VisitingNode);
                    if (!string.IsNullOrWhiteSpace(cur))
                    {
                        cleared.Add(cur);
                    }
                }

                mapState.ClearedNodes = cleared.ToList();
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

    /// <summary>
    /// 保存快照到内存（限制数量）
    /// </summary>
    private void SaveSnapshot(FullStateSnapshot snapshot)
    {
        lock (_syncLock)
        {
            _fullSnapshots.AddLast(snapshot);
            while (_fullSnapshots.Count > _config.MaxSavedFullSnapshots && _fullSnapshots.First != null)
            {
                _fullSnapshots.RemoveFirst();
            }
        }
    }

    #endregion

    #region 心跳与断线检测

    /// <summary>
    /// 心跳检测（依赖外部调用 <see cref="UpdateHeartbeat"/> 上报）
    /// </summary>
    private void CheckHeartbeats(object? state)
    {
        try
        {
            DateTime now = DateTime.UtcNow;
            List<string>? timedOut = null;

            lock (_syncLock)
            {
                // 扫描所有已上报过心跳的玩家：如果距离上次心跳超过阈值，则判定为超时。
                foreach ((string playerId, DateTime last) in _lastHeartbeatUtc)
                {
                    // 已经判定过超时的玩家，不重复触发断线逻辑（避免重复保存快照/重复事件）。
                    if (_timedOutPlayers.Contains(playerId))
                    {
                        continue;
                    }

                    // 注意：这里用 TotalSeconds 做阈值判定，便于配置以秒为单位进行调参。
                    if ((now - last).TotalSeconds > _config.HeartbeatTimeoutSeconds)
                    {
                        timedOut ??= [];
                        timedOut.Add(playerId);
                        _timedOutPlayers.Add(playerId);
                    }
                }
            }

            // 事件回调放在锁外触发，避免外部订阅者反向调用造成死锁。
            if (timedOut == null)
            {
                return;
            }

            // 锁外逐个触发：断线处理可能会保存快照/触发事件/启动延迟任务。
            foreach (string playerId in timedOut)
            {
                OnPlayerDisconnected(playerId, DisconnectReason.Timeout);
            }
        }
        catch (Exception ex)
        {
            LogDebug($"[ReconnectionManager] Heartbeat check skipped: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新玩家心跳时间戳（通常由主机在收到玩家心跳/行为包时调用）。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    public void UpdateHeartbeat(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            // 记录“最后一次心跳到达时间”。如果玩家之前被标记为超时，这里会解除超时标记。
            _lastHeartbeatUtc[playerId] = DateTime.UtcNow;
            _timedOutPlayers.Remove(playerId);
        }
    }

    /// <summary>
    /// 检测到玩家断线（心跳超时或连接断开时调用）
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    /// <param name="reason">断连原因。</param>
    /// <remarks>
    /// 断线后会保存快照并触发 <see cref="PlayerDisconnected"/> 事件，同时启动超时计时。
    /// </remarks>
    public void OnPlayerDisconnected(string playerId, DisconnectReason reason)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        LogWarning($"[ReconnectionManager] Player {playerId} disconnected, reason: {reason}");

        SavePlayerSnapshotBeforeDisconnect(playerId);
        PlayerDisconnected?.Invoke(playerId, reason);

        // Fire-and-forget：到达最大重连窗口后再次检查是否已重连（避免永久保留快照）。
        Task.Delay(TimeSpan.FromMinutes(_config.MaxReconnectionMinutes))
            .ContinueWith(_ => CheckReconnectionTimeout(playerId));
    }

    /// <summary>
    /// 在玩家断线前保存一次玩家快照，并写入断线时间/token 等重连辅助字段。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    private void SavePlayerSnapshotBeforeDisconnect(string playerId)
    {
        try
        {
            PlayerStateSnapshot snapshot = SavePlayerStateSnapshot(playerId);

            // 没有 token 时生成一次，确保后续重连请求可以进行最基本的校验。
            snapshot.ReconnectToken = string.IsNullOrWhiteSpace(snapshot.ReconnectToken)
                ? GenerateReconnectToken()
                : snapshot.ReconnectToken;
            snapshot.DisconnectTime = DateTime.UtcNow.Ticks;
            snapshot.LastUpdateTime = snapshot.DisconnectTime;

            lock (_syncLock)
            {
                _playerSnapshots[playerId] = snapshot;

                if (_playerSnapshots.Count > _config.MaxSavedPlayerSnapshots)
                {
                    // 控制内存占用：移除最早断线的玩家快照。
                    string? oldestKey = null;
                    long oldestDisconnectTime = long.MaxValue;

                    foreach ((string key, PlayerStateSnapshot s) in _playerSnapshots)
                    {
                        long dt = s.DisconnectTime;
                        if (dt > 0 && dt < oldestDisconnectTime)
                        {
                            oldestDisconnectTime = dt;
                            oldestKey = key;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(oldestKey))
                    {
                        _playerSnapshots.Remove(oldestKey);
                    }
                }
            }

            LogInformation($"[ReconnectionManager] Saved snapshot for player {playerId} before disconnect");
        }
        catch (Exception ex)
        {
            LogError($"[ReconnectionManager] Error saving player {playerId} snapshot: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建玩家状态快照：优先从 INetworkManager 的玩家对象抽取（兼容本地/远端）。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    /// <returns>玩家状态快照（无法获取玩家对象时会返回占位快照）。</returns>
    private PlayerStateSnapshot SavePlayerStateSnapshot(string playerId)
    {
        try
        {
            INetworkManager? manager = _serviceProvider.GetService<INetworkManager>();
            INetworkPlayer? player = null;

            if (manager != null)
            {
                // 优先按 playerId 在当前玩家列表中查找（这里用 userName 作为匹配字段）。
                // 如果未来 playerId 与 userName 分离，这里需要改成更稳定的唯一键。
                player = (manager.GetAllPlayers() ?? Enumerable.Empty<INetworkPlayer>())
                    .FirstOrDefault(p => p != null && string.Equals(p.userName, playerId, StringComparison.Ordinal));

                // 找不到目标时退化为自身玩家：至少能带回“当前局部可获取”的状态信息。
                player ??= manager.GetSelf();
            }

            if (player != null)
            {
                // 从玩家对象抽取快照（血量、法力、金币、位置等）。
                PlayerStateSnapshot snapshot = CreateSnapshotFromNetworkPlayer(player);
                snapshot.PlayerId = playerId;
                return snapshot;
            }
        }
        catch
        {
            // ignored
        }

        // 最终兜底：返回一个“占位快照”，保证重连流程不会因为缺失数据直接崩溃。
        return new PlayerStateSnapshot
        {
            PlayerId = playerId,
            UserName = playerId,
            Timestamp = DateTime.UtcNow,
            Health = 0,
            MaxHealth = 0,
            Block = 0,
            Shield = 0,
            ManaGroup = [0, 0, 0, 0],
            Gold = 0,
            Cards = [],
            Exhibits = [],
            Potions = [],
            StatusEffects = [],
            GameLocation = new LocationSnapshot { X = -1, Y = -1 },
            IsInBattle = false,
        };
    }

    #endregion

    #region 重连流程

    /// <summary>
    /// 处理玩家重连请求：校验 token 与重连窗口，成功则发送快照与追赶事件。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    /// <param name="reconnectToken">重连令牌（断线时生成并保存于快照中）。</param>
    /// <returns>重连结果（成功时包含玩家快照）。</returns>
    public ReconnectionResult RequestReconnection(string playerId, string reconnectToken)
    {
        // 进入重连流程标记：用于外部 UI/逻辑感知“当前正在处理重连”。
        if (!IsReconnecting)
        {
            IsReconnecting = true;
        }

        try
        {
            PlayerStateSnapshot snapshot;
            lock (_syncLock)
            {
                // 读取玩家断线时保存的快照。没有快照代表无法恢复（或已被清理）。
                if (!_playerSnapshots.TryGetValue(playerId, out snapshot!))
                {
                    return ReconnectionResult.Failed("No saved state for player");
                }
            }

            // token 不一致直接拒绝（避免误恢复/被伪造请求复用状态）。
            if (!string.IsNullOrWhiteSpace(snapshot.ReconnectToken) &&
                !string.Equals(snapshot.ReconnectToken, reconnectToken, StringComparison.Ordinal))
            {
                return ReconnectionResult.Failed("Invalid reconnect token");
            }

            // 超过重连窗口：清理快照并拒绝。
            DateTime disconnectAt = snapshot.DisconnectTime > 0
                ? new DateTime(snapshot.DisconnectTime, DateTimeKind.Utc)
                : snapshot.Timestamp.ToUniversalTime();

            if (DateTime.UtcNow - disconnectAt > TimeSpan.FromMinutes(_config.MaxReconnectionMinutes))
            {
                RemovePlayerSnapshot(playerId);
                return ReconnectionResult.Failed("Reconnection timeout");
            }

            LogInformation($"[ReconnectionManager] Reconnection approved for player {playerId}");

            // 注意：恢复包发送、事件触发顺序保持“先发恢复数据，再通知已重连”，
            // 避免订阅者收到重连事件时客户端尚未拿到必要数据。
            SendReconnectionSnapshot(playerId, snapshot);
            NotifyPlayerReconnected(playerId);

            return ReconnectionResult.Success(snapshot);
        }
        catch (Exception ex)
        {
            LogError($"[ReconnectionManager] Error in reconnection request: {ex.Message}");
            return ReconnectionResult.Failed($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 向目标玩家发送“重连恢复包”（完整快照 + 玩家快照 + 漏掉的事件）。
    /// </summary>
    /// <param name="playerId">目标玩家 ID。</param>
    /// <param name="snapshot">断线前保存的玩家快照。</param>
    private void SendReconnectionSnapshot(string playerId, PlayerStateSnapshot snapshot)
    {
        try
        {
            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client == null || !client.IsConnected)
            {
                LogWarning($"[ReconnectionManager] Skip sending reconnection snapshot: client not connected (target={playerId}).");
                return;
            }

            // 主机权威：只有主机才有资格生成并发送完整恢复数据（避免多个客户端各自生成导致不一致）。
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                LogDebug($"[ReconnectionManager] Skip sending reconnection snapshot: not host (target={playerId}).");
                return;
            }

            // 主机生成“完整快照 + 断线前玩家快照 + 漏掉的事件”组成恢复包。
            FullStateSnapshot fullSnapshot = CreateFullSnapshot();

            // 这里使用 DisconnectTime 作为“最后已知索引”的近似值（ticks）。
            // 若后续引入明确的 EventIndex，可在此处改为更准确的字段。
            long lastKnownEventIndex = snapshot.DisconnectTime > 0 ? snapshot.DisconnectTime : 0;
            List<GameEvent> missedEvents = GetMissedEvents(lastKnownEventIndex);

            // 通过网络事件发送恢复包：目标玩家据此进行状态恢复与事件追赶。
            client.SendGameEventData(NetworkMessageTypes.FullStateSyncResponse, new
            {
                TargetPlayerId = playerId,
                PlayerSnapshot = snapshot,
                FullSnapshot = fullSnapshot,
                MissedEvents = missedEvents,
                ServerTime = DateTime.UtcNow.Ticks
            });

            LogInformation($"[ReconnectionManager] Sent reconnection snapshot to player {playerId}");
        }
        catch (Exception ex)
        {
            LogError($"[ReconnectionManager] Failed to send reconnection snapshot to player {playerId}: {ex.Message}");
        }
    }

    /// <summary>
    /// 通知其他模块“玩家已重连”（本地事件 + 可选的网络广播）。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    private void NotifyPlayerReconnected(string playerId)
    {
        try
        {
            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client != null && client.IsConnected && NetworkIdentityTracker.GetSelfIsHost())
            {
                // 广播重连尝试事件，便于其他模块（UI/逻辑）感知与响应。
                client.SendGameEventData(NetworkMessageTypes.OnReconnectionAttempt, new
                {
                    PlayerId = playerId,
                    Timestamp = DateTime.UtcNow.Ticks
                });
            }
        }
        catch
        {
            // ignored
        }

        // 本地事件：通知插件内的其他模块（比如 UI、战斗/地图恢复逻辑）。
        PlayerReconnected?.Invoke(playerId);
    }

    /// <summary>
    /// 重连窗口结束后检查玩家是否仍未重连：若仍存在快照则判定超时并清理。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    private void CheckReconnectionTimeout(string playerId)
    {
        bool hasSnapshot;
        lock (_syncLock)
        {
            // 延迟任务到点后检查：如果快照仍存在，说明在窗口期内没有成功重连。
            hasSnapshot = _playerSnapshots.ContainsKey(playerId);
        }

        if (!hasSnapshot)
        {
            return;
        }

        RemovePlayerSnapshot(playerId);
        ReconnectionTimeout?.Invoke(playerId);
        LogInformation($"[ReconnectionManager] Reconnection timeout for player {playerId}");
    }

    /// <summary>
    /// 生成重连令牌（随机 32 字节，转为 64 位十六进制字符串）。
    /// </summary>
    private string GenerateReconnectToken()
    {
        byte[] bytes = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// 移除玩家相关的断线快照与心跳状态。
    /// </summary>
    /// <param name="playerId">玩家唯一标识。</param>
    private void RemovePlayerSnapshot(string playerId)
    {
        lock (_syncLock)
        {
            _playerSnapshots.Remove(playerId);
            _lastHeartbeatUtc.Remove(playerId);
            _timedOutPlayers.Remove(playerId);
        }
    }

    #endregion

    #region 事件追赶（主机侧）

    /// <summary>
    /// 记录游戏事件到追赶历史（通常由主机在收到关键事件后调用）。
    /// </summary>
    /// <param name="gameEvent">事件对象。</param>
    public void RecordGameEvent(GameEvent gameEvent)
    {
        if (gameEvent == null)
        {
            return;
        }

        lock (_syncLock)
        {
            // 以 Timestamp 作为有序键：同一 tick 的事件会覆盖（一般不会发生；发生时视为“最后写入为准”）。
            _eventHistory[gameEvent.Timestamp] = gameEvent;

            if (_eventHistory.Count > _config.MaxHistoryEvents)
            {
                // 只保留最近 N 条事件，避免断线追赶历史无限增长。
                int removeCount = _eventHistory.Count - _config.MaxHistoryEvents;
                for (int i = 0; i < removeCount; i++)
                {
                    _eventHistory.RemoveAt(0);
                }
            }
        }
    }

    /// <summary>
    /// 获取指定索引之后漏掉的事件（用于断线追赶）。
    /// </summary>
    /// <param name="lastKnownEventIndex">玩家最后已知事件索引（当前实现使用 ticks 近似）。</param>
    /// <returns>漏掉的事件列表（按时间顺序）。</returns>
    public List<GameEvent> GetMissedEvents(long lastKnownEventIndex)
    {
        lock (_syncLock)
        {
            // SortedList 按 key 升序；这里筛选后 ToList 保持时间顺序，便于客户端回放。
            return _eventHistory
                .Where(e => e.Key > lastKnownEventIndex)
                .Select(e => e.Value)
                .ToList();
        }
    }

    #endregion

    #region 统计与资源释放

    /// <summary>
    /// 获取当前重连管理器的运行统计信息（线程安全）。
    /// </summary>
    public ReconnectionManagerStats GetStats()
    {
        lock (_syncLock)
        {
            return new ReconnectionManagerStats
            {
                ActiveSnapshots = _playerSnapshots.Count,
                TotalEvents = _eventHistory.Count,
                IsConnected = IsConnected,
                IsReconnecting = IsReconnecting,
                MaxReconnectionMinutes = _config.MaxReconnectionMinutes,
                SnapshotIntervalSeconds = _config.SnapshotIntervalSeconds
            };
        }
    }

    /// <summary>
    /// 释放资源：取消事件订阅并停止定时器。
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (_client != null)
            {
                _client.OnConnectionStateChanged -= OnConnectionStateChanged;
                _client.OnGameEventReceived -= OnGameEventReceived;
            }
        }
        catch
        {
            // ignored
        }

        _heartbeatTimer.Dispose();
        _snapshotTimer.Dispose();
    }

    #endregion

    #region 事件与回调

    /// <summary>
    /// 玩家断线事件（触发时机：心跳超时或连接断开判定）。
    /// </summary>
    public event Action<string, DisconnectReason>? PlayerDisconnected;

    /// <summary>
    /// 玩家重连成功事件（触发时机：重连请求校验通过并发送恢复包后）。
    /// </summary>
    public event Action<string>? PlayerReconnected;

    /// <summary>
    /// 玩家重连超时事件（触发时机：超过重连窗口仍未重连）。
    /// </summary>
    public event Action<string>? ReconnectionTimeout;

    /// <summary>
    /// 网络连接状态变化回调（来自 <see cref="INetworkClient"/>）。
    /// </summary>
    /// <param name="connected">是否已连接。</param>
    private void OnConnectionStateChanged(bool connected)
    {
        IsConnected = connected;
        if (connected)
        {
            IsReconnecting = false;
        }
    }

    /// <summary>
    /// 网络游戏事件回调：主机侧用于维护追赶历史。
    /// </summary>
    /// <param name="eventType">事件类型。</param>
    /// <param name="payload">事件负载。</param>
    private void OnGameEventReceived(string eventType, object payload)
    {
        try
        {
            // 仅主机维护事件历史（用于断线追赶）。
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            // 避免把 FullSync 控制消息写入追赶历史（降低循环风险与负载放大）
            if (string.Equals(eventType, NetworkMessageTypes.FullStateSyncRequest, StringComparison.Ordinal) ||
                string.Equals(eventType, NetworkMessageTypes.FullStateSyncResponse, StringComparison.Ordinal))
            {
                return;
            }

            // 这里的 "unknown" 表示未提供明确的 SourceId；如果后续能拿到发送者，可替换为真实玩家ID。
            RecordGameEvent(new GameEvent(eventType, "unknown", payload)
            {
                Timestamp = DateTime.UtcNow.Ticks
            });
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region 快照构建工具

    /// <summary>
    /// 从网络玩家对象提取可恢复的核心状态，构建 <see cref="PlayerStateSnapshot"/>。
    /// </summary>
    /// <param name="player">网络玩家对象。</param>
    private PlayerStateSnapshot CreateSnapshotFromNetworkPlayer(INetworkPlayer player)
    {
        // mana 不在 INetworkPlayer 接口中声明：使用兼容层从实现类读取并规范化为 4 位数组。
        int[] mana = player.GetManaArraySafe();

        return new PlayerStateSnapshot
        {
            PlayerId = player.userName ?? string.Empty,
            UserName = player.userName ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            Health = player.HP,
            MaxHealth = player.maxHP,
            Block = player.block,
            Shield = player.shield,
            ManaGroup = mana,
            Gold = player.coins,
            Cards = [],
            Exhibits = [],
            Potions = [],
            StatusEffects = [],
            GameLocation = new LocationSnapshot { X = player.location_X, Y = player.location_Y },
            IsInBattle = false,
            CharacterType = player.chara ?? string.Empty,
            IsPlayersTurn = !player.endturn
        };
    }

    #endregion

    #region 日志

    private void LogInformation(string message)
    {
        _logger?.LogInformation(message);
        _fallbackLogger?.LogInfo(message);
    }

    private void LogWarning(string message)
    {
        _logger?.LogWarning(message);
        _fallbackLogger?.LogWarning(message);
    }

    private void LogError(string message)
    {
        _logger?.LogError(message);
        _fallbackLogger?.LogError(message);
    }

    private void LogDebug(string message)
    {
        _logger?.LogDebug(message);
        _fallbackLogger?.LogDebug(message);
    }

    #endregion
}

#region 相关数据结构

/// <summary>
/// 断连原因
/// </summary>
public enum DisconnectReason
{
    /// <summary>
    /// 心跳超时。
    /// </summary>
    Timeout,

    /// <summary>
    /// 主动断开/主动退出。
    /// </summary>
    Manual,

    /// <summary>
    /// 网络错误导致断开。
    /// </summary>
    NetworkError,

    /// <summary>
    /// 被踢出房间/服务器。
    /// </summary>
    Kicked,

    /// <summary>
    /// 服务器关闭导致断开。
    /// </summary>
    ServerShutdown
}

/// <summary>
/// 重连配置
/// </summary>
public class ReconnectionConfig
{
    /// <summary>
    /// 最大允许重连窗口（分钟）。超过此时间仍未重连则判定超时并清理快照。
    /// </summary>
    public int MaxReconnectionMinutes { get; set; } = 5;

    /// <summary>
    /// 完整快照保存间隔（秒）。
    /// </summary>
    public int SnapshotIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 心跳超时阈值（秒）。超过此阈值视为断线。
    /// </summary>
    public int HeartbeatTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 最大事件追赶历史条数（超过后会移除最旧的事件）。
    /// </summary>
    public int MaxHistoryEvents { get; set; } = 1000;

    /// <summary>
    /// 内存中保留的完整快照数量上限。
    /// </summary>
    public int MaxSavedFullSnapshots { get; set; } = 3;

    /// <summary>
    /// 内存中保留的玩家断线快照数量上限。
    /// </summary>
    public int MaxSavedPlayerSnapshots { get; set; } = 64;
}

/// <summary>
/// 统计信息
/// </summary>
public class ReconnectionManagerStats
{
    /// <summary>
    /// 当前保存的玩家快照数量。
    /// </summary>
    public int ActiveSnapshots { get; set; }

    /// <summary>
    /// 当前追赶历史中的事件数量。
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// 当前连接状态。
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 当前是否处于重连流程中。
    /// </summary>
    public bool IsReconnecting { get; set; }

    /// <summary>
    /// 最大允许重连窗口（分钟）。
    /// </summary>
    public int MaxReconnectionMinutes { get; set; }

    /// <summary>
    /// 完整快照保存间隔（秒）。
    /// </summary>
    public int SnapshotIntervalSeconds { get; set; }
}

#endregion

