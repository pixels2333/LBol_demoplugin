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
	public sealed class MoodEpiphany : Mood
	{
		public override bool ForceNotShowDownText
		{
			get
			{
				return true;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.Count = this.PlayCount;
			this._playedCardCount = 0;
			this._justGained = true;
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			yield return new MoodChangeAction(base.Battle.Player, this, null);
			yield break;
		}
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
		protected override void OnRemoving(Unit unit)
		{
			if (this._playedCardCount >= 12 && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Epiphany);
			}
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Count = Math.Max(base.Count, this.PlayCount);
			}
			return flag;
		}
		public override string UnitEffectName
		{
			get
			{
				return "DunwuLoop";
			}
		}
		[UsedImplicitly]
		public ManaGroup Mana = ManaGroup.Philosophies(1);
		[UsedImplicitly]
		public int PlayCount = 5;
		private bool _justGained;
		private int _playedCardCount;
	}
}
