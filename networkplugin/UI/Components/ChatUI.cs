using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.UI.Components;

/// <summary>
/// 聊天UI组件 - 显示多人聊天消息
/// </summary>
public class ChatUI : MonoBehaviour
{
    [Header("UI组件")]
    public TMP_InputField inputField;        
    public TextMeshProUGUI chatDisplay;
    public ScrollRect scrollRect;
    public Button sendButton;
    public GameObject chatContainer;
    public GameObject messagePrefab;

    [Header("设置")]
    public int maxMessages = 100;
    public Color playerMessageColor = Color.white;
    public Color systemMessageColor = Color.yellow;
    public float messageFadeTime = 10f;

    private Queue<ChatMessage> messageQueue = new();      // 存储聊天消息的队列，先进先出
    private List<GameObject> messageObjects = [];          // 存储UI消息对象列表，用于管理和清理
    private IServiceProvider _serviceProvider;            // 依赖注入服务提供者
    private INetworkClient _networkClient;                // 网络客户端接口，负责消息发送

    private void Start()
    {
        _serviceProvider = ModService.ServiceProvider;
        _networkClient = _serviceProvider?.GetService<INetworkClient>();

        SetupUI();
        RegisterNetworkEvents();
    }    // 初始化聊天UI，设置服务依赖和事件注册

    private void Update()
    {
        // 处理消息淡出
        UpdateMessageFading();
    }    // 每帧更新，处理消息淡出等视觉效果

    private void OnDestroy()
    {
        UnregisterNetworkEvents();
    }    // 组件销毁时清理网络事件监听

