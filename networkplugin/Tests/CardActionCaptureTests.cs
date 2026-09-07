using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.Messages;
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
}
