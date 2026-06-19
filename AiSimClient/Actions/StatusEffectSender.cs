using System;
using System.Collections.Generic;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 状态效果发送器：OnStatusEffectApplied（对齐 ApplyStatusEffectAction_Patch.cs L307）。
// payload 在游戏内是 Dictionary<string,object>，外部程序用 Dictionary 构造占位字段。
public class StatusEffectSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    private static readonly string[] SampleEffects = { "Strength", "Vulnerable", "Weak", "Dexterity", "Frail" };

    public StatusEffectSender(SimNetworkClient client) => _client = client;

    public void SendStatusEffectApplied(string targetId, string effectType, int stack)
    {
        var statusData = new Dictionary<string, object>
        {
            ["Timestamp"] = DateTime.Now.Ticks,
            ["PlayerId"] = _client.SelfPlayerId,
            ["TargetId"] = targetId,
            ["StatusEffectType"] = effectType,
            ["Stack"] = stack,
            ["EffectCategory"] = effectType.Contains("Vulnerable") || effectType.Contains("Weak") ? "IDebuff" : "IBuff",
        };
        _client.SendGameEventData("OnStatusEffectApplied", statusData);
    }

    public void RunRandom(string selfId)
    {
        string effect = SampleEffects[Rng.Next(SampleEffects.Length)];
        SendStatusEffectApplied(selfId, effect, Rng.Next(1, 5));
    }
}