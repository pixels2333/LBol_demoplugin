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
    // cardId/cardName/cardType/gunName/gunType 传入真实值时，接收端可创建真实卡牌与施法者，
    // 避免 actionSourceCard 为 null、施法者为空壳导致的“空包弹”问题。
    // targetIsEnemy=true 时 TargetUnitKind="Enemy"，接收端从 EnemyGroup 按 Id 查找目标敌人。
    public void SendOnRemoteCardUse(string targetPlayerId, string targetName, bool targetIsEnemy,
        string cardId, string cardName, string cardType, string gunName, string gunType)
    {
        bool hasRealCard = !string.IsNullOrWhiteSpace(cardId);
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
            TargetUnitKind = targetIsEnemy ? "Enemy" : "Player",
            Card = new
            {
                CardId = hasRealCard ? cardId! : $"Card_{Rng.Next(1000, 9999)}",
                InstanceId = Guid.NewGuid().ToString("N"),
                CardName = string.IsNullOrWhiteSpace(cardName) ? SampleCardNames[Rng.Next(SampleCardNames.Length)] : cardName,
                CardType = string.IsNullOrWhiteSpace(cardType) ? SampleCardTypes[Rng.Next(SampleCardTypes.Length)] : cardType,
                IsUpgraded = false,
                UpgradeCounter = 0,
            },
            ConsumingMana = new { Red = 0, Blue = 0, White = 0, Colorless = 0 },
            Kicker = false,
            SenderStatusEffects = Array.Empty<object>(),
            // Actions 不能为空：接收端 BuildReplayActions 在 actions.Count==0 时直接 return，
            // 导致战斗中也没反应。给一个 Damage 动作，并补齐 GunName/GunType（对齐真实发送端）。
            Actions = new object[]
            {
                new
                {
                    Kind = "Damage",
                    Damage = 5,
                    DamageType = "Attack",
                    IsAccuracy = false,
                    DontBreakPerfect = false,
                    GunName = string.IsNullOrWhiteSpace(gunName) ? "Instant" : gunName,
                    GunType = string.IsNullOrWhiteSpace(gunType) ? "Single" : gunType,
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