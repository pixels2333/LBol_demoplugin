using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.Client;

#region 网络客户端：连接管理、事件路由与自动重连

/// <summary>
/// LiteNetLib 网络客户端，负责与游戏服务器的连接建立、数据传输和事件管理。
/// </summary>
/// <remarks>
/// 支持 JSON 序列化、基于 Timer 的自动重连、心跳保活以及连接状态监控。
/// 通过 <see cref="ISynchronizationManager"/> 将网络事件桥接到游戏状态同步引擎。
/// 基于 LiteNetLib 的 UDP 协议实现，使用 <see cref="DeliveryMethod.ReliableOrdered"/> 保证消息有序到达。
/// </remarks>
public class NetworkClient : INetworkClient
{
    #region 字段与常量

    /// <summary>
    /// LiteNetLib 底层网络事件监听器，负责将底层 UDP 事件（连接、断连、数据接收）
    /// 转换为 C# 事件供本类订阅和处理。
    /// </summary>
    private EventBasedNetListener _listener;
    /// <summary>LiteNetLib 网络管理器，负责 UDP 连接管理。</summary>

    /// <summary>LiteNetLib 网络管理器，负责 UDP 连接的底层生命周期管理（启动、停止、连接、断开）。</summary>
    private NetManager _netManager;

    /// <summary>与服务器的连接对等体，用于发送和接收数据。</summary>
    private NetPeer _serverPeer;

    /// <summary>
    /// 连接密钥，用于在连接建立阶段验证客户端身份。
    /// 服务器在 <code>OnConnectionRequest</code> 中比对密钥，不匹配则拒绝连接。
    /// </summary>
    private string _connectionKey;

    /// <summary>
    /// 上次心跳发送的 UTC 时间，用于计算下次心跳时机。
    /// 使用 <code>DateTime.MinValue</code> 作为哨兵值，表示"尚未发送过心跳"。
    /// </summary>
    private DateTime _lastHeartbeatSentUtc = DateTime.MinValue;
    /// <summary>
    /// 心跳发送间隔（毫秒），按连接超时的 1/3 自动调整，范围 1s~10s。
    /// 服务器通过心跳判断客户端是否存活，未收到心跳将在超时时断开连接。
    /// </summary>
    private int _heartbeatIntervalMs = 5_000;

    /// <summary>注入的网络管理器实例，用于获取玩家信息和联机状态。</summary>
    private INetworkManager _networkManager;
    /// <summary>注入的网络玩家实例，表示当前客户端玩家；优先级次于 <see cref="INetworkManager.GetSelf"/>。</summary>
    private INetworkPlayer _networkPlayer;
    /// <summary>当 _networkManager 或 _networkPlayer 不可用时的兜底本地玩家实例。</summary>
    private INetworkPlayer _fallbackSelf;
    
    /// <summary>用于桥接网络事件到游戏状态同步引擎的同步管理器实例。</summary>
    private readonly ISynchronizationManager _synchronizationManager;

    #endregion

    #region 公共事件

    /// <summary>当成功连接到服务器时触发。参数为远程端点地址。</summary>
    public event Action<string> OnConnected;

    /// <summary>当与服务器断开时触发。参数为远程端点地址和断开原因。</summary>
    public event Action<string, string> OnDisconnected;

    /// <summary>当收到游戏同步事件时触发。参数为事件类型和负载数据。</summary>
    public event Action<string, object> OnGameEventReceived;

    /// <summary>当收到服务器系统响应时触发。参数为响应头和负载数据。</summary>
    public event Action<string, object> OnResponseReceived;

    /// <summary>当连接状态发生变化时触发。参数 true=已连接，false=已断开。</summary>
    public event Action<bool> OnConnectionStateChanged;

    #endregion

    #region 自动重连配置

    /// <summary>是否启用自动重连，默认关闭，由上层逻辑按需开启。</summary>
    private bool _autoReconnectEnabled = false;
    /// <summary>重连尝试间隔（毫秒），默认值取自 <see cref="NetworkConstants.ReconnectIntervalMs"/>。</summary>
    private int _retryInterval = NetworkConstants.ReconnectIntervalMs;
    /// <summary>
    /// 连接超时（毫秒），与服务端默认值 30s 保持一致。
    /// 取值过小会导致"结束回合→空闲→断连"的误判。
    /// </summary>
    private int _connectionTimeout = 30_000;
    /// <summary>上次成功连接的主机地址，用于断线后自动重连。</summary>
    private string _lastConnectHost;
    /// <summary>上次成功连接的端口号，用于断线后自动重连。</summary>
    private int _lastConnectPort;
    /// <summary>保护重连相关字段（<see cref="_autoReconnectEnabled"/>、<see cref="_reconnectTimer"/> 等）线程安全的锁对象。</summary>
    private readonly object _reconnectLock = new();
    /// <summary>用于调度自动重连尝试的定时器，仅在启用自动重连时创建。</summary>
    private Timer _reconnectTimer;

    #endregion

    #region Payload 日志去重

