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
/// 断线重连管理器 - 处理玩家断线检测、状态保存、重连恢复。
/// 说明：当前项目以“主机权威”为核心，重连数据（快照/追赶）优先由主机生成并广播给重连玩家。
/// </summary>
public sealed class ReconnectionManager : IDisposable
{
    private readonly ReconnectionConfig _config;
    private readonly ILogger<ReconnectionManager>? _logger;
    private readonly ManualLogSource? _fallbackLogger;
    private readonly IServiceProvider _serviceProvider;

    private readonly object _syncLock = new();
    private INetworkClient? _client;
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

    private readonly Timer _heartbeatTimer;
    private readonly Timer _snapshotTimer;

    public bool IsConnected { get; private set; }
    public bool IsReconnecting { get; private set; }

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

        _heartbeatTimer = new Timer(CheckHeartbeats, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _snapshotTimer = new Timer(SavePeriodicSnapshot, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// 初始化重连管理器
    /// </summary>
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

            NetworkIdentityTracker.EnsureSubscribed(_client);

            _client.OnConnectionStateChanged += OnConnectionStateChanged;
            _client.OnGameEventReceived += OnGameEventReceived;

            IsConnected = _client.IsConnected;

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

    /// <summary>
    /// 保存完整状态快照（定期执行）
    /// </summary>
    private void SavePeriodicSnapshot(object? state)
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
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
            snapshot.GameState.GameStarted = GameStateUtils.GetCurrentGameRun() != null;

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
                foreach ((string playerId, DateTime last) in _lastHeartbeatUtc)
                {
                    if (_timedOutPlayers.Contains(playerId))
                    {
                        continue;
                    }

                    if ((now - last).TotalSeconds > _config.HeartbeatTimeoutSeconds)
                    {
                        timedOut ??= [];
                        timedOut.Add(playerId);
                        _timedOutPlayers.Add(playerId);
                    }
                }
            }

            if (timedOut == null)
            {
                return;
            }

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

    public void UpdateHeartbeat(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            _lastHeartbeatUtc[playerId] = DateTime.UtcNow;
            _timedOutPlayers.Remove(playerId);
        }
    }

    /// <summary>
    /// 检测到玩家断线（心跳超时或连接断开时调用）
    /// </summary>
    public void OnPlayerDisconnected(string playerId, DisconnectReason reason)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        LogWarning($"[ReconnectionManager] Player {playerId} disconnected, reason: {reason}");

        SavePlayerSnapshotBeforeDisconnect(playerId);
        PlayerDisconnected?.Invoke(playerId, reason);

