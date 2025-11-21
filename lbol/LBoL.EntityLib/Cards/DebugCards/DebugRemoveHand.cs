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

namespace LBoL.EntityLib.Cards.DebugCards
{
	// Token: 0x0200037B RID: 891
	[UsedImplicitly]
	public sealed class DebugRemoveHand : Card
	{
		// Token: 0x06000CBD RID: 3261 RVA: 0x000188DC File Offset: 0x00016ADC
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, list.Count, list);
			}
			return null;
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x00018922 File Offset: 0x00016B22
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			if (selectHandInteraction == null)
			{
				yield break;
			}
			IReadOnlyList<Card> selectedCards = selectHandInteraction.SelectedCards;
			foreach (Card card in selectedCards)
			{
				if (card.Battle != null)
				{
					yield return new RemoveCardAction(card);
				}
			}
			IEnumerator<Card> enumerator = null;
			yield break;
			yield break;
		}
	}
}