    /// <summary>
    /// 保护 Payload 预览日志去重相关字段的锁对象，避免高频事件导致日志刷屏。
    /// </summary>
    private readonly object _payloadPreviewLock = new();
    /// <summary>已打印过预览日志的事件类型集合，用于避免高频事件刷屏。</summary>
    private readonly HashSet<string> _payloadPreviewLogged = new(StringComparer.Ordinal);

    #endregion

    #region 构造函数

    /// <summary>
    /// 从配置管理器构造客户端，自动读取连接密钥和超时配置。
    /// </summary>
    /// <param name="configManager">配置管理器，提供连接密钥与网络超时等设置。</param>
    /// <param name="synchronizationManager">同步管理器，用于桥接网络事件到游戏状态同步引擎。</param>
    /// <remarks>
    /// 超时取自 <c>NetworkTimeoutSeconds</c>（最小 5s），心跳间隔取超时的 1/3（范围 1s~10s）。
    /// LiteNetLib 的 <c>DisconnectTimeout</c> 会在无任何数据包时触发断连判定。
    /// </remarks>
    public NetworkClient(ConfigManager configManager, ISynchronizationManager synchronizationManager)
        : this(configManager?.RelayServerConnectionKey?.Value ?? "LBoL_Network_Plugin", null, null, synchronizationManager)
    {
        try
        {
            if (configManager != null)
            {
                // 对齐客户端断连超时与服务端配置，避免超时不一致导致双方状态不同步。
                _connectionTimeout = Math.Max(5_000, configManager.NetworkTimeoutSeconds.Value * 1000);
                _netManager.DisconnectTimeout = _connectionTimeout;

                // 心跳频率应快于超时时间，设为超时的 1/3，同时限定范围避免日志过密集。
                _heartbeatIntervalMs = Math.Max(1_000, Math.Min(10_000, _connectionTimeout / 3));
            }
        }
        catch
        {
            // TODO: 当 configManager 某些属性被 BepInEx 包装为 ConfigEntry 时可能在访问 Value 前尚未初始化，需核实并处理。
        }
    }

    /// <summary>
    /// 使用指定参数初始化网络客户端。
    /// </summary>
    /// <param name="connectionKey">连接密钥，用于服务器身份验证。</param>
    /// <param name="networkManager">网络管理器实例，用于获取玩家信息。</param>
    /// <param name="networkPlayer">网络玩家实例，用于表示当前客户端玩家。</param>
    /// <param name="synchronizationManager">同步管理器实例，可选。</param>
    public NetworkClient(string connectionKey, INetworkManager networkManager, INetworkPlayer networkPlayer, ISynchronizationManager synchronizationManager = null)
    {
        _networkManager = networkManager; // 注入 NetworkManager 以便 GetSelf() 优先从中获取玩家信息（如服务器分配的 PlayerId）
        _connectionKey = connectionKey; // 连接密钥用于服务器验证，必须在 ConnectToServer 时通过 NetDataWriter 发送
        _listener = new EventBasedNetListener(); // LiteNetLib 的事件监听器，负责触发连接、断连和数据接收事件
        _netManager = new NetManager(_listener); // LiteNetLib 的核心网络管理器，负责 UDP 连接和数据传输
        _networkPlayer = networkPlayer; // 注入当前玩家实例，优先级次于 NetworkManager.GetSelf()，兜底 LocalNetworkPlayer
        _synchronizationManager = synchronizationManager; // 注入同步管理器以桥接网络事件到游戏状态同步引擎，允许在事件处理器中调用其方法
        RegisterEvents(); // 注册 LiteNetLib 事件处理器，覆盖连接、断连和数据接收三大核心事件
    }

    #endregion

    #region 连接管理

    /// <summary>
    /// 启动网络客户端，绑定本地 UDP 端口并开始监听。
    /// </summary>
    /// <exception cref="Exception">当 LiteNetLib 端口绑定失败时抛出。</exception>
    /// <remarks>必须在 <see cref="ConnectToServer"/> 之前调用；在游戏主循环中通过 <see cref="PollEvents"/> 驱动事件处理。</remarks>
    public void Start()
    {
        // 同步连接超时配置到 NetManager，确保 LiteNetLib 断连判定与客户端逻辑一致
        _netManager.DisconnectTimeout = _connectionTimeout;

        if (_netManager.Start())
        {
            Plugin.Logger?.LogInfo("[客户端] 网络客户端已启动。");
        }
        else
        {
            Plugin.Logger?.LogError("[客户端] 网络客户端启动失败。");
            throw new Exception("Failed to start NetworkClient");
        }
    }

    /// <summary>
    /// 获取当前客户端对应的本地玩家实例。
    /// </summary>
    /// <returns>
    /// 优先级：<see cref="INetworkManager.GetSelf"/> → 构造注入的 <c>_networkPlayer</c> →
    /// 懒创建的 <see cref="LocalNetworkPlayer"/> 兜底实例。
    /// </returns>
    public INetworkPlayer GetSelf()
    {
        try
        {
            // 优先从 NetworkManager 获取（联机时已携带服务器分配的 PlayerId）
            INetworkPlayer p = _networkManager?.GetSelf();
            if (p != null)
            {
                return p;
            }

            // 构造函数注入的玩家实例（如 LocalNetworkPlayer），用于离线模式或尚未联机时
            if (_networkPlayer != null)
            {
                return _networkPlayer;
            }

            // 兜底：懒创建一个只持有限连接信息的本地玩家
            // 使用 ??= 确保多线程环境下仅创建一次实例
            return _fallbackSelf ??= new LocalNetworkPlayer(this);
        }
        catch
        {
            // NetworkManager 或其依赖可能尚未完成初始化，返回兜底实例避免空引用
            return _fallbackSelf ??= new LocalNetworkPlayer(this);
        }
    }

