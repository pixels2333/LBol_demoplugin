using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Core.Battle.BattleActions
{
	public class UpgradeCardsAction : SimpleAction
	{
		public Card[] Cards { get; }
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
		public override string ExportDebugDetails()
		{
			return "Cards = [" + string.Join(", ", Enumerable.Select<Card, string>(this.Cards, (Card c) => c.Name)) + "]";
		}
	}
}
