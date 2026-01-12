using System;
using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.Client;

/// <summary>
/// 网络管理器实现类
/// 负责管理连接到游戏的网络玩家，包括玩家的注册、移除和查询
/// 这是INetworkManager接口的具体实现，目前处于开发阶段
/// </summary>
/// <param name="networkClient">网络客户端实例，用于处理网络通信</param>
/// <remarks>
/// 这个类是客户端侧的轻量玩家管理实现：
/// - 玩家列表/身份主要由服务器 GameEvent 驱动（见 <see cref="NetworkIdentityTracker"/>）。
/// - 远端玩家的“真实同步”由各个 *SyncPatch 负责，本类只维护可查询的玩家集合。
/// </remarks>
public class NetworkManager : INetworkManager
{
    #region 私有字段

    /// <summary>
    /// 网络客户端实例，用于与服务器通信
    /// </summary>
    private readonly INetworkClient _networkClient;

    private readonly object _playersLock = new();

    /// <summary>
    /// 玩家列表，存储所有已连接的玩家信息
    /// 键为玩家ID，值为网络玩家实例
    /// </summary>
    private readonly Dictionary<string, INetworkPlayer> _players = [];

    /// <summary>
    /// 当前客户端的玩家实例
    /// </summary>
    private INetworkPlayer _selfPlayer;

    private string _selfKey = "self";

    // 高频读取优化：玩家列表快照缓存（仅当玩家列表结构变化时重建，避免每次 GetAllPlayers 分配 List）。
    private INetworkPlayer[] _playersSnapshot = Array.Empty<INetworkPlayer>();
    private int _playersSnapshotRevision = -1;
    private int _playersRevision;

    public NetworkManager(INetworkClient networkClient, LocalNetworkPlayer selfPlayer)
    {
        _networkClient = networkClient;
        _selfPlayer = selfPlayer ?? new LocalNetworkPlayer(networkClient);
        lock (_playersLock)
        {
            _players[_selfKey] = _selfPlayer;
            MarkPlayersDirty_NoLock();
        }

        TrySubscribeToClientEvents();
    }

    #endregion

    #region INetworkManager 实现

    /// <summary>
    /// 覆盖接口默认实现：联机状态以客户端连接为准（而不是玩家数量）。
    /// </summary>
    public bool IsConnected => _networkClient?.IsConnected == true;

    /// <summary>
    /// 获取所有已连接的网络玩家
    /// 返回当前会话中所有玩家的只读集合
    /// </summary>
    /// <returns>所有网络玩家的枚举集合</returns>
    public IEnumerable<INetworkPlayer> GetAllPlayers()
    {
        SyncPlayersFromIdentityTracker();
        return GetPlayersSnapshot();
    }

