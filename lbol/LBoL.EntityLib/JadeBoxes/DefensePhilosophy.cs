using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class DefensePhilosophy : JadeBox
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Defense)
			{
				yield return new GainManaAction(base.Mana);
				if (base.Battle.HandZone.Count > 0)
				{
					yield return new DiscardAction(Enumerable.First<Card>(base.Battle.HandZone));
				}
			}
			yield break;
		}
	}
}
