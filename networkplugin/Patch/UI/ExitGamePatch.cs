using System;
using System.Reflection;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 退出/返回主菜单相关补丁（参照 Together in Spire: ExitGamePatch.java）
/// - 联机时隐藏“放弃存档/弃档”等入口
/// - 联机时将“退出游戏/返回主菜单”改为“断开联机并返回主菜单”
/// - 拦截系统退出（OnWantsToQuit）避免直接关闭进程
/// </summary>
[HarmonyPatch]
public static class ExitGamePatch
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

    private static INetworkManager TryGetNetworkManager()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsMultiplayerConnected()
    {
        return TryGetNetworkClient()?.IsConnected == true;
    }

    private static bool TryIsHost()
    {
        try
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
                return false;

            PropertyInfo prop = client.GetType().GetProperty("IsHost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.PropertyType == typeof(bool))
                return (bool)prop.GetValue(client);

            FieldInfo field = client.GetType().GetField("IsHost", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field?.FieldType == typeof(bool))
                return (bool)field.GetValue(client);
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private static void DisconnectMultiplayer()
    {
        try
        {
            TryGetNetworkClient()?.Stop();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExitGamePatch] Stop network client failed: {ex.Message}");
        }

        try
        {
            INetworkManager manager = TryGetNetworkManager();
            if (manager == null)
                return;

            MethodInfo clear = manager.GetType().GetMethod("ClearAllPlayers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            clear?.Invoke(manager, null);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExitGamePatch] Clear players failed: {ex.Message}");
        }
    }

    private static void DisconnectAndReturnToMainMenu()
    {
        DisconnectMultiplayer();

        try
        {
            if (Singleton<GameMaster>.Instance?.CurrentGameRun != null)
            {
                GameMaster.LeaveGameRun();
            }
            else
            {
                UiManager.GetPanel<MainMenuPanel>()?.RefreshProfile();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExitGamePatch] Return to main menu failed: {ex.Message}");
        }
    }

    private static void ShowConnectedQuitDialog(string textKey)
    {
        try
        {
            bool isHost = TryIsHost();
            string subText = isHost
                ? "联机模式：你是房主，退出将导致房间关闭并影响所有已连接玩家。\n是否继续？"
                : "联机模式：退出将断开联机并返回主菜单。\n之后可以重新连接继续游戏。\n是否继续？";

            UiManager.GetDialog<MessageDialog>().Show(new MessageContent
            {
                TextKey = textKey,
                SubText = subText,
                Buttons = DialogButtons.ConfirmCancel,
                Icon = MessageIcon.Warning,
                OnConfirm = DisconnectAndReturnToMainMenu
            });
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExitGamePatch] Show quit dialog failed: {ex.Message}");
        }
    }

    private static void ShowConnectedAbandonBlockedDialog()
    {
        try
        {
            UiManager.GetDialog<MessageDialog>().Show(new MessageContent
            {
                Text = "联机模式下已禁用“放弃存档/弃档”。\n请先断开联机后再进行该操作。",
                Buttons = DialogButtons.Confirm,
                Icon = MessageIcon.Warning
            });
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExitGamePatch] Show abandon blocked dialog failed: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(MainMenuPanel), "RefreshProfile")]
    [HarmonyPostfix]
    public static void MainMenuPanel_RefreshProfile_Postfix(MainMenuPanel __instance)
    {
        try
        {
            if (!IsMultiplayerConnected())
                return;

            Button abandonButton = Traverse.Create(__instance).Field("abandonGameButton").GetValue<Button>();
            abandonButton?.gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ExitGamePatch] RefreshProfile patch failed: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(MainMenuPanel), "UI_AbandonGameClicked")]
    [HarmonyPrefix]
    public static bool MainMenuPanel_UI_AbandonGameClicked_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        ShowConnectedAbandonBlockedDialog();
        return false;
    }

    [HarmonyPatch(typeof(MainMenuPanel), "UI_QuitGame")]
    [HarmonyPrefix]
    public static bool MainMenuPanel_UI_QuitGame_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        ShowConnectedQuitDialog("QuitGame");
        return false;
    }

    [HarmonyPatch(typeof(SettingPanel), "UI_LeaveGameRun")]
    [HarmonyPrefix]
    public static bool SettingPanel_UI_LeaveGameRun_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        ShowConnectedQuitDialog("ReturnToMainMenu");
        return false;
    }

    [HarmonyPatch(typeof(SettingPanel), "UI_Quit")]
    [HarmonyPrefix]
    public static bool SettingPanel_UI_Quit_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        ShowConnectedQuitDialog("QuitGame");
        return false;
    }

    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.OnWantsToQuit))]
    [HarmonyPrefix]
    public static bool GameMaster_OnWantsToQuit_Prefix(ref bool __result)
    {
        if (!IsMultiplayerConnected())
            return true;

        __result = false;
        ShowConnectedQuitDialog("QuitGame");
        return false;
    }

    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.QuitGame))]
    [HarmonyPrefix]
    public static bool GameMaster_QuitGame_Prefix()
    {
        if (!IsMultiplayerConnected())
            return true;

        DisconnectAndReturnToMainMenu();
        return false;
    }
}

