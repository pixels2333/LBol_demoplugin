using System;
using System.Collections.Generic;

namespace NetworkPlugin.Utils;

internal class NetworkEventBuffer
{
        public long Timestamp { get; }

        public Dictionary<string, object> OriginalData { get; }

        public DateTime ReceivedAt { get; }

        public ProcessingStatus Status { get; set; }

        public NetworkEventBuffer(long timestamp, Dictionary<string, object> originalData)
    {
        Timestamp = timestamp;
        OriginalData = originalData ?? throw new ArgumentNullException(nameof(originalData));
        ReceivedAt = DateTime.Now;
        Status = ProcessingStatus.Pending;
    }

        public bool IsTimeout(TimeSpan timeout)
    {
        return DateTime.Now - ReceivedAt > timeout;
    }

        public enum ProcessingStatus
    {
                Pending,

                Processing,

                Completed,

                Discarded
    }
}
