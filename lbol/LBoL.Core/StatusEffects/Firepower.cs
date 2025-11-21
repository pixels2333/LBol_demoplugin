using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000098 RID: 152
	[UsedImplicitly]
	public sealed class Firepower : StatusEffect, IOpposing<FirepowerNegative>
	{
		// Token: 0x0600075D RID: 1885 RVA: 0x00015C00 File Offset: 0x00013E00
		private void CheckAchievement()
		{
			if (base.Level >= 50 && base.Owner is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Firepower);
			}
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x00015C55 File Offset: 0x00013E55
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			this.CheckAchievement();
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x00015C75 File Offset: 0x00013E75
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				this.CheckAchievement();
			}
			return flag;
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x00015C88 File Offset: 0x00013E88
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
				args.AddModifier(this);
			}
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00015CC8 File Offset: 0x00013EC8
		public OpposeResult Oppose(FirepowerNegative other)
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
