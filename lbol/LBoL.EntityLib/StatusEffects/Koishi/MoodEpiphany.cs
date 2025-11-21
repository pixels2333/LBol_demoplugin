using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x0200007C RID: 124
	public sealed class MoodEpiphany : Mood
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060001AD RID: 429 RVA: 0x000054CD File Offset: 0x000036CD
		public override bool ForceNotShowDownText
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060001AE RID: 430 RVA: 0x000054D0 File Offset: 0x000036D0
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.Count = this.PlayCount;
			this._playedCardCount = 0;
			this._justGained = true;
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x060001AF RID: 431 RVA: 0x0000553D File Offset: 0x0000373D
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			yield return new MoodChangeAction(base.Battle.Player, this, null);
			yield break;
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000554D File Offset: 0x0000374D
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.SourceCard == args.Card && this._justGained)
			{
				this._justGained = false;
			}
			else
			{
				int num = base.Count - 1;
				base.Count = num;
				this._playedCardCount++;
				yield return new GainManaAction(this.Mana);
				yield return new DrawCardAction();
				if (base.Count <= 0)
				{
					yield return new MoodChangeAction(base.Battle.Player, this, null);
				}
			}
			yield break;
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00005564 File Offset: 0x00003764
		protected override void OnRemoving(Unit unit)
		{
			if (this._playedCardCount >= 12 && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Epiphany);
			}
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x000055A1 File Offset: 0x000037A1
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Count = Math.Max(base.Count, this.PlayCount);
			}
			return flag;
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060001B3 RID: 435 RVA: 0x000055C4 File Offset: 0x000037C4
		public override string UnitEffectName
		{
			get
			{
				return "DunwuLoop";
			}
		}

		// Token: 0x0400000D RID: 13
		[UsedImplicitly]
		public ManaGroup Mana = ManaGroup.Philosophies(1);

		// Token: 0x0400000E RID: 14
		[UsedImplicitly]
		public int PlayCount = 5;

		// Token: 0x0400000F RID: 15
		private bool _justGained;

		// Token: 0x04000010 RID: 16
		private int _playedCardCount;
	}
}
