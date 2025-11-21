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

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000275 RID: 629
	[UsedImplicitly]
	public sealed class HuiyinRemove : Card
	{
		// Token: 0x060009FB RID: 2555 RVA: 0x00015190 File Offset: 0x00013390
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(base.GameRun.BaseDeckWithoutUnremovable);
			Card deckCardByInstanceId = base.GameRun.GetDeckCardByInstanceId(base.InstanceId);
			if (deckCardByInstanceId != null)
			{
				list.Remove(deckCardByInstanceId);
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(1, 1, list, SelectedCardHandling.DoNothing);
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x000151DF File Offset: 0x000133DF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? Enumerable.FirstOrDefault<Card>(selectCardInteraction.SelectedCards) : null);
			if (card != null)
			{
				base.GameRun.RemoveDeckCard(card, true);
			}
			yield break;
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x000151F6 File Offset: 0x000133F6
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			Card deckCardByInstanceId = base.GameRun.GetDeckCardByInstanceId(base.InstanceId);
			if (deckCardByInstanceId != null)
			{
				base.GameRun.RemoveDeckCard(deckCardByInstanceId, false);
			}
			yield return new RemoveCardAction(this);
			yield break;
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x00015206 File Offset: 0x00013406
		public override IEnumerable<BattleAction> AfterFollowPlayAction()
		{
			Card deckCardByInstanceId = base.GameRun.GetDeckCardByInstanceId(base.InstanceId);
			if (deckCardByInstanceId != null)
			{
				base.GameRun.RemoveDeckCard(deckCardByInstanceId, false);
			}
			yield return new RemoveCardAction(this);
			yield break;
		}
	}
}
