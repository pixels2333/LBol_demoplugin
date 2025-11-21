using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	// Token: 0x02000086 RID: 134
	[UsedImplicitly]
	public sealed class BrokenTimelineSe : ExtraTurnPartner
	{
		// Token: 0x060001E3 RID: 483 RVA: 0x00005CF4 File Offset: 0x00003EF4
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

		// Token: 0x060001E4 RID: 484 RVA: 0x00005D6C File Offset: 0x00003F6C
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

		// Token: 0x060001E5 RID: 485 RVA: 0x00005DD5 File Offset: 0x00003FD5
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x04000013 RID: 19
		private bool _presentationOnce;
	}
}
