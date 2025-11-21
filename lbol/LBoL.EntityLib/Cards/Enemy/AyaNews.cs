using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x02000354 RID: 852
	[UsedImplicitly]
	public sealed class AyaNews : Card
	{
		// Token: 0x06000C5C RID: 3164 RVA: 0x00018245 File Offset: 0x00016445
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield break;
		}
	}
}
