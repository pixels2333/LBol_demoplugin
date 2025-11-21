using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000075 RID: 117
	[UsedImplicitly]
	public sealed class Inspiration : StatusEffect
	{
		// Token: 0x06000196 RID: 406 RVA: 0x00005268 File Offset: 0x00003468
		protected override void OnAdded(Unit unit)
		{
			string text = "Inspiration" + base.Level.ToString();
			this.React(PerformAction.Effect(base.Owner, text, 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
		}

		// Token: 0x06000197 RID: 407 RVA: 0x000052B8 File Offset: 0x000034B8
		public override bool Stack(StatusEffect other)
		{
			string text = "Inspiration" + (base.Level + 1).ToString();
			this.React(PerformAction.Effect(base.Owner, text, 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
			return base.Stack(other);
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000530F File Offset: 0x0000350F
		public override IEnumerable<BattleAction> StackAction(Unit targetOwner, int targetLevel)
		{
			yield return new ApplyStatusEffectAction<MoodEpiphany>(targetOwner, new int?(targetLevel), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
