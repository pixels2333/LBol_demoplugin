// 服务器入站消息队列：基于 MessagePriorities 提供优先级队列与简单的队列上限保护。
using System.Collections.Generic;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 服务器入站消息优先级队列
/// 基于 MessagePriorities 提供四个优先级级别（Low/Normal/High/Critical）的队列与队列上限保护
/// </summary>
public sealed class ServerMessageQueue
{
    private readonly Queue<ServerInboundMessage>[] _queues =
    {
        new Queue<ServerInboundMessage>(), // Low
        new Queue<ServerInboundMessage>(), // Normal
        new Queue<ServerInboundMessage>(), // High
        new Queue<ServerInboundMessage>(), // Critical
    };

    private int _count;

    /// <summary>当前队列中的消息总数</summary>
    public int Count => _count;

    /// <summary>
    /// 将消息入队，按优先级放入对应队列
    /// </summary>
    /// <param name="message">入站消息</param>
    /// <param name="maxQueueSize">队列上限，超过时丢弃最低优先级消息</param>
    public void Enqueue(ServerInboundMessage message, int maxQueueSize)
    {
        if (maxQueueSize > 0 && _count >= maxQueueSize)
        {
            DropOne();
            if (maxQueueSize > 0 && _count >= maxQueueSize)
            {
                return;
            }
        }

        _queues[(int)message.Priority].Enqueue(message);
        _count++;
    }

    /// <summary>
    /// 尝试从最高优先级队列出队一条消息
    /// </summary>
    /// <param name="message">出队的消息实例</param>
    /// <returns>是否成功出队</returns>
    public bool TryDequeueHighest(out ServerInboundMessage message)
    {
        for (int priority = 3; priority >= 0; priority--)
        {
            if (_queues[priority].Count > 0)
            {
                message = _queues[priority].Dequeue();
                _count--;
                return true;
            }
        }

        message = default;
        return false;
    }

    /// <summary>
    /// 丢弃一条最低优先级的消息
    /// </summary>
    private void DropOne()
    {
        for (int priority = 0; priority <= 3; priority++)
        {
            if (_queues[priority].Count > 0)
            {
                _queues[priority].Dequeue();
                _count--;
                return;
            }
        }
    }
}
