using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Room;

public class RoomStatus
{
        public string RoomId { get; set; } = string.Empty;

        public int PlayerCount { get; set; }

        public int MaxPlayers { get; set; }

        public string HostPlayerId { get; set; } = string.Empty;

        public bool IsInGame { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> PlayerIds { get; set; } = [];

        public int Ping { get; set; }
}
