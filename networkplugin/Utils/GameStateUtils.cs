using System;
using System.Reflection;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Core.Units;
using NetworkPlugin.Network.Services;

namespace NetworkPlugin.Utils
{
        public static class GameStateUtils
    {
        private static GameMaster _cachedGameMaster;

        public static Func<PlayerUnit> GetCurrentPlayerProvider { get; set; } = DefaultGetCurrentPlayer;
        public static Func<string> GetCurrentPlayerIdProvider { get; set; } = DefaultGetCurrentPlayerId;
        public static Func<string> GetCurrentPlayerNameProvider { get; set; } = DefaultGetCurrentPlayerName;
        public static Func<GameRunController> GetCurrentGameRunProvider { get; set; } = DefaultGetCurrentGameRun;
        public static Func<bool> IsHostProvider { get; set; } = DefaultIsHost;

        public static void ResetProviders()
        {
            GetCurrentPlayerProvider = DefaultGetCurrentPlayer;
            GetCurrentPlayerIdProvider = DefaultGetCurrentPlayerId;
            GetCurrentPlayerNameProvider = DefaultGetCurrentPlayerName;
            GetCurrentGameRunProvider = DefaultGetCurrentGameRun;
            IsHostProvider = DefaultIsHost;
        }

        public static PlayerUnit GetCurrentPlayer() => GetCurrentPlayerProvider();
        private static PlayerUnit DefaultGetCurrentPlayer()
        {
            var gameRun = GetCurrentGameRun();
            return gameRun?.Player;
        }

        public static string GetCurrentPlayerId() => GetCurrentPlayerIdProvider();
        private static string DefaultGetCurrentPlayerId()
        {
            string netId = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(netId))
            {
                return netId;
            }
            var player = GetCurrentPlayer();
            return player?.Id ?? "unknown_player";
        }

        public static string GetCurrentPlayerName() => GetCurrentPlayerNameProvider();
        private static string DefaultGetCurrentPlayerName()
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

        public static GameRunController GetCurrentGameRun() => GetCurrentGameRunProvider();
        private static GameRunController DefaultGetCurrentGameRun()
        {
            _ = TryGetCurrentGameRun(out GameRunController run, out _);
            return run;
        }

        public static bool TryGetCurrentGameRun(out GameRunController run, out string source)
        {

            if (_cachedGameMaster == null)
            {
                _cachedGameMaster = UnityEngine.Object.FindObjectOfType<GameMaster>();
            }
            GameMaster gm = _cachedGameMaster;
            run = gm?.CurrentGameRun;
            if (run != null)
            {
                source = "GameMaster.CurrentGameRun";
                return true;
            }

            try
            {
                UiPanelBase[] panels = UnityEngine.Object.FindObjectsOfType<UiPanelBase>(true);
                FieldInfo gameRunField = typeof(UiPanelBase).GetField("_gameRun", BindingFlags.Instance | BindingFlags.NonPublic);
                if (panels == null || gameRunField == null)
                {
                    source = gameRunField == null ? "MissingUiPanelBase._gameRun" : "UiPanelBase.None";
                    run = null;
                    return false;
                }

                foreach (UiPanelBase panel in panels)
                {
                    if (panel == null)
                    {
                        continue;
                    }

                    if (gameRunField.GetValue(panel) is WeakReference<GameRunController> weakRef
                        && weakRef.TryGetTarget(out GameRunController panelRun)
                        && panelRun != null)
                    {
                        run = panelRun;
                        source = $"UiPanelBase:{panel.GetType().Name}";
                        return true;
                    }
                }

                source = "UiPanelBase.None";
                run = null;
                return false;
            }
            catch
            {

            }

            source = "Unavailable";
            run = null;
            return false;
        }

        public static bool IsHost() => IsHostProvider();
        private static bool DefaultIsHost()
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
