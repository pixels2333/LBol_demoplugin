// 服务器公共内核：统一 LiteNetLib 生命周期、连接鉴权、入站消息队列、心跳/超时清理等基础能力。
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using LiteNetLib;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 服务器公共内核
/// 统一 LiteNetLib 生命周期、连接鉴权、入站消息队列、心跳/超时清理等基础能力
/// </summary>
public sealed class ServerCore : IServerCore
{
    private readonly ServerMessageQueue _messageQueue = new ServerMessageQueue();
    private readonly Dictionary<NetPeer, CorePeerSession> _sessions = new Dictionary<NetPeer, CorePeerSession>();
    private readonly object _lock = new object();

    private Thread? _thread;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    /// <inheritdoc />
    public ServerOptions Options { get; }
    /// <inheritdoc />
    public IServerLogger Logger { get; }
    /// <inheritdoc />
    public IServerMessageCodec Codec { get; }

    /// <inheritdoc />
    public EventBasedNetListener Listener { get; }
    /// <inheritdoc />
    public NetManager NetManager { get; }

    /// <inheritdoc />
    public event Action<NetPeer>? PeerConnected;
    /// <inheritdoc />
    public event Action<NetPeer, DisconnectInfo>? PeerDisconnected;
    /// <inheritdoc />
    public event Action<NetPeer, int>? PeerLatencyUpdated;
    /// <inheritdoc />
    public event Action<ServerInboundMessage>? MessageReceived;

    /// <summary>
    /// 初始化服务器内核
    /// </summary>
    /// <param name="options">服务器配置选项</param>
    /// <param name="logger">日志记录器（可选，默认使用空日志）</param>
    /// <param name="codec">消息编解码器（可选，默认使用 JSON 编解码）</param>
    public ServerCore(ServerOptions options, IServerLogger? logger = null, IServerMessageCodec? codec = null)
    {
        Options = options ?? new ServerOptions();
        Logger = logger ?? new NullServerLogger();
        Codec = codec ?? new DefaultServerMessageCodec();

        Listener = new EventBasedNetListener();
        NetManager = new NetManager(Listener)
        {
            UnsyncedEvents = true,
            AutoRecycle = true,
            DisconnectTimeout = Options.DisconnectTimeoutMs,
            PingInterval = Options.PingIntervalMs,
        };

        RegisterCoreEvents();
    }

    /// <inheritdoc />
    public void Start()
    {
        if (_isRunning)
        {
            Logger.Warn("[ServerCore] Server is already running");
            return;
        }

        NetManager.Start(Options.Port);
        _isRunning = true;
        Logger.Info($"[ServerCore] Server started on port {Options.Port}");

        if (Options.UseBackgroundThread)
        {
            _cts = new CancellationTokenSource();
            _thread = new Thread(RunLoop)
            {
                IsBackground = true,
                Name = "ServerCore Main Loop"
            };
            _thread.Start();
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { /* cancellation source already disposed */ }
        catch (Exception ex) { Logger?.Warn($"[ServerCore] 取消 CancellationTokenSource 失败: {ex.Message}"); }

        try
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Join(500);
            }
        }
        catch
        {
            // ignore
        }

        NetManager.Stop();

        lock (_lock)
        {
            _sessions.Clear();
        }

