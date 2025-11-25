using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class FairyTeam : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this && card.CardType == CardType.Friend));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, list.Count, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> cards = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			int gunTimes = 1;
			if (cards != null && cards.Count > 0)
			{
				gunTimes += cards.Count;
				yield return new ExileManyCardAction(cards);
				ManaGroup manaGroup = Enumerable.Aggregate<Card, ManaGroup>(cards, ManaGroup.Empty, (ManaGroup current, Card card) => current + card.CostToMana(true));
				yield return new GainManaAction(manaGroup);
			}
			base.CardGuns = new Guns(base.GunName, gunTimes, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
