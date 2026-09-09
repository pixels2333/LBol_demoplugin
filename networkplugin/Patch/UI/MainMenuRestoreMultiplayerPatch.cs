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

/// <summary>
/// 主菜单原生恢复解耦补丁：
/// 废除原先对 MainMenuPanel.UI_RestoreGameClicked 的联机弹窗拦截。
/// 玩家在主菜单点击“继续游戏”时，始终直接执行原版逻辑恢复单人存档，绝不打扰单人玩家。
/// </summary>
[HarmonyPatch]
public static class MainMenuRestoreMultiplayerPatch
{
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.UI_RestoreGameClicked))]
    [HarmonyPrefix]
    public static bool MainMenuPanel_UI_RestoreGameClicked_Prefix()
    {
        Plugin.Logger?.LogInfo("[MainMenuRestore] 主菜单点击继续游戏，原生进入单人模式。");
        return true;
    }
}
