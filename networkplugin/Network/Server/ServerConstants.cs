namespace NetworkPlugin.Network.Server;

public static class ServerConstants
{
        public const int SessionCleanupIntervalMs = 2000;

        public const int BgThreadSleepMs = 15;

        public const int MaxRoomCount = 100;

        public const int DefaultPort = 7777;

        public const int DefaultRelayPort = 8888;

        public const int DefaultMaxConnections = 1000;

        public const int HeartbeatTimeoutSeconds = 30;

        public const int DefaultMaxPlayersPerRoom = 4;

        public const int DisconnectTimeoutSeconds = 30;
}
