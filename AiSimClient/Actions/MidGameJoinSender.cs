using System;
using AiSimClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AiSimClient.Actions;

// MidGameJoinResponse 载荷解析（房主批准后返回）。
public class MidGameJoinResponseData
{
    public string RequestId { get; set; } = "";
    public bool Approved { get; set; }
    public string? Reason { get; set; }
    public string? JoinToken { get; set; }
    public string? RoomId { get; set; }
}

/// <summary>
/// 中途加入发送器。对齐 MidGameJoinManager 的流程：
/// 1) 发 DirectMessage 封装的 MidGameJoinRequest 给房主（RoomId="default"）
/// 2) 房主自动批准后回 MidGameJoinResponse（含 JoinToken）
/// 3) 收到批准后发 FullStateSyncRequest（DirectMessage 封装）获取完整快照
/// </summary>
public class MidGameJoinSender
{
    private readonly SimNetworkClient _client;
    private string? _hostPlayerId;
    private string? _lastJoinToken;
    private string? _lastRoomId;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
    };

    public MidGameJoinSender(SimNetworkClient client)
    {
        _client = client;
        _client.OnMidGameJoinResponse += HandleResponse;
    }

    // 从 Welcome 的 PlayerList 找房主 ID（IsHost=true 且非自身）。
    public void SetHostFromWelcome(WelcomeData welcome)
    {
        _hostPlayerId = null;
        var list = welcome.PlayerList ?? welcome.Players;
        if (list == null) return;
        foreach (var p in list)
        {
            if (p.IsHost && p.PlayerId != welcome.PlayerId)
            {
                _hostPlayerId = p.PlayerId;
                return;
            }
        }
    }

    public bool CanRequest => !string.IsNullOrEmpty(_hostPlayerId) && !string.IsNullOrEmpty(_client.SelfPlayerId);

    // 步骤1：发 MidGameJoinRequest（DirectMessage 封装，定向发给房主）。
    public void SendJoinRequest(string roomId = "default")
    {
        if (string.IsNullOrEmpty(_hostPlayerId))
        {
            _client.SendGameEventData("__log__", new { Msg = "无房主 ID，无法请求中途加入" });
            return;
        }
        _lastRoomId = roomId;
        var payload = new
        {
            RequestId = Guid.NewGuid().ToString("N"),
            RoomId = roomId,
            PlayerName = _client.PlayerName,
            ClientPlayerId = _client.SelfPlayerId,
            ClientTimeUtcTicks = DateTime.UtcNow.Ticks,
        };
        // DirectMessage 封装：外层 Type="DirectMessage"，内层 Type="MidGameJoinRequest"。
        var dm = new
        {
            TargetPlayerId = _hostPlayerId,
            Type = "MidGameJoinRequest",
            Payload = payload,
        };
        _client.SendGameEventData("DirectMessage", dm);
    }

    // 步骤2：收到 MidGameJoinResponse 后，发 FullStateSyncRequest。
    private void HandleResponse(string json)
    {
        try
        {
            var resp = JsonConvert.DeserializeObject<MidGameJoinResponseData>(json, JsonSettings);
            if (resp == null || !resp.Approved)
            {
                return;
            }
            _lastJoinToken = resp.JoinToken;
            SendFullStateSyncRequest(resp.JoinToken ?? "", resp.RoomId ?? _lastRoomId ?? "default");
        }
        catch
        {
            // ignored
        }
    }

    // 步骤3：发 FullStateSyncRequest（DirectMessage 封装）获取完整快照。
    public void SendFullStateSyncRequest(string joinToken, string roomId)
    {
        if (string.IsNullOrEmpty(_hostPlayerId)) return;
        var payload = new
        {
            RequestId = Guid.NewGuid().ToString("N"),
            RoomId = roomId,
            TargetPlayerId = _client.SelfPlayerId,
            LastKnownEventIndex = 0,
            JoinToken = joinToken,
        };
        var dm = new
        {
            TargetPlayerId = _hostPlayerId,
            Type = "FullStateSyncRequest",
            Payload = payload,
        };
        _client.SendGameEventData("DirectMessage", dm);
    }
}