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
using UnityEngine;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000236 RID: 566
	[UsedImplicitly]
	public sealed class Aya : EnemyUnit
	{
		// Token: 0x170000DD RID: 221
		// (get) Token: 0x0600087A RID: 2170 RVA: 0x000127CC File Offset: 0x000109CC
		// (set) Token: 0x0600087B RID: 2171 RVA: 0x000127D4 File Offset: 0x000109D4
		private Aya.MoveType Next { get; set; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x0600087C RID: 2172 RVA: 0x000127DD File Offset: 0x000109DD
		// (set) Token: 0x0600087D RID: 2173 RVA: 0x000127E5 File Offset: 0x000109E5
		private int AyaNewsHappenCount { get; set; }

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x0600087E RID: 2174 RVA: 0x000127EE File Offset: 0x000109EE
		// (set) Token: 0x0600087F RID: 2175 RVA: 0x000127F6 File Offset: 0x000109F6
		private int AyaNewsCountDown { get; set; }

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000880 RID: 2176 RVA: 0x000127FF File Offset: 0x000109FF
		// (set) Token: 0x06000881 RID: 2177 RVA: 0x00012807 File Offset: 0x00010A07
		private int ShootCountDown { get; set; }

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000882 RID: 2178 RVA: 0x00012810 File Offset: 0x00010A10
		// (set) Token: 0x06000883 RID: 2179 RVA: 0x00012818 File Offset: 0x00010A18
		private Type SpecialReport { get; set; }

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000884 RID: 2180 RVA: 0x00012821 File Offset: 0x00010A21
		// (set) Token: 0x06000885 RID: 2181 RVA: 0x00012829 File Offset: 0x00010A29
		private bool AyaIsDamaged { get; set; }

		// Token: 0x06000886 RID: 2182 RVA: 0x00012834 File Offset: 0x00010A34
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Aya.MoveType.AyaNews;
			this.AyaNewsHappenCount = 0;
			this.AyaNewsCountDown = 3;
			this.ShootCountDown = 2;
			string id = battle.Player.Id;
			if (!(id == "Reimu"))
			{
				if (!(id == "Marisa"))
				{
					if (!(id == "Sakuya"))
					{
						if (!(id == "Cirno"))
						{
							if (!(id == "Koishi"))
							{
								if (!(id == "Alice"))
								{
									this.SpecialReport = typeof(AyaNewsReimuSp);
									Debug.LogWarning("This character needs a new special report in Aya battle.");
								}
								else
								{
									this.SpecialReport = typeof(AyaNewsAliceSp);
								}
							}
							else
							{
								this.SpecialReport = typeof(AyaNewsKoishiSp);
							}
						}
						else
						{
							this.SpecialReport = typeof(AyaNewsCirnoSp);
						}
					}
					else
					{
						this.SpecialReport = typeof(AyaNewsSakuyaSp);
					}
				}
				else
				{
					this.SpecialReport = typeof(AyaNewsMarisaSp);
				}
			}
			else
			{
				this.SpecialReport = typeof(AyaNewsReimuSp);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DamageEventArgs>(base.DamageReceived, new Func<DamageEventArgs, IEnumerable<BattleAction>>(this.OnDamageReceived));
			this.AyaIsDamaged = false;
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0001297B File Offset: 0x00010B7B
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.Aya1".Localize(true), 3.5f, 0.5f, 0f, true);
			yield return new ApplyStatusEffectAction<WindGirl>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x0001298B File Offset: 0x00010B8B
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs arg)
		{
			if (!this.AyaIsDamaged)
			{
				yield return PerformAction.Chat(this, "Chat.Aya2".Localize(true), 3f, 0f, 0f, true);
				this.AyaIsDamaged = true;
			}
			yield break;
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x0001299B File Offset: 0x00010B9B
		private IEnumerable<BattleAction> AyaNewsActions()
		{
			int num = this.AyaNewsHappenCount + 1;
			this.AyaNewsHappenCount = num;
			this.AyaNewsCountDown = 3;
			if (this.AyaNewsHappenCount == 3)
			{
				string text = "Chat.Aya3".LocalizeFormat(new object[] { base.Battle.Player.GetName() });
				yield return PerformAction.Chat(this, text, 3f, 1f, 0f, true);
				yield return new ApplyStatusEffectAction<InterviewDone>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard(typeof(AyaNews)));
			list.Add(base.HasStatusEffect<InterviewDone>() ? Library.CreateCard(this.SpecialReport) : Library.CreateCard(typeof(AyaNews)));
			list.Add(Library.CreateCard(typeof(AyaNews)));
			List<Card> cards = list;
			yield return new EnemyMoveAction(this, base.GetMove(3), true);
			yield return PerformAction.Animation(this, "skill1", 1f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(cards, AddCardsType.Normal);
			yield break;
		}

		// Token: 0x0600088A RID: 2186 RVA: 0x000129AB File Offset: 0x00010BAB
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Aya.MoveType.ShootAccuracy:
				yield return base.AttackMove(base.GetMove(0), base.Gun2, base.Damage2 + base.EnemyBattleRng.NextInt(0, 2), 1, true, "Instant", true);
				break;
			case Aya.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(1), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, 1), 2, false, "Instant", true);
				break;
			case Aya.MoveType.AyaNews:
				yield return new SimpleEnemyMove(Intention.AddCard().WithMoveName(base.GetMove(2)), this.AyaNewsActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x0600088B RID: 2187 RVA: 0x000129BC File Offset: 0x00010BBC
		protected override void UpdateMoveCounters()
		{
			int num = this.AyaNewsCountDown - 1;
			this.AyaNewsCountDown = num;
			if (this.AyaNewsCountDown <= 0)
			{
				this.Next = Aya.MoveType.AyaNews;
				return;
			}
			if (!base.HasStatusEffect<InterviewDone>() && !this.AyaIsDamaged)
			{
				this.Next = Aya.MoveType.AyaNews;
				return;
			}
			num = this.ShootCountDown - 1;
			this.ShootCountDown = num;
			if (this.ShootCountDown <= 0)
			{
				this.Next = Aya.MoveType.ShootAccuracy;
				this.ShootCountDown = base.EnemyMoveRng.NextInt(2, 3);
				return;
			}
			this.Next = Aya.MoveType.DoubleShoot;
		}

		// Token: 0x0200071A RID: 1818
		private enum MoveType
		{
			// Token: 0x040009F7 RID: 2551
			ShootAccuracy,
			// Token: 0x040009F8 RID: 2552
			DoubleShoot,
			// Token: 0x040009F9 RID: 2553
			AyaNews
		}
	}
}
