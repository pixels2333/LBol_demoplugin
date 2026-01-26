// ============================================================================
// NetworkConnection.cs
// 网络连接封装类
// 功能：为服务器侧连接提供统一的 NetPeer 发送能力，支持房间广播和定向发送
// ============================================================================

using System;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkPlugin.Network.Room;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.Messages;

/// <summary>
/// 网络连接封装类
/// <para>封装了网络连接的相关信息，提供统一的消息发送接口</para>
/// </summary>
public class NetworkConnection
{
    #region 公共属性
    
    /// <summary>
    /// 当前房间ID
    /// </summary>
    public string CurrentRoomId { get; internal set; }
    
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string PlayerId { get; internal set; }
    
    /// <summary>
    /// LiteNetLib 网络对等端
    /// </summary>
    public NetPeer Peer { get; internal set; }
    
    #endregion
    
    #region 私有字段
    
    /// <summary>
    /// 原始发送委托（用于自定义发送逻辑）
    /// </summary>
    private readonly Action<NetPeer, string, string, DeliveryMethod>? _sendRaw;
    
    #endregion
    
    #region 构造函数
    
    /// <summary>
    /// 初始化 NetworkConnection 类的新实例
    /// </summary>
    /// <param name="peer">网络对等端</param>
    /// <param name="playerId">玩家ID</param>
    /// <param name="currentRoomId">当前房间ID（可选，默认为空字符串）</param>
    /// <param name="sendRaw">自定义发送委托（可选，用于覆盖默认发送逻辑）</param>
    /// <exception cref="ArgumentNullException">当 peer 或 playerId 为 null 时抛出</exception>
    public NetworkConnection(NetPeer peer, string playerId, string currentRoomId = "", Action<NetPeer, string, string, DeliveryMethod>? sendRaw = null)
    {
        // 参数验证
        Peer = peer ?? throw new ArgumentNullException(nameof(peer));
        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        
        // 初始化字段
        CurrentRoomId = currentRoomId ?? string.Empty;
        _sendRaw = sendRaw;
    }
    
    #endregion
    
    #region 公共方法
    
    /// <summary>
    /// 发送网络消息
    /// </summary>
    /// <param name="message">要发送的网络消息</param>
    /// <param name="deliveryMethod">消息传递方法（默认为可靠有序）</param>
    /// <remarks>
    /// 如果提供了自定义发送委托，则使用委托发送；
    /// 否则使用默认的 LiteNetLib 发送机制
    /// </remarks>
    public void SendMessage(NetworkMessage message, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        // 空消息检查
        if (message == null)
        {
            return;
        }

        // 序列化消息负载
        string payloadJson = message.Payload switch
        {
            null => string.Empty,           // 空负载
            string s => s,                  // 已经是字符串
            _ => JsonCompat.Serialize(message.Payload), // 序列化对象
        };

        // 使用自定义发送委托（如果提供）
        if (_sendRaw != null)
        {
            _sendRaw(Peer, message.Type, payloadJson, deliveryMethod);
            return;
        }

        // 使用默认的 LiteNetLib 发送机制
        NetDataWriter writer = new();
        writer.Put(message.Type);      // 写入消息类型
        writer.Put(payloadJson);       // 写入负载JSON
        Peer.Send(writer, deliveryMethod); // 发送消息
    }
    
    #endregion
}
