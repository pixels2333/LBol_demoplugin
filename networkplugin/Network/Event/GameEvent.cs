using System;

namespace NetworkPlugin.Network.Event;

public class GameEvent
{
        public string EventId { get; set; } = Guid.NewGuid().ToString("N");

        public string EventType { get; set; } = "Unknown";

        public long Timestamp { get; set; } = DateTime.Now.Ticks;

        public long EventIndex { get; set; } = 0;

        public string UserName { get; set; } = "unknown";

        public string? TargetId { get; set; }

        public object Data { get; set; }

        public bool IsProcessed { get; set; } = false;

        public string Source { get; set; } = "Unknown";

        public GameEvent() { }

        public GameEvent(string eventType, string playerId,  object data)
    {
        EventType = eventType;
        UserName = playerId;
        Data = data;
        Timestamp = DateTime.Now.Ticks;
    }

}
