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
	// Token: 0x0200048B RID: 1163
	[UsedImplicitly]
	public sealed class PassionMemory : Card
	{
		// Token: 0x06000F8B RID: 3979 RVA: 0x0001BBED File Offset: 0x00019DED
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
