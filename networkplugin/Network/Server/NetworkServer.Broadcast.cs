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
    /// 广播游戏事件给所有已连接玩家，可选择排除发送者自身。
    /// </summary>
    /// <param name="eventType">事件类型标识。</param>
    /// <param name="eventData">事件数据对象，将被序列化为 JSON。</param>
    /// <param name="excludePeerId">要排除的 Peer ID（通常为发送者），避免回声。</param>
    /// <remarks>
    /// 使用 <see cref="DeliveryMethod.ReliableOrdered"/> 保证所有客户端按相同顺序接收事件。
    /// 单客户端发送异常被捕获并记录，不影响其他客户端的广播。
    /// </remarks>
    private void BroadcastGameEvent(string eventType, string jsonPayload, int excludePeerId)
    {
        string json = jsonPayload;

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
    /// 广播系统消息给所有已连接玩家，可选择排除特定 peer。
    /// </summary>
    /// <param name="messageType">消息类型标识。</param>
    /// <param name="data">消息数据。</param>
    /// <param name="excludePeerId">可选的排除 Peer ID。</param>
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
    /// 向指定 peer 单播消息，数据自动序列化为 JSON。
    /// </summary>
    /// <param name="peer">目标网络对等体。</param>
    /// <param name="messageType">消息类型标识。</param>
    /// <param name="data">消息数据对象。</param>
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
    /// <summary>
    /// 安全地从 Metadata 字典中提取整数属性，支持 int/long/float/double/string/JsonElement 多种来源。
    /// </summary>
    /// <param name="metadata">玩家会话的元数据字典。</param>
    /// <param name="key">属性名。</param>
    /// <returns>转换后的整数值；无法转换时返回 null。</returns>
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

    /// <summary>
    /// 安全地从 Metadata 字典中提取字符串属性。
    /// </summary>
    /// <param name="metadata">玩家会话的元数据字典。</param>
    /// <param name="key">属性名。</param>
    /// <returns>字符串值；非字符串类型返回 <c>ToString()</c>；失败返回 null。</returns>
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

    /// <summary>
    /// 向新连接玩家发送 Welcome 消息，包含玩家身份、房主状态、重连令牌及当前完整玩家列表。
    /// </summary>
    /// <param name="peer">新连接的网络对等体。</param>
    /// <param name="session">对应的玩家会话。</param>
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
    /// 广播当前完整玩家列表给所有已连接客户端。
    /// 在玩家加入、离开、重连、房主变更时调用，保持各客户端 UI 同步。
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

    /// <summary>
    /// 清理已超过重连优雅期的断线会话，并在房主被移除时自动迁移房主身份。
    /// </summary>
    /// <remarks>
    /// 流程：
    /// 1. 扫描 <c>_disconnectedAtByPlayerId</c>，找出超过 <see cref="_reconnectGracePeriod"/> 的条目；
    /// 2. 若被移除的玩家是房主，则从剩余已连接玩家中选举新房主；
    /// 3. 广播更新后的玩家列表。
    /// </remarks>
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

        // 若房主被移除，需重新选举：第一个已连接玩家成为新房主
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
