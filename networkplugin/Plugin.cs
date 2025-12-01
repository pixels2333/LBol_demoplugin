using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin;

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
    /// 服务提供者，负责管理和解析所有注册的服务接口
    /// 使用依赖注入模式，管理网络管理器、客户端等核心服务的生命周期
    /// </summary>
    private ServiceProvider serviceProvider;
    // private IService service; // 你的服务接口

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
        // 第1步：创建服务容器，用于依赖注入管理
        var services = new ServiceCollection();

        // 注入配置服务，BepInEx框架已内置配置服务，此处跳过
        // 使用 BepInEx 的原生配置系统进行插件配置管理

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
        // 可以从容器中获取任何已注册的服务接口实现
        // var service = serviceProvider.GetService<IService>(); // 示例服务获取

        // 第5步：调用服务方法执行具体功能
        // service.method(params); // 使用注入的服务

        // 将服务提供者注册到模块服务中，供其他组件使用
        ModService.ServiceProvider = serviceProvider;

        // 插件启动逻辑开始
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        // 安全检查：确保GameObject不为空
        if (gameObject == null)
        {
            Logger.LogError("GameObject is null, cannot call DontDestroyOnLoad.");
            return;
        }
        
        // 将插件对象设为不随场景切换而销毁，保证网络连接的稳定性
        DontDestroyOnLoad(gameObject);
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        // 加载所有Harmony补丁，实现游戏逻辑的网络化扩展
        harmony.PatchAll();
        Logger.LogInfo("补丁已加载");
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
        // 如果serviceProvider实现了IDisposable接口，需要在此处进行Dispose操作
        // 避免内存泄漏和资源占用问题
        serviceProvider?.Dispose();
    }
}
