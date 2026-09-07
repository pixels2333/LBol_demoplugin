using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

public interface IPlayerNetworkSync
{
        void SendData();

        void PostSaveLoad();

        void UpdateHealth(bool updateServer);

        void UpdateBlock(bool updateServer);

        void UpdateMaxHP(bool updateServer);

        void UpdateCoins(bool updateServer);

        void UpdatePlayerInfo(bool updateServer);

        void UpdateMood(bool updateServer);

        void UpdateStatusEffects(bool updateServer);

        void UpdateUltimatePower(bool updateServer);

        void UpdateExhibits(bool updateServer);

        void UpdateMana(bool updateServer);

        void UpdateEndTurn(bool updateServer);

        void UpdateLocation(MapNode visitingnode, bool updateServer = true);

        void UpdateLiveStatus(bool updateServer);
}
