using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

/// <summary>
/// 配置管理器类
/// 使用BepInEx原生配置系统管理插件配置
/// 配置自动保存到 BepInEx\config\NetworkPlugin.cfg
/// </summary>
public class ConfigManager
{
    /// <summary>
    /// 功能开关配置区域 - 控制各种同步功能的启用状态
    /// </summary>
    #region 功能开关

    /// <summary>
    /// 卡牌同步开关
    /// 控制卡牌使用、抽取、洗牌等行为的网络同步
    /// </summary>
    public ConfigEntry<bool> EnableCardSync { get; private set; }

    /// <summary>
    /// 法力同步开关
    /// 控制法力消耗、恢复、增益等行为的网络同步
    /// </summary>
    public ConfigEntry<bool> EnableManaSync { get; private set; }

    /// <summary>
    /// 战斗同步开关
    /// 控制伤害计算、状态效果、战斗结果的同步
    /// </summary>
    public ConfigEntry<bool> EnableBattleSync { get; private set; }

    /// <summary>
    /// 地图同步开关
    /// 控制地图探索、节点状态、地图事件的同步
    /// </summary>
    public ConfigEntry<bool> EnableMapSync { get; private set; }

    #endregion

    /// <summary>
    /// 性能参数配置区域 - 调整队列大小和缓存策略
    /// </summary>
    #region 性能参数

    /// <summary>
    /// 事件队列最大容量
    /// 网络不可用时事件队列的最大条目数量
    /// 超过此容量的新事件会被丢弃
    /// </summary>
    public ConfigEntry<int> MaxQueueSize { get; private set; }

    /// <summary>
    /// 状态缓存存活时间（分钟）
    /// 本地状态缓存的存活时间，超过此时间的缓存会被清理
    /// 默认为5分钟，可以根据需要调整
    /// </summary>
    public ConfigEntry<float> StateCacheExpiryMinutes { get; private set; }

    /// <summary>
    /// 网络超时时间（秒）
    /// 网络操作的超时时间，超过此时间未响应则判定为超时
    /// 默认为30秒
    /// </summary>
    public ConfigEntry<int> NetworkTimeoutSeconds { get; private set; }

    /// <summary>
    /// 重连尝试次数
    /// 网络断开后的最大重连尝试次数
    /// 超过此次数后停止自动重连
    /// </summary>
    public ConfigEntry<int> MaxReconnectAttempts { get; private set; }

    #endregion

    /// <summary>
    /// 网络参数配置区域 - 网络连接相关参数
    /// </summary>
    #region 网络参数

    /// <summary>
    /// 服务器端口号
    /// 联机服务器监听的端口号
    /// 默认值为 7777
    /// </summary>
    public ConfigEntry<int> ServerPort { get; private set; }

    /// <summary>
    /// 服务器IP地址
    /// 连接服务器的IP地址
    /// 默认为本地回环地址 127.0.0.1
    /// </summary>
    public ConfigEntry<string> ServerIP { get; private set; }

    /// <summary>
    /// 日志详细程度
    /// 控制日志输出的详细程度
    /// 0 = 仅错误，1 = 错误和警告，2 = 全部
    /// </summary>
    public ConfigEntry<int> LogVerbosity { get; private set; }

    /// <summary>
    /// 中继服务器端口
    /// 中继服务器监听的端口号
    /// </summary>
    public ConfigEntry<int> RelayServerPort { get; private set; }

    /// <summary>
    /// 中继服务器最大连接数
    /// 中继服务器允许的最大并发连接数
    /// </summary>
    public ConfigEntry<int> RelayServerMaxConnections { get; private set; }

    /// <summary>
    /// 中继服务器连接密钥
    /// 客户端连接中继服务器时需要验证的密钥
    /// </summary>
    public ConfigEntry<string> RelayServerConnectionKey { get; private set; }