    /// <summary>
    /// 根据玩家ID获取对应的网络玩家实例
    /// </summary>
    /// <param name="id">玩家的唯一标识符</param>
    /// <returns>对应的INetworkPlayer实例，如果未找到则返回null</returns> 
    /// <remarks>
    /// 本方法会先与 <see cref="NetworkIdentityTracker"/> 同步玩家ID列表，然后在本地缓存中查询。
    /// </remarks>
    public INetworkPlayer GetPlayer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        SyncPlayersFromIdentityTracker();
        lock (_playersLock)
        {
            _players.TryGetValue(id, out var player);
            return player;
        }
    }

    /// <summary>
    /// 获取当前已注册的网络玩家数量
    /// </summary>
    /// <returns>玩家总数</returns>
    /// <exception cref="NotImplementedException">当前方法尚未实现</exception>
    /// <remarks>
    /// 待实现功能：
    /// 1. 返回内部玩家集合的数量
    /// 2. 确保计数的准确性和线程安全
    /// 3. 可以用于判断联机状态
    /// </remarks>
    public int GetPlayerCount()
    {
        // 单机/未连接：保持至少为 1，避免将“单机”判定为 0 人导致某些逻辑异常。
        if (_networkClient?.IsConnected != true)
        {
            return 1;
        }

        // 联机：优先使用服务器侧分配的 PlayerId 列表（Welcome / PlayerListUpdate）统计。
        // 该列表由 NetworkIdentityTracker 从 GameEvent 中提取。
        try
        {
            HashSet<string> ids = NetworkIdentityTracker.GetPlayerIdsSnapshot();
            if (ids.Count > 0)
            {
                return ids.Count;
            }
        }
        catch
        {
            // ignored
        }

        SyncPlayersFromIdentityTracker();
        lock (_playersLock)
        {
            return Math.Max(1, _players.Count);
        }
    }

    /// <summary>
    /// 获取当前本地玩家的网络玩家实例
    /// </summary>
    /// <returns>当前玩家的INetworkPlayer实例，如果未注册则返回null</returns>
    /// <remarks>
    /// 本地玩家对象在构造函数中注入（或创建）并保持稳定引用；其 <c>userName</c> 会在可能时
    /// 优先使用服务器侧分配的 PlayerId（见 <see cref="NetworkIdentityTracker.GetSelfPlayerId"/>）。
    /// </remarks>
    public INetworkPlayer GetSelf()
    {
        return _selfPlayer;
    }

    /// <summary>
    /// 注册新的网络玩家到管理器中
    /// </summary>
    /// <param name="player">要注册的网络玩家实例</param>
    /// <exception cref="ArgumentNullException">当player参数为null时</exception>
    /// <remarks>
    /// 本方法仅维护本地玩家集合与快照缓存，不负责网络广播；网络侧的加入/离开由事件驱动同步。
    /// </remarks>
    public void RegisterPlayer(INetworkPlayer player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        string id = player.userName;
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N");
        }

        lock (_playersLock)
        {
            bool changed = !_players.TryGetValue(id, out INetworkPlayer existing) || !ReferenceEquals(existing, player);
            _players[id] = player;
            if (ReferenceEquals(player, _selfPlayer))
            {
                _selfPlayer = player;
                _selfKey = id;
            }

            if (changed)
            {
                MarkPlayersDirty_NoLock();
            }
        }
    }

    /// <summary>
    /// 从管理器中移除指定的网络玩家
    /// </summary>
    /// <param name="id">要移除的玩家ID</param>
    /// <remarks>
    /// 本方法仅修改本地集合并刷新快照；玩家离开事件会由网络事件驱动同步。
    /// </remarks>
    public void RemovePlayer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        lock (_playersLock)
        {
            if (string.Equals(id, _selfKey, StringComparison.Ordinal))
            {
                return;
            }

            if (_players.Remove(id))
            {
                MarkPlayersDirty_NoLock();
            }
        }
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 设置本地玩家实例
    /// 在连接建立后由网络组件调用
    /// </summary>
    /// <param name="player">本地玩家实例</param>
    internal void SetSelfPlayer(INetworkPlayer player)
    {
        _selfPlayer = player;
    }

    /// <summary>
    /// 清空所有玩家数据
    /// 在断开连接时调用
    /// </summary>
    internal void ClearAllPlayers()
    {
        lock (_playersLock)
        {
            _players.Clear();
            _selfKey = "self";
            _players[_selfKey] = _selfPlayer;
            MarkPlayersDirty_NoLock();
        }
    }

    /// <summary>
    /// 更新玩家信息
    /// 当接收到玩家状态更新时调用
    /// </summary>
    /// <param name="playerInfo">更新后的玩家信息</param>
    internal void UpdatePlayerInfo(object playerInfo)
    {
        if (playerInfo == null)
        {
            return;
        }

        try
        {
            if (!TryGetJsonElement(playerInfo, out JsonElement root))
            {
                string s = playerInfo as string ?? playerInfo.ToString();
                if (string.IsNullOrWhiteSpace(s) || !TryGetJsonElement(s, out root))
                {
                    return;
                }
            }

            if (root.ValueKind == JsonValueKind.Array)
            {
                UpdatePlayersFromArray(root);
                return;
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (TryGetPlayersArrayFromWelcome(root, out JsonElement list))
            {
                UpdatePlayersFromArray(list);
                return;
            }

            if (root.TryGetProperty("Players", out JsonElement players) && players.ValueKind == JsonValueKind.Array)
            {
                UpdatePlayersFromArray(players);
                return;
            }

            UpdateSinglePlayer(root);
        }
        catch
        {
            // ignored
        }
    }

    public INetworkPlayer GetPlayerByPeerId(int peerId)
    {
        // LiteNetLib 的 PeerId 并不会在当前协议中与 PlayerId 做映射下发，暂无法可靠实现。
        return null;
    }

    #endregion

    #region 事件订阅与同步

    private void TrySubscribeToClientEvents()
    {
        try
        {
            NetworkIdentityTracker.EnsureSubscribed(_networkClient);

            _networkClient.OnGameEventReceived += OnGameEventReceived;
            _networkClient.OnConnectionStateChanged += OnConnectionStateChanged;
        }
        catch
        {
            // ignored
        }
    }

    private void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        ClearAllPlayers();
    }

    private void OnGameEventReceived(string eventType, object payload)
    {
        // 仅在与“身份/玩家列表”相关的事件到来时刷新，避免对其他大量同步事件造成额外开销。
        if (eventType != NetworkMessageTypes.Welcome &&
            eventType != NetworkMessageTypes.PlayerListUpdate &&
            eventType != NetworkMessageTypes.PlayerJoined &&
            eventType != NetworkMessageTypes.PlayerLeft &&
            eventType != NetworkMessageTypes.HostChanged)
        {
            return;
        }

        SyncPlayersFromIdentityTracker();
        TryUpdatePlayersFromPayload(eventType, payload);
    }

    private void SyncPlayersFromIdentityTracker()
    {
        if (_networkClient?.IsConnected != true)
        {
            return;
        }

        try
        {
            NetworkIdentityTracker.EnsureSubscribed(_networkClient);
            HashSet<string> ids = NetworkIdentityTracker.GetPlayerIdsSnapshot();
            if (ids.Count == 0)
            {
                return;
            }

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();

            lock (_playersLock)
            {
                bool changed = false;

                // Self key switch: "self" -> server-assigned PlayerId
                if (!string.IsNullOrWhiteSpace(selfId) && !string.Equals(_selfKey, selfId, StringComparison.Ordinal))
                {
                    if (_players.Remove(_selfKey))
                    {
                        changed = true;
                    }

                    _selfKey = selfId;
                    if (!_players.TryGetValue(_selfKey, out INetworkPlayer existing) || !ReferenceEquals(existing, _selfPlayer))
                    {
                        _players[_selfKey] = _selfPlayer;
                        changed = true;
                    }
                }

                // Ensure all known ids exist.
                foreach (string id in ids)
                {
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    if (string.Equals(id, _selfKey, StringComparison.Ordinal))
                    {
                        if (!_players.TryGetValue(id, out INetworkPlayer existing) || !ReferenceEquals(existing, _selfPlayer))
                        {
                            _players[id] = _selfPlayer;
                            changed = true;
                        }
                        continue;
                    }

                    if (!_players.ContainsKey(id))
                    {
                        _players[id] = new RemoteNetworkPlayer(id);
                        changed = true;
                    }
                }

                // Remove players no longer present (keep self).
                List<string> toRemove = null;
                foreach (string key in _players.Keys)
                {
                    if (string.Equals(key, _selfKey, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!ids.Contains(key))
                    {
                        (toRemove ??= new List<string>()).Add(key);
                    }
                }

                if (toRemove != null)
                {
                    foreach (string key in toRemove)
                    {
                        if (_players.Remove(key))
                        {
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    MarkPlayersDirty_NoLock();
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private void TryUpdatePlayersFromPayload(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        try
        {
            switch (eventType)
            {
                case NetworkMessageTypes.Welcome:
                    if (TryGetPlayersArrayFromWelcome(root, out JsonElement list))
                    {
                        UpdatePlayersFromArray(list);
                    }

                    return;
                case NetworkMessageTypes.PlayerListUpdate:
                    if (root.TryGetProperty("Players", out JsonElement players) && players.ValueKind == JsonValueKind.Array)
                    {
                        UpdatePlayersFromArray(players);
                    }

                    return;
                case NetworkMessageTypes.PlayerJoined:
                    UpdateSinglePlayer(root);
                    return;
                case NetworkMessageTypes.PlayerLeft:
                    string leftId = GetString(root, "PlayerId");
                    if (!string.IsNullOrWhiteSpace(leftId))
                    {
                        RemovePlayer(leftId);
                    }

                    return;
            }
        }
        catch
        {
            // ignored
        }
    }

    private void UpdatePlayersFromArray(JsonElement playersArray)
    {
        if (playersArray.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (JsonElement p in playersArray.EnumerateArray())
        {
            UpdateSinglePlayer(p);
        }
    }

    private void UpdateSinglePlayer(JsonElement playerObj)
    {
        string playerId = GetString(playerObj, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        string playerName = GetString(playerObj, "PlayerName");
        string characterId = GetString(playerObj, "CharacterId");
        int locX = GetInt(playerObj, "LocationX", -1);
        int locY = GetInt(playerObj, "LocationY", -1);
        string locName = GetString(playerObj, "LocationName");

        lock (_playersLock)
        {
            bool changed = false;
            if (string.Equals(playerId, _selfKey, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(_selfPlayer?.userName) && !string.IsNullOrWhiteSpace(playerName))
                {
                    _selfPlayer.userName = playerName;
                }

                return;
            }

            if (!_players.TryGetValue(playerId, out INetworkPlayer existing) || existing == null)
            {
                existing = new RemoteNetworkPlayer(playerId);
                _players[playerId] = existing;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(playerName))
            {
                existing.userName = playerName;
            }

            if (!string.IsNullOrWhiteSpace(characterId))
            {
                existing.chara = characterId;
            }

            if (!string.IsNullOrWhiteSpace(locName))
            {
                existing.location = locName;
            }

            if (locX >= 0)
            {
                existing.location_X = locX;
            }

            if (locY >= 0)
            {
                existing.location_Y = locY;
            }

            if (changed)
            {
                MarkPlayersDirty_NoLock();
            }
        }
    }

    private static bool TryGetPlayersArrayFromWelcome(JsonElement root, out JsonElement list)
    {
        if (root.TryGetProperty("Players", out list) && list.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        if (root.TryGetProperty("PlayerList", out list) && list.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        list = default;
        return false;
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

    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    private static int GetInt(JsonElement elem, string property, int defaultValue)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return defaultValue;
            }

            return p.ValueKind switch
            {
                JsonValueKind.Number => p.TryGetInt32(out int i) ? i : defaultValue,
                JsonValueKind.String => int.TryParse(p.GetString(), out int i) ? i : defaultValue,
                _ => defaultValue,
            };
        }
        catch
        {
            return defaultValue;
        }
    }

    private IEnumerable<INetworkPlayer> GetPlayersSnapshot()
    {
        lock (_playersLock)
        {
            if (_playersSnapshotRevision == _playersRevision)
            {
                return _playersSnapshot;
            }

            int count = _players.Count;
            if (count <= 0)
            {
                _playersSnapshot = Array.Empty<INetworkPlayer>();
            }
            else
            {
                var arr = new INetworkPlayer[count];
                int i = 0;
                foreach (INetworkPlayer p in _players.Values)
                {
                    arr[i++] = p;
                }

                _playersSnapshot = arr;
            }

            _playersSnapshotRevision = _playersRevision;
            return _playersSnapshot;
        }
    }

    private void MarkPlayersDirty_NoLock()
    {
        unchecked
        {
            _playersRevision++;
        }
    }

    #endregion
}
