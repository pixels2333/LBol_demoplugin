// 服务器入站消息队列：基于 MessagePriorities 提供优先级队列与简单的队列上限保护。
using System.Collections.Generic;
using NetworkPlugin.Network.Messages;

namespace NetworkPlugin.Network.Server.Core;

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

    public int Count => _count;

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

