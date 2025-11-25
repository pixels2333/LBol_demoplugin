using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class HardworkRabbit : EnemyUnit
	{
		private HardworkRabbit.MoveType Next { get; set; }
		private bool Accuracy
		{
			get
			{
				return base.HasStatusEffect<AccuracyModule>();
			}
		}
		private bool MoonBag { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = HardworkRabbit.MoveType.Attack;
			this.BaseDamage = base.Damage1;
			this.MoonBag = true;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new Func<UnitEventArgs, IEnumerable<BattleAction>>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<InvincibleEternal>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Chat(this, "Chat.HardworkRabbit1".Localize(true), 3f, 0.5f, 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs arg)
		{
			if (this.MoonBag)
			{
				if (base.Battle.HandIsFull)
				{
					yield return new AddCardsToDrawZoneAction(Library.CreateCards<MoonTipsBag>(1, false), DrawZoneTarget.Top, AddCardsType.Normal);
				}
				else
				{
					yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<MoonTipsBag>() });
				}
				this.MoonBag = false;
			}
			yield break;
		}
		private int BaseDamage { get; set; }
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			HardworkRabbit.MoveType next = this.Next;
			if (next != HardworkRabbit.MoveType.Attack)
			{
				if (next != HardworkRabbit.MoveType.Buff)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return new SimpleEnemyMove(Intention.PositiveEffect().WithMoveName(base.GetMove(1)), this.BuffActions());
			}
			else
			{
				yield return new SimpleEnemyMove(Intention.Attack(this.BaseDamage, 2, this.Accuracy).WithMoveName(base.GetMove(0)), this.MoonRabbitMove());
				yield return new SimpleEnemyMove(Intention.Attack(this.BaseDamage * 2, true), this.MoonRabbitAttack());
			}
			yield break;
		}
		private IEnumerable<BattleAction> MoonRabbitMove()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield break;
		}
		private IEnumerable<BattleAction> MoonRabbitAttack()
		{
			List<DamageAction> damageActions = new List<DamageAction>();
			DamageAction damageAction = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)this.BaseDamage, this.Accuracy), base.Gun1, GunType.First);
			damageActions.Add(damageAction);
			yield return damageAction;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			DamageAction damageAction2 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)this.BaseDamage, this.Accuracy), "Instant", GunType.Middle);
			damageActions.Add(damageAction2);
			yield return damageAction2;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			DamageAction damageAction3 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)(this.BaseDamage * 2), true), "Instant", GunType.Last);
			damageActions.Add(damageAction3);
			yield return damageAction3;
			yield return new StatisticalTotalDamageAction(damageActions);
			this.BaseDamage++;
			yield break;
		}
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			if (!base.HasStatusEffect<AccuracyModule>() && base.Battle.Player.HasStatusEffect<Graze>())
			{
				yield return new ApplyStatusEffectAction<AccuracyModule>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<LightingModule>() && base.GameRun.BaseDeck.Count <= 20)
			{
				yield return new ApplyStatusEffectAction<LightingModule>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<PurifyModule>())
			{
				yield return new ApplyStatusEffectAction<PurifyModule>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<LightingModule>())
			{
				yield return new ApplyStatusEffectAction<LightingModule>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<AccuracyModule>())
			{
				yield return new ApplyStatusEffectAction<AccuracyModule>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			yield return PerformAction.Chat(this, "Chat.HardworkRabbit2".Localize(true), 3f, 0f, 1f, true);
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			HardworkRabbit.MoveType moveType;
			switch (base.TurnCounter % 3)
			{
			case 0:
				moveType = HardworkRabbit.MoveType.Attack;
				break;
			case 1:
				moveType = HardworkRabbit.MoveType.Buff;
				break;
			case 2:
				moveType = HardworkRabbit.MoveType.Attack;
				break;
			default:
				moveType = this.Next;
				break;
			}
			this.Next = moveType;
		}
		private enum MoveType
		{
			Attack,
			Buff
		}
	}
}
