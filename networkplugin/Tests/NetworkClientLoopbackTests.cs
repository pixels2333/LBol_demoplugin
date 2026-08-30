using System.Text.Json;
using FluentAssertions;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkClientLoopbackTests
{
    [Fact]
    public void BroadcastState_DispatchesLocalLoopback_ViaMainThreadQueue()
    {
        // Arrange
        var client = new NetworkClient("key", null, null, null);
        string? receivedType = null;
        string? receivedPayload = null;

        client.OnGameEventReceived += (type, payload) =>
        {
            receivedType = type;
            receivedPayload = payload as string;
        };

        var stateData = new { RoomId = "room_123", Stage = 2 };

        // Act
        client.BroadcastState("RoomStateChanged", stateData);

        // Assert: Before flushing main thread queue, event is not yet dispatched (async decoupling)
        receivedType.Should().BeNull();

        // Flush main thread action queue
        Plugin.FlushMainThreadActionsForTest();

        // Assert: After flushing, local loopback event is received
        receivedType.Should().Be("RoomStateChanged");
        receivedPayload.Should().NotBeNull();

        using var doc = JsonDocument.Parse(receivedPayload!);
        doc.RootElement.GetProperty("RoomId").GetString().Should().Be("room_123");
        doc.RootElement.GetProperty("Stage").GetInt32().Should().Be(2);
    }

    [Fact]
    public void BroadcastAction_DoesNotDispatch_LocalLoopback()
    {
        // Arrange
        var client = new NetworkClient("key", null, null, null);
        int callCount = 0;

        client.OnGameEventReceived += (_, _) =>
        {
            callCount++;
        };

        var actionData = new { CardId = "Strike", Target = 1 };

        // Act
        client.BroadcastAction("PlayerCardUsed", actionData);
        Plugin.FlushMainThreadActionsForTest();

        // Assert: Action broadcast should exclude self to prevent echo
        callCount.Should().Be(0);
    }

    [Fact]
    public void SendDirect_WhenTargetIsSelf_DispatchesLocalLoopback()
    {
        // Arrange
        var mockSelf = new Moq.Mock<INetworkPlayer>();
        mockSelf.Setup(p => p.playerId).Returns("player_self");
        var client = new NetworkClient("key", null, mockSelf.Object, null);

        string? receivedType = null;
        string? receivedPayload = null;

        client.OnGameEventReceived += (type, payload) =>
        {
            receivedType = type;
            receivedPayload = payload as string;
        };

        var healData = new { TargetPlayerId = "player_self", HealAmount = 15 };

        // Act: Target is self
        client.SendDirect("player_self", "GapPlayerHealed", healData);
        Plugin.FlushMainThreadActionsForTest();

        // Assert: Direct message to self should loopback locally
        receivedType.Should().Be("GapPlayerHealed");
        receivedPayload.Should().NotBeNull();

        using var doc = JsonDocument.Parse(receivedPayload!);
        doc.RootElement.GetProperty("HealAmount").GetInt32().Should().Be(15);
    }

    [Fact]
    public void SendDirect_WhenTargetIsRemoteAndDisconnected_DoesNotLoopback()
    {
        // Arrange
        var mockSelf = new Moq.Mock<INetworkPlayer>();
        mockSelf.Setup(p => p.playerId).Returns("player_self");
        var client = new NetworkClient("key", null, mockSelf.Object, null);

        int callCount = 0;
        client.OnGameEventReceived += (_, _) =>
        {
            callCount++;
        };

        var tradeData = new { TargetPlayerId = "player_other", Item = "CardA" };

        // Act: Target is other player, client is not connected
        client.SendDirect("player_other", "TradeRequest", tradeData);
        Plugin.FlushMainThreadActionsForTest();

        // Assert: Remote targeted message should not loopback to self
        callCount.Should().Be(0);
    }

    [Fact]
    public void SendGameEventData_WithIncludeSelfTrue_DispatchesLocalLoopback()
    {
        // Arrange
        var client = new NetworkClient("key", null, null, null);
        string? receivedType = null;

        client.OnGameEventReceived += (type, _) =>
        {
            receivedType = type;
        };

        var customOptions = new NetworkEventOptions
        {
            IncludeSelf = true,
            DeliveryMethod = LiteNetLib.DeliveryMethod.ReliableOrdered
        };

        // Act
        client.SendGameEventData("CustomEvent", new { Foo = "Bar" }, customOptions);
        Plugin.FlushMainThreadActionsForTest();

        // Assert
        receivedType.Should().Be("CustomEvent");
    }
}
