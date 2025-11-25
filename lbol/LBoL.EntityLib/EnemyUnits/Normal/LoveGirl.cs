using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.Cards.Misfortune;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class LoveGirl : EnemyUnit
	{
		private LoveGirl.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			base.CountDown = 7;
			this.UpdateMoveCounters();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Effect(this, "LoveGirlEffectManager", 0f, null, 0f, PerformAction.EffectBehavior.Add, 0f);
			yield return new ApplyStatusEffectAction<LoveGirlDamageReduce>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Animation(this, "shoot3", 1.5f, null, 0f, -1);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<LoveLetter>(base.Count1, false), DrawZoneTarget.Random, AddCardsType.OneByOne);
			yield break;
		}
		private void OnStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (args.Effect is LoveGirlDamageIncrease && base.HasStatusEffect<LoveGirlDamageIncrease>())
			{
				if (base.IsInTurn)
				{
					this._nextLove = true;
					return;
				}
				this.Next = LoveGirl.MoveType.FallInLove;
				base.UpdateTurnMoves();
			}
		}
		private string EscapeMoveName
		{
			get
			{
				return base.GetSpellCardName(default(int?), 1);
			}
		}
		private string FallInLoveMoveName
		{
			get
			{
				return base.GetSpellCardName(default(int?), 2);
			}
		}
		private IEnumerable<BattleAction> EscapeActions()
		{
			yield return PerformAction.Chat(this, "Chat.LoveGirlEscape".Localize(true), 3f, 0f, 3f, true);
			yield return new AddCardsToDeckAction(Library.CreateCards<Regret>(1, false));
			yield return new EscapeAction(this);
			yield break;
		}
		private IEnumerable<BattleAction> FallInLoveActions()
		{
			yield return PerformAction.Chat(this, "Chat.LoveGirlFallInLove".Localize(true), 3f, 0f, 3f, true);
			if (!base.GameRun.Player.HasExhibit<Qingshu>())
			{
				base.GameRun.ExtraExhibitReward = Library.CreateExhibit<Qingshu>();
			}
			yield return new EscapeAction(this);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case LoveGirl.MoveType.Attack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, base.Damage2), 2, false, "Instant", false);
				break;
			case LoveGirl.MoveType.Escape:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.EscapeMoveName, default(int?), default(int?), false), this.EscapeActions());
				break;
			case LoveGirl.MoveType.FallInLove:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.FallInLoveMoveName, default(int?), default(int?), false), this.FallInLoveActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (base.CountDown > 0)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			if (this._nextLove)
			{
				this.Next = LoveGirl.MoveType.FallInLove;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? LoveGirl.MoveType.Escape : LoveGirl.MoveType.Attack);
		}
		private bool _nextLove;
		private enum MoveType
		{
			Attack,
			Escape,
			FallInLove
		}
	}
}
