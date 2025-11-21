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
	// Token: 0x020000AB RID: 171
	public sealed class SuperExtraTurn : StatusEffect
	{
		// Token: 0x1700029A RID: 666
		// (get) Token: 0x060007F5 RID: 2037 RVA: 0x00017753 File Offset: 0x00015953
		// (set) Token: 0x060007F6 RID: 2038 RVA: 0x0001775B File Offset: 0x0001595B
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

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x060007F7 RID: 2039 RVA: 0x00017784 File Offset: 0x00015984
		public bool IsInExtraTurnByThis
		{
			get
			{
				return this.Status == TurnStatus.ExtraTurnByThis;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x060007F8 RID: 2040 RVA: 0x00017790 File Offset: 0x00015990
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

		// Token: 0x060007F9 RID: 2041 RVA: 0x00017823 File Offset: 0x00015A23
		protected override string GetBaseDescription()
		{
			if (!this.IsInExtraTurnByThis)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x0001783C File Offset: 0x00015A3C
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

		// Token: 0x060007FB RID: 2043 RVA: 0x00017901 File Offset: 0x00015B01
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				base.Count += other.Level;
			}
			return flag;
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x00017920 File Offset: 0x00015B20
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (this.IsInExtraTurnByThis)
			{
				base.Count = base.Level - base.Battle.TurnCardUsageHistory.Count;
			}
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x00017947 File Offset: 0x00015B47
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

		// Token: 0x060007FE RID: 2046 RVA: 0x00017957 File Offset: 0x00015B57
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (this._playingEffect)
			{
				this._playingEffect = false;
				yield return PerformAction.Effect(base.Battle.Player, "MoonNight", 0f, null, 0f, PerformAction.EffectBehavior.DieOut, 0f);
			}
			yield break;
		}

		// Token: 0x060007FF RID: 2047 RVA: 0x00017968 File Offset: 0x00015B68
		protected override void OnRemoving(Unit unit)
		{
			base.OnRemoving(unit);
			if (this._playingEffect)
			{
				this._playingEffect = false;
				this.React(PerformAction.Effect(base.Battle.Player, "MoonNight", 0f, null, 0f, PerformAction.EffectBehavior.DieOut, 0f));
			}
		}

		// Token: 0x06000800 RID: 2048 RVA: 0x000179BC File Offset: 0x00015BBC
		public override bool ShouldPreventCardUsage(Card card)
		{
			return this.IsInExtraTurnByThis && base.Count <= 0;
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000801 RID: 2049 RVA: 0x000179D4 File Offset: 0x00015BD4
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardHuiye".Localize(true);
			}
		}

		// Token: 0x0400036C RID: 876
		private TurnStatus _status;

		// Token: 0x0400036D RID: 877
		private bool _playingEffect;
	}
}
