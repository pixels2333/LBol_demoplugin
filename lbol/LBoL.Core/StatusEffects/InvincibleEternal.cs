using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x0200009F RID: 159
	[UsedImplicitly]
	public sealed class InvincibleEternal : StatusEffect
	{
		// Token: 0x06000781 RID: 1921 RVA: 0x0001624D File Offset: 0x0001444D
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x00016268 File Offset: 0x00014468
		private void OnDamageTaking(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			int num = damageInfo.Damage.RoundToInt();
			if (num > 1)
			{
				base.NotifyActivating();
				args.DamageInfo = damageInfo.ReduceActualDamageBy(num - 1);
				args.AddModifier(this);
			}
		}

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000783 RID: 1923 RVA: 0x000162AA File Offset: 0x000144AA
		public override string UnitEffectName
		{
			get
			{
				return "InvincibleLoop";
			}
		}
	}
}
