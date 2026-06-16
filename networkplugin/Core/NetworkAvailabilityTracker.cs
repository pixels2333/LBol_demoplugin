using System;
using BepInEx.Logging;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Core;

/// <summary>
/// 网络可用性跟踪器
/// 负责跟踪网络客户端连接状态、处理连接恢复/断开事件
/// </summary>
public sealed class NetworkAvailabilityTracker
{
    private readonly INetworkClient _networkClient;
    private readonly ManualLogSource _logger;

    /// <summary>网络客户端是否可用</summary>
    public bool IsAvailable { get; private set; }
    /// <summary>最近一次网络连接时间</summary>
    public DateTime LastConnectionTime { get; private set; } = DateTime.MinValue;
    /// <summary>最近一次同步完成时间</summary>
    public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;
    /// <summary>最近一次 FullSync 请求的 Tick 计数（用于节流）</summary>
    public long LastFullSyncRequestAtTicks { get; set; }

    // 节流：避免重连等路径在短时间内重复发起 FullStateSyncRequest
    private static readonly TimeSpan FullSyncThrottleInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// 初始化网络可用性跟踪器
    /// </summary>
    /// <param name="networkClient">网络客户端实例</param>
    /// <param name="logger">日志记录器</param>
    public NetworkAvailabilityTracker(INetworkClient networkClient, ManualLogSource logger = null)
    {
        _networkClient = networkClient;
        _logger = logger;
    }

    /// <summary>
    /// 标记一次同步完成
    /// </summary>
    public void MarkSyncCompleted()
    {
        LastSyncTime = DateTime.Now;
    }

    /// <summary>
    /// 检查网络客户端是否可用
    /// </summary>
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

    /// <summary>
    /// 标记网络可用并记录时间
    /// </summary>
    public void SetAvailable()
    {
        IsAvailable = true;
        LastConnectionTime = DateTime.Now;
    }

    /// <summary>
    /// 标记网络不可用
    /// </summary>
    public void SetUnavailable()
    {
        IsAvailable = false;
    }

    /// <summary>
    /// 获取 FullSync 请求是否可通过节流
    /// </summary>
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
