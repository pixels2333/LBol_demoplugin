using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
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

    private IServiceProvider _serviceProvider;    // 依赖注入服务提供者
    private INetworkClient _networkClient;      // 网络客户端接口
    private float lastPingUpdate;                // 上次延迟更新时间
    private ConnectionState _lastConnectionState; // 上次连接状态，用于状态变化检测

    private void Start()
    {
        _serviceProvider = ModService.ServiceProvider;
        _networkClient = _serviceProvider?.GetService<INetworkClient>();        

        if (_networkClient != null)
        {
            NetworkIdentityTracker.EnsureSubscribed(_networkClient);
        }

        SetupUI();
        RegisterNetworkEvents();
        UpdateConnectionStatus();
    } // 组件启动时初始化依赖注入，设置UI和注册网络事件

    private void Update()
    {
        UpdatePingDisplay();
        UpdateConnectionStatus();
    } // 每帧更新延迟显示和连接状态

    private void OnDestroy()
    {
        UnregisterNetworkEvents();
    } // 组件销毁时取消注册网络事件，避免内存泄漏

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
    } // 设置UI组件，监听按钮点击事件并初始化连接状态

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
    } // 注册网络连接事件，监听连接状态、断开连接和延迟更新事件

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
    } // 取消注册网络事件，防止重复调用和内存泄漏

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
    } // 更新连接状态显示，检测状态变化并更新UI和玩家数量

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
    } // 获取当前连接状态，根据网络客户端属性返回相应的状态枚举

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
    } // 更新连接状态UI，根据状态设置图标颜色、状态文本和按钮可用性

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

        int ping = GetPingValue();
        pingText.text = $"延迟: {ping}ms";

        var pingColor = ping switch
        {
            var p when p > highPingThreshold => highPingColor,
            var p when p > mediumPingThreshold => mediumPingColor,
            _ => lowPingColor
        };

        pingText.color = pingColor;
        lastPingUpdate = Time.time;
    } // 更新延迟显示，根据延迟值设置颜色并在指定间隔内更新

    /// <summary>
    /// 获取延迟值
    /// </summary>
    private int GetPingValue()
    {
        try
        {
            return _networkClient?.Ping ?? 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Error getting ping: {ex.Message}");
            return 0;
        }
    } // 获取网络延迟值，从网络客户端获取实际延迟或返回0

    /// <summary>
    /// 更新玩家数量显示
    /// </summary>
    private void UpdatePlayerCount()
    {
        if (playerCountText == null)
        {
            return;
        }

        int playerCount = GetConnectedPlayerCount();
        playerCountText.text = $"玩家: {playerCount}";
    } // 更新玩家数量显示，获取连接玩家数量并更新UI文本

    /// <summary>
    /// 获取连接的玩家数量
    /// </summary>
    private int GetConnectedPlayerCount()
    {
        try
        {
            if (_networkClient?.IsConnected != true)
            {
                return 0;
            }

            // 基于 PlayerListUpdate 的快照统计玩家数量（比“已连接=1”更接近真实联机房间人数）
            return NetworkIdentityTracker.GetPlayerIdsSnapshot().Count;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Error getting player count: {ex.Message}");
            return 0;
        }
    } // 获取连接的玩家数量，根据网络连接状态返回玩家数量

    /// <summary>
    /// 处理重连按钮点击
    /// </summary>
    private void OnReconnectButtonClicked()
    {
        try
        {
            if (_networkClient != null)
            {
                if (_networkClient.IsConnected)
                {
                    AddSystemLog("已处于连接状态");
                    return;
                }

                string host = Plugin.ConfigManager?.ServerIP?.Value;
                int port = Plugin.ConfigManager?.ServerPort?.Value ?? 0;

                if (string.IsNullOrWhiteSpace(host) || port <= 0)
                {
                    AddSystemLog("重连失败：未配置 ServerIP/ServerPort");
                    return;
                }

                // 尽力启动客户端（可能已启动，异常忽略）
                try
                {
                    _networkClient.Start();
                }
                catch
                {
                    // ignore
                }

                // 启用自动重连，避免按钮点击后立刻掉线导致“卡死”体验
                _networkClient.EnableAutoReconnect(true);

                _networkClient.ConnectToServer(host, port);
                AddSystemLog($"正在尝试重新连接: {host}:{port} ...");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Reconnect failed: {ex.Message}");
            AddSystemLog("重连失败，请检查网络设置");
        }
    } // 处理重连按钮点击，尝试重新连接网络并记录状态

    /// <summary>
    /// 处理连接成功事件
    /// </summary>
    private void OnConnected()
    {
        AddSystemLog("连接成功");
    } // 处理连接成功事件，记录连接成功的系统日志

    /// <summary>
    /// 处理连接断开事件
    /// </summary>
    private void OnDisconnected()
    {
        AddSystemLog("连接断开");
    } // 处理连接断开事件，记录连接断开的系统日志

    /// <summary>
    /// 添加系统日志到UI
    /// </summary>
    private void AddSystemLog(string message)
    {
        Plugin.Logger?.LogInfo($"[NetworkStatusIndicator] {message}");
    } // 添加系统日志到控制台，包含组件标识符信息

    /// <summary>
    /// 显示连接详情面板
    /// </summary>
    public void ShowConnectionDetails()
    {
        try
        {
            string details = GenerateConnectionDetails();
            Plugin.Logger?.LogInfo($"[NetworkStatus] Connection Details:\n{details}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[NetworkStatusIndicator] Error showing connection details: {ex.Message}");
        }
    } // 显示连接详情面板，生成并输出详细的网络连接信息

    /// <summary>
    /// 生成连接详情
    /// </summary>
    private string GenerateConnectionDetails()
    {
        StringBuilder details = new System.Text.StringBuilder();
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
    } // 生成连接详情字符串，包含状态、延迟、地址和NAT信息

    // 静态属性用于状态存储
    private static bool _upnpEnabled = false;
    private static string _natType = "Unknown";

    /// <summary>
    /// 设置UPnP状态
    /// </summary>
    public static void SetUpnpStatus(bool enabled)
    {
        _upnpEnabled = enabled;
    } // 设置UPnP状态，用于网络连接详情显示

    /// <summary>
    /// 设置NAT类型
    /// </summary>
    public static void SetNatType(string natType)
    {
        _natType = natType;
    } // 设置NAT类型，用于网络连接详情显示

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
    } // 获取连接状态字符串表示，用于UI显示和日志记录
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
