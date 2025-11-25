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
	[UsedImplicitly]
	public sealed class FrostArmor : StatusEffect
	{
		private void CheckAchievement()
		{
			if (base.Level >= 99 && base.Owner is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.FrostArmor);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			this.React(new CastBlockShieldAction(base.Owner, base.Level, 0, BlockShieldType.Direct, false));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
			this.CheckAchievement();
		}
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
		public override string UnitEffectName
		{
			get
			{
				return "Hanbinjia";
			}
		}
	}
}
