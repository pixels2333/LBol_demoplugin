using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x0200002E RID: 46
	[UsedImplicitly]
	public sealed class MomentPowerSe : StatusEffect
	{
		// Token: 0x06000081 RID: 129 RVA: 0x00002DEA File Offset: 0x00000FEA
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnSeAdding));
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00002E09 File Offset: 0x00001009
		private IEnumerable<BattleAction> OnSeAdding(StatusEffectApplyEventArgs args)
		{
			StatusEffect se = args.Effect;
			if (se is TempFirepower)
			{
				base.NotifyActivating();
				int level = base.Level;
				if (level >= se.Level)
				{
					args.CancelBy(this);
					yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(se.Level), default(int?), default(int?), default(int?), 0f, true);
				}
				else
				{
					se.Level -= level;
					yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(level), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			if (se is TempSpirit)
			{
				base.NotifyActivating();
				int level2 = base.Level;
				if (level2 >= se.Level)
				{
					args.CancelBy(this);
					yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(se.Level), default(int?), default(int?), default(int?), 0f, true);
				}
				else
				{
					se.Level -= level2;
					yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(level2), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			yield break;
		}
	}
}
