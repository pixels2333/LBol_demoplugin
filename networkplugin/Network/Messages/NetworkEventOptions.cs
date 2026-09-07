using LiteNetLib;

namespace NetworkPlugin.Network.Messages;

public struct NetworkEventOptions
{
        public bool IncludeSelf { get; set; }

        public string TargetPlayerId { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; }

        public static NetworkEventOptions StateBroadcast => new()
    {
        IncludeSelf = true,
        TargetPlayerId = null,
        DeliveryMethod = DeliveryMethod.ReliableOrdered
    };

        public static NetworkEventOptions ActionBroadcast => new()
    {
        IncludeSelf = false,
        TargetPlayerId = null,
        DeliveryMethod = DeliveryMethod.ReliableOrdered
    };

        public static NetworkEventOptions Targeted(string targetPlayerId) => new()
    {
        IncludeSelf = false,
        TargetPlayerId = targetPlayerId,
        DeliveryMethod = DeliveryMethod.ReliableOrdered
    };
}
