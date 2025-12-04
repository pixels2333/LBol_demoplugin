using BepInEx.Logging;

namespace NetworkPlugin.Configuration;

/// <summary>
/// 配置管理器使用示例类
/// 展示如何在其他类中访问和使用配置
/// </summary>
/// <remarks>
/// <para>使用方式：</para>
/// <para>1. 通过依赖注入获取配置管理器：在构造函数中注入 ConfigManager</para>
/// <para>2. 通过静态属性访问：直接使用 Plugin.ConfigManager</para>
/// <para>
/// 示例代码：
/// <code>
/// // 方法1：通过静态属性访问
/// if (Plugin.ConfigManager.EnableCardSync.Value)
/// {
///     // 执行卡牌同步逻辑
/// }
///
/// // 方法2：在构造函数中注入配置管理器
/// public class MyService
/// {
///     private readonly ConfigManager _config;
///
///     public MyService(ConfigManager config)
///     {
///         _config = config;
///     }
///
///     public void DoWork()
///     {
///         int maxQueueSize = _config.MaxQueueSize.Value;
///         // 使用配置值
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public class ConfigUsageExample
{
    private readonly ManualLogSource _logger = Plugin.Logger;
    private readonly ConfigManager _configManager;

    /// <summary>
    /// 通过依赖注入获取配置管理器
    /// </summary>
    /// <param name="configManager">配置管理器实例</param>
    public ConfigUsageExample(ConfigManager configManager)
    {
        _configManager = configManager;
    }

    /// <summary>
    /// 示例：检查功能开关是否启用
    /// </summary>
    public bool IsCardSyncEnabled()
    {
        // 访问配置的值
        return _configManager.EnableCardSync.Value;
    }

    /// <summary>
    /// 示例：获取性能参数
    /// </summary>
    public int GetMaxQueueSize()
    {
        return _configManager.MaxQueueSize.Value;
    }

    /// <summary>
    /// 示例：获取网络参数
    /// </summary>
    public (string ip, int port) GetServerInfo()
    {
        return (
            _configManager.ServerIP.Value,
            _configManager.ServerPort.Value
        );
    }

    /// <summary>
    /// 示例：根据配置执行不同的逻辑
    /// </summary>
    public void ProcessWithConfig()
    {
        // 根据功能开关决定是否执行特定逻辑
        if (_configManager.EnableBattleSync.Value)
        {
            _logger.LogInfo("战斗同步已启用，执行战斗同步逻辑");
            // 执行战斗同步相关代码
        }

        // 根据日志详细程度输出不同级别的日志
        int verbosity = _configManager.LogVerbosity.Value;
        if (verbosity >= 2)
        {
            _logger.LogInfo("详细日志：正在处理数据");
        }
        else if (verbosity >= 1)
        {
            _logger.LogWarning("简要日志：处理中");
        }

        // 使用性能参数
        int maxAttempts = _configManager.MaxReconnectAttempts.Value;
        _logger.LogInfo($"最大重连尝试次数：{maxAttempts}");
    }

    /// <summary>
    /// 静态方法示例：直接通过Plugin类访问配置
    /// 适合在静态方法或无法使用依赖注入的场景
    /// </summary>
    public static void StaticExample()
    {
        if (Plugin.ConfigManager.EnableMapSync.Value)
        {
            Plugin.Logger.LogInfo("地图同步已启用");
        }

        // 获取网络配置
        string serverIP = Plugin.ConfigManager.ServerIP.Value;
        int serverPort = Plugin.ConfigManager.ServerPort.Value;
        Plugin.Logger.LogInfo($"服务器地址: {serverIP}:{serverPort}");
    }
}
