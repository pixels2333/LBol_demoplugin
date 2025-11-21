using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200016A RID: 362
	[UsedImplicitly]
	public sealed class He3 : Exhibit
	{
		// Token: 0x06000503 RID: 1283 RVA: 0x0000CA69 File Offset: 0x0000AC69
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000504 RID: 1284 RVA: 0x0000CA88 File Offset: 0x0000AC88
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.ConsumingMana.Amount >= base.Value1)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
