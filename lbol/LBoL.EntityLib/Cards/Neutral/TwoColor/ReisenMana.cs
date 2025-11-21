using System;
using System.Collections.Generic;
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
	// Token: 0x020002A4 RID: 676
	[UsedImplicitly]
	public sealed class ReisenMana : Card
	{
		// Token: 0x06000A81 RID: 2689 RVA: 0x00015C80 File Offset: 0x00013E80
		public override Interaction Precondition()
		{
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<UManaCard>());
			list.Add(Library.CreateCard<RManaCard>());
			List<Card> list2 = list;
			return new SelectCardInteraction(1, 1, list2, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x00015CB2 File Offset: 0x00013EB2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null)
			{
				yield break;
			}
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			List<Card> list = new List<Card>();
			list.Add(selectCardInteraction.SelectedCards[0]);
			list.Add(selectCardInteraction.SelectedCards[0].Clone(false));
			List<Card> list2 = list;
			yield return new AddCardsToHandAction(list2, AddCardsType.Normal);
			yield break;
		}
	}
}
