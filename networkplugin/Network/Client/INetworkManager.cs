using System.Collections.Generic;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;

/// <summary>
/// 网络管理器接口
/// 负责管理连接到游戏的网络玩家，包括玩家的注册、移除和查询
/// 提供统一的玩家状态管理和联机状态检测功能
/// </summary>
public interface INetworkManager
{
    /// <summary>
    /// 获取当前本地玩家的网络玩家实例
    /// </summary>
    /// <returns>当前玩家的INetworkPlayer实例，如果未注册则返回null</returns>
    INetworkPlayer GetSelf();

    /// <summary>
    /// 注册新的网络玩家到管理器中
    /// </summary>
    /// <param name="player">要注册的网络玩家实例</param>
    void RegisterPlayer(INetworkPlayer player);

    /// <summary>
    /// 从管理器中移除指定的网络玩家
    /// </summary>
    /// <param name="id">要移除的玩家ID</param>
    void RemovePlayer(string id);

    /// <summary>
    /// 根据玩家ID获取对应的网络玩家实例
    /// </summary>
    /// <param name="id">玩家的唯一标识符</param>
    /// <returns>对应的INetworkPlayer实例，如果未找到则返回null</returns>
    INetworkPlayer GetPlayer(string id);

    /// <summary>
    /// 获取所有已注册的网络玩家集合
    /// </summary>
    /// <returns>包含所有网络玩家的枚举集合</returns>
    IEnumerable<INetworkPlayer> GetAllPlayers();

    /// <summary>
    /// 获取当前已注册的网络玩家数量
    /// </summary>
    /// <returns>玩家总数</returns>
    int GetPlayerCount();

    /// <summary>
    /// 检查当前是否处于联机状态
    /// 当有一个或更多玩家连接时认为处于联机状态
    /// </summary>
    bool IsConnected => GetPlayerCount() > 0;
}