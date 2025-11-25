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
	[UsedImplicitly]
	public sealed class Knife : Card
	{
		protected override string GetBaseDescription()
		{
			if (base.Value2 <= 0)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasExhibit<SakuyaW>() && base.Battle.Player.GetExhibit<SakuyaW>().Active;
			}
		}
		[UsedImplicitly]
		public DamageInfo DropDamage
		{
			get
			{
				return DamageInfo.Attack((float)base.Value1, false);
			}
		}
		public override bool OnDiscardVisual
		{
			get
			{
				return false;
			}
		}
		public override bool OnExileVisual
		{
			get
			{
				return false;
			}
		}
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}
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
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			if (base.Battle.BattleShouldEnd || srcZone != CardZone.Hand)
			{
				return null;
			}
			return this.LeaveHandReactor();
		}
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			if (base.Battle.BattleShouldEnd || srcZone != CardZone.Hand)
			{
				return null;
			}
			return this.LeaveHandReactor();
		}
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (base.Battle.BattleShouldEnd || srcZone != CardZone.Hand || dstZone == CardZone.Hand)
			{
				return null;
			}
			return this.LeaveHandReactor();
		}
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
		private sealed class KnifeAchievementCounter : ICustomCounter
		{
			public CustomCounterResetTiming AutoResetTiming
			{
				get
				{
					return CustomCounterResetTiming.PlayerTurnStart;
				}
			}
			public void Increase(BattleController battle)
			{
				this._counter++;
				if (this._counter >= 10 && battle.GameRun.IsAutoSeed && battle.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					battle.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Knife);
				}
			}
			public void Reset(BattleController battle)
			{
				this._counter = 0;
			}
			private int _counter;
		}
	}
}
