using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200017D RID: 381
	[UsedImplicitly]
	public sealed class LoushuiShaozi : Exhibit
	{
		// Token: 0x06000557 RID: 1367 RVA: 0x0000D1B0 File Offset: 0x0000B3B0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, delegate(UnitEventArgs _)
			{
				base.Counter = 0;
				base.Active = false;
			});
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x0000D1FC File Offset: 0x0000B3FC
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Active = false;
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0000D20C File Offset: 0x0000B40C
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.Card.CardType == CardType.Skill)
			{
				base.Counter = (base.Counter + 1) % base.Value1;
				if (base.Counter == 1)
				{
					base.Active = true;
				}
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					yield return new DrawManyCardAction(base.Value2);
				}
			}
			yield break;
		}
	}
}