    /// <summary>
    /// 中继服务器最大房间数
    /// 中继服务器允许创建的最大房间数量
    /// </summary>
    public ConfigEntry<int> RelayServerMaxRooms { get; private set; }

    /// <summary>
    /// 中继服务器每个房间最大玩家数
    /// 每个游戏房间允许的最大玩家数量
    /// </summary>
    public ConfigEntry<int> RelayServerMaxPlayersPerRoom { get; private set; }

    #endregion

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
    }

    /// <summary>
    /// 绑定功能开关配置
    /// </summary>
    private void BindFeatureToggles(ConfigFile configFile)
    {
        // 在General.Toggles区域下绑定所有功能开关
        EnableCardSync = configFile.Bind(
            "General.Toggles",
            "EnableCardSync",
            true,
            "控制卡牌使用、抽取、洗牌等行为的网络同步"
        );

        EnableManaSync = configFile.Bind(
            "General.Toggles",
            "EnableManaSync",
            true,
            "控制法力消耗、恢复、增益等行为的网络同步"
        );

        EnableBattleSync = configFile.Bind(
            "General.Toggles",
            "EnableBattleSync",
            true,
            "控制伤害计算、状态效果、战斗结果的同步"
        );

        EnableMapSync = configFile.Bind(
            "General.Toggles",
            "EnableMapSync",
            true,
            "控制地图探索、节点状态、地图事件的同步"
        );
    }

    /// <summary>
    /// 绑定性能参数配置
    /// </summary>
    private void BindPerformanceSettings(ConfigFile configFile)
    {
        // 在Performance区域下绑定性能相关配置
        MaxQueueSize = configFile.Bind(
            "Performance",
            "MaxQueueSize",
            100,
            "网络不可用时事件队列的最大条目数量，超过此容量的新事件会被丢弃"
        );

        StateCacheExpiryMinutes = configFile.Bind(
            "Performance",
            "StateCacheExpiryMinutes",
            5.0f,
            "本地状态缓存的存活时间（分钟），超过此时间的缓存会被清理"
        );

        NetworkTimeoutSeconds = configFile.Bind(
            "Performance",
            "NetworkTimeoutSeconds",
            30,
            "网络操作的超时时间（秒），超过此时间未响应则判定为超时"
        );

        MaxReconnectAttempts = configFile.Bind(
            "Performance",
            "MaxReconnectAttempts",
            5,
            "网络断开后的最大重连尝试次数，超过此次数后停止自动重连"
        );
    }

    /// <summary>
    /// 绑定网络参数配置
    /// </summary>
    private void BindNetworkSettings(ConfigFile configFile)
    {
        // 在Network区域下绑定网络相关配置
        ServerPort = configFile.Bind(
            "Network",
            "ServerPort",
            7777,
            "联机服务器监听的端口号"
        );

        ServerIP = configFile.Bind(
            "Network",
            "ServerIP",
            "127.0.0.1",
            "连接服务器的IP地址"
        );

        LogVerbosity = configFile.Bind(
            "Network",
            "LogVerbosity",
            2,
            "日志详细程度，0 = 仅错误，1 = 错误和警告，2 = 全部"
        );

        // 中继服务器相关配置
        RelayServerPort = configFile.Bind(
            "RelayServer",
            "Port",
            8888,
            "中继服务器监听端口"
        );

        RelayServerMaxConnections = configFile.Bind(
            "RelayServer",
            "MaxConnections",
            1000,
            "中继服务器最大连接数"
        );

        RelayServerConnectionKey = configFile.Bind(
            "RelayServer",
            "ConnectionKey",
            "LBoL_Network_Plugin",
            "中继服务器连接密钥"
        );

        RelayServerMaxRooms = configFile.Bind(
            "RelayServer",
            "MaxRooms",
            100,
            "中继服务器最大房间数"
        );

        RelayServerMaxPlayersPerRoom = configFile.Bind(
            "RelayServer",
            "MaxPlayersPerRoom",
            4,
            "中继服务器每个房间最大玩家数"
        );
    }
}