    /// <summary>
    /// 设置UI组件
    /// </summary>
    private void SetupUI()
    {
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnMessageSubmitted);
            inputField.characterLimit = 200;
        }

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        if (chatDisplay != null)
        {
            chatDisplay.text = "聊天系统已启用...\n";
        }

        // 初始隐藏聊天容器
        if (chatContainer != null)
        {
            chatContainer.SetActive(false);
        }
    }

    /// <summary>
    /// 注册网络事件
    /// </summary>
    private void RegisterNetworkEvents()
    {
        // 注册聊天消息接收事件（当前通过 NetworkClient.OnGameEventReceived 分发）
        if (_networkClient is NetworkClient concrete)
        {
            NetworkIdentityTracker.EnsureSubscribed(concrete);
            concrete.OnGameEventReceived += OnNetworkGameEventReceived;
        }
    }

    /// <summary>
    /// 取消注册网络事件
    /// </summary>
    private void UnregisterNetworkEvents()
    {
        if (_networkClient is NetworkClient concrete)
        {
            concrete.OnGameEventReceived -= OnNetworkGameEventReceived;
        }
    }

    private void OnNetworkGameEventReceived(string eventType, object payload)
    {
        if (!string.Equals(eventType, NetworkMessageTypes.ChatMessage, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            ChatMessage message = TryDeserializeChatMessage(payload);
            if (message != null)
            {
                OnChatMessageReceived(message);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ChatUI] Error handling chat message: {ex.Message}");
        }
    }

    private static ChatMessage TryDeserializeChatMessage(object payload)
    {
        if (payload == null)
        {
            return null;
        }

        try
        {
            if (payload is ChatMessage cm)
            {
                return cm;
            }

            if (payload is JsonElement je)
            {
                return JsonSerializer.Deserialize<ChatMessage>(je.GetRawText());
            }

            if (payload is string s)
            {
                return JsonSerializer.Deserialize<ChatMessage>(s);
            }

            return JsonSerializer.Deserialize<ChatMessage>(JsonSerializer.Serialize(payload));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 切换聊天窗口显示
    /// </summary>
    public void ToggleChatWindow()
    {
        if (chatContainer != null)
        {
            bool isActive = chatContainer.activeSelf;
            chatContainer.SetActive(!isActive);

            if (!isActive)
            {
                // 重新激活时聚焦输入框
                if (inputField != null)
                {
                    inputField.Select();
                    inputField.ActivateInputField();
                }
            }
        }
    }    // 切换聊天窗口的显示/隐藏状态

    /// <summary>
    /// 发送聊天消息
    /// </summary>
    public void SendMessage(string message)
    {
        if (string.IsNullOrEmpty(message) || _networkClient == null || !_networkClient.IsConnected)
        {
            return;
        }

        ChatMessage chatMessage = new ChatMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            PlayerId = GetCurrentPlayerId(),
            PlayerName = GetCurrentPlayerName(),
            Content = message,
            Timestamp = DateTime.Now,
            MessageType = ChatMessageType.Player
        };

        try
        {
            _networkClient.SendRequest(NetworkMessageTypes.ChatMessage, System.Text.Json.JsonSerializer.Serialize(chatMessage));
            AddMessageToChat(chatMessage);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ChatUI] Failed to send chat message: {ex.Message}");
            AddSystemMessage("消息发送失败，请检查网络连接");
        }
    }    // 发送聊天消息到网络并显示在本地聊天窗口

    /// <summary>
    /// 添加消息到聊天显示
    /// </summary>
    public void AddMessageToChat(ChatMessage message)
    {
        messageQueue.Enqueue(message);

        if (messageQueue.Count > maxMessages)
        {
            messageQueue.Dequeue();
        }

        CreateMessageObject(message);
        UpdateChatDisplay();
    }

    /// <summary>
    /// 添加系统消息
    /// </summary>
    public void AddSystemMessage(string message)
    {
        ChatMessage systemMessage = new ChatMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            PlayerId = "System",
            PlayerName = "系统",
            Content = message,
            Timestamp = DateTime.Now,
            MessageType = ChatMessageType.System
        };

        AddMessageToChat(systemMessage);
    }

    /// <summary>
    /// 处理接收到的聊天消息
    /// </summary>
    private void OnChatMessageReceived(ChatMessage message)
    {
        AddMessageToChat(message);
    }

    /// <summary>
    /// 创建消息UI对象
    /// </summary>
    private void CreateMessageObject(ChatMessage message)
    {
        if (messagePrefab == null || chatContainer == null)
        {
            return;
        }

        var messageObj = Instantiate(messagePrefab, chatContainer.transform);
        var textComponent = messageObj.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            string formattedMessage = FormatMessage(message);
            textComponent.text = formattedMessage;
            textComponent.color = message.MessageType == ChatMessageType.System ? systemMessageColor : playerMessageColor;

            // 存储消息对象用于后续管理
            messageObj.name = $"Message_{message.MessageId}";
            messageObjects.Add(messageObj);
        }

        // 限制消息对象数量
        if (messageObjects.Count > maxMessages)
        {
            var oldestMessage = messageObjects[0];
            messageObjects.RemoveAt(0);
            Destroy(oldestMessage);
        }
    }

    /// <summary>
    /// 格式化消息显示
    /// </summary>
    private string FormatMessage(ChatMessage message)
    {
        string timeStr = message.Timestamp.ToString("HH:mm:ss");
        return message.MessageType switch
        {
            ChatMessageType.System => $"[{timeStr}] {message.Content}",
            ChatMessageType.Player => $"[{timeStr}] {message.PlayerName}: {message.Content}",
            _ => $"[{timeStr}] {message.Content}"
        };
    }

    /// <summary>
    /// 更新聊天显示
    /// </summary>
    private void UpdateChatDisplay()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f; // 滚动到底部
        }
    }

    /// <summary>
    /// 更新消息淡出效果
    /// </summary>
    private void UpdateMessageFading()
    {
        var now = DateTime.Now;

        foreach (var messageObj in messageObjects)
        {
            if (messageObj == null)
            {
                continue;
            }

            var textComponent = messageObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                var messageAge = now - new DateTime(messageObj.name.GetHashCode());
                if (messageAge.TotalSeconds > messageFadeTime)
                {
                    float alpha = Mathf.Clamp01(1f - (float)(messageAge.TotalSeconds - messageFadeTime) / messageFadeTime);
                    var color = textComponent.color;
                    color.a = alpha;
                    textComponent.color = color;
                }
            }
        }
    }

    /// <summary>
    /// 处理消息提交（回车键）
    /// </summary>
    private void OnMessageSubmitted(string message)
    {
        SendMessage(message);
        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();
    }

    /// <summary>
    /// 处理发送按钮点击
    /// </summary>
    private void OnSendButtonClicked()
    {
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            SendMessage(inputField.text);
            inputField.text = "";
        }
    }

    /// <summary>
    /// 获取当前玩家ID
    /// </summary>
    private string GetCurrentPlayerId()
    {
        try
        {
            string id = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            id = GameStateUtils.GetCurrentPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return "Unknown_Player";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ChatUI] Error getting player ID: {ex.Message}");
            return "Unknown_Player";
        }
    }

    /// <summary>
    /// 获取当前玩家名称
    /// </summary>
    private string GetCurrentPlayerName()
    {
        try
        {
            object player = GameStateUtils.GetCurrentPlayer();
            if (player != null)
            {
                // LBoL 的玩家对象可能有 userName/Name 等字段，优先取可读的名字
                var prop = player.GetType().GetProperty("userName") ?? player.GetType().GetProperty("UserName") ?? player.GetType().GetProperty("Name");
                if (prop != null && prop.PropertyType == typeof(string))
                {
                    string name = prop.GetValue(player) as string;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }

                var fallback = player.GetType().GetProperty("ModelName") ?? player.GetType().GetProperty("Id");
                if (fallback != null)
                {
                    object v = fallback.GetValue(player);
                    if (v != null)
                    {
                        return v.ToString();
                    }
                }
            }

            string id = NetworkIdentityTracker.GetSelfPlayerId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return "未知玩家";
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[ChatUI] Error getting player name: {ex.Message}");
            return "未知玩家";
        }
    }

    /// <summary>
    /// 设置聊天窗口可见性
    /// </summary>
    public void SetChatWindowVisible(bool visible)
    {
        if (chatContainer != null)
        {
            chatContainer.SetActive(visible);
        }
    }

    /// <summary>
    /// 清空聊天记录
    /// </summary>
    public void ClearChat()
    {
        foreach (var messageObj in messageObjects)
        {
            if (messageObj != null)
            {
                Destroy(messageObj);
            }
        }
        messageObjects.Clear();
        messageQueue.Clear();
    }
}

/// <summary>
/// 聊天消息数据结构
/// </summary>
[Serializable]
public class ChatMessage
{
    public string MessageId { get; set; }     // 消息的唯一标识符，用于消息追踪和管理
    public string PlayerId { get; set; }     // 发送消息的玩家ID
    public string PlayerName { get; set; }   // 发送消息的玩家显示名称
    public string Content { get; set; }      // 消息的具体内容文本
    public DateTime Timestamp { get; set; }  // 消息发送的时间戳
    public ChatMessageType MessageType { get; set; } // 消息类型（玩家消息/系统消息/错误消息）
}

/// <summary>
/// 聊天消息类型
/// </summary>
public enum ChatMessageType
{
    Player,
    System,
    Error
}
