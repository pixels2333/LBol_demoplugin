using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200013E RID: 318
	[UsedImplicitly]
	public sealed class TaiguGouyu : ShiningExhibit
	{
		// Token: 0x0600045D RID: 1117 RVA: 0x0000BA23 File Offset: 0x00009C23
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x0000BA42 File Offset: 0x00009C42
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.ConsumingMana.Amount >= base.Value1)
			{
				base.NotifyActivating();
				yield return new GainTurnManaAction(base.Mana);
			}
			yield break;
		}
	}
}
