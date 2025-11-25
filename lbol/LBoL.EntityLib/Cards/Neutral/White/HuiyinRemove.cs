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
	[UsedImplicitly]
	public sealed class HuiyinRemove : Card
	{
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
