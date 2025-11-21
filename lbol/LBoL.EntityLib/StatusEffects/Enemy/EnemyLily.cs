using System;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200009F RID: 159
	public sealed class EnemyLily : StatusEffect
	{
		// Token: 0x06000239 RID: 569 RVA: 0x0000695C File Offset: 0x00004B5C
		protected override void OnAdded(Unit unit)
		{
			this.React(PerformAction.Sfx("FairySupport", 0f));
			this.React(PerformAction.Effect(base.Owner, "LilyFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
		}
	}
}
