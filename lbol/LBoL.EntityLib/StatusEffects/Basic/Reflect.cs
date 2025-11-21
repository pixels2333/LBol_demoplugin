using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000F5 RID: 245
	[UsedImplicitly]
	public sealed class Reflect : StatusEffect
	{
		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600036C RID: 876 RVA: 0x00008E72 File Offset: 0x00007072
		// (set) Token: 0x0600036D RID: 877 RVA: 0x00008E7A File Offset: 0x0000707A
		public string Gun { get; set; } = "Reflect";

		// Token: 0x0600036E RID: 878 RVA: 0x00008E83 File Offset: 0x00007083
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x0600036F RID: 879 RVA: 0x00008EA2 File Offset: 0x000070A2
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.Source != base.Owner && args.Source.IsAlive && args.DamageInfo.DamageType == DamageType.Attack)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, args.Source, DamageInfo.Reaction((float)base.Level, false), this.Gun, GunType.Single);
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
