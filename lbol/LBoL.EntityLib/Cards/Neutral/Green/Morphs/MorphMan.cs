using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	// Token: 0x02000308 RID: 776
	[UsedImplicitly]
	public sealed class MorphMan : OptionCard
	{
		// Token: 0x06000B88 RID: 2952 RVA: 0x000171F3 File Offset: 0x000153F3
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return new GainPowerAction(base.Value1);
			yield break;
		}
	}
}
