using HarmonyLib;

namespace NetworkPlugin;

/// <summary>
/// 网络插件的配置信息类
/// 定义了插件的基本标识信息、版本号和补丁管理器
/// 为整个网络插件系统提供统一的配置和补丁管理入口
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
    /// 在BepInEx管理界面和日志中显示的用户友好名称
    /// </summary>
    public const string PLUGIN_NAME = "Network Plugin";

    /// <summary>
    /// 插件的版本号
    /// 采用语义化版本控制，格式为 主版本号.次版本号.修订号
    /// 用于插件更新管理和版本兼容性检查
    /// </summary>
    public const string PLUGIN_VERSION = "1.0.1";

    /// <summary>
    /// Harmony补丁管理器实例
    /// 用于运行时修改和扩展游戏逻辑，实现网络功能的无缝集成
    /// 通过反射机制安全地修改和注入游戏代码
    /// </summary>
    public static readonly Harmony harmony = new("pixels.lbol.mods.networkmod");
}