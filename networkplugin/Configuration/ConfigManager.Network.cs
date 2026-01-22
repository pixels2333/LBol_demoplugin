using BepInEx;
using BepInEx.Configuration;
using System;

namespace NetworkPlugin.Configuration;

public partial class ConfigManager
{
    /// <summary>
    /// 网络参数配置区域 - 网络连接相关参数
    /// </summary>
    #region 网络参数

    /// <summary>
    /// 玩家自定义 PlayerId（可选）。
    /// 说明：优先用于联机层身份标识；为空时将回退到服务器 Welcome 下发的 PlayerId。
    /// </summary>
    public ConfigEntry<string> PlayerIdOverride { get; private set; }

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
    /// 绑定网络参数配置
    /// </summary>
    private void BindNetworkSettings(ConfigFile configFile)
    {
        // 玩家自定义身份（可选）。
        PlayerIdOverride = configFile.Bind(
            "Network",
            "PlayerIdOverride",
            "",
            "玩家自定义 PlayerId（可选；为空则使用服务器下发的 PlayerId）。"
        );

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
