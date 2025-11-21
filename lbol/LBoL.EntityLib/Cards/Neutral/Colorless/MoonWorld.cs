using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.White;

namespace LBoL.EntityLib.Cards.Neutral.Colorless
{
	// Token: 0x0200030B RID: 779
	[UsedImplicitly]
	public sealed class MoonWorld : Card
	{
		// Token: 0x06000B92 RID: 2962 RVA: 0x000172F3 File Offset: 0x000154F3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoonWorldSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
