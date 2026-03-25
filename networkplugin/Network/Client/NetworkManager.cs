using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;


/// <summary>
/// 网络管理器实现，管理联机会话中的玩家信息，依赖 INetworkClient 进行网络通信（待实现）。
/// </summary>
public class NetworkManager(INetworkClient networkClient) : INetworkManager
{
    /// <summary>获取所有注册的网络玩家（待实现）。</summary>
    public IEnumerable<INetworkPlayer> GetAllPlayers()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 根据标识符获取玩家实例（待实现）。
    /// </summary>
    /// <param name="id">玩家唯一标识符。</param>
    public INetworkPlayer GetPlayer(string id)
    {
        throw new NotImplementedException();
    }

    /// <summary>获取当前玩家数量（待实现）。</summary>
    public int GetPlayerCount()
    {
        throw new NotImplementedException();
    }

    /// <summary>获取本地玩家实例（待实现）。</summary>
    public INetworkPlayer GetSelf()
    {
        // networkClient.SendRequest("GetSelf", ClientData.username);
        throw new NotImplementedException("GetSelf method is not implemented yet.");
    }

    /// <summary>
    /// 注册一个新玩家（待实现）。
    /// </summary>
    /// <param name="player">要注册的玩家实例。</param>
    public void RegisterPlayer(INetworkPlayer player)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 根据标识符移除玩家（待实现）。
    /// </summary>
    /// <param name="id">玩家唯一标识符。</param>
    public void RemovePlayer(string id)
    {
        throw new NotImplementedException();
    }

    
}
