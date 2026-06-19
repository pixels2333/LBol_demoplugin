using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 出牌动作发送器：构造 OnRemoteCardUse / OnRemoteCardResolved 载荷。
// 外部程序无游戏运行时，Card/Mana/Actions 用随机占位值，用于测试接收端处理链路。
public class CardPlaySender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    private static readonly string[] SampleCardNames =
        { "Fireball", "Heal", "Strike", "Block", "Lightning", "Frost" };
    private static readonly string[] SampleCardTypes = { "Attack", "Skill", "Ability" };

    public CardPlaySender(SimNetworkClient client)
    {
        _client = client;
    }

    // 发送远程用牌事件，字段对齐 RemoteCardUsePatch.cs L494-L522。
    public void SendOnRemoteCardUse(string targetPlayerId, string targetName)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            RequestId = Guid.NewGuid().ToString("N"),
            EventType = "OnRemoteCardUse",
            SenderPlayerId = _client.SelfPlayerId,
            SenderName = _client.PlayerName,
            SenderCharacterId = _client.CharacterId,
            TargetPlayerId = targetPlayerId,
            TargetName = targetName,
            Card = new
            {
                CardId = $"Card_{Rng.Next(1000, 9999)}",
                InstanceId = Guid.NewGuid().ToString("N"),
                CardName = SampleCardNames[Rng.Next(SampleCardNames.Length)],
                CardType = SampleCardTypes[Rng.Next(SampleCardTypes.Length)],
                IsUpgraded = false,
                UpgradeCounter = 0,
            },
            ConsumingMana = new { Red = 0, Blue = 0, White = 0, Colorless = 0 },
            Kicker = false,
            SenderStatusEffects = Array.Empty<object>(),
            // Actions 不能为空：接收端 BuildReplayActions 在 actions.Count==0 时直接 return，
            // 导致战斗中也没反应。给一个占位 Damage 动作（Kind=Damage, DamageType=Attack）。
            Actions = new object[]
            {
                new
                {
                    Kind = "Damage",
                    Damage = 5,
                    DamageType = "Attack",
                    IsAccuracy = false,
                    DontBreakPerfect = false,
                },
            },
        };
        _client.SendGameEventData("OnRemoteCardUse", payload);
    }

    // 发送结算完成事件，对齐 RemoteCardUsePatch.Execution.cs L333。
    public void SendOnRemoteCardResolved(string targetPlayerId)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            RequestId = Guid.NewGuid().ToString("N"),
            SenderPlayerId = _client.SelfPlayerId,
            TargetPlayerId = targetPlayerId,
        };
        _client.SendGameEventData("OnRemoteCardResolved", payload);
    }
}