using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 复活发送器：OnResurrectRequest（客户端→Host）与 OnPlayerResurrected（对齐 DeathPatches.cs L357）。
public class ResurrectSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public ResurrectSender(SimNetworkClient client) => _client = client;

    public void SendResurrectRequest(string targetPlayerId)
    {
        var payload = new
        {
            RequestId = Guid.NewGuid().ToString("N"),
            RequesterPlayerId = _client.SelfPlayerId,
            TargetPlayerId = targetPlayerId,
            Timestamp = DateTime.UtcNow.Ticks,
        };
        _client.SendGameEventData("OnResurrectRequest", payload);
    }

    public void SendPlayerResurrected(int resurrectionHp, int maxHp)
    {
        var payload = new
        {
            PlayerId = _client.SelfPlayerId,
            TargetPlayerId = _client.SelfPlayerId,
            ResurrectionHp = resurrectionHp,
            MaxHp = maxHp,
            Status = "Alive",
            Timestamp = DateTime.UtcNow.Ticks,
        };
        _client.SendGameEventData("OnPlayerResurrected", payload);
    }

    public void RunRandom()
    {
        SendPlayerResurrected(Rng.Next(10, 40), 100);
    }
}