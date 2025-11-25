using System;
using System.Collections.Generic;
using System.Linq;
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
	public sealed class Long : EnemyUnit
	{
		private Long.MoveType LastAttack { get; set; }
		private Long.MoveType Next { get; set; }
		private Type SpecialReport { get; set; }
		private int LightLevel
		{
			get
			{
				LongLight statusEffect = base.GetStatusEffect<LongLight>();
				if (statusEffect == null)
				{
					return 0;
				}
				return statusEffect.Level;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.LastAttack = (this.Next = Long.MoveType.ShootAddCard);
			this._paperCountDown = 2;
			base.CountDown = 3;
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
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
			base.HandleBattleEvent<DamageEventArgs>(base.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnDamageReceived));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			if (base.GameRun.Difficulty == GameDifficulty.Lunatic)
			{
				yield return new ApplyStatusEffectAction<LongLight>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield return new AddCardsToHandAction(Library.CreateCards<Bribery>(1, false), AddCardsType.Normal);
			yield break;
		}
		private void OnStatusEffectAdded(StatusEffectApplyEventArgs arg)
		{
			if (arg.Effect is LongEscape)
			{
				LongEscape statusEffect = base.GetStatusEffect<LongEscape>();
				if (base.Hp <= statusEffect.Level)
				{
					this.Next = Long.MoveType.Escape;
					if (base.IsInTurn)
					{
						Debug.LogError("LongEscape should not add to Long during her turn.");
						return;
					}
					base.UpdateTurnMoves();
				}
			}
		}
		private void OnDamageReceived(DamageEventArgs args)
		{
			if (base.HasStatusEffect<LongEscape>())
			{
				LongEscape statusEffect = base.GetStatusEffect<LongEscape>();
				if (base.Hp <= statusEffect.Level)
				{
					this.Next = Long.MoveType.Escape;
					if (!base.IsInTurn)
					{
						base.UpdateTurnMoves();
					}
				}
			}
		}
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun3, base.Damage3, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<LongLight>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			if (!Enumerable.Any<Card>(base.Battle.EnumerateAllCardsButExile(), (Card card) => card is Bribery))
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<Bribery>() });
			}
			yield break;
			yield break;
		}
		private IEnumerable<BattleAction> AddPapersAction()
		{
			if (!this._paperChatOnce)
			{
				PlayerUnit player = base.Battle.Player;
				string text = (player.HasHomeName ? player.GetHomeName() : "Chat.LongPlace".Localize(true));
				yield return PerformAction.Chat(this, "Chat.LongPaper".LocalizeFormat(new object[] { text }), 3f, 0f, 1.5f, true);
			}
			yield return PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1);
			int lightLevel = this.LightLevel;
			List<Card> list2;
			if (lightLevel < 3)
			{
				switch (lightLevel)
				{
				case 0:
				{
					List<Card> list = new List<Card>();
					list.Add(Library.CreateCard<HatateNews>());
					list.Add(Library.CreateCard<AyaNews>());
					list.Add(Library.CreateCard<HatateNews>());
					list2 = list;
					break;
				}
				case 1:
				{
					List<Card> list3 = new List<Card>();
					list3.Add(Library.CreateCard<AyaNews>());
					list3.Add(Library.CreateCard<HatateNews>());
					list3.Add(Library.CreateCard<AyaNews>());
					list2 = list3;
					break;
				}
				case 2:
				{
					List<Card> list4 = new List<Card>();
					list4.Add(Library.CreateCard<HatateNews>());
					list4.Add(Library.CreateCard(this.SpecialReport));
					list4.Add(Library.CreateCard<AyaNews>());
					list2 = list4;
					break;
				}
				default:
					list2 = null;
					break;
				}
			}
			else
			{
				List<Card> list5 = new List<Card>();
				list5.Add(Library.CreateCard<AyaNews>());
				list5.Add(Library.CreateCard(this.SpecialReport));
				list5.Add(Library.CreateCard<AyaNews>());
				list2 = list5;
			}
			List<Card> list6 = list2;
			yield return new AddCardsToDiscardAction(list6, AddCardsType.Normal);
			if (!this._paperChatOnce)
			{
				this._paperChatOnce = true;
				yield return PerformAction.Chat(base.Battle.Player, "Chat.LongPaperPlayer".Localize(true), 3f, 0f, 1.5f, true);
			}
			yield break;
		}
		private IEnumerable<BattleAction> EscapeActions()
		{
			yield return PerformAction.Chat(this, "Chat.LongEscape1".Localize(true), 3f, 0f, 3.2f, true);
			yield return PerformAction.Chat(this, "Chat.LongEscape2".Localize(true), 3f, 0f, 3.2f, true);
			yield return new EscapeAction(this);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Long.MoveType.ShootGraze:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", true);
				yield return base.DefendMove(this, null, 0, 0, 3, false, null);
				break;
			case Long.MoveType.ShootAddCard:
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 3, false, "Instant", true);
				int num = this.LightLevel;
				Card[] array;
				if (num < 3)
				{
					switch (num)
					{
					case 0:
						array = new Card[]
						{
							Library.CreateCard<Xingguang>(),
							Library.CreateCard<Xingguang>()
						};
						break;
					case 1:
						array = new Card[]
						{
							Library.CreateCard<Xingguang>(),
							Library.CreateCard<Xuanguang>()
						};
						break;
					case 2:
						array = new Card[]
						{
							Library.CreateCard<Riguang>(),
							Library.CreateCard<Xuanguang>()
						};
						break;
					default:
						array = null;
						break;
					}
				}
				else
				{
					array = new Card[]
					{
						Library.CreateCard<Yueguang>(),
						Library.CreateCard<Xuanguang>(),
						Library.CreateCard<Riguang>()
					};
				}
				IEnumerable<Card> enumerable = array;
				yield return base.AddCardMove(null, enumerable, EnemyUnit.AddCardZone.Discard, null, false);
				break;
			}
			case Long.MoveType.BuffAddCard:
				yield return new SimpleEnemyMove(Intention.AddCard().WithMoveName(base.GetMove(5)), this.AddPapersAction());
				yield return base.PositiveMove(null, typeof(Firepower), new int?(base.Power), default(int?), false, null);
				break;
			case Long.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(new int?(2), 3), new int?(base.Damage3), true), this.SpellActions());
				break;
			case Long.MoveType.Escape:
				yield return new SimpleEnemyMove(Intention.SpellCard(base.GetSpellCardName(default(int?), 4), default(int?), default(int?), false), this.EscapeActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (this.Next != Long.MoveType.Escape)
			{
				int num = base.CountDown;
				if (num == 1 || num == 2)
				{
					yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
				}
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			if (base.HasStatusEffect<LongEscape>())
			{
				LongEscape statusEffect = base.GetStatusEffect<LongEscape>();
				if (base.Hp <= statusEffect.Level)
				{
					this.Next = Long.MoveType.Escape;
					return;
				}
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Long.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			this._paperCountDown--;
			if (this._paperCountDown <= 0)
			{
				this.Next = Long.MoveType.BuffAddCard;
				this._paperCountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			if (this.LastAttack == Long.MoveType.ShootAddCard)
			{
				this.LastAttack = (this.Next = Long.MoveType.ShootGraze);
				return;
			}
			this.LastAttack = (this.Next = Long.MoveType.ShootAddCard);
		}
		private bool _paperChatOnce;
		private int _paperCountDown;
		private enum MoveType
		{
			ShootGraze,
			ShootAddCard,
			BuffAddCard,
			Spell,
			Escape
		}
	}
}
