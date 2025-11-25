using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	[UsedImplicitly]
	public sealed class BrokenTimelineSe : ExtraTurnPartner
	{
		protected override void OnAdded(Unit unit)
		{
			this._presentationOnce = true;
			base.ThisTurnActivating = false;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
				{
					base.ThisTurnActivating = true;
				}
			});
			base.HandleOwnerEvent<ManaEventArgs>(base.Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(this.OnManaGaining));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}
		private void OnManaGaining(ManaEventArgs args)
		{
			if (base.ThisTurnActivating && args.Cause == ActionCause.TurnStart)
			{
				if (this._presentationOnce)
				{
					base.NotifyActivating();
					this._presentationOnce = false;
				}
				if (base.Battle.ExtraTurnMana.IsEmpty)
				{
					args.CancelBy(this);
					return;
				}
				args.Value = base.Battle.ExtraTurnMana;
				args.AddModifier(this);
			}
		}
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
		private bool _presentationOnce;
	}
}
