using System;
using System.Collections.Generic;
using LiteNetLib;
using NetworkPlugin.Network.Server.Core;

namespace NetworkPlugin.Network.Server;

/// <summary>
/// Business-level server skeleton built on top of <see cref="IServerCore"/>.
/// Centralizes session lifecycle (connect/disconnect/latency), heartbeat/message timestamps,
/// reconnect token management, and game/system message dispatch.
/// </summary>
public abstract class BaseGameServer
{
    protected readonly IServerCore Core;
    protected readonly object SyncRoot;

    protected readonly Dictionary<NetPeer, PlayerSession> SessionsByPeer = new();
    protected readonly Dictionary<string, PlayerSession> SessionsByPlayerId = new();
    protected readonly Dictionary<string, DateTime> DisconnectedAtByPlayerId = new();

    protected BaseGameServer(IServerCore core, object? syncRoot = null)
    {
        Core = core ?? throw new ArgumentNullException(nameof(core));
        SyncRoot = syncRoot ?? new object();

        RegisterCoreEvents();
    }

    public virtual void Start() => Core.Start();
    public virtual void Stop() => Core.Stop();
    public virtual void PollEvents() => Core.PollEvents();

    protected static string GenerateReconnectToken() => Guid.NewGuid().ToString("N");

    protected abstract TimeSpan ReconnectGracePeriod { get; }

    protected abstract string CreatePlayerId(NetPeer peer);
    protected abstract PlayerSession CreateSession(NetPeer peer, string playerId);

    protected abstract bool IsGameEventType(string messageType);
    protected abstract void HandleGameEvent(PlayerSession session, string eventType, string jsonPayload, DeliveryMethod deliveryMethod);
    protected abstract void HandleSystemMessage(PlayerSession session, string messageType, string jsonPayload, DeliveryMethod deliveryMethod);

    protected virtual void OnSessionConnected(PlayerSession session) { }
    protected virtual void OnSessionDisconnected(PlayerSession session, DisconnectInfo disconnectInfo) { }

    protected bool TryGetSession(NetPeer peer, out PlayerSession session)
    {
        lock (SyncRoot)
        {
            return SessionsByPeer.TryGetValue(peer, out session!);
        }
    }

    protected bool TryGetSession(string playerId, out PlayerSession session)
    {
        lock (SyncRoot)
        {
            return SessionsByPlayerId.TryGetValue(playerId, out session!);
        }
    }

    protected bool TryGetDisconnectedAt(string playerId, out DateTime disconnectedAt)
    {
        lock (SyncRoot)
        {
            return DisconnectedAtByPlayerId.TryGetValue(playerId, out disconnectedAt);
        }
    }

    protected void MarkDisconnected(string playerId)
    {
        lock (SyncRoot)
        {
            DisconnectedAtByPlayerId[playerId] = DateTime.UtcNow;
        }
    }

    protected void ClearDisconnectedMark(string playerId)
    {
        lock (SyncRoot)
        {
            DisconnectedAtByPlayerId.Remove(playerId);
        }
    }

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

