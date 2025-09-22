using BepInEx;
using BepInEx.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

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

        // 3.构建服务提供者 (ServiceProvider)
        var serviceProvider = services.BuildServiceProvider();
        // 4.使用服务提供者获取服务实例
        // ServiceProvider 负责解析和提供服务实例

        // 5. 从容器中解析服务（获取一个 IEmailSender 实例）
        // var service = serviceProvider.GetService<IService>();
        
        // 6. 使用服务
        // service.method(params);



    }
}
