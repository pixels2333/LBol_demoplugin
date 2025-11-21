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
	// Token: 0x02000456 RID: 1110
	[UsedImplicitly]
	public sealed class BabyDream : Card
	{
		// Token: 0x06000F0C RID: 3852 RVA: 0x0001B381 File Offset: 0x00019581
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DreamCardsAction(base.Value1, base.Value2);
			yield break;
		}
	}
}
