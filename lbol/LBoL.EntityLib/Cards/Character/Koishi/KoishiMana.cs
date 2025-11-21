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
	// Token: 0x02000476 RID: 1142
	[UsedImplicitly]
	public sealed class KoishiMana : Card
	{
		// Token: 0x06000F54 RID: 3924 RVA: 0x0001B800 File Offset: 0x00019A00
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
