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
	// Token: 0x02000486 RID: 1158
	[UsedImplicitly]
	public sealed class MoodChangeBlock : Card
	{
		// Token: 0x06000F7E RID: 3966 RVA: 0x0001BB21 File Offset: 0x00019D21
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoodChangeBlockSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
