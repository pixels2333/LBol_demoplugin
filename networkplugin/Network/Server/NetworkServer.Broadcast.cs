// NOTE: 这里使用了日志系统和依赖注入；如果后续引入分离服务器，需要相应调整日志系统与依赖注入。
// 直连房主服务器：用于房主/客机直连联机，管理会话与广播游戏事件。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Server.Core;
using NetworkPlugin.Utils;
namespace NetworkPlugin.Network.Server;
// 消息发送与广播
public partial class NetworkServer
{
#region 消息发送与广播

    /// <summary>
    /// 广播游戏事件给所有玩家（除了指定的发送者）
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="eventData">事件数据</param>
    /// <param name="excludePeerId">要排除的Peer ID</param>
    private void BroadcastGameEvent(string eventType, object eventData, int excludePeerId)
    {
        string json = JsonCompat.Serialize(eventData);

        foreach (var session in SessionsByPeer.Values)
        {
            if (session.Peer.Id != excludePeerId && session.IsConnected)
            {
                try
                {
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(eventType);
                    writer.Put(json);
                    session.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception ex)
                {
                    Plugin.Logger?.LogError($"[服务器] 广播游戏事件异常: to={session.PlayerId}, err={ex.Message}");
                    _logger?.LogError($"[服务器] 广播游戏事件异常: to={session.PlayerId}, err={ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 广播系统消息给所有玩家
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息数据</param>
    /// <param name="excludePeerId">可选的要排除的Peer ID</param>
    private void BroadcastMessage(string messageType, object data, int? excludePeerId = null)
    {
        foreach (var session in SessionsByPeer.Values)
        {
            if (excludePeerId.HasValue && session.Peer.Id == excludePeerId.Value)
            {
                continue;
            }

            if (session.IsConnected)
            {
                SendMessage(session.Peer, messageType, data);
            }
        }
    }

    /// <summary>
    /// 发送消息给特定玩家
    /// </summary>
    /// <param name="peer">目标网络对等体</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息数据</param>
    private void SendMessage(NetPeer peer, string messageType, object data)
    {
        try
        {
            string json = JsonCompat.Serialize(data);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(messageType);
            writer.Put(json);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[服务器] 发送消息异常: to={peer.EndPoint}, err={ex.Message}");
            _logger?.LogError($"[服务器] 发送消息异常: to={peer.EndPoint}, err={ex.Message}");
        }
    }

    /// <summary>
    /// 发送欢迎消息给新玩家
    /// 包含玩家ID、房主状态和当前玩家列表
    /// </summary>
    /// <param name="peer">新连接的网络对等体</param>
    /// <param name="session">玩家会话</param>
    private static int? TryGetMetadataInt(Dictionary<string, object> metadata, string key)
    {
        if (metadata == null || !metadata.TryGetValue(key, out object value) || value == null)
        {
            return null;
        }

        return value switch
        {
            int i => i,
            long l => (int)l,
            float f => (int)f,
            double d => (int)d,
            string s when int.TryParse(s, out int i) => i,
            JsonElement je when je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out int i) => i,
            JsonElement je when je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out int i) => i,
            _ => null
        };
    }

    private static string TryGetMetadataString(Dictionary<string, object> metadata, string key)
    {
        if (metadata == null || !metadata.TryGetValue(key, out object value) || value == null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            _ => value.ToString()
        };
    }

    private void SendWelcomeMessage(NetPeer peer, PlayerSession session)  
    {
        var welcomeData = new
        {
            Message = "Welcome to the server!",
            PlayerId = session.PlayerId,
            IsHost = session.IsHost,
            ReconnectToken = TryGetMetadataString(session.Metadata, "ReconnectToken"),
            PlayerList = _sessionsByPlayerId.Values.Select(s => new
            {
                PlayerId = s.PlayerId,
                PlayerName = s.PlayerName,
                IsHost = s.IsHost,
                IsConnected = s.IsConnected,
                CharacterId = TryGetMetadataString(s.Metadata, "CharacterId"),
                LocationX = TryGetMetadataInt(s.Metadata, "LocationX") ?? -1,
                LocationY = TryGetMetadataInt(s.Metadata, "LocationY") ?? -1,
                Stage = TryGetMetadataInt(s.Metadata, "Stage") ?? -1,
                LocationName = TryGetMetadataString(s.Metadata, "LocationName")
            }).ToList()
        };

        SendMessage(peer, NetworkMessageTypes.Welcome, welcomeData);
    }

    /// <summary>
    /// 广播玩家列表更新
    /// 通知所有玩家当前连接的玩家状态
    /// </summary>
    private void BroadcastPlayerList()
    {
        var playerList = _sessionsByPlayerId.Values.Select(s => new
        {
            PlayerId = s.PlayerId,
            PlayerName = s.PlayerName,
            IsHost = s.IsHost,
            IsConnected = s.IsConnected,
            CharacterId = TryGetMetadataString(s.Metadata, "CharacterId"),
            LocationX = TryGetMetadataInt(s.Metadata, "LocationX") ?? -1,
            LocationY = TryGetMetadataInt(s.Metadata, "LocationY") ?? -1,
            Stage = TryGetMetadataInt(s.Metadata, "Stage") ?? -1,
            LocationName = TryGetMetadataString(s.Metadata, "LocationName")
        }).ToList();

        BroadcastMessage(NetworkMessageTypes.PlayerListUpdate, new { Players = playerList });
    }

    private void CleanupDisconnectedSessions()
    {
        if (_disconnectedAtByPlayerId.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        List<string> expired = null;

        foreach (var kvp in _disconnectedAtByPlayerId)
        {
            if (now - kvp.Value > _reconnectGracePeriod)
            {
                expired ??= new List<string>();
                expired.Add(kvp.Key);
            }
        }

        if (expired == null)
        {
            return;
        }

        bool hostRemoved = false;
        foreach (string playerId in expired)
        {
            if (_sessionsByPlayerId.TryGetValue(playerId, out var session))
            {
                hostRemoved |= session.IsHost;
                _sessionsByPlayerId.Remove(playerId);
            }

            _disconnectedAtByPlayerId.Remove(playerId);
        }

        if (hostRemoved)
        {
            foreach (var s in _sessionsByPlayerId.Values)
            {
                s.IsHost = false;
            }

            var newHost = SessionsByPeer.Values.FirstOrDefault(s => s.IsConnected);
            if (newHost != null)
            {
                newHost.IsHost = true;
                BroadcastMessage(NetworkMessageTypes.HostChanged, new { NewHostId = newHost.PlayerId });
                Plugin.Logger?.LogInfo($"[服务器] 房主已变更为 {newHost.PlayerId}");
            }
        }

        BroadcastPlayerList();
    }

    #endregion
}
