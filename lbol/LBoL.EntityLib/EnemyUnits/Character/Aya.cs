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
	[UsedImplicitly]
	public sealed class Aya : EnemyUnit
	{
		private Aya.MoveType Next { get; set; }
		private int AyaNewsHappenCount { get; set; }
		private int AyaNewsCountDown { get; set; }
		private int ShootCountDown { get; set; }
		private Type SpecialReport { get; set; }
		private bool AyaIsDamaged { get; set; }
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
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.Aya1".Localize(true), 3.5f, 0.5f, 0f, true);
			yield return new ApplyStatusEffectAction<WindGirl>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs arg)
		{
			if (!this.AyaIsDamaged)
			{
				yield return PerformAction.Chat(this, "Chat.Aya2".Localize(true), 3f, 0f, 0f, true);
				this.AyaIsDamaged = true;
			}
			yield break;
		}
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
		private enum MoveType
		{
			ShootAccuracy,
			DoubleShoot,
			AyaNews
		}
	}
}
