using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000170 RID: 368
	[UsedImplicitly]
	public sealed class HuanxiangxiangYuanqi : Exhibit
	{
		// Token: 0x0600051A RID: 1306 RVA: 0x0000CC3A File Offset: 0x0000AE3A
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0000CC59 File Offset: 0x0000AE59
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Ability)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Owner, base.Owner, base.Value1, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
