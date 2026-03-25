using System.Collections.Generic;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;

/// <summary>
/// 网络管理器接口，负责管理联机会话中所有玩家的注册、查询和移除。
/// </summary>
public interface INetworkManager
{
    /// <summary>获取当前本地玩家的网络玩家实例。</summary>
    INetworkPlayer GetSelf();
    /// <summary>
    /// 注册一个新的网络玩家。
    /// </summary>
    /// <param name="player">要注册的玩家实例。</param>
    void RegisterPlayer(INetworkPlayer player);
    /// <summary>
    /// 根据玩家标识符移除玩家。
    /// </summary>
    /// <param name="id">玩家唯一标识符。</param>
    void RemovePlayer(string id);
    /// <summary>
    /// 根据玩家标识符获取玩家实例。
    /// </summary>
    /// <param name="id">玩家唯一标识符。</param>
    /// <returns>对应的 INetworkPlayer 实例，若不存在则返回 null。</returns>
    INetworkPlayer GetPlayer(string id);
    /// <summary>获取所有已注册的网络玩家列表。</summary>
    IEnumerable<INetworkPlayer> GetAllPlayers();

    //获得玩家数量
    /// <summary>获取当前联机房间中的玩家数量。</summary>
    int GetPlayerCount();

    //是否处于联机状态
    /// <summary>是否处于联机状态（玩家数量大于 0 时为 true）。</summary>
    bool IsConnected => GetPlayerCount() > 0;
    


}
