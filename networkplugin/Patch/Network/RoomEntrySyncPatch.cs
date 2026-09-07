using System;
using HarmonyLib;
using LBoL.Core;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Reconnection;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

[HarmonyPatch]
public static class RoomEntrySyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static ReconnectionManager TryGetReconnectionManager()
        => ServiceProvider?.GetService<ReconnectionManager>();

    private static bool TryGetConnectedHostClient(out INetworkClient client)
    {
        client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            return false;
        }

        NetworkIdentityTracker.EnsureSubscribed(client);
        return NetworkIdentityTracker.GetSelfIsHost();
    }

    private static void TryMarkEnterNodeCheckpoint(MapNode node)
    {
        try
        {
            string nodeKey = $"{node.Act}:{node.X}:{node.Y}:{node.StationType}";
            TryGetReconnectionManager()?.MarkMapCheckpoint("enter_node", nodeKey);
        }
        catch
        {

        }
    }

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

                if (forced)
                {
                    return;
                }

                if (!TryGetConnectedHostClient(out INetworkClient client))
                {
                    return;
                }

                string playerId = NetworkIdentityTracker.GetSelfPlayerId();
                if (string.IsNullOrWhiteSpace(playerId))
                {
                    return;
                }

                TryMarkEnterNodeCheckpoint(node);

                client.SendGameEventData(NetworkMessageTypes.OnMapNodeEnter, new
                {
                    Timestamp = DateTime.UtcNow.Ticks,
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
