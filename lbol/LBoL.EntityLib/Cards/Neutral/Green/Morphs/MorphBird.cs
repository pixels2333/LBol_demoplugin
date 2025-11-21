using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	// Token: 0x02000305 RID: 773
	[UsedImplicitly]
	public sealed class MorphBird : OptionCard
	{
		// Token: 0x06000B82 RID: 2946 RVA: 0x000171AB File Offset: 0x000153AB
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
