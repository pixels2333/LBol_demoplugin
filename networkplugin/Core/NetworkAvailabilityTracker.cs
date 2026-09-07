using System;
using BepInEx.Logging;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Core;

public sealed class NetworkAvailabilityTracker
{
    private readonly INetworkClient _networkClient;
    private readonly ManualLogSource _logger;

        public bool IsAvailable { get; private set; }
        public DateTime LastConnectionTime { get; private set; } = DateTime.MinValue;
        public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;
        public long LastFullSyncRequestAtTicks { get; set; }

    private static readonly TimeSpan FullSyncThrottleInterval = TimeSpan.FromSeconds(2);

        public NetworkAvailabilityTracker(INetworkClient networkClient, ManualLogSource logger = null)
    {
        _networkClient = networkClient;
        _logger = logger;
    }

        public void MarkSyncCompleted()
    {
        LastSyncTime = DateTime.Now;
    }

        public bool CheckAvailability()
    {
        try
        {
            IsAvailable = _networkClient?.IsConnected ?? false;
            return IsAvailable;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"[NetAvailTracker] 网络可用性检查异常: {ex.Message}");
            IsAvailable = false;
            return false;
        }
    }

        public void SetAvailable()
    {
        IsAvailable = true;
        LastConnectionTime = DateTime.Now;
    }

        public void SetUnavailable()
    {
        IsAvailable = false;
    }

        public bool CanRequestFullSync()
    {
        long nowTicks = DateTime.UtcNow.Ticks;
        if (LastFullSyncRequestAtTicks > 0 &&
            (nowTicks - LastFullSyncRequestAtTicks) < FullSyncThrottleInterval.Ticks)
        {
            return false;
        }

        LastFullSyncRequestAtTicks = nowTicks;
        return true;
    }
}
