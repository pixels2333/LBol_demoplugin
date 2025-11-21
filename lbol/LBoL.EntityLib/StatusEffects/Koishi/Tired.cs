using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000082 RID: 130
	[UsedImplicitly]
	public sealed class Tired : StatusEffect
	{
		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060001CB RID: 459 RVA: 0x000058A8 File Offset: 0x00003AA8
		// (set) Token: 0x060001CC RID: 460 RVA: 0x000058B0 File Offset: 0x00003AB0
		private bool ThisTurnActivating { get; set; }

		// Token: 0x060001CD RID: 461 RVA: 0x000058B9 File Offset: 0x00003AB9
		protected override string GetBaseDescription()
		{
			if (!this.ThisTurnActivating)
			{
				return base.ExtraDescription;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x060001CE RID: 462 RVA: 0x000058D0 File Offset: 0x00003AD0
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogWarning(this.DebugName + " should not apply to non-player unit.");
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				return;
			}
			base.Count = base.Limit;
			this.ThisTurnActivating = false;
			base.ShowCount = false;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.GetStatusEffect<Tired>() == this)
				{
					this.ThisTurnActivating = true;
					base.ShowCount = true;
					base.Highlight = true;
				}
			});
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000598D File Offset: 0x00003B8D
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (this.ThisTurnActivating)
			{
				base.Count = base.Limit - base.Battle.TurnCardUsageHistory.Count;
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x000059B4 File Offset: 0x00003BB4
		public override bool ShouldPreventCardUsage(Card card)
		{
			return this.ThisTurnActivating && base.Count <= 0;
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x000059CC File Offset: 0x00003BCC
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.Tired".Localize(true);
			}
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x000059D9 File Offset: 0x00003BD9
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (this.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
