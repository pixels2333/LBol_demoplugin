using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C4 RID: 452
	[UsedImplicitly]
	public sealed class PulpFiction : Exhibit
	{
		// Token: 0x0600068B RID: 1675 RVA: 0x0000EF59 File Offset: 0x0000D159
		protected override void OnEnterBattle()
		{
			base.Counter = base.Value2;
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x0000EF84 File Offset: 0x0000D184
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Counter > 0)
			{
				int num = base.Counter - 1;
				base.Counter = num;
				if (base.Battle.DrawZone.Count > 0)
				{
					base.NotifyActivating();
					for (int i = 0; i < base.Value1; i = num + 1)
					{
						Card card = Enumerable.LastOrDefault<Card>(base.Battle.DrawZone);
						if (card != null)
						{
							yield return new MoveCardAction(card, CardZone.Hand);
						}
						num = i;
					}
				}
				if (base.Counter == 1)
				{
					base.Blackout = true;
				}
			}
			yield break;
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x0000EF94 File Offset: 0x0000D194
		protected override void OnLeaveBattle()
		{
			if (base.Counter > 0)
			{
				base.Counter = 0;
			}
			base.Blackout = false;
		}
	}
}
