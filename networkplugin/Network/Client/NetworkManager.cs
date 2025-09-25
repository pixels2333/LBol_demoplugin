using System;
using System.Diagnostics;

namespace NetworkPlugin.Network.Client;


public class NetworkManager(INetworkClient networkClient) : INetworkManager
{
    
    public void GetSelf()
    {
        networkClient.SendRequest<string>("GetSelf", ClientData.username);

    }


}
