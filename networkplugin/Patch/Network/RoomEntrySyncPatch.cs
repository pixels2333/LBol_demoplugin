using System;
using HarmonyLib;
using LBoL.Core;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 房间/地图节点进入同步补丁（参考 Together in Spire 的 RoomEntryPatch 思路）：
/// - 当本地进入一个新的地图节点时，广播一个“进入节点”的事件，供其他客户端更新位置/推进进度。
/// </summary>
[HarmonyPatch]
public static class RoomEntrySyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    [HarmonyPatch(typeof(GameMap), nameof(GameMap.EnterNode))]
    private static class GameMap_EnterNode_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(GameMap __instance, MapNode node, bool freeMove, bool forced)
        {
            try
            {
                if (node == null)
                {
                    return;
                }

                var client = ServiceProvider?.GetService<INetworkClient>();
                if (client == null || !client.IsConnected)
                {
                    return;
                }

                NetworkIdentityTracker.EnsureSubscribed(client);
                string playerId = NetworkIdentityTracker.GetSelfPlayerId();
                if (string.IsNullOrWhiteSpace(playerId))
                {
                    return;
                }

                client.SendGameEventData(NetworkMessageTypes.OnMapNodeEnter, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = playerId,
                    Node = new
                    {
                        node.X,
                        node.Y,
                        node.Act,
                        StationType = node.StationType.ToString(),
                        NodeType = node.GetType().Name,
                        node.Status,
                    },
                    freeMove,
                    forced,
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[RoomEntrySync] Error in GameMap.EnterNode Postfix: {ex.Message}");
            }
        }
    }
}
