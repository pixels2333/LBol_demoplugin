using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 伤害上报发送器：BattlePlayerDamageReport（对齐 BattleController_Patch.cs L373）。
public class DamageSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public DamageSender(SimNetworkClient client) => _client = client;

    public void SendDamageReport(string targetId, string sourceId, int totalDamage, int hpDamage)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            PlayerId = _client.SelfPlayerId,
            PlayerName = _client.PlayerName,
            IsHost = false,
            Round = 1,
            SourceId = sourceId,
            TargetId = targetId,
            ActionSource = "DamageAction",
            Damage = new
            {
                TotalDamage = totalDamage,
                HpDamage = hpDamage,
                BlockedDamage = 0,
                ShieldedDamage = 0,
                DamageType = "Attack",
                IsGrazed = false,
                IsAccuracy = true,
                OverDamage = 0,
            },
            TargetState = new
            {
                Hp = Math.Max(0, 100 - hpDamage),
                MaxHp = 100,
                Block = 0,
                Shield = 0,
                Status = "Alive",
                IsAlive = true,
            },
        };
        _client.SendGameEventData("BattlePlayerDamageReport", payload);
    }

    public void RunRandom(string selfId)
    {
        int dmg = Rng.Next(5, 25);
        SendDamageReport(selfId, $"Enemy_{Rng.Next(1, 9)}", dmg, dmg);
    }
}