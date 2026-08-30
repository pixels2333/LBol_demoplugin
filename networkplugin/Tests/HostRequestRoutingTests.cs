using System;
using FluentAssertions;
using NetworkPlugin.Network.Messages;
using Xunit;

namespace NetworkPlugin.Tests;

public class HostRequestRoutingTests
{
    [Theory]
    [InlineData(NetworkMessageTypes.OnTradeStartRequest, true)]
    [InlineData(NetworkMessageTypes.OnTradeOfferUpdateRequest, true)]
    [InlineData(NetworkMessageTypes.OnTradeConfirmRequest, true)]
    [InlineData(NetworkMessageTypes.OnTradeCancelRequest, true)]
    [InlineData(NetworkMessageTypes.OnTradeSnapshotRequest, true)]
    [InlineData(NetworkMessageTypes.OnTradePrepareResultRequest, true)]
    [InlineData(NetworkMessageTypes.OnResurrectRequest, true)]
    [InlineData(NetworkMessageTypes.OnGapHealRequest, true)]
    [InlineData(NetworkMessageTypes.OnMapNodeVoteCast, true)]
    [InlineData(NetworkMessageTypes.OnEventVoteCast, true)]
    [InlineData(NetworkMessageTypes.MidGameJoinRequest, true)]
    [InlineData(NetworkMessageTypes.OnTradeStateUpdate, false)]
    [InlineData(NetworkMessageTypes.OnPlayerStateUpdate, false)]
    [InlineData(NetworkMessageTypes.OnCardPlayStart, false)]
    [InlineData(NetworkMessageTypes.ChatMessage, false)]
    public void IsHostRequest_CorrectlyIdentifiesHostRequests(string messageType, bool expected)
    {
        bool result = NetworkMessageTypes.IsHostRequest(messageType);
        result.Should().Be(expected);
    }
}
