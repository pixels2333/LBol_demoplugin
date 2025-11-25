using System;
using LBoL.Base;
namespace LBoL.Core.Battle.Interactions
{
	public class SelectBaseManaInteraction : Interaction
	{
		public int Min { get; }
		public int Max { get; }
		public ManaGroup PendingMana { get; }
		public SelectBaseManaInteraction(int min, int max, ManaGroup pendingMana)
		{
			this.Min = min;
			this.Max = max;
			this.PendingMana = pendingMana;
		}
		public ManaGroup SelectedMana
		{
			get
			{
				return this._selectMana;
			}
			set
			{
				int amount = value.Amount;
				if (amount < this.Min || amount > this.Max)
				{
					throw new InvalidOperationException(string.Format("Invalid {0} count = {1} for {2}", "value", amount, "SelectBaseManaInteraction"));
				}
				this._selectMana = value;
			}
		}
		private ManaGroup _selectMana;
	}
}
