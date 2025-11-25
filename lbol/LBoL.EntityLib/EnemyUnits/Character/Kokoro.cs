using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class Kokoro : EnemyUnit<IKokoroView>
	{
		private Kokoro.MoveType Next { get; set; }
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(3), 4);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Kokoro.MoveType.Nu;
			base.CountDown = 4;
			this._summonType = typeof(MaskBlue);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			IKokoroView view = base.View;
			if (view != null)
			{
				view.SetEffect(SkirtColor.Original);
			}
			yield return new ApplyStatusEffectAction<KokoroDarkPower>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Chat(this, "Chat.Kokoro1".Localize(true), 3f, 0f, 0f, true);
			yield break;
		}
		protected override IEnumerable<BattleAction> AttackActions(string move, string gunName, int damage, int times = 1, bool isAccuracy = false, string followGunName = "Instant")
		{
			SkirtColor skirtColor;
			switch (this.Next)
			{
			case Kokoro.MoveType.Xi:
				skirtColor = SkirtColor.Green;
				break;
			case Kokoro.MoveType.Nu:
				skirtColor = SkirtColor.Red;
				break;
			case Kokoro.MoveType.You:
				skirtColor = SkirtColor.Blue;
				break;
			case Kokoro.MoveType.Renzhen:
				skirtColor = SkirtColor.Yellow;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			SkirtColor skirtColor2 = skirtColor;
			IKokoroView view = base.View;
			yield return new WaitForCoroutineAction((view != null) ? view.SwitchToFace(skirtColor2) : null);
			foreach (BattleAction battleAction in base.AttackActions(move, gunName, damage, times, isAccuracy, followGunName))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Kokoro.MoveType.Xi:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 3, false, "Instant", true);
				yield return new SimpleEnemyMove(Intention.Spawn(), this.XiActions());
				break;
			case Kokoro.MoveType.Nu:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2 + base.EnemyBattleRng.NextInt(0, 2), 1, false, "Instant", true);
				yield return base.AddCardMove(null, Library.CreateCards<Xuanguang>((base.Difficulty == GameDifficulty.Lunatic) ? 2 : 1, false), EnemyUnit.AddCardZone.Discard, null, false);
				yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.NuActions());
				break;
			case Kokoro.MoveType.You:
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage2 + base.EnemyBattleRng.NextInt(0, 2), 1, false, "Instant", true);
				yield return base.DefendMove(this, null, base.Defend, 0, 0, false, null);
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.YouActions());
				break;
			case Kokoro.MoveType.Renzhen:
			{
				KokoroDarkPower statusEffect = base.GetStatusEffect<KokoroDarkPower>();
				if (statusEffect != null)
				{
					this._darkPowerCount = statusEffect.Count;
					statusEffect.NotifyActivating();
					statusEffect.Count = 0;
				}
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCard, new int?(base.Damage4), true), this.AttackActions(this.SpellCard, base.Gun4, base.Damage4, 1, true, "Instant"));
				yield return new SimpleEnemyMove(Intention.KokoroDark(this.DarkPowerDamage, this._darkPowerCount), this.RenzhenActions());
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
		private IEnumerable<BattleAction> XiActions()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ClearOld();
			yield return new ApplyStatusEffectAction<KokoroXi>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			if (Enumerable.Count<EnemyUnit>(base.AllAliveEnemies) <= 3)
			{
				yield return PerformAction.Animation(this, "shoot2", 0f, null, 0f, -1);
				int num = -1;
				for (int i = 0; i < 3; i++)
				{
					this._summonRootIndex++;
					this._summonRootIndex %= 3;
					if (!base.Battle.IsAnyoneInRootIndex(this._summonRootIndex))
					{
						num = this._summonRootIndex;
						break;
					}
				}
				if (num < 0)
				{
					throw new InvalidOperationException("Kokoro trying to summon when there is no place. (3 Mask alive.)");
				}
				yield return new SpawnEnemyAction(this, this._summonType, num, 0f, 0.3f, true);
				this._summonType = this._maskTypes.Without(this._summonType).Sample(base.EnemyMoveRng);
			}
			yield break;
		}
		private IEnumerable<BattleAction> NuActions()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ClearOld();
			yield return new ApplyStatusEffectAction<KokoroNu>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				yield return new ApplyStatusEffectAction<Firepower>(enemyUnit, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> YouActions()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ClearOld();
			int? num = new int?(base.Power);
			int? num2 = default(int?);
			int? num3 = num2;
			num2 = default(int?);
			int? num4 = num2;
			num2 = default(int?);
			yield return new ApplyStatusEffectAction<KokoroYou>(this, num, num3, num4, num2, 0f, true);
			Unit player = base.Battle.Player;
			num2 = new int?(base.Power);
			yield return new ApplyStatusEffectAction<Weak>(player, default(int?), num2, default(int?), default(int?), 0f, false);
			yield break;
		}
		private int DarkPowerDamage
		{
			get
			{
				return this._darkPowerCount * base.Count1;
			}
		}
		private IEnumerable<BattleAction> RenzhenActions()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ClearOld();
			yield return new ApplyStatusEffectAction<KokoroRenzhen>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			if (this._darkPowerCount == 0)
			{
				yield break;
			}
			DamageAction damageAction = new DamageAction(this, base.Battle.Player, DamageInfo.Reaction((float)this.DarkPowerDamage, false), "Instant", GunType.Single);
			yield return damageAction;
			yield return new StatisticalTotalDamageAction(new DamageAction[] { damageAction });
			this._darkPowerCount = 0;
			yield break;
		}
		private BattleAction ClearOld()
		{
			KokoroQing kokoroQing = Enumerable.FirstOrDefault<KokoroQing>(Enumerable.OfType<KokoroQing>(base.StatusEffects));
			if (kokoroQing == null)
			{
				return null;
			}
			return new RemoveStatusEffectAction(kokoroQing, true, 0.1f);
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Kokoro.MoveType.Renzhen;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			this.Next = this._pool.Without(this.Next).Sample(base.EnemyMoveRng);
		}
		private readonly RepeatableRandomPool<Type> _maskTypes = new RepeatableRandomPool<Type>
		{
			{
				typeof(MaskRed),
				1f
			},
			{
				typeof(MaskGreen),
				1f
			},
			{
				typeof(MaskBlue),
				1f
			}
		};
		private Type _summonType;
		private int _summonRootIndex;
		private int _darkPowerCount;
		private readonly RepeatableRandomPool<Kokoro.MoveType> _pool = new RepeatableRandomPool<Kokoro.MoveType>
		{
			{
				Kokoro.MoveType.Xi,
				1.2f
			},
			{
				Kokoro.MoveType.Nu,
				1f
			},
			{
				Kokoro.MoveType.You,
				0.8f
			}
		};
		private enum MoveType
		{
			Xi,
			Nu,
			You,
			Renzhen
		}
	}
}