        Task.Delay(TimeSpan.FromMinutes(_config.MaxReconnectionMinutes))
            .ContinueWith(_ => CheckReconnectionTimeout(playerId));
    }

    private void SavePlayerSnapshotBeforeDisconnect(string playerId)
    {
        try
        {
            PlayerStateSnapshot snapshot = SavePlayerStateSnapshot(playerId);
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
    private PlayerStateSnapshot SavePlayerStateSnapshot(string playerId)
    {
        try
        {
            INetworkManager? manager = _serviceProvider.GetService<INetworkManager>();
            INetworkPlayer? player = null;

            if (manager != null)
            {
                player = (manager.GetAllPlayers() ?? Enumerable.Empty<INetworkPlayer>())
                    .FirstOrDefault(p => p != null && string.Equals(p.userName, playerId, StringComparison.Ordinal));
                player ??= manager.GetSelf();
            }

            if (player != null)
            {
                PlayerStateSnapshot snapshot = CreateSnapshotFromNetworkPlayer(player);
                snapshot.PlayerId = playerId;
                return snapshot;
            }
        }
        catch
        {
            // ignored
        }

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

    public ReconnectionResult RequestReconnection(string playerId, string reconnectToken)
    {
        if (!IsReconnecting)
        {
            IsReconnecting = true;
        }

        try
        {
            PlayerStateSnapshot snapshot;
            lock (_syncLock)
            {
                if (!_playerSnapshots.TryGetValue(playerId, out snapshot!))
                {
                    return ReconnectionResult.Failed("No saved state for player");
                }
            }

            if (!string.IsNullOrWhiteSpace(snapshot.ReconnectToken) &&
                !string.Equals(snapshot.ReconnectToken, reconnectToken, StringComparison.Ordinal))
            {
                return ReconnectionResult.Failed("Invalid reconnect token");
            }

            DateTime disconnectAt = snapshot.DisconnectTime > 0
                ? new DateTime(snapshot.DisconnectTime, DateTimeKind.Utc)
                : snapshot.Timestamp.ToUniversalTime();

            if (DateTime.UtcNow - disconnectAt > TimeSpan.FromMinutes(_config.MaxReconnectionMinutes))
            {
                RemovePlayerSnapshot(playerId);
                return ReconnectionResult.Failed("Reconnection timeout");
            }

            LogInformation($"[ReconnectionManager] Reconnection approved for player {playerId}");

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

            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                LogDebug($"[ReconnectionManager] Skip sending reconnection snapshot: not host (target={playerId}).");
                return;
            }

            FullStateSnapshot fullSnapshot = CreateFullSnapshot();
            long lastKnownEventIndex = snapshot.DisconnectTime > 0 ? snapshot.DisconnectTime : 0;
            List<GameEvent> missedEvents = GetMissedEvents(lastKnownEventIndex);

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

    private void NotifyPlayerReconnected(string playerId)
    {
        try
        {
            INetworkClient? client = _client ?? _serviceProvider.GetService<INetworkClient>();
            if (client != null && client.IsConnected && NetworkIdentityTracker.GetSelfIsHost())
            {
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

        PlayerReconnected?.Invoke(playerId);
    }

    private void CheckReconnectionTimeout(string playerId)
    {
        bool hasSnapshot;
        lock (_syncLock)
        {
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

    private string GenerateReconnectToken()
    {
        byte[] bytes = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
    }

    private void RemovePlayerSnapshot(string playerId)
    {
        lock (_syncLock)
        {
            _playerSnapshots.Remove(playerId);
            _lastHeartbeatUtc.Remove(playerId);
            _timedOutPlayers.Remove(playerId);
        }
    }

    public void RecordGameEvent(GameEvent gameEvent)
    {
        if (gameEvent == null)
        {
            return;
        }

        lock (_syncLock)
        {
            _eventHistory[gameEvent.Timestamp] = gameEvent;

            if (_eventHistory.Count > _config.MaxHistoryEvents)
            {
                int removeCount = _eventHistory.Count - _config.MaxHistoryEvents;
                for (int i = 0; i < removeCount; i++)
                {
                    _eventHistory.RemoveAt(0);
                }
            }
        }
    }

    public List<GameEvent> GetMissedEvents(long lastKnownEventIndex)
    {
        lock (_syncLock)
        {
            return _eventHistory
                .Where(e => e.Key > lastKnownEventIndex)
                .Select(e => e.Value)
                .ToList();
        }
    }

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

    public event Action<string, DisconnectReason>? PlayerDisconnected;
    public event Action<string>? PlayerReconnected;
    public event Action<string>? ReconnectionTimeout;

    private void OnConnectionStateChanged(bool connected)
    {
        IsConnected = connected;
        if (connected)
        {
            IsReconnecting = false;
        }
    }

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

    private PlayerStateSnapshot CreateSnapshotFromNetworkPlayer(INetworkPlayer player)
    {
        int[] mana = player.mana ?? [0, 0, 0, 0];
        if (mana.Length < 4)
        {
            int[] fixedMana = [0, 0, 0, 0];
            for (int i = 0; i < mana.Length; i++)
            {
                fixedMana[i] = mana[i];
            }
            mana = fixedMana;
        }

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
}

/// <summary>
/// 断连原因
/// </summary>
public enum DisconnectReason
{
    Timeout,
    Manual,
    NetworkError,
    Kicked,
    ServerShutdown
}

/// <summary>
/// 重连配置
/// </summary>
public class ReconnectionConfig
{
    public int MaxReconnectionMinutes { get; set; } = 5;
    public int SnapshotIntervalSeconds { get; set; } = 30;
    public int HeartbeatTimeoutSeconds { get; set; } = 30;
    public int MaxHistoryEvents { get; set; } = 1000;
    public int MaxSavedFullSnapshots { get; set; } = 3;
    public int MaxSavedPlayerSnapshots { get; set; } = 64;
}

/// <summary>
/// 统计信息
/// </summary>
public class ReconnectionManagerStats
{
    public int ActiveSnapshots { get; set; }
    public int TotalEvents { get; set; }
    public bool IsConnected { get; set; }
    public bool IsReconnecting { get; set; }
    public int MaxReconnectionMinutes { get; set; }
    public int SnapshotIntervalSeconds { get; set; }
}

