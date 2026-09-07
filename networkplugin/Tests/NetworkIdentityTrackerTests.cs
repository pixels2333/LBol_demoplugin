using System.Text.Json;
using Moq;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkIdentityTrackerTests
{
    private readonly Mock<INetworkClient> _mockClient;

    public NetworkIdentityTrackerTests()
    {
        _mockClient = new Mock<INetworkClient>();
        NetworkIdentityTracker.EnsureSubscribed(_mockClient.Object);

        _mockClient.Raise(m => m.OnConnectionStateChanged += null, false);
    }

    [Fact]
    public void InitialState_IsEmpty()
    {
        Assert.Null(NetworkIdentityTracker.GetSelfPlayerId());
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());
        Assert.Empty(NetworkIdentityTracker.GetPlayerIdsSnapshot());
    }

    [Fact]
    public void WelcomeMessage_SetsSelfAndPlayersList()
    {
        var welcomePayload = "{" +
                             "\"PlayerId\":\"player_1\"," +
                             "\"IsHost\":true," +
                             "\"Players\":[" +
                             "  {\"PlayerId\":\"player_1\",\"IsHost\":true}," +
                             "  {\"PlayerId\":\"player_2\",\"IsHost\":false}" +
                             "]" +
                             "}";

        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);

        Assert.Equal("player_1", NetworkIdentityTracker.GetSelfPlayerId());
        Assert.True(NetworkIdentityTracker.GetSelfIsHost());

        var players = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        Assert.Equal(2, players.Count);
        Assert.Contains("player_1", players);
        Assert.Contains("player_2", players);
    }

    [Fact]
    public void WelcomeMessage_WithPlayerListFallback_SetsSelfAndPlayersList()
    {
        var welcomePayload = "{" +
                             "\"PlayerId\":\"player_1\"," +
                             "\"IsHost\":false," +
                             "\"PlayerList\":[" +
                             "  {\"PlayerId\":\"player_1\",\"IsHost\":false}," +
                             "  {\"PlayerId\":\"player_3\",\"IsHost\":true}" +
                             "]" +
                             "}";

        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);

        Assert.Equal("player_1", NetworkIdentityTracker.GetSelfPlayerId());
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());

        var players = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        Assert.Equal(2, players.Count);
        Assert.Contains("player_1", players);
        Assert.Contains("player_3", players);
    }

    [Fact]
    public void HostChangedMessage_UpdatesHostState()
    {

        var welcomePayload = "{\"PlayerId\":\"player_1\",\"IsHost\":false,\"Players\":[]}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());

        var hostChangedPayload = "{\"NewHostId\":\"player_1\"}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.HostChanged, hostChangedPayload);
        Assert.True(NetworkIdentityTracker.GetSelfIsHost());

        var hostChangedPayload2 = "{\"NewHostId\":\"player_2\"}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.HostChanged, hostChangedPayload2);
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());
    }

    [Fact]
    public void PlayerJoined_AddsPlayerToSnapshot()
    {

        var welcomePayload = "{\"PlayerId\":\"player_1\",\"IsHost\":true,\"Players\":[{\"PlayerId\":\"player_1\"}]}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);

        var joinedPayload = "{\"PlayerId\":\"player_new\"}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.PlayerJoined, joinedPayload);

        var players = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        Assert.Equal(2, players.Count);
        Assert.Contains("player_1", players);
        Assert.Contains("player_new", players);
    }

    [Fact]
    public void PlayerLeft_RemovesPlayerFromSnapshot()
    {

        var welcomePayload = "{" +
                             "\"PlayerId\":\"player_1\"," +
                             "\"IsHost\":true," +
                             "\"Players\":[" +
                             "  {\"PlayerId\":\"player_1\"}," +
                             "  {\"PlayerId\":\"player_leave\"}" +
                             "]" +
                             "}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);

        var leftPayload = "{\"PlayerId\":\"player_leave\"}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.PlayerLeft, leftPayload);

        var players = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        Assert.Single(players);
        Assert.Contains("player_1", players);
        Assert.DoesNotContain("player_leave", players);
    }

    [Fact]
    public void PlayerListUpdate_ReplacesPlayersAndUpdatesHost()
    {

        var welcomePayload = "{" +
                             "\"PlayerId\":\"player_1\"," +
                             "\"IsHost\":true," +
                             "\"Players\":[" +
                             "  {\"PlayerId\":\"player_1\",\"IsHost\":true}," +
                             "  {\"PlayerId\":\"player_2\",\"IsHost\":false}" +
                             "]" +
                             "}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);

        var updatePayload = "{" +
                            "\"Players\":[" +
                            "  {\"PlayerId\":\"player_1\",\"IsHost\":false}," +
                            "  {\"PlayerId\":\"player_3\",\"IsHost\":true}" +
                            "]" +
                            "}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.PlayerListUpdate, updatePayload);

        Assert.Equal("player_1", NetworkIdentityTracker.GetSelfPlayerId());
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());

        var players = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        Assert.Equal(2, players.Count);
        Assert.Contains("player_1", players);
        Assert.Contains("player_3", players);
        Assert.DoesNotContain("player_2", players);
    }

    [Fact]
    public void ConnectionStateChanged_False_ClearsAllState()
    {

        var welcomePayload = "{" +
                             "\"PlayerId\":\"player_1\"," +
                             "\"IsHost\":true," +
                             "\"Players\":[" +
                             "  {\"PlayerId\":\"player_1\"}," +
                             "  {\"PlayerId\":\"player_2\"}" +
                             "]" +
                             "}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);

        _mockClient.Raise(m => m.OnConnectionStateChanged += null, false);

        Assert.Null(NetworkIdentityTracker.GetSelfPlayerId());
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());
        Assert.Empty(NetworkIdentityTracker.GetPlayerIdsSnapshot());
    }
}
