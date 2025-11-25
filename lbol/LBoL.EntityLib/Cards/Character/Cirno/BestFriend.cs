using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class BestFriend : Card
	{
		public override ManaGroup? PlentifulMana
		{
			get
			{
				return new ManaGroup?(base.Mana);
			}
		}
		protected override string GetBaseDescription()
		{
			if (!base.PlentifulHappenThisTurn)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this && card.CardType == CardType.Friend));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			if (readOnlyList != null)
			{
				using (IEnumerator<Card> enumerator = readOnlyList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card = enumerator.Current;
						card.NotifyActivating();
						card.Loyalty += 9;
					}
					yield break;
				}
			}
			yield break;
		}
	}
}
