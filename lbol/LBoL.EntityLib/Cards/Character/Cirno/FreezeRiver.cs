using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004B6 RID: 1206
	[UsedImplicitly]
	public sealed class FreezeRiver : Card
	{
		// Token: 0x06000FFF RID: 4095 RVA: 0x0001C5F0 File Offset: 0x0001A7F0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card card = Enumerable.LastOrDefault<Card>(base.Battle.DrawZone);
			if (card != null)
			{
				card.SetTurnCost(base.Mana);
				yield return new MoveCardAction(card, CardZone.Hand);
			}
			yield break;
		}
	}
}
