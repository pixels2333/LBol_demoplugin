using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Utils;
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
    public TextMeshProUGUI natStatusText;
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
        UpdateConnectionStatus();
    } // 组件启动时初始化依赖注入，设置UI和注册网络事件

    private void Update()
    {
        UpdatePingDisplay();
        UpdateConnectionStatus();
    } // 每 N 帧更新延迟和连接状态（帧节流优化）

    /// <summary>
    /// 设置UI组件
    /// </summary>
    private void SetupUI()
    {
        reconnectButton?.onClick.AddListener(() =>
        {
            if (_networkClient == null)
            {
                return;
            }

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

            try
            {
                _networkClient.Start();
                _networkClient.EnableAutoReconnect(true);
                _networkClient.ConnectToServer(host, port);
                AddSystemLog($"正在尝试重新连接: {host}:{port} ...");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[NetworkStatusIndicator] 重连失败: {ex.Message}");
                AddSystemLog("重连失败，请检查网络设置");
            }
        });

        connectionPanel?.SetActive(true);

        _lastConnectionState = ConnectionState.Disconnected;
    } // 设置UI组件，监听按钮点击事件并初始化连接状态

    /// <summary>
    /// 更新连接状态显示
    /// </summary>
    private void UpdateConnectionStatus()
    {
        ConnectionState currentState;
        if (_networkClient == null)
        {
            currentState = ConnectionState.Disconnected;
        }
        else if (_networkClient.IsConnected)
        {
            currentState = ConnectionState.Connected;
        }
        else if (_networkClient.IsConnecting)
        {
            currentState = ConnectionState.Connecting;
        }
        else
        {
            currentState = ConnectionState.Disconnected;
        }

        if (currentState != _lastConnectionState)
        {
            UpdateConnectionStatusUI(currentState);
            _lastConnectionState = currentState;
        }

        if (playerCountText != null)
        {
            int playerCount = GetConnectedPlayerCount();
            playerCountText.text = $"玩家: {playerCount}";
        }
    } // 更新连接状态显示，检测状态变化并更新UI和玩家数量

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

        if (connectionStatusIcon != null) connectionStatusIcon.color = iconColor;

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
        => _networkClient?.Ping ?? 0; // 获取网络延迟值，从网络客户端获取实际延迟或返回 0



    /// <summary>
    /// 更新 NAT 状态显示
    /// </summary>
    private void UpdateNatStatusDisplay()
    {
        string natSummary = NatTraversal.GetStatusSummary();

        if (natStatusText != null)
        {
            natStatusText.text = natSummary;
            return;
        }

        // 兼容旧预制体：未配置专用文本时，写入连接状态行第二行。
        if (statusText != null)
        {
            string current = statusText.text ?? string.Empty;
            int lineBreak = current.IndexOf('\n');
            string firstLine = lineBreak >= 0 ? current.Substring(0, lineBreak) : current;
            statusText.text = $"{firstLine}\n{natSummary}";
        }
    }

    /// <summary>
    /// 获取连接的玩家数量
    /// </summary>
    private int GetConnectedPlayerCount()
    {
        if (_networkClient?.IsConnected != true)
        {
            return 0;
        }

        // 基于 PlayerListUpdate 的快照统计玩家数量，比“已连接=1”更接近真实联机房间人数。
        return NetworkIdentityTracker.GetPlayerIdsSnapshot().Count;
    } // 获取连接的玩家数量，根据网络连接状态返回玩家数量



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
        StringBuilder details = new StringBuilder();
        details.AppendLine("=== 网络连接详情 ===");
        details.AppendLine($"状态: {_lastConnectionState}");
        details.AppendLine($"延迟: {GetPingValue()}ms");
        details.AppendLine($"玩家数量: {GetConnectedPlayerCount()}");

        if (_networkClient != null)
        {
            details.AppendLine($"本地地址: {_networkClient.LocalEndPoint?.ToString() ?? "Unknown"}");
            details.AppendLine($"远程地址: {_networkClient.RemoteEndPoint?.ToString() ?? "Unknown"}");
        }

        details.AppendLine(NatTraversal.GetStatusSummary());
        details.AppendLine(NatTraversal.GetConnectionStrategySummary());

        string connectionDetailsStr = details.ToString();
        Plugin.Logger?.LogInfo($"[NetworkStatus] Connection Details:\n{connectionDetailsStr}");
    } // 显示连接详情面板，生成并输出详细的网络连接信息

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
