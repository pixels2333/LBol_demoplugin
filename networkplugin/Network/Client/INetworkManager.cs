using System.Collections.Generic;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;

public interface INetworkManager
{
    INetworkPlayer GetSelf();
    void RegisterPlayer(INetworkPlayer player);
    void RemovePlayer(string id);
    INetworkPlayer GetPlayer(string id);
    IEnumerable<INetworkPlayer> GetAllPlayers();

    //获得玩家数量
    int GetPlayerCount();

    //是否处于联机状态
    bool IsConnected => GetPlayerCount() > 0;
    


}
