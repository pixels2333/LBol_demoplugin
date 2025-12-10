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
    /// 根据玩家ID获取对应的网络玩家实例
    /// </summary>
    /// <param name="id">玩家的唯一标识符</param>
    /// <returns>对应的INetworkPlayer实例，如果未找到则返回null</returns>
    INetworkPlayer GetPlayer(string id);

    /// <summary>
    /// 根据玩家的PeerId获取对应的网络玩家实例
    /// </summary>
    /// <param name="peerId">玩家的唯一标识符</param>
    /// <returns>对应的INetworkPlayer实例，如果未找到则返回null</returns>
    INetworkPlayer GetPlayerByPeerId(int peerId);

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
    /// 获取是否处于联机状态
    /// 通过判断玩家数量大于0来确定是否在联机
    /// </summary>
    bool IsConnected => GetPlayerCount() > 0;
}