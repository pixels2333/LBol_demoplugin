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
}
