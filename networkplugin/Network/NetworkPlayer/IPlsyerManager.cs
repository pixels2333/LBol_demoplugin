using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.NetworkPlayer;

public interface IPlayerManager
{
    void RegisterPlayer(INetworkPlayer player);
    void RemovePlayer(string id);
    INetworkPlayer GetPlayer(string id);
    IEnumerable<INetworkPlayer> GetAllPlayers();
}
