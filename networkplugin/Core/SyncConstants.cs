using System;

namespace NetworkPlugin.Core;

public static class SyncConstants
{
        public const int DefaultMaxQueueSize = 100;

        public const double DefaultCacheExpiryMinutes = 5.0;

        public const double EventBufferTimeoutSeconds = 30.0;

        public const double FullSyncThrottleIntervalSeconds = 2.0;

        public const int TimestampMaxFutureSeconds = 5;

        public const int TimestampMaxPastHours = 1;

        public const int DefaultQueueCapacity = 100;

        public const long KeyWindowTicks = TimeSpan.TicksPerSecond * 5;
}
