namespace NetworkPlugin.Network.Room;

public class RoomConfig
{
        public int MaxPlayers { get; set; } = 4;

        public bool IsGameStarted { get; set; } = false;

        public bool IsPublic { get; set; } = true;

        public string? Password { get; set; }

        public string? GameMode { get; set; }

        public string? Description { get; set; }

        public static RoomConfig Default()
    {
        return new RoomConfig { MaxPlayers = 4 };
    }
}
