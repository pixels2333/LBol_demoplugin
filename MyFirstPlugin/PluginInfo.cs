using System;
using HarmonyLib;

namespace MyFirstPlugin;

/// <summary>
/// 皮肤插件元数据信息，包含插件标识符、名称、版本及 Harmony 实例。
/// </summary>
public class PluginInfo
{
    /// <summary>插件唯一标识符。</summary>
    public const string PLUGIN_GUID = "SkinPlugin";
    /// <summary>插件显示名称。</summary>
    public const string PLUGIN_NAME = "koishi skin plugin";
    /// <summary>插件版本号。</summary>
    public const string PLUGIN_VERSION = "1.0.1";

    /// <summary>用于注册和管理 Harmony 补丁的静态实例。</summary>
    public static readonly Harmony harmony = new("pixels.lbol.mods.skinmod");
}
