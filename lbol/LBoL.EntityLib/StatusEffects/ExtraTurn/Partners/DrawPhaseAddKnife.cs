using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	// Token: 0x02000088 RID: 136
	[UsedImplicitly]
	public sealed class DrawPhaseAddKnife : ExtraTurnPartner
	{
		// Token: 0x060001ED RID: 493 RVA: 0x00005F60 File Offset: 0x00004160
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
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x060001EE RID: 494 RVA: 0x00005FD0 File Offset: 0x000041D0
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Limit, false), AddCardsType.Normal);
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x060001EF RID: 495 RVA: 0x00005FE0 File Offset: 0x000041E0
		private void OnPredraw(CardEventArgs args)
		{
			if (base.ThisTurnActivating && args.Cause == ActionCause.TurnStart)
			{
				args.CancelBy(this);
			}
		}
	}
}
