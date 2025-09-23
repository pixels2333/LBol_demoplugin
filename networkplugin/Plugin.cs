using BepInEx;
using BepInEx.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ServiceProvider serviceProvider;
    // private IService service; // 你的服务接口

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
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
    }
    private void ConfigureServices(IServiceCollection services)
    {
        // 注册你的服务接口和实现
        // services.AddSingleton<IService, ServiceImpl>();
        // 你也可以注册其他Logger、配置服务等
        services.AddSingleton(Logger); // 注册BepInEx的Logger
    }

    void Update()
    {
        // 可以在这里或任何其他地方使用service
        // service?.AnotherMethod();
    }
    void OnDestroy()
    {
        // 如果_serviceProvider实现了IDisposable（通常会的），应该在这里进行Dispose
        serviceProvider?.Dispose();
    }
}
