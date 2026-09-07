using System;
using Moq;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class PlayerVitalsHelperTests
{
    [Theory]
    [InlineData("Reimu", 80)]
    [InlineData("Marisa", 75)]
    [InlineData("Sakuya", 80)]
    [InlineData("Cirno", 70)]
    [InlineData("Koishi", 95)]
    [InlineData("Youmu", 80)]
    [InlineData("", 80)]
    [InlineData(null, 80)]
    [InlineData("UnknownChara", 80)]
    public void GetDefaultMaxHpForCharacter_ReturnsCorrectMaxHp(string? charId, int expectedMaxHp)
    {
        int actual = PlayerVitalsHelper.GetDefaultMaxHpForCharacter(charId);
        Assert.Equal(expectedMaxHp, actual);
    }

    [Fact]
    public void TryResolveVitals_WhenNetworkPlayerHasHpAndMaxHp_UsesNetworkPlayerValues()
    {
        var mockPlayer = new Mock<INetworkPlayer>();
        mockPlayer.Setup(p => p.HP).Returns(45);
        mockPlayer.Setup(p => p.maxHP).Returns(80);

        bool result = PlayerVitalsHelper.TryResolveVitals("client_youmu", mockPlayer.Object, out int curHp, out int maxHp);

        Assert.True(result);
        Assert.Equal(45, curHp);
        Assert.Equal(80, maxHp);
    }

    [Fact]
    public void TryResolveVitals_WhenNetworkPlayerMaxHpZero_FallsBackToCharacterDefault_NotLocalPlayer()
    {
        var mockPlayer = new Mock<INetworkPlayer>();
        mockPlayer.Setup(p => p.HP).Returns(0);
        mockPlayer.Setup(p => p.maxHP).Returns(0);
        mockPlayer.Setup(p => p.chara).Returns("Youmu");

        bool result = PlayerVitalsHelper.TryResolveVitals("client_youmu", mockPlayer.Object, out int curHp, out int maxHp);

        Assert.True(result);
        Assert.Equal(80, maxHp);
        Assert.True(curHp > 0);
    }

    [Fact]
    public void TryResolveVitals_ForVirtualAiPlayer_ResolvesMockHp()
    {
        bool result = PlayerVitalsHelper.TryResolveVitals("aidefault", null, out int curHp, out int maxHp);

        Assert.True(result);
        Assert.Equal(80, maxHp);
        Assert.Equal(56, curHp);
    }
}
