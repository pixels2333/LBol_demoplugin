using System;
using System.Linq;
using LBoL.Base;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000199 RID: 409
	public class RemoveAllNegativeStatusEffectAction : SimpleAction
	{
		// Token: 0x06000F06 RID: 3846 RVA: 0x0002885F File Offset: 0x00026A5F
		public RemoveAllNegativeStatusEffectAction(Unit target, float waitTime = 0.2f)
		{
			this._target = target;
			this._waitTime = waitTime;
		}

		// Token: 0x06000F07 RID: 3847 RVA: 0x00028878 File Offset: 0x00026A78
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

		// Token: 0x17000523 RID: 1315
		// (get) Token: 0x06000F08 RID: 3848 RVA: 0x00028990 File Offset: 0x00026B90
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000695 RID: 1685
		private readonly Unit _target;

		// Token: 0x04000696 RID: 1686
		private readonly float _waitTime;
	}
}
