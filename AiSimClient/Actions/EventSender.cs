using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 事件发送器：OnEventSelection（EventSyncPatch.cs L615）与 OnEventVoteCast（EventSyncPatch.cs L1617）。
public class EventSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public EventSender(SimNetworkClient client) => _client = client;

    public void SendEventSelection(string eventId, int optionIndex, int optionId, string optionText)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            EventId = eventId,
            OptionIndex = optionIndex,
            OptionId = optionId,
            OptionText = optionText,
            OptionResult = "TestResult",
            PlayerId = _client.SelfPlayerId,
        };
        _client.SendGameEventData("OnEventSelection", payload);
    }

    public void SendEventVoteCast(string eventId, int optionIndex)
    {
        var payload = new
        {
            PlayerId = _client.SelfPlayerId,
            EventId = eventId,
            OptionIndex = optionIndex,
            Timestamp = DateTime.Now.Ticks,
        };
        _client.SendGameEventData("OnEventVoteCast", payload);
    }

    public void RunRandom()
    {
        string eventId = $"event_{Rng.Next(1, 20)}";
        int optionIndex = Rng.Next(0, 3);
        SendEventSelection(eventId, optionIndex, optionIndex, $"Option{optionIndex}");
    }
}