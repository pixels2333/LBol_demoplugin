using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class CardVfxTimingAndDamageDedupTests
{
    [Fact]
    public void BuildActionBlueprint_DamageAction_PreservesRealDamageAndProperties()
    {
        var player = GameMockFactory.CreateMockPlayer("player_test_1", "Reimu");
        var target = GameMockFactory.CreateMockPlayer("enemy_target_1", "Cirno");
        
        const float expectedDamage = 42f;
        var damageInfo = DamageInfo.Attack(expectedDamage, isAccuracy: true);
        damageInfo.DontBreakPerfect = true;

        var damageAction = new DamageAction(player, target, damageInfo, "DreamSeal", GunType.Single);

        object[] blueprint = RemoteCardUsePatch.BuildActionBlueprint(new BattleAction[] { damageAction });
        Assert.NotNull(blueprint);
        Assert.Single(blueprint);

        string json = JsonSerializer.Serialize(blueprint);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement actionEl = doc.RootElement[0];

        Assert.Equal(ActionBlueprintConstants.KindDamage, NetworkEventHelper.GetString(actionEl, ActionBlueprintConstants.KeyKind));
        Assert.Equal("DreamSeal", NetworkEventHelper.GetString(actionEl, ActionBlueprintConstants.KeyGunName));
        Assert.Equal("Single", NetworkEventHelper.GetString(actionEl, ActionBlueprintConstants.KeyGunType));

        // 验证真实伤害数值与标志位完整无损
        Assert.True(actionEl.TryGetProperty(ActionBlueprintConstants.KeyDamage, out JsonElement dmgEl));
        Assert.Equal(expectedDamage, dmgEl.GetSingle());

        Assert.Equal("Attack", NetworkEventHelper.GetString(actionEl, ActionBlueprintConstants.KeyDamageType));
        Assert.True(actionEl.TryGetProperty(ActionBlueprintConstants.KeyIsAccuracy, out JsonElement accEl));
        Assert.True(accEl.GetBoolean());
        Assert.True(actionEl.TryGetProperty(ActionBlueprintConstants.KeyDontBreakPerfect, out JsonElement dbpEl));
        Assert.True(dbpEl.GetBoolean());
    }

    [Fact]
    public void RemoteCardPlaybackTracker_LifeCycleAndNesting_Accurate()
    {
        RemoteCardPlaybackTracker.ResetForTest();
        Assert.False(RemoteCardPlaybackTracker.IsInCardPlaybackWindow);

        // 第一次进入
        RemoteCardPlaybackTracker.NotifyCardVisualStarted();
        Assert.True(RemoteCardPlaybackTracker.IsInCardPlaybackWindow);

        // 嵌套进入（如 PlayUsSequenceCoroutine 嵌套 PlayVisualsCoroutine）
        RemoteCardPlaybackTracker.NotifyCardVisualStarted();
        Assert.True(RemoteCardPlaybackTracker.IsInCardPlaybackWindow);

        // 退出内层
        RemoteCardPlaybackTracker.NotifyCardVisualEnded();
        Assert.True(RemoteCardPlaybackTracker.IsInCardPlaybackWindow);

        // 退出外层
        RemoteCardPlaybackTracker.NotifyCardVisualEnded();
        Assert.False(RemoteCardPlaybackTracker.IsInCardPlaybackWindow);

        // 防御性测试：多调用一次 Ended 不会导致负数溢出或崩盘
        RemoteCardPlaybackTracker.NotifyCardVisualEnded();
        Assert.False(RemoteCardPlaybackTracker.IsInCardPlaybackWindow);
    }

    [Fact]
    public void DamageBlueprint_MultiHitSequence_PreservesRealDamageOnEachHit()
    {
        var player = GameMockFactory.CreateMockPlayer("player_multi", "Marisa");
        var target = GameMockFactory.CreateMockPlayer("enemy_target_2", "Boss");

        var hit1 = new DamageAction(player, target, DamageInfo.Attack(10f, false), "MasterSpark", GunType.First);
        var hit2 = new DamageAction(player, target, DamageInfo.Attack(15f, false), "MasterSpark", GunType.Middle);
        var hit3 = new DamageAction(player, target, DamageInfo.Attack(25f, true), "MasterSpark", GunType.Last);

        object[] blueprint = RemoteCardUsePatch.BuildActionBlueprint(new BattleAction[] { hit1, hit2, hit3 });
        Assert.NotNull(blueprint);
        Assert.Equal(3, blueprint.Length);

        string json = JsonSerializer.Serialize(blueprint);
        using JsonDocument doc = JsonDocument.Parse(json);

        float[] expectedDamages = [10f, 15f, 25f];
        string[] expectedGunTypes = ["First", "Middle", "Last"];

        for (int i = 0; i < 3; i++)
        {
            JsonElement el = doc.RootElement[i];
            Assert.Equal(ActionBlueprintConstants.KindDamage, NetworkEventHelper.GetString(el, ActionBlueprintConstants.KeyKind));
            Assert.Equal(expectedDamages[i], el.GetProperty(ActionBlueprintConstants.KeyDamage).GetSingle());
            Assert.Equal(expectedGunTypes[i], NetworkEventHelper.GetString(el, ActionBlueprintConstants.KeyGunType));
        }
    }

    [Fact]
    public void MaterializeActionEnumerable_PreservesExecutionWithoutLoss()
    {
        // 模拟 Card_GetActions_Postfix 的即刻物化逻辑
        IEnumerable<BattleAction> MockGetActions()
        {
            yield return new CastBlockShieldAction(null, null, 5, 0, BlockShieldType.Normal);
            yield return new CastBlockShieldAction(null, null, 0, 10, BlockShieldType.Normal);
        }

        IEnumerable<BattleAction> rawEnumerable = MockGetActions();
        List<BattleAction> materialized = rawEnumerable as List<BattleAction> ?? [.. rawEnumerable];

        Assert.Equal(2, materialized.Count);
        Assert.IsType<CastBlockShieldAction>(materialized[0]);
        Assert.IsType<CastBlockShieldAction>(materialized[1]);

        // 验证物化后再次被消费不会二次耗尽
        int count = 0;
        foreach (var action in materialized)
        {
            count++;
        }
        Assert.Equal(2, count);
    }
}
