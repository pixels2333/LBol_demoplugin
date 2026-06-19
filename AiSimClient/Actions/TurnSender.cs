using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 回合控制发送器：EndTurnRequest（对齐 EndTurnSyncPatch.cs L872）
// 与 OnTurnStart/OnTurnEnd 边界快照（对齐 TurnAction_Patch.cs L477，TurnBoundarySnapshot）。
public class TurnSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public TurnSender(SimNetworkClient client) => _client = client;

    // 结束回合请求：EndTurnRequest。
    public void SendEndTurnRequest(string battleId, int round)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            PlayerId = _client.SelfPlayerId,
            BattleId = battleId,
            Round = round,
        };
        _client.SendGameEventData("EndTurnRequest", payload);
    }

    // 回合边界快照：OnTurnStart / OnTurnEnd。
    // 提供 PlayerState 最小值（PlayerId/UserName/基本 HP），否则接收端 senderId 退化为 "unknown" 导致跳过更新。
    // StatusEffects/Intentions 传空，测接收端更新玩家 endturn 标志的反应。
    public void SendTurnBoundary(bool isStart, string battleId, int round)
    {
        var payload = new
        {
            StatusEffects = Array.Empty<object>(),
            PlayerState = new
            {
                PlayerId = _client.SelfPlayerId,
                UserName = _client.SelfPlayerId,
                Health = 30,
                MaxHealth = 30,
                Block = 0,
                Shield = 0,
                ManaGroup = new[] { 3, 0, 0, 0 },
                Gold = 100,
                IsAlive = true,
                GameLocation = new { X = 0, Y = 0, NodeType = "MapNode" },
            },
            Intentions = (object?)null,
            BoundaryType = isStart ? 0 : 1, // 0=Start, 1=End
            BattleId = battleId,
            Round = round,
            TimestampTicks = DateTime.Now.Ticks,
        };
        _client.SendGameEventData(isStart ? "OnTurnStart" : "OnTurnEnd", payload);
    }

    // 随机：发回合开始 + 结束回合请求。
    public void RunRandom()
    {
        string battleId = $"battle_{Rng.Next(1, 99)}";
        int round = Rng.Next(1, 10);
        SendTurnBoundary(true, battleId, round);
        SendEndTurnRequest(battleId, round);
    }
}