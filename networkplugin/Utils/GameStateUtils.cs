using System;
using LBoL.Core;
using LBoL.Core.Units;
using NetworkPlugin.Network;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// 获取当前游戏运行状态的工具方法（尽量通过反射保持兼容）。
    /// </summary>
    public static class GameStateUtils
    {
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
                var gameRunControllerType = typeof(GameRunController);
                var instanceProperty = gameRunControllerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                return instanceProperty?.GetValue(null) as GameRunController;
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
