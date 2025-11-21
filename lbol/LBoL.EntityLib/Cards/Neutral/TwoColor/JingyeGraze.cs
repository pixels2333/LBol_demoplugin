using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000293 RID: 659
	[UsedImplicitly]
	public sealed class JingyeGraze : Card
	{
		// Token: 0x06000A53 RID: 2643 RVA: 0x00015936 File Offset: 0x00013B36
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Battle.Player.HasStatusEffect<Graze>())
			{
				yield return base.BuffAction<TempFirepower>(base.Value2 * base.Battle.Player.GetStatusEffect<Graze>().Level, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
