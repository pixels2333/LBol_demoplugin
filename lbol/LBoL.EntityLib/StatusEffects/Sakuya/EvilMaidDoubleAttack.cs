using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000015 RID: 21
	[UsedImplicitly]
	public sealed class EvilMaidDoubleAttack : StatusEffect
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000029 RID: 41 RVA: 0x000024B6 File Offset: 0x000006B6
		// (set) Token: 0x0600002A RID: 42 RVA: 0x000024BE File Offset: 0x000006BE
		[UsedImplicitly]
		public int DamageRate { get; set; } = 2;

		// Token: 0x0600002B RID: 43 RVA: 0x000024C7 File Offset: 0x000006C7
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000024E4 File Offset: 0x000006E4
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.MultiplyBy(this.DamageRate);
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
				}
			}
		}
	}
}
