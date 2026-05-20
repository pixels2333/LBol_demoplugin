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
// 消息路由处理
public partial class NetworkServer
{
#region 消息处理

    /// <summary>
    /// 检查消息类型是否为游戏事件消息
    /// </summary>
    /// <param name="messageType">消息类型字符串</param>
    /// <returns>如果是游戏事件消息返回true，否则返回false</returns>
    private bool IsGameEvent(string messageType)
    {
        return NetworkMessageTypes.IsGameEvent(messageType, NetworkMessageTypes.GameEventRoute.HostServer);
    }

    /// <summary>
    /// 处理游戏同步事件
    /// 接收客户端发送的游戏事件，更新会话状态，并广播给其他玩家
    /// </summary>
    private void LogRouteProbeOnce(string routeKey, string details)
    {
        if (string.IsNullOrWhiteSpace(routeKey))
        {
            return;
        }

        try
        {
            bool shouldLog;
            lock (_routeProbeOnceKeys)
            {
                shouldLog = _routeProbeOnceKeys.Add(routeKey);
            }

            if (!shouldLog)
            {
                return;
            }

            _logger?.LogInfo($"[RouteProbe] {routeKey}: {details}");
        }
        catch (Exception ex) { _logger?.LogWarning($"[RouteProbe] LogRouteProbeOnce error: {ex.Message}"); }
    }

    private PlayerSession GetConnectedHostSession()
    {
        return SessionsByPeer.Values.FirstOrDefault(session => session.IsHost && session.IsConnected);
    }

    private bool TryGetConnectedTargetSession(string targetPlayerId, out PlayerSession targetSession)
    {
        targetSession = null;
        return !string.IsNullOrWhiteSpace(targetPlayerId) &&
               TryGetSession(targetPlayerId, out targetSession) &&
               targetSession.IsConnected;
    }

