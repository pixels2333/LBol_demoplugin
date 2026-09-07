namespace NetworkPlugin.Network.NetworkPlayer;

public interface IPlayerResources
{
        int coins { get; set; }

        string[] exhibits { get; set; }

        bool tradingStatus { get; set; }

        bool ultimatePower { get; set; }

        int[] mana { get; set; }

        string stance { get; set; }
}
