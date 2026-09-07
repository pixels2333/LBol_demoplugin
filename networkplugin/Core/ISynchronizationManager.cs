using NetworkPlugin.Network.Event;

namespace NetworkPlugin.Core;

public interface ISynchronizationManager
{
        void SyncGameEventToNetwork(GameEvent gameEvent);

        void ProcessEventFromNetwork(object eventData);

        void SendCardPlayEvent(string cardId, string cardName, string cardType,
        int[] manaCost, string targetSelector, object playerState);

        void SendManaConsumeEvent(int[] manaBefore, int[] manaConsumed, string source);

        void SendGapStationEvent(string eventType, object optionData, object playerState);

        void RequestFullSync();

        void OnConnectionRestored();

        void OnConnectionLost();

        object GetSyncStatistics();

        object GetEventBufferStatistics();

        void SendGameEvent(GameEvent gameEvent);
}
