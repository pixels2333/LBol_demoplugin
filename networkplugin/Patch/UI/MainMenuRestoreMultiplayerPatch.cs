using System;
using System.Collections;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 主菜单“继续游戏”补丁：如果存在可继续的存档，提供“单人继续 / 作为房主继续并开启联机”的选择。
///
/// 说明：原版 MainMenuPanel.UI_RestoreGameClicked 会直接调用 GameMaster.RestoreGameRun，
/// 如果此时网络已断开，则会自然进入单人模式。
/// </summary>
[HarmonyPatch]
public static class MainMenuRestoreMultiplayerPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
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

            // 已在局内：尊重原逻辑（原版会给出“AlreadyInGameRun”提示）。
            if (gm.CurrentGameRun != null)
            {
                return true;
            }

            GameRunSaveData save = gm.GameRunSaveData;
            if (save == null)
            {
                return true;
            }

            // 已联机：继续走原逻辑即可（当前目标是修复“断开后继续会变单人”的情况）。
            INetworkClient client = TryGetNetworkClient();
            if (client?.IsConnected == true)
            {
                return true;
            }

            // 弹窗：让用户明确选择继续方式。
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
                            // 降级：至少尝试单人继续。
                            try
                            {
                                GameMaster.RestoreGameRun(save);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    },
                    OnCancel = () =>
                    {
                        try
                        {
                            Plugin.Logger?.LogInfo("[继续游戏] 用户选择：单人继续");
                            GameMaster.RestoreGameRun(save);
                        }
                        catch (Exception ex)
                        {
                            Plugin.Logger?.LogError($"[继续游戏] 单人继续失败: {ex.Message}");
                        }
                    },
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
