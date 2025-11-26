using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Network.Client;


public class NetworkManager(INetworkClient networkClient) : INetworkManager
{
    public IEnumerable<INetworkPlayer> GetAllPlayers()
    {
        throw new NotImplementedException();
    }

    public INetworkPlayer GetPlayer(string id)
    {
        throw new NotImplementedException();
    }

    public int GetPlayerCount()
    {
        throw new NotImplementedException();
    }

    public INetworkPlayer GetSelf()
    {
        // networkClient.SendRequest("GetSelf", ClientData.username);
        throw new NotImplementedException("GetSelf method is not implemented yet.");
    }

    public void RegisterPlayer(INetworkPlayer player)
    {
        throw new NotImplementedException();
    }

    public void RemovePlayer(string id)
    {
        throw new NotImplementedException();
    }


}
