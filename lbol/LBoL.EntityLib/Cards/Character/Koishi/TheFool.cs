using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000494 RID: 1172
	[UsedImplicitly]
	public sealed class TheFool : Card
	{
		// Token: 0x06000FAB RID: 4011 RVA: 0x0001BF21 File Offset: 0x0001A121
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Card card = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone);
			if (card != null)
			{
				yield return new PlayCardAction(card);
			}
			yield break;
		}
	}
}
