using System;
using HarmonyLib;

namespace NetworkPlugin;

/// <summary>
/// LBoL联机MOD的插件信息类
/// 包含插件的基本标识信息和Harmony补丁实例
/// </summary>
public class PluginInfo
{
    /// <summary>
    /// 插件的唯一标识符 (GUID)
    /// 用于BepInEx插件系统的唯一识别
    /// 格式：通常是反向域名格式
    /// </summary>
    public const string PLUGIN_GUID = "NetworkPlugin";

    /// <summary>
    /// 插件的显示名称
    /// 在游戏内插件列表和管理界面中显示
    /// </summary>
    public const string PLUGIN_NAME = "Network Plugin";

    /// <summary>
    /// 插件的版本号
    /// 遵循语义化版本控制 (SemVer) 规范
    /// 格式：主版本号.次版本号.修订号
    /// </summary>
    public const string PLUGIN_VERSION = "1.0.1";

    /// <summary>
    /// Harmony补丁实例
    /// 用于运行时动态修改游戏方法，实现联机功能的核心组件
    /// Harmony ID：pixels.lbol.mods.networkmod
    /// </summary>
    public static readonly Harmony harmony = new("pixels.lbol.mods.networkmod");
}