using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Components;

/// <summary>
/// 网络状态指示器 - 显示连接状态、延迟、玩家列表等
/// </summary>
public class NetworkStatusIndicator : MonoBehaviour
{
    [Header("状态指示器")]
    public Image connectionStatusIcon;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI playerCountText;
    public Button reconnectButton;
    public GameObject connectionPanel;

    [Header("状态颜色")]
    public Color connectedColor = Color.green;
    public Color connectingColor = Color.yellow;
    public Color disconnectedColor = Color.red;
    public Color highPingColor = Color.red;
    public Color mediumPingColor = Color.yellow;
    public Color lowPingColor = Color.green;

    [Header("设置")]
    public float pingUpdateInterval = 1f;
    public float highPingThreshold = 200f;
    public float mediumPingThreshold = 100f;

    private IServiceProvider _serviceProvider;
    private INetworkClient _networkClient;
    private float lastPingUpdate;
    private ConnectionState _lastConnectionState;

    private void Start()
    {
        _serviceProvider = ModService.ServiceProvider;
        _networkClient = _serviceProvider?.GetService<INetworkClient>();

        SetupUI();
        RegisterNetworkEvents();
        UpdateConnectionStatus();
    }

    private void Update()
    {
        UpdatePingDisplay();
        UpdateConnectionStatus();
    }

    private void OnDestroy()
    {
        UnregisterNetworkEvents();
    }

    /// <summary>
    /// 设置UI组件
    /// </summary>
    private void SetupUI()
    {
        if (reconnectButton != null)
        {
            reconnectButton.onClick.AddListener(OnReconnectButtonClicked);
        }

        if (connectionPanel != null)
        {
            connectionPanel.SetActive(true);
        }

        _lastConnectionState = ConnectionState.Disconnected;
    }

    /// <summary>
    /// 注册网络事件
    /// </summary>
    private void RegisterNetworkEvents()
    {
        if (_networkClient != null)
        {
            // _networkClient.OnConnected += OnConnected;
            // _networkClient.OnDisconnected += OnDisconnected;
            // _networkClient.OnPingUpdate += OnPingUpdate;
        }
    }

    /// <summary>
    /// 取消注册网络事件
    /// </summary>
    private void UnregisterNetworkEvents()
    {
        if (_networkClient != null)
        {
            // _networkClient.OnConnected -= OnConnected;
            // _networkClient.OnDisconnected -= OnDisconnected;
            // _networkClient.OnPingUpdate -= OnPingUpdate;
        }
    }

    /// <summary>
    /// 更新连接状态显示
    /// </summary>
    private void UpdateConnectionStatus()
    {
        var currentState = GetConnectionState();

        if (currentState != _lastConnectionState)
        {
            UpdateConnectionStatusUI(currentState);
            _lastConnectionState = currentState;
        }

        UpdatePlayerCount();
    }

    /// <summary>
    /// 获取当前连接状态
    /// </summary>
    private ConnectionState GetConnectionState()
    {
        if (_networkClient == null)
        {
            return ConnectionState.Disconnected;
        }

        if (_networkClient.IsConnected)
        {
            return ConnectionState.Connected;
        }

        if (_networkClient.IsConnecting)
        {
            return ConnectionState.Connecting;
        }

        return ConnectionState.Disconnected;
    }

    /// <summary>
    /// 更新连接状态UI
    /// </summary>
    private void UpdateConnectionStatusUI(ConnectionState state)
    {
        var (iconColor, statusMessage) = state switch
        {
            ConnectionState.Connected => (connectedColor, "已连接"),
            ConnectionState.Connecting => (connectingColor, "连接中..."),
            ConnectionState.Disconnected => (disconnectedColor, "未连接"),
            ConnectionState.Reconnecting => (connectingColor, "重连中..."),
            _ => (disconnectedColor, "未知状态")
        };

        if (connectionStatusIcon != null)
        {
            connectionStatusIcon.color = iconColor;
        }

        if (statusText != null)
        {
            statusText.text = statusMessage;
            statusText.color = iconColor;
        }

        if (reconnectButton != null)
        {
            reconnectButton.interactable = state == ConnectionState.Disconnected;
            reconnectButton.gameObject.SetActive(state == ConnectionState.Disconnected);
        }

        // 根据连接状态显示/隐藏相关面板
        if (connectionPanel != null)
        {
            // 可以根据需要控制面板的显示
        }
    }

