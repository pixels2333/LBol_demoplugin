using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class XiangziEcho : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = new List<Card>();
			foreach (Card card in base.Battle.HandZone)
			{
				if (card != this && !card.IsEcho && card.CanBeDuplicated)
				{
					list.Add(card);
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards.NotEmpty<Card>())
				{
					using (IEnumerator<Card> enumerator = selectedCards.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Card card = enumerator.Current;
							card.IsEcho = true;
						}
						yield break;
					}
				}
			}
			yield break;
		}
	}
}