        Logger.Info("[ServerCore] Server stopped");
    }

    /// <inheritdoc />
    public void PollEvents()
    {
        NetManager.PollEvents();
        DrainMessages();
        CleanupTimeoutConnections();
    }

    /// <inheritdoc />
    public bool TryGetSession(NetPeer peer, out CorePeerSession? session)
    {
        lock (_lock)
        {
            return _sessions.TryGetValue(peer, out session);
        }
    }

    /// <inheritdoc />
    public void MarkPeerSeen(NetPeer peer)
    {
        if (peer == null)
        {
            return;
        }

        lock (_lock)
        {
            if (_sessions.TryGetValue(peer, out var session))
            {
                session.MarkSeen(DateTime.UtcNow);
            }
        }
    }

    /// <summary>
    /// 注册 LiteNetLib 事件回调
    /// </summary>
    private void RegisterCoreEvents()
    {
        Listener.ConnectionRequestEvent += request =>
        {
            try
            {
                if (NetManager.PeersCount >= Options.MaxConnections)
                {
                    request.Reject();
                    Logger.Warn("[ServerCore] Rejecting connection: max connections reached");
                    return;
                }

                if (!string.IsNullOrEmpty(Options.ConnectionKey))
                {
                    string providedKey = request.Data.GetString();
                    if (!string.Equals(providedKey, Options.ConnectionKey, StringComparison.Ordinal))
                    {
                        request.Reject();
                        Logger.Warn("[ServerCore] Rejecting connection: invalid key");
                        return;
                    }
                }

                request.Accept();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[ServerCore] Error handling connection request");
                try { request.Reject(); }
                catch (ObjectDisposedException) { /* peer already disconnected */ }
                catch (Exception rejectEx) { Logger?.Warn($"[ServerCore] 拒绝连接请求失败: {rejectEx.Message}"); }
            }
        };

        Listener.PeerConnectedEvent += peer =>
        {
            lock (_lock)
            {
                CorePeerSession session = new CorePeerSession
                {
                    Peer = peer,
                    ConnectedAt = DateTime.UtcNow
                };
                session.MarkSeen(DateTime.UtcNow);
                _sessions[peer] = session;
            }

            PeerConnected?.Invoke(peer);
        };

        Listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            lock (_lock)
            {
                _sessions.Remove(peer);
            }

            PeerDisconnected?.Invoke(peer, disconnectInfo);
        };

        Listener.NetworkLatencyUpdateEvent += (peer, latency) =>
        {
            lock (_lock)
            {
                if (_sessions.TryGetValue(peer, out var session))
                {
                    session.Ping = latency;
                }
            }

            PeerLatencyUpdated?.Invoke(peer, latency);
        };

        Listener.NetworkReceiveEvent += (fromPeer, reader, deliveryMethod) =>
        {
            try
            {
                if (!Codec.TryDecode(reader, out string messageType, out string jsonPayload))
                {
                    return;
                }

                MarkPeerSeen(fromPeer);

                var priority = MessagePriorities.GetPriority(messageType);
                _messageQueue.Enqueue(
                    new ServerInboundMessage(fromPeer, messageType, jsonPayload, deliveryMethod, priority),
                    Options.MaxQueueSize);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[ServerCore] Error receiving message");
            }
            finally
            {
                reader.Recycle();
            }
        };
    }

    /// <summary>
    /// 排空消息队列，依次触发每个消息的接收回调
    /// </summary>
    private void DrainMessages()
    {
        int handled = 0;
        while (handled < Options.MaxMessagesPerTick && _messageQueue.TryDequeueHighest(out var message))
        {
            try
            {
                MessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[ServerCore] Error processing message: {message.Type}");
            }
            finally
            {
                handled++;
            }
        }
    }

    /// <summary>
    /// 清理超时连接，断开长时间无活动的对等端
    /// </summary>
    private void CleanupTimeoutConnections()
    {
        const int defaultTimeoutSeconds = 30;
        int timeoutSeconds = Math.Max(1, Options.DisconnectTimeoutMs / 1000);

        List<NetPeer>? timeoutPeers = null;
        DateTime now = DateTime.UtcNow;

        lock (_lock)
        {
            foreach (var kvp in _sessions)
            {
                var session = kvp.Value;
                if (session.IsTimeout(timeoutSeconds <= 0 ? defaultTimeoutSeconds : timeoutSeconds, now))
                {
                    if (timeoutPeers == null)
                    {
                        timeoutPeers = new List<NetPeer>();
                    }

                    timeoutPeers.Add(kvp.Key);
                }
            }
        }

        if (timeoutPeers == null)
        {
            return;
        }

        foreach (var peer in timeoutPeers)
        {
            try
            {
                Logger.Warn($"[ServerCore] Disconnecting timeout peer: {peer.EndPoint}");
                peer.Disconnect();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[ServerCore] Failed to disconnect timeout peer");
            }
        }
    }

    /// <summary>
    /// 后台线程主循环，持续轮询网络事件
    /// </summary>
    private void RunLoop()
    {
        CancellationToken token = _cts != null ? _cts.Token : CancellationToken.None;
        while (_isRunning && !token.IsCancellationRequested)
        {
            try
            {
                PollEvents();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[ServerCore] Error in server loop");
            }

            Thread.Sleep(Math.Max(1, Options.BackgroundThreadSleepMs));
        }
    }
}
