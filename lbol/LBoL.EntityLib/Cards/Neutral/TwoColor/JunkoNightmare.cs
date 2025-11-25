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
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class JunkoNightmare : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count <= 0 || base.Value2 <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value2, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> hand = Enumerable.ToList<Card>(base.Battle.HandZone);
			if (precondition != null)
			{
				foreach (Card card in ((SelectHandInteraction)precondition).SelectedCards)
				{
					hand.Remove(card);
				}
			}
			if (hand.Count > 0)
			{
				yield return new ExileManyCardAction(hand);
				yield return new AddCardsToHandAction(Library.CreateCards<CManaCard>(hand.Count, false), AddCardsType.Normal);
			}
			yield break;
			yield break;
		}
	}
}
