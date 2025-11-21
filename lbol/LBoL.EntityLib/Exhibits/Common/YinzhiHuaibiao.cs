using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001AC RID: 428
	[UsedImplicitly]
	public sealed class YinzhiHuaibiao : Exhibit
	{
		// Token: 0x06000625 RID: 1573 RVA: 0x0000E4F4 File Offset: 0x0000C6F4
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x0000E513 File Offset: 0x0000C713
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn)
			{
				base.Counter = (base.Counter + 1) % base.Value1;
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					yield return new GainManaAction(base.Mana);
				}
			}
			yield break;
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x0000E523 File Offset: 0x0000C723
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