    /// <summary>
    /// 注册 LiteNetLib 事件处理器，覆盖连接、断连和数据接收三大核心事件。
    /// </summary>
    /// <remarks>
    /// <para>连接事件流程：连接建立 → 触发 <see cref="OnConnected"/> → 通知 <see cref="ISynchronizationManager.OnConnectionRestored"/> → 发送 PlayerJoined。</para>
    /// <para>断连事件流程：连接断开 → 清空 <c>_serverPeer</c> → 触发 <see cref="OnDisconnected"/> → 通知 <see cref="ISynchronizationManager.OnConnectionLost"/> → 按需启动自动重连。</para>
    /// <para>数据接收流程：读取 messageType → 路由到 <see cref="HandleGameEvent"/> / <see cref="HandleRequestResponse"/> / 心跳消费。</para>
    /// </remarks>
    private void RegisterEvents()
    {
        _listener.PeerConnectedEvent += peer =>
        {
            Plugin.Logger?.LogInfo($"[客户端] 已连接到服务器: {peer.EndPoint}");
            // 保存对端引用，后续发送数据依赖此字段
            _serverPeer = peer;
            _lastHeartbeatSentUtc = DateTime.UtcNow;// 重置心跳时间戳，避免连接恢复后误判心跳间隔过长导致立即发送心跳
            // 连接成功后立即停止可能正在运行的重连定时器
            StopAutoReconnectTimer_NoThrow();

            // 触发连接事件通知
            OnConnected?.Invoke(peer.EndPoint.ToString());
            OnConnectionStateChanged?.Invoke(true);

            // 通知同步管理器：连接已恢复，以便其重建同步上下文
            try
            {
                _synchronizationManager?.OnConnectionRestored();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"[客户端] 通知同步管理器失败: {ex.Message}");
            }

            // 获取当前玩家信息，优先从 NetworkManager 取，回退到注入的 _networkPlayer
            string playerName = null;
            try
            {
                playerName = _networkManager?.GetSelf()?.userName;
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    playerName = _networkPlayer?.userName;
                }
            }
            catch
            {
                // NetworkManager 可能尚未完成初始化，忽略
            }