    /// <summary>
    /// 更新延迟显示
    /// </summary>
    private void UpdatePingDisplay()
    {
        if (Time.time - lastPingUpdate < pingUpdateInterval)
        {
            return;
        }

        if (pingText == null || _networkClient == null)
        {
            return;
        }

        var ping = GetPingValue();
        pingText.text = $"延迟: {ping}ms";

        var pingColor = ping switch
        {
            var p when p > highPingThreshold => highPingColor,
            var p when p > mediumPingThreshold => mediumPingColor,
            _ => lowPingColor
        };

        pingText.color = pingColor;
        lastPingUpdate = Time.time;
    }

    /// <summary>
    /// 获取延迟值
    /// </summary>
    private int GetPingValue()
    {
        try
        {
            // TODO: 从NetworkClient获取实际延迟值
            return _networkClient?.Ping ?? 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Error getting ping: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 更新玩家数量显示
    /// </summary>
    private void UpdatePlayerCount()
    {
        if (playerCountText == null)
        {
            return;
        }

        var playerCount = GetConnectedPlayerCount();
        playerCountText.text = $"玩家: {playerCount}";
    }

    /// <summary>
    /// 获取连接的玩家数量
    /// </summary>
    private int GetConnectedPlayerCount()
    {
        try
        {
            // TODO: 从NetworkClient获取实际玩家数量
            return _networkClient?.IsConnected == true ? 1 : 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Error getting player count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 处理重连按钮点击
    /// </summary>
    private void OnReconnectButtonClicked()
    {
        try
        {
            if (_networkClient != null)
            {
                // TODO: 实现重连逻辑
                // _networkClient.Reconnect();
                AddSystemLog("正在尝试重新连接...");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Reconnect failed: {ex.Message}");
            AddSystemLog("重连失败，请检查网络设置");
        }
    }

    /// <summary>
    /// 处理连接成功事件
    /// </summary>
    private void OnConnected()
    {
        AddSystemLog("连接成功");
    }

    /// <summary>
    /// 处理连接断开事件
    /// </summary>
    private void OnDisconnected()
    {
        AddSystemLog("连接断开");
    }

    /// <summary>
    /// 添加系统日志到UI
    /// </summary>
    private void AddSystemLog(string message)
    {
        Plugin.Logger?.LogInfo($"[NetworkStatusIndicator] {message}");
    }

    /// <summary>
    /// 显示连接详情面板
    /// </summary>
    public void ShowConnectionDetails()
    {
        try
        {
            var details = GenerateConnectionDetails();
            Plugin.Logger?.LogInfo($"[NetworkStatus] Connection Details:\n{details}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Error showing connection details: {ex.Message}");
        }
    }

    /// <summary>
    /// 生成连接详情
    /// </summary>
    private string GenerateConnectionDetails()
    {
        var details = new System.Text.StringBuilder();
        details.AppendLine("=== 网络连接详情 ===");
        details.AppendLine($"状态: {_lastConnectionState}");
        details.AppendLine($"延迟: {GetPingValue()}ms");
        details.AppendLine($"玩家数量: {GetConnectedPlayerCount()}");

        if (_networkClient != null)
        {
            details.AppendLine($"本地地址: {_networkClient.LocalEndPoint?.ToString() ?? "Unknown"}");
            details.AppendLine($"远程地址: {_networkClient.RemoteEndPoint?.ToString() ?? "Unknown"}");
        }

        details.AppendLine($"UPnP状态: {(_upnpEnabled ? "已启用" : "未启用")}");
        details.AppendLine($"NAT类型: {_natType}");

        return details.ToString();
    }

    // 静态属性用于状态存储
    private static bool _upnpEnabled = false;
    private static string _natType = "Unknown";

    /// <summary>
    /// 设置UPnP状态
    /// </summary>
    public static void SetUpnpStatus(bool enabled)
    {
        _upnpEnabled = enabled;
    }

    /// <summary>
    /// 设置NAT类型
    /// </summary>
    public static void SetNatType(string natType)
    {
        _natType = natType;
    }

    /// <summary>
    /// 获取连接状态字符串
    /// </summary>
    public string GetConnectionStatusString()
    {
        return _lastConnectionState switch
        {
            ConnectionState.Connected => "已连接",
            ConnectionState.Connecting => "连接中",
            ConnectionState.Disconnected => "未连接",
            ConnectionState.Reconnecting => "重连中",
            _ => "未知"
        };
    }
}

/// <summary>
/// 连接状态枚举
/// </summary>
public enum ConnectionState
{
    Connected,
    Connecting,
    Disconnected,
    Reconnecting
}