using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AiSimClient.Network;

// 房间内玩家信息（从 Welcome.PlayerList 解析）。NetworkServer 用 PlayerList 字段。
public class PlayerInfo
{
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public bool IsHost { get; set; }
    public bool IsConnected { get; set; }
}

// 战斗敌人快照（对齐 networkplugin EnemyStateSnapshot）。
public class EnemyInfo
{
    public string EnemyId { get; set; } = "";
    public string EnemyName { get; set; } = "";
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Index { get; set; }
    public bool IsAlive { get; set; } = true;
    public string Display => string.IsNullOrWhiteSpace(EnemyName)
        ? $"敌人:{EnemyId}"
        : $"敌人:{EnemyName}";
}

// 目标下拉统一条目：区分玩家与敌人。
public class TargetEntry
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Display { get; set; } = "";
    public bool IsEnemy { get; set; }
}

// OnBattleStart 载荷中对齐 networkplugin BattleStateSnapshot.Enemies 的结构。
public class BattleStateData
{
    public List<EnemyInfo>? Enemies { get; set; }
}

public class BattleStartData
{
    public BattleStateData? BattleState { get; set; }
}

// BattleEnemyIntentChanged 载荷中的 Enemy 子对象。
public class EnemyRef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int RootIndex { get; set; }
}

public class EnemyIntentPayload
{
    public EnemyRef? Enemy { get; set; }
}

// BattleEnemyStateChanged / EnemyStateUpdate 载荷中的 Enemy 子对象。
public class EnemyStateData
{
    public string? SpawnId { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int RootIndex { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int Block { get; set; }
    public int Shield { get; set; }
    public string? Status { get; set; }
    public bool IsAlive { get; set; } = true;
    public bool IsDying { get; set; }
}

// 敌人状态变化事件载荷（对齐 EnemySyncPatch.BuildEnemyUpdateData）。
public class EnemyStateChangedPayload
{
    public string? UpdateType { get; set; }
    public long Timestamp { get; set; }
    public string? BattleId { get; set; }
    public EnemyStateData? Enemy { get; set; }
}

// Welcome 消息载荷：服务器分配的自身 PlayerId 与房间玩家列表。
// NetworkServer.Broadcast.cs 用 PlayerList 字段名；RelayServer 可能用 Players，两者兼容。
public class WelcomeData
{
    public string PlayerId { get; set; } = "";
    public bool IsHost { get; set; }
    // NetworkServer 使用 PlayerList；兼容 Players 字段。
    public List<PlayerInfo>? PlayerList { get; set; }
    public List<PlayerInfo>? Players { get; set; }
}

/// <summary>
/// 独立 LiteNetLib 客户端，复刻游戏内 NetworkClient 的连接/握手/心跳/发送协议，
/// 以新真实玩家身份连接房主 P2P 直连服务器，用于模拟 AI 玩家的网络行为。
/// </summary>
public class SimNetworkClient
{
    private readonly EventBasedNetListener _listener;
    private readonly NetManager _netManager;
    private NetPeer? _serverPeer;
    private string _connectionKey = "LBoL_Network_Plugin";
    private DateTime _lastHeartbeatUtc = DateTime.MinValue;
    private readonly int _heartbeatIntervalMs = 5000;

    public bool IsConnected => _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected;
    public string SelfPlayerId { get; private set; } = "";
    public string PlayerName { get; set; } = "AI Bot";
    // CharacterId 留空：MaUser 资源不存在会导致 OtherPlayersOverlay 头像加载报错刷屏。
    // 留空时接收端会兜底处理，避免日志噪音。
    public string CharacterId { get; set; } = "";

    public event Action<string>? OnConnectedEvent;       // endpoint
    public event Action? OnDisconnectedEvent;
    public event Action<WelcomeData>? OnWelcomeEvent;
    public event Action<List<EnemyInfo>>? OnBattleStartEvent;
    // 单个敌人发现事件：host 通过 BattleEnemyIntentChanged 增量广播每个敌人，sim 端去重累积。
    public event Action<EnemyInfo>? OnEnemyDiscoveredEvent;
    // 敌人状态变化事件：host 通过 BattleEnemyStateChanged 广播敌人 HP/Block/Shield/死亡变化。
    public event Action<EnemyStateData>? OnEnemyStateChangedEvent;
    public event Action<string>? OnMidGameJoinResponse;  // MidGameJoinResponse json
    public event Action<string, string>? OnLog;        // (level, message)

    // 与游戏内 JsonCompat.Settings 对齐：PascalCase、忽略 null、无格式化。
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
        ContractResolver = new DefaultContractResolver(),
    };

    public SimNetworkClient()
    {
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener)
        {
            DisconnectTimeout = 30000,
            UnsyncedEvents = true,
        };
        RegisterEvents();
    }

