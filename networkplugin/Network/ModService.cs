using System;

namespace NetworkPlugin.Network;

public class ModService
{

    public static IServiceProvider ServiceProvider { get; set; }

    //补丁类示例
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
