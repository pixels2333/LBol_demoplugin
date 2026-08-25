using System;
using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
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

    /// <summary>
    /// 玩家列表锁对象，保护对_players字典的访问，确保线程安全
    /// </summary>
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

    /// <summary>
    /// 当前本地玩家的键值，初始为"self"，连接服务器后切换为服务器分配的PlayerId
    /// </summary>
    private string _selfKey = "self";

    /// <summary>
    /// 玩家列表快照缓存，用于高频读取场景（如每帧UI更新）避免重复分配数组
    /// </summary>
    /// <remarks>
    /// 仅当玩家列表结构变化时重建，配合版本号实现惰性更新。
    /// </remarks>
    private INetworkPlayer[] _playersSnapshot = Array.Empty<INetworkPlayer>();

    /// <summary>
    /// 快照缓存的版本号，与 _playersRevision 比对判断是否需重建
    /// </summary>
    private int _playersSnapshotRevision = -1;

    /// <summary>
    /// 玩家列表变更版本号，每次结构变化时递增
    /// </summary>
    private int _playersRevision;

    /// <summary>
    /// 初始化网络管理器并注入网络客户端和本地玩家
    /// </summary>
    /// <param name="networkClient">网络客户端实例，用于处理网络通信</param>
    /// <param name="selfPlayer">本地玩家实例，为null时自动创建默认实例</param>
    /// <remarks>
    /// 构造时即订阅客户端事件，确保玩家状态同步不遗漏早期消息。
    /// </remarks>
    public NetworkManager(INetworkClient networkClient, LocalNetworkPlayer selfPlayer)
    {
        _networkClient = networkClient;
        _selfPlayer = selfPlayer ?? new LocalNetworkPlayer(networkClient);
        lock (_playersLock)
        {
            _players[_selfKey] = _selfPlayer;
            MarkPlayersDirty_NoLock();
        }

        try
        {
            NetworkIdentityTracker.EnsureSubscribed(_networkClient);

            // 复活同步补丁：监听所有对等端（尤其是房主）的复活事件
            ResurrectSyncPatch.EnsureSubscribed(_networkClient);

            // GapOptions 同步补丁：监听 GapOptions 相关事件
            GapOptionsSyncPatch.EnsureSubscribed(_networkClient);

            _networkClient.OnGameEventReceived += OnGameEventReceived;
            _networkClient.OnConnectionStateChanged += OnConnectionStateChanged;
        }
        catch
        {
            // TODO: 应记录事件订阅失败的异常信息，便于排查初始化问题
            // 订阅失败不应阻止 NetworkManager 初始化；后续事件会在下次 EnsureSubscribed 时重试
        }
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
        // 先与服务器侧身份追踪器同步，确保本地缓存反映最新玩家列表
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

        // 同步后再查询，避免读取到已被服务器移除的过期玩家
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
            // TODO: 应记录 NetworkIdentityTracker 调用异常，避免静默失败
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
            // userName 为空时回退到 Guid，保证字典键的非空约束
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
            // 禁止移除 self，避免本地玩家引用丢失导致后续逻辑异常
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
            // 重置 selfKey 为默认值，等待下次连接时由服务器重新分配
            _selfKey = "self";
            _players[_selfKey] = _selfPlayer;
            MarkPlayersDirty_NoLock();
        }
    }

    /// <summary>
    /// 根据 LiteNetLib PeerId 获取对应玩家。当前协议未下发 PeerId→PlayerId 映射，故始终返回 null。
    /// </summary>
    /// <param name="peerId">LiteNetLib 对等端 ID。</param>
    /// <returns>目前固定返回 null；待协议扩展后实现。</returns>
    /// <remarks>
    /// TODO: 协议层需要增加 PeerId 到 PlayerId 的映射下发，才能支持此查询。
    /// </remarks>
    public INetworkPlayer GetPlayerByPeerId(int peerId)
    {
        // LiteNetLib 的 PeerId 并不会在当前协议中与 PlayerId 做映射下发，暂无法可靠实现。
        return null;
    }

    #endregion

    #region 事件订阅与同步

    /// <summary>
    /// 订阅客户端核心事件：身份追踪、复活同步、GapOptions 同步，以及连接状态变化。
    /// </summary>
    /// <remarks>
    /// 在构造函数中调用一次即可；多次调用会被内部去重（EnsureSubscribed 模式）。
    /// 失败时静默处理，避免构造异常导致插件初始化中断。
    /// </remarks>


    /// <summary>
    /// 连接状态变化回调。断连时清空本地玩家集合，防止残留过期数据。
    /// </summary>
    /// <param name="connected">true=已连接，false=已断开。</param>
    private void OnConnectionStateChanged(bool connected)
    {
        // 断连时立即清空玩家集合，防止 UI 或逻辑层读取到已离线的过期玩家数据
        if (connected)
        {
            return;
        }

        ClearAllPlayers();
    }

    /// <summary>
    /// 游戏事件回调。仅在与“身份/玩家列表”相关的事件到来时刷新本地缓存，
    /// 避免对高频同步事件（如 StateSync）造成额外锁开销。
    /// </summary>
    /// <param name="eventType">事件类型标识。</param>
    /// <param name="payload">事件负载，可为 JsonElement 或 JSON 字符串。</param>
    private void OnGameEventReceived(string eventType, object payload)
    {
        // 白名单过滤：仅处理玩家身份相关事件，避免 StateSync 等高频事件触发锁竞争
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

    /// <summary>
    /// 与 <see cref="NetworkIdentityTracker"/> 同步玩家列表，确保本地缓存与服务器视角一致。
    /// </summary>
    /// <remarks>
    /// 核心逻辑：
    /// 1. 将 selfKey 从 "self" 切换为服务器分配的 PlayerId；
    /// 2. 为所有服务器已知 ID 创建 <see cref="RemoteNetworkPlayer"/> 占位；
    /// 3. 移除服务器列表中已不存在的本地条目；
    /// 4. 任何结构变化都会使快照缓存失效（<c>_playersRevision++</c>）。
    /// </remarks>
    private void SyncPlayersFromIdentityTracker()
    {
        // 检查网络客户端是否已连接；未连接则跳过同步，避免操作无效状态
        if (_networkClient?.IsConnected != true)
        {
            return;
        }

        try
        {
            // 确保已订阅身份追踪器，以接收服务器身份变更通知
            NetworkIdentityTracker.EnsureSubscribed(_networkClient);
            // 获取身份追踪器中当前所有已知玩家 ID 的快照（不可变集合）
            HashSet<string> ids = NetworkIdentityTracker.GetPlayerIdsSnapshot();
            // 快照为空时无需更新，直接返回
            if (ids.Count == 0)
            {
                return;
            }

            // 从身份追踪器获取服务器分配的本地玩家 ID
            string selfId = NetworkIdentityTracker.GetSelfPlayerId();

            // 加锁保护 _players 字典的并发访问
            lock (_playersLock)
            {
                // 标记本次同步是否产生了任何变化，用于后续决定是否需要刷新快照缓存
                bool changed = false;

                // ---- 阶段 1：Self Key 迁移 ----
                // 将字典键从初始化时的 "self" 切换为服务器在 Welcome 消息中分配的真实 PlayerId，
                // 确保本地玩家在字典中的键与其他玩家一样使用服务器 ID，保持引用一致性
                if (!string.IsNullOrWhiteSpace(selfId) && !string.Equals(_selfKey, selfId, StringComparison.Ordinal))
                {
                    // 移除旧键 "self" 对应的条目
                    if (_players.Remove(_selfKey))
                    {
                        changed = true;
                    }

                    // 更新本地自我键为服务器分配的真实 PlayerId
                    _selfKey = selfId;
                    // 确保新键下存储的是 _selfPlayer 实例；若不存在或引用不一致则重新赋值
                    if (!_players.TryGetValue(_selfKey, out INetworkPlayer existing) || !ReferenceEquals(existing, _selfPlayer))
                    {
                        _players[_selfKey] = _selfPlayer;
                        changed = true;
                    }
                }

                // ---- 阶段 2：补齐服务器已知玩家 ----
                // 遍历身份追踪器中的所有玩家 ID，为每个 ID 在 _players 字典中创建对应条目。
                // 这样 UI 或逻辑层在收到完整属性数据前即可查询到玩家的存在性
                foreach (string id in ids)
                {
                    // 跳过空或空白 ID，避免无效数据污染玩家列表
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    // ID 等于当前自我键时的处理：确保字典中该键指向 _selfPlayer 实例
                    if (string.Equals(id, _selfKey, StringComparison.Ordinal))
                    {
                        // 检查是否已存在且引用一致，否则强制更新
                        if (!_players.TryGetValue(id, out INetworkPlayer existing) || !ReferenceEquals(existing, _selfPlayer))
                        {
                            _players[id] = _selfPlayer;
                            changed = true;
                        }
                        continue;
                    }

                    // 对于其他玩家 ID，若字典中尚不存在则创建 RemoteNetworkPlayer 占位对象
                    if (!_players.ContainsKey(id))
                    {
                        _players[id] = new RemoteNetworkPlayer(id);
                        changed = true;
                    }
                }

                // ---- 阶段 3：清理已离线的玩家 ----
                // 移除字典中那些不在身份追踪器最新快照中的玩家条目（保留自我玩家）。
                // 使用延迟初始化列表：仅在需要移除时才创建 List，避免常规同步路径产生不必要的 GC 压力
                List<string> toRemove = null;
                // 遍历当前字典中的所有键
                foreach (string key in _players.Keys)
                {
                    // 跳过自我玩家键，不参与移除逻辑
                    if (string.Equals(key, _selfKey, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // 若当前键不在服务器最新 ID 集中，则标记为待移除
                    if (!ids.Contains(key))
                    {
                        (toRemove ??= new List<string>()).Add(key);
                    }
                }

                // 批量执行移除操作
                if (toRemove != null)
                {
                    foreach (string key in toRemove)
                    {
                        // 从字典移除并检查是否实际删除了条目
                        if (_players.Remove(key))
                        {
                            changed = true;
                        }
                    }
                }

                // 若本次同步产生了任何结构变化（添加/移除/替换），则使快照缓存失效
                if (changed)
                {
                    MarkPlayersDirty_NoLock();
                }
            }
        }
        catch
        {
            // TODO: 应记录身份同步失败的异常，防止静默吞掉配置或协议错误
            // 当前仅占位，后续应接入日志系统以捕获异常详情
            // ignored
        }
    }

    /// <summary>
    /// 根据事件类型解析 payload 中的玩家信息，并更新本地玩家集合。
    /// </summary>
    /// <param name="eventType">事件类型。</param>
    /// <param name="payload">事件负载，应为可解析的 JSON。</param>
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
                    if ((root.TryGetProperty("Players", out JsonElement list) && list.ValueKind == JsonValueKind.Array) ||
                        (root.TryGetProperty("PlayerList", out list) && list.ValueKind == JsonValueKind.Array))
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
            // TODO: Payload 解析失败应至少记录 eventType，便于定位协议兼容性问题
            // Payload 解析失败不应中断事件处理流水线
        }
    }

    /// <summary>
    /// 批量更新玩家列表。遍历 JSON 数组，为每个元素调用 <see cref="UpdateSinglePlayer"/>。
    /// </summary>
    /// <param name="playersArray">包含玩家对象的 JSON 数组。</param>
    private void UpdatePlayersFromArray(JsonElement playersArray)
    {
        if (playersArray.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (JsonElement p in playersArray.EnumerateArray())
        {
            UpdateSinglePlayer(p); // 逐个更新玩家
        }
    }

    /// <summary>
    /// 从 JSON 对象中提取玩家属性并更新（或创建）对应的 <see cref="INetworkPlayer"/> 实例。
    /// </summary>
    /// <param name="playerObj">包含 PlayerId/PlayerName/CharacterId/LocationX/LocationY/Stage/LocationName 的 JSON 对象。</param>
    /// <remarks>
    /// 对 self 玩家仅更新可变属性（userName、stage），避免覆盖对象引用；
    /// 对远端玩家创建 <see cref="RemoteNetworkPlayer"/> 并填充所有字段。
    /// </remarks>
    private void UpdateSinglePlayer(JsonElement playerObj)
    {
        string playerId = GetString(playerObj, "PlayerId"); // 提取玩家唯一标识
        if (string.IsNullOrWhiteSpace(playerId)) // ID 为空或空白则无效
        {
            return; // 无效 ID，跳过处理
        }

        string playerName = GetString(playerObj, "PlayerName"); // 提取玩家显示名称
        string characterId = GetString(playerObj, "CharacterId"); // 提取角色标识
        // 使用 -1 作为默认值表示服务器未下发该字段，避免覆盖本地有效值（如坐标 0,0）
        int locX = GetInt(playerObj, "LocationX", -1); // 提取 X 坐标，默认 -1 表示未下发
        int locY = GetInt(playerObj, "LocationY", -1); // 提取 Y 坐标，默认 -1 表示未下发
        int stage = GetInt(playerObj, "Stage", -1); // 提取场景/关卡编号，默认 -1 表示未下发
        string locName = GetString(playerObj, "LocationName"); // 提取位置名称

        lock (_playersLock) // 加锁保护玩家字典线程安全
        {
            bool changed = false; // 标记本次是否新增玩家
            if (string.Equals(playerId, _selfKey, StringComparison.Ordinal)) // 判断目标是否为本地玩家
            {
                try
                {
                    _selfPlayer.playerId = playerId; // 同步玩家 ID（防御性写入）
                }
                catch
                {
                    // TODO: LocalNetworkPlayer setter 失败应记录一次，避免反复静默失败
                    // 某些 LocalNetworkPlayer 实现可能不支持 setter，忽略
                }

                if (string.IsNullOrWhiteSpace(_selfPlayer?.userName) && !string.IsNullOrWhiteSpace(playerName)) // 本地用户名为空且服务器提供了有效名称
                {
                    _selfPlayer.userName = playerName; // 回填本地玩家名称
                }

                if (stage >= 0) // 服务器下发了有效场景值
                {
                    _selfPlayer.stage = stage; // 更新本地玩家场景
                }

                return; // 本地玩家属性处理完毕，直接返回
            }

            if (!_players.TryGetValue(playerId, out INetworkPlayer existing) || existing == null) // 字典中无此玩家或取出的值为空
            {
                existing = new RemoteNetworkPlayer(playerId, playerName); // 新建远端玩家实例
                _players[playerId] = existing; // 加入玩家字典
                changed = true; // 标记列表已变化
            }

            try
            {
                existing.playerId = playerId; // 同步玩家 ID（防御性写入）
            }
            catch
            {
                // TODO: RemoteNetworkPlayer setter 失败应记录异常信息
                // ignored
            }

            if (!string.IsNullOrWhiteSpace(playerName)) // 服务器下发了非空名称
            {
                existing.userName = playerName; // 更新玩家显示名称
            }

            if (!string.IsNullOrWhiteSpace(characterId)) // 服务器下发了非空角色 ID
            {
                existing.chara = characterId; // 更新角色标识
            }

            if (!string.IsNullOrWhiteSpace(locName)) // 服务器下发了非空位置名称
            {
                existing.location = locName; // 更新位置名称
            }

            if (locX >= 0) // X 坐标有效（非默认值 -1）
            {
                existing.location_X = locX; // 更新 X 坐标
            }

            if (locY >= 0) // Y 坐标有效（非默认值 -1）
            {
                existing.location_Y = locY; // 更新 Y 坐标
            }

            if (stage >= 0) // 场景/关卡值有效（非默认值 -1）
            {
                existing.stage = stage; // 更新场景/关卡
            }

            if (changed) // 本次循环新增了远端玩家
            {
                MarkPlayersDirty_NoLock(); // 标记玩家列表脏，通知上层刷新
            }
        }
    }

    /// <summary>
    /// 从 Welcome 消息的 JSON 中提取玩家列表数组。
    /// 兼容 "Players" 和 "PlayerList" 两种属性名（历史协议残留）。
    /// </summary>
    /// <param name="root">Welcome 消息的 JSON 根元素。</param>
    /// <param name="list">输出玩家数组。</param>
    /// <returns>成功提取到数组返回 true，否则 false。</returns>


    /// <summary>
    /// 将事件负载安全转换为 <see cref="JsonElement"/>，支持已经是 JsonElement 的情况或 JSON 字符串。
    /// </summary>
    /// <param name="payload">原始负载对象。</param>
    /// <param name="root">输出的 JSON 根元素。</param>
    /// <returns>转换成功返回 true，否则 false。</returns>
    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            // 若事件系统已反序列化为 JsonElement，直接复用避免二次解析
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            // 某些旧版本消息仍通过字符串形式传递 JSON，需要手动解析
            if (payload is string s)
            {
                using JsonDocument doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
                return true;
            }
        }
        catch
        {
            // 解析失败静默返回 false，由调用方决定是否继续处理
        }

        root = default;
        return false;
    }

    /// <summary>
    /// 安全地从 JSON 对象中提取字符串属性，失败时返回 null。
    /// </summary>
    /// <param name="elem">JSON 对象元素。</param>
    /// <param name="property">属性名。</param>
    /// <returns>字符串值；非字符串类型返回 <c>GetRawText()</c>；失败返回 null。</returns>
    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            // 对非字符串类型回退到原始文本，保证协议兼容（如数字被序列化为字符串的情况）
            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 安全地从 JSON 对象中提取整数属性，失败时返回默认值。
    /// </summary>
    /// <param name="elem">JSON 对象元素。</param>
    /// <param name="property">属性名。</param>
    /// <param name="defaultValue">提取失败时的回退值（如 -1 表示未设置）。</param>
    /// <returns>整数属性值或默认值。</returns>
    private static int GetInt(JsonElement elem, string property, int defaultValue)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return defaultValue;
            }

            // 兼容数字直接量与字符串两种序列化形式，应对不同版本服务器或中间件
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

    /// <summary>
    /// 获取玩家列表的快照缓存。仅在玩家结构变化时重建数组，避免高频读取场景下的重复分配。
    /// </summary>
    /// <returns>当前所有玩家的只读数组快照。</returns>
    /// <remarks>
    /// 使用版本号 <c>_playersSnapshotRevision</c> 与 <c>_playersRevision</c> 比对实现无锁外层的惰性重建。
    /// </remarks>
    private IEnumerable<INetworkPlayer> GetPlayersSnapshot()
    {
        lock (_playersLock) // 线程安全：对玩家列表的读写操作加锁，避免并发问题
        {
            // 版本号一致说明玩家列表结构未变，直接返回缓存快照，避免数组重新分配与复制
            if (_playersSnapshotRevision == _playersRevision)
            {
                return _playersSnapshot; // 缓存命中，直接返回已有的数组引用
            }

            int count = _players.Count; // 获取当前玩家数量
            if (count <= 0) // 无玩家时特殊处理
            {
                // 空集合时使用共享单例，减少 GC 碎片
                _playersSnapshot = Array.Empty<INetworkPlayer>(); // 使用空数组单例，避免重复分配
            }
            else // 有玩家时创建新数组
            {
                INetworkPlayer[] arr = new INetworkPlayer[count]; // 按当前玩家数量分配数组
                int i = 0; // 数组索引计数器
                foreach (INetworkPlayer p in _players.Values) // 遍历字典中的玩家值
                {
                    arr[i++] = p; // 将玩家引用放入数组并递增索引
                }

                _playersSnapshot = arr; // 将新数组赋给缓存快照
            }

            _playersSnapshotRevision = _playersRevision; // 同步版本号，标记缓存已更新
            return _playersSnapshot; // 返回构建好的快照数组
        }
    }

    /// <summary>
    /// 标记玩家列表缓存为脏，使下次 <see cref="GetPlayersSnapshot"/> 重建数组。
    /// </summary>
    /// <remarks>
    /// 必须在已持有 <c>_playersLock</c> 时调用，因此命名包含 "_NoLock"。
    /// 使用 unchecked 自增避免 int 溢出异常（虽然正常游戏不可能达到 2^31 次变更）。
    /// </remarks>
    private void MarkPlayersDirty_NoLock()
    {
        unchecked
        {
            _playersRevision++;
        }
    }

    #endregion
}
