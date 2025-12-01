using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin;

/// <summary>
/// LBoL联机MOD的主插件类
/// 负责插件的初始化、依赖注入配置和生命周期管理
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
    /// 网络玩家实例，负责处理联机功能
    /// </summary>
    private NetWorkPlayer netWorkPlayer;

    /// <summary>
    /// 依赖注入服务提供者，管理所有已注册的服务
    /// </summary>
    private ServiceProvider serviceProvider;

    /// <summary>
    /// Harmony实例，用于运行时方法补丁
    /// </summary>
    private static readonly Harmony harmony = PluginInfo.harmony;

    /// <summary>
    /// 插件初始化方法
    /// 在插件加载时由BepInEx自动调用
    /// 负责设置依赖注入容器、初始化服务和应用Harmony补丁
    /// </summary>
    private void Awake()
    {
        // 创建服务集合容器，用于依赖注入配置
        var services = new ServiceCollection();

        // 配置依赖注入服务
        ConfigureServices(services);

        // 构建服务提供者，创建依赖注入容器
        serviceProvider = services.BuildServiceProvider();

        // 将服务提供者设置给ModService，使整个插件可以访问依赖注入服务
        ModService.ServiceProvider = serviceProvider;

        // 初始化插件日志系统
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

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

        // 应用所有Harmony补丁，修改游戏行为以支持联机功能
        harmony.PatchAll();
        Logger.LogInfo("补丁已加载");
    }

    /// <summary>
    /// 配置依赖注入服务
    /// 注册插件所需的所有服务和接口实现
    /// </summary>
    /// <param name="services">服务集合，用于注册各种服务</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // 注册BepInEx的Logger服务，使用单例模式确保全局统一
        services.AddSingleton(Logger);

        // 注册网络管理器服务，负责网络连接的整体管理
        services.AddSingleton<INetworkManager, NetworkManager>();

        // 注册网络客户端服务，负责具体的网络通信实现
        services.AddSingleton<INetworkClient, NetworkClient>();
    }

    /// <summary>
    /// Unity的Update方法，每帧调用
    /// 用于处理需要实时更新的逻辑
    /// </summary>
    void Update()
    {
        // 这里可以添加每帧需要执行的逻辑
        // 例如：网络消息处理、状态更新等
    }

    /// <summary>
    /// Unity的OnDestroy方法，在对象销毁时调用
    /// 负责清理资源和保存数据
    /// </summary>
    void OnDestroy()
    {
        // 释放依赖注入容器的资源
        // 防止内存泄漏和资源未释放问题
        serviceProvider?.Dispose();

        // 记录插件销毁日志
        Logger?.LogInfo("Plugin has been destroyed and resources cleaned up.");
    }
}