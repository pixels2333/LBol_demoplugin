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

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x02000260 RID: 608
	[UsedImplicitly]
	public sealed class ToolExile : Card
	{
		// Token: 0x060009CC RID: 2508 RVA: 0x00014F04 File Offset: 0x00013104
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, list.Count, list);
			}
			return null;
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x00014F4A File Offset: 0x0001314A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			if (selectHandInteraction == null)
			{
				yield break;
			}
			IReadOnlyList<Card> selectedCards = selectHandInteraction.SelectedCards;
			if (selectedCards.Count > 0)
			{
				yield return new ExileManyCardAction(selectedCards);
			}
			yield break;
		}
	}
}
