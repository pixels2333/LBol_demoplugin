using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Events;

namespace MyFirstPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    // Token: 0x04000001 RID: 1
    private static readonly Harmony harmony = PluginInfo.harmony;

    // Token: 0x04000002 RID: 2
    // internal static ManualLogSource log;
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Loader.SpineLoader.Logger = Logger; // 初始化 SpineLoader 的 Logger
        Patch.Viewer_Loadspine_Patch.Logger = Logger; // 初始化 Patch 的 Logger

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        Plugin.harmony.PatchAll();
        Logger.LogInfo("补丁已加载");
    }
    private void OnDestroy()
    {
        Harmony harmony = Plugin.harmony;
        if (harmony != null)
        {
            harmony.UnpatchSelf();
        }
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is unloaded!");
    }

}
