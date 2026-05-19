using System;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Core;

/// <summary>
/// 网络可用性跟踪器
/// 负责跟踪网络客户端连接状态、处理连接恢复/断开事件
/// </summary>
internal sealed class NetworkAvailabilityTracker
{
    private readonly IServiceProvider _serviceProvider;
    private INetworkClient _networkClient;

    public bool IsAvailable { get; private set; }
    public DateTime LastConnectionTime { get; private set; } = DateTime.MinValue;
    public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;
    public long LastFullSyncRequestAtTicks { get; set; }

    // 节流：避免重连等路径在短时间内重复发起 FullStateSyncRequest
    private static readonly TimeSpan FullSyncThrottleInterval = TimeSpan.FromSeconds(2);

    public NetworkAvailabilityTracker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 标记一次同步完成
    /// </summary>
    public void MarkSyncCompleted()
    {
        LastSyncTime = DateTime.Now;
    }

    /// <summary>
    /// 检查网络客户端是否可用，必要时尝试重新初始化
    /// </summary>
    public bool CheckAvailability()
    {
        try
        {
            if (_networkClient == null)
                InitializeNetworkClient();

            IsAvailable = _networkClient?.IsConnected ?? false;
            return IsAvailable;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetAvailTracker] 网络可用性检查异常: {ex.Message}");
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

    /// <summary>
    /// 从 DI 容器获取并初始化网络客户端
    /// </summary>
    private void InitializeNetworkClient()
    {
        _networkClient = _serviceProvider?.GetService<INetworkClient>();
        if (_networkClient != null)
            Plugin.Logger?.LogInfo("[NetAvailTracker] 网络客户端初始化成功");
        else
            Plugin.Logger?.LogWarning("[NetAvailTracker] 网络客户端不可用 - 运行在离线模式");
    }
}
