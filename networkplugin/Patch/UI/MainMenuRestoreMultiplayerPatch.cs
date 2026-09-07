using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

[HarmonyPatch]
public static class MainMenuRestoreMultiplayerPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static void RestoreSinglePlayer(GameRunSaveData save, string logMessage, string errorMessage)
    {
        try
        {
            Plugin.Logger?.LogInfo(logMessage);
            GameMaster.RestoreGameRun(save);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"{errorMessage}: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.UI_RestoreGameClicked))]
    [HarmonyPrefix]
    public static bool MainMenuPanel_UI_RestoreGameClicked_Prefix()
    {
        try
        {
            GameMaster gm = Singleton<GameMaster>.Instance;
            if (gm == null)
            {
                return true;
            }

            if (gm.CurrentGameRun != null)
            {
                return true;
            }

            GameRunSaveData save = gm.GameRunSaveData;
            if (save == null)
            {
                return true;
            }

            INetworkClient client = TryGetNetworkClient();
            if (client?.IsConnected == true)
            {
                return true;
            }

            UiManager.GetDialog<MessageDialog>().Show(
                new MessageContent
                {
                    Text = "检测到可继续的存档。\n\n确认：作为房主继续存档并开启联机\n取消：单人继续",
                    Icon = MessageIcon.Warning,
                    Buttons = DialogButtons.ConfirmCancel,
                    OnConfirm = () =>
                    {
                        try
                        {
                            Plugin.Logger?.LogInfo("[继续游戏] 用户选择：作为房主继续并开启联机");
                            MainMenuMultiplayerEntryPatch.TryHostLocalServerAndConnectAndRestore(save);
                        }
                        catch (Exception ex)
                        {
                            Plugin.Logger?.LogError($"[继续游戏] 作为房主继续失败: {ex.Message}");

                            RestoreSinglePlayer(save, "[继续游戏] 房主继续失败，回退为单人继续", "[继续游戏] 回退为单人继续失败");
                        }
                    },
                    OnCancel = () => RestoreSinglePlayer(save, "[继续游戏] 用户选择：单人继续", "[继续游戏] 单人继续失败"),
                }
            );

            return false;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[继续游戏] 补丁异常: {ex}");
            return true;
        }
    }
}
