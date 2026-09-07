using System;
using System.Collections.Generic;
using LiteNetLib;

namespace NetworkPlugin.Network.Server;

public class PlayerSession
{
    #region 会话核心属性

        public NetPeer Peer { get; set; } = null!;

        public string PlayerId { get; set; } = string.Empty;

        public string PlayerName { get; set; } = string.Empty;

        public string CurrentRoomId { get; set; } = string.Empty;

        public DateTime ConnectedAt { get; set; }

        public DateTime LastHeartbeat { get; set; }

        public DateTime LastMessageAt { get; set; }

    #endregion

    #region 网络质量与状态

        public int Ping { get; set; }

        public bool IsConnected { get; set; }

        public bool IsHost { get; set; }

    #endregion

    #region 扩展数据

        public Dictionary<string, object> Metadata { get; set; } = [];

    #endregion

    #region 便捷属性与方法

        public string RemoteEndPoint => Peer.EndPoint?.ToString() ?? "unknown";

        public bool IsTimeout(int timeoutSeconds = 30)
    {
        return (DateTime.UtcNow - LastHeartbeat).TotalSeconds > timeoutSeconds;
    }

        public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

        public void UpdateMessageTime()
    {
        LastMessageAt = DateTime.UtcNow;
    }

    #endregion
}