            // 玩家名不可为空，否则服务器无法区分客户端
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Player";
            }

            string characterId = null;
            try
            {
                // 使用 ModelName（而非 DisplayName），可直接用于加载头像/模型资源
                characterId = GameStateUtils.GetCurrentPlayer()?.ModelName;
            }
            catch
            {
                // 游戏流程可能尚未进入（如在主菜单），忽略
            }

            // 构造玩家信息匿名对象：使用匿名类型可自动序列化为 JSON，无需显式 DTO
            // ConnectionTime 使用 Ticks 而非 DateTime，避免跨时区序列化问题
            var playerInfo = new
            {
                PlayerName = playerName, // 玩家显示名，服务器用于广播和 UI 展示
                CharacterId = characterId, // 角色模型标识，用于加载头像和外观资源；可能为 null（主菜单时）
                ConnectionTime = DateTime.Now.Ticks // 本地连接时间戳，服务器用于计算在线时长
            };

            // 向服务器广播本客户端的加入信息
            // PlayerJoined 是服务器识别新客户端并为其分配 Slot 的关键事件
            SendGameEventData(NetworkMessageTypes.PlayerJoined, playerInfo);
        };

        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Plugin.Logger?.LogWarning($"[客户端] 已从服务器断开: {peer.EndPoint}, 原因: {disconnectInfo.Reason}");
            // 清空对端引用，标记为离线状态；后续 IsConnected 将返回 false
            _serverPeer = null;
            // 重置心跳时间戳到哨兵值，避免重连后误判心跳间隔过长导致立即重发
            _lastHeartbeatSentUtc = DateTime.MinValue;

            // 触发断连事件通知
            OnDisconnected?.Invoke(peer.EndPoint.ToString(), disconnectInfo.Reason.ToString());
            OnConnectionStateChanged?.Invoke(false);

            // 通知同步管理器：连接已丢失，以便其暂停同步并进入降级模式
            try
            {
                _synchronizationManager?.OnConnectionLost();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogWarning($"[客户端] 通知同步管理器失败: {ex.Message}");
            }

            // 若已启用自动重连，则启动周期性重连定时器
            if (_autoReconnectEnabled)
            {
                Plugin.Logger?.LogInfo($"[客户端] 已启用自动重连，将在 {_retryInterval}ms 后重试");
                StartAutoReconnectTimer_NoThrow();
            }
        };

        // 数据接收事件：根据消息类型分发到不同的处理器
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            try
            {
                // 读取消息类型标识：这是每个数据包的第一个字段，决定后续路由逻辑
                string messageType = dataReader.GetString();

                // 根据消息类型分发给不同的处理器
                // 优先级：游戏事件 > 心跳响应 > 系统响应 > 未知类型
                if (IsGameEvent(messageType))
                {
                    // 处理同步事件：解析 JSON 并投递到同步管理器
                    HandleGameEvent(messageType, dataReader);
                }
                else if (string.Equals(messageType, NetworkMessageTypes.HeartbeatResponse, StringComparison.Ordinal))
                {
                    // 心跳响应无需业务处理，仅消费掉数据包以避免被 LiteNetLib 误判为未响应而断连
                    // 服务器通过收到任意数据包来刷新超时计时器，因此空消费即完成保活
                    _ = dataReader.GetString();
                }
                else if (string.Equals(messageType, NetworkMessageTypes.GetSelf_RESPONSE, StringComparison.Ordinal))
                {
                    // 处理系统响应消息：如 GetSelf 返回的玩家 ID 和初始状态
                    HandleRequestResponse(fromPeer, dataReader);
                }
                else
                {
                    Plugin.Logger?.LogWarning($"[客户端] 未知消息类型: {messageType}，来自 {fromPeer.EndPoint}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[客户端] 处理来自 {fromPeer.EndPoint} 的数据失败: {ex.Message}");
            }
            finally
            {
                // 回收数据读取器，避免内存泄漏；LiteNetLib 使用对象池复用 NetDataReader
                dataReader.Recycle();
            }
        };
    }

    /// <summary>

    /// 判断消息类型是否为游戏同步事件。
    /// </summary>
    /// <param name="messageType">从数据包中读取的消息类型标识。</param>
    /// <returns>若是游戏事件（如 On*、Mana*、Battle*、StateSyncResponse 等）返回 true，否则 false。</returns>
    /// <remarks>
    /// 委托 <see cref="NetworkMessageTypes.IsGameEvent"/> 按客户端路由表判断，避免在客户端硬编码事件名列表。
    /// </remarks>
    private bool IsGameEvent(string messageType)
    {
        return NetworkMessageTypes.IsGameEvent(messageType, NetworkMessageTypes.GameEventRoute.Client);
    }

    // 说明：FullStateSyncResponse 的“客户端落地”目前由 MidGameJoinManager 通过 DirectMessage
    // 路线完成（Apply FullSnapshot + Replay MissedEvents）。若后续需要支持“非 DirectMessage 路由”
    // 的 FullStateSyncResponse，可在此处补齐通用落地逻辑。

    /// <summary>
    /// 向本地注入一条“伪接收”的 GameEvent（用于回放/追赶/调试）。
    /// 注意：不会向服务器发送任何数据，仅触发本地订阅者（Patch/Manager 等）。
    /// </summary>
    /// <param name="eventType">要注入的事件类型标识，如 <code>StateSync</code>。</param>
    /// <param name="payload">事件负载数据；可为任意对象，由下游订阅者自行解析。</param>
    /// <remarks>
    /// 典型使用场景：断线重连后的历史事件回放、本地调试模拟服务器推送、
    /// MidGameJoinManager 通过 DirectMessage 应用 FullSnapshot 后重放缓存事件。
    /// </remarks>
    public void InjectLocalGameEvent(string eventType, object payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        try
        {
            OnGameEventReceived?.Invoke(eventType, payload);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[客户端] 注入本地游戏事件失败: type={eventType}, err={ex.Message}");
        }
    }

    /// <summary>
    /// 处理从服务器收到的游戏同步事件，解析 JSON 负载并触发本地事件链。
    /// </summary>
    /// <param name="eventType">事件类型标识。</param>
    /// <param name="dataReader">LiteNetLib 数据读取器，当前位置已指向 JSON 负载。</param>
    /// <remarks>
    /// 将负载保持为 JSON 字符串而非强类型对象：下游大多通过 <c>JsonElement</c> 或 <c>string</c> 路径解析，
    /// 避免反序列化成不兼容的对象形状（尤其是 DTO 版本不一致时）。
    /// 事件随后被投递到 <see cref="ISynchronizationManager.ProcessEventFromNetwork"/> 进行状态同步。
    /// </remarks>
    private void HandleGameEvent(string eventType, NetDataReader dataReader)
    {
        try
        {
            // 读取并解析 JSON 格式的事件数据
            string jsonPayload = dataReader.GetString();
            // 保持为 JSON 字符串：下游大多通过 JsonElement/string 路径解析，避免反序列化成不兼容的对象形状。
            object eventData = jsonPayload;

            LogPayloadPreviewOnce(eventType, jsonPayload);

            Plugin.Logger?.LogInfo($"[客户端] 收到游戏事件: {eventType}");
            // 高频：仅 Debug，避免刷屏。
            Plugin.Logger?.LogDebug($"[客户端] 收到游戏事件: {eventType}");

            // 触发游戏事件接收事件
            OnGameEventReceived?.Invoke(eventType, eventData);

            // 将事件传递给同步管理器处理
            _synchronizationManager?.ProcessEventFromNetwork(new
            {
                EventType = eventType,
                Payload = eventData,
                Timestamp = DateTime.Now.Ticks
            });
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[客户端] 处理游戏事件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 对每种事件类型仅打印一次 Payload 预览日志，用于调试时快速定位格式问题。
    /// </summary>
    /// <param name="eventType">事件类型标识。</param>
    /// <param name="jsonPayload">原始 JSON 负载。</param>
    /// <remarks>
    /// 使用 <c>_payloadPreviewLogged</c> 哈希集合做去重，避免高频事件（如 StateSync）刷屏。
    /// 摘要由 <see cref="NetLogHelper.BuildSummary"/> 生成，通常截断到前 256 字符。
    /// </remarks>
    private void LogPayloadPreviewOnce(string eventType, string jsonPayload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            eventType = "<unknown>";
        }

        lock (_payloadPreviewLock)
        {
            // 已记录过该类型则直接跳过，保证控制台清爽
            if (!_payloadPreviewLogged.Add(eventType))
            {
                return;
            }
        }

        // 仅打印一次：用于定位“无效的网络事件数据格式”根因
        string summary = NetLogHelper.BuildSummary(eventType, jsonPayload);
        Plugin.Logger?.LogInfo($"[客户端] Payload 预览(仅一次): event={eventType}, {summary}");
        try
        {
            // 双重保险：某些 Logger 实现可能在异步上下文抛异常，再试一次
            Plugin.Logger?.LogInfo($"[客户端] Payload 预览(仅一次): event={eventType}, {summary}");
        }
        catch
        {
            // 日志失败不应中断网络数据接收流程，吞掉异常继续执行
        }
    }

    /// <summary>
    /// 连接到指定的游戏服务器。
    /// 使用连接密钥进行身份验证，建立可靠的 UDP 连接。
    /// </summary>
    /// <param name="host">服务器主机地址或域名。</param>
    /// <param name="port">服务器监听端口号。</param>
    /// <remarks>
    /// 将连接密钥通过 <see cref="NetDataWriter"/＞ 附加到连接请求数据包中，
    /// 服务器在 <code>OnConnectionRequest</code＞ 中验证该密钥。
    /// 调用后会更新 <see cref="_lastConnectHost"/＞ 和 <see cref="_lastConnectPort"/＞，
    /// 供后续自动重连使用。
    /// </remarks>
    public void ConnectToServer(string host, int port)
    {
        _lastConnectHost = host;
        _lastConnectPort = port;

        string keyToUse = _connectionKey;
        try
        {
            var provider = NetworkPlugin.Network.Services.ModService.ServiceProvider;
            if (provider != null)
            {
                var config = provider.GetService(typeof(NetworkPlugin.Configuration.ConfigManager)) as NetworkPlugin.Configuration.ConfigManager;
                if (config != null)
                {
                    if (NetworkPlugin.Patch.UI.MainMenuMultiplayerEntryPatch.IsLocalServerRunning)
                    {
                        keyToUse = config.HostConnectionKey?.Value ?? "LBoL_Network_Plugin";
                    }
                    else
                    {
                        keyToUse = config.RelayServerConnectionKey?.Value ?? "LBoL_Network_Plugin";
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        Plugin.Logger?.LogInfo($"[客户端] 正在连接服务器 {host}:{port}（密钥: <已隐藏>）...");
        NetDataWriter connectData = new();
        // 将连接密钥写入数据包，用于服务器身份验证
        connectData.Put(keyToUse);
        // 发起连接请求，包含认证信息
        _netManager.Connect(host, port, connectData);
    }

    /// <summary>
    /// 轮询网络事件并驱动心跳发送。
    /// 必须在游戏主循环（如 Unity Update）中定期调用，否则 LiteNetLib 不会处理任何网络数据。
    /// </summary>
    /// <remarks>
    /// 心跳（Heartbeat）是服务器侧会话保活的必要条件；未收到心跳的客户端将被视为离线。
    /// 心跳在 <c>_main thread</c> 上发送，不可在后台线程替代。
    /// </remarks>
    public void PollEvents()
    {
        // 轮询 LiteNetLib 内部队列，触发 PeerConnected/PeerDisconnected/NetworkReceive 等回调
        // 必须在游戏主线程（如 Unity Update）中定期调用，否则网络事件不会处理
        _netManager.PollEvents();

        // 心跳在服务器侧作为会话保活信号，必须随主线程定期发送
        // 若长时间未发心跳，服务器会判定客户端离线并主动断开连接
        SendHeartbeatIfNeeded_NoThrow();
    }

    /// <summary>
    /// 条件式发送心跳包到服务器。使用 <see cref="DeliveryMethod.Unreliable"/> 以降低带宽开销。
    /// </summary>
    /// <remarks>
    /// 间隔由 <c>_heartbeatIntervalMs</c> 控制，默认取连接超时的 1/3（1s~10s）。
    /// 采用 "_NoThrow" 后缀约定：内部吞掉所有异常，避免心跳异常导致游戏主循环崩溃。
    /// </remarks>
    private void SendHeartbeatIfNeeded_NoThrow()
    {
        try
        {
            // 未建立完整连接时不发心跳，避免向 null Peer 发送导致 NullReferenceException
            if (!IsConnected || _serverPeer == null)
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            // 若距离上次心跳不足间隔，跳过本次（典型间隔 5s~10s）
            // 使用 MinValue 作为哨兵值，表示"尚未发送过心跳"，首次连接后立即发送
            if (_lastHeartbeatSentUtc != DateTime.MinValue &&
                (now - _lastHeartbeatSentUtc).TotalMilliseconds < _heartbeatIntervalMs)
            {
                return;
            }

            NetDataWriter writer = new();
            writer.Put(NetworkMessageTypes.Heartbeat);
            // Unreliable：心跳允许丢包，服务器超时窗口内只要收到一次即可保活
            // 使用 Unreliable 可降低带宽开销，避免 ReliableOrdered 的 ACK 流量
            _serverPeer.Send(writer, DeliveryMethod.Unreliable);
            _lastHeartbeatSentUtc = now;
        }
        catch
        {
            // 心跳失败不应中断游戏主循环；LiteNetLib 会在底层自动处理重传/重连
        }
    }

    /// <summary>
    /// 停止网络客户端，断开所有连接并释放资源。
    /// </summary>
    public void Stop()
    {
        // 先停止重连定时器，避免 Stop 后仍在后台尝试重连导致资源泄漏
        StopAutoReconnectTimer_NoThrow();
        _netManager.Stop();
        _serverPeer = null;
        Plugin.Logger?.LogInfo("[客户端] 网络客户端服务已停止。");
    }

    /// <summary>
    /// 启动自动重连的周期性 Timer。
    /// </summary>
    /// <remarks>
    /// 每次触发时检查 <see cref="IsConnected"/> 与 <see cref="IsConnecting"/>，仅在双假时调用 <see cref="ConnectToServer"/>。
    /// 使用 <c>_reconnectLock</c> 保证线程安全，避免重复创建 Timer。
    /// </remarks>
    private void StartAutoReconnectTimer_NoThrow()
    {
        try
        {
            // 若未启用自动重连，则不创建 Timer，直接返回
            if (!_autoReconnectEnabled)
            {
                return;
            }

            // 缺少上次连接地址时无法重连，常见于首次连接失败后立即断线
            if (string.IsNullOrWhiteSpace(_lastConnectHost) || _lastConnectPort <= 0)
            {
                Plugin.Logger?.LogInfo("[客户端] 自动重连跳过：缺少上次连接的地址信息");
                return;
            }

            lock (_reconnectLock)
            {
                // 释放旧 Timer（可能来自之前的断连），再创建新的周期定时器
                // 避免重复创建导致多个定时器同时触发重连
                _reconnectTimer?.Dispose();
                _reconnectTimer = new Timer(_ =>
                {
                    try
                    {
                        if (IsConnected || IsConnecting)
                        {
                            return;
                        }

                        Plugin.Logger?.LogInfo($"[客户端] 自动重连尝试：{_lastConnectHost}:{_lastConnectPort}");
                        ConnectToServer(_lastConnectHost, _lastConnectPort);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger?.LogError($"[客户端] 自动重连异常: {ex.Message}");
                    }
                }, null, _retryInterval, _retryInterval);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[客户端] 启动自动重连计时器失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 释放并重置自动重连 Timer。采用 "_NoThrow" 后缀：内部吞掉所有异常。
    /// </summary>
    private void StopAutoReconnectTimer_NoThrow()
    {
        try
        {
            lock (_reconnectLock)
            {
                _reconnectTimer?.Dispose();
                _reconnectTimer = null;
            }
        }
        catch
        {
            // Timer 释放失败不应影响断连流程的继续执行
        }
    }

    #endregion

    #region 连接属性

    /// <summary>
    /// 获取客户端是否已连接到服务器。
    /// 检查 <c>_serverPeer</c> 存在且 LiteNetLib 状态为 <see cref="ConnectionState.Connected"/>。
    /// </summary>
    /// <remarks>
    /// 该属性在断连事件触发后立即返回 false，可用于 UI 状态指示器和发送前的连接检查。
    /// </remarks>
    public bool IsConnected => _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected;

    /// <summary>
    /// 客户端是否处于“正在连接”状态。
    /// 当尚未建立完整连接但 <c>_netManager</c> 已存在活跃 Peer 时返回 true。
    /// </summary>
    /// <remarks>
    /// 用于 UI 展示"连接中..."状态，避免在连接建立过程中重复发起新连接。
    /// </remarks>
    public bool IsConnecting => !IsConnected && _netManager?.FirstPeer != null;

    /// <summary>
    /// 当前往返延迟（RTT，毫秒）。
    /// 由 LiteNetLib 内部根据 ACK 时间自动计算；断连时返回 9999，便于 UI 区分"极差/离线"与正常延迟。
    /// </summary>
    /// <remarks>
    /// 9999 是人为设定的哨兵值，便于 UI 统一处理"未连接"和"延迟极高"的展示逻辑。
    /// </remarks>
    public int Ping => _serverPeer?.Ping ?? 9999;

    /// <summary>
    /// 本地 UDP 端点。若客户端尚未启动则返回 null。
    /// </summary>
    /// <remarks>
    /// 返回 <code>IPAddress.Any</code> 加本地端口号，表示监听所有网络接口。
    /// 在 NAT 穿透场景下，此端点可用于告知对端回连地址。
    /// </remarks>
    public IPEndPoint LocalEndPoint
    {
        get
        {
            try
            {
                if (_netManager == null || _netManager.LocalPort <= 0)
                {
                    return null;
                }

                return new IPEndPoint(IPAddress.Any, _netManager.LocalPort); // LiteNetLib 监听所有接口，端口由 LocalPort 提供
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 远程服务器端点。未连接时返回 null。
    /// </summary>
    /// <remarks>
    /// 即 <see cref="_serverPeer.EndPoint"/＞ 的快捷访问，用于日志和调试展示。
    /// </remarks>
    public IPEndPoint RemoteEndPoint => _serverPeer?.EndPoint;

    #endregion

    #region 数据发送与响应处理

    /// <summary>
    /// 发送游戏同步事件到服务器，数据使用 JSON 格式序列化。
    /// </summary>
    /// <param name="eventType">事件类型标识符，如 <c>PlayerJoined</c>、<c>StateSync</c>。</param>
    /// <param name="eventData">要发送的事件数据对象；可为匿名对象或 DTO。</param>
    /// <remarks>
    /// 使用 <see cref="DeliveryMethod.ReliableOrdered"/> 保证消息按序到达，适用于游戏状态同步。
    /// 未连接时直接返回并打印警告，避免 NullReferenceException。
    /// </remarks>
    public void SendGameEventData(string eventType, object eventData)
    {
        // 未连接时直接返回，避免向 null _serverPeer 发送导致 NullReferenceException
        if (!IsConnected)
        {
            Plugin.Logger?.LogWarning($"[客户端] 未连接到服务器，无法发送事件: {eventType}");
            return;
        }

        try
        {
            // 统一使用 JsonCompat 序列化，确保与服务器端反序列化兼容
            // JsonCompat 内部处理循环引用和自定义转换器，避免 Newtonsoft.Json 默认行为不一致
            string json = JsonCompat.Serialize(eventData);
            NetDataWriter writer = new();
            writer.Put(eventType);
            writer.Put(json);

            // ReliableOrdered：游戏事件必须按序到达，否则会导致状态错乱（如先收到伤害再收到回血）
            _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            string summary = NetLogHelper.BuildSummary(eventType, json);
            Plugin.Logger?.LogDebug($"[客户端] 已发送游戏事件: {eventType} ({summary})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[客户端] 发送游戏事件失败: {eventType}, 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送通用网络请求到服务器，支持原始类型和复杂 JSON 对象。
    /// </summary>
    /// <typename="T">请求数据类型。</typeparam>
    /// <param name="requestHeader">请求头标识符，用于服务器路由。</param>
    /// <param name="requestData">请求数据；原始类型直接写入，复杂对象序列化为 JSON。</param>
    /// <exception cref="NotSupportedException">当 T 为不支持的原始类型时抛出。</exception>
    /// <remarks>
    /// 原始类型通过 <c>switch</c> 直接映射到 <see cref="NetDataWriter.Put"/> 重载，
    /// 复杂对象统一走 JSON 路径，确保跨语言/跨版本兼容。
    /// </remarks>
    public void SendRequest<T>(string requestHeader, T requestData)
    {
        // 未连接时直接返回，避免 NullReferenceException
        if (!IsConnected)
        {
            Plugin.Logger?.LogInfo("[客户端] 未连接到服务器，无法发送请求。");
            return;
        }

        NetDataWriter writer = new();
        writer.Put(requestHeader);

        string payloadForLog = null;

        // 原始类型分支：直接写入 NetDataWriter，避免 JSON 包装带来的额外字节开销和序列化耗时
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
        {
            switch (requestData)
            {
                case float f: writer.Put(f); break;   // 单精度浮点数：4 字节，用于网络同步中的坐标/角度等
                case double d: writer.Put(d); break;  // 双精度浮点数：8 字节，用于需要高精度的数值
                case long l: writer.Put(l); break;    // 64 位整数：8 字节，用于时间戳或大 ID
                case int i: writer.Put(i); break;     // 32 位整数：4 字节，最常用，用于枚举/计数/状态码
                case string s: writer.Put(s); break;   // 字符串：先写长度（VarInt）再写 UTF-8 字节
                case bool b: writer.Put(b); break;    // 布尔值：1 字节，用于标志位
                default: throw new NotSupportedException($"Type {typeof(T)} is not supported by NetDataWriter.Put");
            }

            try
            {
                payloadForLog = requestData?.ToString();
            }
            catch
            {
                payloadForLog = string.Empty;
            }
        }
        else
        {
            // 复杂对象分支：序列化为 JSON 后写入，保持与游戏事件的统一格式
            string json = JsonCompat.Serialize(requestData);
            writer.Put(json);
            payloadForLog = json;
        }

        _serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        payloadForLog ??= string.Empty;

        string summary = NetLogHelper.BuildSummary(requestHeader, payloadForLog);
        Plugin.Logger?.LogDebug($"[客户端] 已发送请求: {requestHeader} ({summary})");
    }

    /// <summary>
    /// 处理服务器响应数据，解析系统响应消息（如 GetSelf_RESPONSE）。
    /// </summary>
    /// <param name="fromPeer">发送响应的对等端。</param>
    /// <param name="dataReader">响应数据读取器，包含 responseHeader + responseData。</param>
    /// <remarks>
    /// 从 <code>dataReader</code＞ 中按顺序读取字符串：第一个是响应头，第二个是负载 JSON。
    /// 解析后通过 <see cref="OnResponseReceived"/＞ 事件分发给下游订阅者。
    /// </remarks>
    /// <param name="dataReader">响应数据读取器，包含 responseHeader + responseData。</param>
    /// <remarks>
    /// 从 <c>dataReader</c> 中按顺序读取字符串：第一个是响应头，第二个是负载 JSON。
    /// 解析后通过 <see cref="OnResponseReceived"/> 事件分发给下游订阅者。
    /// </remarks>
    private void HandleRequestResponse(NetPeer fromPeer, NetDataReader dataReader)
    {
        try
        {
            // 顺序读取：LiteNetLib 数据包中的字段顺序必须与写入顺序一致
            string responseHeader = dataReader.GetString();
            Plugin.Logger?.LogInfo($"[客户端] 收到响应: type='{responseHeader}', from={fromPeer.EndPoint}");

            string responseData = dataReader.GetString();
            Plugin.Logger?.LogInfo($"[客户端] 响应消息: {responseData}");

            // 触发事件，由 NetworkIdentityTracker / ReconnectionManager 等订阅者消费
            OnResponseReceived?.Invoke(responseHeader, responseData);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[客户端] 处理响应失败: {ex.Message}");
        }
    }

    #endregion

    #region 高级功能实现

    /// <summary>
    /// 获取当前网络连接的统计信息和质量指标。
    /// </summary>
    /// <returns>
    /// 未连接时返回 <c>{ Status = "Not Connected" }</c>；
    /// 已连接时返回包含延迟、MTU、连接时间、地址等信息的匿名对象。
    /// </returns>
    /// <remarks>
    /// 供 UI 面板（如联机状态 HUD）调用，展示实时网络质量。
    /// </remarks>
    public object GetConnectionStats()
    {
        // 无活跃 Peer 时返回离线状态，避免访问 null 属性导致 NullReferenceException
        if (_serverPeer == null)
        {
            return new { Status = "Not Connected" };
        }

        // 提取 LiteNetLib 内置的网络质量指标，供 UI 面板展示
        return new
        {
            Status = "Connected",
            Ping = _serverPeer.Ping, // 往返延迟（毫秒），由 LiteNetLib 根据 ACK 时间自动计算
            Mtu = _serverPeer.Mtu, // 当前路径最大传输单元（字节），动态协商得出
            ConnectionTime = DateTime.Now.Ticks, // 连接建立时间戳（Ticks），用于计算在线时长
            Address = _serverPeer.EndPoint.ToString() // 远程服务器地址字符串，便于日志和 UI 展示
        };
    }

    /// <summary>
    /// 设置 LiteNetLib 连接超时时间。
    /// </summary>
    /// <param name="timeoutMs">超时时间（毫秒），建议与服务端配置保持一致。</param>
    /// <remarks>
    /// 同时更新本地字段 <c>_connectionTimeout</c> 和 <c>_netManager.DisconnectTimeout</c>，
    /// 确保后续连接与当前运行实例均使用新值。
    /// </remarks>
    public void SetConnectionTimeout(int timeoutMs)
    {
        _connectionTimeout = timeoutMs;
        // 同步更新已启动的 NetManager，否则旧实例仍使用上一次配置导致超时不一致
        if (_netManager != null) _netManager.DisconnectTimeout = timeoutMs;
        Plugin.Logger?.LogInfo($"[客户端] 连接超时已设置为 {timeoutMs}ms");
    }

    /// <summary>
    /// 启用或禁用自动重连功能。
    /// </summary>
    /// <param name="enabled">true 启用自动重连，false 禁用。</param>
    /// <param name="retryInterval">重试间隔（毫秒），默认 <see cref="NetworkConstants.ReconnectIntervalMs"/> (5000ms)。</param>
    /// <remarks>
    /// 启用后，断连时会自动创建周期性 Timer 尝试恢复连接；
    /// 禁用时会清理现有 Timer，避免后台资源泄漏。
    /// </remarks>
    public void EnableAutoReconnect(bool enabled, int retryInterval = NetworkConstants.ReconnectIntervalMs)
    {
        _autoReconnectEnabled = enabled;
        _retryInterval = retryInterval;
        Plugin.Logger?.LogInfo($"[客户端] 自动重连{(enabled ? "已启用" : "已禁用")}, 重试间隔: {retryInterval}ms");

        // 若禁用自动重连，立即清理现有定时器，避免后台资源泄漏和意外重连行为
        if (!enabled)
        {
            StopAutoReconnectTimer_NoThrow();
        }
    }



    #endregion
}

#endregion
