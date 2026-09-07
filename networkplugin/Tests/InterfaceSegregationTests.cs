using NetworkPlugin.Network.NetworkPlayer;
using Xunit;

namespace NetworkPlugin.Tests;

public class InterfaceSegregationTests
{
    [Fact]
    public void INetworkPlayer_IsCompositeInterface()
    {

        var playerType = typeof(INetworkPlayer);
        var interfaces = playerType.GetInterfaces();

        Assert.Contains(typeof(IPlayerIdentity), interfaces);
        Assert.Contains(typeof(IPlayerBattleState), interfaces);
        Assert.Contains(typeof(IPlayerResources), interfaces);
        Assert.Contains(typeof(IPlayerNetworkSync), interfaces);
    }

    [Fact]
    public void IPlayerIdentity_HasRequiredMembers()
    {
        var members = typeof(IPlayerIdentity).GetProperties().Select(p => p.Name).ToHashSet();
        Assert.Contains("playerId", members);
        Assert.Contains("userName", members);
        Assert.Contains("chara", members);
    }
}
