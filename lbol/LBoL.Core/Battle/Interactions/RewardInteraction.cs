using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle.Interactions
{
	public class RewardInteraction : Interaction
	{
		public IReadOnlyList<Exhibit> PendingExhibits { get; }
		public RewardInteraction(IEnumerable<Exhibit> exhibits)
		{
			this.PendingExhibits = new List<Exhibit>(exhibits).AsReadOnly();
		}
	}
}
