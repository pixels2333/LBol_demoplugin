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
    /// 判断消息类型是否为游戏事件。
    /// </summary>
    /// <param name="messageType">消息类型标识。</param>
    /// <returns>若是游戏事件（按 HostServer 路由表判断）返回 true，否则 false。</returns>
    private bool IsGameEvent(string messageType)
    {
        return NetworkMessageTypes.IsGameEvent(messageType, NetworkMessageTypes.GameEventRoute.HostServer);
    }

    /// <summary>
    /// 对每种受控路由仅打印一次探测日志，用于确认 RoomState/FullStateSync 等消息流转正常。
    /// </summary>
    /// <param name="routeKey">路由标识，如 "RoomStateRequest"、"DirectMessage/Type"。</param>
    /// <param name="details">日志详情，通常包含 sender 与 target 的 PlayerId。</param>
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

    /// <summary>
    /// 获取当前已连接且标记为房主的会话。
    /// </summary>
    /// <returns>房主会话；无房主或房主未连接时返回 null。</returns>
    private PlayerSession GetConnectedHostSession()
    {
        lock (SyncRoot)
        {
            return SessionsByPeer.Values.FirstOrDefault(session => session.IsHost && session.IsConnected);
        }
    }

    /// <summary>
    /// 尝试按目标 PlayerId 获取已连接的会话。
    /// </summary>
    /// <param name="targetPlayerId">目标玩家 ID。</param>
    /// <param name="targetSession">输出会话实例。</param>
    /// <returns>目标存在且已连接返回 true，否则 false。</returns>
    private bool TryGetConnectedTargetSession(string targetPlayerId, out PlayerSession targetSession)
    {
        targetSession = null;
        return !string.IsNullOrWhiteSpace(targetPlayerId) &&
               TryGetSession(targetPlayerId, out targetSession) &&
               targetSession.IsConnected;
    }

    /// <summary>
    /// 安全地从 JSON 对象中提取字符串属性，支持多属性名候选（按顺序匹配第一个非空值）。
    /// </summary>
    /// <param name="root">JSON 对象根元素。</param>
    /// <param name="propertyNames">一个或多个属性名候选，用于兼容历史字段命名（如 TargetPlayerId/RequesterId）。</param>
    /// <returns>首个非空字符串值；全部未命中返回 null。</returns>
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

    /// <summary>
    /// 从 DirectMessage 等嵌套结构中提取内层 Payload JSON。
    /// </summary>
    /// <param name="root">外层 JSON 对象。</param>
    /// <returns>"Payload" 属性的原始 JSON 文本；不存在时返回 "{}"。</returns>
    private static string GetNestedPayloadJson(JsonElement root)
    {
        return root.TryGetProperty("Payload", out JsonElement payloadElement)
            ? payloadElement.GetRawText()
            : "{}";
    }

    /// <summary>
    /// 判断消息类型是否为受控路由消息（RoomState/FullStateSync 系列），并立即执行定向转发。
    /// </summary>
    /// <param name="senderSession">发送者会话。</param>
    /// <param name="messageType">消息类型标识。</param>
    /// <param name="jsonPayload">JSON 负载。</param>
    /// <returns>若是受控消息且已被路由消费返回 true，否则 false（由调用方继续广播）。</returns>
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
        if (TryGetSession(fromPeer, out var session))
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
