namespace NetworkPlugin.Network;

public static class NetworkConstants
{
        public const int DefaultPort = 7777;

        public const int RelayPort = 8888;

        public const int MaxConnections = 1000;

        public const int HeartbeatIntervalMs = 5000;

        public const int ConnectTimeoutMs = 300;

        public const int NatStunPort = 3478;

        public const int NatTtl = 300;

        public const int BgThreadSleepMs = 15;

        public const int MaxRoomCount = 100;

        public const int SessionCleanupIntervalMs = 2000;

        public const int ReconnectIntervalMs = 5000;

        public const int ReconnectGracePeriodSeconds = 60;

        public const int DefaultMaxReconnectAttempts = 5;

        public const int DefaultNetworkTimeoutSeconds = 30;
}
