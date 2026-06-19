using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 治疗动作发送器：战斗内 BattlePlayerHealReport（对齐 BattleController_Patch.cs L709）
// 与 Gap 治疗 OnGapHealRequest（对齐 ResurrectSyncPatch.cs）。
public class HealSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public HealSender(SimNetworkClient client) => _client = client;

    // 战斗内治疗上报：BattlePlayerHealReport。
    public void SendBattleHeal(string targetId, int healValue, int actualHeal, int hp, int maxHp)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            PlayerId = _client.SelfPlayerId,
            PlayerName = _client.PlayerName,
            IsHost = false,
            Round = 1,
            TargetId = targetId,
            HealValue = healValue,
            ActualHeal = actualHeal,
            TargetState = new
            {
                Hp = hp,
                MaxHp = maxHp,
                Block = 0,
                Shield = 0,
                Status = "Alive",
                IsAlive = true,
            },
        };
        _client.SendGameEventData("BattlePlayerHealReport", payload);
    }

    // Gap 治疗请求：OnGapHealRequest（客户端→Host）。
    public void SendGapHeal(string targetPlayerId)
    {
        var payload = new
        {
            RequestId = Guid.NewGuid().ToString("N"),
            RequesterPlayerId = _client.SelfPlayerId,
            TargetPlayerId = targetPlayerId,
            Timestamp = DateTime.UtcNow.Ticks,
        };
        _client.SendGameEventData("OnGapHealRequest", payload);
    }

    // 随机治疗：战斗内随机回血。
    public void RunRandom(string selfId)
    {
        int heal = Rng.Next(5, 30);
        SendBattleHeal(selfId, heal, heal, Rng.Next(20, 80), 100);
    }
}