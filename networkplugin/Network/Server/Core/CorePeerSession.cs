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
