using NetworkPlugin.UI.Models;

namespace NetworkPlugin.UI.Payloads;

public class ResurrectPayload
{
		public System.Collections.Generic.List<DeadPlayerEntry> Players { get; set; }

		public bool CanCancel { get; set; } = true;

		public System.Func<int, int> CostCalculator { get; set; }
}
