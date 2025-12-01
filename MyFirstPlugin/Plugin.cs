using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Events;

namespace MyFirstPlugin;

/// <summary>
/// 我的第一个LBoL插件示例
/// 演示如何创建基本的BepInEx插件和Harmony补丁
/// 包含Spine加载器和查看器补丁的初始化
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("LBoL.exe")]
public class Plugin : BaseUnityPlugin
{
    /// <summary>
    /// 静态日志源，用于在整个插件中记录日志信息
    /// </summary>
    internal static new ManualLogSource Logger;

    /// <summary>
    /// Harmony实例，用于运行时方法补丁
    /// 从PluginInfo类获取预配置的Harmony实例
    /// </summary>
    private static readonly Harmony harmony = PluginInfo.harmony;

    /// <summary>
    /// 插件初始化方法
    /// 在插件加载时由BepInEx自动调用
    /// 负责设置日志系统、初始化组件和应用Harmony补丁
    /// </summary>
    private void Awake()
    {
        try
        {
            // 初始化插件日志系统
            Logger = base.Logger;

            // 将日志传递给各个组件，确保统一的日志记录
            Loader.SpineLoader.Logger = Logger; // 初始化 Spine 加载器的日志
            Patch.Viewer_Loadspine_Patch.Logger = Logger; // 初始化查看器补丁的日志

            // 验证GameObject有效性，确保可以进行DontDestroyOnLoad操作
            if (gameObject == null)
            {
                Logger.LogError("GameObject is null, cannot call DontDestroyOnLoad.");
                return;
            }

            // 设置GameObject在场景切换时不被销毁，确保插件持久运行
            DontDestroyOnLoad(gameObject);

            // 记录插件加载完成日志
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // 应用所有Harmony补丁，修改游戏行为
            Plugin.harmony.PatchAll();
            Logger.LogInfo("补丁已加载");
        }
        catch (Exception ex)
        {
            // 捕获并记录初始化过程中的异常
            Logger?.LogError($"Error during plugin initialization: {ex}");
        }
    }

    /// <summary>
    /// Unity的OnDestroy方法，在对象销毁时调用
    /// 负责清理资源和记录卸载日志
    /// </summary>
    private void OnDestroy()
    {
        // 重新获取日志源（因为可能在某些情况下需要重新初始化）
        Logger = base.Logger;

        // 注意：UnpatchSelf()通常不需要手动调用，Harmony会自动处理
        // harmony?.UnpatchSelf();

        // 记录插件卸载日志
        Logger?.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is unloaded!");
    }
}