using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x0200009D RID: 157
	[UsedImplicitly]
	public sealed class GuangxueMicai : StatusEffect
	{
		// Token: 0x06000779 RID: 1913 RVA: 0x00016173 File Offset: 0x00014373
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00016190 File Offset: 0x00014390
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				damageInfo.Damage = damageInfo.Amount * 0.5f;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x0600077B RID: 1915 RVA: 0x000161D0 File Offset: 0x000143D0
		public override string UnitEffectName
		{
			get
			{
				return "GuangxueMicaiLoop";
			}
		}
	}
}
