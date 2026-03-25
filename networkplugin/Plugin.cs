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
/// LBoL 网络联机插件主入口，负责初始化依赖注入容器、注册网络服务并应用 Harmony 补丁。
/// </summary>
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private NetWorkPlayer netWorkPlayer; // 你的网络玩家类实例


    private ServiceProvider serviceProvider;
    // private IService service; // 你的服务接口

    private static readonly Harmony harmony = PluginInfo.harmony;

    /// <summary>
    /// BepInEx 插件初始化入口，配置依赖注入服务容器并加载 Harmony 补丁。
    /// </summary>
    private void Awake()
    {

        // 1. 创建一个 ServiceCollection 对象
        var services = new ServiceCollection();

        //注册配置,bepinex有自己的配置服务,跳过

        // 2. 注册服务：告诉容器接口和其对应的实现
        // Scoped: 在每次客户端请求的生命周期内，服务只创建一次实例
        // Transient: 每次请求服务时，都会创建一个新的实例
        // Singleton: 在应用程序的整个生命周期内，服务只创建一个实例
        // services.AddSingleton<IService, Service>();
        //或者通过
        ConfigureServices(services);

        // 3.构建服务提供者 (ServiceProvider)
        serviceProvider = services.BuildServiceProvider();

        // 4.使用服务提供者获取服务实例
        // ServiceProvider 负责解析和提供服务实例
        //  从容器中解析服务（获取一个 IEmailSender 实例）
        // var service = serviceProvider.GetService<IService>();

        // 5.使用服务
        // service.method(params);

        ModService.ServiceProvider = serviceProvider;






        // // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        if (gameObject == null)
        {
            Logger.LogError("GameObject is null, cannot call DontDestroyOnLoad.");
            return;
        }
        DontDestroyOnLoad(gameObject);
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        harmony.PatchAll();
        Logger.LogInfo("补丁已加载");
    }
    /// <summary>
    /// 配置依赖注入服务，注册 Logger、网络管理器和网络客户端。
    /// </summary>
    /// <param name="services">依赖注入服务集合。</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // 注册你的服务接口和实现
        // services.AddSingleton<IService, ServiceImpl>();
        // 你也可以注册其他Logger、配置服务等
        services.AddSingleton(Logger); // 注册BepInEx的Logger
        services.AddSingleton<INetworkManager, NetworkManager>(); // 注册网络管理器
        services.AddSingleton<INetworkClient, NetworkClient>(); // 注册网络客户端

    
    
    }

    /// <summary>
    /// 每帧更新逻辑，可用于轮询网络事件或执行周期性操作。
    /// </summary>
    void Update()
    {
        // 可以在这里或任何其他地方使用service
        // service?.AnotherMethod();
    }
    /// <summary>
    /// 插件销毁时的清理逻辑，释放依赖注入服务提供者资源。
    /// </summary>
    void OnDestroy()
    {
        // 如果_serviceProvider实现了IDisposable（通常会的），应该在这里进行Dispose
        serviceProvider?.Dispose();
    }
}
