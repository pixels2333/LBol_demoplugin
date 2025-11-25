using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.EntityLib.StatusEffects.Others;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	public sealed class Potion : Card
	{
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				PlayerUnit player = base.Battle.Player;
				PotionBaseDamageSe potionBaseDamageSe = ((player != null) ? player.GetStatusEffect<PotionBaseDamageSe>() : null);
				if (potionBaseDamageSe == null)
				{
					return 0;
				}
				return potionBaseDamageSe.Level;
			}
		}
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor(true);
		}
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor(true);
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(new LazySequencedReactor(this.AddToHandReactor));
			}
		}
		private IEnumerable<BattleAction> AddToHandReactor()
		{
			base.NotifyActivating();
			List<DamageAction> list = new List<DamageAction>();
			foreach (BattleAction action in this.EnterHandReactor(true))
			{
				yield return action;
				DamageAction damageAction = action as DamageAction;
				if (damageAction != null)
				{
					list.Add(damageAction);
				}
				action = null;
			}
			IEnumerator<BattleAction> enumerator = null;
			if (list.NotEmpty<DamageAction>())
			{
				yield return new StatisticalTotalDamageAction(list);
			}
			base.Battle.IncreaseCounter<Potion.PotionAchievementCounter>();
			yield break;
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			return this.EnterHandReactor(false);
		}
		private IEnumerable<BattleAction> EnterHandReactor(bool ensureInHand = true)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (ensureInHand && base.Zone != CardZone.Hand)
			{
				Debug.LogWarning(this.Name + " is not in hand.");
				yield break;
			}
			yield return new GainManaAction(base.Mana);
			int num = 1;
			SplitPotionSe statusEffect = base.Battle.Player.GetStatusEffect<SplitPotionSe>();
			if (statusEffect != null)
			{
				num += statusEffect.Level;
			}
			EnemyUnit[] array = base.Battle.EnemyGroup.Alives.SampleManyOrAll(num, base.GameRun.BattleRng);
			foreach (EnemyUnit target in array)
			{
				if (target != null && target.IsAlive)
				{
					yield return base.AttackAction(target);
					PoisonPotionSe statusEffect2 = base.Battle.Player.GetStatusEffect<PoisonPotionSe>();
					if (statusEffect2 != null && target.IsAlive)
					{
						statusEffect2.NotifyActivating();
						yield return base.DebuffAction<Poison>(target, statusEffect2.Level, 0, 0, 0, true, 0.1f);
					}
					ColdPotionSe statusEffect3 = base.Battle.Player.GetStatusEffect<ColdPotionSe>();
					if (statusEffect3 != null && target.IsAlive)
					{
						statusEffect3.NotifyActivating();
						yield return base.DebuffAction<Cold>(target, 0, 0, 0, 0, true, 0.1f);
					}
				}
				target = null;
			}
			EnemyUnit[] array2 = null;
			yield return new ExileCardAction(this);
			ChargingPotionSe statusEffect4 = base.Battle.Player.GetStatusEffect<ChargingPotionSe>();
			if (statusEffect4 != null)
			{
				statusEffect4.NotifyActivating();
				yield return base.BuffAction<Charging>(statusEffect4.Level, 0, 0, 0, 0.2f);
			}
			PotionDefenseSe statusEffect5 = base.Battle.Player.GetStatusEffect<PotionDefenseSe>();
			if (statusEffect5 != null)
			{
				statusEffect5.NotifyActivating();
				yield return base.DefenseAction(0, statusEffect5.Level, BlockShieldType.Direct, false);
			}
			base.Battle.IncreaseCounter<Potion.PotionAchievementCounter>();
			yield break;
		}
		private sealed class PotionAchievementCounter : ICustomCounter
		{
			public CustomCounterResetTiming AutoResetTiming
			{
				get
				{
					return CustomCounterResetTiming.PlayerActionStart;
				}
			}
			public void Increase(BattleController battle)
			{
				this._counter++;
				if (this._counter >= 8 && battle.GameRun.IsAutoSeed && battle.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					battle.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Potion);
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
