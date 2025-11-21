using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000A4 RID: 164
	[UsedImplicitly]
	public sealed class FlatPeach : StatusEffect
	{
		// Token: 0x0600024C RID: 588 RVA: 0x00006BD6 File Offset: 0x00004DD6
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x0600024D RID: 589 RVA: 0x00006BF5 File Offset: 0x00004DF5
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Owner, base.Owner, base.Level, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
