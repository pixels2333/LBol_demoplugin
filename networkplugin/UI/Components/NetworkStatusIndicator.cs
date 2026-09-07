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

    private IServiceProvider _serviceProvider;
    private INetworkClient _networkClient;
    private float lastPingUpdate;
    private ConnectionState _lastConnectionState;

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
    }

    private void Update()
    {
        UpdatePingDisplay();
        UpdateConnectionStatus();
    }

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
    }

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
    }

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

        if (connectionPanel != null)
        {

        }
    }

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
    }

        private int GetPingValue()
        => _networkClient?.Ping ?? 0;

        private void UpdateNatStatusDisplay()
    {
        string natSummary = NatTraversal.GetStatusSummary();

        if (natStatusText != null)
        {
            natStatusText.text = natSummary;
            return;
        }

        if (statusText != null)
        {
            string current = statusText.text ?? string.Empty;
            int lineBreak = current.IndexOf('\n');
            string firstLine = lineBreak >= 0 ? current.Substring(0, lineBreak) : current;
            statusText.text = $"{firstLine}\n{natSummary}";
        }
    }

        private int GetConnectedPlayerCount()
    {
        if (_networkClient?.IsConnected != true)
        {
            return 0;
        }

        return NetworkIdentityTracker.GetPlayerIdsSnapshot().Count;
    }

        private void AddSystemLog(string message)
    {
        Plugin.Logger?.LogInfo($"[NetworkStatusIndicator] {message}");
    }

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
    }

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

public enum ConnectionState
{
    Connected,
    Connecting,
    Disconnected,
    Reconnecting
}
