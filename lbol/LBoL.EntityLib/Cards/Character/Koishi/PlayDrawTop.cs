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
	// Token: 0x02000490 RID: 1168
	[UsedImplicitly]
	public sealed class PlayDrawTop : Card
	{
		// Token: 0x06000F98 RID: 3992 RVA: 0x0001BD19 File Offset: 0x00019F19
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
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
