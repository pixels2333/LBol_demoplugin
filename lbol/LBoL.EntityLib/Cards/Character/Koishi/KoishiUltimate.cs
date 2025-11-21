using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200047D RID: 1149
	[UsedImplicitly]
	public sealed class KoishiUltimate : Card
	{
		// Token: 0x06000F64 RID: 3940 RVA: 0x0001B91A File Offset: 0x00019B1A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Card card = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone);
			if (card != null)
			{
				yield return new PlayCardAction(card);
			}
			yield return base.BuffAction<KoishiUltimateSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
