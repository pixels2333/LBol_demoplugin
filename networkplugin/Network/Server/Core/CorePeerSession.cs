// 服务器内核会话：仅包含网络层的连接与活跃信息，避免把游戏层状态耦合进内核。
using System;
using LiteNetLib;

namespace NetworkPlugin.Network.Server.Core;

public sealed class CorePeerSession
{
    public NetPeer Peer { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastSeenAt { get; private set; }
    public int Ping { get; internal set; }

    public void MarkSeen(DateTime nowUtc)
    {
        LastSeenAt = nowUtc;
    }

    public bool IsTimeout(int timeoutSeconds, DateTime nowUtc)
    {
        return (nowUtc - LastSeenAt).TotalSeconds > timeoutSeconds;
    }
}

