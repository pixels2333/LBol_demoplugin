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

        var client = new NetworkClient("key", null, null, null);
        string? receivedType = null;
        string? receivedPayload = null;

        client.OnGameEventReceived += (type, payload) =>
        {
            receivedType = type;
            receivedPayload = payload as string;
        };

        var stateData = new { RoomId = "room_123", Stage = 2 };

        client.BroadcastState("RoomStateChanged", stateData);

        receivedType.Should().BeNull();

        Plugin.FlushMainThreadActionsForTest();

        receivedType.Should().Be("RoomStateChanged");
        receivedPayload.Should().NotBeNull();

        using var doc = JsonDocument.Parse(receivedPayload!);
        doc.RootElement.GetProperty("RoomId").GetString().Should().Be("room_123");
        doc.RootElement.GetProperty("Stage").GetInt32().Should().Be(2);
    }

    [Fact]
    public void BroadcastAction_DoesNotDispatch_LocalLoopback()
    {

        var client = new NetworkClient("key", null, null, null);
        int callCount = 0;

        client.OnGameEventReceived += (_, _) =>
        {
            callCount++;
        };

        var actionData = new { CardId = "Strike", Target = 1 };

        client.BroadcastAction("PlayerCardUsed", actionData);
        Plugin.FlushMainThreadActionsForTest();

        callCount.Should().Be(0);
    }

    [Fact]
    public void SendDirect_WhenTargetIsSelf_DispatchesLocalLoopback()
    {

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

        client.SendDirect("player_self", "GapPlayerHealed", healData);
        Plugin.FlushMainThreadActionsForTest();

        receivedType.Should().Be("GapPlayerHealed");
        receivedPayload.Should().NotBeNull();

        using var doc = JsonDocument.Parse(receivedPayload!);
        doc.RootElement.GetProperty("HealAmount").GetInt32().Should().Be(15);
    }

    [Fact]
    public void SendDirect_WhenTargetIsRemoteAndDisconnected_DoesNotLoopback()
    {

        var mockSelf = new Moq.Mock<INetworkPlayer>();
        mockSelf.Setup(p => p.playerId).Returns("player_self");
        var client = new NetworkClient("key", null, mockSelf.Object, null);

        int callCount = 0;
        client.OnGameEventReceived += (_, _) =>
        {
            callCount++;
        };

        var tradeData = new { TargetPlayerId = "player_other", Item = "CardA" };

        client.SendDirect("player_other", "TradeRequest", tradeData);
        Plugin.FlushMainThreadActionsForTest();

        callCount.Should().Be(0);
    }

    [Fact]
    public void SendGameEventData_WithIncludeSelfTrue_DispatchesLocalLoopback()
    {

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

        client.SendGameEventData("CustomEvent", new { Foo = "Bar" }, customOptions);
        Plugin.FlushMainThreadActionsForTest();

        receivedType.Should().Be("CustomEvent");
    }
}
