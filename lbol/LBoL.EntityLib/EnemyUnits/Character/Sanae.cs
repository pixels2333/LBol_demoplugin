using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class Sanae : EnemyUnit
	{
		private Sanae.MoveType Last { get; set; }
		private Sanae.MoveType Next { get; set; }
		private string MultiShootMove
		{
			get
			{
				return base.GetSpellCardName(new int?(0), 1);
			}
		}
		private string ShootAccuracyMove
		{
			get
			{
				return base.GetSpellCardName(new int?(2), 3);
			}
		}
		private string HealMove
		{
			get
			{
				return base.GetSpellCardName(new int?(4), 5);
			}
		}
		private string Spell
		{
			get
			{
				return base.GetSpellCardName(new int?(6), 7);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Sanae.MoveType.Spell;
			this.Last = Sanae.MoveType.ShootAccuracy;
			this.HealCountDown = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Curiosity>(this, new int?((base.Difficulty == GameDifficulty.Lunatic) ? 2 : 1), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(3), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Spell(this, "灵力掠夺者");
			yield return PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<SpiritNegative>(base.Battle.Player, new int?(2), default(int?), default(int?), default(int?), 0.3f, true);
			yield return new ApplyStatusEffectAction<Spirit>(this, new int?(2), default(int?), default(int?), default(int?), 0.3f, true);
			yield return new CastBlockShieldAction(this, base.Defend, base.Defend, BlockShieldType.Normal, false);
			base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
			yield break;
		}
		private IEnumerable<BattleAction> HealActions()
		{
			yield return new EnemyMoveAction(this, this.HealMove, true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new HealAction(this, this, base.Count1, HealType.Normal, 0.2f);
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(2), default(int?), default(int?), default(int?), 0f, true);
			this.HealCountDown = base.EnemyMoveRng.NextInt(4, 5);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Sanae.MoveType.MultiShoot:
			{
				yield return base.AttackMove(this.MultiShootMove, base.Gun1, base.Damage1, 4, false, "InstantSpeed.3", true);
				string text = null;
				Type typeFromHandle = typeof(Weak);
				int? num = new int?((base.Difficulty == GameDifficulty.Lunatic) ? 3 : 2);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, true, false, null);
				break;
			}
			case Sanae.MoveType.ShootAccuracy:
				yield return base.AttackMove(this.ShootAccuracyMove, base.Gun2, base.Damage2, 1, true, "Instant", true);
				yield return base.AddCardMove(null, typeof(Yueguang), (base.Difficulty == GameDifficulty.Lunatic) ? 3 : 2, EnemyUnit.AddCardZone.Discard, null, false);
				break;
			case Sanae.MoveType.Heal:
				yield return new SimpleEnemyMove(Intention.Heal().WithMoveName(this.HealMove), this.HealActions());
				yield return base.ClearMove(0f);
				break;
			case Sanae.MoveType.Spell:
			{
				string spell = this.Spell;
				int? num = default(int?);
				int? num2 = num;
				num = default(int?);
				yield return new SimpleEnemyMove(Intention.SpellCard(spell, num2, num, false), this.SpellActions());
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			int countDown = base.CountDown;
			if (countDown == 1 || countDown == 2)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}
		private int HealCountDown { get; set; }
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Sanae.MoveType.Spell;
				return;
			}
			if (base.Hp <= base.MaxHp / 2)
			{
				num = this.HealCountDown - 1;
				this.HealCountDown = num;
				if (this.HealCountDown <= 0)
				{
					this.Next = Sanae.MoveType.Heal;
					return;
				}
			}
			this.Next = ((this.Last == Sanae.MoveType.MultiShoot) ? Sanae.MoveType.ShootAccuracy : Sanae.MoveType.MultiShoot);
			this.Last = this.Next;
		}
		private enum MoveType
		{
			MultiShoot,
			ShootAccuracy,
			Heal,
			Spell
		}
	}
}
