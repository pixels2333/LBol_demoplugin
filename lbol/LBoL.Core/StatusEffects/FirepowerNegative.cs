using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000099 RID: 153
	[UsedImplicitly]
	public sealed class FirepowerNegative : StatusEffect, IOpposing<Firepower>
	{
		// Token: 0x06000763 RID: 1891 RVA: 0x00015D24 File Offset: 0x00013F24
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00015D40 File Offset: 0x00013F40
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.ReduceBy(base.Level);
				args.AddModifier(this);
			}
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00015D80 File Offset: 0x00013F80
		public OpposeResult Oppose(Firepower other)
		{
			if (base.Level < other.Level)
			{
				other.Level -= base.Level;
				return OpposeResult.KeepOther;
			}
			if (base.Level == other.Level)
			{
				return OpposeResult.Neutralize;
			}
			base.Level -= other.Level;
			return OpposeResult.KeepSelf;
		}
	}
}
