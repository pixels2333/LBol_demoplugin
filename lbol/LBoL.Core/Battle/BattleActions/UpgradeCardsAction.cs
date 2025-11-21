using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B1 RID: 433
	public class UpgradeCardsAction : SimpleAction
	{
		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x06000F65 RID: 3941 RVA: 0x000295DB File Offset: 0x000277DB
		public Card[] Cards { get; }

		// Token: 0x06000F66 RID: 3942 RVA: 0x000295E4 File Offset: 0x000277E4
		public UpgradeCardsAction(IEnumerable<Card> cards)
		{
			List<Card> list = new List<Card>();
			foreach (Card card in cards)
			{
				if (!card.CanUpgrade)
				{
					throw new InvalidOperationException("Cannot upgrade " + card.Name);
				}
				list.Add(card);
			}
			this.Cards = list.ToArray();
		}

		// Token: 0x06000F67 RID: 3943 RVA: 0x00029664 File Offset: 0x00027864
		protected override void ResolvePhase()
		{
			foreach (Card card in this.Cards)
			{
				if (card.CanUpgrade)
				{
					card.Upgrade();
				}
				else
				{
					Debug.LogError("Cannot upgrade " + card.Name);
				}
			}
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x000296B0 File Offset: 0x000278B0
		public override string ExportDebugDetails()
		{
			return "Cards = [" + string.Join(", ", Enumerable.Select<Card, string>(this.Cards, (Card c) => c.Name)) + "]";
		}
	}
}
