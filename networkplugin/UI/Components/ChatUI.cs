using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Chat;
using NetworkPlugin.Network.Services;
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
    private List<GameObject> messageObjects = [];          // 存储 UI 消息对象列表，用于管理和清理
    private Dictionary<GameObject, DateTime> messageCreatedAt = [];
    private INetworkClient _networkClient;                // 网络客户端接口，负责消息发送

    private void Start()
    {
        _networkClient = ModService.ServiceProvider?.GetService<INetworkClient>();

        SetupUI();

        // 注册聊天消息接收事件（当前通过 NetworkClient.OnGameEventReceived 分发）
        if (_networkClient is NetworkClient concrete)
        {
            NetworkIdentityTracker.EnsureSubscribed(concrete);
            concrete.OnGameEventReceived += OnNetworkGameEventReceived;
        }
    }    // 初始化聊天UI，设置服务依赖和事件注册

    private void Update()
    {
        // 处理消息淡出
        UpdateMessageFading();
    }    // 每帧更新，处理消息淡出等视觉效果

    private void OnDestroy()
    {
        if (_networkClient is NetworkClient concrete)
        {
            concrete.OnGameEventReceived -= OnNetworkGameEventReceived;
        }
    }    // 组件销毁时清理网络事件监听

    /// <summary>
    /// 设置UI组件
    /// </summary>
    private void SetupUI()
    {
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(message =>
            {
                SendMessage(message);
                inputField.text = "";
                inputField.Select();
                inputField.ActivateInputField();
            });
            inputField.characterLimit = 200;
        }

        sendButton?.onClick.AddListener(() =>
        {
            if (inputField != null && !string.IsNullOrEmpty(inputField.text))
            {
                SendMessage(inputField.text);
                inputField.text = "";
            }
        });

        if (chatDisplay != null) chatDisplay.text = "聊天系统已启用...\n";

        // 初始隐藏聊天容器
        chatContainer?.SetActive(false);
    }

    private void OnNetworkGameEventReceived(string eventType, object payload)
    {
        if (!string.Equals(eventType, NetworkMessageTypes.ChatMessage, StringComparison.Ordinal))
        {
            return;
        }

        ChatMessage message = TryDeserializeChatMessage(payload);
        if (message != null)
        {
            AddMessageToChat(message);
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
                return JsonCompat.Deserialize<ChatMessage>(je.GetRawText());
            }

            if (payload is string s)
            {
                return JsonCompat.Deserialize<ChatMessage>(s);
            }

            return JsonCompat.Deserialize<ChatMessage>(JsonCompat.Serialize(payload));
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
            Timestamp = DateTime.UtcNow,
            MessageType = ChatMessageType.Normal
        };

        try
        {
            _networkClient.SendRequest(NetworkMessageTypes.ChatMessage, JsonCompat.Serialize(chatMessage));
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
        
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f; // 滚动到底部
        }
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
            Timestamp = DateTime.UtcNow,
            MessageType = ChatMessageType.System
        };

        AddMessageToChat(systemMessage);
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
            string timeStr = message.Timestamp.ToString("HH:mm:ss");
            string formattedMessage = message.MessageType switch
            {
                ChatMessageType.System => $"[{timeStr}] {message.Content}",
                _ => $"[{timeStr}] {message.PlayerName}: {message.Content}"
            };
            textComponent.text = formattedMessage;
            textComponent.color = message.MessageType == ChatMessageType.System ? systemMessageColor : playerMessageColor;

            // 存储消息对象用于后续管理
            messageObj.name = $"Message_{message.MessageId}";
            messageObjects.Add(messageObj);
            messageCreatedAt[messageObj] = DateTime.UtcNow;
        }

        // 限制消息对象数量
        if (messageObjects.Count > maxMessages)
        {
            var oldestMessage = messageObjects[0];
            messageObjects.RemoveAt(0);
            messageCreatedAt.Remove(oldestMessage);
            Destroy(oldestMessage);
        }
    }



    /// <summary>
    /// 更新消息淡出效果
    /// </summary>
    private void UpdateMessageFading()
    {
        var now = DateTime.UtcNow;

        for (int i = messageObjects.Count - 1; i >= 0; i--)
        {
            var messageObj = messageObjects[i];
            if (messageObj == null)
            {
                messageObjects.RemoveAt(i);
                continue;
            }

            var textComponent = messageObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                if (!messageCreatedAt.TryGetValue(messageObj, out DateTime createdAt))
                {
                    createdAt = now;
                    messageCreatedAt[messageObj] = createdAt;
                }

                var messageAge = now - createdAt;
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
    /// 获取当前玩家ID
    /// </summary>
    private string GetCurrentPlayerId()
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

    /// <summary>
    /// 获取当前玩家名称
    /// </summary>
    private string GetCurrentPlayerName()
    {
        object player = GameStateUtils.GetCurrentPlayer();
        if (player != null)
        {
            string name = TryReadPlayerString(player, "userName", "UserName", "Name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            // LBoL 的玩家对象可能有 `ModelName` / `Id` 等字段，这里用作兜底显示名。
            string fallback = TryReadPlayerString(player, "ModelName", "Id");
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }
        }

        string id = NetworkIdentityTracker.GetSelfPlayerId();
        return !string.IsNullOrWhiteSpace(id) ? id : "未知玩家";
    }

    private static string TryReadPlayerString(object player, params string[] propertyNames)
    {
        Type playerType = player.GetType();
        foreach (string propertyName in propertyNames)
        {
            var property = playerType.GetProperty(propertyName);
            if (property == null)
            {
                continue;
            }

            object value = property.GetValue(player);
            if (value is string text && !string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (value != null)
            {
                string fallback = value.ToString();
                if (!string.IsNullOrWhiteSpace(fallback))
                {
                    return fallback;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 设置聊天窗口可见性
    /// </summary>
    public void SetChatWindowVisible(bool visible)
    {
        chatContainer?.SetActive(visible);
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
        messageCreatedAt.Clear();
        messageQueue.Clear();
    }
}
