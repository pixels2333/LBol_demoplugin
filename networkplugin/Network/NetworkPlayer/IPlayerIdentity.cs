namespace NetworkPlugin.Network.NetworkPlayer;

public interface IPlayerIdentity
{
        string playerId { get; set; }

        string userName { get; set; }

        string chara { get; set; }

        bool IsLobbyOwner();
}
