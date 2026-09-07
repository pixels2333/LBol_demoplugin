namespace NetworkPlugin.Network.NetworkPlayer;

public interface IPlayerBattleState
{
        int HP { get; set; }

        int maxHP { get; set; }

        int block { get; set; }

        int shield { get; set; }

        bool endturn { get; set; }

        string location { get; set; }

        int stage { get; set; }

        string mood { get; set; }

        int location_X { get; set; }

        int location_Y { get; set; }

        bool IsPlayerInSameRoom();

        bool IsPlayerOnSameAct();

        bool ShouldRenderCharacter();

        bool ShouldRenderCharacterInfoBox();

        void IsNearDeath(bool updateServer);

        void Takedamage(int damage);

        void DealDamage(int damage);

        void Teleport(int x, int y);

        void Resurrect(string username, int newhp);
}
