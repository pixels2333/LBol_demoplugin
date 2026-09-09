using System.Collections.Generic;
using System.Text.Json;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class CardActionCaptureTests
{
    [Fact]
    public void CardActionPayload_SerializedWithActions_CanBeParsedProperly()
    {
        var payload = new
        {
            PlayerId = "p_123",
            PlayerName = "Reimu",
            CardName = "博丽御币",
            CardId = "ReimuCardAttack",
            IsUs = false,
            Actions = new object[]
            {
                new
                {
                    Kind = "PerformAction",
                    Type = "Gun",
                    GunId = "ReimuShoot",
                    WaitTime = 0.2f
                },
                new
                {
                    Kind = "PerformAction",
                    Type = "Effect",
                    EffectName = "ReimuBlink",
                    Delay = 0f
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        Assert.True(NetworkEventHelper.TryGetJsonElement(json, out JsonElement root));

        Assert.Equal("p_123", NetworkEventHelper.GetString(root, "PlayerId"));
        Assert.Equal("博丽御币", NetworkEventHelper.GetString(root, "CardName"));
        Assert.False(root.GetProperty("IsUs").GetBoolean());

        Assert.True(root.TryGetProperty("Actions", out JsonElement actionsEl));
        Assert.Equal(JsonValueKind.Array, actionsEl.ValueKind);
        Assert.Equal(2, actionsEl.GetArrayLength());

        var firstAction = actionsEl[0];
        Assert.Equal("PerformAction", NetworkEventHelper.GetString(firstAction, "Kind"));
        Assert.Equal("Gun", NetworkEventHelper.GetString(firstAction, "Type"));
        Assert.Equal("ReimuShoot", NetworkEventHelper.GetString(firstAction, "GunId"));
    }

    [Fact]
    public void BuildActionBlueprint_WithCastBlockShieldAction_GeneratesBlockShieldBlueprint()
    {
        var player = GameMockFactory.CreateMockPlayer("reimu_player", "Reimu");
        var blockAction = new CastBlockShieldAction(player, player, 12, 5, BlockShieldType.Normal, cast: true);

        object[] blueprint = RemoteCardUsePatch.BuildActionBlueprint(new BattleAction[] { blockAction });

        Assert.NotNull(blueprint);
        Assert.Single(blueprint);

        string json = JsonSerializer.Serialize(blueprint);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.Equal(1, root.GetArrayLength());

        JsonElement elem = root[0];
        Assert.Equal("BlockShield", NetworkEventHelper.GetString(elem, "Kind"));
        Assert.Equal(12f, elem.GetProperty("Block").GetSingle());
        Assert.Equal(5f, elem.GetProperty("Shield").GetSingle());
        Assert.True(elem.GetProperty("Cast").GetBoolean());
        Assert.Equal("Normal", NetworkEventHelper.GetString(elem, "Type"));

        Assert.True(elem.TryGetProperty("Source", out JsonElement sourceEl));
        Assert.Equal("Player", NetworkEventHelper.GetString(sourceEl, "Kind"));

        Assert.True(elem.TryGetProperty("Target", out JsonElement targetEl));
        Assert.Equal("Player", NetworkEventHelper.GetString(targetEl, "Kind"));
    }

    [Fact]
    public void BuildActionBlueprint_WithMixedActions_ExtractsAllKindsAccurately()
    {
        var player = GameMockFactory.CreateMockPlayer("p1", "Player1");
        var actions = new BattleAction[]
        {
            new CastBlockShieldAction(player, player, 8, 0, BlockShieldType.Normal, cast: true),
            PerformAction.Sfx("ShieldCast", 0.1f),
            new HealAction(player, player, 10, HealType.Normal, 0.2f),
            PerformAction.Spell(player, "MasterSpark")
        };

        object[] blueprint = RemoteCardUsePatch.BuildActionBlueprint(actions, out bool hasDamage, out bool hasHeal, out bool hasStatus);

        Assert.False(hasDamage);
        Assert.True(hasHeal);
        Assert.False(hasStatus);
        Assert.NotNull(blueprint);
        Assert.Equal(4, blueprint.Length);

        string json = JsonSerializer.Serialize(blueprint);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal("BlockShield", NetworkEventHelper.GetString(root[0], "Kind"));
        Assert.Equal(8f, root[0].GetProperty("Block").GetSingle());

        Assert.Equal("PerformAction", NetworkEventHelper.GetString(root[1], "Kind"));
        Assert.Equal("Sfx", NetworkEventHelper.GetString(root[1], "Type"));
        Assert.Equal("ShieldCast", NetworkEventHelper.GetString(root[1], "Id"));

        Assert.Equal("Heal", NetworkEventHelper.GetString(root[2], "Kind"));
        Assert.Equal(10, root[2].GetProperty("Amount").GetInt32());

        Assert.Equal("PerformAction", NetworkEventHelper.GetString(root[3], "Kind"));
        Assert.Equal("Spell", NetworkEventHelper.GetString(root[3], "Type"));
        Assert.Equal("MasterSpark", NetworkEventHelper.GetString(root[3], "SpellName"));
    }

    [Fact]
    public void BuildActionBlueprint_EmptyOrNullActions_ReturnsEmptyArray()
    {
        object[] nullResult = RemoteCardUsePatch.BuildActionBlueprint(null);
        Assert.NotNull(nullResult);
        Assert.Empty(nullResult);

        object[] emptyResult = RemoteCardUsePatch.BuildActionBlueprint(new BattleAction[0]);
        Assert.NotNull(emptyResult);
        Assert.Empty(emptyResult);
    }

    [Fact]
    public void BlockShieldPayload_SerializedAndParsed_PreservesAllFields()
    {
        var payload = new
        {
            Timestamp = 638612345678901234L,
            PlayerId = "player_001",
            PlayerName = "Reimu",
            IsHost = false,
            CardName = "阴阳宝玉",
            CardId = "YinyangBaoyuCard",
            UsName = (string?)null,
            IsUs = false,
            Actions = new object[]
            {
                new
                {
                    Kind = "BlockShield",
                    Source = new { Kind = "Player", PlayerId = "player_001" },
                    Target = new { Kind = "Player", PlayerId = "player_001" },
                    Block = 8.0f,
                    Shield = 0.0f,
                    Cast = true,
                    Type = "Normal"
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        Assert.True(NetworkEventHelper.TryGetJsonElement(json, out JsonElement root));

        Assert.Equal("player_001", NetworkEventHelper.GetString(root, "PlayerId"));
        Assert.Equal("阴阳宝玉", NetworkEventHelper.GetString(root, "CardName"));
        Assert.False(root.GetProperty("IsUs").GetBoolean());

        Assert.True(root.TryGetProperty("Actions", out JsonElement actionsEl));
        Assert.Equal(JsonValueKind.Array, actionsEl.ValueKind);
        Assert.Equal(1, actionsEl.GetArrayLength());

        var blockAction = actionsEl[0];
        Assert.Equal("BlockShield", NetworkEventHelper.GetString(blockAction, "Kind"));
        Assert.Equal(8.0f, blockAction.GetProperty("Block").GetSingle());
        Assert.Equal(0.0f, blockAction.GetProperty("Shield").GetSingle());
        Assert.True(blockAction.GetProperty("Cast").GetBoolean());
        Assert.Equal("Normal", NetworkEventHelper.GetString(blockAction, "Type"));

        var source = blockAction.GetProperty("Source");
        Assert.Equal("Player", NetworkEventHelper.GetString(source, "Kind"));
        Assert.Equal("player_001", NetworkEventHelper.GetString(source, "PlayerId"));
    }

    [Fact]
    public void BuildReplayActions_WithBlockShieldAction_ReconstructsActionCorrectly()
    {
        var player = GameMockFactory.CreateMockPlayer("player_001", "Reimu");
        var payload = new
        {
            Actions = new object[]
            {
                new
                {
                    Kind = "BlockShield",
                    Source = new { Kind = "Player", PlayerId = "player_001" },
                    Target = new { Kind = "Player", PlayerId = "player_001" },
                    Block = 15.0f,
                    Shield = 7.0f,
                    Cast = false,
                    Type = "Direct"
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        using JsonDocument doc = JsonDocument.Parse(json);

        List<BattleAction> replayed = RemoteCardUsePatch.BuildReplayActions(doc.RootElement, null, player, player);

        Assert.NotNull(replayed);
        Assert.Single(replayed);

        var action = Assert.IsType<CastBlockShieldAction>(replayed[0]);
        Assert.Equal(15f, action.Args.Block);
        Assert.Equal(7f, action.Args.Shield);
        Assert.False(action.Cast);
        Assert.Equal(BlockShieldType.Direct, action.Args.Type);
    }

    [Fact]
    public void BuildActionBlueprint_ShieldOnlyAndBlockOnly_PreservesValuesAccurately()
    {
        var player = GameMockFactory.CreateMockPlayer("reimu", "Reimu");
        var shieldOnly = new CastBlockShieldAction(player, player, 0, 10, BlockShieldType.Normal, cast: true);
        var blockOnly = new CastBlockShieldAction(player, player, 14, 0, BlockShieldType.Normal, cast: false);

        object[] blueprint = RemoteCardUsePatch.BuildActionBlueprint(new BattleAction[] { shieldOnly, blockOnly });

        Assert.Equal(2, blueprint.Length);

        string json = JsonSerializer.Serialize(blueprint);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(0f, root[0].GetProperty("Block").GetSingle());
        Assert.Equal(10f, root[0].GetProperty("Shield").GetSingle());
        Assert.True(root[0].GetProperty("Cast").GetBoolean());

        Assert.Equal(14f, root[1].GetProperty("Block").GetSingle());
        Assert.Equal(0f, root[1].GetProperty("Shield").GetSingle());
        Assert.False(root[1].GetProperty("Cast").GetBoolean());
    }

    [Fact]
    public void ShootStatus_BoxedEnumConversion_DoesNotThrowAndDetectsBusy()
    {
        object idleObj = LBoL.Presentation.Units.UnitView.ShootStatus.Idle;
        object directObj = LBoL.Presentation.Units.UnitView.ShootStatus.Direct;
        object complexObj = LBoL.Presentation.Units.UnitView.ShootStatus.Complex;

        // Verify that Convert.ToInt32 handles boxed enums safely without throwing
        var exIdle = Record.Exception(() => System.Convert.ToInt32(idleObj));
        var exDirect = Record.Exception(() => System.Convert.ToInt32(directObj));
        var exComplex = Record.Exception(() => System.Convert.ToInt32(complexObj));
        Assert.Null(exIdle);
        Assert.Null(exDirect);
        Assert.Null(exComplex);

        // Verify values
        Assert.Equal(0, System.Convert.ToInt32(idleObj));
        Assert.Equal(1, System.Convert.ToInt32(directObj));
        Assert.Equal(2, System.Convert.ToInt32(complexObj));

        // Verify busy evaluation
        bool isIdleBusy = idleObj != null && System.Convert.ToInt32(idleObj) != 0;
        bool isDirectBusy = directObj != null && System.Convert.ToInt32(directObj) != 0;
        bool isComplexBusy = complexObj != null && System.Convert.ToInt32(complexObj) != 0;

        Assert.False(isIdleBusy);
        Assert.True(isDirectBusy);
        Assert.True(isComplexBusy);
    }

    [Fact]
    public void BuildReplayActions_EmptyOrMissingActions_ReturnsEmptyList()
    {
        var player = GameMockFactory.CreateMockPlayer("player_001", "Reimu");

        using JsonDocument docMissing = JsonDocument.Parse("{}");
        var resMissing = RemoteCardUsePatch.BuildReplayActions(docMissing.RootElement, null, player, player);
        Assert.NotNull(resMissing);
        Assert.Empty(resMissing);

        using JsonDocument docEmpty = JsonDocument.Parse("{\"Actions\": []}");
        var resEmpty = RemoteCardUsePatch.BuildReplayActions(docEmpty.RootElement, null, player, player);
        Assert.NotNull(resEmpty);
        Assert.Empty(resEmpty);

        using JsonDocument docUnknown = JsonDocument.Parse("{\"Actions\": [{\"Kind\": \"UnknownKind\"}]}");
        var resUnknown = RemoteCardUsePatch.BuildReplayActions(docUnknown.RootElement, null, player, player);
        Assert.NotNull(resUnknown);
        Assert.Empty(resUnknown);
    }

    [Fact]
    public void BuildReplayActions_BlockShield_HandlesMissingOrNegativeFieldsSafely()
    {
        var player = GameMockFactory.CreateMockPlayer("player_001", "Reimu");
        var payload = new
        {
            Actions = new object[]
            {
                new
                {
                    Kind = "BlockShield",
                    // Missing Source and Target, negative Block/Shield
                    Block = -5.0f,
                    Shield = -2.0f,
                    Cast = true,
                    Type = "Normal"
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        using JsonDocument doc = JsonDocument.Parse(json);

        List<BattleAction> replayed = RemoteCardUsePatch.BuildReplayActions(doc.RootElement, null, player, null);

        Assert.NotNull(replayed);
        Assert.Single(replayed);

        var action = Assert.IsType<CastBlockShieldAction>(replayed[0]);
        // Clamped to >= 0
        Assert.Equal(0f, action.Args.Block);
        Assert.Equal(0f, action.Args.Shield);
        // Source and target safely fell back to caster
        Assert.Equal(player, action.Args.Source);
        Assert.Equal(player, action.Args.Target);
    }

    [Fact]
    public void BuildActionBlueprint_WithNullElementsInCollection_FiltersSafely()
    {
        var player = GameMockFactory.CreateMockPlayer("p", "P");
        var valid = new CastBlockShieldAction(player, player, 10, 0);
        var actions = new BattleAction[] { null, valid, null };

        object[] bp = RemoteCardUsePatch.BuildActionBlueprint(actions);
        Assert.NotNull(bp);
        Assert.Single(bp);
    }

    [Fact]
    public void BuildReplayActions_WithDamageAction_ReconstructsDamageActionCorrectly()
    {
        var player = GameMockFactory.CreateMockPlayer("player_001", "Reimu");
        var payload = new
        {
            Actions = new object[]
            {
                new
                {
                    Kind = "Damage",
                    Caster = new { Kind = "Player", PlayerId = "player_001" },
                    Damage = 25.0f,
                    DamageType = "Attack",
                    GunName = "ReimuShoot",
                    GunType = "Single"
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        using JsonDocument doc = JsonDocument.Parse(json);

        List<BattleAction> replayed = RemoteCardUsePatch.BuildReplayActions(doc.RootElement, null, player, player);

        Assert.NotNull(replayed);
        Assert.Single(replayed);

        var action = Assert.IsType<DamageAction>(replayed[0]);
        Assert.Equal(25f, action.DealingArgs.DamageInfo.Damage);
        Assert.Equal(DamageType.Attack, action.DealingArgs.DamageInfo.DamageType);
        Assert.Equal("ReimuShoot", action.GunName);
        Assert.Equal(GunType.Single, action.GunType);
        Assert.Equal(player, action.DealingArgs.Source);
    }

    [Fact]
    public void BuildReplayActions_WithMultipleActionTypes_ReconstructsInOrder()
    {
        var player = GameMockFactory.CreateMockPlayer("player_001", "Reimu");
        var payload = new
        {
            Actions = new object[]
            {
                new
                {
                    Kind = "BlockShield",
                    Block = 10f,
                    Shield = 0f,
                    Cast = true,
                    Type = "Normal"
                },
                new
                {
                    Kind = "Heal",
                    Amount = 15,
                    HealType = "Normal",
                    WaitTime = 0.2f
                },
                new
                {
                    Kind = "Damage",
                    Damage = 30f,
                    DamageType = "Reaction",
                    GunName = "Instant"
                }
            }
        };

        string json = JsonSerializer.Serialize(payload);
        using JsonDocument doc = JsonDocument.Parse(json);

        List<BattleAction> replayed = RemoteCardUsePatch.BuildReplayActions(doc.RootElement, null, player, player);

        Assert.NotNull(replayed);
        Assert.Equal(3, replayed.Count);

        Assert.IsType<CastBlockShieldAction>(replayed[0]);
        Assert.IsType<HealAction>(replayed[1]);
        Assert.IsType<DamageAction>(replayed[2]);
    }

    [Fact]
    public void ActionBlueprintConstants_MatchesExpectedProtocolValues()
    {
        Assert.Equal("Damage", ActionBlueprintConstants.KindDamage);
        Assert.Equal("BlockShield", ActionBlueprintConstants.KindBlockShield);
        Assert.Equal("Heal", ActionBlueprintConstants.KindHeal);
        Assert.Equal("ApplyStatusEffect", ActionBlueprintConstants.KindApplyStatusEffect);
        Assert.Equal("PerformAction", ActionBlueprintConstants.KindPerformAction);
        Assert.Equal("Animation", ActionBlueprintConstants.KindAnimation);
        Assert.Equal("Sfx", ActionBlueprintConstants.KindSfx);
        Assert.Equal("Wait", ActionBlueprintConstants.KindWait);

        Assert.Equal("Player", ActionBlueprintConstants.UnitPlayer);
        Assert.Equal("Enemy", ActionBlueprintConstants.UnitEnemy);
        Assert.Equal("Unknown", ActionBlueprintConstants.UnitUnknown);

        Assert.Equal("defend", ActionBlueprintConstants.AnimDefend);
        Assert.Equal("spell", ActionBlueprintConstants.AnimSpell);
        Assert.Equal("cast", ActionBlueprintConstants.AnimCast);
        Assert.Equal("hit", ActionBlueprintConstants.AnimHit);

        Assert.Equal("CastShield", ActionBlueprintConstants.VfxCastShield);
        Assert.Equal("CastBlock", ActionBlueprintConstants.VfxCastBlock);
        Assert.Equal("CardCast", ActionBlueprintConstants.VfxCardCast);
        Assert.Equal("ShieldCast", ActionBlueprintConstants.SfxShieldCast);
    }

    [Fact]
    public void ResolveUnitView_RemotePlayerNotFound_DoesNotFallbackToLocalPlayer()
    {
        // When resolving for a remote player without matching remote view, it must return null rather than local PlayerUnitView
        var view = RemoteCardUsePatch.ResolveUnitView(null, default, null, "remote_nonexistent_player");
        Assert.Null(view);

        // When elem specifies Kind="Player" and a remote PlayerId, it must also return null if not found
        using JsonDocument doc = JsonDocument.Parse("{\"Kind\":\"Player\",\"PlayerId\":\"remote_nonexistent_player\"}");
        var viewFromElem = RemoteCardUsePatch.ResolveUnitView(null, doc.RootElement, null, null);
        Assert.Null(viewFromElem);
    }

    [Fact]
    public void SafePlayAnimation_And_PlayShieldVfx_HandleNullSafely()
    {
        // SafePlayAnimation with nulls must not throw
        var exAnim = Record.Exception(() =>
        {
            RemoteCardUsePatch.SafePlayAnimation(null, null);
            RemoteCardUsePatch.SafePlayAnimation(null, ActionBlueprintConstants.AnimDefend, ActionBlueprintConstants.AnimSpell);
        });
        Assert.Null(exAnim);

        // PlayShieldVfx with null target view must not throw
        var exVfx = Record.Exception(() =>
        {
            RemoteCardUsePatch.PlayShieldVfx(null, true);
            RemoteCardUsePatch.PlayShieldVfx(null, false);
        });
        Assert.Null(exVfx);
    }
}


