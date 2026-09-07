namespace NetworkPlugin.Network.Room;

public class RoomFilter
{
        public bool? IsPublic { get; set; }

        public int? MaxPlayers { get; set; }

        public bool? IsInGame { get; set; }

        public string? GameMode { get; set; }
}
