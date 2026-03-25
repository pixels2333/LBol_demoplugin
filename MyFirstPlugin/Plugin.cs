using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Events;

namespace MyFirstPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("LBoL.exe")]
/// <summary>
/// LBoL 皮肤插件主入口，负责加载自定义角色皮肤（Spine 骨骼动画）并应用 Harmony 补丁。
/// </summary>
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    // Token: 0x04000001 RID: 1
    private static readonly Harmony harmony = PluginInfo.harmony;

    // Token: 0x04000002 RID: 2
    // internal static ManualLogSource log;
    /// <summary>
    /// BepInEx 插件初始化入口，加载 Logger、初始化 SpineLoader 和补丁，并应用所有 Harmony 补丁。
    /// </summary>
    private void Awake()
    {
        // Plugin startup logic
        try
        {
            Logger = base.Logger;
            Loader.SpineLoader.Logger = Logger; // 初始化 SpineLoader 的 Logger
            Patch.Viewer_Loadspine_Patch.Logger = Logger; // 初始化 Patch 的 Logger
            if (gameObject == null)
            {
                Logger.LogError("GameObject is null, cannot call DontDestroyOnLoad.");
                return;
            }
            DontDestroyOnLoad(gameObject);
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Plugin.harmony.PatchAll();
            Logger.LogInfo("补丁已加载");
        }
        catch (Exception ex)
        {
            Logger?.LogError($"Error during plugin initialization: {ex}");
        }
    }
    /// <summary>
    /// 插件销毁时的清理逻辑，记录插件卸载日志。
    /// </summary>
    private void OnDestroy()
    {
        Logger = base.Logger;
        // harmony?.UnpatchSelf();
        Logger?.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is unloaded!");

    }

}
