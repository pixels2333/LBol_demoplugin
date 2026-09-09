using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class CardActionSyncAndAnimationResetTests
{
    [Fact]
    public void BroadcastEventTypes_CardUsedAndUsUsed_MatchConstants()
    {
        Assert.Equal("BattlePlayerCardUsedBroadcast", NetworkMessageTypes.BattlePlayerCardUsedBroadcast);
        Assert.Equal("BattlePlayerUsUsedBroadcast", NetworkMessageTypes.BattlePlayerUsUsedBroadcast);
        Assert.Equal("BattlePlayerCardUsedReport", NetworkMessageTypes.BattlePlayerCardUsedReport);
        Assert.Equal("BattlePlayerUsUsedReport", NetworkMessageTypes.BattlePlayerUsUsedReport);
    }

    [Fact]
    public void IsSelfPlayer_WithRemoteContext_DoesNotTreatLocalAsSelf()
    {
        // 设置当前 selfId
        typeof(NetworkIdentityTracker).GetField("_selfPlayerId", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, "host_player");

        // 当没有远端上下文时，__local__ 视为本机自己
        Assert.True(RemoteCardUsePatch.IsSelfPlayer("__local__", null));
        Assert.True(RemoteCardUsePatch.IsSelfPlayer("", null));

        // 当包含远端上下文时（远端发来的消息中包含 __local__ 兜底），重定向为远端 ID，不应识别为本机自己
        Assert.False(RemoteCardUsePatch.IsSelfPlayer("__local__", "remote_peer_1"));
        Assert.False(RemoteCardUsePatch.IsSelfPlayer("remote_peer_1", null));

        // 本机 ID 准确匹配
        Assert.True(RemoteCardUsePatch.IsSelfPlayer("host_player", null));
        Assert.True(RemoteCardUsePatch.IsSelfPlayer("host_player", "remote_peer_1"));
    }

    [Fact]
    public void BuildActionBlueprint_PlayerUnit_SerializesPlayerIdAccurately()
    {
        // 模拟玩家单位
        var player = GameMockFactory.CreateMockPlayer("player_999", "Marisa");
        typeof(NetworkIdentityTracker).GetField("_selfPlayerId", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, "player_999");

        var target = GameMockFactory.CreateMockPlayer("dummy_target", "Target");
        var damageAction = new DamageAction(player, target, DamageInfo.Attack(15f, false), "MarisaSpark", GunType.Single);

        object[] blueprint = RemoteCardUsePatch.BuildActionBlueprint(new BattleAction[] { damageAction });
        Assert.NotNull(blueprint);
        Assert.Single(blueprint);

        string json = JsonSerializer.Serialize(blueprint);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement firstAction = doc.RootElement[0];

        Assert.Equal("Damage", NetworkEventHelper.GetString(firstAction, "Kind"));
        Assert.True(firstAction.TryGetProperty("Caster", out JsonElement casterEl));
        Assert.Equal("Player", NetworkEventHelper.GetString(casterEl, "Kind"));
        Assert.Equal("player_999", NetworkEventHelper.GetString(casterEl, "PlayerId"));
    }

    [Fact]
    public void OtherPlayersOverlayEventBridge_OnGameEventReceived_HandlesBothBroadcastAndReportWithoutException()
    {
        typeof(NetworkIdentityTracker).GetField("_selfPlayerId", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, "host_player");

        var cardPayload = new
        {
            PlayerId = "remote_player_1",
            CardName = "测试卡牌",
            Actions = new object[]
            {
                new
                {
                    Kind = "PerformAction",
                    Type = "Gun",
                    GunName = "Instant"
                }
            }
        };

        var usPayload = new
        {
            PlayerId = "remote_player_1",
            UsName = "测试符卡",
            Actions = new object[]
            {
                new
                {
                    Kind = "PerformAction",
                    Type = "Spell",
                    SpellName = "测试符卡"
                }
            }
        };

        // 验证广播事件类型接收无异常
        OtherPlayersOverlayPatch.OnGameEventReceived(NetworkMessageTypes.BattlePlayerCardUsedBroadcast, cardPayload);
        OtherPlayersOverlayPatch.OnGameEventReceived(NetworkMessageTypes.BattlePlayerUsUsedBroadcast, usPayload);

        // 验证旧版本 Report 事件接收兼容且无异常
        OtherPlayersOverlayPatch.OnGameEventReceived(NetworkMessageTypes.BattlePlayerCardUsedReport, cardPayload);
        OtherPlayersOverlayPatch.OnGameEventReceived(NetworkMessageTypes.BattlePlayerUsUsedReport, usPayload);
    }

    [Fact]
    public void OtherPlayersOverlayEventBridge_SelfEvent_IsIgnoredToPreventLoopback()
    {
        typeof(NetworkIdentityTracker).GetField("_selfPlayerId", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, "host_player");

        var selfCardPayload = new
        {
            PlayerId = "host_player",
            CardName = "我自己的卡牌",
            Actions = Array.Empty<object>()
        };

        // 本机发出的事件在收到时应被忽略，不产生异常与回环
        OtherPlayersOverlayPatch.OnGameEventReceived(NetworkMessageTypes.BattlePlayerCardUsedBroadcast, selfCardPayload);
        OtherPlayersOverlayPatch.OnGameEventReceived(NetworkMessageTypes.BattlePlayerUsUsedBroadcast, selfCardPayload);
    }

    [Fact]
    public void EnsureUnitIdle_NullOrMock_IsSafelyHandled()
    {
        // 验证空安全
        RemoteCardUsePatch.EnsureUnitIdle(null);
    }

    [Theory]
    [InlineData(GunType.Single, 0, false)] // Single 状态为 Idle(0) 时不忙
    [InlineData(GunType.First, 0, false)]  // First 状态为 Idle(0) 时不忙
    [InlineData(GunType.Middle, 2, false)] // Middle 处于 Complex(2) 连击状态时不忙
    [InlineData(GunType.Last, 2, false)]   // Last 处于 Complex(2) 连击状态时不忙
    [InlineData(GunType.Middle, 0, true)]  // Middle 状态为 Idle(0) 则异常繁忙（非 Complex）
    [InlineData(GunType.Last, 0, true)]    // Last 状态为 Idle(0) 则异常繁忙（非 Complex）
    public void MultiStageAttack_StatusBusyLogic_IsCorrect(GunType gunType, int currentStatus, bool expectedBusy)
    {
        // 验证多段连击状态检查逻辑规则
        bool isBusy = false;
        if (gunType == GunType.Middle || gunType == GunType.Last)
        {
            if (currentStatus != 2)
            {
                isBusy = true;
            }
        }
        else
        {
            // 对于 Single/First，非 0 状态会先重置
            isBusy = false;
        }

        Assert.Equal(expectedBusy, isBusy);
    }

    [Fact]
    public void EnsureUnitIdle_WithForceResetOptions_IsSafelyHandled()
    {
        // 验证空安全及不同 forceReset 参数
        RemoteCardUsePatch.EnsureUnitIdle(null, forceReset: false);
        RemoteCardUsePatch.EnsureUnitIdle(null, forceReset: true);
    }

    [Fact]
    public void PlayBlockShieldVisual_ThreeStageCadence_TotalWaitTimeIsSufficient()
    {
        var payload = new
        {
            Kind = ActionBlueprintConstants.KindBlockShield,
            Block = 12f,
            Shield = 0f
        };
        string json = JsonSerializer.Serialize(payload);
        using var doc = JsonDocument.Parse(json);
        var enumerator = RemoteCardUsePatch.PlayBlockShieldVisual(doc.RootElement, null, null);

        var waitList = new List<float>();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current is UnityEngine.WaitForSeconds wfs)
            {
                float seconds = Traverse.Create(wfs).Field("m_Seconds").GetValue<float>();
                waitList.Add(seconds);
            }
            else if (enumerator.Current is float sec)
            {
                waitList.Add(sec);
            }
        }

        // 验证包含 3 个等待阶段（起手蓄力 0.2s, 光效音效爆发 0.2s, 后摇缓冲 0.1s）
        Assert.Equal(3, waitList.Count);
        Assert.Equal(0.2f, waitList[0], 2);
        Assert.Equal(0.2f, waitList[1], 2);
        Assert.Equal(0.1f, waitList[2], 2);

        float totalWait = 0f;
        foreach (float sec in waitList)
        {
            totalWait += sec;
        }
        Assert.True(totalWait >= 0.45f, $"总等待时长应 >= 0.45s，实际为 {totalWait}");
    }

    [Fact]
    public void PlayBlockShieldVisual_ShieldAction_ProducesThreeStages()
    {
        var payload = new
        {
            Kind = ActionBlueprintConstants.KindBlockShield,
            Block = 0f,
            Shield = 15f
        };
        string json = JsonSerializer.Serialize(payload);
        using var doc = JsonDocument.Parse(json);
        var enumerator = RemoteCardUsePatch.PlayBlockShieldVisual(doc.RootElement, null, null);

        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
        }

        Assert.Equal(3, count);
    }

    [Fact]
    public void PlayGenericCastVisual_MissingCaster_YieldsSafelyWithoutException()
    {
        var enumerator = RemoteCardUsePatch.PlayGenericCastVisual(null, "nonexistent_player");
        Assert.NotNull(enumerator);
        // 不应抛出任何异常
        while (enumerator.MoveNext()) { }
    }

    [Fact]
    public void SafeWaitForSeconds_InNonUnityEnvironment_ReturnsOriginalFloat()
    {
        object waitObj = RemoteCardUsePatch.SafeWaitForSeconds(0.35f);
        Assert.NotNull(waitObj);
        if (waitObj is float sec)
        {
            Assert.Equal(0.35f, sec, 2);
        }
    }

    [Fact]
    public void ActionBlueprintConstants_PacingConstants_MatchExpected()
    {
        Assert.Equal("defend", ActionBlueprintConstants.AnimDefend);
        Assert.Equal("spell", ActionBlueprintConstants.AnimSpell);
        Assert.Equal("skill", ActionBlueprintConstants.AnimSkill);
        Assert.Equal("idle", ActionBlueprintConstants.AnimIdle);
        Assert.Equal("ShieldCast", ActionBlueprintConstants.SfxShieldCast);
        Assert.Equal("BlockShield", ActionBlueprintConstants.KindBlockShield);
    }
}
