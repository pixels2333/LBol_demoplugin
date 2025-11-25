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
	[UsedImplicitly]
	public sealed class ClockCorpseSe : ExtraTurnPartner
	{
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
