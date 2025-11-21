using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003C9 RID: 969
	[UsedImplicitly]
	public sealed class ChuanyueJiejie : Card
	{
		// Token: 0x06000DA8 RID: 3496 RVA: 0x0001994D File Offset: 0x00017B4D
		public override Interaction Precondition()
		{
			if (base.Battle.ExileZone.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(1, 1, base.Battle.ExileZone, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x00019977 File Offset: 0x00017B77
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards[0] : null);
			if (card != null)
			{
				yield return new MoveCardAction(card, CardZone.Hand);
			}
			yield break;
		}
	}
}
