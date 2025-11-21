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

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004A4 RID: 1188
	[UsedImplicitly]
	public sealed class CirnoEcho : Card
	{
		// Token: 0x06000FD1 RID: 4049 RVA: 0x0001C23C File Offset: 0x0001A43C
		public override Interaction Precondition()
		{
			IReadOnlyList<Card> discardZone = base.Battle.DiscardZone;
			if (discardZone.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(base.Value1, base.Value1, discardZone, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000FD2 RID: 4050 RVA: 0x0001C273 File Offset: 0x0001A473
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? Enumerable.FirstOrDefault<Card>(selectCardInteraction.SelectedCards) : null);
			if (card != null)
			{
				if (card.CanBeDuplicated)
				{
					card.IsEcho = true;
				}
				yield return new MoveCardAction(card, CardZone.Hand);
			}
			yield break;
		}
	}
}
