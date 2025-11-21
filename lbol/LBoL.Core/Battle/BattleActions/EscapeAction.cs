using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200017D RID: 381
	public class EscapeAction : EventBattleAction<UnitEventArgs>
	{
		// Token: 0x06000E6E RID: 3694 RVA: 0x000274F0 File Offset: 0x000256F0
		public EscapeAction(Unit unit)
		{
			if (!(unit is EnemyUnit))
			{
				throw new InvalidOperationException("Cannot let non-enemy unit " + unit.DebugName + " escape");
			}
			base.Args = new UnitEventArgs
			{
				Unit = unit,
				CanCancel = false
			};
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x0002753F File Offset: 0x0002573F
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Escaping", delegate
			{
				base.Args.Unit.Escaping.Execute(base.Args);
			}, false);
			yield return base.CreatePhase("Main", delegate
			{
				base.Args.CanCancel = false;
				base.Battle.Escape((EnemyUnit)base.Args.Unit);
			}, true);
			yield return base.CreatePhase("Escaped", delegate
			{
				base.Args.Unit.Escaped.Execute(base.Args);
				base.Battle.EnemyEscaped.Execute(base.Args);
			}, false);
			yield return base.CreatePhase("ClearEffects", delegate
			{
				foreach (StatusEffect statusEffect in Enumerable.ToList<StatusEffect>(base.Args.Unit.StatusEffects))
				{
					base.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, new ActionCause?(ActionCause.None));
				}
			}, false);
			yield break;
		}
	}
}
