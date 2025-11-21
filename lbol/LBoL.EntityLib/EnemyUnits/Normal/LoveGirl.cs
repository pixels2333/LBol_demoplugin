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
	// Token: 0x020001DE RID: 478
	[UsedImplicitly]
	public sealed class LoveGirl : EnemyUnit
	{
		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000773 RID: 1907 RVA: 0x00010C03 File Offset: 0x0000EE03
		// (set) Token: 0x06000774 RID: 1908 RVA: 0x00010C0B File Offset: 0x0000EE0B
		private LoveGirl.MoveType Next { get; set; }

		// Token: 0x06000775 RID: 1909 RVA: 0x00010C14 File Offset: 0x0000EE14
		protected override void OnEnterBattle(BattleController battle)
		{
			base.CountDown = 7;
			this.UpdateMoveCounters();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdded));
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00010C63 File Offset: 0x0000EE63
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Effect(this, "LoveGirlEffectManager", 0f, null, 0f, PerformAction.EffectBehavior.Add, 0f);
			yield return new ApplyStatusEffectAction<LoveGirlDamageReduce>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Animation(this, "shoot3", 1.5f, null, 0f, -1);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<LoveLetter>(base.Count1, false), DrawZoneTarget.Random, AddCardsType.OneByOne);
			yield break;
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00010C73 File Offset: 0x0000EE73
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

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000778 RID: 1912 RVA: 0x00010CA8 File Offset: 0x0000EEA8
		private string EscapeMoveName
		{
			get
			{
				return base.GetSpellCardName(default(int?), 1);
			}
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000779 RID: 1913 RVA: 0x00010CC8 File Offset: 0x0000EEC8
		private string FallInLoveMoveName
		{
			get
			{
				return base.GetSpellCardName(default(int?), 2);
			}
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00010CE5 File Offset: 0x0000EEE5
		private IEnumerable<BattleAction> EscapeActions()
		{
			yield return PerformAction.Chat(this, "Chat.LoveGirlEscape".Localize(true), 3f, 0f, 3f, true);
			yield return new AddCardsToDeckAction(Library.CreateCards<Regret>(1, false));
			yield return new EscapeAction(this);
			yield break;
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x00010CF5 File Offset: 0x0000EEF5
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

		// Token: 0x0600077C RID: 1916 RVA: 0x00010D05 File Offset: 0x0000EF05
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

		// Token: 0x0600077D RID: 1917 RVA: 0x00010D18 File Offset: 0x0000EF18
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

		// Token: 0x04000077 RID: 119
		private bool _nextLove;

		// Token: 0x020006D2 RID: 1746
		private enum MoveType
		{
			// Token: 0x040008CE RID: 2254
			Attack,
			// Token: 0x040008CF RID: 2255
			Escape,
			// Token: 0x040008D0 RID: 2256
			FallInLove
		}
	}
}
