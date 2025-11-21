using System;
using LBoL.Base.Extensions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000162 RID: 354
	[ActionViewerType(typeof(ApplyStatusEffectAction))]
	public sealed class ApplyStatusEffectAction<TEffect> : ApplyStatusEffectAction where TEffect : StatusEffect
	{
		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x06000DD6 RID: 3542 RVA: 0x00026169 File Offset: 0x00024369
		public override string Name
		{
			get
			{
				return "ApplyStatusEffectAction".TryRemoveEnd("Action");
			}
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x0002617C File Offset: 0x0002437C
		public ApplyStatusEffectAction(Unit target, int? level = null, int? duration = null, int? count = null, int? limit = null, float occupationTime = 0f, bool startAutoDecreasing = true)
			: base(typeof(TEffect), target, level, duration, count, limit, occupationTime, startAutoDecreasing)
		{
		}
	}
}
