using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 法力发送器：ManaConsumeStarted/Completed/ManaRegain（对齐 EnergySyncPatch.cs）。
// SnapshotMana 字段：Red/Blue/Green/White/Colorless/Philosophy/Any/Hybrid/HybridColor/Total。
public class ManaSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public ManaSender(SimNetworkClient client) => _client = client;

    private static object SnapshotMana(int red, int blue, int white) => new
    {
        Red = red,
        Blue = blue,
        Green = 0,
        White = white,
        Colorless = 0,
        Philosophy = 0,
        Any = 0,
        Hybrid = 0,
        HybridColor = "None",
        Total = red + blue + white,
    };

    public void SendManaConsumeStarted(int beforeRed, int beforeBlue, int beforeWhite, int costRed, int costBlue)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            EventType = "ManaConsumeStarted",
            PlayerId = _client.SelfPlayerId,
            BattleId = 0,
            ManaBefore = SnapshotMana(beforeRed, beforeBlue, beforeWhite),
            ManaAfter = SnapshotMana(beforeRed, beforeBlue, beforeWhite),
            Detail = new { Consuming = SnapshotMana(costRed, costBlue, 0) },
        };
        _client.SendGameEventData("ManaConsumeStarted", payload);
    }

    public void SendManaConsumeCompleted(int beforeRed, int beforeBlue, int beforeWhite, int afterRed, int afterBlue, int afterWhite, int costRed, int costBlue)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            EventType = "ManaConsumeCompleted",
            PlayerId = _client.SelfPlayerId,
            BattleId = 0,
            ManaBefore = SnapshotMana(beforeRed, beforeBlue, beforeWhite),
            ManaAfter = SnapshotMana(afterRed, afterBlue, afterWhite),
            Detail = new { Consumed = SnapshotMana(costRed, costBlue, 0) },
        };
        _client.SendGameEventData("ManaConsumeCompleted", payload);
    }

    public void SendManaRegain(int beforeRed, int beforeBlue, int beforeWhite, int afterRed, int afterBlue, int afterWhite)
    {
        var payload = new
        {
            Timestamp = DateTime.Now.Ticks,
            EventType = "ManaRegain",
            PlayerId = _client.SelfPlayerId,
            BattleId = 0,
            ManaBefore = SnapshotMana(beforeRed, beforeBlue, beforeWhite),
            ManaAfter = SnapshotMana(afterRed, afterBlue, afterWhite),
            Detail = new { Requested = SnapshotMana(1, 1, 1), Applied = SnapshotMana(1, 1, 1) },
        };
        _client.SendGameEventData("ManaRegain", payload);
    }

    public void RunRandom()
    {
        int r = Rng.Next(0, 3), b = Rng.Next(0, 3), w = Rng.Next(0, 3);
        SendManaConsumeStarted(r, b, w, 1, 0);
        SendManaConsumeCompleted(r, b, w, Math.Max(0, r - 1), b, w, 1, 0);
    }
}