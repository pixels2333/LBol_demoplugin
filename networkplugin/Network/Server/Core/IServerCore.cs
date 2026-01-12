using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// Abstraction over the underlying server networking core.
/// Keeps business servers (Host/Relay) decoupled from the concrete <see cref="ServerCore"/> implementation.
/// </summary>
public interface IServerCore
{
    ServerOptions Options { get; }

    EventBasedNetListener Listener { get; }
    NetManager NetManager { get; }

    event Action<NetPeer>? PeerConnected;
    event Action<NetPeer, DisconnectInfo>? PeerDisconnected;
    event Action<NetPeer, int>? PeerLatencyUpdated;
    event Action<ServerInboundMessage>? MessageReceived;

    void Start();
    void Stop();
    void PollEvents();
    void MarkPeerSeen(NetPeer peer);
    bool TryGetSession(NetPeer peer, out CorePeerSession? session);
}
