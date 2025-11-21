using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	// Token: 0x02000087 RID: 135
	[UsedImplicitly]
	public sealed class ClockCorpseSe : ExtraTurnPartner
	{
		// Token: 0x060001E8 RID: 488 RVA: 0x00005E30 File Offset: 0x00004030
		protected override void OnAdded(Unit unit)
		{
			base.ThisTurnActivating = false;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
				{
					base.ThisTurnActivating = true;
				}
			});
			base.HandleOwnerEvent<CardEventArgs>(base.Battle.Predraw, new GameEventHandler<CardEventArgs>(this.OnPredraw));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x00005EA8 File Offset: 0x000040A8
		private void OnPredraw(CardEventArgs args)
		{
			if (!base.ThisTurnActivating || args.Cause == ActionCause.TurnStart)
			{
				return;
			}
			if (args.Cause == ActionCause.Card)
			{
				Card card = args.ActionSource as Card;
				if (card != null && card.IsReplenish && !base.Battle.Player.IsInTurn)
				{
					return;
				}
			}
			args.CancelBy(this);
			base.NotifyActivating();
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00005F08 File Offset: 0x00004108
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
