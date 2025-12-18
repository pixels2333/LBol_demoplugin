using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

/// <summary>
/// 配置管理器类
/// 使用BepInEx原生配置系统管理插件配置
/// 配置自动保存到 BepInEx\config\NetworkPlugin.cfg
/// </summary>
public partial class ConfigManager
{
    /// <summary>
    /// 实例化配置管理器并绑定所有配置项
    /// </summary>
    /// <param name="configFile">BepInEx配置文件实例</param>
    public ConfigManager(ConfigFile configFile)
    {
        BindConfigurations(configFile);
    }

    /// <summary>
    /// 绑定所有配置项到BepInEx配置系统
    /// </summary>
    /// <param name="configFile">BepInEx配置文件实例</param>
    private void BindConfigurations(ConfigFile configFile)
    {
        // 绑定功能开关配置
        BindFeatureToggles(configFile);

        // 绑定性能参数配置
        BindPerformanceSettings(configFile);

        // 绑定网络参数配置
        BindNetworkSettings(configFile);

        // 绑定同步配置
        BindSyncConfiguration(configFile);

        // 绑定游戏平衡配置
        BindGameBalanceSettings(configFile);
    }
}
