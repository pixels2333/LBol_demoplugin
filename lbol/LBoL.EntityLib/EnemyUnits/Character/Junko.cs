using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class Junko : EnemyUnit
	{
		private Junko.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<JunkoPurify>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			this._junkoColorLevel = 1;
			Unit player = base.Battle.Player;
			int? num = new int?(1);
			int? num2 = new int?(0);
			yield return new ApplyStatusEffectAction<JunkoColor>(player, num, default(int?), num2, default(int?), 0f, true);
			foreach (BattleAction battleAction in this.StartActions())
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			this.Next = Junko.MoveType.MultiShoot;
			base.CountDown = 2;
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> StartActions()
		{
			yield return PerformAction.Spell(this, "掌上的纯光");
			yield return PerformAction.Animation(this, "spell", 0f, null, 0f, -1);
			yield return PerformAction.Gun(this, base.Battle.Player, "Junko3", 1f);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Chunguang>(base.Count1, false), AddCardsType.OneByOne);
			yield break;
		}
		private IEnumerable<BattleAction> DefendFirst()
		{
			yield return new EnemyMoveAction(this, base.GetSpellCardName(default(int?), 3), true);
			yield return new CastBlockShieldAction(this, 0, base.Defend, BlockShieldType.Normal, true);
			yield break;
		}
		private IEnumerable<BattleAction> NegativeSecond()
		{
			yield return PerformAction.Gun(this, base.Battle.Player, "Junko3B", 0.5f);
			int junkoColorLevel = this._junkoColorLevel;
			if (junkoColorLevel < 3)
			{
				if (junkoColorLevel != 1)
				{
					if (junkoColorLevel == 2)
					{
						Type typeFromHandle = typeof(Weak);
						Unit player = base.Battle.Player;
						int? num = new int?(2);
						yield return new ApplyStatusEffectAction(typeFromHandle, player, default(int?), num, default(int?), default(int?), 0f, false);
						yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
						Type typeFromHandle2 = typeof(Fragil);
						Unit player2 = base.Battle.Player;
						num = new int?(2);
						yield return new ApplyStatusEffectAction(typeFromHandle2, player2, default(int?), num, default(int?), default(int?), 0.3f, false);
						yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
					}
				}
				else
				{
					Type typeFromHandle3 = typeof(Weak);
					Unit player3 = base.Battle.Player;
					int? num = new int?(2);
					yield return new ApplyStatusEffectAction(typeFromHandle3, player3, default(int?), num, default(int?), default(int?), 0f, false);
					yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
				}
			}
			else
			{
				Type typeFromHandle4 = typeof(Weak);
				Unit player4 = base.Battle.Player;
				int? num = new int?(2);
				yield return new ApplyStatusEffectAction(typeFromHandle4, player4, default(int?), num, default(int?), default(int?), 0f, false);
				yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
				Type typeFromHandle5 = typeof(Fragil);
				Unit player5 = base.Battle.Player;
				num = new int?(2);
				yield return new ApplyStatusEffectAction(typeFromHandle5, player5, default(int?), num, default(int?), default(int?), 0.3f, false);
				yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
				Type typeFromHandle6 = typeof(Vulnerable);
				Unit player6 = base.Battle.Player;
				num = new int?(2);
				yield return new ApplyStatusEffectAction(typeFromHandle6, player6, default(int?), num, default(int?), default(int?), 0.3f, false);
				yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
			}
			yield break;
		}
		private IEnumerable<BattleAction> LilyActions()
		{
			yield return new EnemyMoveAction(this, base.GetSpellCardName(default(int?), 2), true);
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun3, base.Damage3, 2, true, "InstantSpeed.3"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<JunkoLily>(this, new int?(base.Count2), default(int?), default(int?), default(int?), 0f, true);
			base.CountDown = 4;
			this._spelled = true;
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Junko.MoveType.Start:
				yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(default(int?), 0), default(int?), default(int?), false), this.StartActions());
				break;
			case Junko.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetSpellCardName(default(int?), 1), base.Gun1, base.Damage1, 6, false, "Instant", true);
				break;
			case Junko.MoveType.Lunatic:
				yield return base.AttackMove(base.GetSpellCardName(default(int?), 4), base.Gun2, base.Damage2, 3, false, "Instant", true);
				yield return base.AddCardMove(null, Library.CreateCards<LBoL.EntityLib.Cards.Enemy.Lunatic>(2, false), EnemyUnit.AddCardZone.Hand, null, false);
				break;
			case Junko.MoveType.Defend:
				yield return new SimpleEnemyMove(Intention.Defend().WithMoveName(base.GetSpellCardName(default(int?), 3)), this.DefendFirst());
				yield return base.ClearMove(0.3f);
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.NegativeSecond());
				break;
			case Junko.MoveType.Lily:
				yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(default(int?), 2), new int?(base.Damage3), new int?(2), true), this.LilyActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (base.CountDown == 1)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			num = base.CountDown;
			Junko.MoveType moveType;
			if (num > 0)
			{
				if (num != 1)
				{
					if (num == 2)
					{
						if (this._spelled)
						{
							moveType = Junko.MoveType.Defend;
							goto IL_003B;
						}
					}
					moveType = Junko.MoveType.MultiShoot;
				}
				else
				{
					moveType = Junko.MoveType.Lunatic;
				}
			}
			else
			{
				moveType = Junko.MoveType.Lily;
			}
			IL_003B:
			this.Next = moveType;
		}
		public IEnumerable<BattleAction> JunkoColorActions(int level)
		{
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?((level - this._junkoColorLevel) * base.Power), default(int?), default(int?), default(int?), 0f, true);
			this._junkoColorLevel = level;
			yield break;
		}
		private bool _spelled;
		private int _junkoColorLevel;
		private enum MoveType
		{
			Start,
			MultiShoot,
			Lunatic,
			Defend,
			Lily
		}
	}
}
