using System;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class EnemyLily : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this.React(PerformAction.Sfx("FairySupport", 0f));
			this.React(PerformAction.Effect(base.Owner, "LilyFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
		}
	}
}
