using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000023 RID: 35
	public sealed class TimeAuraSe : StatusEffect
	{
		// Token: 0x06000054 RID: 84 RVA: 0x000028CC File Offset: 0x00000ACC
		private void CheckAchievement()
		{
			if (base.Level >= 200 && base.Owner is PlayerUnit && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.TimeAura);
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00002924 File Offset: 0x00000B24
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
			this.CheckAchievement();
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002949 File Offset: 0x00000B49
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				this.CheckAchievement();
			}
			return flag;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x0000295B File Offset: 0x00000B5B
		private IEnumerable<BattleAction> OnOwnerTurnEnding(UnitEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.EnemyGroup.Alives != null)
			{
				base.NotifyActivating();
				int aliveCount = Enumerable.Count<EnemyUnit>(base.Battle.EnemyGroup.Alives);
				yield return PerformAction.Sfx("SakuyaSpellMain", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "TimePulse", 0.5f, "SakuyaSpellMain", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup.Alives)
				{
					yield return PerformAction.Effect(enemyUnit, "TimePulseHit", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				}
				IEnumerator<EnemyUnit> enumerator = null;
				yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)base.Level, false), "Instant.4", GunType.Single);
				if (aliveCount < base.Level)
				{
					base.Level -= aliveCount;
				}
				else
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
			yield break;
		}
	}
}
