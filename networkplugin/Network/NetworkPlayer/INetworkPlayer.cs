using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

public interface INetworkPlayer : IPlayerIdentity, IPlayerBattleState, IPlayerResources, IPlayerNetworkSync
{
}
