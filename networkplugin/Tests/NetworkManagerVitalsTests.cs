using System.Text.Json;
using Moq;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkManagerVitalsTests
{
    [Fact]
    public void PlayerListUpdate_WithHpAndMaxHp_UpdatesRemotePlayerVitals()
    {
        var mockClient = new Mock<INetworkClient>();
        mockClient.Setup(c => c.IsConnected).Returns(true);
        var manager = new NetworkManager(mockClient.Object);

        var payload = "{" +
                      "\"Players\":[" +
                      "  {\"PlayerId\":\"player_1\",\"PlayerName\":\"Local\",\"IsHost\":true,\"Hp\":80,\"MaxHp\":80}," +
                      "  {\"PlayerId\":\"player_2\",\"PlayerName\":\"Youmu\",\"IsHost\":false,\"Hp\":42,\"MaxHp\":75,\"CharacterId\":\"Youmu\"}" +
                      "]" +
                      "}";

        mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.PlayerListUpdate, payload);

        INetworkPlayer remotePlayer = manager.GetPlayer("player_2");
        Assert.NotNull(remotePlayer);
        Assert.Equal("Youmu", remotePlayer.userName);
        Assert.Equal("Youmu", remotePlayer.chara);
        Assert.Equal(42, remotePlayer.HP);
        Assert.Equal(75, remotePlayer.maxHP);
    }

    [Fact]
    public void Welcome_WithHpAndMaxHp_UpdatesRemotePlayerVitals()
    {
        var mockClient = new Mock<INetworkClient>();
        mockClient.Setup(c => c.IsConnected).Returns(true);
        var manager = new NetworkManager(mockClient.Object);

        var payload = "{" +
                      "\"PlayerId\":\"player_1\"," +
                      "\"Players\":[" +
                      "  {\"PlayerId\":\"player_1\",\"PlayerName\":\"Local\",\"Hp\":80,\"MaxHp\":80}," +
                      "  {\"PlayerId\":\"player_3\",\"PlayerName\":\"Cirno\",\"Hp\":35,\"MaxHp\":70,\"CharacterId\":\"Cirno\"}" +
                      "]" +
                      "}";

        mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, payload);

        INetworkPlayer remotePlayer = manager.GetPlayer("player_3");
        Assert.NotNull(remotePlayer);
        Assert.Equal(35, remotePlayer.HP);
        Assert.Equal(70, remotePlayer.maxHP);
    }
}
