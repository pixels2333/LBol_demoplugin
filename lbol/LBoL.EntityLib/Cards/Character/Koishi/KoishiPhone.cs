using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000477 RID: 1143
	[UsedImplicitly]
	public sealed class KoishiPhone : Card
	{
		// Token: 0x06000F56 RID: 3926 RVA: 0x0001B81F File Offset: 0x00019A1F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<KoishiPhoneSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
