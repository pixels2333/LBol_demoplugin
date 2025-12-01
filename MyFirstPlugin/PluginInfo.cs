using System;
using HarmonyLib;

namespace MyFirstPlugin;

/// <summary>
/// 我的第一个LBoL插件的插件信息类
/// 包含插件的基本标识信息和Harmony补丁实例
/// 这是一个皮肤插件的示例配置
/// </summary>
public class PluginInfo
{
    /// <summary>
    /// 插件的唯一标识符 (GUID)
    /// 用于BepInEx插件系统的唯一识别
    /// 注意：虽然名为"SkinPlugin"，但这是一个示例插件
    /// </summary>
    public const string PLUGIN_GUID = "SkinPlugin";

    /// <summary>
    /// 插件的显示名称
    /// 在游戏内插件列表和管理界面中显示
    /// 表明这是一个恋恋（古明地恋）皮肤插件
    /// </summary>
    public const string PLUGIN_NAME = "koishi skin plugin";

    /// <summary>
    /// 插件的版本号
    /// 遵循语义化版本控制 (SemVer) 规范
    /// 格式：主版本号.次版本号.修订号
    /// </summary>
    public const string PLUGIN_VERSION = "1.0.1";

    /// <summary>
    /// Harmony补丁实例
    /// 用于运行时动态修改游戏方法，实现皮肤替换功能
    /// Harmony ID：pixels.lbol.mods.skinmod
    /// </summary>
    public static readonly Harmony harmony = new("pixels.lbol.mods.skinmod");
}