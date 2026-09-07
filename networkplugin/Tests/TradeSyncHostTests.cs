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
using static NetworkPlugin.Patch.Network.TradeSyncPatch;

namespace NetworkPlugin.Tests;

public class TradeSyncHostTests : IDisposable
{
    private readonly Mock<INetworkClient> _mockClient;
    private readonly Mock<INetworkManager> _mockNetworkManager;
    private readonly Mock<INetworkPlayer> _mockSelfPlayer;
    private readonly ServiceProvider _serviceProvider;

    public TradeSyncHostTests()
    {
        _mockClient = new Mock<INetworkClient>();
        _mockClient.Setup(c => c.IsConnected).Returns(true);
        _mockClient.Setup(c => c.BroadcastState(It.IsAny<string>(), It.IsAny<object>()))
                   .Callback<string, object>((type, payload) =>
                   {
                       string json = JsonCompat.Serialize(payload);
                       _mockClient.Raise(m => m.OnGameEventReceived += null, type, json);
                   });

        _mockSelfPlayer = new Mock<INetworkPlayer>();
        _mockSelfPlayer.Setup(p => p.playerId).Returns("host_1");
        _mockSelfPlayer.Setup(p => p.IsLobbyOwner()).Returns(true);

        _mockNetworkManager = new Mock<INetworkManager>();
        _mockNetworkManager.Setup(m => m.GetSelf()).Returns(_mockSelfPlayer.Object);
        _mockNetworkManager.Setup(m => m.GetPlayerCount()).Returns(2);

        var services = new ServiceCollection();
        services.AddSingleton(_mockClient.Object);
        services.AddSingleton(_mockNetworkManager.Object);
        _serviceProvider = services.BuildServiceProvider();

        ModService.ServiceProvider = _serviceProvider;

        NetworkIdentityTracker.EnsureSubscribed(_mockClient.Object);
        TradeSyncPatch.EnsureSubscribed(_mockClient.Object);

        var welcomePayload = "{" +
                             "\"PlayerId\":\"host_1\"," +
                             "\"IsHost\":true," +
                             "\"Players\":[" +
                             "  {\"PlayerId\":\"host_1\",\"IsHost\":true}," +
                             "  {\"PlayerId\":\"client_2\",\"IsHost\":false}" +
                             "]" +
                             "}";
        _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);
        Plugin.FlushMainThreadActionsForTest();
    }

    public void Dispose()
    {
        Plugin.FlushMainThreadActionsForTest();
        _serviceProvider?.Dispose();
        ModService.ServiceProvider = null;
    }

    [Fact]
    public void RequestStartTrade_WhenHost_DirectlyHandlesLocallyAndUpdatesState()
    {
        string tradeId = "trade_test_" + Guid.NewGuid().ToString("N");
        TradeSessionState receivedState = null;

        Action<TradeSessionState> handler = state =>
        {
            if (state != null && state.TradeId == tradeId)
            {
                receivedState = state;
            }
        };

        TradeSyncPatch.OnTradeStateUpdated += handler;
        try
        {
            TradeSyncPatch.RequestStartTrade(tradeId, "host_1", "client_2", 4);
            Plugin.FlushMainThreadActionsForTest();

            var hostSession = TradeSyncPatch.GetHostSession(tradeId);

            Assert.NotNull(hostSession);
            Assert.NotNull(receivedState);
            Assert.Equal(tradeId, receivedState.TradeId);
            Assert.Equal("host_1", receivedState.PlayerAId);
            Assert.Equal("client_2", receivedState.PlayerBId);
            Assert.Equal(4, receivedState.MaxTradeSlots);
            Assert.Equal(TradeStatus.Open, receivedState.Status);

            var lastKnown = TradeSyncPatch.GetLastKnown(tradeId);
            Assert.NotNull(lastKnown);
            Assert.Equal(TradeStatus.Open, lastKnown.Status);
        }
        finally
        {
            TradeSyncPatch.OnTradeStateUpdated -= handler;
        }
    }

    [Fact]
    public void RequestOfferUpdate_WhenHost_UpdatesLocalStateImmediately()
    {
        string tradeId = "trade_offer_" + Guid.NewGuid().ToString("N");
        TradeSyncPatch.RequestStartTrade(tradeId, "host_1", "client_2", 4);
        Plugin.FlushMainThreadActionsForTest();

        TradeSessionState updatedState = null;
        Action<TradeSessionState> handler = state =>
        {
            if (state != null && state.TradeId == tradeId)
            {
                updatedState = state;
            }
        };

        TradeSyncPatch.OnTradeStateUpdated += handler;
        try
        {
            var offerCards = new List<CardRef>
            {
                new CardRef { CardId = "TestCardA" }
            };

            TradeSyncPatch.RequestOfferUpdate(tradeId, "host_1", offerCards, 150, new List<string> { "ExhibitA" });
            Plugin.FlushMainThreadActionsForTest();

            Assert.NotNull(updatedState);
            Assert.Equal(150, updatedState.MoneyA);
            Assert.Single(updatedState.OfferA);
            Assert.Equal("TestCardA", updatedState.OfferA[0].CardId);
            Assert.Single(updatedState.ExhibitsA);
            Assert.Equal("ExhibitA", updatedState.ExhibitsA[0].ExhibitId);
        }
        finally
        {
            TradeSyncPatch.OnTradeStateUpdated -= handler;
        }
    }

    [Fact]
    public void RequestConfirm_BothParticipants_TransitionsToPreparing()
    {
        string tradeId = "trade_confirm_" + Guid.NewGuid().ToString("N");
        TradeSyncPatch.RequestStartTrade(tradeId, "host_1", "client_2", 4);
        Plugin.FlushMainThreadActionsForTest();

        TradeSessionState currentState = null;
        Action<TradeSessionState> handler = state =>
        {
            if (state != null && state.TradeId == tradeId)
            {
                currentState = state;
            }
        };

        TradeSyncPatch.OnTradeStateUpdated += handler;
        try
        {

            TradeSyncPatch.RequestConfirm(tradeId, "host_1");
            Plugin.FlushMainThreadActionsForTest();

            Assert.NotNull(currentState);
            Assert.True(currentState.AConfirmed);
            Assert.False(currentState.BConfirmed);
            Assert.Equal(TradeStatus.Open, currentState.Status);

            var clientConfirmJson = "{" +
                                   $"\"TradeId\":\"{tradeId}\"," +
                                   "\"RequesterPlayerId\":\"client_2\"" +
                                   "}";
            _mockClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.OnTradeConfirmRequest, clientConfirmJson);
            Plugin.FlushMainThreadActionsForTest();

            Assert.True(currentState.AConfirmed);
            Assert.True(currentState.BConfirmed);
            Assert.Equal(TradeStatus.Preparing, currentState.Status);
        }
        finally
        {
            TradeSyncPatch.OnTradeStateUpdated -= handler;
        }
    }

    [Fact]
    public void NetworkIdentityTracker_UpdatesIdentityOnWelcome()
    {
        Assert.Equal("host_1", NetworkIdentityTracker.GetSelfPlayerId());
        Assert.True(NetworkIdentityTracker.GetSelfIsHost());

        _mockClient.Raise(m => m.OnConnectionStateChanged += null, false);
        Assert.Null(NetworkIdentityTracker.GetSelfPlayerId());
        Assert.False(NetworkIdentityTracker.GetSelfIsHost());
    }

    [Fact]
    public void NetworkManager_InitializesTradeSyncSubscription_ColdBootClientReceivesTradeBroadcast()
    {
        var clientMock = new Mock<INetworkClient>();
        clientMock.Setup(c => c.IsConnected).Returns(true);

        var netMgr = new NetworkManager(clientMock.Object);

        string tradeId = "coldboot_trade_" + Guid.NewGuid().ToString("N");
        TradeSessionState receivedState = null;

        Action<TradeSessionState> handler = state =>
        {
            if (state != null && state.TradeId == tradeId)
            {
                receivedState = state;
            }
        };

        TradeSyncPatch.OnTradeStateUpdated += handler;
        try
        {

            var stateBroadcastJson = "{" +
                                     $"\"TradeId\":\"{tradeId}\"," +
                                     "\"PlayerAId\":\"host_1\"," +
                                     "\"PlayerBId\":\"client_2\"," +
                                     "\"PlayerAName\":\"HostPlayer\"," +
                                     "\"PlayerBName\":\"ClientPlayer\"," +
                                     "\"MaxTradeSlots\":5," +
                                     "\"Status\":\"Open\"," +
                                     "\"AConfirmed\":false," +
                                     "\"BConfirmed\":false" +
                                     "}";

            clientMock.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.OnTradeStateUpdate, stateBroadcastJson);
            Plugin.FlushMainThreadActionsForTest();

            Assert.NotNull(receivedState);
            Assert.Equal(tradeId, receivedState.TradeId);
            Assert.Equal("host_1", receivedState.PlayerAId);
            Assert.Equal("client_2", receivedState.PlayerBId);
            Assert.Equal(TradeStatus.Open, receivedState.Status);

            var lastKnown = TradeSyncPatch.GetLastKnown(tradeId);
            Assert.NotNull(lastKnown);
            Assert.Equal(tradeId, lastKnown.TradeId);
        }
        finally
        {
            TradeSyncPatch.OnTradeStateUpdated -= handler;
        }
    }
}
