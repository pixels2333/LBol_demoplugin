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
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200048D RID: 1165
	[UsedImplicitly]
	public sealed class PeaceEndTurn : Card
	{
		// Token: 0x06000F90 RID: 3984 RVA: 0x0001BC40 File Offset: 0x00019E40
		public override Interaction Precondition()
		{
			if (base.Value1 <= 0)
			{
				return null;
			}
			IEnumerable<Card> enumerable;
			if (!this.IsUpgraded)
			{
				IEnumerable<Card> handZone = base.Battle.HandZone;
				enumerable = handZone;
			}
			else
			{
				enumerable = Enumerable.Concat<Card>(base.Battle.HandZone, base.Battle.DiscardZone);
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(enumerable, (Card card) => card != this));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(0, base.Value1, list, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000F91 RID: 3985 RVA: 0x0001BCBA File Offset: 0x00019EBA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> cards = ((SelectCardInteraction)precondition).SelectedCards;
				foreach (Card card in cards)
				{
					if (card.Zone == CardZone.Discard)
					{
						yield return new MoveCardAction(card, CardZone.Hand);
					}
				}
				IEnumerator<Card> enumerator = null;
				foreach (Card card2 in cards)
				{
					if (card2.Zone == CardZone.Hand && !card2.IsRetain && !card2.Summoned)
					{
						card2.IsTempRetain = true;
					}
				}
				cards = null;
			}
			yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
			yield return new RequestEndPlayerTurnAction();
			yield break;
			yield break;
		}
	}
}
