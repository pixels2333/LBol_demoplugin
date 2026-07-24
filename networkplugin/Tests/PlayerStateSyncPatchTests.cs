using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests
{
    public class PlayerStateSyncPatchTests : IDisposable
    {
        private readonly Mock<INetworkClient> _mockNetworkClient;
        private readonly ServiceProvider _serviceProvider;

        public PlayerStateSyncPatchTests()
        {
            _mockNetworkClient = new Mock<INetworkClient>();
            
            // Build simple ServiceProvider for DI dependency resolution
            var services = new ServiceCollection();
            services.AddSingleton(_mockNetworkClient.Object);
            _serviceProvider = services.BuildServiceProvider();
            
            ModService.ServiceProvider = _serviceProvider;

            // Reset GameStateUtils before each test
            GameStateUtils.ResetProviders();
        }

        public void Dispose()
        {
            GameStateUtils.ResetProviders();
            _serviceProvider?.Dispose();
            ModService.ServiceProvider = null;
        }

        private void SetupNetwork(bool isConnected, string selfPlayerId, bool isHost = true)
        {
            _mockNetworkClient.Setup(c => c.IsConnected).Returns(isConnected);
            
            // Subscribing the mock client to NetworkIdentityTracker
            NetworkIdentityTracker.EnsureSubscribed(_mockNetworkClient.Object);
            
            // Send welcome message payload to initialize self player id
            var welcomePayload = $"{{\"PlayerId\":\"{selfPlayerId}\",\"IsHost\":{isHost.ToString().ToLower()},\"Players\":[]}}";
            _mockNetworkClient.Raise(m => m.OnGameEventReceived += null, NetworkMessageTypes.Welcome, welcomePayload);
        }

        [Fact]
        public void GainMoney_ShouldSendUpdateEvent_WhenMoneyChanges()
        {
            // Arrange
            SetupNetwork(isConnected: true, selfPlayerId: "player_me");
            
            // Create mock player & game run using GameMockFactory
            var mockPlayer = GameMockFactory.CreateMockPlayer(id: "char_reimu", name: "Reimu");
            var mockGameRun = GameMockFactory.CreateMockGameRun(player: mockPlayer, money: 100);

            // Register GameStateUtils delegation providers
            GameStateUtils.GetCurrentGameRunProvider = () => mockGameRun;
            GameStateUtils.GetCurrentPlayerProvider = () => mockPlayer;
            GameStateUtils.GetCurrentPlayerIdProvider = () => "char_reimu";

            // Verify GameStateUtils mock returns correct values
            Assert.Same(mockGameRun, GameStateUtils.GetCurrentGameRun());
            Assert.Equal("char_reimu", GameStateUtils.GetCurrentPlayerId());
            Assert.Equal(100, mockGameRun.Money);

            // Act
            // Prefix records the current state (100)
            PlayerStateSyncPatch.GameRun_GainMoney_Sync.Prefix(mockGameRun, 50, out int beforeState);
            Assert.Equal(100, beforeState);

            // Simulate the game changing the money state from 100 to 150
            GameMockFactory.SetPrivateFieldOrProperty(mockGameRun, "Money", 150);
            Assert.Equal(150, mockGameRun.Money);

            // Postfix should detect the change and send network sync
            PlayerStateSyncPatch.GameRun_GainMoney_Sync.Postfix(mockGameRun, 50, beforeState);

            // Assert
            _mockNetworkClient.Verify(c => c.SendGameEventData(
                NetworkMessageTypes.OnPlayerStateUpdate, 
                It.Is<object>(obj => 
                    VerifyPayload(obj, "MoneyGained", 100, 150, 50, "char_reimu")
                )
            ), Times.Once);
        }

        [Fact]
        public void GainMoney_ShouldNotSendEvent_WhenMoneyIsSame()
        {
            // Arrange
            SetupNetwork(isConnected: true, selfPlayerId: "player_me");
            var mockPlayer = GameMockFactory.CreateMockPlayer(id: "char_reimu");
            var mockGameRun = GameMockFactory.CreateMockGameRun(player: mockPlayer, money: 100);

            GameStateUtils.GetCurrentGameRunProvider = () => mockGameRun;
            GameStateUtils.GetCurrentPlayerProvider = () => mockPlayer;

            // Act
            PlayerStateSyncPatch.GameRun_GainMoney_Sync.Prefix(mockGameRun, 0, out int beforeState);
            // Money doesn't change
            PlayerStateSyncPatch.GameRun_GainMoney_Sync.Postfix(mockGameRun, 0, beforeState);

            // Assert
            _mockNetworkClient.Verify(c => c.SendGameEventData(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GainMoney_ShouldNotSendEvent_WhenDisconnected()
        {
            // Arrange
            SetupNetwork(isConnected: false, selfPlayerId: "player_me");
            var mockPlayer = GameMockFactory.CreateMockPlayer();
            var mockGameRun = GameMockFactory.CreateMockGameRun(player: mockPlayer, money: 100);

            GameStateUtils.GetCurrentGameRunProvider = () => mockGameRun;
            GameStateUtils.GetCurrentPlayerProvider = () => mockPlayer;

            // Act
            PlayerStateSyncPatch.GameRun_GainMoney_Sync.Prefix(mockGameRun, 50, out int beforeState);
            GameMockFactory.SetPrivateFieldOrProperty(mockGameRun, "Money", 150);
            PlayerStateSyncPatch.GameRun_GainMoney_Sync.Postfix(mockGameRun, 50, beforeState);

            // Assert
            _mockNetworkClient.Verify(c => c.SendGameEventData(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void ConsumeMoney_ShouldSendUpdateEvent_WhenCostIncurred()
        {
            // Arrange
            SetupNetwork(isConnected: true, selfPlayerId: "player_me");
            var mockPlayer = GameMockFactory.CreateMockPlayer(id: "char_marisa", name: "Marisa");
            var mockGameRun = GameMockFactory.CreateMockGameRun(player: mockPlayer, money: 200);

            GameStateUtils.GetCurrentGameRunProvider = () => mockGameRun;
            GameStateUtils.GetCurrentPlayerProvider = () => mockPlayer;
            GameStateUtils.GetCurrentPlayerIdProvider = () => "char_marisa";

            // Act
            PlayerStateSyncPatch.GameRun_ConsumeMoney_Sync.Prefix(mockGameRun, 50, out int beforeState);
            Assert.Equal(200, beforeState);

            GameMockFactory.SetPrivateFieldOrProperty(mockGameRun, "Money", 150);
            PlayerStateSyncPatch.GameRun_ConsumeMoney_Sync.Postfix(mockGameRun, 50, beforeState);

            // Assert
            _mockNetworkClient.Verify(c => c.SendGameEventData(
                NetworkMessageTypes.OnPlayerStateUpdate, 
                It.Is<object>(obj => 
                    VerifyPayload(obj, "MoneyConsumed", 200, 150, -50, "char_marisa")
                )
            ), Times.Once);
        }

        [Fact]
        public void LoseMoney_ShouldSendUpdateEvent_WhenMoneyLost()
        {
            // Arrange
            SetupNetwork(isConnected: true, selfPlayerId: "player_me");
            var mockPlayer = GameMockFactory.CreateMockPlayer(id: "char_koishi", name: "Koishi");
            var mockGameRun = GameMockFactory.CreateMockGameRun(player: mockPlayer, money: 300);

            GameStateUtils.GetCurrentGameRunProvider = () => mockGameRun;
            GameStateUtils.GetCurrentPlayerProvider = () => mockPlayer;
            GameStateUtils.GetCurrentPlayerIdProvider = () => "char_koishi";

            // Act
            PlayerStateSyncPatch.GameRun_LoseMoney_Sync.Prefix(mockGameRun, 80, out int beforeState);
            Assert.Equal(300, beforeState);

            GameMockFactory.SetPrivateFieldOrProperty(mockGameRun, "Money", 220);
            PlayerStateSyncPatch.GameRun_LoseMoney_Sync.Postfix(mockGameRun, 80, beforeState);

            // Assert
            _mockNetworkClient.Verify(c => c.SendGameEventData(
                NetworkMessageTypes.OnPlayerStateUpdate, 
                It.Is<object>(obj => 
                    VerifyPayload(obj, "MoneyLost", 300, 220, -80, "char_koishi")
                )
            ), Times.Once);
        }

        private static bool VerifyPayload(object obj, string expectedUpdateType, int before, int after, int delta, string playerUnitId)
        {
            if (obj == null) return false;
            
            // Use reflection to verify properties of the anonymous object payload
            var type = obj.GetType();
            var updateType = type.GetProperty("UpdateType")?.GetValue(obj) as string;
            var payloadPlayerId = type.GetProperty("PlayerUnitId")?.GetValue(obj) as string;
            var payloadDelta = (int)(type.GetProperty("Delta")?.GetValue(obj) ?? 0);
            
            var beforeObj = type.GetProperty("Before")?.GetValue(obj);
            var afterObj = type.GetProperty("After")?.GetValue(obj);
            
            if (beforeObj == null || afterObj == null) return false;
            
            var beforeMoney = (int)(beforeObj.GetType().GetProperty("Money")?.GetValue(beforeObj) ?? 0);
            var afterMoney = (int)(afterObj.GetType().GetProperty("Money")?.GetValue(afterObj) ?? 0);

            return updateType == expectedUpdateType &&
                   payloadPlayerId == playerUnitId &&
                   payloadDelta == delta &&
                   beforeMoney == before &&
                   afterMoney == after;
        }
    }
}
