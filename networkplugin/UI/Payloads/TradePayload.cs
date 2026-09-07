namespace NetworkPlugin.UI.Payloads;

public class TradePayload
{
		public string TradeId { get; set; }

		public string Player1Id { get; set; }

		public string Player2Id { get; set; }

		public string Player1Name { get; set; }

		public string Player2Name { get; set; }

		public bool CanCancel { get; set; } = true;

		public int MaxTradeSlots { get; set; } = 3;
}
