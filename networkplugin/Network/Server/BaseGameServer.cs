using System;
using System.Collections.Generic;
using LiteNetLib;
using NetworkPlugin.Network.Server.Core;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// 基于 <see cref="IServerCore"/> 构建的业务层服务器骨架。
/// 集中管理会话生命周期（连接/断线/延迟）、心跳/消息时间戳、
/// 重连令牌管理和游戏/系统消息分发。
/// </summary>
/// <remarks>
/// English: Business-level server skeleton built on top of IServerCore.
/// Centralizes session lifecycle (connect/disconnect/latency), heartbeat/message timestamps,
/// reconnect token management, and game/system message dispatch.
/// </remarks>
public abstract class BaseGameServer
{
    /// <summary>底层服务器内核</summary>
    protected readonly IServerCore Core;
    /// <summary>线程同步锁</summary>
    protected readonly object SyncRoot;

    /// <summary>按 NetPeer 索引的会话字典</summary>
    protected readonly Dictionary<NetPeer, PlayerSession> SessionsByPeer = new();
    /// <summary>按玩家ID索引的会话字典</summary>
    protected readonly Dictionary<string, PlayerSession> SessionsByPlayerId = new();
    /// <summary>已断线玩家及断线时间的记录</summary>
    protected readonly Dictionary<string, DateTime> DisconnectedAtByPlayerId = new();

    /// <summary>
    /// 初始化基础游戏服务器
    /// </summary>
    /// <param name="core">底层服务器内核实例</param>
    /// <param name="syncRoot">可选的自定义线程同步锁</param>
    /// <exception cref="ArgumentNullException">当 core 为 null 时抛出</exception>
    protected BaseGameServer(IServerCore core, object? syncRoot = null)
    {
        Core = core ?? throw new ArgumentNullException(nameof(core));
        SyncRoot = syncRoot ?? new object();

        RegisterCoreEvents();
    }

    /// <summary>启动服务器</summary>
    public virtual void Start() => Core.Start();
    /// <summary>停止服务器</summary>
    public virtual void Stop() => Core.Stop();
    /// <summary>轮询网络事件</summary>
    public virtual void PollEvents() => Core.PollEvents();

    /// <summary>
    /// 生成重连令牌
    /// </summary>
    /// <returns>唯一重连令牌字符串</returns>
    protected static string GenerateReconnectToken() => Guid.NewGuid().ToString("N");

    /// <summary>重连优雅期时长</summary>
    protected abstract TimeSpan ReconnectGracePeriod { get; }

    /// <summary>为对等端创建玩家ID</summary>
    /// <param name="peer">网络对等端</param>
    /// <returns>玩家ID</returns>
    protected abstract string CreatePlayerId(NetPeer peer);
    /// <summary>为对等端创建会话</summary>
    /// <param name="peer">网络对等端</param>
    /// <param name="playerId">玩家ID</param>
    /// <returns>玩家会话实例</returns>
    protected abstract PlayerSession CreateSession(NetPeer peer, string playerId);

    /// <summary>判断消息类型是否为游戏事件</summary>
    protected abstract bool IsGameEventType(string messageType);
    /// <summary>处理游戏事件</summary>
    protected abstract void HandleGameEvent(PlayerSession session, string eventType, string jsonPayload, DeliveryMethod deliveryMethod);
    /// <summary>处理系统消息</summary>
    protected abstract void HandleSystemMessage(PlayerSession session, string messageType, string jsonPayload, DeliveryMethod deliveryMethod);

    /// <summary>会话连接时的回调</summary>
    protected virtual void OnSessionConnected(PlayerSession session) { }
    /// <summary>会话断线时的回调</summary>
    protected virtual void OnSessionDisconnected(PlayerSession session, DisconnectInfo disconnectInfo) { }

    /// <summary>
    /// 尝试通过 NetPeer 获取会话
    /// </summary>
    protected bool TryGetSession(NetPeer peer, out PlayerSession session)
    {
        lock (SyncRoot)
        {
            return SessionsByPeer.TryGetValue(peer, out session!);
        }
    }

    /// <summary>
    /// 尝试通过玩家ID获取会话
    /// </summary>
    protected bool TryGetSession(string playerId, out PlayerSession session)
    {
        lock (SyncRoot)
        {
            return SessionsByPlayerId.TryGetValue(playerId, out session!);
        }
    }

    /// <summary>
    /// 尝试获取玩家断线时间
    /// </summary>
    protected bool TryGetDisconnectedAt(string playerId, out DateTime disconnectedAt)
    {
        lock (SyncRoot)
        {
            return DisconnectedAtByPlayerId.TryGetValue(playerId, out disconnectedAt);
        }
    }

    /// <summary>
    /// 标记玩家已断线，记录断线时间
    /// </summary>
    protected void MarkDisconnected(string playerId)
    {
        lock (SyncRoot)
        {
            DisconnectedAtByPlayerId[playerId] = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 清除玩家的断线标记
    /// </summary>
    protected void ClearDisconnectedMark(string playerId)
    {
        lock (SyncRoot)
        {
            DisconnectedAtByPlayerId.Remove(playerId);
        }
    }

    /// <summary>
    /// 注册底层内核事件回调
    /// </summary>
    private void RegisterCoreEvents()
    {
        Core.PeerConnected += peer =>
        {
            PlayerSession session;
            lock (SyncRoot)
            {
                string playerId = CreatePlayerId(peer);
                session = CreateSession(peer, playerId);

                session.Metadata["ReconnectToken"] = GenerateReconnectToken();
                session.IsConnected = true;
                session.UpdateHeartbeat();
                session.UpdateMessageTime();

                SessionsByPeer[peer] = session;
                SessionsByPlayerId[playerId] = session;
                DisconnectedAtByPlayerId.Remove(playerId);
            }

            OnSessionConnected(session);
        };

        Core.PeerDisconnected += (peer, disconnectInfo) =>
        {
            PlayerSession session;
            lock (SyncRoot)
            {
                if (!SessionsByPeer.TryGetValue(peer, out session!))
                {
                    return;
                }

                SessionsByPeer.Remove(peer);
                session.IsConnected = false;
                DisconnectedAtByPlayerId[session.PlayerId] = DateTime.UtcNow;
            }

            OnSessionDisconnected(session, disconnectInfo);
        };

        Core.PeerLatencyUpdated += (peer, latency) =>
        {
            lock (SyncRoot)
            {
                if (SessionsByPeer.TryGetValue(peer, out var session))
                {
                    session.Ping = latency;
                }
            }
        };

        Core.MessageReceived += inbound =>
        {
            PlayerSession session;
            lock (SyncRoot)
            {
                if (!SessionsByPeer.TryGetValue(inbound.FromPeer, out session!))
                {
                    return;
                }
                session.UpdateMessageTime();
            }

            if (IsGameEventType(inbound.Type))
            {
                HandleGameEvent(session, inbound.Type, inbound.JsonPayload, inbound.DeliveryMethod);
                return;
            }

            HandleSystemMessage(session, inbound.Type, inbound.JsonPayload, inbound.DeliveryMethod);
        };
    }
}
