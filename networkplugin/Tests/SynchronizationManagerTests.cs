using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BepInEx.Configuration;
using BepInEx.Logging;
using Moq;
using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NetworkPlugin.Tests;

public class SynchronizationManagerTests : IDisposable
{
    private readonly string _configFilePath;
    private readonly ConfigFile _configFile;
    private readonly ConfigManager _configManager;
    private readonly Mock<INetworkClient> _mockNetworkClient;
    private readonly Mock<ManualLogSource> _mockLogger;
    private readonly NetworkAvailabilityTracker _netAvailTracker;
    private readonly SynchronizationManager _syncManager;

    public SynchronizationManagerTests()
    {
        _configFilePath = Path.Combine(Path.GetTempPath(), $"LBol_Test_{Guid.NewGuid()}.cfg");
        _configFile = new ConfigFile(_configFilePath, saveOnInit: true);
        _configManager = new ConfigManager(_configFile);

        _mockNetworkClient = new Mock<INetworkClient>();
        _mockLogger = new Mock<ManualLogSource>("TestLogger");

        _netAvailTracker = new NetworkAvailabilityTracker(_mockNetworkClient.Object, _mockLogger.Object);
        _syncManager = new SynchronizationManager(
            _mockNetworkClient.Object,
            _netAvailTracker,
            _mockLogger.Object,
            _configManager
        );

        NetworkIdentityTracker.EnsureSubscribed(_mockNetworkClient.Object);

        var welcomePayload = "{\"PlayerId\":\"player_me\",\"IsHost\":false,\"Players\":[{\"PlayerId\":\"player_me\",\"IsHost\":false}]}";
        _mockNetworkClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);
    }

    public void Dispose()
    {
        if (File.Exists(_configFilePath))
        {
            try
            {
                File.Delete(_configFilePath);
            }
            catch
            {

            }
        }
    }

    [Fact]
    public void Constructor_NullDependencies_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SynchronizationManager(null!, _netAvailTracker, _mockLogger.Object, _configManager));
        Assert.Throws<ArgumentNullException>(() => new SynchronizationManager(_mockNetworkClient.Object, null!, _mockLogger.Object, _configManager));
        Assert.Throws<ArgumentNullException>(() => new SynchronizationManager(_mockNetworkClient.Object, _netAvailTracker, null!, _configManager));
        Assert.Throws<ArgumentNullException>(() => new SynchronizationManager(_mockNetworkClient.Object, _netAvailTracker, _mockLogger.Object, null!));
    }

    [Fact]
    public void SyncGameEventToNetwork_NetworkAvailable_SendsEvent()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);
        var gameEvent = new GameEvent("CardPlayed", "player_me", new Dictionary<string, object> { ["CardId"] = "card_1" });

        _syncManager.SyncGameEventToNetwork(gameEvent);

        _mockNetworkClient.Verify(c => c.SendRequest("CardPlayed", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void SyncGameEventToNetwork_NetworkUnavailable_QueuesEvent()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(false);
        var gameEvent = new GameEvent("CardPlayed", "player_me", new Dictionary<string, object> { ["CardId"] = "card_1" });

        _syncManager.SyncGameEventToNetwork(gameEvent);

        _mockNetworkClient.Verify(c => c.SendRequest(It.IsAny<string>(), It.IsAny<object>()), Times.Never);

        var stats = _syncManager.GetSyncStatistics();
        var statsProp = stats.GetType().GetProperty("QueuedEvents");
        Assert.NotNull(statsProp);
        var queuedCount = (int)statsProp.GetValue(stats)!;
        Assert.Equal(1, queuedCount);
    }

    [Fact]
    public void SyncGameEventToNetwork_FilteredEvent_IsIgnored()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);
        _configManager.EnableCardSync.Value = false;
        var gameEvent = new GameEvent(NetworkMessageTypes.OnCardPlayStart, "player_me", new Dictionary<string, object> { ["CardId"] = "card_1" });

        _syncManager.SyncGameEventToNetwork(gameEvent);

        _mockNetworkClient.Verify(c => c.SendRequest(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public void OnConnectionRestored_SendsQueuedEvents_And_RequestsFullSync()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(false);
        var event1 = new GameEvent("CardPlayed", "player_me", new Dictionary<string, object> { ["CardId"] = "card_1" });
        var event2 = new GameEvent("CardPlayed", "player_me", new Dictionary<string, object> { ["CardId"] = "card_2" });

        _syncManager.SyncGameEventToNetwork(event1);
        _syncManager.SyncGameEventToNetwork(event2);

        var statsBefore = _syncManager.GetSyncStatistics();
        var queuedCountBefore = (int)statsBefore.GetType().GetProperty("QueuedEvents")!.GetValue(statsBefore)!;
        Assert.Equal(2, queuedCountBefore);

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);

        _syncManager.OnConnectionRestored();

        _mockNetworkClient.Verify(c => c.SendRequest("CardPlayed", It.IsAny<object>()), Times.Exactly(2));
        _mockNetworkClient.Verify(c => c.SendRequest(NetworkMessageTypes.OnConnectionEstablished.ToString(), It.IsAny<object>()), Times.Once);
        _mockNetworkClient.Verify(c => c.SendRequest(NetworkMessageTypes.FullStateSyncRequest.ToString(), It.IsAny<object>()), Times.Once);

        var statsAfter = _syncManager.GetSyncStatistics();
        var queuedCountAfter = (int)statsAfter.GetType().GetProperty("QueuedEvents")!.GetValue(statsAfter)!;
        Assert.Equal(0, queuedCountAfter);
    }

    [Fact]
    public void OnConnectionLost_SetsUnavailable_And_SwitchesToOfflineMode()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);
        _syncManager.SyncGameEventToNetwork(new GameEvent("Dummy", "player_me", new Dictionary<string, object>()));

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(false);

        _syncManager.OnConnectionLost();

        Assert.False(_netAvailTracker.IsAvailable);

        _mockNetworkClient.Verify(c => c.SendRequest("ConnectionLost", It.IsAny<object>()), Times.Never);
        _mockNetworkClient.Verify(c => c.SendGameEventData("ConnectionLost", It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public void RequestFullSync_ThrottleActive_SkipsSubsequentRequests()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);

        _syncManager.RequestFullSync();
        _mockNetworkClient.Verify(c => c.SendRequest(NetworkMessageTypes.FullStateSyncRequest.ToString(), It.IsAny<object>()), Times.Once);

        _syncManager.RequestFullSync();
        _mockNetworkClient.Verify(c => c.SendRequest(NetworkMessageTypes.FullStateSyncRequest.ToString(), It.IsAny<object>()), Times.Once);

        _netAvailTracker.LastFullSyncRequestAtTicks = DateTime.UtcNow.Ticks - TimeSpan.FromSeconds(5).Ticks;

        _syncManager.RequestFullSync();
        _mockNetworkClient.Verify(c => c.SendRequest(NetworkMessageTypes.FullStateSyncRequest.ToString(), It.IsAny<object>()), Times.Exactly(2));
    }

    private static bool CheckDictValue(object obj, string key, string expectedValue)
    {
        if (obj is Dictionary<string, object> dict && dict.TryGetValue(key, out var val))
        {
            return val?.ToString() == expectedValue;
        }
        return false;
    }

    [Fact]
    public void SendCardPlayEvent_ConstructsCorrectGameEvent()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);

        _syncManager.SendCardPlayEvent("c1", "Strike", "Attack", new[] { 1, 0, 0, 0 }, "EnemySelector", null);

        _mockNetworkClient.Verify(c => c.SendRequest("CardPlayed", It.Is<object>(obj =>
            obj is Dictionary<string, object> &&
            CheckDictValue(obj, "CardId", "c1") &&
            CheckDictValue(obj, "CardName", "Strike")
        )), Times.Once);
    }

    [Fact]
    public void SendManaConsumeEvent_ConstructsCorrectGameEvent_WithConvertedMana()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);

        _syncManager.SendManaConsumeEvent(new[] { 2, 2, 0, 0 }, new[] { 1, 0, 0, 0 }, "CardPlay");

        _mockNetworkClient.Verify(c => c.SendRequest("ManaConsumeStarted", It.Is<object>(obj =>
            obj is Dictionary<string, object> &&
            CheckDictValue(obj, "Source", "CardPlay")
        )), Times.Once);
    }

    [Fact]
    public void SendGapStationEvent_ConstructsCorrectGameEvent()
    {

        _mockNetworkClient.Setup(c => c.IsConnected).Returns(true);

        _syncManager.SendGapStationEvent("GapTradeOptionSelected", "TradeGoods", "FullHealth");

        _mockNetworkClient.Verify(c => c.SendRequest("GapTradeOptionSelected", It.Is<object>(obj =>
            obj is Dictionary<string, object> &&
            CheckDictValue(obj, "OptionData", "TradeGoods") &&
            CheckDictValue(obj, "PlayerState", "FullHealth")
        )), Times.Once);
    }

    [Fact]
    public void ProcessEventFromNetwork_EventEnqueuedAndApplied()
    {

        var rawEvent = new Dictionary<string, object>
        {
            ["EventType"] = "OnDamageDealt",
            ["Payload"] = "10 dmg",
            ["Timestamp"] = DateTime.Now.Ticks,
            ["PlayerName"] = "Bob"
        };

        _syncManager.ProcessEventFromNetwork(rawEvent);

        var stats = _syncManager.GetSyncStatistics();
        var cachedCount = (int)stats.GetType().GetProperty("CachedStates")!.GetValue(stats)!;
        Assert.Equal(1, cachedCount);
    }

    [Fact]
    public void SyncGameEventToNetwork_LargeBatchSingleThread_QueuesAllEvents()
    {

        _configManager.MaxQueueSize.Value = 500;
        var syncManager = new SynchronizationManager(
            _mockNetworkClient.Object,
            _netAvailTracker,
            _mockLogger.Object,
            _configManager
        );
        _mockNetworkClient.Setup(c => c.IsConnected).Returns(false);
        int totalEvents = 400;

        for (int i = 0; i < totalEvents; i++)
        {
            var gameEvent = new GameEvent("CardPlayed", "player_me", new Dictionary<string, object> { ["Index"] = i });
            syncManager.SyncGameEventToNetwork(gameEvent);
        }

        var stats = syncManager.GetSyncStatistics();
        var queuedCount = (int)stats.GetType().GetProperty("QueuedEvents")!.GetValue(stats)!;
        Assert.Equal(totalEvents, queuedCount);
    }

    [Fact]
    public void SyncGameEventToNetwork_ExceedsMaxQueueSize_CapsQueue()
    {

        _configManager.MaxQueueSize.Value = 50;
        var syncManager = new SynchronizationManager(
            _mockNetworkClient.Object,
            _netAvailTracker,
            _mockLogger.Object,
            _configManager
        );
        _mockNetworkClient.Setup(c => c.IsConnected).Returns(false);

        for (int i = 0; i < 100; i++)
        {
            var gameEvent = new GameEvent("CardPlayed", "player_me", new Dictionary<string, object> { ["Index"] = i });
            syncManager.SyncGameEventToNetwork(gameEvent);
        }

        var stats = syncManager.GetSyncStatistics();
        var queuedCount = (int)stats.GetType().GetProperty("QueuedEvents")!.GetValue(stats)!;
        Assert.Equal(50, queuedCount);
    }
}