    private static string TryGetJsonStringProperty(JsonElement root, params string[] propertyNames)
    {
        for (int index = 0; index < propertyNames.Length; index++)
        {
            string propertyName = propertyNames[index];
            if (!root.TryGetProperty(propertyName, out JsonElement propertyValue) || propertyValue.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            string value = propertyValue.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string GetNestedPayloadJson(JsonElement root)
    {
        return root.TryGetProperty("Payload", out JsonElement payloadElement)
            ? payloadElement.GetRawText()
            : "{}";
    }

    private bool TryRouteControlledMessage(PlayerSession senderSession, string messageType, string jsonPayload)
    {
        switch (messageType)
        {
            case NetworkMessageTypes.RoomStateRequest:
                RouteRoomStateRequest(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.RoomStateUpload:
                RouteRoomStateUpload(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.RoomStateResponse:
                RouteRoomStateResponse(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.FullStateSyncRequest:
                RouteFullStateSyncRequest(senderSession, jsonPayload);
                return true;
            case NetworkMessageTypes.FullStateSyncResponse:
                RouteFullStateSyncResponse(senderSession, jsonPayload);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Host/直连模式下的 RoomStateRequest 路由：请求定向转发给房主（主机作为房间状态中枢）。
    /// </summary>
    private void RouteRoomStateRequest(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            PlayerSession hostSession = GetConnectedHostSession();
            if (hostSession == null)
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.RoomStateRequest,
                $"sender={senderSession?.PlayerId}, host={hostSession.PlayerId}");

            // 定向转发给房主：由房主侧 RoomStateManager 生成并回发 RoomStateResponse。
            SendRawJsonToPeer(hostSession.Peer, NetworkMessageTypes.RoomStateRequest, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 RoomStateRequest 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 RoomStateUpload 路由：上传定向转发给房主。
    /// </summary>
    private void RouteRoomStateUpload(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            PlayerSession hostSession = GetConnectedHostSession();
            if (hostSession == null)
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.RoomStateUpload,
                $"sender={senderSession?.PlayerId}, host={hostSession.PlayerId}");

            SendRawJsonToPeer(hostSession.Peer, NetworkMessageTypes.RoomStateUpload, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 RoomStateUpload 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 RoomStateResponse 路由：仅单播给请求方（payload.TargetPlayerId 或 payload.RequesterId）。
    /// </summary>
    private void RouteRoomStateResponse(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            if (!senderSession.IsHost)
            {
                return;
            }

            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId", "RequesterId");

            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                return;
            }

            if (!TryGetConnectedTargetSession(targetPlayerId, out PlayerSession targetSession))
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.RoomStateResponse,
                $"host={senderSession?.PlayerId}, target={targetPlayerId}");

            SendRawJsonToPeer(targetSession.Peer, NetworkMessageTypes.RoomStateResponse, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 RoomStateResponse 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 FullStateSyncRequest 路由：将请求转发给房主（由房主侧 MidGameJoinManager 生成快照并回发响应）。
    /// </summary>
    /// <param name="senderSession">请求方会话。</param>
    /// <param name="jsonPayload">请求负载 JSON。</param>
    private void RouteFullStateSyncRequest(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            // 查找房主会话（直连模式的权威端）。
            PlayerSession hostSession = GetConnectedHostSession();
            if (hostSession == null)
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.FullStateSyncRequest,
                $"sender={senderSession?.PlayerId}, host={hostSession.PlayerId}");

            // 请求必须以自己为 TargetPlayerId，避免代替他人拉取快照。
            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId");
            if (!string.IsNullOrWhiteSpace(targetPlayerId) && !string.Equals(targetPlayerId, senderSession.PlayerId, StringComparison.Ordinal))
            {
                return;
            }

            // 定向转发给房主：由房主侧进行 JoinToken 校验与快照生成。
            SendRawJsonToPeer(hostSession.Peer, NetworkMessageTypes.FullStateSyncRequest, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 FullStateSyncRequest 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// Host/直连模式下的 FullStateSyncResponse 路由：仅单播给请求方（payload.TargetPlayerId）。
    /// </summary>
    /// <param name="senderSession">响应发送方（应为房主）。</param>
    /// <param name="jsonPayload">响应负载 JSON。</param>
    private void RouteFullStateSyncResponse(PlayerSession senderSession, string jsonPayload)
    {
        try
        {
            if (!senderSession.IsHost)
            {
                return;
            }

            JsonElement root = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
            string targetPlayerId = TryGetJsonStringProperty(root, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                return;
            }

            if (!TryGetConnectedTargetSession(targetPlayerId, out PlayerSession targetSession))
            {
                return;
            }

            LogRouteProbeOnce(NetworkMessageTypes.FullStateSyncResponse,
                $"host={senderSession?.PlayerId}, target={targetPlayerId}");

            SendRawJsonToPeer(targetSession.Peer, NetworkMessageTypes.FullStateSyncResponse, jsonPayload);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 路由 FullStateSyncResponse 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 向指定 peer 发送一条“类型 + JSON payload”的原始消息（避免二次序列化改变字段结构）。
    /// </summary>
    /// <param name="peer">目标 peer。</param>
    /// <param name="messageType">消息类型。</param>
    /// <param name="jsonPayload">JSON payload 字符串。</param>
    private void SendRawJsonToPeer(NetPeer peer, string messageType, string jsonPayload)
    {
        try
        {
            NetDataWriter writer = new();
            writer.Put(messageType);
            writer.Put(jsonPayload ?? string.Empty);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[服务器] 发送原始消息异常: type={messageType}, err={ex.Message}");
        }
    }

    /// <summary>
    /// 处理心跳包
    /// 更新玩家会话的心跳时间并发送响应
    /// </summary>
    /// <param name="fromPeer">发送心跳的网络对等体</param>
    private void HandleHeartbeat(NetPeer fromPeer)
    {
        if (SessionsByPeer.TryGetValue(fromPeer, out var session))
        {
            session.UpdateHeartbeat();

            SendMessage(fromPeer, NetworkMessageTypes.HeartbeatResponse, new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Ping = session.Ping
            });
        }
    }

    #endregion
}
