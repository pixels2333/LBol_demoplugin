using System;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Core.Units;
using NetworkPlugin.Network;
using UnityEngine;

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
            try
            {
                // Avoid creating a new singleton instance implicitly.
                if (_cachedGameMaster != null)
                {
                    return _cachedGameMaster;
                }

                _cachedGameMaster = Object.FindObjectOfType<GameMaster>();
                return _cachedGameMaster;
            }
            catch
            {
                return null;
            }
        }

        public static PlayerUnit GetCurrentPlayer()
        {
            try
            {
                var gameRun = GetCurrentGameRun();
                return gameRun?.Player;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting current player: {ex.Message}");
                return null;
            }
        }

        public static string GetCurrentPlayerId()
        {
            var player = GetCurrentPlayer();
            return player?.Id ?? "unknown_player";
        }

        public static GameRunController GetCurrentGameRun()
        {
            try
            {
                // LBoL run lives on GameMaster; this avoids relying on an Instance property that may not exist.
                GameMaster gm = TryGetGameMaster();
                return gm?.CurrentGameRun;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting current game run: {ex.Message}");
                return null;
            }
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

                var networkClient = serviceProvider.GetService(typeof(NetworkPlugin.Network.Client.INetworkClient));
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
    }
}
