using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public class EscapeAction : EventBattleAction<UnitEventArgs>
	{
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