    private void Log(string level, string msg) => OnLog?.Invoke(level, msg);

    private void RegisterEvents()
    {
        // 连接建立：保存对端引用并发送 PlayerJoined 握手消息。
        _listener.PeerConnectedEvent += peer =>
        {
            _serverPeer = peer;
            _lastHeartbeatUtc = DateTime.MinValue;
            Log("INFO", $"已连接到服务器 {peer.EndPoint}");
            SendPlayerJoined();
            OnConnectedEvent?.Invoke(peer.EndPoint?.ToString() ?? "");
        };

        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            _serverPeer = null;
            _lastHeartbeatUtc = DateTime.MinValue;
            SelfPlayerId = "";
            Log("WARN", $"已断开: {peer.EndPoint}, 原因: {info.Reason}");
            OnDisconnectedEvent?.Invoke();
        };

        // 数据接收：读取消息类型 + JSON 载荷并路由。
        _listener.NetworkReceiveEvent += (peer, reader, method) =>
        {
            try
            {
                string msgType = reader.GetString();
                string json = reader.AvailableBytes > 0 ? reader.GetString() : "";

                if (msgType == "Welcome")
                {
                    HandleWelcome(json);
                }
                else if (msgType == "MidGameJoinResponse")
                {
                    Log("RECV", $"MidGameJoinResponse: {Truncate(json, 300)}");
                    OnMidGameJoinResponse?.Invoke(json);
                }
                else if (msgType == "OnBattleStart")
                {
                    HandleBattleStart(json);
                }
                else if (msgType == "BattleEnemyIntentChanged")
                {
                    HandleEnemyIntentChanged(json);
                }
                else if (msgType == "BattleEnemyStateChanged" || msgType == "EnemyStateUpdate")
                {
                    HandleEnemyStateChanged(json);
                }
                else if (msgType == "FullStateSyncResponse")
                {
                    Log("RECV", $"FullStateSyncResponse: {Truncate(json, 300)}");
                }
                else if (msgType == "HeartbeatResponse" || msgType == "Heartbeat")
                {
                    // 心跳响应仅消费，刷新超时计时器。
                }
                else
                {
                    Log("RECV", $"{msgType}: {Truncate(json, 200)}");
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", $"解析消息失败: {ex.Message}");
            }
            finally
            {
                reader.Recycle();
            }
        };
    }

