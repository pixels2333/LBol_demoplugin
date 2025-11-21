using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000391 RID: 913
	[UsedImplicitly]
	public sealed class Knife : Card
	{
		// Token: 0x06000CFB RID: 3323 RVA: 0x00018D4F File Offset: 0x00016F4F
		protected override string GetBaseDescription()
		{
			if (base.Value2 <= 0)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000CFC RID: 3324 RVA: 0x00018D67 File Offset: 0x00016F67
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasExhibit<SakuyaW>() && base.Battle.Player.GetExhibit<SakuyaW>().Active;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000CFD RID: 3325 RVA: 0x00018D9A File Offset: 0x00016F9A
		[UsedImplicitly]
		public DamageInfo DropDamage
		{
			get
			{
				return DamageInfo.Attack((float)base.Value1, false);
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000CFE RID: 3326 RVA: 0x00018DA9 File Offset: 0x00016FA9
		public override bool OnDiscardVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000CFF RID: 3327 RVA: 0x00018DAC File Offset: 0x00016FAC
		public override bool OnExileVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x06000D00 RID: 3328 RVA: 0x00018DAF File Offset: 0x00016FAF
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x06000D01 RID: 3329 RVA: 0x00018DB2 File Offset: 0x00016FB2
		protected override int AdditionalValue2
		{
			get
			{
				if (base.Battle == null || !base.Battle.Player.HasStatusEffect<KnifeWithLockedOn>())
				{
					return 0;
				}
				return base.Battle.Player.GetStatusEffect<KnifeWithLockedOn>().Level;
			}
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x00018DE5 File Offset: 0x00016FE5
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			if (base.Battle.BattleShouldEnd || srcZone != CardZone.Hand)
			{
				return null;
			}
			return this.LeaveHandReactor();
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x00018E00 File Offset: 0x00017000
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			if (base.Battle.BattleShouldEnd || srcZone != CardZone.Hand)
			{
				return null;
			}
			return this.LeaveHandReactor();
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x00018E1B File Offset: 0x0001701B
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (base.Battle.BattleShouldEnd || srcZone != CardZone.Hand || dstZone == CardZone.Hand)
			{
				return null;
			}
			return this.LeaveHandReactor();
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x00018E3A File Offset: 0x0001703A
		private IEnumerable<BattleAction> LeaveHandReactor()
		{
			EnemyUnit enemyUnit;
			if ((enemyUnit = Enumerable.FirstOrDefault<EnemyUnit>(base.Battle.EnemyGroup.Alives, (EnemyUnit enemy) => enemy.HasStatusEffect<KnifeTarget>())) == null)
			{
				enemyUnit = base.Battle.EnemyGroup.Alives.MinBy((EnemyUnit unit) => unit.Hp);
			}
			EnemyUnit enemyUnit2 = enemyUnit;
			yield return base.AttackAction(enemyUnit2, this.DropDamage, this.IsUpgraded ? "TSakuyaKnifeAutoB" : "TSakuyaKnifeAuto");
			if (base.Battle.Player.HasStatusEffect<DangerousMagicianSe>())
			{
				int level = base.Battle.Player.GetStatusEffect<DangerousMagicianSe>().Level;
				yield return base.BuffAction<TimeAuraSe>(level, 0, 0, 0, 0f);
			}
			base.Battle.IncreaseCounter<Knife.KnifeAchievementCounter>();
			yield break;
		}

		// Token: 0x06000D06 RID: 3334 RVA: 0x00018E4A File Offset: 0x0001704A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Value2 > 0)
			{
				EnemyUnit selectedEnemy = selector.SelectedEnemy;
				if (selectedEnemy.IsAlive)
				{
					yield return base.DebuffAction<LockedOn>(selectedEnemy, base.Value2, 0, 0, 0, true, 0.2f);
				}
			}
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.Player.HasStatusEffect<DangerousMagicianSe>())
			{
				int level = base.Battle.Player.GetStatusEffect<DangerousMagicianSe>().Level;
				yield return base.BuffAction<TimeAuraSe>(level, 0, 0, 0, 0f);
			}
			base.Battle.IncreaseCounter<Knife.KnifeAchievementCounter>();
			yield break;
		}

		// Token: 0x020008DF RID: 2271
		private sealed class KnifeAchievementCounter : ICustomCounter
		{
			// Token: 0x17000826 RID: 2086
			// (get) Token: 0x06002BE4 RID: 11236 RVA: 0x0006892F File Offset: 0x00066B2F
			public CustomCounterResetTiming AutoResetTiming
			{
				get
				{
					return CustomCounterResetTiming.PlayerTurnStart;
				}
			}

			// Token: 0x06002BE5 RID: 11237 RVA: 0x00068934 File Offset: 0x00066B34
			public void Increase(BattleController battle)
			{
				this._counter++;
				if (this._counter >= 10 && battle.GameRun.IsAutoSeed && battle.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					battle.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Knife);
				}
			}

			// Token: 0x06002BE6 RID: 11238 RVA: 0x0006898A File Offset: 0x00066B8A
			public void Reset(BattleController battle)
			{
				this._counter = 0;
			}

			// Token: 0x04001253 RID: 4691
			private int _counter;
		}
	}
}
