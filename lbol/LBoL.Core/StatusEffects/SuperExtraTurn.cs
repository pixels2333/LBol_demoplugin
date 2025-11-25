using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	public sealed class SuperExtraTurn : StatusEffect
	{
		public TurnStatus Status
		{
			get
			{
				return this._status;
			}
			set
			{
				this._status = value;
				if (this.IsInExtraTurnByThis)
				{
					base.Count = base.Level;
				}
				base.ShowCount = this.IsInExtraTurnByThis;
			}
		}
		public bool IsInExtraTurnByThis
		{
			get
			{
				return this.Status == TurnStatus.ExtraTurnByThis;
			}
		}
		[UsedImplicitly]
		public string StatusString
		{
			get
			{
				string text;
				switch (this.Status)
				{
				case TurnStatus.NaturalTurn:
					text = UiUtils.WrapByColor("Game.NaturalTurn".Localize(true), GlobalConfig.UiGreen);
					break;
				case TurnStatus.ExtraTurn:
					text = UiUtils.WrapByColor("Game.ExtraTurn".Localize(true), GlobalConfig.UiRed);
					break;
				case TurnStatus.ExtraTurnByThis:
					text = UiUtils.WrapByColor("Game.ExtraTurn".Localize(true), GlobalConfig.UiRed);
					break;
				case TurnStatus.OutTurn:
					text = UiUtils.WrapByColor("Game.OutTurn".Localize(true), GlobalConfig.UiBlue);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				return text;
			}
		}
		protected override string GetBaseDescription()
		{
			if (!this.IsInExtraTurnByThis)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnAdded(Unit unit)
		{
			base.Limit = 0;
			if (base.Battle.Player.IsInTurn)
			{
				if (base.Battle.Player.IsExtraTurn)
				{
					this.Status = TurnStatus.ExtraTurn;
				}
				else
				{
					this.Status = TurnStatus.NaturalTurn;
					base.Limit = 1;
				}
			}
			else
			{
				this.Status = TurnStatus.OutTurn;
			}
			base.Count = base.Level;
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Count += other.Level;
			}
			return flag;
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (this.IsInExtraTurnByThis)
			{
				base.Count = base.Level - base.Battle.TurnCardUsageHistory.Count;
			}
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			if (this.IsInExtraTurnByThis && !this._playingEffect)
			{
				this._playingEffect = true;
				yield return PerformAction.Sfx("永夜返小", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "MoonNight", 0f, null, 0f, PerformAction.EffectBehavior.Add, 0f);
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (this._playingEffect)
			{
				this._playingEffect = false;
				yield return PerformAction.Effect(base.Battle.Player, "MoonNight", 0f, null, 0f, PerformAction.EffectBehavior.DieOut, 0f);
			}
			yield break;
		}
		protected override void OnRemoving(Unit unit)
		{
			base.OnRemoving(unit);
			if (this._playingEffect)
			{
				this._playingEffect = false;
				this.React(PerformAction.Effect(base.Battle.Player, "MoonNight", 0f, null, 0f, PerformAction.EffectBehavior.DieOut, 0f));
			}
		}
		public override bool ShouldPreventCardUsage(Card card)
		{
			return this.IsInExtraTurnByThis && base.Count <= 0;
		}
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardHuiye".Localize(true);
			}
		}
		private TurnStatus _status;
		private bool _playingEffect;
	}
}
