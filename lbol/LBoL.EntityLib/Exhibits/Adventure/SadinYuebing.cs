using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C6 RID: 454
	[UsedImplicitly]
	public sealed class SadinYuebing : Exhibit
	{
		// Token: 0x06000693 RID: 1683 RVA: 0x0000EFE4 File Offset: 0x0000D1E4
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x0000F008 File Offset: 0x0000D208
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card c) => c.CanUpgradeAndPositive));
			if (list.Count > 0)
			{
				base.NotifyActivating();
				List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Where<Card>(list, (Card c) => c.Config.Colors.Count > 1));
				if (list2.Count > 0)
				{
					Card card = list2.Sample(base.GameRun.BattleRng);
					yield return new UpgradeCardAction(card);
				}
				else
				{
					Card card2 = list.Sample(base.GameRun.BattleRng);
					yield return new UpgradeCardAction(card2);
				}
			}
			yield break;
		}
	}
}
