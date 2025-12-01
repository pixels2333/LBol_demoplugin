using System;

namespace NetworkPlugin.Network;

/// <summary>
/// 模块服务静态类
/// 提供全局的服务提供者访问点，用于依赖注入和跨组件服务访问
/// 作为整个网络插件的中央服务注册和访问中心
/// </summary>
public static class ModService
{
    /// <summary>
    /// 全局服务提供者实例
    /// 通过依赖注入容器管理所有服务的生命周期和依赖关系
    /// 在插件初始化时设置，供所有补丁和服务类使用
    /// </summary>
    public static IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// 补丁类使用示例
    /// 展示如何在Harmony补丁中正确使用ModService获取依赖注入的服务
    /// </summary>
    // 补丁类示例
    //  public static void Postfix(SomeOriginalClass __instance)
    //     {
    //         // 1. 从静态类中获取 ServiceProvider
    //         var serviceProvider = ModServices.ServiceProvider;
    //         if (serviceProvider == null)
    //         {
    //             // 在这里可以添加日志或错误处理，以防服务未被正确初始化
    //             return;
    //         }

    //         // 2. 从 ServiceProvider 中解析出您需要的服务实例
    //         // 使用 GetRequiredService 如果服务不存在会抛出异常，更安全
    //         var myApiService = serviceProvider.GetRequiredService<IMyApiInterface>();

    //         // 3. 现在您可以自由地使用这个服务了
    //         myApiService.PassInstance(__instance); // 将原始类的实例传递给您的服务
    //         myApiService.DoSomethingElse();
    //     }
}
