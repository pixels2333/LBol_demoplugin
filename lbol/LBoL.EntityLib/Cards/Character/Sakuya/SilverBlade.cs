using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003B9 RID: 953
	[UsedImplicitly]
	public sealed class SilverBlade : Card
	{
		// Token: 0x06000D79 RID: 3449 RVA: 0x00019590 File Offset: 0x00017790
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<SilverBladeSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
