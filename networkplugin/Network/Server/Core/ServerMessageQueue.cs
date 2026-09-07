using System.Collections.Generic;

namespace NetworkPlugin.Network.Server.Core;

public sealed class ServerMessageQueue
{
    private readonly object _lock = new();
    private readonly Queue<ServerInboundMessage>[] _queues =
    {
        new Queue<ServerInboundMessage>(),
        new Queue<ServerInboundMessage>(),
        new Queue<ServerInboundMessage>(),
        new Queue<ServerInboundMessage>(),
    };

    private int _count;

        public int Count
    {
        get
        {
            lock (_lock)
            {
                return _count;
            }
        }
    }

        public void Enqueue(ServerInboundMessage message, int maxQueueSize)
    {
        lock (_lock)
        {
            if (maxQueueSize > 0 && _count >= maxQueueSize)
            {
                DropOneLocked();
                if (maxQueueSize > 0 && _count >= maxQueueSize)
                {
                    return;
                }
            }

            _queues[(int)message.Priority].Enqueue(message);
            _count++;
        }
    }

        public bool TryDequeueHighest(out ServerInboundMessage message)
    {
        lock (_lock)
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
    }

        public void Clear()
    {
        lock (_lock)
        {
            for (int priority = 0; priority <= 3; priority++)
            {
                _queues[priority].Clear();
            }
            _count = 0;
        }
    }

        private void DropOneLocked()
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
