using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000455 RID: 1109
	[UsedImplicitly]
	public sealed class AncestorDream : Card
	{
		// Token: 0x06000F0A RID: 3850 RVA: 0x0001B362 File Offset: 0x00019562
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DreamCardsAction(base.Value1, base.Value2);
			yield break;
		}
	}
}