    private void HandleWelcome(string json)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<WelcomeData>(json, JsonSettings);
            if (data == null)
            {
                Log("ERROR", "Welcome 反序列化为 null");
                return;
            }
            SelfPlayerId = data.PlayerId ?? "";
            // NetworkServer 用 PlayerList，RelayServer 可能用 Players；优先 PlayerList。
            var list = data.PlayerList ?? data.Players ?? new List<PlayerInfo>();
            data.PlayerList = list;
            Log("INFO", $"收到 Welcome: PlayerId={data.PlayerId}, IsHost={data.IsHost}, 玩家数={list.Count}");
            foreach (var p in list)
                Log("INFO", $"  玩家: {p.PlayerId}={p.PlayerName} IsHost={p.IsHost} Connected={p.IsConnected}");
            OnWelcomeEvent?.Invoke(data);
        }
        catch (Exception ex)
        {
            Log("ERROR", $"解析 Welcome 失败: {ex.Message}");
        }
    }

    // 解析 OnBattleStart，提取当前战斗敌人列表供目标下拉使用。
    private void HandleBattleStart(string json)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<BattleStartData>(json, JsonSettings);
            var enemies = data?.BattleState?.Enemies ?? new List<EnemyInfo>();
            enemies = enemies.FindAll(e => e.IsAlive && !string.IsNullOrWhiteSpace(e.EnemyId));
            Log("INFO", $"收到 OnBattleStart: 存活敌人 {enemies.Count} 个");
            foreach (var e in enemies)
                Log("INFO", $"  敌人: {e.EnemyId}={e.EnemyName} Hp={e.Health}/{e.MaxHealth} Idx={e.Index}");
            OnBattleStartEvent?.Invoke(enemies);
        }
        catch (Exception ex)
        {
            Log("ERROR", $"解析 OnBattleStart 失败: {ex.Message}");
        }
    }

    // 解析 BattleEnemyIntentChanged，提取单个敌人信息。host 在 sim 端连接后会重广播当前敌人意图，
    // 也在每回合敌人更新意图时广播，使 sim 端能持续获取/刷新敌人列表。
    private void HandleEnemyIntentChanged(string json)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<EnemyIntentPayload>(json, JsonSettings);
            var enemyRef = data?.Enemy;
            if (enemyRef == null || string.IsNullOrWhiteSpace(enemyRef.Id))
            {
                return;
            }

            var info = new EnemyInfo
            {
                EnemyId = enemyRef.Id,
                EnemyName = enemyRef.Name ?? "",
                Index = enemyRef.RootIndex,
                IsAlive = true,
            };
            Log("INFO", $"发现敌人: {info.EnemyId}={info.EnemyName} Idx={info.Index}");
            OnEnemyDiscoveredEvent?.Invoke(info);
        }
        catch (Exception ex)
        {
            Log("ERROR", $"解析 BattleEnemyIntentChanged 失败: {ex.Message}");
        }
    }

    // 解析 BattleEnemyStateChanged / EnemyStateUpdate，提取敌人 HP/Block/Shield/死亡状态。
    private void HandleEnemyStateChanged(string json)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<EnemyStateChangedPayload>(json, JsonSettings);
            var enemy = data?.Enemy;
            if (enemy == null || (string.IsNullOrWhiteSpace(enemy.Id) && string.IsNullOrWhiteSpace(enemy.SpawnId)))
            {
                return;
            }

            Log("INFO", $"敌人状态变化: {enemy.Name ?? enemy.Id} HP={enemy.CurrentHp}/{enemy.MaxHp} Block={enemy.Block} Alive={enemy.IsAlive} Type={data?.UpdateType}");
            OnEnemyStateChangedEvent?.Invoke(enemy);
        }
        catch (Exception ex)
        {
            Log("ERROR", $"解析 BattleEnemyStateChanged 失败: {ex.Message}");
        }
    }

    public void SetConnectionKey(string key) =>
        _connectionKey = string.IsNullOrWhiteSpace(key) ? "LBoL_Network_Plugin" : key;

    // 绑定本地 UDP 端口，必须在 ConnectToServer 之前调用。
    public bool Start()
    {
        if (_netManager.Start())
        {
            Log("INFO", "网络客户端已启动");
            return true;
        }
        Log("ERROR", "网络客户端启动失败");
        return false;
    }

    // 发起连接：将连接密钥写入 NetDataWriter 作为认证信息。
    public void ConnectToServer(string host, int port)
    {
        var writer = new NetDataWriter();
        writer.Put(_connectionKey);
        _netManager.Connect(host, port, writer);
        Log("INFO", $"正在连接 {host}:{port}...");
    }

    public void Disconnect()
    {
        _netManager.DisconnectAll();
        _serverPeer = null;
    }

    public void Stop()
    {
        _netManager.Stop();
        _serverPeer = null;
    }

    // 必须由 UI 线程定时调用（DispatcherTimer），驱动事件处理与心跳。
    public void PollEvents()
    {
        _netManager.PollEvents();
        SendHeartbeatIfNeeded();
    }

    private void SendHeartbeatIfNeeded()
    {
        if (!IsConnected || _serverPeer == null) return;
        var now = DateTime.UtcNow;
        if (_lastHeartbeatUtc != DateTime.MinValue &&
            (now - _lastHeartbeatUtc).TotalMilliseconds < _heartbeatIntervalMs)
            return;
        var writer = new NetDataWriter();
        writer.Put("Heartbeat");
        _serverPeer.Send(writer, DeliveryMethod.Unreliable);
        _lastHeartbeatUtc = now;
    }

    // 发送游戏事件：type + JSON → ReliableOrdered，与游戏内 NetworkClient.SendGameEventData 一致。
    public void SendGameEventData(string eventType, object payload)
    {
        if (!IsConnected || _serverPeer == null)
        {
            Log("WARN", $"未连接，无法发送 {eventType}");
            return;
        }
        try
        {
            string json = JsonConvert.SerializeObject(payload, JsonSettings);
            var writer = new NetDataWriter();
            writer.Put(eventType);
            writer.Put(json);
            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Log("SEND", $"{eventType}: {Truncate(json, 300)}");
        }
        catch (Exception ex)
        {
            Log("ERROR", $"发送 {eventType} 失败: {ex.Message}");
        }
    }

    // 握手：连上后立即广播本客户端的加入信息。
    private void SendPlayerJoined()
    {
        var info = new
        {
            PlayerName = string.IsNullOrWhiteSpace(PlayerName) ? "AI Bot" : PlayerName,
            CharacterId = CharacterId,
            ConnectionTime = DateTime.Now.Ticks,
        };
        SendGameEventData("PlayerJoined", info);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "...";
}