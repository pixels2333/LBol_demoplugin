using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000DF RID: 223
	[UsedImplicitly]
	public sealed class FrostArmor : StatusEffect
	{
		// Token: 0x06000320 RID: 800 RVA: 0x00008658 File Offset: 0x00006858
		private void CheckAchievement()
		{
			if (base.Level >= 99 && base.Owner is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.FrostArmor);
			}
		}

		// Token: 0x06000321 RID: 801 RVA: 0x000086B0 File Offset: 0x000068B0
		protected override void OnAdded(Unit unit)
		{
			this.React(new CastBlockShieldAction(base.Owner, base.Level, 0, BlockShieldType.Direct, false));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
			this.CheckAchievement();
		}

		// Token: 0x06000322 RID: 802 RVA: 0x0000871C File Offset: 0x0000691C
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Level, 0, BlockShieldType.Direct, false);
			int num = base.Level - 1;
			base.Level = num;
			if (base.Level <= 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000872C File Offset: 0x0000692C
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f)
			{
				int num = base.Level - 1;
				base.Level = num;
				if (base.Level <= 0)
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00008743 File Offset: 0x00006943
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				this.CheckAchievement();
				this.React(new CastBlockShieldAction(base.Owner, other.Level, 0, BlockShieldType.Direct, false));
			}
			return flag;
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000325 RID: 805 RVA: 0x00008774 File Offset: 0x00006974
		public override string UnitEffectName
		{
			get
			{
				return "Hanbinjia";
			}
		}
	}
}
