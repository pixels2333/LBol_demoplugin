using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	[UsedImplicitly]
	public sealed class SanaeRestart : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, list.Count, list);
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			if (precondition != null)
			{
				list.AddRange(((SelectHandInteraction)precondition).SelectedCards);
			}
			List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Concat<Card>(list, base.Battle.DiscardZone));
			foreach (Card card in list2)
			{
				yield return new MoveCardToDrawZoneAction(card, DrawZoneTarget.Top);
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			yield return new ReshuffleAction();
			yield return new DrawManyCardAction(base.Value1);
			yield return new GainManaAction(base.Mana);
			yield break;
			yield break;
		}
	}
}
