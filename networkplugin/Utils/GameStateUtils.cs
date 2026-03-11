using System;
using System.Reflection;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Core.Units;
using NetworkPlugin.Network;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// 获取当前游戏运行状态的工具方法（尽量通过反射保持兼容）。
    /// </summary>
    public static class GameStateUtils
    {
        private static GameMaster _cachedGameMaster;

        private static GameMaster TryGetGameMaster()
        {
            if (_cachedGameMaster != null)
            {
                return _cachedGameMaster;
            }

            // 避免隐式创建新的单例实例。
            _cachedGameMaster = UnityEngine.Object.FindObjectOfType<GameMaster>();
            return _cachedGameMaster;
        }

        public static PlayerUnit GetCurrentPlayer()
        {
            var gameRun = GetCurrentGameRun();
            return gameRun?.Player;
        }

        public static string GetCurrentPlayerId()
        {
            var player = GetCurrentPlayer();
            return player?.Id ?? "unknown_player";
        }

        public static string GetCurrentPlayerName()
        {
            PlayerUnit player = GetCurrentPlayer();
            if (player != null)
            {
                string directName = TryGetPlayerStringProperty(player,
                    "playerName",
                    "PlayerName",
                    "userName",
                    "UserName",
                    "Name");

                if (!string.IsNullOrWhiteSpace(directName))
                {
                    return directName;
                }

                string fallbackName = TryGetPlayerStringProperty(player, "ModelName", "Id");
                if (!string.IsNullOrWhiteSpace(fallbackName))
                {
                    return fallbackName;
                }
            }

            return GetCurrentPlayerId();
        }

        public static GameRunController GetCurrentGameRun()
        {
            // LBoL 的 run 挂在 GameMaster 上，这样就不用依赖可能不存在的 Instance 属性。
            GameMaster gm = TryGetGameMaster();
            return gm?.CurrentGameRun;
        }

        public static bool IsHost()
        {
            try
            {
                var serviceProvider = ModService.ServiceProvider;
                if (serviceProvider == null)
                {
                    return true;
                }

                var networkClient = serviceProvider.GetService(typeof(Network.Client.INetworkClient));
                if (networkClient == null)
                {
                    return true;
                }

                var prop = networkClient.GetType().GetProperty("IsHost");
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    return (bool)prop.GetValue(networkClient);
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error checking host status: {ex.Message}");
                return false;
            }
        }

        private static string TryGetPlayerStringProperty(PlayerUnit player, params string[] propertyNames)
        {
            if (player == null)
            {
                return null;
            }

            Type playerType = player.GetType();
            foreach (string propertyName in propertyNames)
            {
                PropertyInfo property = playerType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    continue;
                }

                object value;
                try
                {
                    value = property.GetValue(player);
                }
                catch
                {
                    continue;
                }

                if (value is string text && !string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }

                if (value != null)
                {
                    string fallback = value.ToString();
                    if (!string.IsNullOrWhiteSpace(fallback))
                    {
                        return fallback;
                    }
                }
            }

            return null;
        }
    }
}
