using System;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Room;

namespace NetworkPlugin.Network.Messages;

public class NetworkConnection(string currentRoomId, string playerId, NetworkClient client)
{
    public string CurrentRoomId { get; internal set; } = currentRoomId;
    public string PlayerId { get; internal set; } = playerId;
    public NetworkClient Client { get; internal set; } = client;

    public void SendMessage(NetworkMessage message)
    {
        throw new NotImplementedException();
    }
}
