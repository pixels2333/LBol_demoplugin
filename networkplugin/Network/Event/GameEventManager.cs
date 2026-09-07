using System.Threading;

namespace NetworkPlugin.Network.Event;

public static class GameEventManager
{
    private static long _nextEventIndex = 0;

        public static GameEvent CreateEvent(string eventType, string userName, object data)
    {
        GameEvent gameEvent = new(eventType, userName, data)
        {
            EventIndex = Interlocked.Increment(ref _nextEventIndex)
        };
        return gameEvent;
    }

        public static long GetCurrentEventIndex()
    {
        return _nextEventIndex;
    }

        public static void ResetEventIndex()
    {
        _nextEventIndex = 0;
    }

        public static void SetEventIndex(long index)
    {
        _nextEventIndex = index;
    }
}
