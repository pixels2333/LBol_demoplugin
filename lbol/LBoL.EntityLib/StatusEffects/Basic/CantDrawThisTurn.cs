using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000ED RID: 237
	public sealed class CantDrawThisTurn : StatusEffect
	{
		// Token: 0x0600034E RID: 846 RVA: 0x00008B60 File Offset: 0x00006D60
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<CardEventArgs>(base.Battle.Predraw, new GameEventHandler<CardEventArgs>(this.OnPredraw));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnded));
			base.Highlight = true;
		}

		// Token: 0x0600034F RID: 847 RVA: 0x00008BAE File Offset: 0x00006DAE
		private void OnPredraw(CardEventArgs args)
		{
			base.NotifyActivating();
			args.CancelBy(this);
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00008BBD File Offset: 0x00006DBD
		private IEnumerable<BattleAction> OnTurnEnded(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
