using System;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Core.Battle.BattleActions
{
	public class UpgradeCardAction : SimpleAction
	{
		public Card Card { get; }
		public UpgradeCardAction(Card card)
		{
			if (!card.CanUpgrade)
			{
				throw new InvalidOperationException("Cannot upgrade " + card.Name);
			}
			this.Card = card;
		}
		protected override void ResolvePhase()
		{
			if (this.Card.CanUpgrade)
			{
				this.Card.Upgrade();
				return;
			}
			Debug.LogError("Cannot upgrade " + this.Card.Name);
		}
		public override string ExportDebugDetails()
		{
			return "Card = " + this.Card.Name;
		}
	}
}
