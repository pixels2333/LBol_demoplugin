using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000E2 RID: 226
	[UsedImplicitly]
	public sealed class Immune : StatusEffect
	{
		// Token: 0x0600032E RID: 814 RVA: 0x0000887A File Offset: 0x00006A7A
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
		}

		// Token: 0x0600032F RID: 815 RVA: 0x00008894 File Offset: 0x00006A94
		private void OnDamageTaking(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				base.NotifyActivating();
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
				args.AddModifier(this);
			}
		}
	}
}
