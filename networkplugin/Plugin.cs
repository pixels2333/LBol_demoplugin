using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Core;
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
/// <summary>
/// LBoL网络插件的主入口类
/// 负责插件的初始化、服务注册、生命周期管理和网络功能的整体协调
/// 实现了BepInEx插件的完整生命周期管理
/// </summary>
public class Plugin : BaseUnityPlugin
{
    /// <summary>
    /// 插件日志输出器，用于记录插件运行状态和调试信息
    /// 通过BepInEx框架提供的日志服务，支持不同级别的日志输出
    /// </summary>
    internal static new ManualLogSource Logger;

    /// <summary>
    /// 网络玩家实例，管理玩家的网络连接和状态同步
    /// 该实例负责处理玩家在网络环境中的身份验证、数据同步和通信功能
    /// </summary>
    private NetWorkPlayer netWorkPlayer;

    /// <summary>
    /// 配置管理器实例，管理插件的所有配置项
    /// 使用BepInEx原生的配置系统，自动加载和保存配置
    /// </summary>
    public static ConfigManager ConfigManager { get; private set; }

    /// <summary>
    /// 服务提供者，负责管理和解析所有注册的服务接口
    /// 使用依赖注入模式，管理网络管理器、客户端等核心服务的生命周期
    /// </summary>
    private ServiceProvider serviceProvider;

    /// <summary>
    /// Harmony补丁实例，用于运行时修改和扩展游戏逻辑
    /// 通过网络补丁实现游戏机制的网络化同步和功能增强
    /// </summary>
    private static readonly Harmony harmony = PluginInfo.harmony;

    /// <summary>
    /// 插件唤醒方法，在插件加载时自动调用
    /// 负责初始化所有系统组件、注册服务配置、设置网络环境
    /// 是插件启动流程的核心入口点
    /// </summary>
    private void Awake()
    {
        // 插件启动逻辑开始
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        // 初始化配置管理器，使用BepInEx原生的配置系统
        ConfigManager = new ConfigManager(Config);
        Logger.LogInfo("配置管理器已初始化");

        // 第1步：创建服务容器，用于依赖注入管理
        ServiceCollection services = new ServiceCollection();

        // 注册配置管理器到DI容器，供其他服务使用
        services.AddSingleton(ConfigManager);

        // 第2步：注册服务接口和对应的实现类
        // Scoped 服务模式：在每次请求的生命周期内创建一次实例
        // Transient 服务模式：每次请求都创建新实例
        // Singleton 服务模式：在整个应用生命周期内共享单例
        // services.AddSingleton<IService, Service>(); // 示例服务注册
        // 通过配置方法完成具体服务注册
        ConfigureServices(services);

        // 第3步：构建服务提供者，完成依赖注入容器的初始化
        serviceProvider = services.BuildServiceProvider();

        // 第4步：使用服务提供者获取所需服务实例
        // ServiceProvider 负责解析和根据配置提供相应的服务实例
        // 可以从容器中获取任何已注册的服务
        // var service = serviceProvider.GetService<IService>(); // 示例服务获取

        // 第5步：调用服务方法执行具体功能
        // service.method(params); // 使用注入的服务

        // 将服务提供者注册到模块服务中，供其他组件使用
        ModService.ServiceProvider = serviceProvider;

        // 安全检查：确保GameObject不为空
        if (gameObject == null)
        {
            Logger.LogError("GameObject is null, cannot call DontDestroyOnLoad.");
            return;
        }

        // 设置GameObject在场景切换时不被销毁，确保插件持久运行
        DontDestroyOnLoad(gameObject);

        // 应用所有Harmony补丁，修改游戏行为以支持联机功能
        harmony.PatchAll();
        Logger.LogInfo("补丁已加载");

        // 输出当前配置信息到日志
        LogCurrentConfig();
    }

    /// <summary>
    /// 配置服务注册，将各种接口和实现类注册到依赖注入容器中
    /// 采用单例模式注册核心网络服务，确保整个应用中共享同一实例
    /// </summary>
    /// <param name="services">服务容器，用于注册各种服务接口和实现</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // 注册你的自定义服务接口和实现类
        // services.AddSingleton<IService, ServiceImpl>();

        // 注册系统服务和框架服务
        services.AddSingleton(Logger); // 注册BepInEx的日志服务
        services.AddSingleton<INetworkManager, NetworkManager>(); // 注册网络管理器服务
        services.AddSingleton<INetworkClient, NetworkClient>(); // 注册网络客户端服务
        services.AddSingleton<ISynchronizationManager, SynchronizationManager>(); // 注册同步管理器服务
    }

    /// <summary>
    /// 每帧调用的更新方法
    /// 在游戏主循环中执行，可用于处理需要持续更新的网络相关逻辑
    /// 目前保留用于未来可能的实时网络状态监控和处理
    /// </summary>
    void Update()
    {
        // 可以在这里或任何其他地方使用注入的服务
        // service?.AnotherMethod();
    }

    /// <summary>
    /// 插件销毁方法，在插件卸载或游戏关闭时调用
    /// 负责清理所有资源，包括服务提供者的释放和资源回收
    /// </summary>
    void OnDestroy()
    {
        // 释放依赖注入容器的资源
        // 防止内存泄漏和资源未释放问题
        // 如果serviceProvider实现了IDisposable接口，需要在此处进行Dispose操作

        serviceProvider?.Dispose();

        // 记录插件销毁日志
        Logger?.LogInfo("Plugin has been destroyed and resources cleaned up.");
    }

    /// <summary>
    /// 输出当前配置信息到日志，用于调试和验证配置加载
    /// </summary>
    private void LogCurrentConfig()
    {
        Logger.LogInfo("=== 当前配置信息 ===");
        Logger.LogInfo($"功能开关:");
        Logger.LogInfo($"  卡牌同步: {ConfigManager.EnableCardSync.Value}");
        Logger.LogInfo($"  法力同步: {ConfigManager.EnableManaSync.Value}");
        Logger.LogInfo($"  战斗同步: {ConfigManager.EnableBattleSync.Value}");
        Logger.LogInfo($"  地图同步: {ConfigManager.EnableMapSync.Value}");
        Logger.LogInfo($"性能参数:");
        Logger.LogInfo($"  最大队列大小: {ConfigManager.MaxQueueSize.Value}");
        Logger.LogInfo($"  缓存过期时间: {ConfigManager.StateCacheExpiryMinutes.Value} 分钟");
        Logger.LogInfo($"  网络超时时间: {ConfigManager.NetworkTimeoutSeconds.Value} 秒");
        Logger.LogInfo($"  最大重连尝试: {ConfigManager.MaxReconnectAttempts.Value}");
        Logger.LogInfo($"网络参数:");
        Logger.LogInfo($"  服务器IP: {ConfigManager.ServerIP.Value}");
        Logger.LogInfo($"  服务器端口: {ConfigManager.ServerPort.Value}");
        Logger.LogInfo($"  日志详细程度: {ConfigManager.LogVerbosity.Value}");
        Logger.LogInfo("===================");
    }
}