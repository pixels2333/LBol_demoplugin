using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x0200027D RID: 637
	[UsedImplicitly]
	public sealed class TannvCard : Card
	{
		// Token: 0x06000A15 RID: 2581 RVA: 0x00015411 File Offset: 0x00013611
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.DrawZone.Count > 0)
			{
				Card drawCard = Enumerable.First<Card>(base.Battle.DrawZone);
				yield return new MoveCardAction(drawCard, CardZone.Hand);
				drawCard.SetTurnCost(base.Mana);
				drawCard = null;
			}
			if (base.Battle.DiscardZone.Count > 0)
			{
				Card drawCard = Enumerable.Last<Card>(base.Battle.DiscardZone);
				yield return new MoveCardAction(drawCard, CardZone.Hand);
				drawCard.SetTurnCost(base.Mana);
				drawCard = null;
			}
			yield break;
		}
	}
}
