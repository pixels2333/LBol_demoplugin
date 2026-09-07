using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class ResurrectSyncTests : IDisposable
{
    private readonly Mock<INetworkClient> _mockClient;
    private readonly Mock<INetworkManager> _mockNetworkManager;
    private readonly Mock<INetworkPlayer> _mockSelfPlayer;
    private readonly Mock<INetworkPlayer> _mockTargetPlayer;
    private readonly ServiceProvider _serviceProvider;

    public ResurrectSyncTests()
    {
        _mockClient = new Mock<INetworkClient>();
        _mockClient.Setup(c => c.IsConnected).Returns(true);

        _mockSelfPlayer = new Mock<INetworkPlayer>();
        _mockSelfPlayer.Setup(p => p.playerId).Returns("host_1");
        _mockSelfPlayer.Setup(p => p.userName).Returns("Host");
        _mockSelfPlayer.Setup(p => p.IsLobbyOwner()).Returns(true);
        _mockSelfPlayer.Setup(p => p.HP).Returns(80);
        _mockSelfPlayer.Setup(p => p.maxHP).Returns(100);

        _mockTargetPlayer = new Mock<INetworkPlayer>();
        _mockTargetPlayer.Setup(p => p.playerId).Returns("client_2");
        _mockTargetPlayer.Setup(p => p.userName).Returns("Client");
        _mockTargetPlayer.Setup(p => p.HP).Returns(30);
        _mockTargetPlayer.Setup(p => p.maxHP).Returns(100);

        _mockNetworkManager = new Mock<INetworkManager>();
        _mockNetworkManager.Setup(m => m.GetSelf()).Returns(_mockSelfPlayer.Object);
        _mockNetworkManager.Setup(m => m.GetPlayer("client_2")).Returns(_mockTargetPlayer.Object);
        _mockNetworkManager.Setup(m => m.GetPlayer("host_1")).Returns(_mockSelfPlayer.Object);
        _mockNetworkManager.Setup(m => m.GetAllPlayers()).Returns(new List<INetworkPlayer> { _mockSelfPlayer.Object, _mockTargetPlayer.Object });

        var services = new ServiceCollection();
        services.AddSingleton(_mockClient.Object);
        services.AddSingleton(_mockNetworkManager.Object);
        _serviceProvider = services.BuildServiceProvider();

        ModService.ServiceProvider = _serviceProvider;

        NetworkIdentityTracker.EnsureSubscribed(_mockClient.Object);
        ResurrectSyncPatch.EnsureSubscribed(_mockClient.Object);

        var welcomePayload = "{" +
                             "\"PlayerId\":\"host_1\"," +
                             "\"IsHost\":true," +
                             "\"Players\":[" +
                             "  {\"PlayerId\":\"host_1\",\"IsHost\":true}," +
                             "  {\"PlayerId\":\"client_2\",\"IsHost\":false}" +
                             "]" +
                             "}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);
    }

    [Fact]
    public void GapHealRequest_WhenValidTarget_BroadcastsOnGapPlayerHealed()
    {
        string broadcastType = null;
        object broadcastPayload = null;

        _mockClient.Setup(c => c.BroadcastState(It.IsAny<string>(), It.IsAny<object>()))
                   .Callback<string, object>((type, payload) =>
                   {
                       broadcastType = type;
                       broadcastPayload = payload;
                   });

        var healRequest = new
        {
            RequestId = "req_123",
            RequesterPlayerId = "host_1",
            TargetPlayerId = "client_2",
            Timestamp = DateTime.UtcNow.Ticks
        };

        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.OnGapHealRequest, JsonCompat.Serialize(healRequest));

        Assert.Equal(NetworkMessageTypes.OnGapPlayerHealed, broadcastType);
        Assert.NotNull(broadcastPayload);
    }

    [Fact]
    public void GapHealRequest_WhenTargetNotFound_BroadcastsOnGapHealFailed()
    {
        string broadcastType = null;
        object broadcastPayload = null;

        _mockClient.Setup(c => c.BroadcastState(It.IsAny<string>(), It.IsAny<object>()))
                   .Callback<string, object>((type, payload) =>
                   {
                       broadcastType = type;
                       broadcastPayload = payload;
                   });

        var healRequest = new
        {
            RequestId = "req_not_found",
            RequesterPlayerId = "host_1",
            TargetPlayerId = "unknown_player",
            Timestamp = DateTime.UtcNow.Ticks
        };

        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.OnGapHealRequest, JsonCompat.Serialize(healRequest));

        Assert.Equal(NetworkMessageTypes.OnGapHealFailed, broadcastType);
        Assert.NotNull(broadcastPayload);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        ModService.ServiceProvider = null;
    }
}
