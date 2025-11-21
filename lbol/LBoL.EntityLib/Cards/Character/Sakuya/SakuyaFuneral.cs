using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003A9 RID: 937
	[UsedImplicitly]
	public sealed class SakuyaFuneral : Card
	{
		// Token: 0x06000D51 RID: 3409 RVA: 0x00019347 File Offset: 0x00017547
		public override Interaction Precondition()
		{
			if (base.Battle.ExileZone.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(1, 1, base.Battle.ExileZone, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000D52 RID: 3410 RVA: 0x00019371 File Offset: 0x00017571
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards[0] : null);
			if (card != null)
			{
				yield return new MoveCardAction(card, CardZone.Hand);
				card.SetTurnCost(base.Mana);
			}
			yield break;
		}
	}
}
