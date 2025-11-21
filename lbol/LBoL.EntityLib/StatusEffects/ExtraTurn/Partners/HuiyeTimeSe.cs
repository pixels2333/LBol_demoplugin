using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	// Token: 0x02000089 RID: 137
	[UsedImplicitly]
	public sealed class HuiyeTimeSe : ExtraTurnPartner
	{
		// Token: 0x060001F2 RID: 498 RVA: 0x00006044 File Offset: 0x00004244
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is PlayerUnit))
			{
				Debug.LogWarning(this.DebugName + " should not apply to non-player unit.");
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				return;
			}
			base.Count = base.Limit;
			base.ThisTurnActivating = false;
			base.ShowCount = false;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
				{
					base.ThisTurnActivating = true;
					base.ShowCount = true;
					base.Highlight = true;
				}
			});
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00006101 File Offset: 0x00004301
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				base.Count = base.Limit - base.Battle.TurnCardUsageHistory.Count;
			}
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00006128 File Offset: 0x00004328
		public override bool ShouldPreventCardUsage(Card card)
		{
			return base.ThisTurnActivating && base.Count <= 0;
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001F5 RID: 501 RVA: 0x00006140 File Offset: 0x00004340
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardHuiye".Localize(true);
			}
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000614D File Offset: 0x0000434D
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
