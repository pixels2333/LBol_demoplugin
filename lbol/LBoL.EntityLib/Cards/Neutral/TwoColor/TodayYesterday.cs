using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B4 RID: 692
	[UsedImplicitly]
	public sealed class TodayYesterday : Card
	{
		// Token: 0x06000AA8 RID: 2728 RVA: 0x00015F82 File Offset: 0x00014182
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TodayYesterdaySe>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<Graze>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
