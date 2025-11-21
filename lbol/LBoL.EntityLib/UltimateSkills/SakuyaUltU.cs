using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x02000010 RID: 16
	[UsedImplicitly]
	public sealed class SakuyaUltU : UltimateSkill
	{
		// Token: 0x0600001C RID: 28 RVA: 0x000022D4 File Offset: 0x000004D4
		public SakuyaUltU()
		{
			base.TargetType = TargetType.Self;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000022E3 File Offset: 0x000004E3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return PerformAction.Spell(base.Owner, "The World");
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "skill", 1.6f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<ExtraTurn>(base.Battle.Player, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
