using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.UI;

[HarmonyPatch]
public static class LobbySyncPatches
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    [HarmonyPatch(typeof(StartGamePanel), "SelectPlayer")]
    [HarmonyPostfix]
    public static void StartGamePanel_SelectPlayer_Postfix(StartGamePanel __instance, int index)
    {
        try
        {
            var players = Traverse.Create(__instance).Field("_players").GetValue<List<PlayerUnit>>();
            if (players != null && index >= 0 && index < players.Count)
            {
                PlayerUnit selectedPlayer = players[index];
                string characterId = selectedPlayer?.Id;
                if (string.IsNullOrWhiteSpace(characterId)) return;

                INetworkClient client = TryGetNetworkClient();
                if (client != null && client.IsConnected)
                {
                    // 更新本地 NetworkPlayer
                    var networkManager = ServiceProvider?.GetService<NetworkManager>();
                    if (networkManager != null)
                    {
                        var selfPlayer = networkManager.GetSelf();
                        if (selfPlayer != null)
                        {
                            selfPlayer.chara = characterId;
                        }
                    }

                    // 广播 CharacterId 给服务器 (通过 UpdatePlayerLocation，服务器能自动更新 s.Metadata["CharacterId"] 并广播更新列表)
                    client.SendRequest(
                        "UpdatePlayerLocation",
                        JsonCompat.Serialize(
                            new
                            {
                                CharacterId = characterId
                            }
                        )
                    );
                    Plugin.Logger?.LogInfo($"[Lobby] 本地玩家切换角色为: {characterId}，已广播同步给服务器和房间成员。");

                    // 刷新房间列表 UI 呈现最新角色
                    MainMenuMultiplayerEntryPatch.RefreshRoomList();
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[Lobby] SelectPlayer Postfix 失败: {ex}");
        }
    }
}
