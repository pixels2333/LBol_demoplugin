using System;
using System.Linq;
using LBoL.Base;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class RemoveAllNegativeStatusEffectAction : SimpleAction
	{
		public RemoveAllNegativeStatusEffectAction(Unit target, float waitTime = 0.2f)
		{
			this._target = target;
			this._waitTime = waitTime;
		}
		protected override void ResolvePhase()
		{
			base.React(PerformAction.Sfx("JingHua", 0f), null, default(ActionCause?));
			base.React(PerformAction.Effect(this._target, "JingHua", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f), null, default(ActionCause?));
			foreach (StatusEffect statusEffect in Enumerable.ToList<StatusEffect>(Enumerable.Where<StatusEffect>(this._target.StatusEffects, (StatusEffect se) => se.Type == StatusEffectType.Negative)))
			{
				base.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, default(ActionCause?));
			}
			base.React(PerformAction.Wait(this._waitTime, false), null, default(ActionCause?));
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		private readonly Unit _target;
		private readonly float _waitTime;
	}
}
