using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Core;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 存档重定向补丁：
/// 1. 挂钩 GameMaster.SaveGameRun。在多人模式下拦截原版 game{slot}.sav 写入，重定向保存至 MultiplayerSaveManager。
/// 2. 挂钩 GameMaster.SaveProfileWithEndingGameRun。对局结束时自动清理多人专属存档。
/// </summary>
[HarmonyPatch]
public static class GameMasterSaveHookPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

    /// <summary>
    /// 标记当前是否处于多人对局生命周期中。
    /// </summary>
    public static bool IsMultiplayerRunActive { get; set; }

    /// <summary>
    /// 判断当前是否处于多人联机活动中。
    /// </summary>
    public static bool IsMultiplayerActive()
    {
        var currentRun = Singleton<GameMaster>.Instance?.CurrentGameRun;
        if (currentRun == null)
        {
            return false;
        }

        if (IsMultiplayerRunActive)
        {
            return true;
        }

        INetworkClient client = TryGetNetworkClient();
        if (client != null && client.IsConnected)
        {
            return true;
        }

        try
        {
            if (MultiplayerSaveManager.HasMultiplayerSave())
            {
                var save = MultiplayerSaveManager.LoadMultiplayerSave();
                if (save != null && currentRun.RootSeed == save.RootSeed)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[GameMasterSaveHookPatch] IsMultiplayerActive 检查存档种子异常: {ex.Message}");
        }

        return false;
    }

    [HarmonyPatch(typeof(GameMaster), "SaveGameRun", new Type[] { typeof(GameRunSaveData), typeof(bool) })]
    [HarmonyPrefix]
    public static bool SaveGameRun_Prefix(GameMaster __instance, GameRunSaveData data, bool normalSave)
    {
        try
        {
            if (!IsMultiplayerActive())
            {
                // 单人模式：放行原版保存逻辑（写入 game{slot}.sav）
                return true;
            }

            if (data == null)
            {
                return false;
            }

            data.SaveTimestamp = LBoL.Core.Utils.ToIso8601Timestamp(DateTime.Now);
            if (normalSave)
            {
                data.PlayedSeconds = Singleton<GameMaster>.Instance.CurrentGameRunPlayedSeconds;
            }
            VersionInfo versionInfo = VersionInfo.Current;
            data.GameVersion = versionInfo.Version;
            data.GameRevision = versionInfo.Revision;

            int slot = __instance.CurrentSaveIndex ?? 0;
            MultiplayerSaveManager.SaveMultiplayerSave(data, slot);

            // 房主端自动更新 Roster 名单
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                INetworkManager netManager = ServiceProvider?.GetService<INetworkManager>();
                string hostId = NetworkIdentityTracker.GetSelfPlayerId() ?? "Host";
                string hostName = netManager?.GetSelf()?.userName;
                string chara = data.Player?.Name ?? "Reimu";
                MultiplayerSaveManager.RecordRoomPlayers(data.RootSeed, hostId, hostName, chara, netManager?.GetAllPlayers());
            }

            Traverse.Create(__instance).Property("GameRunSaveData").SetValue(data);
            if (normalSave)
            {
                UiManager.GetPanel<SystemBoard>()?.ShowGameSaveHint();
            }

            Plugin.Logger?.LogInfo($"[GameMasterSaveHookPatch] 拦截多人模式 SaveGameRun，已重定向写入多人专属存档 (Slot {slot}, Seed {data.RootSeed})");
            return false; // 跳过原版 game{slot}.sav 写入，实现完全物理隔离
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameMasterSaveHookPatch] SaveGameRun_Prefix 异常: {ex}");
            return true;
        }
    }

    /// <summary>
    /// 拦截原版删除存档，防止多人对局结算时误删原版单人 game{slot}.sav 存档。
    /// </summary>
    [HarmonyPatch(typeof(GameMaster), "TryDeleteSaveData", new Type[] { typeof(string), typeof(bool) })]
    [HarmonyPrefix]
    public static bool TryDeleteSaveData_Prefix(string filename)
    {
        try
        {
            if (IsMultiplayerActive())
            {
                int slot = Singleton<GameMaster>.Instance?.CurrentSaveIndex ?? 0;
                string singlePlayerSaveName = $"game{slot}.sav";
                if (string.Equals(filename, singlePlayerSaveName, StringComparison.OrdinalIgnoreCase) ||
                    (filename != null && filename.StartsWith("game", StringComparison.OrdinalIgnoreCase) && filename.EndsWith(".sav", StringComparison.OrdinalIgnoreCase)))
                {
                    Plugin.Logger?.LogInfo($"[GameMasterSaveHookPatch] 多人模式运行中，拦截删除单人原生存档文件: {filename}");
                    return false; // 拦截删除，保护单机存档完好无损
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameMasterSaveHookPatch] TryDeleteSaveData_Prefix 异常: {ex}");
        }
        return true;
    }

    [HarmonyPatch(typeof(GameMaster), "SaveProfileWithEndingGameRun", new Type[] { typeof(GameRunController), typeof(int), typeof(GameRunRecordSaveData) })]
    [HarmonyPostfix]
    public static void SaveProfileWithEndingGameRun_Postfix(GameMaster __instance, GameRunController gameRun)
    {
        try
        {
            if (IsMultiplayerActive())
            {
                int slot = __instance.CurrentSaveIndex ?? 0;
                Plugin.Logger?.LogInfo($"[GameMasterSaveHookPatch] 多人对局结束 (SaveProfileWithEndingGameRun)，已清理多人专属存档 (Slot {slot})。");
                MultiplayerSaveManager.DeleteMultiplayerSave(slot);
                if (gameRun != null && gameRun.RootSeed != 0)
                {
                    MultiplayerSaveManager.DeleteRoster(gameRun.RootSeed);
                }
                IsMultiplayerRunActive = false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameMasterSaveHookPatch] SaveProfileWithEndingGameRun_Postfix 异常: {ex}");
        }
    }

    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.LeaveGameRun))]
    [HarmonyPrefix]
    public static void LeaveGameRun_Prefix()
    {
        IsMultiplayerRunActive = false;
    }

    /// <summary>
    /// 主菜单刷新档案时，确保重置联机标记并将 GameMaster.GameRunSaveData 对齐回真实的单人存档（game{slot}.sav），
    /// 确保多人存档绝不泄漏至主菜单。
    /// </summary>
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.RefreshProfile))]
    [HarmonyPrefix]
    public static void MainMenuPanel_RefreshProfile_Prefix()
    {
        try
        {
            IsMultiplayerRunActive = false;
            var gm = Singleton<GameMaster>.Instance;
            if (gm != null && gm.CurrentSaveIndex.HasValue)
            {
                GameMaster.TryLoadGameRunSaveData(gm.CurrentSaveIndex.Value, out var singlePlayerSave);
                Traverse.Create(gm).Property("GameRunSaveData").SetValue(singlePlayerSave);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[GameMasterSaveHookPatch] MainMenuPanel_RefreshProfile_Prefix 刷新单人存档异常: {ex}");
        }
    }
}
