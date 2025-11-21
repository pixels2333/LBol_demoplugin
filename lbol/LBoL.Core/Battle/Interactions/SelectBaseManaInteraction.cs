using System;
using LBoL.Base;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000153 RID: 339
	public class SelectBaseManaInteraction : Interaction
	{
		// Token: 0x170004BD RID: 1213
		// (get) Token: 0x06000D8C RID: 3468 RVA: 0x00025765 File Offset: 0x00023965
		public int Min { get; }

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x06000D8D RID: 3469 RVA: 0x0002576D File Offset: 0x0002396D
		public int Max { get; }

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06000D8E RID: 3470 RVA: 0x00025775 File Offset: 0x00023975
		public ManaGroup PendingMana { get; }

		// Token: 0x06000D8F RID: 3471 RVA: 0x0002577D File Offset: 0x0002397D
		public SelectBaseManaInteraction(int min, int max, ManaGroup pendingMana)
		{
			this.Min = min;
			this.Max = max;
			this.PendingMana = pendingMana;
		}

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x06000D90 RID: 3472 RVA: 0x0002579A File Offset: 0x0002399A
		// (set) Token: 0x06000D91 RID: 3473 RVA: 0x000257A4 File Offset: 0x000239A4
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

		// Token: 0x0400063D RID: 1597
		private ManaGroup _selectMana;
	}
}
