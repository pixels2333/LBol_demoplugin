using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	// Token: 0x02000306 RID: 774
	[UsedImplicitly]
	public sealed class MorphDog : OptionCard
	{
		// Token: 0x06000B84 RID: 2948 RVA: 0x000171C3 File Offset: 0x000153C3
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
