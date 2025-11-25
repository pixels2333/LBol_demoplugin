using System;
using LBoL.Base.Extensions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	[ActionViewerType(typeof(ApplyStatusEffectAction))]
	public sealed class ApplyStatusEffectAction<TEffect> : ApplyStatusEffectAction where TEffect : StatusEffect
	{
		public override string Name
		{
			get
			{
				return "ApplyStatusEffectAction".TryRemoveEnd("Action");
			}
		}
		public ApplyStatusEffectAction(Unit target, int? level = null, int? duration = null, int? count = null, int? limit = null, float occupationTime = 0f, bool startAutoDecreasing = true)
			: base(typeof(TEffect), target, level, duration, count, limit, occupationTime, startAutoDecreasing)
		{
		}
	}
}
