using System.Collections.Generic;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;

/// <summary>
/// 网络管理器接口，负责管理游戏中的网络玩家和连接状态
/// 提供玩家注册、移除、查询等核心功能
/// </summary>
public interface INetworkManager
{
    /// <summary>
    /// 获取当前本地玩家实例
    /// </summary>
    /// <returns>本地网络玩家对象</returns>
    INetworkPlayer GetSelf();

    /// <summary>
    /// 注册新的网络玩家
    /// 当有新玩家加入游戏时调用此方法
    /// </summary>
    /// <param name="player">要注册的网络玩家对象</param>
    void RegisterPlayer(INetworkPlayer player);

    /// <summary>
    /// 移除指定ID的网络玩家
    /// 当玩家离开游戏时调用此方法
    /// </summary>
    /// <param name="id">要移除的玩家ID</param>
    void RemovePlayer(string id);

    /// <summary>
    /// 根据玩家ID获取对应的网络玩家对象
    /// </summary>
    /// <param name="id">玩家ID</param>
    /// <returns>对应的网络玩家对象，如果不存在则返回null</returns>
    INetworkPlayer GetPlayer(string id);

    /// <summary>
    /// 获取所有已注册的网络玩家
    /// </summary>
    /// <returns>包含所有网络玩家的枚举集合</returns>
    IEnumerable<INetworkPlayer> GetAllPlayers();

    /// <summary>
    /// 获取当前游戏中的玩家数量
    /// </summary>
    /// <returns>玩家总数</returns>
    int GetPlayerCount();

    /// <summary>
    /// 获取是否处于联机状态
    /// 通过判断玩家数量大于0来确定是否在联机
    /// </summary>
    bool IsConnected => GetPlayerCount() > 0;
}
