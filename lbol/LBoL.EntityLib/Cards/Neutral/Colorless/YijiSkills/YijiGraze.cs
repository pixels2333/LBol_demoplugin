using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills
{
	// Token: 0x0200030F RID: 783
	[UsedImplicitly]
	public sealed class YijiGraze : OptionCard
	{
		// Token: 0x06000B9A RID: 2970 RVA: 0x000173B5 File Offset: 0x000155B5
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
