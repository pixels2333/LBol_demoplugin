using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	// Token: 0x0200008B RID: 139
	[UsedImplicitly]
	public sealed class UpgradeAllHandSe : ExtraTurnPartner
	{
		// Token: 0x060001FE RID: 510 RVA: 0x000062E8 File Offset: 0x000044E8
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
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x060001FF RID: 511 RVA: 0x00006362 File Offset: 0x00004562
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				base.NotifyActivating();
				yield return base.UpgradeAllHandsAction();
			}
			yield break;
		}

		// Token: 0x06000200 RID: 512 RVA: 0x00006372 File Offset: 0x00004572
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
