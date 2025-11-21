using System;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B0 RID: 432
	public class UpgradeCardAction : SimpleAction
	{
		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x06000F61 RID: 3937 RVA: 0x0002955A File Offset: 0x0002775A
		public Card Card { get; }

		// Token: 0x06000F62 RID: 3938 RVA: 0x00029562 File Offset: 0x00027762
		public UpgradeCardAction(Card card)
		{
			if (!card.CanUpgrade)
			{
				throw new InvalidOperationException("Cannot upgrade " + card.Name);
			}
			this.Card = card;
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x0002958F File Offset: 0x0002778F
		protected override void ResolvePhase()
		{
			if (this.Card.CanUpgrade)
			{
				this.Card.Upgrade();
				return;
			}
			Debug.LogError("Cannot upgrade " + this.Card.Name);
		}

		// Token: 0x06000F64 RID: 3940 RVA: 0x000295C4 File Offset: 0x000277C4
		public override string ExportDebugDetails()
		{
			return "Card = " + this.Card.Name;
		}
	}
}
