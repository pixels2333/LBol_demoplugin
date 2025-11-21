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
	// Token: 0x0200023D RID: 573
	[UsedImplicitly]
	public sealed class Kokoro : EnemyUnit<IKokoroView>
	{
		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060008C7 RID: 2247 RVA: 0x00012FB9 File Offset: 0x000111B9
		// (set) Token: 0x060008C8 RID: 2248 RVA: 0x00012FC1 File Offset: 0x000111C1
		private Kokoro.MoveType Next { get; set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x00012FCA File Offset: 0x000111CA
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(3), 4);
			}
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x00012FD9 File Offset: 0x000111D9
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Kokoro.MoveType.Nu;
			base.CountDown = 4;
			this._summonType = typeof(MaskBlue);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x00013016 File Offset: 0x00011216
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

		// Token: 0x060008CC RID: 2252 RVA: 0x00013026 File Offset: 0x00011226
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

		// Token: 0x060008CD RID: 2253 RVA: 0x00013063 File Offset: 0x00011263
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

		// Token: 0x060008CE RID: 2254 RVA: 0x00013073 File Offset: 0x00011273
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

		// Token: 0x060008CF RID: 2255 RVA: 0x00013083 File Offset: 0x00011283
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

		// Token: 0x060008D0 RID: 2256 RVA: 0x00013093 File Offset: 0x00011293
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

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060008D1 RID: 2257 RVA: 0x000130A3 File Offset: 0x000112A3
		private int DarkPowerDamage
		{
			get
			{
				return this._darkPowerCount * base.Count1;
			}
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x000130B2 File Offset: 0x000112B2
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

		// Token: 0x060008D3 RID: 2259 RVA: 0x000130C4 File Offset: 0x000112C4
		private BattleAction ClearOld()
		{
			KokoroQing kokoroQing = Enumerable.FirstOrDefault<KokoroQing>(Enumerable.OfType<KokoroQing>(base.StatusEffects));
			if (kokoroQing == null)
			{
				return null;
			}
			return new RemoveStatusEffectAction(kokoroQing, true, 0.1f);
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x000130F4 File Offset: 0x000112F4
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

		// Token: 0x040000AE RID: 174
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

		// Token: 0x040000AF RID: 175
		private Type _summonType;

		// Token: 0x040000B0 RID: 176
		private int _summonRootIndex;

		// Token: 0x040000B1 RID: 177
		private int _darkPowerCount;

		// Token: 0x040000B2 RID: 178
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

		// Token: 0x0200073B RID: 1851
		private enum MoveType
		{
			// Token: 0x04000A88 RID: 2696
			Xi,
			// Token: 0x04000A89 RID: 2697
			Nu,
			// Token: 0x04000A8A RID: 2698
			You,
			// Token: 0x04000A8B RID: 2699
			Renzhen
		}
	}
}
